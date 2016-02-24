using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace System.ComponentModel
{
    class ThreadSafePropertyChangeNotifier : PropertyChangeNotifier
    {
        AsyncLock _lock = new AsyncLock();       

        protected async Task<T> WhileLockedAsync<T>(Func<T> func, CancellationToken token)
        {
            using (await _lock.LockAsync(token))
            { return func(); }
        }

        protected Task<T> WhileLockedAsync<T>(Func<T> func)
        { return WhileLockedAsync(func, CancellationToken.None); }

        protected async Task WhileLockedAsync(Action action, CancellationToken token)
        {
            using (await _lock.LockAsync(token))
            { action(); }
        }

        protected Task WhileLockedAsync(Action action)
        { return WhileLockedAsync(action, CancellationToken.None); }

        protected T WhileLocked<T>(Func<T> func)
        {
            using (_lock.Lock())
            { return func(); }
        }

        protected void WhileLocked(Action action)
        {
            using (_lock.Lock())
            { action(); }
        }

        protected override T GetValue<T>(ref T backingField)
        {
            using (_lock.Lock())
            { return base.GetValue<T>(ref backingField); }
        }

        protected override void SetValue<T>(
            ref T backingField
          , T value
          , [CallerMemberName] string propertyName = null)
        {
            using (_lock.Lock())
            { base.SetValue(ref backingField, value, propertyName); }
        }

        protected override T GetValue<T>(ICoreProperty<T> coreProperty) => 
            WhileLocked(() => base.GetValue(coreProperty));

        protected Task<T> GetValueAsync<T>(ICoreProperty<T> coreProperty) => 
            WhileLockedAsync(() => base.GetValue(coreProperty));

        protected override void SetValue<T>(ICoreProperty<T> coreProperty, IDataChangeEvent<T> eventData) => 
            WhileLocked(() => base.SetValue(coreProperty, eventData));

        protected Task SetValueAsync<T>(ICoreProperty<T> coreProperty, IDataChangeEvent<T> eventData) => 
            WhileLockedAsync(() => base.SetValue(coreProperty, eventData));
    }
}
