using System.Collections.Specialized;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;

namespace DH.Game.ViewModels
{
    public partial class ShopDiamondItemViewModel : ViewModelBase
    {

	    [AutoNotify] private PackageCfg cfg;
		[AutoNotify] private string iconPath;
		[AutoNotify] private string cntValueStr;
		[AutoNotify] private bool isShowDiscount;
		// [AutoNotify] private string disValueStr;
		[AutoNotify] private BtnPriceNodeModel btnPriceNodeModel;
		[AutoNotify] private string limitCntDesc;
		[AutoNotify] private ItemPriceNodeModel itemPriceNodeModel;
        [Preserve]
        public ShopDiamondItemViewModel(int id)
        {
	        cfg = ConfigCenter.PackageCfgColl.GetDataById(id);
	        InitDiamondInfo();
	        RefreshDiamondInfo();
	        DataCenter.shopData.DiamondDouble.CollectionChanged += DiamondDoubleChanged;
        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        DataCenter.shopData.DiamondDouble.CollectionChanged -= DiamondDoubleChanged;
        }

        private void DiamondDoubleChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
	        RefreshDiamondInfo();
        }

        private void InitDiamondInfo()
        {
	        var count = cfg?.Reward?[0].Count ?? 0;
	        IconPath = ShopManager.Instance.GetPackageIcon(cfg.Id); //UIHelper.GetRewardsIconPath(cfg?.Reward?[0]);
	        CntValueStr = $"x{count}";
	        BtnPriceNodeModel = new BtnPriceNodeModel(Cfg.Id);
	        // DisValueStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips11,count); //ShopManager.Instance.GetPackageDiscount(Cfg.Id);
	        RefreshDiamondInfo();
        }

        private void RefreshDiamondInfo()
        {
	        var limitCnt = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.gemShop_doubleTimes).Content[0];
	        if (limitCnt > 0)
	        {
		        var buyCnt = DataCenter.shopData.GetDiamondBuyCnt(Cfg.Id);
		        IsShowDiscount = limitCnt > buyCnt;
		        LimitCntDesc = limitCnt > buyCnt
			        ? $"{LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips24)}{limitCnt - buyCnt}"
			        : $"{LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips24)}{0}";
		        if (cfg != null && cfg.Reward != null && cfg.Reward.Count > 0)
		        {
			        ItemPriceNodeModel = new ItemPriceNodeModel(cfg.Reward[0],false,null,false,"+{0}");
		        }
	        }
	        else
	        {
		        LimitCntDesc = string.Empty;
		        IsShowDiscount = false;

	        }

        }

        [Command]
        private void OnClickBtnBuy()
        {
	        ShopManager.Instance.SendBuyPackageBuyRecharge(cfg.Id, (packId) =>
	        {
		        DataCenter.shopData.SetDiamondShop(packId);
	        });
        }

        
    }
}