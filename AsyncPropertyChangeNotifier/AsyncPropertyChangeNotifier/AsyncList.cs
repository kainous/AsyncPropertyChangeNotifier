using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace System.ComponentModel
{
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

        protected virtual T GetAtCore(int index)
        { return _items[index]; }

        public async Task<T> GetAtAsync(Task<int> index, CancellationToken token)
        {
            using (await _lock.ReaderLockAsync(token))
            { return GetAtCore(await index); }
        }

        public Task<T> GetAtAsync(Task<int> index)
        { return GetAtAsync(index, CancellationToken.None); }

        public Task<T> GetAtAsync(int index)
        { return GetAtAsync(Task.FromResult(index), CancellationToken.None); }

        public T GetAt(int index)
        {
            using (_lock.ReaderLock())
            { return GetAtCore(index); }
        }

        #endregion

        #region SetAt

        protected virtual void SetAtCore(int index, T value)
        { _items[index] = value; }

        public async Task SetAtAsync(Task<int> index, Task<T> value, CancellationToken token)
        {
            int innerIndex = await index;
            T innerValue = await value;

            using (await _lock.WriterLockAsync(token))
            { SetAtCore(innerIndex, innerValue); }

            CollectionChanged.Replace(this, innerIndex, innerValue);
        }

        public Task SetAtAsync(Task<int> index, Task<T> value)
        { return SetAtAsync(index, value, CancellationToken.None); }

        public Task SetAtAsync(int index, Task<T> value)
        { return SetAtAsync(Task.FromResult(index), value); }

        public Task SetAtAsync(int index, T value)
        { return SetAtAsync(Task.FromResult(index), Task.FromResult(value)); }

        public void SetAt(int index, T value)
        {
            using (_lock.WriterLock())
            { SetAtCore(index, value); }

            CollectionChanged.Replace(this, index, value);
        }
        #endregion

        #region Public IList<T> members
        public T this[int index]
        {
            get { return GetAt(index); }
            set { SetAt(index, value); }
        }

        protected virtual long GetCountCore()
        { return _items.Count; }

        public int Count
        {
            get
            {
                using (_lock.ReaderLock())
                { return (int)GetCountCore(); }
            }
        }
        #endregion

        #region Explicit ICollection members

        protected virtual bool GetIsReadOnlyCore()
        { return ((ICollection<T>)_items).IsReadOnly; }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                using (_lock.ReaderLock())
                { return GetIsReadOnlyCore(); }
            }
        }

        void ICollection<T>.Add(T item)
        { Add(new[] { item }); }

        #endregion

        #region Insertion logic

        protected virtual void InsertCore(int index, IEnumerable<T> items)
        { _items.InsertRange(index, items); }

        public async Task InsertAsync(Task<int> index, Task<IEnumerable<T>> items, CancellationToken token)
        {
            var newItems = await items;
            if (newItems != null)
            {
                int newIndex = await index;

                //Must not await within the lock
                using (await _lock.WriterLockAsync(token))
                { InsertCore(newIndex, newItems); }

                CollectionChanged.Add(this, newIndex, newItems);
                PropertyChanged?.FireAndForget(this, nameof(Count));
            }
        }

        public async Task<int> AddAsync(Task<IEnumerable<T>> items)
        {
            //Must not await inside a lock
            var newItems = await items;
            if (newItems != null)
            {
                using (await _lock.UpgradeableReaderLockAsync())
                {
                    var index = _items.Count;
                    await InsertAsync(Task.FromResult(index), items);
                    return index;
                }
            }
            return -1;
        }

        public Task InsertAsync(int index, Task<IEnumerable<T>> items)
        { return InsertAsync(Task.FromResult(index), items); }

        public Task InsertAsync(int index, IEnumerable<T> items)
        { return InsertAsync(Task.FromResult(index), Task.FromResult(items)); }

        public void Insert(int index, IEnumerable<T> items)
        {
            if (items != null)
            {
                using (_lock.WriterLock())
                { InsertCore(index, items); }

                CollectionChanged.Add(this, index, (object)items);
                PropertyChanged?.FireAndForget(this, nameof(Count));
            }
        }

        public void Insert(int index, params T[] items)
        { Insert(index, items); }

        public Task<int> AddAsync(IEnumerable<T> items)
        { return AddAsync(Task.FromResult(items)); }

        public int Add(IEnumerable<T> items)
        {
            using (_lock.UpgradeableReaderLock())
            {
                var index = _items.Count;
                Insert(index, items);
                return index;
            }
        }

        #endregion

        public Task ClearAsync()
        {
            using (await _lock.WriterLockAsync())
            { }
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
}
