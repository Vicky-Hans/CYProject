using System.Collections.Specialized;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using INotifyCollectionChanged = DH.UIFramework.Observables.INotifyCollectionChanged;

namespace DH.UIFramework
{
    internal class TabGroupViewModel : ViewModelBase
    {
        public int SelectIndex
        {
            get => selectIndex;
            set
            {
                var lastSelected = selectIndex;
                Set(ref selectIndex, value);

                if (lastSelected >= 0 && lastSelected < SubTabList.Count)
                {
                    SubTabList[lastSelected].Selected = false;
                }
            }
        }
        
        public ObservableList<SubTabViewModelBase> SubTabList 
        {
            get { return this.subTabList; }
            set
            {
                if (this.subTabList == value)
                    return;

                var collection = this.subTabList as INotifyCollectionChanged;
                if (collection != null)
                    collection.CollectionChanged -= OnCollectionChanged;
          
                this.subTabList = value;
                this.OnItemsChanged();

                collection = this.subTabList as INotifyCollectionChanged;
                if (collection != null)
                    collection.CollectionChanged += OnCollectionChanged;
            }
        }

        private ObservableList<SubTabViewModelBase> subTabList;
        private int selectIndex = -1;

        public TabGroupViewModel()
        {
            SubTabList = new ObservableList<SubTabViewModelBase>();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            OnItemsChanged();
        }
        
        private void OnItemsChanged()
        {
            if (subTabList == null)
            {
                return;
            }

            var count = subTabList.Count;
            for (int i = 0; i < count; ++i)
            {
                var subTab = subTabList[i];
                subTab.ItemIndex = i;
                subTab.SetSelectedCallback(OnItemSelected);

                if (subTab.Selected)
                {
                    SelectIndex = i;
                }
            }
        }

        private void OnItemSelected(int idx)
        {
            SelectIndex = idx;
        }
    }
}