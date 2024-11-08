using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;



namespace DH.Game.ViewModels
{
    public partial class ShopDiamondViewModel : ViewModelBase
    {
        
        [AutoNotify] private ObservableList<ShopDiamondItemViewModel> scrollViewGemList = new();
        [AutoNotify] private string titleName;
        [Preserve]
        public ShopDiamondViewModel()
        {
            InitDiamondList();
            TitleName = ShopManager.Instance.GetTitleName(4);
        }
        
        protected override void OnDispose()
        {
            base.OnDispose();
            UIHelper.ViewModelBaseOnDisposes(scrollViewGemList);
        }

        
        private void InitDiamondList()
        {
            scrollViewGemList.ClearAndDispose();
            var list = ShopManager.Instance.GetDiamondShopList();
            for (int i = 0; i < list.Count; i++)
            {
                scrollViewGemList.Add(new ShopDiamondItemViewModel(list[i]));
            }
        }

        
    }
}