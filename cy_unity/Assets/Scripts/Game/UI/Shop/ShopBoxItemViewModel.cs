using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
namespace DH.Game.ViewModels
{
	public enum DrawType
	{
		Base,
		Clothes,
	}

	public partial class ShopBoxItemViewModel : ViewModelBase
    {
	    [AutoNotify] private EquipChestCfg cfg;
		[AutoNotify] private string iconPath;
		[AutoNotify] private string bgPath;
		[AutoNotify] private string bgPath1;
		[AutoNotify] private string nameStr;
		[AutoNotify] private string tipsDescStr;
		public CommonAdvIconViewModel CommonAdvVm;
		[AutoNotify] private ItemPriceNodeModel itemPriceNodeModel;
		[AutoNotify] private ShopBuyState shopBuyState;

		[AutoNotify] private DrawType showDrawType= DrawType.Base;
		[AutoNotify] private string tipsDescStr1;
		
		[AutoNotify] private ItemPriceNodeModel itemPriceNodeModelOne;
		[AutoNotify] private ItemPriceNodeModel itemPriceNodeModelTen;
		
		
		[AutoNotify] private string btnDrawOneCnt;
		[AutoNotify] private string btnDrawTenCnt;
        [Preserve]
        public ShopBoxItemViewModel(EquipChestCfg cfg)
        {
	        Cfg = cfg;
	        if (Cfg.Id == (int)EquipChestId.Clothes)
	        {
		        ShowDrawType = DrawType.Clothes;
		        RefreshClothesData();
		        IconPath = UIHelper.NoneImagePath();
		        BgPath = "shop[shop_panel_23]";

	        }
	        else
	        {
		        IconPath = ShopManager.Instance.GetEquipChestIconPath(Cfg);
		        BgPath = ShopManager.Instance.GetEquipChestBg(Cfg);
	        }

	        BgPath1 = ShopManager.Instance.GetEquipChestBg1(Cfg);
	        NameStr = ShopManager.Instance.GetEquipChestName(Cfg);
	        TipsDescStr = ShopManager.Instance.GetEquipChestDesc(Cfg);
	        BtnDrawOneCnt = LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips22, 1);
	        BtnDrawTenCnt = LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips22, 10);
	        RefreshState();
	        DataCenter.itemsData.OnItemUpdate += ItemUpdate;
	        DataCenter.shopData.Recruit.PropertyChanged += RecruitChanged;
	        CommonAdvVm = new CommonAdvIconViewModel();

	     
        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        DataCenter.itemsData.OnItemUpdate -= ItemUpdate;
	        DataCenter.shopData.Recruit.PropertyChanged -= RecruitChanged;
	        ItemPriceNodeModel?.Dispose();
	        CommonAdvVm?.Dispose();
        }

        private void RecruitChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName is nameof(Recruit.CommFree) or nameof(Recruit.CollectFree))
	        {
		        RefreshState();
	        }
	        
	        if (e.PropertyName is nameof(Recruit.MinGuarantee) or nameof(Recruit.MaxGuarantee))
	        {
		        RefreshClothesData();
	        }
        }

        private void ItemUpdate(ResourceData data)
        {
	        RefreshState();
        }

        private void RefreshClothesData()
        {
	        TipsDescStr1 = ShopManager.Instance.GetClothesDrawTips();
        }


        private void RefreshState()
        {
	        ShopBuyState = ShopManager.Instance.GetEquipChestState(cfg?.Id ?? 0 ,false);
	        var reward = ShopManager.Instance.GetEquipChestItem(cfg?.Id ?? 0);
	        if (reward != null)
	        {
		        ItemPriceNodeModel = new ItemPriceNodeModel(reward, true,null,!UIHelper.IsDiamond(reward));

		        if (ShowDrawType == DrawType.Clothes)
		        {
			        ItemPriceNodeModelOne = new ItemPriceNodeModel(reward, true,null,!UIHelper.IsDiamond(reward));
		        }
	        }

	        var rewardTen = ShopManager.Instance.GetEquipChestItem(cfg?.Id ?? 0,true);
	        if (rewardTen != null)
	        {
		        if (ShowDrawType == DrawType.Clothes)
		        {
			        ItemPriceNodeModelTen = new ItemPriceNodeModel(rewardTen, true,null,!UIHelper.IsDiamond(rewardTen));
		        }
	        }
        }

        [Command]
        private void OnClickBtnRuleTips()
        {
	        if (ShowDrawType == DrawType.Clothes)
	        {
		        UIManager.Instance.OpenDialog<ShopDrawClothesInfoView>(new ShopDrawClothesInfoViewModel()).Forget();
		        return;
	        }

	        //打开宝箱
	        UIManager.Instance.OpenDialog<ShopBoxInfoView>(new ShopBoxInfoViewModel(Cfg.Id)).Forget();
        }

        [Command]
        private void OnClickBtnAd()
        {
	        ShopManager.Instance.SendAdDraw(cfg.Id);
        }

        [Command]
        private void OnClickBtnFree()
        {
	        ShopManager.Instance.SendAdDraw(cfg.Id);
        }

        [Command]
        private void OnClickBtnUseItem(int cnt)
        {
	        if (ShowDrawType == DrawType.Clothes)
	        {
		        var itemPrice = cnt == 10 ? itemPriceNodeModelTen : itemPriceNodeModelOne;
		        if (!UIHelper.CheckRewardIsEnough(itemPrice.Reward,isJump:true))return;
					ShopManager.Instance.SendItemDrawClothes(cfg.Id,itemPrice.Reward,cnt);
	        }
	        else
	        {
		        if (!UIHelper.CheckRewardIsEnough(itemPriceNodeModel.Reward,isJump:true))return;
		        ShopManager.Instance.SendItemDraw(cfg.Id,itemPriceNodeModel.Reward);
	        }
        }

        
    }
}