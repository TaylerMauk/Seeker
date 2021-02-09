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
        private MainViewModel m_mainViewModel;

        public MainWindow()
        {
            InitializeComponent();

            m_mainViewModel = new MainViewModel();

            DataContext = m_mainViewModel;
            gridSearchParams.DataContext = m_mainViewModel.SearchViewModel;
            gridSearchResults.DataContext = m_mainViewModel.ResultsViewModel;
            txtBlockResultsCount.DataContext = m_mainViewModel.ResultsViewModel;
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
