using System;
using System.Windows.Input;

namespace CVCapturePanel.ViewModel
{
    public class RelayCommand : ICommand
    {
        private readonly Func<bool> canExecute;
        private readonly Action execute;

        /// <summary>
        ///     Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        /// <summary>
        ///     Raised when RaiseCanExecuteChanged is called.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        ///     Determines whether this RelayCommand can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command. If the command does not require data to be passed,
        ///     this object can be set to null.
        /// </param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            return canExecute?.Invoke() ?? true;
        }

        /// <summary>
        ///     Executes the RelayCommand on the current command target.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command. If the command does not require data to be passed,
        ///     this object can be set to null.
        /// </param>
        public void Execute(object parameter)
        {
            execute();
        }

        /// <summary>
        ///     Method used to raise the CanExecuteChanged event
        ///     to indicate that the return value of the CanExecute
        ///     method has changed.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}