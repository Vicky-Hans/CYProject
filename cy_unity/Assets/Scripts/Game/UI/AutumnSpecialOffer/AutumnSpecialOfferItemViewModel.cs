using System;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class AutumnSpecialOfferItemViewModel : ViewModelBase
    {
	    public override bool AutoDispose => true;
		[AutoNotify] private string titleStr;
		[AutoNotify] private bool isShowAdBtn;
		[AutoNotify] private bool isBuyOver;
		[AutoNotify] private string limitBuyStr;
	    [AutoNotify] private ObservableList<SelectCellItemEffectViewModel> awardScrollviewList = new();
	    public CommonAdvIconViewModel CommonAdvIconVm;
	    public BtnPriceNodeModel BtnPriceNodeModel;
	    private PackageCfg cfg;
	    private int Id => cfg.Id;
	    private AutumnSpecialData Data => DataCenter.autumnSpecialData;
        [Preserve]
        public AutumnSpecialOfferItemViewModel(PackageCfg cfg)
        {
	        this.cfg = cfg;
	        CommonAdvIconVm = new CommonAdvIconViewModel();
	        
	        var packageCfg = ConfigCenter.PackageCfgColl.GetDataById(Id);
	        var cfgInfo = ConfigCenter.RechargeCfgColl.GetDataById(packageCfg.RechargeId);
	        string priceStr = "";
	        if (cfgInfo != null) priceStr = PayController.Instance.GetLocalizePrice(cfgInfo.PayId);
	        BtnPriceNodeModel = new BtnPriceNodeModel(priceStr);
	        
	        InitUI();
	        InitRewardList();
	        Data.PropertyChanged += OnPropertyChanged;
        }
        protected override void OnDispose()
        {
	        Data.PropertyChanged -= OnPropertyChanged;
	        base.OnDispose();
        }
        
        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(Data.OptionalRecord))
	        {
		        InitRewardList();
	        }
        }
        
        private void InitUI()
        {
	        var languageCfg = ConfigCenter.PackageLanguageCfgColl.GetDataById(Id);
	        TitleStr = languageCfg.Value;
	        LimitBuyStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips18,cfg.BuyLimit - Data.GetBuyNum(Id),cfg.BuyLimit);
	        IsBuyOver = !Data.IsCanGetAward(Id);
        }
        
        
        #region 自选相关
        
        private void InitRewardList()
        {
	        AwardScrollviewList.Clear();
	        
	        if (cfg.OptionalReward is { Count: > 0 })
	        {
		        var selectItemViewModel = SelectCellItemEffectViewModel.Create(cfg.OptionalReward,ECellItemSizeType.Size117X76,selectIndex:Data.GetSelectPacket(cfg.Id));
		        selectItemViewModel.ClickEvent = OnClickSelectReward;
		        var state = Data.IsCanGetAward(Id)?ECellItemState.None : ECellItemState.Finish;
		        selectItemViewModel.CellItemBaseViewVm.State = state;
		        selectItemViewModel.CellItemBaseViewVm.SetClickAction(state == ECellItemState.Finish? null: OnClickSelectReward);
		        
		        selectItemViewModel.CellItemBaseViewVm.CellItemBaseViewVm .IsOpenMask = true;
		        AwardScrollviewList.Add(selectItemViewModel);
	        }

	        if (cfg.Reward is { Count: > 0 })
	        {
		        for (int i = 0; i<cfg.Reward.Count ; i++)
		        {
			        var reward = cfg.Reward[i];
			        var selectItemViewModel = SelectCellItemEffectViewModel.Create(reward,ECellItemSizeType.Size117X76);
			        selectItemViewModel.CellItemBaseViewVm.State = Data.IsCanGetAward(Id)?ECellItemState.None : ECellItemState.Finish;
			        selectItemViewModel.CellItemBaseViewVm.CellItemBaseViewVm .IsOpenMask = true;
			        AwardScrollviewList.Add(selectItemViewModel);
		        }
	        }
	        
        }
        private void OnClickSelectReward(Tuple<Vector3, Vector3> info)
        {
	        OnClickSelectReward();
        }
        private void OnClickSelectReward()  
        {
	        if (!Data.IsCanGetAward(Id))return;
	        if(cfg==null || cfg.OptionalReward ==null || cfg.OptionalReward.Count==0) return;
	        UIManager.Instance.OpenDialog<CommonSelectRewardView>(new CommonSelectRewardViewModel(cfg.OptionalReward,Data.GetSelectPacket(cfg.Id)
		        ,(selectIndex)=> {
			        Data.SetSelectPacket(cfg.Id,selectIndex);
		        })).Forget();
        }
        

        #endregion



        [Command]
        private void OnClickBuyBtn()
        {
	        if (!Data.IsCanGetAward(Id))return;
	        if (cfg.OptionalReward is { Count: > 0 } && !Data.CheckSelectPacket(cfg.Id))
	        {
		        ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.Trigger08));
		        return;
	        }
	        ShopManager.Instance.SendBuyPackageBuyRecharge(Id, null, 0, Data.GetSelectPacket(Id), 1,
		        (rewardList, costList) =>
		        {
			        Lodash.DealRewards(rewardList,costList);
			        UIHelper.OpenCommonRewardView(rewardList);
			        Data.GetAward(Id);
		        });
        }

        [Command]
        private void OnClickAdvBtn()
        {
	        
        }

        
    }
}