using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;


namespace DH.Game.ViewModels
{
    public partial class RechargePointCellViewModel : ViewModelBase
    {
        
		[AutoNotify] private string titleTextStr;
		[AutoNotify] private string idTextStr;
	    [AutoNotify] private ObservableList<CellItemBaseViewModel> infoScrollViewList = new();
		[AutoNotify] private BtnPriceNodeModel btnPriceNodeVm;
		private RechargeCfg curCfg;

        [Preserve]
        public RechargePointCellViewModel(RechargeCfg cfg)
        {
	        curCfg = cfg;
	        foreach (var item in cfg.Items)
	        {
		        var tempVm = CellItemBaseViewModel.Create(item);
		        tempVm.SetSize(ECellItemSizeType.Size90X80);
		        InfoScrollViewList.Add(tempVm);
	        }
	        IdTextStr = cfg.Id.ToString();
	        TitleTextStr = cfg.Name;
	        string priceStr = PayController.Instance.GetLocalizePrice(cfg.PayId);
	        // long pointNum = cfg.Points;
			BtnPriceNodeVm = new (priceStr);
        }

        [Command]
        private void OnClickOpBtn()
        {
	        // 这里传 packageId
	        // PayController.Instance.Pay(curCfg.PackageId,callback:(result) =>
	        // {
		       //  if (result == null || result.Status != 0)
		       //  {
			      //   DHLog.Debug($"超值月卡购买失败 {result?.Status}");
			      //   return;
		       //  }
		       //  UIHelper.OpenCommonRewardView(result.Rewards);
		       //  Lodash.DealRewards(result.Rewards);
	        // });
	        //
            ShopManager.Instance.SendBuyPackageBuyRecharge(curCfg.PackageId);
        }
    }
}