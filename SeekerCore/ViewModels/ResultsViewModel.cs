using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace SeekerCore.ViewModels
{
    class ResultsViewModel : ViewModelBase
    {
        /// <summary>
        /// Number of results returned from search
        /// </summary>
        public int SearchTotalResultCount
        {
            get
            {
                return m_searchTotalResultCount;
            }
            set
            {
                m_searchTotalResultCount = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(SearchTotalResultCount)));
            }
        }
        private int m_searchTotalResultCount;

        /// <summary>
        /// Result file names
        /// </summary>
        public ObservableCollection<string> SearchResultEntries { get; private set; }

        /// <summary>
        /// Index of selected file from search results
        /// </summary>
        public int SelectedFileIndex
        {
            get
            {
                return m_selectedFileIndex;
            }
            set
            {
                m_selectedFileIndex = value;

                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedFileIndex)));
                OnSelectedResultFileIndexChanged();
            }
        }
        private int m_selectedFileIndex;

        /// <summary>
        /// Path of selected file from search results
        /// </summary>
        public string SelectedFilePath
        {
            get
            {
                return m_selectedFilePath;
            }
            set
            {
                m_selectedFilePath = value;

                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedFilePath)));
            }
        }
        private string m_selectedFilePath;

        /// <summary>
        /// Name of selected file from search results
        /// </summary>
        public string SelectedFileName
        {
            get
            {
                return m_selectedFileName;
            }
            set
            {
                m_selectedFileName = value;

                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedFileName)));
            }
        }
        private string m_selectedFileName;

        /// <summary>
        /// Last access time of selected file from search results
        /// </summary>
        public string SelectedFileLastAccessTime
        {
            get
            {
                return m_selectedFileLastAccesTime;
            }
            set
            {
                m_selectedFileLastAccesTime = value;

                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedFileLastAccessTime)));
            }
        }
        private string m_selectedFileLastAccesTime;

        /// <summary>
        /// Last write time of selected file from search results
        /// </summary>
        public string SelectedFileLastWriteTime
        {
            get
            {
                return m_selectedFileLastWriteTime;
            }
            set
            {
                m_selectedFileLastWriteTime = value;

                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedFileLastWriteTime)));
            }
        }
        private string m_selectedFileLastWriteTime;

        /// <summary>
        /// Creation time of selected file from search results
        /// </summary>
        public string SelectedFileCreationTime
        {
            get
            {
                return m_selectedFileCreationTime;
            }
            set
            {
                m_selectedFileCreationTime = value;

                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedFileCreationTime)));
            }
        }
        private string m_selectedFileCreationTime;

        private MainViewModel m_mainViewModel;

        public ResultsViewModel(MainViewModel mainViewModel)
        {
            m_mainViewModel = mainViewModel;

            SearchResultEntries = new ObservableCollection<string>();

            m_selectedFileIndex = -1;
        }

        public void AddSearchEntries(string[] entries)
        {
            foreach (string entry in entries)
                SearchResultEntries.Add(entry.Substring(entry.LastIndexOf("\\") + 1));

            SearchTotalResultCount = entries.Length;
        }

        public void ClearSearchEntries()
        {
            SearchResultEntries.Clear();
            SearchTotalResultCount = 0;
        }

        private void OnSelectedResultFileIndexChanged()
        {
            if (m_selectedFileIndex < 0 || m_selectedFileIndex > m_searchTotalResultCount)
                return;

            FileInfo fileInfo = new FileInfo(m_mainViewModel.MainSearchAgent.Results.entries[m_selectedFileIndex]);
            SelectedFilePath = fileInfo.FullName;
            SelectedFileName = fileInfo.Name;
            SelectedFileCreationTime = fileInfo.CreationTime.ToString("F");
            SelectedFileLastAccessTime = fileInfo.LastAccessTime.ToString("F");
            SelectedFileLastWriteTime = fileInfo.LastWriteTime.ToString("F");
        }
    }
}
