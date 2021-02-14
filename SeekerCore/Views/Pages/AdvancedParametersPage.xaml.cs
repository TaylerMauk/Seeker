using SeekerCore.Views.Windows;
using System.Windows;
using System.Windows.Controls;

namespace SeekerCore.Views.Pages
{
    /// <summary>
    /// Interaction logic for AdvancedParametersPage.xaml
    /// </summary>
    public partial class AdvancedParametersPage : Page
    {
        public Window Owner { get; set; }

        public AdvancedParametersPage()
        {
            InitializeComponent();
        }

        private void OnAddSearchDirectoryButtonClick(object sender, RoutedEventArgs e)
        {
            new AddSearchDirectoryDialog(Owner).ShowDialog();
        }
    }
}
