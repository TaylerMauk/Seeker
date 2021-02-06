using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using static SeekerCore.Model.SearchConstructs;

namespace SeekerCore.Model
{
    /// <summary>
    /// Logic for searching the filesystem
    /// </summary>
    class SearchAgent
    {
        [DllImport("SeekerFilesystem.dll")]
        private static extern void FindFiles(CriteriaInfo[] criteria, SearchParameters searchParameters, out SearchResults outResults);

        [DllImport("SeekerFilesystem.dll")]
        private static extern void GetSubdirectories(string directory, out SearchResults outResults);

        public enum ActivityState
        {
            INACTIVE,
            ACTIVE
        }

        /// <summary>
        /// Results from searches
        /// </summary>
        public SearchResults Results { get; private set; }

        /// <summary>
        /// Current state of the agent
        /// </summary>
        public ActivityState State { get; private set; }

        public event EventHandler StateChanged;

        /// <summary>
        /// Number of active threads
        /// </summary>
        private int m_threadCount;

        public SearchAgent()
        {
            State = ActivityState.INACTIVE;
            InitializeResultsStruct();

            m_threadCount = 0;
        }


        /// <summary>
        /// Performs a file search in a new thread and results are stored in <see cref="Results"/>.
        /// Subscribe to <see cref="StateChanged"/> to know when the search is finished (the 
        /// <see cref="State"/> will be <see cref="ActivityState.INACTIVE"/>).
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="searchParameters"></param>
        public void RunFileSearch(CriteriaInfo[] criteria, SearchParameters searchParameters)
        {
            if (!Directory.Exists(searchParameters.rootDirectory))
                throw new DirectoryNotFoundException();

            if (m_threadCount == 0)
                InitializeResultsStruct();

            new Thread(new ThreadStart(() => {
                SearchResults sr;
                ActivateThread();
                FindFiles(criteria, searchParameters, out sr);

                // Keep running list of all results
                string[] tmpResults = new string[Results.count + sr.count];
                Results.entries.CopyTo(tmpResults, 0);
                sr.entries.CopyTo(tmpResults, Results.count);
                Results = new SearchResults
                {
                    count = tmpResults.Length,
                    entries = new string[tmpResults.Length]
                };
                tmpResults.CopyTo(Results.entries, 0);

                DeactivateThread();
            })).Start();
        }

        /// <summary>
        /// Performs subdirectory enumeration in a new thread and results are stored in
        /// <see cref="Results"/>. Subscribe to <see cref="StateChanged"/> to know when the
        /// search is finished (the <see cref="State"/> will be <see cref="ActivityState.INACTIVE"/>).
        /// </summary>
        /// <param name="directory"></param>
        public void RunSubdirectoryEnumeration(string directory)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException();

            if (m_threadCount == 0)
                InitializeResultsStruct();

            new Thread(new ThreadStart(() => {
                SearchResults sr;
                ActivateThread();
                GetSubdirectories(directory, out sr);
                Results = sr;
                DeactivateThread();
            })).Start();
        }

        /// <summary>
        /// Performs subdirectory enumeration
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public SearchResults GetSubdirectories(string directory)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException();

            SearchResults sr;
            GetSubdirectories(directory, out sr);
            return sr;
        }

        private void ActivateThread()
        {
            ++m_threadCount;
            if (m_threadCount == 1)
            {
                // Change state only if this is first active thread
                State = ActivityState.ACTIVE;
                StateChanged.Invoke(this, null);
            }
        }

        private void DeactivateThread()
        {
            --m_threadCount;
            if (m_threadCount == 0)
            {
                // Change state only if there are no more active threads
                State = ActivityState.INACTIVE;
                StateChanged.Invoke(this, null);
            }
        }

        private void InitializeResultsStruct()
        {
            Results = new SearchResults
            {
                count = 0,
                entries = Array.Empty<string>()
            };
        }
    }
}
