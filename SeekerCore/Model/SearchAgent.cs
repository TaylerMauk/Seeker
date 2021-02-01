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

        public SearchAgent() { }

        public SearchResults GetFiles(CriteriaInfo[] criteria, SearchParameters searchParameters)
        {
            if (!Directory.Exists(searchParameters.rootDirectory))
                throw new DirectoryNotFoundException();

            SearchResults resultFiles;
            FindFiles(criteria, searchParameters, out resultFiles);

            return resultFiles;
        }

        public SearchResults GetSubdirectories(string directory)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException();

            SearchResults subdirectories;
            GetSubdirectories(directory, out subdirectories);

            return subdirectories;
        }
    }
}
