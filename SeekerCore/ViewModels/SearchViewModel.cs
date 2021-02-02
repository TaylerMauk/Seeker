using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
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
        /// Directory to begin search
        /// </summary>
        public string SearchDirectory
        {
            get
            {
                return m_searchDirectory;
            }
            set
            {
                m_searchDirectory = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(SearchDirectory)));
            }
        }
        private string m_searchDirectory;

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
                ((StartSearchCommand)ExecuteSearchCommand).RaiseCanExecuteChanged();
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

        public ICommand ExecuteSearchCommand { get; set; }

        public ObservableCollection<string> SearchResultEntries { get; private set; }

        private SearchAgent m_searchAgent;
        private LanguageParser m_languageParser;
        private CriteriaInfo[] m_searchCriteriaInfo;
        private SearchResults m_searchResults;

        public SearchViewModel()
        {
            m_languageParser = new LanguageParser();
            m_searchAgent = new SearchAgent();
            ExecuteSearchCommand = new StartSearchCommand(ExecuteSearch, CanExecuteSearch);
            SearchResultEntries = new ObservableCollection<string>();
            SearchingIndicatorVisibility = Visibility.Collapsed;

            m_searchAgent.StateChanged += OnSearchAgentStateChanged;
        }

        private void OnSearchAgentStateChanged(object sender, EventArgs e)
        {
            Debug.Write("SEARCH AGENT STATE CHANGED TO ");

            if (m_searchAgent.State == SearchAgent.ActivityState.ACTIVE)
            {
                Debug.WriteLine("ACTIVE");
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    SearchResultEntries.Clear();
                    SearchTotalResultCount = 0;
                    SearchRuntime = 0;
                    SearchingIndicatorVisibility = Visibility.Visible;
                });
            }
            else
            {
                Debug.WriteLine("INACTIVE");
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    SearchingIndicatorVisibility = Visibility.Collapsed;
                });
            }

            App.Current.Dispatcher.Invoke((Action)delegate
            {
                ((StartSearchCommand)ExecuteSearchCommand).RaiseCanExecuteChanged();
            });
        }

        private void ExecuteSearch()
        {
            new Thread(new ThreadStart(BeginSearch)).Start();
        }

        private bool CanExecuteSearch()
        {
            if (null == m_languageParser)
                return false;
            if (m_searchAgent.State == SearchAgent.ActivityState.ACTIVE)
                return false;

            return m_languageParser.IsPhraseValid(m_searchPhrase);
        }

        private void BeginSearch()
        {
            SearchParameters searchParameters = new SearchParameters
            {
                criteriaCount = m_searchCriteriaInfo.Length,
                rootDirectory = SearchDirectory,
                isRecursive = true
            };

            Stopwatch searchStopwatch = Stopwatch.StartNew();
            m_searchResults = m_searchAgent.GetFiles(m_searchCriteriaInfo, searchParameters);
            searchStopwatch.Stop();
            SearchRuntime = searchStopwatch.Elapsed.TotalSeconds;

            // Display results
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                if (null == m_searchResults.entries)
                    return;

                foreach (string entry in m_searchResults.entries)
                    SearchResultEntries.Add(entry);
                SearchTotalResultCount = m_searchResults.count;
            });
        }
    }
}
