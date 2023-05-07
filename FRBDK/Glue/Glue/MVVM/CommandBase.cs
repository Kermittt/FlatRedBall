using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FlatRedBall.Glue.MVVM
{
    public class CommandBase : ICommand
    {
        public event EventHandler CanExecuteChanged;

        Action execute;
        Func<bool> canExecuteFunc;

        public CommandBase(Action execute, Func<bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecuteFunc = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (canExecuteFunc != null)
            {
                return canExecuteFunc();
            }
            else
            {
                return true;
            }
        }

        public void Execute(object parameter)
        {
            execute?.Invoke();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
