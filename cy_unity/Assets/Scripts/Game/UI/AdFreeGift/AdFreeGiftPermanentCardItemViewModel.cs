using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class AdFreeGiftPermanentCardItemViewModel : ViewModelBase
    {	   
	    [AutoNotify] private bool isShowSelf = true;
	    [AutoNotify] private ObservableList<AdFreeGiftEffectItemViewModel> scrollViewLockList = new();
	    [AutoNotify] private BtnPriceNodeModel btnPriceNodeVm;
	    private MonthCardData Data => DataCenter.monthCardData; 
        [Preserve]
        public AdFreeGiftPermanentCardItemViewModel()
        {
	        var MonthCard = ConfigCenter.MonthlyVipMainCfgColl.GetDataById((int)MonthType.PermanentCard);
	        
	        ScrollViewLockList.Clear();
	        for (int i = 0; i < MonthCard.EffectId.Count; i++)
	        {
		        var effectId =  MonthCard.EffectId[i];
		        var effectCfg = ConfigCenter.MonthlyVipEffectLanguageCfgColl.GetDataById(effectId);
		        var effectCfg2 = ConfigCenter.MonthlyVipEffectCfgColl.GetDataById(effectId);
		        var desTemplate = Data.OutPutEffectDes(effectCfg.Dec, effectCfg2.Value);
		        ScrollViewLockList.Add(new AdFreeGiftEffectItemViewModel((MonthCardEffectType)MonthCard.EffectId[i],desTemplate));
	        }
	        var packageCfg = ConfigCenter.PackageCfgColl.GetDataById(MonthCard.PackageId);
	        var cfgInfo = ConfigCenter.RechargeCfgColl.GetDataById(packageCfg.RechargeId);
	        string priceStr = "";
	        if (cfgInfo != null) priceStr = PayController.Instance.GetLocalizePrice(cfgInfo.PayId);
	        BtnPriceNodeVm = new BtnPriceNodeModel(priceStr);
        }


        [Command]
        private void OnClickBuyButton()
        {
	        if (Data.IsPermanent)return;
            
	        var cfg = ConfigCenter.MonthlyVipMainCfgColl.GetDataById((int)MonthType.PermanentCard);
	        ShopManager.Instance.SendBuyPackageBuyRecharge(cfg.PackageId,null,0,-1,1, (rewardList,costList) =>
	        {
		        Data.LifetimeCard = true;
		        Lodash.DealRewards(rewardList); 
		        Lodash.DealRewards(costList,false); 
		        //UIHelper.OpenCommonRewardView(rewardList.ToList());
                
		        UIManager.Instance.OpenDialog<MonthCardAwardShowView>(new MonthCardAwardShowViewModel(cfg,rewardList)).Forget();
		        IsShowSelf = false;
	        });
        }

        
    }
}