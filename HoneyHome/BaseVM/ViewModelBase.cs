using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HoneyHome.BaseVM
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private readonly static object TrueBox = true, FalseBox = false;

        private Dictionary<string, object> values = null;

        public event PropertyChangedEventHandler PropertyChanged;

        protected T Get<T>([CallerMemberName] string propertyName = null)
        {
            return (T)(this[propertyName] ?? default(T));
        }

        protected bool Set<T>(T value, [CallerMemberName] string propertyName = null)
        {
            object result = null;
            values?.TryGetValue(propertyName, out result);

            if (!System.Object.Equals(value, result))
            {
                this[propertyName] = value;
                RaisePropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        protected bool Set(bool value, [CallerMemberName] string propertyName = null)
        {
            object result = null;
            values?.TryGetValue(propertyName, out result);

            if (result == null || (value && result == FalseBox) || (!value && result == TrueBox))
            {
                this[propertyName] = value ? TrueBox : FalseBox;
                RaisePropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private object this[string property]
        {
            get
            {
                object result = null;
                values?.TryGetValue(property, out result);
                return result;
            }

            set
            {
                if (values == null)
                    values = new Dictionary<string, object>();

                values[property] = value;
            }
        }

        protected void Initialize(Dictionary<string, object> initialValues)
        {
            if (values == null)
                values = new Dictionary<string, object>(initialValues);
            else
                throw new Exception($"ViewModelBase '{this.GetType().FullName}' already initialize");
        }


        #region Command Generation

        protected RelayCommand CreateCommand(Action action, [CallerMemberName] string propertyName = null)
                                => GetCommand<RelayCommand>(action, null, propertyName);

        protected RelayCommand CreateCommand(Action action, Func<bool> canExecuteFunc, [CallerMemberName] string propertyName = null)
                                => GetCommand<RelayCommand>(action, canExecuteFunc, propertyName);


        protected RelayCommand<T> CreateCommand<T>(Action<T> action, [CallerMemberName] string propertyName = null)
                                => GetCommand<RelayCommand<T>>(action, null, propertyName);

        protected RelayCommand<T> CreateCommand<T>(Action<T> action, Func<T, bool> canExecuteFunc, [CallerMemberName] string propertyName = null)
                                => GetCommand<RelayCommand<T>>(action, canExecuteFunc, propertyName);

        private T GetCommand<T>(Delegate action, Delegate canExecuteFunc, string propertyName)
        {
            T result = Get<T>(propertyName);
            if (result == null)
            {
                result = (T)Activator.CreateInstance(typeof(T), action, canExecuteFunc);

                this[propertyName] = result;
            }
            return result;
        }

        #endregion
    }
}
