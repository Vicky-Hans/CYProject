using System.ComponentModel;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
namespace DH.Game.ViewModels
{
    public partial class ShopBoxInfoViewModel : ViewModelBase
    {
	    [AutoNotify] private EquipChestCfg cfg;
	    [AutoNotify] private int chestId;
		[AutoNotify] private string nameStr;
		[AutoNotify] private string iconPath;
	    [AutoNotify] private ObservableList<CellItemBaseViewModel> scrollViewList = new();
	    [AutoNotify] private ShopBuyState shopBuyState;
	    [AutoNotify] private ItemPriceNodeModel itemPriceNodeModel;
        [Preserve]
        public ShopBoxInfoViewModel(int chestId)
        {
	        ChestId = chestId;
	        Cfg = ConfigCenter.EquipChestCfgColl.GetDataById(chestId);
	        NameStr = ShopManager.Instance.GetEquipChestName(chestId);
	        IconPath = ShopManager.Instance.GetEquipChestIconPath(chestId);
	        RefreshRewardList();
	        RefreshState();
	        DataCenter.itemsData.OnItemUpdate += ItemUpdate;
	        DataCenter.shopData.Recruit.PropertyChanged += RecruitChanged;
        }
        
        protected override void OnDispose()
        {
	        base.OnDispose();
	        DataCenter.itemsData.OnItemUpdate -= ItemUpdate;
	        DataCenter.shopData.Recruit.PropertyChanged -= RecruitChanged;
	        ItemPriceNodeModel?.Dispose();
        }

        private void RecruitChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName is nameof(Recruit.CommFree) or nameof(Recruit.CollectFree))
	        {
		        RefreshState();
	        }
        }

        private void ItemUpdate(ResourceData data)
        {
	        RefreshState();
        }

        private void RefreshRewardList()
        {
	        scrollViewList.Clear();
	        var rewardList = ShopManager.Instance.GetBoxAllReward(ChestId,DataCenter.shopData.GetBoxInfoLv());
	        foreach (var item in rewardList)
	        {
		        var itemModel = CellItemBaseViewModel.Create(item);
		        // itemModel.FongType = ECellItemFontType.Bottom;
		        scrollViewList.Add(itemModel);
	        }
        }
        
        private void RefreshState()
        {
	        ShopBuyState = ShopManager.Instance.GetEquipChestState(cfg?.Id ?? 0,false);
	        var reward = ShopManager.Instance.GetEquipChestItem(cfg?.Id ?? 0);
	        if(reward!=null)
		        ItemPriceNodeModel = new ItemPriceNodeModel(reward, true);
        }

        [Command]
        private void OnClickBtnClose()
        {
	        UIManager.Instance.CloseDialog<ShopBoxInfoView>();
        }

        [Command]
        private void OnClickBtnAd()
        {
	        ShopManager.Instance.SendAdDraw(chestId);
        }
        
        [Command]
        private void OnClickBtnUseItem()
        {
	        ShopManager.Instance.SendItemDraw(chestId,itemPriceNodeModel.Reward, (list) =>
	        {
		        OnClickBtnClose();
	        });
        }

        
    }
}