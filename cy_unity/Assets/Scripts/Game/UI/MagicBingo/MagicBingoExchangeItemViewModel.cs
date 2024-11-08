using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class MagicBingoExchangeItemViewModel : ViewModelBase
    {
	    public override bool AutoDispose => true;
		[AutoNotify] private string limitNumStr;
	    [AutoNotify] private ObservableList<SelectCellItemViewModel> awardScrollviewList = new();
	    private ExchangeShopCfg cfg;
	    private MagicBingoData Data => DataCenter.mgicBingoData;
	    
	    [AutoNotify] private bool isBuyOver;
	    [AutoNotify] private CellItemBaseViewModel cellItemBaseVm;
        [Preserve]
        public MagicBingoExchangeItemViewModel(ExchangeShopCfg cfg)
        {
	        this.cfg = cfg;
	        InitRewardList();
	        InitUI();
	        Data.PropertyChanged += OptionalChange;
	        PlayerInfoManager.Instance.PropertyChanged  += OptionalChange;
        }
        protected override void OnDispose()
        {
	        Data.PropertyChanged -= OptionalChange;
	        PlayerInfoManager.Instance.PropertyChanged  -= OptionalChange;
	        foreach (var item in awardScrollviewList)
	        {
		        item.Dispose();
	        }
	        cellItemBaseVm?.Dispose();
	        base.OnDispose();
        }
        #region 奖励
        private void InitRewardList()
        {
	        AwardScrollviewList.Clear();
	        
	        if (cfg.Reward is { Count: > 0 })
	        {
		        for (int i = 0; i<cfg.Reward.Count ; i++)
		        {
			        var reward = cfg.Reward[i];
			        var selectItemViewModel = SelectCellItemViewModel.Create(reward,ECellItemSizeType.Size132X132);
			        AwardScrollviewList.Add(selectItemViewModel);
		        }
	        }
	        if (cfg.OptionalReward is { Count: > 0 })
	        {
		        var selectItemViewModel = SelectCellItemViewModel.Create(cfg.OptionalReward,ECellItemSizeType.Size132X132,selectIndex:Data.GetOptionalSelectIndex(cfg.Id,2));
		        selectItemViewModel.ClickEvent = OnClickSelectReward;
		        selectItemViewModel.IsButEnabled = Data.IsCanExchange(cfg.Id);
		        AwardScrollviewList.Add(selectItemViewModel);
	        }
	        
        }
        private void OnClickSelectReward()  
        {
	        if (!Data.IsCanExchange(cfg.Id))return;
	        if(cfg==null || cfg.OptionalReward ==null || cfg.OptionalReward.Count==0) return;
	        UIManager.Instance.OpenDialog<CommonSelectRewardView>(new CommonSelectRewardViewModel(cfg.OptionalReward,Data.GetOptionalSelectIndex(cfg.Id,2)
		        ,selectEventCb)).Forget();
        }
        
        private async void selectEventCb(int selectIndex)
        {
	        Data.SetOptionalSelectIndex(cfg.Id,selectIndex,2);
        }
        
        private void OptionalChange(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName is nameof(Data.OptionalRecord) or nameof(PlayerInfoManager.Instance.SecondDay))
	        {
		        InitRewardList();
	        }
        }
        
        #endregion

        #region UI

        private void InitUI()
        {
	        LimitNumStr = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips24) +$"{cfg.BuyLimit - Data.GetExchangeNums(cfg.Id)}/{cfg.BuyLimit}";
	        IsBuyOver = Data.GetExchangeNums(cfg.Id) >= cfg.BuyLimit;
	        CellItemBaseVm =  CellItemBaseViewModel.Create(cfg.Consumption[0],ECellItemSizeType.Size132X132);
        }
        
        #endregion
        
        [Command]
        private async void OnClickBtn()
        {
	        if (!Data.IsCanExchange(cfg.Id)) return;
	        
	        if (!UIHelper.CheckRewardIsEnough(cfg.Consumption,isJump:true))return;
	        
	        if (cfg.OptionalReward is { Count: > 0 } && !Data.IsSelectOptionalReward(cfg.Id,2))
	        {
		        ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.Trigger08));
		        return;
	        }
	        
	        var req = new ReqBingoExchange()
	        {
		        Id = cfg.Id,
		        OptionalIndex = Data.GetOptionalSelectIndex(cfg.Id,2)
	        };
	        var result = await GameNetworkManager.Instance.SendAsync<RspBingoExchange>(req);
	        if (NetHelper.CheckNetErrorMessage(result.rsp, true))
	        {
				Lodash.DealRewards(result.rsp.Rewards.ToList());
				Lodash.DealRewards(result.rsp.Cost.ToList(),false);
				UIHelper.OpenCommonRewardView(result.rsp.Rewards.ToList());
				Data.Exchange(cfg.Id);
				InitUI();
	        }
        }
        
    }
}