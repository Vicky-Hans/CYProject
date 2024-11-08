using System;
using System.Linq;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;


namespace DH.Game.ViewModels
{
	public partial class PatrolFastRewardViewModel : ViewModelBase
	{

		[AutoNotify] private ObservableList<CellItemBaseViewModel> scrollViewList = new();
		[AutoNotify] private string adLastNumsStr;
		[AutoNotify] private string goldLastNumsStr;

		[AutoNotify] private bool adButGray;
		[AutoNotify] private bool powerButGray;
		[AutoNotify] private string fastAwardText;
		private int PowerCos => ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.PatrolIncome_05).Content[0];
		public CopyCfg CopyThreadCfg;
		private HangupData Data => DataCenter.mainStageData.Hangup;

		[AutoNotify] private CommonAdvIconViewModel commonAdvIconVm;
		[AutoNotify] private ItemPriceNodeModel itemPriceNodeVm;

		[Preserve]
		public PatrolFastRewardViewModel()
		{
			InitButs();

			CommonAdvIconVm = new CommonAdvIconViewModel();

			var hours = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.PatrolIncome_02).Content[0];
			FastAwardText = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips52,hours);
			
			var powerCos = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.PatrolIncome_05)
				.Content[0];
			ItemPriceNodeVm = new ItemPriceNodeModel(new Reward(RewardType.Item,
				(int)GameConst.ItemIdCode.EnergyDrink, powerCos));

			CopyThreadCfg = ConfigCenter.CopyCfgColl.GetDataById(DataCenter.mainStageData.GetMaxPassChapter());
			ScrollViewList.Clear();
			
			for (int i = 0; i < CopyThreadCfg.Hangup.Count; i++)
			{
				var AwardItem = new Reward(CopyThreadCfg.Hangup[i].Type, CopyThreadCfg.Hangup[i].Id,
 
					(CopyThreadCfg.Hangup[i].Count * hours*Data.GetPatrolEarningsAttr())/100);
				ScrollViewList.Add(
					CellItemBaseViewModel.Create(AwardItem, ECellItemSizeType.Size150X135));
			}
			
			if (CopyThreadCfg.Special is {Count: > 0})
				for (int i = 0; i < CopyThreadCfg.Special.Count; i++)
				{
					var maxnums = Math.Min(CopyThreadCfg.Special[i].Count *hours , CopyThreadCfg.SpecialLimit[i]);
					var AwardItem = new Reward(CopyThreadCfg.Special[i].Type,
						CopyThreadCfg.Special[i].Id, maxnums);
					ScrollViewList.Add(
						CellItemBaseViewModel.Create(AwardItem, ECellItemSizeType.Size150X135));
				}
			
			if (CopyThreadCfg.HeroEquip is {Count: > 0})
				for (int i = 0; i < CopyThreadCfg.HeroEquip.Count; i++)
				{
					var maxnums = CopyThreadCfg.HeroEquip[i].Count *
					              Math.Min(hours, CopyThreadCfg.HeroEquipLimit[0]);
					var AwardItem = new Reward(CopyThreadCfg.HeroEquip[i].Type,
						CopyThreadCfg.HeroEquip[i].Id, maxnums);
					ScrollViewList.Add(
						CellItemBaseViewModel.Create(AwardItem, ECellItemSizeType.Size150X135));
				}
		}

		void InitButs()
		{
			// var adNums = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.PatrolIncome_06)
			// 	.Content[0];
			AdLastNumsStr = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips24) +
			                Data.AdFreeCount;// + "/" + adNums;
			
			AdButGray = Data.AdFreeCount <= 0  || !UIHelper.CheckRewardIsEnough(new Reward(RewardType.Lives,(int)GameConst.ItemIdCode.EnergyDrink,PowerCos));

			//var powerNums = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.PatrolIncome_03).Content[0] + Data.MonthAddPatrolNums();
			GoldLastNumsStr = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips24) +
			                  Data.FreeCount;// + "/" + powerNums;
			PowerButGray = Data.FreeCount <= 0 || !UIHelper.CheckRewardIsEnough(new Reward(RewardType.Lives,(int)GameConst.ItemIdCode.EnergyDrink,PowerCos));
		}

		[Command]
        private void OnClickClose()
        {
	        UIManager.Instance.CloseDialog<PatrolFastRewardView>();
        }

        /// <summary>
        /// 广告免费
        /// </summary>
        [Command]
        private async void OnClickFastBtn()
        {
	        if (Data.AdFreeCount <= 0 || 
	            !UIHelper.CheckRewardIsEnough(new Reward(RewardType.Lives,(int)GameConst.ItemIdCode.EnergyDrink,PowerCos),isJump:true))return;

	        //广告抽奖
	        UIHelper.ShowRewardAds(() =>
	        {
		        GetCallback(3);
	        });
	    }

        
        [Command]
        private void OnClickGettingBtn()
        {
	        if (Data.FreeCount <= 0 ||
	            !UIHelper.CheckRewardIsEnough(new Reward(RewardType.Lives,(int)GameConst.ItemIdCode.EnergyDrink,PowerCos),isJump:true))return;
	        GetCallback(2);
        }
        
        private async void GetCallback(int op)
        {
	        var req = new ReqMainHangupClaim();
	        req.Op = op;
	        var result = await GameNetworkManager.Instance.SendAsync<RspMainHangupClaim>(req);
	        if (NetHelper.CheckNetErrorMessage(result.rsp, true))
	        {
		        Lodash.DealRewards(result.rsp.Reward.ToList());
		        Lodash.DealRewards(result.rsp.Cost.ToList(),false);
		        if (op == 3) Data.AdFreeCount--;
		        if (op == 2) Data.FreeCount--;
		        UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList());
		        InitButs();
	        }
        }
    }
}