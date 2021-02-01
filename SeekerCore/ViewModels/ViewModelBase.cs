using System.ComponentModel;

namespace SeekerCore.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged(sender, e);
        }
    }
}
