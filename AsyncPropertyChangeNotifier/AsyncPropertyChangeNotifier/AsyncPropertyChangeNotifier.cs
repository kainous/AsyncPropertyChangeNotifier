using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Async;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace System.ComponentModel
{
    public interface IValidatePropertyChanging
    {
        bool PropertyCanChange(string propertyName, object potentialValue);
    }

    public class ICoreProperty<T>
    {
        //public 

        //private void SetValue
    }

    public class AsyncList<T> 
        : IList<T>
        , INotifyCollectionChanged
        , INotifyPropertyChanged
    {
        private readonly List<T> _items = new List<T>();
        private readonly AsyncReaderWriterLock _lock = new AsyncReaderWriterLock();

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        #region GetAt

        public async Task<T> GetAtAsync(Task<int> index)
        {
            using (await _lock.ReaderLockAsync())
            { return _items[await index]; }
        }

        public Task<T> GetAtAsync(int index)
        { return GetAtAsync(Task.FromResult(index)); }

        public T GetAt(int index)
        {
            using (_lock.ReaderLock())
            { return _items[index]; }
        }

        #endregion

        #region SetAt
        public async Task SetAtAsync(Task<int> index, Task<T> value)
        {
            var lockTask = _lock.WriterLockAsync();

            await Task.WhenAll(index, value, lockTask);
            int innerIndex = await index;
            T innerValue = await value;

            using (await lockTask)
            { _items[innerIndex] = innerValue; }

            CollectionChanged.Replace(this, innerIndex, innerValue);
        }

        public Task SetAtAsync(int index, Task<T> value)
        { return SetAtAsync(Task.FromResult(index), value); }

        public Task SetAtAsync(int index, T value)
        { return SetAtAsync(Task.FromResult(index), Task.FromResult(value)); }

        public void SetAt(int index, T value)
        {
            using (_lock.WriterLock())
            { _items[index] = value; }

            CollectionChanged.Replace(this, index, value);
        }
        #endregion

        #region Public IList<T> members
        public T this[int index]
        {
            get { return GetAt(index); }
            set { SetAt(index, value); }
        }

        public int Count
        {
            get
            {
                using (_lock.ReaderLock())
                { return _items.Count; }
            }
        }
        #endregion

        #region Explicit ICollection members

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                using (_lock.ReaderLock())
                { return ((ICollection<T>)_items).IsReadOnly; }
            }
        }

        #endregion        

        public async Task InsertAsync(Task<int> index, Task<IEnumerable<T>> items)
        {
            var newItems = await items;
            if (newItems == null)
            { throw new ArgumentNullException(nameof(items)); }

            using (await _lock.WriterLockAsync())
            { _items.InsertRange(await index, newItems); }
        }


        public async Task<int> AddAsync(Task<IEnumerable<T>> items)
        {
            using (await _lock.UpgradeableReaderLockAsync())
            {
                var index = _items.Count;
                await InsertAsync(Task.FromResult(index), items);
                return index;
            }
        }


        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }




    public class AsyncPropertyChangeNotifier
        : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private BlockingCollection<IValidatePropertyChanging> _PropertyChangingSubscribers
            = new BlockingCollection<IValidatePropertyChanging>();

        public void SubscribeToPropertyChangingValidation(IValidatePropertyChanging subscriber)
        {
            _PropertyChangingSubscribers.Add(subscriber);


        }

        public void UnsubscribeFromPropertyChangingValidation(IValidatePropertyChanging subscriber)
        {
            _PropertyChangingSubscribers.t
            }

        protected virtual async Task<bool> CanPropertyChange(
            object potentialValue
          , [CallerMemberName] string propertyName = null)
        {
            foreach (var subscriber in _PropertyChangingSubscribers)
            {

            }

            var args = new CancelablePropertyChangingEventArgs(propertyName, potentialValue);
            var tasks = PropertyChanging.GetInvocationList()
                                        .OfType<EventHandler<CancelablePropertyChangingEventArgs>>()
                                        .Select(h =>
                                        {
                                            h(this, args);
                                            args.
                                        })



                from handler in PropertyChanging.GetInvocationList().OfType<EventHandler<CancelablePropertyChangingEventArgs>>()
                select Task.Run(() => handler(this, args));

            tasks.AsParallel()



            foreach (var handler in PropertyChanging.GetInvocationList())
            {

            }

            PropertyChanging?.Invoke(
                propertyName
              , new CancelablePropertyChangingEventArgs(propertyName, potentialValue));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var d = new CancelablePropertyChangingEventArgs(propertyName, ;
            d.PropertyName

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
