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
    public partial class ShopDailyItemViewModel : ViewModelBase
    {
	    [AutoNotify] private int dailyId;
	    [AutoNotify] private DailyShopCfg cfg;
		[AutoNotify] private string nameStr;
		[AutoNotify] private string limitValueStr;
		[AutoNotify] private CellItemBaseViewModel cellItemBaseViewModel;
		[AutoNotify] private ShopBuyState shopBuyState;
		[AutoNotify] private ItemPriceNodeModel itemPriceViewModel;
		[AutoNotify] private string baPath;
		[AutoNotify] private bool isShowDis;
		[AutoNotify] private string discountStr;
		[AutoNotify] private bool isFinish;
		[AutoNotify] private Color nameColor;
		public CommonAdvIconViewModel CommonAdvVm;
		private ShopGoodsData dailyData;
		public ShopGoodsData DailyData
		{
			get => dailyData;
			set
			{
				var old = dailyData;
				Set(ref dailyData, value);
				if (old != null)
				{
					old.PropertyChanged -= DailyChange;
				}
                
				if (dailyData != null)
				{
					dailyData.PropertyChanged += DailyChange;
				}
			}
		}

		

		[Preserve]
        public ShopDailyItemViewModel(DailyShopCfg cfg)
        {
	        DailyId = cfg.Id;
	        Cfg = cfg;
	        RefreshInfo();
	        CommonAdvVm = new CommonAdvIconViewModel();
        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        DailyData = null;
	        ItemPriceViewModel?.Dispose();
	        CellItemBaseViewModel?.Dispose();
	        CommonAdvVm?.Dispose();
        }
        
        private void DailyChange(object sender, PropertyChangedEventArgs e)
        {
	        RefreshInfo();
        }

        private void RefreshInfo()
        {
	        DailyData = DataCenter.shopData.GetDailyData(dailyId);
	        if (DailyData != null && Cfg!=null)
	        {
		        IsFinish = DailyData.Limit == 0;
			    CellItemBaseViewModel = CellItemBaseViewModel.Create(dailyData.Reward);
			    LimitValueStr = $"{dailyData.Limit}/{Cfg.Limit}";
			    NameStr = UIHelper.GetRewardName(DailyData.Reward);
		        RefreshState();
		        if (Cfg.TypeId == 1)
		        {
			        ItemPriceViewModel = new ItemPriceNodeModel(UIHelper.GetGold(dailyData.Price));
		        }else if (Cfg.TypeId == 2)
		        {
			        ItemPriceViewModel = new ItemPriceNodeModel(UIHelper.GetDiamond(dailyData.Price));
		        }

		        IsShowDis = dailyData.Discount!=10 && dailyData.Discount!=0;
		        DiscountStr = ShopManager.Instance.GetDailyDiscountDesc(dailyData.Discount);// "打折"; //LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips23, dailyData.Discount);
	        }
	        else
	        {
		        IsFinish = false;
		        IsShowDis = false;
		        LimitValueStr = string.Empty;
		        CellItemBaseViewModel = null;
		        NameStr = string.Empty;
	        }
        }

        private void RefreshState()
        {
	        if (Cfg != null)
	        {
		        switch (Cfg.TypeId)
		        {
			        case -2:
			        {
				        var buyCnt = Cfg.Limit - dailyData.Limit;
				        if (buyCnt == 0)
				        {
					        ShopBuyState = ShopBuyState.Free;
				        }else if (buyCnt == 1 || buyCnt == 2)
				        {
					        ShopBuyState = ShopBuyState.Adv;
				        }
				        else
				        {
					        ShopBuyState = ShopBuyState.Item;
				        }

				        break;
			        }
			        case -1: ShopBuyState = ShopBuyState.Adv; break;
			        case 0: ShopBuyState = ShopBuyState.Free; break;
			        case 1: ShopBuyState = ShopBuyState.Item; break;
			        case 2: ShopBuyState = ShopBuyState.Item; break;
		        }

		        BaPath = ShopBuyState is ShopBuyState.Adv or ShopBuyState.Free ? "shop[shop_panel_9]":"shop[shop_panel_7]";
		        nameColor = UIHelper.HexColorStrToColor(ShopBuyState is ShopBuyState.Adv or ShopBuyState.Free
			        ? "#283512"
			        : "#67472A");
	        }
        }


        [Command]
        private void OnClickBtnAd()
        {
	        UIHelper.ShowRewardAds(() =>
	        {
		        ShopManager.Instance.SendShopDailyBuy(dailyId).Forget();
	        });
        }

        [Command]
        private void OnClickBtnFree()
        {
	        ShopManager.Instance.SendShopDailyBuy(dailyId).Forget();
        }

        [Command]
        private void OnClickBtnUseItem()
        {
	        if(dailyData==null || cfg==null) return;
	        int limit = dailyData.Limit;
	        if (limit <= 0)
	        {
		        // ToastManager.Show("达到限购上限");
		        return;
	        }
	        if (limit == 1)
	        {
		        if(!UIHelper.CheckRewardIsEnough(itemPriceViewModel.Reward,isJump:true)) return;
		        ShopManager.Instance.SendShopDailyBuy(dailyId).Forget();
		        return;
	        }
	        
	        var selectModel = new ShopSelectLimitViewModel(UIHelper.ResourceDataToReward(cellItemBaseViewModel.BaseData),itemPriceViewModel.Reward,
		        (selectNum) =>
		        {
			        if(!UIHelper.CheckRewardIsEnough(itemPriceViewModel.Reward,true,selectNum)) return;
			        ShopManager.Instance.SendShopDailyBuy(dailyId,null,selectNum).Forget();
		        },limit);
	        UIManager.Instance.OpenDialog<ShopSelectLimitView>(selectModel).Forget();
        }

        
    }
}