using System;
using System.Windows.Input;

namespace CameraCaptureWPF.Helpers
{
    public class RelayCommand : ICommand
    {
        private readonly Func<bool> targetCanExecuteMethod;
        private readonly Action targetExecuteMethod;

        public RelayCommand(Action executeMethod)
        {
            targetExecuteMethod = executeMethod;
        }

        public RelayCommand(Action executeMethod, Func<bool> canExecuteMethod)
        {
            targetExecuteMethod = executeMethod;
            targetCanExecuteMethod = canExecuteMethod;
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (targetCanExecuteMethod != null) return targetCanExecuteMethod();
            if (targetExecuteMethod != null) return true;
            return false;
        }

        // Beware - should use weak references if command instance lifetime is longer than lifetime
        // of UI objects that get hooked up to command
        // Prism commands solve this in their implementation
        public event EventHandler CanExecuteChanged = delegate { };

        void ICommand.Execute(object parameter)
        {
            targetExecuteMethod?.Invoke();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}