using System.Runtime.InteropServices;

namespace SeekerCore.Model
{
    public static class SearchConstructs
    {
        public enum CriteriaType
        {
            EXCLUDE = 1 << 0,
            FLIPFLOP = 1 << 1,
            SINGLE = 1 << 2
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CriteriaInfo
        {
            public CriteriaType type;
            public string keyphrase;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SearchParameters
        {
            public int criteriaCount;
            public string rootDirectory;
            public bool isRecursive;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SearchResults
        {
            public int count;

            [MarshalAs(UnmanagedType.SafeArray)]
            public string[] entries;
        }
    }
}
