using System;
using System.IO;
using System.Runtime.InteropServices;
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

        public event EventHandler StateChanged;

        public ActivityState State;

        public SearchAgent()
        {
            State = ActivityState.INACTIVE;
        }

        public SearchResults GetFiles(CriteriaInfo[] criteria, SearchParameters searchParameters)
        {
            if (!Directory.Exists(searchParameters.rootDirectory))
                throw new DirectoryNotFoundException();

            ToggleState();
            SearchResults resultFiles;
            FindFiles(criteria, searchParameters, out resultFiles);
            ToggleState();

            return resultFiles;
        }

        public SearchResults GetSubdirectories(string directory)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException();

            ToggleState();
            SearchResults subdirectories;
            GetSubdirectories(directory, out subdirectories);
            ToggleState();

            return subdirectories;
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
