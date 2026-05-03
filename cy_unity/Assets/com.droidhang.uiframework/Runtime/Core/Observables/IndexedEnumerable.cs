using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

namespace DH.UIFramework.Observables
{
    /// <summary>
    /// 参考WPF IndexedEnumerable进行修改
    /// 过滤对象时只会修改迭代器部分逻辑，不需要对该对象进行删除操作
    /// 排序时每次均需要把对象添加到sortCollection进行排序，同时切换迭代器到sortCollection
    /// </summary>
    public class IndexedEnumerable : IEnumerable
    {
        private IEnumerable _enumerable; //可迭代的集合对象
        private IEnumerator _enumerator; //当前迭代器
        private IEnumerator _changeTracker;
        private ICollection _collection;
        private ArrayList _sortCollection;
        private IList _list;
        private int _enumeratorVersion;
        private object _cachedItem;
        private int _cachedIndex = -1;
        private int _cachedVersion = -1;
        private int _cachedCount = -1;
        private bool? _cachedIsEmpty;
        private Predicate<object> _filterCallback;
        private IComparer _comparer;

        internal IndexedEnumerable(IEnumerable collection)
            : this(collection, (Predicate<object>)null, (IComparer)null)
        {
        }

        public IndexedEnumerable(IEnumerable collection, Predicate<object> filterCallback, IComparer comparer)
        {
            _filterCallback = filterCallback;
            SetCollection(collection);
            if (collection is INotifyCollectionChanged collectionChanged)
                collectionChanged.CollectionChanged += CollectionChangedOnCollectionChanged;
            Comparer = comparer;
        }

        private void CollectionChangedOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_comparer == null) return;

            InvalidateEnumerator();
        }

        internal int IndexOf(object item)
        {
            int num1;
            if (GetNativeIndexOf(item, out num1))
                return num1;
            if (EnsureCacheCurrent() && item == _cachedItem)
                return _cachedIndex;
            var index = -1;
            if (_cachedIndex >= 0)
                UseNewEnumerator();
            var num2 = 0;
            while (_enumerator.MoveNext())
            {
                if (Equals(_enumerator.Current, item))
                {
                    index = num2;
                    break;
                }

                ++num2;
            }

            if (index >= 0)
            {
                CacheCurrentItem(index, _enumerator.Current);
            }
            else
            {
                ClearAllCaches();
                DisposeEnumerator(ref _enumerator);
            }

            return index;
        }

        public int Count
        {
            get
            {
                if (_collection == null) return 0;

                EnsureCacheCurrent();
                var count1 = 0;
                if (GetNativeCount(out count1))
                    return count1;
                if (_cachedCount >= 0)
                    return _cachedCount;
                var count2 = 0;
                foreach (var obj in this)
                    ++count2;
                _cachedCount = count2;
                _cachedIsEmpty = new bool?(_cachedCount == 0);
                return count2;
            }
        }

        public bool IsEmpty
        {
            get
            {
                bool isEmpty;
                if (GetNativeIsEmpty(out isEmpty))
                    return isEmpty;
                if (_cachedIsEmpty.HasValue)
                    return _cachedIsEmpty.Value;
                var enumerator = GetEnumerator();
                _cachedIsEmpty = new bool?(!enumerator.MoveNext());
                if (enumerator is IDisposable disposable)
                    disposable.Dispose();
                if (_cachedIsEmpty.Value)
                    _cachedCount = 0;
                return _cachedIsEmpty.Value;
            }
        }

        public object this[int index]
        {
            get
            {
                object obj;
                if (GetNativeItemAt(index, out obj))
                    return obj;
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                var num = index - _cachedIndex;
                if (num < 0)
                {
                    UseNewEnumerator();
                    num = index + 1;
                }

                if (EnsureCacheCurrent())
                {
                    if (index == _cachedIndex)
                        return _cachedItem;
                }
                else
                {
                    num = index + 1;
                }

                while (num > 0 && _enumerator.MoveNext())
                    --num;
                if (num != 0)
                    throw new ArgumentOutOfRangeException($"{nameof(index)}:{index}");
                CacheCurrentItem(index, _enumerator.Current);
                return _cachedItem;
            }
        }

        internal IEnumerable Enumerable => _enumerable;

        public ICollection Collection => _collection;

        public IEnumerator GetEnumerator()
        {
            return (IEnumerator)new FilteredEnumerator(this, Enumerable, FilterCallback);
        }

        public object GetItem(object source, bool previous)
        {
            if (source == null)
            {
                if (Count == 0)
                {
                    return null;
                }

                return this[0];
            }
            
            var index = IndexOf(source);
            if (previous)
            {
                index--;
            }
            else
            {
                index++;
            }

            if (index < 0)
            {
                index = Count - 1;
            }
            else if (index >= Count)
            {
                index = 0;
            }

            return this[index];
        }

        internal static void CopyTo(IEnumerable collection, Array array, int index)
        {
            if (collection is ICollection collection1)
            {
                collection1.CopyTo(array, index);
            }
            else
            {
                var list = (IList)array;
                foreach (var obj in collection)
                {
                    if (index >= array.Length)
                        throw new ArgumentException("CopyToNotEnoughSpace", nameof(index));
                    list[index] = obj;
                    ++index;
                }
            }
        }

        public void Invalidate()
        {
            ClearAllCaches();
            _enumerable = (IEnumerable)null;
            DisposeEnumerator(ref _enumerator);
            DisposeEnumerator(ref _changeTracker);
            
            if (_collection != null && _collection is INotifyCollectionChanged collectionChanged)
                collectionChanged.CollectionChanged -= CollectionChangedOnCollectionChanged;
            
            _collection = (ICollection)null;
            _list = (IList)null;
            _filterCallback = (Predicate<object>)null;
        }

        public IComparer Comparer
        {
            get => _comparer;
            set
            {
                if (Equals(_comparer, value)) return;

                _comparer = value;
                if (_comparer != null) _list = null;

                if (_comparer != null)
                {
                    InvalidateEnumerator();
                }
                else
                {
                    _enumerable = _collection;
                    _sortCollection?.Clear();
                    InvalidateEnumerator();
                }
            }
        }

        private void PrepareLocalArray()
        {
            if (_collection == null)
            {
                _sortCollection?.Clear();
                return;
            }

            if (_sortCollection == null)
                _sortCollection = new ArrayList();
            else
                _sortCollection.Clear();

            foreach (var item in _collection) _sortCollection.Add(item);
            _sortCollection.Sort(_comparer);
            _enumerable = _sortCollection;
        }

        public Predicate<object> FilterCallback
        {
            get => _filterCallback;
            set
            {
                if (_filterCallback == value) return;

                _filterCallback = value;
                if (_filterCallback != null) _list = null;
                InvalidateEnumerator();
            }
        }

        private void CacheCurrentItem(int index, object item)
        {
            _cachedIndex = index;
            _cachedItem = item;
            _cachedVersion = _enumeratorVersion;
        }

        private bool EnsureCacheCurrent()
        {
            var num = EnsureEnumerator();
            if (num != _cachedVersion)
            {
                ClearAllCaches();
                _cachedVersion = num;
            }

            return num == _cachedVersion && _cachedIndex >= 0;
        }

        private int EnsureEnumerator()
        {
            if (_enumerator == null)
                UseNewEnumerator();
            else
                try
                {
                    _changeTracker.MoveNext();
                }
                catch (InvalidOperationException)
                {
                    UseNewEnumerator();
                }

            return _enumeratorVersion;
        }

        private void UseNewEnumerator()
        {
            ++_enumeratorVersion;
            DisposeEnumerator(ref _changeTracker);
            _changeTracker = _enumerable.GetEnumerator();
            DisposeEnumerator(ref _enumerator);
            _enumerator = GetEnumerator();
            _cachedIndex = -1;
            _cachedItem = (object)null;
        }

        public void InvalidateEnumerator()
        {
            if (_comparer != null)
            {
                PrepareLocalArray();
            }
            
            ++_enumeratorVersion;
            DisposeEnumerator(ref _enumerator);
            ClearAllCaches();
        }

        private void DisposeEnumerator(ref IEnumerator ie)
        {
            if (ie == null) return;

            if (ie is IDisposable disposable)
                disposable.Dispose();
            ie = (IEnumerator)null;
        }

        private void ClearAllCaches()
        {
            _cachedItem = (object)null;
            _cachedIndex = -1;
            _cachedCount = -1;
        }

        private void SetCollection(IEnumerable collection)
        {
            _enumerable = collection;
            _collection = collection as ICollection;

            if (_filterCallback == null) _list = collection as IList;
        }

        private bool GetNativeCount(out int value)
        {
            var nativeCount = false;
            value = -1;
            if (Collection != null && FilterCallback == null)
            {
                value = Collection.Count;
                nativeCount = true;
            }

            return nativeCount;
        }

        private bool GetNativeIsEmpty(out bool isEmpty)
        {
            var nativeIsEmpty = false;
            isEmpty = true;
            if (Collection != null && FilterCallback == null)
            {
                isEmpty = Collection.Count == 0;
                nativeIsEmpty = true;
            }

            return nativeIsEmpty;
        }

        private bool GetNativeIndexOf(object item, out int value)
        {
            var nativeIndexOf = false;
            value = -1;
            if (_list != null && FilterCallback == null)
            {
                value = _list.IndexOf(item);
                nativeIndexOf = true;
            }

            return nativeIndexOf;
        }

        private bool GetNativeItemAt(int index, out object value)
        {
            var nativeItemAt = false;
            value = (object)null;
            if (_list != null && FilterCallback == null)
            {
                value = _list[index];
                nativeItemAt = true;
            }

            return nativeItemAt;
        }

        private class FilteredEnumerator : IEnumerator, IDisposable
        {
            private IEnumerable _enumerable;
            private IEnumerator _enumerator;
            private IndexedEnumerable _indexedEnumerable;
            private Predicate<object> _filterCallback;

            internal bool ignoreFilter;

            public FilteredEnumerator(
                IndexedEnumerable indexedEnumerable,
                IEnumerable enumerable,
                Predicate<object> filterCallback)
            {
                _enumerable = enumerable;
                _enumerator = _enumerable.GetEnumerator();
                _filterCallback = filterCallback;
                _indexedEnumerable = indexedEnumerable;
            }

            void IEnumerator.Reset()
            {
                if (_indexedEnumerable._enumerable == null)
                    throw new InvalidOperationException("EnumeratorVersionChanged");
                Dispose();
                _enumerator = _enumerable.GetEnumerator();
            }

            bool IEnumerator.MoveNext()
            {
                if (_indexedEnumerable._enumerable == null)
                    throw new InvalidOperationException("EnumeratorVersionChanged");
                bool flag;
                if (_filterCallback == null || ignoreFilter)
                    flag = _enumerator.MoveNext();
                else
                    while ((flag = _enumerator.MoveNext()) && !_filterCallback(_enumerator.Current))
                        ;
                return flag;
            }

            object IEnumerator.Current => _enumerator.Current;

            public void Dispose()
            {
                if (_enumerator is IDisposable enumerator)
                    enumerator.Dispose();
                _enumerator = (IEnumerator)null;
            }
        }
    }
}