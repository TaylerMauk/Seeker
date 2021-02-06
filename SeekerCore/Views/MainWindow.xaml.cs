using System;
using System.Windows;
using SeekerCore.ViewModels;

namespace SeekerCore.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SearchViewModel m_searchViewModel;

        public MainWindow()
        {
            InitializeComponent();

            m_searchViewModel = new SearchViewModel();
            DataContext = m_searchViewModel;
            //txtBoxSearchEntry.DataContext = m_searchViewModel;
            //txtBlockTranslation.DataContext = m_searchViewModel;
            //btnStartSearch.DataContext = m_searchViewModel;
            //txtBlockSearchRuntime.DataContext = m_searchViewModel;
            //txtBlockSearchTotalResults.DataContext = m_searchViewModel;
            //listBoxSearchDirectories.DataContext = m_searchViewModel;
            //btnStartSearch.DataContext = m_searchViewModel;
            //listBoxSearchResultEntries.DataContext = m_searchViewModel;
            //stackPanelSearchingIndicator.DataContext = m_searchViewModel;
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

        private void OnAddSearchDirectoryButtonClick(object sender, RoutedEventArgs e)
        {
            new AddSearchDirectoryDialog(this).ShowDialog();
        }
    }
}
