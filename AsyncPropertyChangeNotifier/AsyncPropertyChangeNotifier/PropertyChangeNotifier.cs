using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace System.ComponentModel
{
    public abstract class PropertyChangeNotifier
        : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private AsyncList<IValidatePropertyChanging> _PropertyChangingSubscribers
            = new AsyncList<IValidatePropertyChanging>();

        public IDisposable SubscribeToPropertyChangingValidation(IValidatePropertyChanging subscriber)
        {
            _PropertyChangingSubscribers.Add(subscriber);
            return new DisposableAction(() => _PropertyChangingSubscribers.Remove(subscriber));
        }

        protected virtual bool CanPropertyChange(
            object potentialValue
          , [CallerMemberName] string propertyName = null)
        {
            var pcs = _PropertyChangingSubscribers;
            var testResult = pcs.AsParallel()
                                .All(o => o.PropertyCanChange(propertyName, potentialValue));
            return testResult;
        }

        protected virtual void OnPropertyChanged(
            [CallerMemberName] string propertyName = null)
        => PropertyChanged.FireAndForget(this, propertyName);

        protected virtual T GetValue<T>(ref T backingField) => backingField;

        protected virtual void SetValue<T>(
            ref T backingField
          , T value
          , [CallerMemberName] string propertyName = null)
        {
            if (!object.Equals(backingField, value) && CanPropertyChange(value, propertyName))
            {
                backingField = value;
                OnPropertyChanged(propertyName);
            }
        }

        protected virtual T GetValue<T>(ICoreProperty<T> coreProperty) => coreProperty.Recent.Value;

        protected virtual void SetValue<T>(
            ICoreProperty<T> coreProperty
          , IDataChangeEvent<T> eventData)

            => coreProperty.Recent = eventData;

        protected void SetValue<T>(ICoreProperty<T> coreProperty, T value) =>
            SetValue(coreProperty, new DataChangeEvent<T>(value));

        protected static ICoreProperty<T> CreateProperty<T>(
            string propertyName
          , T defaultValue = default(T))

          => new CoreProperty<T>(propertyName, defaultValue);
    }
}
