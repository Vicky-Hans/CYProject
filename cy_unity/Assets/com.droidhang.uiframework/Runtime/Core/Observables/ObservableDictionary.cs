using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;
using NotifyCollectionChangedEventHandler = System.Collections.Specialized.NotifyCollectionChangedEventHandler;
using INotifyCollectionChanged = System.Collections.Specialized.INotifyCollectionChanged;
using NotifyCollectionChangedAction = System.Collections.Specialized.NotifyCollectionChangedAction;
using NotifyCollectionChangedEventArgs = System.Collections.Specialized.NotifyCollectionChangedEventArgs;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;
using PropertyChangedEventHandler = System.ComponentModel.PropertyChangedEventHandler;

namespace DH.UIFramework.Observables
{
    [Serializable]
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, INotifyCollectionChanged,
        INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs CountEventArgs = new("Count");
        private static readonly PropertyChangedEventArgs IndexerEventArgs = new("Item[]");
        private static readonly PropertyChangedEventArgs KeysEventArgs = new("Keys");
        private static readonly PropertyChangedEventArgs ValuesEventArgs = new("Values");

        private readonly object propertyChangedLock = new();
        private readonly object collectionChangedLock = new();
        private List<PropertyChangedEventHandler>   propertyChanged;
        private List<NotifyCollectionChangedEventHandler>   collectionChanged;

        protected Dictionary<TKey, TValue> dictionary;

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

        public ObservableDictionary()
        {
            dictionary = new Dictionary<TKey, TValue>();
        }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            this.dictionary = new Dictionary<TKey, TValue>(dictionary);
        }

        public ObservableDictionary(IEqualityComparer<TKey> comparer)
        {
            dictionary = new Dictionary<TKey, TValue>(comparer);
        }

        public ObservableDictionary(int capacity)
        {
            dictionary = new Dictionary<TKey, TValue>(capacity);
        }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            this.dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        public ObservableDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
        }

        public TValue this[TKey key]
        {
            get
            {
                if (!dictionary.ContainsKey(key))
                    return default;
                return dictionary[key];
            }
            set => Insert(key, value, false);
        }

        public ICollection<TKey> Keys => dictionary.Keys;

        public ICollection<TValue> Values => dictionary.Values;

        public void Add(TKey key, TValue value)
        {
            Insert(key, value, true);
        }

        public bool Remove(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            TValue value;
            dictionary.TryGetValue(key, out value);
            var removed = dictionary.Remove(key);
            if (removed)
            {
                OnPropertyChanged(NotifyCollectionChangedAction.Remove);
                if (collectionChanged != null)
                    OnCollectionChanged(NotifyCollectionChangedAction.Remove,
                        new KeyValuePair<TKey, TValue>(key, value));
            }

            return removed;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Insert(item.Key, item.Value, true);
        }

        public void Clear()
        {
            if (dictionary.Count > 0)
            {
                dictionary.Clear();
                OnPropertyChanged(NotifyCollectionChangedAction.Reset);
                if (collectionChanged != null)
                    OnCollectionChanged();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary)dictionary).CopyTo(array, arrayIndex);
        }

        public int Count => dictionary.Count;

        public bool IsReadOnly => ((IDictionary)dictionary).IsReadOnly;

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)dictionary).GetEnumerator();
        }

        public void AddRange(IDictionary<TKey, TValue> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            if (items.Count > 0)
            {
                if (dictionary.Count > 0)
                {
                    if (items.Keys.Any((k) => dictionary.ContainsKey(k)))
                        throw new ArgumentException("An item with the same key has already been added.");
                    else
                        foreach (var item in items)
                            ((IDictionary<TKey, TValue>)dictionary).Add(item);
                }
                else
                {
                    dictionary = new Dictionary<TKey, TValue>(items);
                }

                OnPropertyChanged(NotifyCollectionChangedAction.Add);
                if (collectionChanged != null)
                    OnCollectionChanged(NotifyCollectionChangedAction.Add, items.ToArray());
            }
        }

        private void Insert(TKey key, TValue value, bool add)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            TValue item;
            if (dictionary.TryGetValue(key, out item))
            {
                if (add)
                    throw new ArgumentException("An item with the same key has already been added.");

                if (Equals(item, value))
                    return;

                dictionary[key] = value;
                OnPropertyChanged(NotifyCollectionChangedAction.Replace);
                if (collectionChanged != null)
                    OnCollectionChanged(NotifyCollectionChangedAction.Replace,
                        new KeyValuePair<TKey, TValue>(key, value), new KeyValuePair<TKey, TValue>(key, item));
            }
            else
            {
                dictionary[key] = value;
                OnPropertyChanged(NotifyCollectionChangedAction.Add);
                if (collectionChanged != null)
                    OnCollectionChanged(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value));
            }
        }

        private void OnPropertyChanged(NotifyCollectionChangedAction action)
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                {
                    OnPropertyChanged(CountEventArgs);
                    OnPropertyChanged(IndexerEventArgs);
                    OnPropertyChanged(KeysEventArgs);
                    OnPropertyChanged(ValuesEventArgs);
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    OnPropertyChanged(IndexerEventArgs);
                    OnPropertyChanged(ValuesEventArgs);
                    break;
                }
                case NotifyCollectionChangedAction.Move:
                default:
                {
                    OnPropertyChanged(CountEventArgs);
                    OnPropertyChanged(IndexerEventArgs);
                    OnPropertyChanged(KeysEventArgs);
                    OnPropertyChanged(ValuesEventArgs);
                    break;
                }
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
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
                        item(this, eventArgs);
                    }
                }
                finally
                {
                    ListPool<PropertyChangedEventHandler>.Release(tempList);
                }
            }
        }

        private void OnCollectionChanged()
        {
            if (collectionChanged != null)
            {
                var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                OnCollectionChanged(eventArgs);
            }
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (collectionChanged != null)
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
        
        private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> changedItem)
        {
            if (collectionChanged != null)
            {
                var eventArgs = new NotifyCollectionChangedEventArgs(action, changedItem);
                OnCollectionChanged(eventArgs);
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> newItem,
            KeyValuePair<TKey, TValue> oldItem)
        {
            if (collectionChanged != null)
            {
                var eventArgs = new NotifyCollectionChangedEventArgs(action, newItem, oldItem);
                OnCollectionChanged(eventArgs);
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, IList newItems)
        {
            if (collectionChanged != null)
            {
                var eventArgs = new NotifyCollectionChangedEventArgs(action, newItems);
                OnCollectionChanged(eventArgs);
            }
        }

        object IDictionary.this[object key]
        {
            get => ((IDictionary)dictionary)[(TKey)key];
            set => Insert((TKey)key, (TValue)value, false);
        }

        ICollection IDictionary.Keys => ((IDictionary)dictionary).Keys;

        ICollection IDictionary.Values => ((IDictionary)dictionary).Values;

        bool IDictionary.Contains(object key)
        {
            return ((IDictionary)dictionary).Contains((TKey)key);
        }

        void IDictionary.Add(object key, object value)
        {
            Add((TKey)key, (TValue)value);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)dictionary).GetEnumerator();
        }

        void IDictionary.Remove(object key)
        {
            Remove((TKey)key);
        }

        bool IDictionary.IsFixedSize => ((IDictionary)dictionary).IsFixedSize;

        void ICollection.CopyTo(Array array, int index)
        {
            ((IDictionary)dictionary).CopyTo(array, index);
        }

        object ICollection.SyncRoot => ((IDictionary)dictionary).SyncRoot;

        bool ICollection.IsSynchronized => ((IDictionary)dictionary).IsSynchronized;
    }
}