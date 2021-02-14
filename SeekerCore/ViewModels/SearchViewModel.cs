﻿using System;
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

        public bool IsUsingSimpleParameters
        {
            get
            {
                return m_isUsingSimpleParameters;
            }
            set
            {
                m_isUsingSimpleParameters = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs(nameof(IsUsingSimpleParameters)));
                ((RelayCommand)UseSimpleParametersCommand).RaiseCanExecuteChanged();
                ((RelayCommand)UseAdvancedParametersCommand).RaiseCanExecuteChanged();
            }
        }
        private bool m_isUsingSimpleParameters;

        public ICommand ExecuteSearchCommand { get; set; }

        public ICommand AddSearchDirectoryCommand { get; set; }

        public ICommand RemoveSearchDirectoryCommand { get; set; }

        public ICommand UseSimpleParametersCommand { get; set; }

        public ICommand UseAdvancedParametersCommand { get; set; }

        private LanguageParser m_languageParser;
        private CriteriaInfo[] m_searchCriteriaInfo;
        private Stopwatch m_searchRuntimeStopwatch;
        private MainViewModel m_mainViewModel;

        public SearchViewModel(MainViewModel mainViewModel)
        {
            m_mainViewModel = mainViewModel;

            ExecuteSearchCommand = new RelayCommand(ExecuteSearch, CanExecuteSearch);
            AddSearchDirectoryCommand = new RelayCommand(AddSearchDirectory, CanExecuteAddSearchDirectory);
            RemoveSearchDirectoryCommand = new RelayCommand(RemoveSearchDirectory, CanExecuteRemoveSearchDirectory);
            UseSimpleParametersCommand = new RelayCommand(UseSimpleParameters, CanExecuteUseSimpleParameters);
            UseAdvancedParametersCommand = new RelayCommand(UseAdvancedParameters, CanExecuteUseAdvancedParameters);

            SearchDirectories = new ObservableCollection<string>();
            SearchingIndicatorVisibility = Visibility.Collapsed;

            m_languageParser = new LanguageParser();
            m_searchDirectoryRemovalIndex = -1;
            m_isUsingSimpleParameters = true;

            m_mainViewModel.MainSearchAgent.StateChanged += OnSearchAgentStateChanged;
        }

        private void OnSearchAgentStateChanged(object sender, EventArgs e)
        {
            Debug.Write("SEARCH AGENT STATE CHANGED TO ");

            if (m_mainViewModel.MainSearchAgent.State == SearchAgent.ActivityState.ACTIVE)
            {
                Debug.WriteLine("ACTIVE");

                App.Current.Dispatcher.Invoke(() => {
                    m_mainViewModel.ResultsViewModel.ClearSearchEntries();
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
                    if (null != m_mainViewModel.MainSearchAgent.Results.entries)
                        m_mainViewModel.ResultsViewModel.AddSearchEntries(m_mainViewModel.MainSearchAgent.Results.entries);
                });
            }

            App.Current.Dispatcher.Invoke(() => {
                ((RelayCommand)ExecuteSearchCommand).RaiseCanExecuteChanged();
                ((RelayCommand)AddSearchDirectoryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RemoveSearchDirectoryCommand).RaiseCanExecuteChanged();
            });
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
                m_mainViewModel.MainSearchAgent.RunFileSearch(m_searchCriteriaInfo, searchParameters);
            }

        }

        private bool CanExecuteSearch()
        {
            if (null == m_languageParser)
                return false;
            if (m_mainViewModel.MainSearchAgent.State == SearchAgent.ActivityState.ACTIVE)
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
            return m_mainViewModel.MainSearchAgent.State == SearchAgent.ActivityState.INACTIVE &&
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
            return m_mainViewModel.MainSearchAgent.State == SearchAgent.ActivityState.INACTIVE &&
                SearchDirectories.Count > 0 &&
                SearchDirectoryRemovalIndex != -1;
        }

        private void UseSimpleParameters()
        {
            IsUsingSimpleParameters = true;
        }

        private bool CanExecuteUseSimpleParameters()
        {
            return !m_isUsingSimpleParameters;
        }

        private void UseAdvancedParameters()
        {
            IsUsingSimpleParameters = false;
        }

        private bool CanExecuteUseAdvancedParameters()
        {
            return m_isUsingSimpleParameters;
        }
    }
}
