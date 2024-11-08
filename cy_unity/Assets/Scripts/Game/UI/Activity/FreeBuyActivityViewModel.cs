using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using DH.Config;
using DH.Data;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;


namespace DH.Game.ViewModels
{
    public partial class FreeBuyActivityViewModel : ViewModelBase
    {
        
		 [AutoNotify] private ObservableList<FreeBuyActivityItemViewModel> scrollViewList = new();
		[AutoNotify] private BtnPriceNodeModel btnPriceNodeVm;
		[AutoNotify] private string btnTipsStr;

		[AutoNotify] private int packageId;
		[AutoNotify] private bool isBuy;
		[AutoNotify] private CommonTopViewModel commonTopViewModel;

		[AutoNotify] private int startPos;
        [Preserve]
        public FreeBuyActivityViewModel()
        {
	        CommonTopViewModel = new CommonTopViewModel(new List<GameConst.ItemIdCode>()
	        {
		        GameConst.ItemIdCode.Stone
	        });
	        
	        var list = ConfigCenter.PackageCfgColl.GetDataByRule((int)EPackageType.FreeBuy);
	        if (list != null && list.Count > 0)
	        {
		        packageId = list[0].Id;
		        scrollViewList.Add(new FreeBuyActivityItemViewModel(list[0],null));
	        }

	        var cfgList = ConfigCenter.PurchaseRewardCfgColl.DataItems.ToList();
	        for (int i = 0; i < cfgList.Count; i++)
	        {
		        if (DataCenter.giftPackData.IsBuyGift() && StartPos == 0 && !DataCenter.giftPackData.IsGetReward(cfgList[i].Id))
		        {
			        StartPos = i + 1;
		        }

		        scrollViewList.Add(new FreeBuyActivityItemViewModel(null,cfgList[i]));
	        }

	        BtnPriceNodeVm = new BtnPriceNodeModel(packageId);
	        RefreshBuy();
        }

        private void RefreshBuy()
        {
	        IsBuy = DataCenter.giftPackData.IsBuyGift();
        }


        [Command]
        private void OnClickBtnAd()
        {
	        if(!UIHelper.CheckRewardIsEnough(btnPriceNodeVm.Reward,true)) return;
	        if(packageId==0) return;
	        if(DataCenter.giftPackData.IsBuyGift()) return;
	        ShopManager.Instance.SendBuyPackageBuyRecharge(packageId, (id) =>
	        {
		        DataCenter.giftPackData.ZeroBuy = ServerTime.Instance.GetNowTime();
		        RefreshBuy();
	        });
        }

        
    }
}