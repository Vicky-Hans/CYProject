using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class ShopGoldItemViewModel : ViewModelBase
    {
        
	    [AutoNotify] private PackageCfg cfg;
	    [AutoNotify] private string iconPath;
	    [AutoNotify] private string bgPath;
	    [AutoNotify] private string priceBgPath;
	    [AutoNotify] private string cntValueStr;
	    [AutoNotify] private ItemPriceNodeModel itemPriceNodeModel;
	    [AutoNotify] private ShopBuyState shopBuyState;
	    [AutoNotify] private Color priceColor;
	    public CommonAdvIconViewModel CommonAdvVm;
        [Preserve]
        public ShopGoldItemViewModel(int id)
        {
	        cfg = ConfigCenter.PackageCfgColl.GetDataById(id);
	        InitDiamondInfo();
	        RefreshDiamondInfo();
	        DataCenter.shopData.PropertyChanged += ShopPropertyChanged;
	        
	        CommonAdvVm = new CommonAdvIconViewModel();
        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        DataCenter.shopData.PropertyChanged -= ShopPropertyChanged;
	        ItemPriceNodeModel?.Dispose();
	        CommonAdvVm?.Dispose();
        }

        private void ShopPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(DataCenter.shopData.CoinFree))
	        {
		        RefreshDiamondInfo();
	        }
        }

        private void InitDiamondInfo()
        {
	        IconPath =  IconPath = ShopManager.Instance.GetPackageIcon(cfg.Id); //UIHelper.GetRewardsIconPath(cfg?.Reward?[0]);
	        CntValueStr = $"x{cfg?.Reward?[0].Count ?? 0}";
	        ItemPriceNodeModel = new ItemPriceNodeModel(Cfg.Cost?[0]);
        }

        private void RefreshDiamondInfo()
        {
		    ShopBuyState = ShopManager.Instance.GetPackageState(Cfg.Id,cfg.RechargeId == -2?DataCenter.shopData.GetGoldCnt():0);
		    BgPath = ShopBuyState is ShopBuyState.Adv or ShopBuyState.Free ? "shop[shop_panel_9]":"shop[shop_panel_7]";
		    PriceBgPath  = ShopBuyState is ShopBuyState.Adv or ShopBuyState.Free ? "shop[shop_panel_10]":"shop[shop_panel_8]";
		    priceColor = UIHelper.HexColorStrToColor(ShopBuyState is ShopBuyState.Adv or ShopBuyState.Free
			    ? "#283512"
			    : "#67472A");

        }

        [Command]
        private void OnClickBtnAd()
        {
	        ShopManager.Instance.SendBuyPackageBuyRecharge(cfg.Id, (id) =>
	        {
		        var packageCfg = ConfigCenter.PackageCfgColl.GetDataById(id);
		        if (packageCfg != null && packageCfg.RechargeId == -2)
		        {
			        DataCenter.shopData.SetAddGoldCnt();
		        }
	        },cfg.RechargeId == -2?DataCenter.shopData.GetGoldCnt():0);
        }

        [Command]
        private void OnClickBtnFree()
        {
	        ShopManager.Instance.SendBuyPackageBuyRecharge(cfg.Id, (id) =>
	        {
		        var packageCfg = ConfigCenter.PackageCfgColl.GetDataById(id);
		        if (packageCfg != null && packageCfg.RechargeId == -2)
		        {
			        DataCenter.shopData.SetAddGoldCnt();
		        }
	        },cfg.RechargeId == -2?DataCenter.shopData.GetGoldCnt():0);
        }

        [Command]
        private void OnClickBtnUseItem()
        {
	        if(cfg?.Reward?[0]==null) return;
	        
	        var selectModel = new ShopSelectLimitViewModel(cfg?.Reward?[0],itemPriceNodeModel.Reward,
		        (selectNum) =>
		        {
			        if(!UIHelper.CheckRewardIsEnough(itemPriceNodeModel.Reward,true,selectNum)) return;
			        ShopManager.Instance.SendBuyPackageBuyRecharge(cfg.Id, (id) =>
			        {
				        var packageCfg = ConfigCenter.PackageCfgColl.GetDataById(id);
				        if (packageCfg != null && packageCfg.RechargeId == -2)
				        {
					        DataCenter.shopData.SetAddGoldCnt();
				        }
			        },cfg.RechargeId == -2?DataCenter.shopData.GetGoldCnt():0,0,selectNum);
		        },ShopManager.Instance.GetLimitBuyCnt());
	        UIManager.Instance.OpenDialog<ShopSelectLimitView>(selectModel).Forget();
        }

        
    }
}