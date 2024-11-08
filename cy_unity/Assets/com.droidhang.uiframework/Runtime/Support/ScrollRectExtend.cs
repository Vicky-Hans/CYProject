using System;
using System.Collections;
using System.Collections.Specialized;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.UIFramework.Observables;
using DHFramework;
using UnityEngine;
using UnityEngine.UI;
using INotifyCollectionChanged = DH.UIFramework.Observables.INotifyCollectionChanged;

namespace DH.UIFramework
{
    /// <summary>
    /// 普通列表组件
    /// 复用UGUI原有ScrollRect功能，增加对数据绑定的支持
    /// </summary>
    public class ScrollRectExtend : ScrollRect, ICollectionView
    {
        public Func<object, object> BindDictionaryGetValueFunc;
        public event Action OnAssetLoaded;

        private ScrollRect scrollRect;
        private IndexedEnumerable indexedEnumerable;
        private string prefabPath;
        private GameObject prefab;
        private Predicate<object> filter;
        private IComparer comparer;
        private bool dirty;

        public bool AssetReady => prefab;

        Predicate<object> ICollectionView.Filter
        {
            set
            {
                if (Equals(filter, value)) return;

                filter = value;
                if (filter == null) return;

                if (indexedEnumerable == null) return;

                indexedEnumerable.FilterCallback = filter;
            }
        }

        public Predicate<object> CellItemShowPredicate { get; set; }

        IComparer ICollectionView.Comparer
        {
            set
            {
                if (Equals(comparer, value)) return;
                comparer = value;
                if (comparer == null) return;
                if (indexedEnumerable == null) return;

                indexedEnumerable.Comparer = comparer;
            }
        }

        public object GetItem(object source, bool previous)
        {
            return indexedEnumerable?.GetItem(source, previous) ?? null;
        }
        
        public void Refresh()
        {
            if (prefab == null) return;

            if (indexedEnumerable == null) return;

            indexedEnumerable.InvalidateEnumerator();
            dirty = true;
        }

        public ICollection Collection
        {
            get => indexedEnumerable.Collection;
            set
            {
                if (indexedEnumerable != null && Equals(indexedEnumerable.Collection, value))
                    return;

                var collection = indexedEnumerable?.Collection as INotifyCollectionChanged;
                if (collection != null)
                    collection.CollectionChanged -= OnCollectionChanged;

                bool dirtyView = indexedEnumerable != null;
                if (indexedEnumerable != null)
                {
                    indexedEnumerable.Invalidate();
                }

                indexedEnumerable = new IndexedEnumerable(value, filter, comparer);

                if (dirtyView)
                {
                    dirty = true;
                }
                else
                {
                    OnItemsChanged();
                }

                collection = indexedEnumerable.Collection as INotifyCollectionChanged;
                if (collection != null)
                    collection.CollectionChanged += OnCollectionChanged;
            }
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

        protected async UniTaskVoid LoadPrefab()
        {
            prefab = await AssetsManager.LoadAssetAsync<GameObject>(prefabPath);
            OnItemsChanged();
            OnAssetLoaded?.Invoke();
        }

        protected void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            indexedEnumerable?.InvalidateEnumerator();
            dirty = true;
        }

        protected virtual void OnItemsChanged()
        {
            if (indexedEnumerable == null) return;

            if (!prefab) return;

            RefreshView();
        }

        protected override void LateUpdate()
        {
            if (dirty && prefab)
            {
                dirty = false;
                RefreshView();
            }

            base.LateUpdate();
        }

        private void RefreshView()
        {
            // var childCount = content.childCount;
            // var range = indexedEnumerable.Count;
            // for (var index = 0; index < range; index++)
            // {
            //     GameObject item = null;
            //     if (index >= childCount)
            //         item = Instantiate(prefab, content, false);
            //     else
            //         item = content.GetChild(index).gameObject;
            //     var context = item.GetComponent<BaseView>();
            //     context.SetDataContext(GetValue(indexedEnumerable[index]));
            //     if (index != -1) item.transform.SetSiblingIndex(index);
            // }
            // for (var index = range; index < childCount; index++) Destroy(content.GetChild(index).gameObject);
            var childCount = content.childCount;
            var range = indexedEnumerable.Count;
            if (childCount > range)
            {
                for (var i = childCount-1; i >= range; i--)
                {
                    DestroyImmediate(content.GetChild(i).gameObject);
                }
            }
            childCount = content.childCount;
            for (var i = 0; i < range; i++)
            {
                var item = i >= childCount ? Instantiate(prefab, content, false) : content.GetChild(i).gameObject;
                var context = item.GetComponent<BaseView>();
                context.SetDataContext(GetValue(indexedEnumerable[i]));
                item.transform.SetSiblingIndex(i);
            }
        }

        private object GetValue(object obj)
        {
            if (BindDictionaryGetValueFunc != null) return BindDictionaryGetValueFunc.Invoke(obj);
            return obj;
        }

        protected override void OnDestroy()
        {
            indexedEnumerable?.Invalidate();
            indexedEnumerable = null;
            PrefabPath = null;

            base.OnDestroy();
        }
    }
}