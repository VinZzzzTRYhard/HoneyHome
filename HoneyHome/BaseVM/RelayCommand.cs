using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HoneyHome.BaseVM
{
    public class RelayCommand<T> : ICommand
    {
        #region Fields

        private Action<T> action;
        private Func<T, bool> func;

        #endregion Fields

        public RelayCommand(Action<T> action)
        {
            this.action = action;
        }
        public RelayCommand(Action<T> action, Func<T, bool> func)
        {
            this.action = action;
            this.func = func;
        }

        #region Events

        public event EventHandler CanExecuteChanged;

        #endregion Events

        #region Methods

        private T CheckParam(object parameter)
        {
            T value = default(T);
            if (parameter != null && !(parameter is T))
            {
                throw new ArgumentException("Wrong parameter type.", "parameter");
            }
            if (parameter != null)
            {
                value = (T)parameter;
            }

            return value;
        }

        public void Execute(object parameter)
        {
            T value = CheckParam(parameter);
            var canExecute = CanExecute(value);
            if (canExecute)
            {
                action(value);
            }
        }

        public bool CanExecute(object parameter)
        {
            bool result = true;
            T value = CheckParam(parameter);
            if (func != null)
            {
                result = func(value);
            }
            return result;
        }

        public void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion Methods
    }
    public class RelayCommand : RelayCommand<object>
    {
        #region Constructors
        public RelayCommand(Action action)
            : base((obj) =>
            {
                if (action != null) action();
            })
        {

        }
        public RelayCommand(Action action, Func<bool> canExecute)
            : base((obj) =>
            {
                if (action != null) action();
            }, (obj) => canExecute == null || canExecute())
        {

        }

        #endregion Constructors
    }
}
