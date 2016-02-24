using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace System.ComponentModel
{
    class ThreadSafePropertyChangeNotifier : PropertyChangeNotifier
    {
        AsyncLock _lock = new AsyncLock();

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
    }
}
