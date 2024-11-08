using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;

namespace DH.Game.ViewModels
{
    public partial class ShopBoxRewardViewModel : ViewModelBase
    {
	    public override bool AutoDispose => true;
	    [AutoNotify] private int selectLv;
	    [AutoNotify] private ObservableList<ShopBoxRewardItemViewModel> scrollViewList = new();

        [Preserve]
        public ShopBoxRewardViewModel()
        {
	        SelectLv = DataCenter.shopData.GetBoxInfoLv();
	        InitShopBoxItem();
        }
        
        private void InitShopBoxItem()
        {
	        var chestList = ShopManager.Instance.GetEquipChestList();
	        foreach (var item in chestList)
	        {
		        ScrollViewList.Add(new ShopBoxRewardItemViewModel(item.Id,SelectLv));
	        }
        }

        private void RefreshList()
        {
	        foreach (var item in scrollViewList)
	        {
		        item.SelectLv = SelectLv;
	        }
        }


        [Command]
        private void OnClickBtnClose()
        {
	        UIManager.Instance.CloseDialog<ShopBoxRewardView>();
        }

		[Command]
		private void OnClickBtnDel()
		{
			if (SelectLv > 1)
			{
				SelectLv -= 1;
				RefreshList();
			}
		}

		[Command]
		private void OnClickBtnAdd()
		{
			if (!ShopManager.Instance.IsEquipChestMax(SelectLv))
			{
				SelectLv += 1;
				RefreshList();
			}
		}

        
    }
}