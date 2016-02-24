using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.ComponentModel
{
    public class DispatchSafeList<T> : AsyncList<T>
    {
        private readonly Action<Action> _dispatcher;
        public DispatchSafeList(Action<Action> dispatcherCall)
        {
            _dispatcher = dispatcherCall;
            base.CollectionChanged += OnCollectionChanged;
            base.PropertyChanged += OnPropertyChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        { _dispatcher(() => CollectionChanged?.Invoke(sender, args)); }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        { _dispatcher(() => PropertyChanged?.Invoke(sender, args)); }

        public override event NotifyCollectionChangedEventHandler CollectionChanged;
        public override event PropertyChangedEventHandler PropertyChanged;
    }
}
