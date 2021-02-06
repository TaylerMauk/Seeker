using System.Windows;

namespace SeekerCore.Views
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
            this.DataContext = Owner.DataContext;
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
