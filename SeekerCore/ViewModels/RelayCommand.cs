using System;
using System.Windows.Input;

namespace SeekerCore.ViewModels
{
    class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action m_action;
        private Func<bool> m_canExecuteAction;

        public RelayCommand(Action action, Func<bool> canExecuteAction)
        {
            m_action = action;
            m_canExecuteAction = canExecuteAction;
        }

        public bool CanExecute(object parameter)
        {
            return m_canExecuteAction();
        }

        public void Execute(object parameter)
        {
            m_action();
        }

        public void RaiseCanExecuteChanged()
        {
            if (null == this)
                return;

            CanExecuteChanged(this, null);
        }
    }
}
