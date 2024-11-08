using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class ShopGoldViewModel : ViewModelBase
    {
        [AutoNotify] private ObservableList<ShopGoldItemViewModel> scrollViewCoinList = new();
        [AutoNotify] private string titleName;

        [Preserve]
        public ShopGoldViewModel()
        {
            TitleName = ShopManager.Instance.GetTitleName(5);
            InitGoldList();
        }
        
        protected override void OnDispose()
        {
            base.OnDispose();
            UIHelper.ViewModelBaseOnDisposes(scrollViewCoinList);
        }
        private void InitGoldList()
        {
            scrollViewCoinList.ClearAndDispose();
            var list = ShopManager.Instance.GetGoldShopList();
            foreach (var t in list)
            {
                scrollViewCoinList.Add(new ShopGoldItemViewModel(t));
            }
        }
        
    }
}