using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DataEditor.Utils
{
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _action;

        public AsyncRelayCommand(Func<Task> action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            await _action.Invoke();
        }

        public event EventHandler CanExecuteChanged;
    }
}
