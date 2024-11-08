using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.Data;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using Game.UI.MainUi;


namespace DH.Game.ViewModels
{
    public partial class BoosterPackViewModel : ViewModelBase
    {
		[AutoNotify] private string timesDesStr;
		[AutoNotify] private int scrollViewPos;
	    [AutoNotify] private ObservableList<BoosterPackItemViewModel> boosterPackItemList = new();

	    private int curTriggerType;
	    private TriggerGiftOneData curTriggerData;
        [Preserve]
        public BoosterPackViewModel(int triggerType)
        {
	        curTriggerType = triggerType;
	        InitPanel();
        }

        private void InitPanel()
        {
	        if (DataCenter.triggerGiftData.Data.ContainsKey(curTriggerType))
	        {
		        curTriggerData = DataCenter.triggerGiftData.Data[curTriggerType];
	        }
	        var items = ConfigCenter.TriggerGiftCfgColl.GetDataByType(curTriggerType);
	        for (int i = 0; i < items.Count; i++)
	        {
		        var cfg = items[i];
		        BoosterPackItemViewModel tempVm = new BoosterPackItemViewModel(cfg,OnClickItemCallback);
		        tempVm.IsShowArrowNode = i != items.Count - 1;
		        boosterPackItemList.Add(tempVm);
	        }
	        UpdateTimesDes();
	        UpdatePanel();
        }

        [Command]
        private void OnClickCloseBtn()
        {
	        UIManager.Instance.CloseDialog<BoosterPackView>();
        }
		public void UpdateTimesDes()
		{
			if(curTriggerData == null) return;
			var leftTime = ServerTime.Instance.RemainTime(curTriggerData.EndStamp);
			if (leftTime < 0)
			{
				TimesDesStr =  string.Empty;
				MainUiManager.Instance.RemoveRightBut(MainStageInfoNodeRightButType.BoosterPack);
				OnClickCloseBtn();
				return;
			}
			if (leftTime >= 86400)
			{
				TimesDesStr =   UIHelper.ConvertTimeSecondToString(leftTime, ETimeFormatType.TimeFormatTypeHourMinuteWithUnit);
				return;
			}
			TimesDesStr =   ServerTime.Instance.Seconds2Hhmmss(leftTime);
		}

		private float interval;
		public override void Update()
		{
			if (UIHelper.CalculateTime(ref interval))
			{
				UpdateTimesDes();
			}
		}
		protected override void OnDispose()
		{
			foreach (var item in boosterPackItemList)
			{
				item.Dispose();
			}
			base.OnDispose();
		}

		private  void OnClickItemCallback(TriggerGiftCfg cfg)
		{
			
			if (cfg.Package == -1)
			{
				OnCellFree(cfg);
			}
			else
			{
				ShopManager.Instance.SendBuyPackageBuyRecharge(cfg.Package, null, 0, 
					DataCenter.triggerGiftData.GetSelectPacket(cfg.Id), 1,
					(rewardList, costList) =>
					{
						Lodash.DealRewards(rewardList,costList);
						UIHelper.OpenCommonRewardView(rewardList);
						DataCenter.triggerGiftData.BuyTriggerGift(cfg.Id);
						UpdatePanel();
					});
			}
		}

		private async void OnCellFree(TriggerGiftCfg cfg)
		{
			var data = new ReqTriggerGiftConditionBuy();
			data.Id = cfg.Id;
			var result = await GameNetworkManager.Instance.SendAsync<RspTriggerGiftConditionBuy>(data);
			NetHelper.CheckNetErrorMessage(result.rsp, true, () =>
			{
				UIHelper.OpenCommonRewardView(result.rsp.Rewards.ToList());
				Lodash.DealRewards(result.rsp.Rewards.ToList());
				DataCenter.triggerGiftData.BuyTriggerGift(cfg.Id);
				
				UpdatePanel();
			});
		}

		private async void UpdatePanel()
		{
			await UniTask.Delay(200);
			var index = -1;
			bool isShowBtn = false;
			for (int i = 0; i < boosterPackItemList.Count; i++)
			{
				var item = boosterPackItemList[i];
				item.UpdatePanel();
				if (item.CurState == EBoosterPackState.CanOp)
				{
					index = i;
				}

				if (!isShowBtn && item.CurState != EBoosterPackState.SoldOut)
				{
					isShowBtn = true;
				}
			}
			
			ScrollViewPos = index;
			if (!isShowBtn)
			{
				MainUiManager.Instance.RemoveRightBut(MainStageInfoNodeRightButType.BoosterPack);
				OnClickCloseBtn();
			}
		}
    }
}