using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;


namespace DH.Game.ViewModels
{
    public partial class AdFreePlusPermanentCardItemViewModel : ViewModelBase
    { 
	    public override bool AutoDispose => true;
	    [AutoNotify] private BtnPriceNodeModel btnPriceNodeVm;
	    [AutoNotify] private ObservableList<CellItemBaseViewModel> awardScrollviewList = new();
	    private MonthCardData Data => DataCenter.monthCardData; 
	    [AutoNotify] private bool isShowGrayButton;
	    public CommonPlayerNameViewModel CommonPlayerNameVm;
        [Preserve]
        public AdFreePlusPermanentCardItemViewModel()
        {
	        var MonthCard = ConfigCenter.MonthlyVipMainCfgColl.GetDataById((int)MonthType.PermanentCard);

	        AwardScrollviewList.Clear();
	        for (int i = 0; i < MonthCard.Reward.Count; i++)
	        {
		        var cellModel = CellItemBaseViewModel.Create(MonthCard.Reward[i]);
		        cellModel.SetSize(ECellItemSizeType.Size90X80);
		        AwardScrollviewList.Add(cellModel);
	        }
	        
	        var packageCfg = ConfigCenter.PackageCfgColl.GetDataById(MonthCard.PackageId);
	        var cfgInfo = ConfigCenter.RechargeCfgColl.GetDataById(packageCfg.RechargeId);
	        string priceStr = "";
	        if (cfgInfo != null) priceStr = PayController.Instance.GetLocalizePrice(cfgInfo.PayId);
	        BtnPriceNodeVm = new BtnPriceNodeModel(priceStr);
	        IsShowGrayButton = Data.IsPermanent;
	        CommonPlayerNameVm = new CommonPlayerNameViewModel(DataCenter.charcaterData.Digest.Name,UIHelper.HexColorStrToColor("#6d4f3a"),true);
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
		        IsShowGrayButton = Data.IsPermanent;
	        });
        } 
    }
}