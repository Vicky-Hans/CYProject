using System.Collections;
using System.Collections.Specialized;
using DH.UIFramework.Observables;
using INotifyCollectionChanged = DH.UIFramework.Observables.INotifyCollectionChanged;

namespace DH.UIFramework
{
    public partial class TabGroupView
    {
        public IList Items
        {
            get => tabGroupVM.SubTabList;
            set => tabGroupVM.SubTabList = (ObservableList<SubTabViewModelBase>)value;
        }

        public IDictionary ItemsMap
        {
            get => itemsMap;
            set
            {
                if (itemsMap == value)
                    return;

                var collection = itemsMap as INotifyCollectionChanged;
                if (collection != null)
                    collection.CollectionChanged -= OnDictionaryCollectionChanged;

                itemsMap = value;
                OnItemsMapChanged();

                collection = itemsMap as INotifyCollectionChanged;
                if (collection != null)
                    collection.CollectionChanged += OnDictionaryCollectionChanged;
            }
        }

        private IDictionary itemsMap;

        /// <summary>
        /// 使用scrollView 动态生成tab列表
        /// </summary>
        public ScrollRectExtend tabScrollView;

        public void Init(string itemPrefabPath, SelectChangeCallback changeCallback)
        {
            SetAnimCallback(changeCallback);

            tabScrollView.PrefabPath = itemPrefabPath;
            tabGroupVM = new TabGroupViewModel();

            var bindingSet = this.CreateBindingSet<TabGroupView, TabGroupViewModel>();
            bindingSet.Bind(tabScrollView).For(v => v.Collection).To(vm => vm.SubTabList);
            bindingSet.Bind(this).For(v => v.CurIndex).To(vm => vm.SelectIndex);
            bindingSet.Build();

            this.SetDataContext(tabGroupVM);
        }

        private void OnItemsMapChanged()
        {
            if (itemsMap == null) return;

            Items.Clear();

            foreach (var item in itemsMap.Values) Items.Add(item);
        }

        private void OnDictionaryCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            var newItems = (eventArgs.NewItems as IDictionary)?.Values;
            var oldItems = (eventArgs.OldItems as IDictionary)?.Values;

            switch (eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (newItems != null)
                        foreach (var item in newItems)
                            Items.Add(item);

                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (oldItems != null)
                        foreach (var item in oldItems)
                            Items.Remove(item);

                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (oldItems != null)
                        foreach (var item in oldItems)
                            Items.Remove(item);

                    if (newItems != null)
                        foreach (var item in newItems)
                            Items.Add(item);

                    break;
                case NotifyCollectionChangedAction.Reset:
                    Items.Clear();
                    break;
            }
        }
    }
}