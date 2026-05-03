using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;

#if NETFX_CORE
using System.Reflection;
#endif

using NotifyCollectionChangedEventHandler = System.Collections.Specialized.NotifyCollectionChangedEventHandler;
using NotifyCollectionChangedAction = System.Collections.Specialized.NotifyCollectionChangedAction;
using NotifyCollectionChangedEventArgs = System.Collections.Specialized.NotifyCollectionChangedEventArgs;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;
using PropertyChangedEventHandler = System.ComponentModel.PropertyChangedEventHandler;

namespace DH.UIFramework.Observables
{
    [Serializable]
    public class ObservableList<T> : IList<T>, IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs CountEventArgs = new("Count");
        private static readonly PropertyChangedEventArgs IndexerEventArgs = new("Item[]");

        private readonly object propertyChangedLock = new();
        private readonly object collectionChangedLock = new();
        private List<PropertyChangedEventHandler>   propertyChanged;
        private List<NotifyCollectionChangedEventHandler>   collectionChanged;

        private SimpleMonitor monitor = new();
        private IList<T> items;

        [NonSerialized] private object syncRoot;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                lock (propertyChangedLock)
                {
                    propertyChanged ??= ListPool<PropertyChangedEventHandler>.Get();
                    propertyChanged.Add(value);
                }
            }
            remove
            {
                lock (propertyChangedLock)
                {
                    propertyChanged.Remove(value);
                    if (propertyChanged.Count == 0)
                    {
                        ListPool<PropertyChangedEventHandler>.Release(propertyChanged);
                        propertyChanged = null;
                    }
                }
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                lock (collectionChangedLock)
                {
                    collectionChanged ??= ListPool<NotifyCollectionChangedEventHandler>.Get();
                    collectionChanged.Add(value);
                }
            }
            remove
            {
                lock (collectionChangedLock)
                {
                    collectionChanged.Remove(value);
                    if (collectionChanged.Count == 0)
                    {
                        ListPool<NotifyCollectionChangedEventHandler>.Release(collectionChanged);
                        collectionChanged = null;
                    }
                }
            }
        }

        public ObservableList()
        {
            items = new List<T>();
        }

        public ObservableList(List<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            items = new List<T>();
            using (IEnumerator<T> enumerator = list.GetEnumerator())
            {
                while (enumerator.MoveNext()) items.Add(enumerator.Current);
            }
        }

        public int Count => items.Count;

        protected IList<T> Items => items;

        public T this[int index]
        {
            get => items[index];
            set
            {
                if (items.IsReadOnly)
                    throw new NotSupportedException("ReadOnlyCollection");

                if (index < 0 || index >= items.Count)
                    throw new ArgumentOutOfRangeException(string.Format("ArgumentOutOfRangeException:{0}", index));

                SetItem(index, value);
            }
        }

        public int FindIndex(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException();

            var size = items.Count;
            for (var i = 0; i < size; i++)
                if (match(items[i]))
                    return i;
            return -1;
        }

        public T Find(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException();

            var size = items.Count;
            for (var i = 0; i < size; i++)
                if (match(items[i]))
                    return items[i];
            return default;
        }

        public int FindLastIndex(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException();

            var size = items.Count;
            for (var i = size - 1; i >= 0; i--)
                if (match(items[i]))
                    return i;
            return -1;
        }

        public T FindLast(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException();

            var size = items.Count;
            for (var i = size - 1; i >= 0; i--)
                if (match(items[i]))
                    return items[i];
            return default;
        }

        public List<T> FindAll(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException();

            var list = new List<T>();
            var size = items.Count;
            for (var i = 0; i < size; i++)
                if (match(items[i]))
                    list.Add(items[i]);
            return list;
        }

        public void Add(T item)
        {
            if (items.IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            var index = items.Count;
            InsertItem(index, item);
        }

        public void Sort()
        {
            (items as List<T>)?.Sort();
        }

        public void Clear()
        {
            if (items.IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            ClearItems();
        }

        public void CopyTo(T[] array, int index)
        {
            items.CopyTo(array, index);
        }

        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        public List<T>.Enumerator GetEnumerator()
        {
            return (items as List<T>)?.GetEnumerator() ?? new List<T>.Enumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (items.IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            if (index < 0 || index > items.Count)
                throw new ArgumentOutOfRangeException(string.Format("ArgumentOutOfRangeException:{0}", index));

            InsertItem(index, item);
        }

        public bool Remove(T item)
        {
            if (items.IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            var index = items.IndexOf(item);
            if (index < 0)
                return false;
            RemoveItem(index);
            return true;
        }

        public bool Remove(Func<T, bool> predicate)
        {
            if (items.IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");
            var index = -1;
            for (var i = 0; i < items.Count; i++)
                if (predicate(items[i]))
                {
                    index = i;
                    break;
                }

            if (index == -1) return false;

            RemoveItem(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (items.IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            if (index < 0 || index >= items.Count)
                throw new ArgumentOutOfRangeException(string.Format("ArgumentOutOfRangeException:{0}", index));

            RemoveItem(index);
        }

        public void Move(int oldIndex, int newIndex)
        {
            MoveItem(oldIndex, newIndex);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (items.IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            var index = items.Count;
            InsertItem(index, collection);
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (items.IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            if (index < 0 || index > items.Count)
                throw new ArgumentOutOfRangeException(string.Format("ArgumentOutOfRangeException:{0}", index));

            InsertItem(index, collection);
        }

        public void RemoveRange(int index, int count)
        {
            if (items.IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            if (index < 0 || index >= items.Count)
                throw new ArgumentOutOfRangeException(string.Format("ArgumentOutOfRangeException:{0}", index));

            RemoveItem(index, count);
        }
        
        public void RemoveAll(Predicate<T> match)
        {
            for (int i = Count - 1; i >= 0; --i)
            {
                if (!match(items[i]))
                {
                    continue;
                }
                
                RemoveItem(i);
            }
        }

        bool ICollection<T>.IsReadOnly => items.IsReadOnly;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)items).GetEnumerator();
        }

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot
        {
            get
            {
                if (syncRoot == null)
                {
                    var c = items as ICollection;
                    if (c != null)
                        syncRoot = c.SyncRoot;
                    else
                        Interlocked.CompareExchange<object>(ref syncRoot, new object(), null);
                }

                return syncRoot;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (array.Rank != 1)
                throw new ArgumentException("RankMultiDimNotSupported");

            if (array.GetLowerBound(0) != 0)
                throw new ArgumentException("NonZeroLowerBound");

            if (index < 0)
                throw new ArgumentOutOfRangeException(string.Format("ArgumentOutOfRangeException:{0}", index));

            if (array.Length - index < Count)
                throw new ArgumentException("ArrayPlusOffTooSmall");

            var tArray = array as T[];
            if (tArray != null)
            {
                items.CopyTo(tArray, index);
            }
            else
            {
                var targetType = array.GetType().GetElementType();
                var sourceType = typeof(T);
                if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType)))
                    throw new ArgumentException("InvalidArrayType");

                var objects = array as object[];
                if (objects == null)
                    throw new ArgumentException("InvalidArrayType");

                var count = items.Count;
                try
                {
                    for (var i = 0; i < count; i++) objects[index++] = items[i];
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArgumentException("InvalidArrayType");
                }
            }
        }

        object IList.this[int index]
        {
            get => items[index];
            set
            {
                if (value == null && !(default(T) == null))
                    throw new ArgumentNullException("value");

                try
                {
                    this[index] = (T)value;
                }
                catch (InvalidCastException e)
                {
                    throw new ArgumentException("", e);
                }
            }
        }

        bool IList.IsReadOnly => items.IsReadOnly;

        bool IList.IsFixedSize
        {
            get
            {
                var list = items as IList;
                if (list != null) return list.IsFixedSize;
                return items.IsReadOnly;
            }
        }

        int IList.Add(object value)
        {
            if (items.IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            if (value == null && !(default(T) == null))
                throw new ArgumentNullException("value");

            try
            {
                Add((T)value);
            }
            catch (InvalidCastException e)
            {
                throw new ArgumentException("", e);
            }

            return Count - 1;
        }

        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value)) return Contains((T)value);
            return false;
        }

        int IList.IndexOf(object value)
        {
            if (IsCompatibleObject(value)) return IndexOf((T)value);
            return -1;
        }

        void IList.Insert(int index, object value)
        {
            if (items.IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            if (value == null && !(default(T) == null))
                throw new ArgumentNullException("value");

            try
            {
                Insert(index, (T)value);
            }
            catch (InvalidCastException e)
            {
                throw new ArgumentException("", e);
            }
        }

        void IList.Remove(object value)
        {
            if (items.IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            if (IsCompatibleObject(value)) Remove((T)value);
        }

        protected virtual void ClearItems()
        {
            CheckReentrancy();
            items.Clear();
            OnPropertyChanged(CountEventArgs);
            OnPropertyChanged(IndexerEventArgs);
            OnCollectionReset();
        }

        protected virtual void RemoveItem(int index)
        {
            CheckReentrancy();
            var removedItem = this[index];

            items.RemoveAt(index);

            OnPropertyChanged(CountEventArgs);
            OnPropertyChanged(IndexerEventArgs);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItem, index);
        }

        protected virtual void RemoveItem(int index, int count)
        {
            CheckReentrancy();

            var list = items as List<T>;
            var changedItems = list.GetRange(index, count);
            list.RemoveRange(index, count);

            OnPropertyChanged(CountEventArgs);
            OnPropertyChanged(IndexerEventArgs);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, changedItems, index);
        }

        protected virtual void InsertItem(int index, T item)
        {
            CheckReentrancy();

            items.Insert(index, item);

            OnPropertyChanged(CountEventArgs);
            OnPropertyChanged(IndexerEventArgs);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        protected virtual void InsertItem(int index, IEnumerable<T> collection)
        {
            CheckReentrancy();

            (items as List<T>).InsertRange(index, collection);

            OnPropertyChanged(CountEventArgs);
            OnPropertyChanged(IndexerEventArgs);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, ToList(collection), index);
        }

        protected virtual void SetItem(int index, T item)
        {
            CheckReentrancy();
            var originalItem = this[index];

            items[index] = item;

            OnPropertyChanged(IndexerEventArgs);
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, originalItem, item, index);
        }

        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            CheckReentrancy();

            var removedItem = this[oldIndex];

            items.RemoveAt(oldIndex);
            items.Insert(newIndex, removedItem);

            OnPropertyChanged(IndexerEventArgs);
            OnCollectionChanged(NotifyCollectionChangedAction.Move, removedItem, newIndex, oldIndex);
        }


        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (propertyChanged != null)
            {
                var tempList = ListPool<PropertyChangedEventHandler>.Get();
                try
                {
                    // 防止迭代器失效
                    lock (propertyChangedLock)
                    {
                        tempList.AddRange(propertyChanged);
                    }

                    foreach (var item in tempList)
                    {
                        item(this, e);
                    }
                }
                finally
                {
                    ListPool<PropertyChangedEventHandler>.Release(tempList);
                }
            }
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (collectionChanged != null)
                using (BlockReentrancy())
                {
                    var tempList = ListPool<NotifyCollectionChangedEventHandler>.Get();
                    try
                    {
                        // 防止迭代器失效
                        lock (collectionChangedLock)
                        {
                            tempList.AddRange(collectionChanged);
                        }

                        foreach (var item in tempList)
                        {
                            item(this, e);
                        }
                    }
                    finally
                    {
                        ListPool<NotifyCollectionChangedEventHandler>.Release(tempList);
                    }
                }
        }

        protected IDisposable BlockReentrancy()
        {
            monitor.Enter();
            return monitor;
        }

        protected void CheckReentrancy()
        {
            if (monitor.Busy)
                if (collectionChanged != null && collectionChanged.Count > 1)
                    throw new InvalidOperationException();
        }

        private IList ToList(IEnumerable<T> collection)
        {
            if (collection is IList)
                return (IList)collection;

            var list = new List<T>();
            list.AddRange(collection);
            return list;
        }

        private static bool IsCompatibleObject(object value)
        {
            return value is T || (value == null && default(T) == null);
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            if (collectionChanged != null)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, IList changedItems, int index)
        {
            if (collectionChanged != null)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, changedItems, index));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            if (collectionChanged != null)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem,
            int index)
        {
            if (collectionChanged != null)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        private void OnCollectionReset()
        {
            if (collectionChanged != null)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        [Serializable()]
        private class SimpleMonitor : IDisposable
        {
            private int _busyCount;
            public bool Busy => _busyCount > 0;

            public void Enter()
            {
                ++_busyCount;
            }

            public void Dispose()
            {
                --_busyCount;
            }
        }
    }
}