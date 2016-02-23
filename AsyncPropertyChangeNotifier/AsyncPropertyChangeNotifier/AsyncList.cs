using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
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
        #region Private members

        private readonly List<T> _items = new List<T>();
        private readonly AsyncReaderWriterLock _lock = new AsyncReaderWriterLock();

        #endregion

        #region Events

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Utility methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TIn ReadLock<TIn>(Func<TIn> func)
        {
            using (_lock.ReaderLock())
            { return func(); }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TOut ReadLock<TIn, TOut>(Func<TIn, TOut> func, TIn parameter)
        {
            using (_lock.ReaderLock())
            { return func(parameter); }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<TIn> ReadLock<TIn>(Func<TIn> func, CancellationToken token)
        {
            using (await _lock.ReaderLockAsync(token))
            { return func(); }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<TOut> ReadLock<TIn, TOut>(
            Func<TIn, TOut> func
          , Task<TIn> input
          , CancellationToken token)
        {
            var newInput = await input;
            using (await _lock.ReaderLockAsync(token))
            { return func(newInput); }
        }

        #endregion

        #region GetAt

        protected virtual T GetAtCore(int index) => _items[index];

        public Task<T> GetAtAsync(Task<int> index, CancellationToken token) => ReadLock(GetAtCore, index, token);

        public Task<T> GetAtAsync(Task<int> index) => GetAtAsync(index, CancellationToken.None);

        public Task<T> GetAtAsync(int index) => GetAtAsync(Task.FromResult(index), CancellationToken.None);

        public T GetAt(int index) => ReadLock(GetAtCore, index);

        #endregion

        #region SetAt

        protected virtual void SetAtCore(int index, T value) => _items[index] = value;

        public async Task SetAtAsync(Task<int> index, Task<T> value, CancellationToken token)
        {
            int innerIndex = await index;
            T innerValue = await value;

            using (await _lock.WriterLockAsync(token))
            { SetAtCore(innerIndex, innerValue); }

            CollectionChanged.Replace(this, innerIndex, innerValue);
        }

        public Task SetAtAsync(Task<int> index, Task<T> value) => SetAtAsync(index, value, CancellationToken.None);

        public Task SetAtAsync(int index, Task<T> value) => SetAtAsync(Task.FromResult(index), value);

        public Task SetAtAsync(int index, T value) => SetAtAsync(Task.FromResult(index), Task.FromResult(value));

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

        protected virtual long GetCountCore() => _items.Count;



        public int Count => ReadLock(() => (int)GetCountCore());

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

        public Task InsertAsync(Task<int> index, Task<IEnumerable<T>> items)
        { return InsertAsync(index, items, CancellationToken.None); }

        public async Task<int> AddAsync(Task<IEnumerable<T>> items, CancellationToken token)
        {
            //Must not await inside a lock
            var newItems = await items;
            if (newItems != null)
            {
                using (await _lock.UpgradeableReaderLockAsync(token))
                {
                    var index = _items.Count;
                    await InsertAsync(Task.FromResult(index), items, token);
                    return index;
                }
            }
            return -1;
        }

        public Task<int> AddAsync(Task<IEnumerable<T>> items)
        { return AddAsync(items, CancellationToken.None); }

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

        void IList<T>.Insert(int index, T item)
        { Insert(index, item); }

        #endregion

        #region Clear logic
        protected virtual void ClearCore()
        { _items.Clear(); }

        public async Task ClearAsync(CancellationToken token)
        {
            using (await _lock.WriterLockAsync(token))
            { ClearCore(); }

            CollectionChanged.Reset(this);
            PropertyChanged?.FireAndForget(this, nameof(Count));
        }

        public Task ClearAsync()
        { return ClearAsync(CancellationToken.None); }

        public void Clear()
        {
            using (_lock.WriterLock())
            { ClearCore(); }

            CollectionChanged.Reset(this);
            PropertyChanged?.FireAndForget(this, nameof(Count));
        }

        #endregion

        #region CopyTo logic

        public async Task<IReadOnlyList<T>> AsReadonlyCopy()
        {
            T[] copy;
            using (await _lock.ReaderLockAsync())
            {
                copy = new T[_items.Count];
                _items.CopyTo(copy);
            }

            return copy.ToList().AsReadOnly();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            using (_lock.ReaderLock())
            { _items.CopyTo(array, arrayIndex); }
        }

        #endregion

        #region Enumerator logic
        public IEnumerator<T> GetEnumerator()
        {
            T[] copy;
            using (_lock.ReaderLock())
            {
                copy = new T[_items.Count];
                CopyTo(copy, 0);
            }

            foreach (var item in copy)
            { yield return item; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Searching logic

        public async Task<bool> ContainsAsync(
            Task<T> item
          , IEqualityComparer<T> comparer
          , CancellationToken token)
        {
            var newItem = await item;
            using (await _lock.ReaderLockAsync(token))
            { return _items.Contains(newItem, comparer); }
        }
        public Task<bool> ContainsAsync(Task<T> item, IEqualityComparer<T> comparer)
        { return ContainsAsync(item, comparer, CancellationToken.None); }

        public async Task<bool> ContainsAsync(Task<T> item)
        {
            var newItem = await item;
            using (await _lock.ReaderLockAsync())
            { return _items.Contains(newItem); }
        }

        public Task<bool> ContainsAsync(T item)
        { return ContainsAsync(Task.FromResult(item)); }

        public bool Contains(T item)
        {
            using (_lock.ReaderLock())
            { return _items.Contains(item); }
        }

        protected virtual int IndexOfCore(T item)
        { return _items.IndexOf(item); }

        public async Task<int> IndexOfAsync(Task<T> item, CancellationToken token)
        {
            var newItem = await item;
            using (await _lock.ReaderLockAsync(token))
            { return IndexOfCore(newItem); }
        }

        public Task<int> IndexOfAsync(Task<T> item)
        { return IndexOfAsync(item, CancellationToken.None); }

        public int IndexOf(T item)
        {
            using (_lock.ReaderLock())
            { return IndexOfCore(item); }
        }

        #endregion

        #region Remove logic
        private IEnumerable<KeyValuePair<int, T>> GetReversedEnumerable()
        {
            for (int i = Count - 1; i >= 0; i--)
            { yield return new KeyValuePair<int, T>(i, GetAt(i)); }
        }

        protected virtual void RemoveAtCore(int index)
        { _items.RemoveAt(index); }

        public async Task<IDictionary<int, T>> RemoveWhere(
            Func<int, T, bool> condition
          , CancellationToken token)
        {
            var result = new Dictionary<int, T>();
            long itemCount;
            using (await _lock.WriterLockAsync(token))
            {
                itemCount = GetCountCore();
                foreach (var kvp in GetReversedEnumerable())
                {
                    RemoveAtCore(kvp.Key);
                    result.Add(kvp.Key, kvp.Value);
                }
            }

            CollectionChanged.NotifyRemove(this, itemCount, result);
            PropertyChanged?.FireAndForget(this, nameof(Count));
            return result;
        }

        public Task<IDictionary<int, T>> RemoveWhereAsync(
            Func<T, bool> condition
          , CancellationToken token) => RemoveWhere((_, item) => condition(item), token);

        public Task<IDictionary<int, T>> RemoveWhereAsync(Func<T, bool> condition)
            => RemoveWhereAsync(condition, CancellationToken.None);


        public async Task RemoveAtAsync(Task<int> index)
        {
            long itemCount;
            T removedItem;
            var newIndex = await index;
            using (await _lock.WriterLockAsync())
            {
                itemCount = GetCountCore();
                removedItem = GetAtCore(newIndex);
                RemoveAtCore(newIndex);
            }

            CollectionChanged.NotifyRemove(
                this
              , itemCount
              , new Dictionary<int, T> { { newIndex, removedItem } });

            PropertyChanged.FireAndForget(this, nameof(Count));
        }

        public Task RemoveAtAsync(int index)
        { return RemoveAtAsync(Task.FromResult(index)); }

        public async Task<int> RemoveAsync(Task<T> item)
        {
            var newItem = await item;
            using (await _lock.UpgradeableReaderLockAsync())
            {
                int index = IndexOfCore(newItem);
                if (index > 0)
                { await RemoveAtAsync(index); }
                return index;
            }

        }

        public Task<int> RemoveAsync(T item)
        { return RemoveAsync(Task.FromResult(item)); }

        public void RemoveAt(int index)
        {
            long itemCount;
            T removedItem;

            using (_lock.WriterLock())
            {
                itemCount = GetCountCore();
                removedItem = GetAtCore(index);
                RemoveAtCore(index);
            }

            CollectionChanged.NotifyRemove(
                this
              , itemCount
              , new Dictionary<int, T> { { index, removedItem } });

            PropertyChanged.FireAndForget(this, nameof(Count));
        }

        public int Remove(T item)
        {
            using (_lock.UpgradeableReaderLock())
            {
                int index = IndexOfCore(item);
                if (index >= 0)
                { RemoveAt(index); }
                return index;
            }
        }

        bool ICollection<T>.Remove(T item)
        { return Remove(item) != -1; }

        #endregion
    }
}
