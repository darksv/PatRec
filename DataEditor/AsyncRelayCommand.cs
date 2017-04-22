using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DataEditor
{
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object, Task> _action;

        private readonly Predicate<object> _canExecute;

        public AsyncRelayCommand(Func<object, Task> action, Predicate<object> canExecute = null)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public async void Execute(object parameter)
        {
            await _action.Invoke(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}
