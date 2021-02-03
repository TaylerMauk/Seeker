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

        public SearchAgent()
        {
            State = ActivityState.INACTIVE;
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

            new Thread(new ThreadStart(() => {
                SearchResults sr;
                ToggleState();
                FindFiles(criteria, searchParameters, out sr);
                Results = sr;
                ToggleState();
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

            new Thread(new ThreadStart(() => {
                SearchResults sr;
                ToggleState();
                GetSubdirectories(directory, out sr);
                Results = sr;
                ToggleState();
            })).Start();
        }

        /// <summary>
        /// Toggles <see cref="State"/> between <see cref="ActivityState"/> states
        /// </summary>
        private void ToggleState()
        {
            if (State == ActivityState.ACTIVE)
                State = ActivityState.INACTIVE;
            else
                State = ActivityState.ACTIVE;

            StateChanged.Invoke(this, null);
        }
    }
}
