using SeekerCore.Model;

namespace SeekerCore.ViewModels
{
    /// <summary>
    /// ViewModel for main window
    /// </summary>
    class MainViewModel : ViewModelBase
    {
        public SearchViewModel SearchViewModel { get; private set; }

        public ResultsViewModel ResultsViewModel { get; private set; }

        public SearchAgent MainSearchAgent { get; private set; }

        public MainViewModel()
        {
            MainSearchAgent = new SearchAgent();
            SearchViewModel = new SearchViewModel(this);
            ResultsViewModel = new ResultsViewModel(this);
        }
    }
}
