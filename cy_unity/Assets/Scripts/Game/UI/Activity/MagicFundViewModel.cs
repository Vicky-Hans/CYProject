using System.Collections.Generic;
using System.Linq;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class MagicFundViewModel : ViewModelBase
    {
        
	    [AutoNotify] private ObservableList<MagicFundCellViewModel> scrollViewList = new();
	    [AutoNotify] private string opBtnTextStr;
	    [AutoNotify] private bool isShowOpBtnText;
	    [AutoNotify] private BtnPriceNodeModel opBtnPriceVm;
	    [AutoNotify] private bool isCanClickOpBtn;
	    [AutoNotify] private bool isShowCostNode;
	    [AutoNotify] private int showIndex;

	    private int packageId = 39;
        [Preserve]
        public MagicFundViewModel()
        {
	        OpBtnPriceVm = new(packageId);
	        UpdateOpBtnState();
			InitScrollview();
	        UpdateShowIndex();
        }

        protected override void OnDispose()
        {
	        opBtnPriceVm.Dispose();
	        foreach (var item in ScrollViewList)
	        {
		        item.Dispose();
	        }
	        base.OnDispose();
        }

        private void InitScrollview()
        {
	        var cfgs = ConfigCenter.ActivityFundCfgColl.GetDataByType(1);
	        List<ActivityFundCfg> tempList = cfgs.ToList();
	        tempList.Sort((a, b) => a.Factor.CompareTo(b.Factor));
	        ScrollViewList.Clear();
	        foreach (var cfg in tempList)
	        {
		        MagicFundCellViewModel tempVm = new MagicFundCellViewModel(cfg, OnClickClaimBtn);
		        ScrollViewList.Add(tempVm);
	        }
        }

        private void UpdateOpBtnState()
        {
	        if (!DataCenter.magicDrawData.FundPlus)
	        {
		        IsCanClickOpBtn = true;
		        IsShowCostNode = true;
		        OpBtnTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Tehuijijin_02);
		        IsShowOpBtnText = false;
	        }
	        else
	        {
		        IsCanClickOpBtn = false;
		        IsShowCostNode = false;
		        OpBtnTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Tehuijijin_04);
		        IsShowOpBtnText = true;
	        }
        }

        private void UpdateScrollViewList()
        {
	        foreach (var item in ScrollViewList)
	        {
		        item.UpdatePanel();
	        }
        }

        private void UpdateShowIndex()
        {
	        showIndex = -2;
	        // 检查是否购买
	        // TO-DO: 购买后，检查展示的index是否需要刷新位置
	        if (DataCenter.magicDrawData.FundPlus)
	        {
		        //检查免费领取的
		        int pluseIndex = 0;
		        int freeIndex = 0;
		        foreach (var item in ScrollViewList)
		        {
			        if (pluseIndex == 0 && !DataCenter.magicDrawData.FundPlusClaimed.Contains(item.CurCfg.Id))
			        {
				        pluseIndex = item.CurCfg.Factor;
			        }
			        if (freeIndex == 0 &&!DataCenter.magicDrawData.FundClaimed.Contains(item.CurCfg.Id))
			        {
				        freeIndex = item.CurCfg.Factor;
			        }
			        if(freeIndex!=0 && pluseIndex!=0) break;
		        }
		        ShowIndex = Mathf.Min(freeIndex, pluseIndex) -1;
	        }
	        else
	        {
		        int freeIndex = 0;
		        foreach (var item in ScrollViewList)
		        {
			        if (!DataCenter.magicDrawData.FundClaimed.Contains(item.CurCfg.Id))
			        {
				        freeIndex = item.CurCfg.Factor;
				        break;
			        }
		        }
		        ShowIndex = freeIndex -1;
	        }
        }

        [Command]
        private void OnClickCloseBtn()
        {
	        UIManager.Instance.CloseDialog<MagicFundView>();
        }

		[Command]
		private void OnClickOpBtn()
		{
			if(DataCenter.magicDrawData.FundPlus) return;
			if( !IsCanClickOpBtn) return;
			// // 点击购买
			// PayController.Instance.Pay(packageId,callback:(result) =>
			// {
			// 	if (result == null || result.Status != 0)
			// 	{
			// 		DHLog.Debug($" 基金 购买失败 {result?.Status}");
			// 		// if(result == null) return;
			// 		// var str = UIHelper.GetNetErrorMessage(result.Status);
			// 		// ToastManager.Show(str);
			// 		UpdateOpBtnState();
			// 		return;
			// 	}
			// 	Lodash.DealRewards(result.Rewards);
			// 	DataCenter.magicDrawData.FundPlus = true;
			// 	UpdateScrollViewList();
			// 	UpdateOpBtnState();
			// 	ToastManager.ShowLanguage(GlobalLanguageId.ConfRecharge0);
			// });
			
			ShopManager.Instance.SendBuyPackageBuyRecharge(packageId,null,0,-1,1, (rewardList,costList) =>
			{
				Lodash.DealRewards(rewardList,costList);
				DataCenter.magicDrawData.FundPlus = true;
				UpdateScrollViewList();
				UpdateOpBtnState();
				ToastManager.ShowLanguage(GlobalLanguageId.ConfRecharge0);
			});
		}

		private async void OnClickClaimBtn()
		{
			RspMagicFundClaim data = new RspMagicFundClaim();
			var result = await GameNetworkManager.Instance.SendAsync<RspMagicFundClaim>(data);
			if (result.rsp ==null ||result.rsp.Status != 0)
			{
				DHLog.Debug(" 请求领取基金 失败 ");
				if(result.rsp == null) return;
				var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
				ToastManager.Show(str);
				return ;
			}
			DataCenter.magicDrawData.UpdateMagicFundClaimedList(result.rsp.FreeIds.ToList(), result.rsp.PlusIds.ToList());
			Lodash.DealRewards(result.rsp.Reward.ToList());
			UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList());
			UpdateScrollViewList();
			UpdateShowIndex();
		}


    }
}