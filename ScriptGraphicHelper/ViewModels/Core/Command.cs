using System;
using System.Windows.Input;

namespace ScriptGraphicHelper.ViewModels.Core
{
    public class Command : ICommand
    {
        private Action<object?>? _execute;
        private Predicate<object?>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public Command(Action<object?>? execute = null, Predicate<object?>? canExecute = null)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }

        public virtual void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object? parameter)
        {
            return this._canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            this._execute?.Invoke(parameter);
        }
    }
}
