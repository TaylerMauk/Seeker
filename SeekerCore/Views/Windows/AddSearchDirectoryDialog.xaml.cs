using System.Windows;
using SeekerCore.ViewModels;

namespace SeekerCore.Views.Windows
{
    /// <summary>
    /// Interaction logic for AddSearchDirectoryDialog.xaml
    /// </summary>
    public partial class AddSearchDirectoryDialog : Window
    {
        public AddSearchDirectoryDialog(Window owner)
        {
            InitializeComponent();

            this.Owner = owner;
            this.DataContext = ((MainViewModel)Owner.DataContext).SearchViewModel;

            txtBoxSearchDirectory.Focus();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnSubmitClick(object sender, RoutedEventArgs e)
        {
            DialogResult = txtBoxSearchDirectory.Text != string.Empty;
            Close();
        }
    }
}
