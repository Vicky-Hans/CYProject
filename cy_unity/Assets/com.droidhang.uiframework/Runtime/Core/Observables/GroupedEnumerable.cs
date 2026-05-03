using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace DH.UIFramework.Observables
{
    /// <summary>
    /// 对collection进行分组，KeySelector必须赋值，KeyComparer按需赋值
    /// </summary>
    public class GroupedEnumerable : IEnumerable
    {
        #region 分组的Key相关的属性

        public Func<object, object> KeySelector
        {
            get => keySelector;
            set
            {
                if (keySelector == value) return;

                keySelector = value;
                ClearGroupList();
            }
        }
        
        public IComparer KeyComparer
        {
            get => keyComparer;
            set
            {
                if (Equals(keyComparer, value)) return;

                keyComparer = value;

                Sort();
            }
        }
        
        internal IList List => groupCollection;

        #endregion

        #region 每个组里的列表相关的属性

        public IComparer Comparer
        {
            set
            {
                comparer = value;
                EnsurePrepareGroupList();
                
                foreach (var group in groupCollection)
                {
                    group.Comparer = value;
                }
            }
        }
        
        public Predicate<object> Filter
        {
            set
            {
                filter = value;
                EnsurePrepareGroupList();
                
                foreach (var group in groupCollection)
                {
                    group.FilterCallback = value;
                }
            }
        }

        #endregion
        
        internal IEnumerable Collection => collection;
        
        private IEnumerable collection;
        private Func<object, object> keySelector;
        private IComparer keyComparer; //对分组的key进行排序
        private IComparer comparer;
        private Predicate<object> filter;

        private IEqualityComparer keyEquality;
        private List<IndexedKeyEnumerable> groupCollection;
        private bool invalid = true;
        
        public GroupedEnumerable(IEnumerable collection, Func<object, object> keySelector, IComparer keyComparer = null)
        {
            this.collection = collection;
            this.keySelector = keySelector;
            this.keyComparer = keyComparer;
            keyEquality = EqualityComparer<object>.Default;
            groupCollection = new List<IndexedKeyEnumerable>();
            
            if (collection is INotifyCollectionChanged collectionChanged)
                collectionChanged.CollectionChanged += CollectionChangedOnCollectionChanged;
            
            PrepareGroupList();
        }

        internal IndexedKeyEnumerable this[int index]
        {
            get
            {
                EnsurePrepareGroupList();
                
                if (index < 0 || index >= groupCollection.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return groupCollection[index];
            }
        }
        
        public IEnumerator GetEnumerator()
        {
            EnsurePrepareGroupList();
            
            return groupCollection.GetEnumerator();
        }

        internal List<int> GetGroupCountList()
        {
            EnsurePrepareGroupList();
            
            var countList = new List<int>();

            foreach (var group in groupCollection)
            {
                countList.Add(group.Count);
            }

            return countList;
        }

        internal void InvalidateEnumerator()
        {
            invalid = true;
            groupCollection.Clear();
            foreach (var group in groupCollection)
            {
                group.InvalidateEnumerator();
            }
        }
        
        internal void Invalidate()
        {
            ClearGroupList();
            
            if (collection != null && collection is INotifyCollectionChanged collectionChanged)
                collectionChanged.CollectionChanged -= CollectionChangedOnCollectionChanged;
            
            collection = (ICollection)null;
            keySelector = null;
            keyComparer = null;
        }

        private void EnsurePrepareGroupList()
        {
            if (invalid)
            {
                PrepareGroupList();
            }
        }

        private void PrepareGroupList()
        {
            if (keySelector == null || collection == null)
            {
                return;
            }

            foreach (var item in collection)
            {
                GetGrouping(keySelector(item), true).Add(item);
            }

            invalid = false;
            
            Sort();
        }

        private int InternalGetHashCode(object key)
        {
            return (key == null) ? 0 : keyEquality.GetHashCode(key) & 0x7FFFFFFF;
        }

        private IndexedKeyEnumerable GetGrouping(object key, bool create) {
            var hashCode = InternalGetHashCode(key);
            var group = groupCollection.Find(group => group.HashCode == hashCode && group.Key.Equals(key));

            if (group == null && create)
            {
                group = new IndexedKeyEnumerable(key, hashCode, new ObservableList<object>());
                group.FilterCallback = filter;
                group.Comparer = comparer;
                groupCollection.Add(group);
            }
            
            return group;
        }

        private void Sort()
        {
            if (invalid)
            {
                return;
            }
            
            if (keyComparer != null && groupCollection != null)
            {
                groupCollection.Sort(CompareKeys);
            }
        }
        
        private int CompareKeys(IndexedKeyEnumerable x, IndexedKeyEnumerable y)
        {
            return keyComparer.Compare(x.Key, y.Key);
        }

        private void CollectionChangedOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (KeySelector == null) return;

            ClearGroupList();
        }
        
        private void ClearGroupList()
        {
            foreach (var group in groupCollection)
            {
                group.Invalidate();
            }
            
            groupCollection.Clear();
            invalid = true;
        }
    }
}
