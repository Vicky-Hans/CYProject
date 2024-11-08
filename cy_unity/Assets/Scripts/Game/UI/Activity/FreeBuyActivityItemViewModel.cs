using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine.SocialPlatforms;


namespace DH.Game.ViewModels
{
    public partial class FreeBuyActivityItemViewModel : ViewModelBase
    {
	    public override bool AutoDispose => true;
	    [AutoNotify] private PurchaseRewardCfg cfg;
	    [AutoNotify] private PackageCfg packageCfg;
		[AutoNotify] private string bgPath;
		[AutoNotify] private string descStr;
		public Func<object, object> GetGridCellCallback => GetGridCellCallbackByIndex;
		[AutoNotify] private ObservableDictionary<int,CellItemViewModel> gridDictionary = new();

        [Preserve]
        public FreeBuyActivityItemViewModel(PackageCfg packageCfg,PurchaseRewardCfg cfg)
        {
	        BgPath = cfg == null ? "purchase[purchase_panel_3]" : "school[school_panel_12]";
	        Cfg = cfg;
	        PackageCfg = packageCfg;
	        if (Cfg != null)
	        {
		        for (int i = 0; i < Cfg.Reward.Count; i++)
		        {
			        gridDictionary.Add(i,CellItemViewModel.Create(Cfg.Reward[i]));
		        }
	        }
	        else if(PackageCfg!=null)
	        {
		        for (int i = 0; i < PackageCfg.Reward.Count; i++)
		        {
			        gridDictionary.Add(i,CellItemViewModel.Create(PackageCfg.Reward[i]));
		        }
	        }

	        RefreshState();
	        DataCenter.giftPackData.ZeroClaim.CollectionChanged += DataCollectionChanged;
	        DataCenter.giftPackData.PropertyChanged += DataPropertyChanged;
        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        DataCenter.giftPackData.ZeroClaim.CollectionChanged -= DataCollectionChanged;
	        DataCenter.giftPackData.PropertyChanged -= DataPropertyChanged;
        }

        private void DataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
	        RefreshState();
        }

        private void DataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
	        RefreshState();
        }

        private void RefreshState()
        {
	        var isBuy = DataCenter.giftPackData.IsBuyGift();
	        if (Cfg == null)
	        {
		        foreach (var item in gridDictionary)
		        {
			        item.Value.State = isBuy ? ECellItemState.Finish : ECellItemState.None;
			        item.Value.IsShowLock = !isBuy;
		        }

		        DescStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Oyuanpurchase_03);
	        }
	        else
	        {
		        var isFinish = DataCenter.giftPackData.IsGetReward(Cfg.Id);
		        var isGeting =!ServerTime.Instance.IsOpenTime(DataCenter.giftPackData.ZeroBuy + Cfg.Time * 3600);

		        var temp = LocalizeHelper.GetGlobal(GlobalLanguageId.Oyuanpurchase_04, Cfg.Time);
		        if (Cfg.Time >= 24)
		        {
			        temp = LocalizeHelper.GetGlobal(GlobalLanguageId.Oyuanpurchase_05, Cfg.Time / 24);
		        }

		        foreach (var item in gridDictionary)
		        {
			        if (isBuy)
			        {
				        if (isFinish)
				        {
					        item.Value.State = ECellItemState.Finish;
					        item.Value.IsShowLock = false;
					        DescStr = $"{temp}:{LocalizeHelper.GetGlobal(GlobalLanguageId.Oyuanpurchase_08)}";//" $"购买后{Cfg.Time}小时可以领取:已领取";
					        item.Value.SetClickAction(null);
				        }
				        else
				        {
					        item.Value.State =isGeting ? ECellItemState.GetIng:ECellItemState.None;
					        item.Value.IsShowLock = !isGeting;
					        DescStr = isGeting?$"{temp}:{LocalizeHelper.GetGlobal(GlobalLanguageId.Oyuanpurchase_07)}":$"{LocalizeHelper.GetGlobal(GlobalLanguageId.Oyuanpurchase_06)}{UIHelper.GetRefreshDayTime(DataCenter.giftPackData.ZeroBuy+Cfg.Time * 3600)}";
					        item.Value.SetClickAction(isGeting?(info) =>
					        {
						        ClickGetClaim();
					        }:null);
				        }
			        }
			        else
			        {
				        item.Value.State = ECellItemState.None;
				        item.Value.IsShowLock = true;
				        DescStr = temp;
				        item.Value.SetClickAction(null);
			        }
		        }
	        }
        }

        public void ClickGetClaim()
        {
	        if (Cfg != null)
	        {
		        ActivityUIManager.Instance.SendGiftPackZeroClaim(Cfg.Id, -1,() =>
		        {
			        if (ActivityUIManager.Instance.CheckAllGetReward())
			        {
				        UIManager.Instance.CloseDialog<FreeBuyActivityView>();
			        }
		        }).Forget();
	        }
        }


        private object GetGridCellCallbackByIndex(object index)
		{
			if (gridDictionary != null && gridDictionary.TryGetValue((int)index, out var value))
			{
				return value;
			}
			return null;
		}

		private float delay;
		public override void Update()
		{
			base.Update();
			if(!UIHelper.CalculateTime(ref delay)) return;
			RefreshState();
		}
    }
}