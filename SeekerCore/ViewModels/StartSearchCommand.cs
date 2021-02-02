using System;
using System.Windows.Input;

namespace SeekerCore.ViewModels
{
    class StartSearchCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool Executable { get; set; }

        private Action m_action;
        private Func<bool> m_canExecuteAction;

        public StartSearchCommand(Action action, Func<bool> canExecuteAction)
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
            CanExecuteChanged(this, null);
        }
    }
}
