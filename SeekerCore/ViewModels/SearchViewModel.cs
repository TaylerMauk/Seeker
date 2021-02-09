using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using SeekerCore.Model;
using static SeekerCore.Model.SearchConstructs;

namespace SeekerCore.ViewModels
{
    class SearchViewModel : ViewModelBase
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
        /// Approximate runtime of search in seconds
        /// </summary>
        public double SearchRuntime
        {
            get
            {
                return m_searchRuntime;
            }
            set
            {
                m_searchRuntime = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(SearchRuntime)));
            }
        }
        private double m_searchRuntime;

        /// <summary>
        /// User-friendly translation of <see cref="SearchPhrase"/>
        /// </summary>
        public string FriendlyTranslation
        {
            get
            {
                return m_friendlyTranslation;
            }
            set
            {
                m_friendlyTranslation = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(FriendlyTranslation)));
            }
        }
        private string m_friendlyTranslation;

        /// <summary>
        /// Search phrase entered by the user
        /// </summary>
        public string SearchPhrase
        {
            get
            {
                return m_searchPhrase;
            }
            set
            {
                m_searchPhrase = value;

                if (string.IsNullOrEmpty(m_searchPhrase))
                {
                    FriendlyTranslation = string.Empty;
                }
                else if (m_languageParser.IsPhraseValid(m_searchPhrase))
                {
                    m_searchCriteriaInfo = m_languageParser.Parse(m_searchPhrase);
                    FriendlyTranslation = m_languageParser.GetFriendlyTranslation(m_searchCriteriaInfo);
                }
                else
                {
                    FriendlyTranslation = "INVALID";
                }

                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(SearchPhrase)));
                ((RelayCommand)ExecuteSearchCommand).RaiseCanExecuteChanged();
            }
        }
        private string m_searchPhrase;

        public Visibility SearchingIndicatorVisibility
        {
            get
            {
                return m_searchingIndicatorVisibility;
            }
            private set
            {
                m_searchingIndicatorVisibility = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(SearchingIndicatorVisibility)));
            }
        }
        private Visibility m_searchingIndicatorVisibility;

        /// <summary>
        /// Root directories for search
        /// </summary>
        public ObservableCollection<string> SearchDirectories { get; set; }

        /// <summary>
        /// Used when adding a new directory to <see cref="SearchDirectories"/>
        /// </summary>
        public string NewSearchDirectory
        {
            get
            {
                return m_newSearchDirectory;
            }
            set
            {
                m_newSearchDirectory = value;

                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(NewSearchDirectory)));
                ((RelayCommand)AddSearchDirectoryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ExecuteSearchCommand).RaiseCanExecuteChanged();
            }
        }
        private string m_newSearchDirectory;

        /// <summary>
        /// Used when removing a directory from <see cref="SearchDirectories"/>
        /// </summary>
        public int SearchDirectoryRemovalIndex
        {
            get
            {
                return m_searchDirectoryRemovalIndex;
            }
            set
            {
                m_searchDirectoryRemovalIndex = value;

                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(SearchDirectoryRemovalIndex)));
                ((RelayCommand)RemoveSearchDirectoryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ExecuteSearchCommand).RaiseCanExecuteChanged();
            }
        }
        private int m_searchDirectoryRemovalIndex;

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

        public ICommand ExecuteSearchCommand { get; set; }

        public ICommand AddSearchDirectoryCommand { get; set; }

        public ICommand RemoveSearchDirectoryCommand { get; set; }

        private SearchAgent m_searchAgent;
        private LanguageParser m_languageParser;
        private CriteriaInfo[] m_searchCriteriaInfo;
        private Stopwatch m_searchRuntimeStopwatch;

        public SearchViewModel()
        {
            ExecuteSearchCommand = new RelayCommand(ExecuteSearch, CanExecuteSearch);
            AddSearchDirectoryCommand = new RelayCommand(AddSearchDirectory, CanExecuteAddSearchDirectory);
            RemoveSearchDirectoryCommand = new RelayCommand(RemoveSearchDirectory, CanExecuteRemoveSearchDirectory);

            SearchDirectories = new ObservableCollection<string>();
            SearchResultEntries = new ObservableCollection<string>();
            SearchingIndicatorVisibility = Visibility.Collapsed;

            m_languageParser = new LanguageParser();
            m_searchAgent = new SearchAgent();
            m_searchDirectoryRemovalIndex = -1;
            m_selectedFileIndex = -1;

            m_searchAgent.StateChanged += OnSearchAgentStateChanged;
        }

        private void OnSearchAgentStateChanged(object sender, EventArgs e)
        {
            Debug.Write("SEARCH AGENT STATE CHANGED TO ");

            if (m_searchAgent.State == SearchAgent.ActivityState.ACTIVE)
            {
                Debug.WriteLine("ACTIVE");

                App.Current.Dispatcher.Invoke(() => {
                    SearchResultEntries.Clear();
                    SearchTotalResultCount = 0;
                    SearchRuntime = 0;
                    SearchingIndicatorVisibility = Visibility.Visible;
                });

                m_searchRuntimeStopwatch = Stopwatch.StartNew();
            }
            else
            {
                Debug.WriteLine("INACTIVE");

                m_searchRuntimeStopwatch.Stop();
                SearchRuntime = m_searchRuntimeStopwatch.Elapsed.TotalSeconds;

                App.Current.Dispatcher.Invoke(() => {
                    SearchingIndicatorVisibility = Visibility.Collapsed;
                    if (null != m_searchAgent.Results.entries)
                    {
                        foreach (string entry in m_searchAgent.Results.entries)
                            SearchResultEntries.Add(entry.Substring(entry.LastIndexOf("\\") + 1));
                        SearchTotalResultCount = m_searchAgent.Results.count;
                    }
                });
            }

            App.Current.Dispatcher.Invoke(() => {
                ((RelayCommand)ExecuteSearchCommand).RaiseCanExecuteChanged();
                ((RelayCommand)AddSearchDirectoryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RemoveSearchDirectoryCommand).RaiseCanExecuteChanged();
            });
        }

        private void OnSelectedResultFileIndexChanged()
        {
            if (m_selectedFileIndex < 0 || m_selectedFileIndex > m_searchAgent.Results.count)
                return;

            FileInfo fileInfo = new FileInfo(m_searchAgent.Results.entries[m_selectedFileIndex]);
            SelectedFilePath = fileInfo.FullName;
            SelectedFileName = fileInfo.Name;
            SelectedFileCreationTime = fileInfo.CreationTime.ToString("F");
            SelectedFileLastAccessTime = fileInfo.LastAccessTime.ToString("F");
            SelectedFileLastWriteTime = fileInfo.LastWriteTime.ToString("F");
        }

        private void ExecuteSearch()
        {
            SearchParameters searchParameters;
            for (int i = 0; i < SearchDirectories.Count; ++i)
            {
                searchParameters = new SearchParameters
                {
                    criteriaCount = m_searchCriteriaInfo.Length,
                    rootDirectory = SearchDirectories[i],
                    isRecursive = true
                };
                m_searchAgent.RunFileSearch(m_searchCriteriaInfo, searchParameters);
            }

        }

        private bool CanExecuteSearch()
        {
            if (null == m_languageParser)
                return false;
            if (m_searchAgent.State == SearchAgent.ActivityState.ACTIVE)
                return false;

            return SearchDirectories.Count > 0 && m_languageParser.IsPhraseValid(m_searchPhrase);
        }

        private void AddSearchDirectory()
        {
            SearchDirectories.Add(NewSearchDirectory);
            NewSearchDirectory = string.Empty;
        }

        private bool CanExecuteAddSearchDirectory()
        {
            return m_searchAgent.State == SearchAgent.ActivityState.INACTIVE &&
                null != NewSearchDirectory &&
                NewSearchDirectory != string.Empty &&
                Directory.Exists(NewSearchDirectory);
        }

        private void RemoveSearchDirectory()
        {
            SearchDirectories.RemoveAt(SearchDirectoryRemovalIndex);
            SearchDirectoryRemovalIndex = -1;
        }

        private bool CanExecuteRemoveSearchDirectory()
        {
            return m_searchAgent.State == SearchAgent.ActivityState.INACTIVE &&
                SearchDirectories.Count > 0 &&
                SearchDirectoryRemovalIndex != -1;
        }
    }
}
