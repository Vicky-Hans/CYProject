using System;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System.Collections.Specialized;
using System.Linq;
using DH.Config;
using DH.Data;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class MagicBingoTaskItemViewModel : ViewModelBase
    {
  
		[AutoNotify] private string titleStr;
		[AutoNotify] private string timeStr;
		[AutoNotify] private float expSlideValue;
	    [AutoNotify] private  LuckEggTaskItemState state;
	    [AutoNotify] private string progressDesc;
	    [AutoNotify] private bool isAdv;
	    public CommonAdvIconViewModel CommonAdvIconVm;
	    private ActivityTaskCfg cfg;
	    public MagicBingoData Data => DataCenter.mgicBingoData;
	    public Func<object, object> GetItemGridCellCallback => GetItemGridCellCallbackByIndex;
	    [AutoNotify] private ObservableDictionary<int,CellItemViewModel> itemGridDictionary = new();
        [Preserve]
        public MagicBingoTaskItemViewModel(ActivityTaskCfg cfg)
        {
	        this.cfg = cfg;
	        IsAdv = cfg.EventType == 8;//看广告
	        InitUI();	
	        RefreshState();
	        CommonAdvIconVm = new CommonAdvIconViewModel();
	        Data.TaskClaimed.CollectionChanged += TaskChanged;
	        Data.TaskDailyClaimed.CollectionChanged += TaskChanged;
	        Data.TaskProgress.CollectionChanged += TaskChanged;
	        Data.TaskDailyProgress.CollectionChanged += TaskChanged;
	        RefreshTimeDesc();
        }
        protected override void OnDispose()
        {
	        base.OnDispose();
	        CommonAdvIconVm?.Dispose();
	        Data.TaskClaimed.CollectionChanged -= TaskChanged;
	        Data.TaskProgress.CollectionChanged -= TaskChanged;
	        Data.TaskDailyClaimed.CollectionChanged -= TaskChanged;
	        Data.TaskDailyProgress.CollectionChanged -= TaskChanged;
        }
        private void InitUI()
        {
	        TitleStr = Data.GetTaskDesc(cfg);
	        ItemGridDictionary.Clear();
	        for (int i = 0; i < cfg.Reward.Count; i++)
	        {
		        var vm = CellItemViewModel.Create(cfg.Reward[i], ECellItemSizeType.Size117X76);
		        vm.CellItemBaseViewVm.IsOpenMask = true;
		        ItemGridDictionary.Add(i,vm);
		        

	        }
        }
        private void TaskChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
	        RefreshState();
        }
        
        private void RefreshState()
        {
	        var all = cfg.EventLoad;
	        var value =  Data.GetTaskProgress(cfg.Id,cfg.EventCondi);
	        ProgressDesc = $"{value}/{all}";
	        ExpSlideValue = (float)value / all;
	        State = Data.GetTaskState(cfg.Id,cfg.TypeCondi);
	        RefreshRewardState();
        }
        
        public void RefreshRewardState()
        {
	        foreach (var item in ItemGridDictionary)
	        {
		        item.Value.State = State switch
		        {
			        LuckEggTaskItemState.Finish => ECellItemState.Finish,
			        LuckEggTaskItemState.NotGetAward => ECellItemState.GetIng,
			        _ => ECellItemState.None,
		        };
	        }
        }
        [Command]
        private void OnClickGoBtn()
        {
	        JumpManager.Instance.Jump(cfg.TaskList);
        }

        [Command]
        private async void OnClickGetBtn()
        {
	        if(Data.IsTimeOver()) return;
	        if(state != LuckEggTaskItemState.NotGetAward) return;
	        var req = new ReqBingoTaskClaim()
	        {
		        Id = cfg.Id,
	        };
	        var result =await GameNetworkManager.Instance.SendAsync<RspBingoTaskClaim>(req);
	        if (NetHelper.CheckNetErrorMessage(result.rsp, true))
	        {
		        Lodash.DealRewards(result.rsp.Rewards.ToList());
		        Data.SetTaskState(cfg.Id,cfg.EventCondi);
		        UIHelper.OpenCommonRewardView(result.rsp.Rewards.ToList());
	        }
        }
        [Command]
        private async void OnClickAdvBtn()
        {
	        if(Data.IsTimeOver()) return;
	        if (Data.IsFinishTask(cfg.Id,cfg.TypeCondi))return;
	        UIHelper.ShowRewardAds(null, () =>
	        {
		        State = LuckEggTaskItemState.NotGetAward;
		        OnClickGetBtn();
	        });

        }
        
        
        private float interval;
        private void RefreshTimeDesc()
        {
	        TimeStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips14) + ServerTime.Instance.SecondsDHms(ServerTime.Instance.SecondDaySeconds());
           
        }
        public override void Update()
        {
	        if (UIHelper.CalculateTime(ref interval))
	        {
		        RefreshTimeDesc();
	        }
        }
        private object GetItemGridCellCallbackByIndex(object index)
        {
	        if (itemGridDictionary.TryGetValue((int)index, out CellItemViewModel ret))
	        {
		        return ret;
	        }

	        return null;
        }
    }
}