using System;
using System.Collections;
using System.Collections.Generic;
using INotifyCollectionChanged = DH.UIFramework.Observables.INotifyCollectionChanged;
using System.Collections.Specialized;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.UIFramework.Observables;
using UnityEngine;

namespace DH.UIFramework
{
    public partial class UICircularScrollView : ICollectionView
    {
        public Predicate<object> CellItemShowPredicate { set; get; }
        public Func<object, object> BindDictionaryGetValueFunc;

        private IndexedEnumerable indexedEnumerable;
        public event Action OnAssetLoaded;

        protected GameObject prefab;
        private string prefabPath;
        private Comparison<IComparable> sortFunc;
        protected Predicate<object> filter;
        protected IComparer comparer;

        Predicate<object> ICollectionView.Filter
        {
            set
            {
                if (Equals(filter, value)) return;

                filter = value;
                if (filter == null) return;
                
                OnSetFilter(value);
            }
        }

        IComparer ICollectionView.Comparer
        {
            set
            {
                if (Equals(comparer, value)) return;
                comparer = value;
                if (comparer == null) return;

                OnSetComparer(value);
            }
        }

        public object GetItem(object source, bool previous)
        {
            return indexedEnumerable?.GetItem(source, previous) ?? null;
        }
        
        protected virtual void OnSetFilter(Predicate<object> target)
        {
            if (indexedEnumerable == null) return;

            indexedEnumerable.FilterCallback = target;
        }

        protected virtual void OnSetComparer(IComparer target)
        {
            if (indexedEnumerable == null) return;

            indexedEnumerable.Comparer = target;
        }

        protected virtual ICollection GetCollection()
        {
            return indexedEnumerable.Collection;
        }

        protected virtual void OnSetCollection(ICollection targetValue)
        {
            if (indexedEnumerable != null && Equals(indexedEnumerable.Collection, targetValue))
                return;

            var collection = indexedEnumerable?.Collection as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged -= OnCollectionChanged;

            if (indexedEnumerable != null)
            {
                indexedEnumerable.Invalidate();
            }

            indexedEnumerable = new IndexedEnumerable(targetValue, filter, comparer);
            OnItemsChanged();

            collection = indexedEnumerable.Collection as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged += OnCollectionChanged;
        }

        public ICollection Collection
        {
            get => GetCollection();
            set => OnSetCollection(value);
        }

        public string PrefabPath
        {
            get => prefabPath;
            set
            {
                if (prefabPath == value) return;

                prefabPath = value;
                if (string.IsNullOrEmpty(prefabPath) && prefab != null)
                {
                    AssetsManager.Release(prefab);
                    prefab = null;
                }
                else
                {
                    LoadPrefab().Forget();
                }
            }
        }

        public virtual void Refresh()
        {
            if (InValidPrefab()) return;

            if (indexedEnumerable == null) return;
            indexedEnumerable.InvalidateEnumerator();

            ShowList(indexedEnumerable.Count);
            m_Dirty = !CheckItemIsOutRange();
        }

        protected async UniTaskVoid LoadPrefab()
        {
            if (m_loadAsync)
            {
                prefab = await AssetsManager.LoadAssetAsync<GameObject>(prefabPath);
            }
            else
            {
                prefab = AssetsManager.LoadAssetSync<GameObject>(prefabPath);
            }
            
            OnPrefabsPrepared();

            if (m_PreloadItemCount > 0)
            {
                var objList = new List<GameObject>();
                for (int i = 0; i < m_PreloadItemCount; ++i)
                {
                    objList.Add(GetPoolsObj(prefab, m_Content.transform));
                }

                foreach (var obj in objList)
                {
                    SetPoolsObj(obj);
                }
            }
            
            OnAssetLoaded?.Invoke();
        }

        public async UniTask PreLoadDynamicPrefab(string path)
        {
            Init();
            
            if (!m_DynamicPrefabDic.ContainsKey(path))
            {
                m_DynamicPrefabDic.Add(path, null);
                await LoadDynamicPrefab(path);
            }
        }

        protected async UniTask LoadDynamicPrefab(string path)
        {
            if (m_loadAsync)
            {
                prefab = await AssetsManager.LoadAssetAsync<GameObject>(path);
            }
            else
            {
                prefab = AssetsManager.LoadAssetSync<GameObject>(path);
            }
            
            m_DynamicPrefabDic[path] = prefab;
            InitPrefab(prefab);
        }

        protected void OnItemRemoved(GameObject cell)
        {
            var context = cell.GetComponent<BaseView>();
            if(m_CellSizeType == CellItemSizeType.DirectionAdaptingSize)
            {
                var adapting = cell.GetComponent<CircularScrollItemAdapting>();

                if (adapting)
                {
                    adapting.Init(-1, m_Direction == e_Direction.Horizontal ? 0 : 1, null);
                }
            }

            if (context)
            {
                context.SetDataContext(null);
                if (context is ICircularScrollItem circularItem) circularItem.OnItemHideCallback();
            }
        }

        protected void OnItemUpdated(GameObject cell, int index)
        {
            if (indexedEnumerable == null) return;
            var data = indexedEnumerable[index];

            var context = cell.GetComponent<BaseView>();
            var vmData = GetValue(indexedEnumerable[index]);
            context.SetDataContext(vmData);

            if(m_CellSizeType == CellItemSizeType.DirectionAdaptingSize)
            {
                var adapting = cell.GetOrAddComponent<CircularScrollItemAdapting>();
                adapting.Init(index, m_Direction == e_Direction.Horizontal ? 0 : 1, OneCellSizeChanged);
            }

            if (context is ICircularScrollItem circularItem) circularItem.OnItemShowCallback(index, vmData);
        }

        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            indexedEnumerable?.InvalidateEnumerator();
            dataDirty = true;
        }

        private object GetValue(object obj)
        {
            if (BindDictionaryGetValueFunc != null) return BindDictionaryGetValueFunc.Invoke(obj);
            return obj;
        }

        protected virtual void OnItemsChanged()
        {
            if (indexedEnumerable == null) return;

            if (InValidPrefab()) return;

            ShowList(indexedEnumerable.Count, true);
        }

        protected virtual void OnPrefabsPrepared()
        {
            Init();
            InitPrefab(prefab);
            OnItemsChanged();
        }

        protected virtual bool InValidPrefab()
        {
            return !prefab && GetPrefabPathFunction == null;
        }

        private float GetItemAdaptingSize(GameObject go, int idx)
        {
            if (!NeedShowCellItem(idx))
            {
                return 1;
            }
            
            if (m_CellSizeType == CellItemSizeType.DirectionAdaptingSize)
            {
                float size = 0;

                if (go)
                {
                    var adapting = go.GetComponent<CircularScrollItemAdapting>();

                    if (adapting)
                    {
                        return adapting.GetItemSize();
                    }
                }

                return m_Direction == e_Direction.Horizontal ? m_CellObjectWidth : m_CellObjectHeight;
            }

            return -1;
        }
    }
}