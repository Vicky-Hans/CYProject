using System;
using System.Collections;
using System.Collections.Specialized;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.UIFramework.Observables;
using UnityEngine;
using INotifyCollectionChanged = DH.UIFramework.Observables.INotifyCollectionChanged;

namespace DH.UIFramework
{
    public partial class GroupExpandCircularScrollView : IGroupedCollectionView
    {
        public GameObject expandButtonPrefab;
        private string expandButtonPrefabPath;
        private GroupedEnumerable groupedEnumerable;
        private Func<object, object> keySelector;
        private IComparer keyComparer; //对分组的key进行排序
        
        public string ExpandButtonPrefabPath
        {
            get => expandButtonPrefabPath;
            set
            {
                if (expandButtonPrefabPath == value) return;

                expandButtonPrefabPath = value;
                if (string.IsNullOrEmpty(expandButtonPrefabPath) && expandButtonPrefab != null)
                {
                    AssetsManager.Release(expandButtonPrefab);
                    expandButtonPrefab = null;
                }
                else
                {
                    LoadExpandButtonPrefab().Forget();
                }
            }
        }
        
        public Func<object, object> KeySelector
        {
            get => keySelector;
            set
            {
                if (keySelector == value) return;

                keySelector = value;
                if (groupedEnumerable == null) return;
                groupedEnumerable.KeySelector = value;
            }
        }
        
        public IComparer KeyComparer
        {
            get => keyComparer;
            set
            {
                if (Equals(keyComparer, value)) return;

                keyComparer = value;
                
                if (groupedEnumerable == null) return;
                groupedEnumerable.KeyComparer = value;
            }
        }

        public override void Refresh()
        {
            if (InValidPrefab()) return;

            if (groupedEnumerable == null) return;
            groupedEnumerable.InvalidateEnumerator();
            
            ShowList(groupedEnumerable.GetGroupCountList());
        }

        protected override void OnItemsChanged()
        {
            if (groupedEnumerable == null) return;

            if (InValidPrefab()) return;

            ShowList(groupedEnumerable.GetGroupCountList());
        }

        protected override ICollection GetCollection()
        {
            return groupedEnumerable.List;
        }
        
        protected override void OnSetCollection(ICollection targetValue)
        {
            if (groupedEnumerable != null && Equals(groupedEnumerable.Collection, targetValue))
                return;

            var collection = groupedEnumerable?.Collection as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged -= OnCollectionChanged;

            if (groupedEnumerable != null)
            {
                groupedEnumerable.Invalidate();
            }

            groupedEnumerable = new GroupedEnumerable(targetValue, KeySelector, KeyComparer);
            groupedEnumerable.Filter = filter;
            groupedEnumerable.Comparer = comparer;
            OnItemsChanged();

            collection = groupedEnumerable.Collection as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged += OnCollectionChanged;
        }

        protected override void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            dataDirty = true;
        }

        protected override void OnSetFilter(Predicate<object> target)
        {
            if (groupedEnumerable == null) return;

            groupedEnumerable.Filter = target;
        }

        protected override void OnSetComparer(IComparer target)
        {
            if (groupedEnumerable == null) return;

            groupedEnumerable.Comparer = target;
        }

        private async UniTaskVoid LoadExpandButtonPrefab()
        {
            expandButtonPrefab = await AssetsManager.LoadAssetAsync<GameObject>(expandButtonPrefabPath);
            OnPrefabsPrepared();
        }

        protected override void OnPrefabsPrepared()
        {
            if(InValidPrefab())
            {
                return;
            }

            Init();
            InitExpandButtonPrefab();
            InitPrefab(prefab);
            OnItemsChanged();
        }

        protected override bool InValidPrefab()
        {
            return !prefab || !expandButtonPrefab;
        }
        
        protected void OnBindGroupBtn(GameObject cell, int groupIdx)
        {
            if (!cell || groupedEnumerable == null) return;
            var data = groupedEnumerable[groupIdx];

            var context = cell.GetComponent<BaseView>();
            
            if (context)
            {
                context.SetDataContext(data.Key);
            }
        }
        
        protected void OnBindGroupCell(GameObject cell, int groupIdx, int index)
        {
            if (!cell || groupedEnumerable == null) return;
            var data = groupedEnumerable[groupIdx];

            var context = cell.GetComponent<BaseView>();
            context.SetDataContext(data[index]);
        }
    }
}

