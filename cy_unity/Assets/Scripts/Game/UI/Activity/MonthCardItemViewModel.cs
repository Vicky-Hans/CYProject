using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
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

namespace DH.Game.ViewModels
{
    public partial class MonthCardItemViewModel : ViewModelBase
    {
        
	    [AutoNotify] private ObservableList<MonthCardPropertyViewModel> effectDesList = new();
		[AutoNotify] private string timeDesStr;
	    [AutoNotify] private ObservableList<CellItemViewModel> awardScrollviewList = new();
		[AutoNotify] private bool isShowNotInForceStr;
		[AutoNotify] private bool isShowGetAwardBut;
		[AutoNotify] private string titleText;
		[AutoNotify] private CellItemBaseViewModel atOnceAwardCellVm;
		
		private readonly MonthType monthType = MonthType.MonthCard;
		private MonthlyVipMainCfg cfg;

		public BtnPriceNodeModel PriceNodeModel;
		
		private MonthCardData Data => DataCenter.monthCardData; 
		
        [Preserve]
        public MonthCardItemViewModel()
        {
	        cfg = ConfigCenter.MonthlyVipMainCfgColl.GetDataById((int)monthType);
	        
	        InitAwardList();

	        for (int i = 0; i < cfg.EffectId.Count; i++)
	        {
		        var effectId = cfg.EffectId[i];
		        var effectCfg = ConfigCenter.MonthlyVipEffectLanguageCfgColl.GetDataById(effectId);
		        var effectCfg2 = ConfigCenter.MonthlyVipEffectCfgColl.GetDataById(effectId);
		        var desTemplate = Data.OutPutEffectDes(effectCfg.Dec, effectCfg2.Value);
		        EffectDesList.Add(new MonthCardPropertyViewModel(desTemplate));
	        }
	        TitleText =ConfigCenter.MonthlyVipMainLanguageCfgColl.GetDataById(cfg.Id).Name;
	        
	        AtOnceAwardCellVm = CellItemBaseViewModel.Create(cfg.Reward[0]);
	        AtOnceAwardCellVm.SetSize(ECellItemSizeType.Size90X80);
	        
	        var packageCfg = ConfigCenter.PackageCfgColl.GetDataById(cfg.PackageId);
	        var cfgInfo = ConfigCenter.RechargeCfgColl.GetDataById(packageCfg.RechargeId);
	        string priceStr = "";
	        if (cfgInfo != null) priceStr = PayController.Instance.GetLocalizePrice(cfgInfo.PayId);
	        PriceNodeModel = new BtnPriceNodeModel(priceStr);
	        IsShowGetAwardBut = GetState() == ECellItemState.GetIng;
	        UpdateState();
	        PlayerInfoManager.Instance.PropertyChanged += ProperChange;
        }
        
        public void ProperChange(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName is nameof(PlayerInfoManager.Instance.SecondDay))
	        {
		        RefreshAwardList();
	        }
        }
        private void UpdateState()
        {
	        var times = Data.GetLastTime(monthType);
	        var notOver = times > 0;
	        IsShowNotInForceStr = !notOver;
	        if (notOver)
	        {
		        TimeDesStr =ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.MonthlyVip_tips04).Name 
		                    + ServerTime.Instance.SecondsDHms(times);
	        }
	        else
	        {
		        RefreshAwardList();
		        TimeDesStr = "";
	        }

        }
        private float time;
        public override void Update()
        {
	        base.Update();
	        if (!UIHelper.CalculateTime(ref time)) return;//每秒刷新
	        UpdateState();
        }
        
        public void InitAwardList()
        {
	        AwardScrollviewList.Clear();
	        for (int i = 0; i < cfg.DaiyReward.Count; i++)
	        {
		        var cellModel = CellItemViewModel.Create(cfg.DaiyReward[i]);
		        cellModel.SetSize(ECellItemSizeType.Size120X100);
		        cellModel.State = GetState();
		        AwardScrollviewList.Add(cellModel);
	        }
        }

        private void RefreshAwardList()
        {
	        foreach (var item in AwardScrollviewList)
	        {
		        item.State = GetState();
	        }
	        IsShowGetAwardBut = GetState() == ECellItemState.GetIng;
        }

        private ECellItemState GetState()
        {
	        if (!Data.IsMonthVip)
	        {
		        return ECellItemState.None;
	        }
	        

	        if (Data.ToDayIsGetAward(monthType))
	        {
		        return ECellItemState.Finish;
	        }
	        
	        if (!Data.ToDayIsGetAward(monthType) && Data.NormalCard.AllNums < Data.NormalCard.MaxNum)
	        {
		        return ECellItemState.GetIng;
	        }
	        return ECellItemState.None;
        }

        [Command]
        private async void OnClickBuyButton()
        {
	        // PayController.Instance.Pay(cfg.PackageId,callback:(result) =>
	        // {
		       //  if (result == null || result.Status != 0)
		       //  {
			      //   DHLog.Debug($"超值月卡购买失败 {result.Status}");
			      //   return;
		       //  }
		       //  OnBuyCallback(result);
	        // });

	        ShopManager.Instance.SendBuyPackageBuyRecharge(cfg.PackageId,null,0,-1,1, (rewardList,costList) =>
	        {
		        OnBuyCallback(rewardList,costList).Forget();
	        });
        }
        [Command]
        private async void GetDayAwardBtn()
        {
	        if (!Data.IsMonthVip)return;

	        if (!Data.IsCanGetAward(MonthType.MonthCard))return;

	        if (!Data.ToDayIsGetAward(monthType))
	        {
		        var req = new ReqMonthCardClaim();
		        req.Op = cfg.Id;
		        var tempRes = await GameNetworkManager.Instance.SendAsync<RspMonthCardClaim>(req);
		        if (tempRes.rsp.Status == 0)
		        {
			        Lodash.DealRewards(tempRes.rsp.Reward.ToList());
			        Data.GetTodayAward(monthType);
			        InitAwardList();
			        // CommonRewardMsgBoxViewModel temp = new CommonRewardMsgBoxViewModel(tempRes.rsp.Reward.ToList(),false,true);
			        // UIManager.Instance.OpenDialog<CommonRewardMsgBoxView>(temp).Forget();
			        UIHelper.OpenCommonRewardView(tempRes.rsp.Reward.ToList());
		        }
		        else
		        {
			        ToastManager.Show(UIHelper.GetNetErrorMessage(tempRes.rsp.Status));
		        }
	        }
        }
        
        private async UniTask OnBuyCallback(List<Resource> rewardList,List<Resource> costList)
        {
	        Lodash.DealRewards(rewardList,costList);
	        Data.OnBuyNormal();
	        if (Data.IsCanGetAward(MonthType.MonthCard) && !Data.ToDayIsGetAward(monthType))
	        {

		        var req = new ReqMonthCardClaim();
		        req.Op = cfg.Id;
		        var tempRes = await GameNetworkManager.Instance.SendAsync<RspMonthCardClaim>(req);
		        if (tempRes.rsp.Status == 0)
		        {
			        Lodash.DealRewards(tempRes.rsp.Reward.ToList());
			        Data.GetTodayAward(monthType);
			        RefreshAwardList();
			        List<Resource> tempRewards = new List<Resource>(rewardList);
			        List<Resource> tempRewards2 = new List<Resource>(tempRes.rsp.Reward);
			        var tempRewards3 = UIHelper.MergeLists(tempRewards, tempRewards2);
			        UIManager.Instance.OpenDialog<MonthCardAwardShowView>(new MonthCardAwardShowViewModel(cfg,tempRewards3,true)).Forget();
		        }
		        else
		        {
			        ToastManager.Show(UIHelper.GetNetErrorMessage(tempRes.rsp.Status));
		        }
	        }
	        else
	        {
		        UIManager.Instance.OpenDialog<MonthCardAwardShowView>(new MonthCardAwardShowViewModel(cfg,rewardList)).Forget();
	        }

	        if (Data.IsMonthVipPlus && Data.DoubleStatus != 2)
	        {
		        Data.DoubleStatus = 1;
	        }
        }
        protected override void OnDispose()
        {
	        PlayerInfoManager.Instance.PropertyChanged -= ProperChange;
	        foreach (var item in effectDesList)
	        {
		        item.Dispose();
	        }
	        foreach (var item in awardScrollviewList)
	        {
		        item.Dispose();
	        }
	        PriceNodeModel.Dispose();
	        base.OnDispose();
        }
        
    }
}