using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class AdFreeGiftPrivilegeMonthCardItemViewModel : ViewModelBase
    {
	    [AutoNotify] private bool isShowSelf = true;
	    [AutoNotify] private ObservableList<AdFreeGiftEffectItemViewModel> scrollViewLockList = new();
	    [AutoNotify] private ObservableList<CellItemBaseViewModel> awardScrollviewList = new();
	    [AutoNotify] private CellItemBaseViewModel atOnceAwardCellVm;
	    [AutoNotify] private BtnPriceNodeModel btnPriceNodeVm;
	    private MonthCardData Data => DataCenter.monthCardData;
	    private MonthlyVipMainCfg MonthCard;
	    [AutoNotify] private string titleStr;
	    
        [Preserve]
        public AdFreeGiftPrivilegeMonthCardItemViewModel()
        {
	        MonthCard = ConfigCenter.MonthlyVipMainCfgColl.GetDataById((int)MonthType.PrivilegeMonthCard);
	        ScrollViewLockList.Clear();
	        for (int i = 0; i < MonthCard.EffectId.Count; i++)
	        {
		        var effectId =  MonthCard.EffectId[i];
		        var effectCfg = ConfigCenter.MonthlyVipEffectLanguageCfgColl.GetDataById(effectId);
		        var effectCfg2 = ConfigCenter.MonthlyVipEffectCfgColl.GetDataById(effectId);
		        var desTemplate = Data.OutPutEffectDes(effectCfg.Dec, effectCfg2.Value);
		        ScrollViewLockList.Add(new AdFreeGiftEffectItemViewModel((MonthCardEffectType)MonthCard.EffectId[i],desTemplate));
	        }
	        TitleStr = ConfigCenter.MonthlyVipMainLanguageCfgColl.GetDataById((int)MonthType.PrivilegeMonthCard).Name;
	        AwardScrollviewList.Clear();
	        for (int i = 0; i < MonthCard.DaiyReward.Count; i++)
	        {
		        var cellModel = CellItemBaseViewModel.Create(MonthCard.DaiyReward[i]);
		        cellModel.SetSize(ECellItemSizeType.Size90X80);
		        AwardScrollviewList.Add(cellModel);
	        }
	        
	        AtOnceAwardCellVm =  CellItemBaseViewModel.Create(MonthCard.Reward[0]);
	        AtOnceAwardCellVm.SetSize(ECellItemSizeType.Size90X80);
	        
	        var packageCfg = ConfigCenter.PackageCfgColl.GetDataById(MonthCard.PackageId);
	        var cfgInfo = ConfigCenter.RechargeCfgColl.GetDataById(packageCfg.RechargeId);
	        string priceStr = "";
	        if (cfgInfo != null) priceStr = PayController.Instance.GetLocalizePrice(cfgInfo.PayId);
	        BtnPriceNodeVm = new BtnPriceNodeModel(priceStr);
        }


        [Command]
        private void OnClickBuyButton()
        {
	        ShopManager.Instance.SendBuyPackageBuyRecharge(MonthCard.PackageId,null,0,-1,1, (rewardList,costList) =>
	        {
		        OnBuyCallback(rewardList,costList).Forget();
	        });
	        
        }
        private async UniTask OnBuyCallback(List<Resource> rewardList,List<Resource> costList)
        {
	        Lodash.DealRewards(rewardList,costList);
	        Data.OnBuyPlus();
	        if (Data.IsCanGetAward(MonthType.PrivilegeMonthCard) && !Data.ToDayIsGetAward(MonthType.PrivilegeMonthCard))
	        {
		        var req = new ReqMonthCardClaim();
		        req.Op = MonthCard.Id;
		        var tempRes = await GameNetworkManager.Instance.SendAsync<RspMonthCardClaim>(req);
		        if (tempRes.rsp.Status == 0)
		        {
			        Lodash.DealRewards(tempRes.rsp.Reward.ToList());
			        Data.GetTodayAward(MonthType.PrivilegeMonthCard);
			        List<Resource> tempRewards = new List<Resource>(rewardList);
			        List<Resource> tempRewards2 = new List<Resource>(tempRes.rsp.Reward);
			        var tempRewards3 = UIHelper.MergeLists(tempRewards, tempRewards2);
			        UIManager.Instance.OpenDialog<MonthCardAwardShowView>(new MonthCardAwardShowViewModel(MonthCard,tempRewards3,true)).Forget();
			        IsShowSelf = false;
		        }
		        else
		        {
			        ToastManager.Show(UIHelper.GetNetErrorMessage(tempRes.rsp.Status));
		        }
	        }
	        else
	        {
		        UIManager.Instance.OpenDialog<MonthCardAwardShowView>(new MonthCardAwardShowViewModel(MonthCard,rewardList)).Forget();
	        }
	        if (Data.IsMonthVip&& Data.DoubleStatus != 2)
	        {
		        Data.DoubleStatus = 1;
	        }
	        IsShowSelf = false;
        }
        
    }
}