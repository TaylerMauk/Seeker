using System;
using System.Windows;
using System.Windows.Controls;
using SeekerCore.ViewModels;
using SeekerCore.Views.Pages;

namespace SeekerCore.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Page SelectedParameterPage
        {
            get
            {
                return m_selectedParameterPage;
            }
            set
            {
                m_selectedParameterPage = value;
                frameParameters.Navigate(m_selectedParameterPage);
            }
        }
        private Page m_selectedParameterPage;

        private const string SYNTAX_GUIDE_TEXT =
            "Separate requirements using a comma\n" +
            "    my, file\n" +
            "Create alternate requirements using a pipe\n" +
            "    my | flie\n" +
            "Exclude words by using a hyphen\n" +
            "    -my\n\n" +
            "Still lost?\n" +
            "    Don't worry! Just start typing and check out the translation.";

        private MainViewModel m_mainViewModel;
        private AdvancedParametersPage m_advancedParamsPage;
        private SimpleParametersPage m_simpleParamsPage;

        public MainWindow()
        {
            InitializeComponent();

            txtBlockSyntaxGuide.Text = SYNTAX_GUIDE_TEXT;

            m_advancedParamsPage = new AdvancedParametersPage();
            m_simpleParamsPage = new SimpleParametersPage();
            m_mainViewModel = new MainViewModel();
            m_selectedParameterPage = m_mainViewModel.SearchViewModel.IsUsingSimpleParameters ?
                m_simpleParamsPage :
                m_advancedParamsPage;


            DataContext = m_mainViewModel;
            loadingIndicator.DataContext = m_mainViewModel.SearchViewModel;
            gridSearchParams.DataContext = m_mainViewModel.SearchViewModel;
            gridSearchResults.DataContext = m_mainViewModel.ResultsViewModel;
            txtBlockResultsCount.DataContext = m_mainViewModel.ResultsViewModel;
            frameParameters.DataContext = this;

            m_advancedParamsPage.Owner = this;
            m_advancedParamsPage.DataContext = gridSearchParams.DataContext;
            m_simpleParamsPage.DataContext = gridSearchParams.DataContext;
        }

        private void OnWindowStateChanged(object sender, EventArgs e)
        {
            RefreshMaximizeRestoreButton();
        }

        private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void OnMaximizeRestoreButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RefreshMaximizeRestoreButton()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.maximizeButton.Visibility = Visibility.Collapsed;
                this.restoreButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.maximizeButton.Visibility = Visibility.Visible;
                this.restoreButton.Visibility = Visibility.Collapsed;
            }
        }

        private void OnBtnUseSimpleParamsClick(object sender, RoutedEventArgs e)
        {
            SelectedParameterPage = m_simpleParamsPage;
        }

        private void OnBtnUseAdvancedParamsClick(object sender, RoutedEventArgs e)
        {
            SelectedParameterPage = m_advancedParamsPage;
        }
    }
}
