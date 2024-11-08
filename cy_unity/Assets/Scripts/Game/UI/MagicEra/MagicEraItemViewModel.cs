using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;


namespace DH.Game.ViewModels
{
	public partial class MagicEraItemViewModel : ViewModelBase
    {
        
	    [AutoNotify] private ObservableList<SelectCellItemEffectViewModel> awardScrollviewList = new();
	    private MagicAgeData Data => DataCenter.magicAgeData;
	    [AutoNotify] private BtnPriceNodeModel btnPriceVM;

	    [AutoNotify] private MagicEraItemState state;
	    
	    [AutoNotify] private string bgImg;
	    [AutoNotify] private string bgArrowBg;
	    [AutoNotify] private string bgArrow;
	    [AutoNotify] private string limitDes;
	    
	    [AutoNotify] private bool showArrowGo;
	    [AutoNotify] private bool isFree;
	    public AgeMagicPackageCfg cfg;
        [Preserve]
        public MagicEraItemViewModel(AgeMagicPackageCfg Cfg,bool isEnd)
        {
	        cfg = Cfg;

	        IsFree = cfg.PackageId == 0;
	        if (cfg.PackageId != 0)
	        {
		        var packageCfg = ConfigCenter.PackageCfgColl.GetDataById(Cfg.PackageId);
		        var cfgInfo = ConfigCenter.RechargeCfgColl.GetDataById(packageCfg.RechargeId);
		        string priceStr = "";
		        if (cfgInfo != null) priceStr = PayController.Instance.GetLocalizePrice(cfgInfo.PayId);
		        BtnPriceVM = new BtnPriceNodeModel(priceStr); 
	        }
	        
	        ShowArrowGo = !isEnd;
	        InitBg();
	        Data.PropertyChanged += OptionalChange;
        }
        
        
        private void InitRewardList()
        {
	        AwardScrollviewList.Clear();
	        
	        if (cfg.OptionalReward is { Count: > 0 })
	        {
		        var selectItemViewModel = SelectCellItemEffectViewModel.Create(cfg.OptionalReward,ECellItemSizeType.Size132X132,selectIndex:Data.GetSelectPacket(cfg.Id));
		        selectItemViewModel.ClickEvent = OnClickSelectReward;
		        var state = GetState();
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
			        var selectItemViewModel = SelectCellItemEffectViewModel.Create(reward,ECellItemSizeType.Size132X132);
			        selectItemViewModel.CellItemBaseViewVm.State = GetState();
			        selectItemViewModel.CellItemBaseViewVm.CellItemBaseViewVm .IsOpenMask = true;
			        AwardScrollviewList.Add(selectItemViewModel);
		        }
	        }
	        
        }
        private void OnClickSelectReward(Tuple<Vector3, Vector3> info)
        {
	        OnClickSelectReward();
        }
	    ECellItemState GetState()
        {
	        if (State == MagicEraItemState.Get)
	        {
		        return ECellItemState.Finish;
	        }
	        if (State == MagicEraItemState.NotGet && cfg.PackageId == 0)
	        {
		        return ECellItemState.GetIng;
	        }
	        return ECellItemState.None;
        }

        private void OnClickSelectReward()  
        {
	        if (Data.IsBuyOver())return;
	        if(cfg==null || cfg.OptionalReward ==null || cfg.OptionalReward.Count==0) return;
	        UIManager.Instance.OpenDialog<CommonSelectRewardView>(new CommonSelectRewardViewModel(cfg.OptionalReward,Data.GetSelectPacket(cfg.Id)
		        ,(selectIndex)=> {
			        Data.SetSelectPacket(cfg.Id,selectIndex);
		        })).Forget();
        }
        private void OptionalChange(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(Data.OptionalRecord))
	        {
		        InitRewardList();
	        }
	        if (e.PropertyName == nameof(Data.GetAwards))
	        {
		        InitBg();
	        }
        }
        
        private void InitBg()
        {
	        State = Data.GetState(cfg.Id);
	        LimitDes = LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips18, cfg.BuyLimit - Data.GetNums(cfg.Id), cfg.BuyLimit);
	        BgImg = State switch
	        {
		        MagicEraItemState.Lock => "magic[magic_panel_3]",
		        MagicEraItemState.NotGet => "magic[magic_panel_2]",
		        _ => "magic[magic_panel_4]",
	        };
	        BgArrowBg = State switch
	        {
		        MagicEraItemState.Lock => "magic[magic_panel_7]",
		        MagicEraItemState.NotGet => "magic[magic_panel_7]",
		        _ => "magic[magic_panel_8]",
	        };
	        BgArrow = State switch
	        {
		        MagicEraItemState.Lock => "magic[magic_panel_5]",
		        MagicEraItemState.NotGet => "magic[magic_panel_5]",
		        _ => "magic[magic_panel_6]",
	        };
	        InitRewardList();
        }

        [Command]
        private async void FreeOnClickBtn()
        {
	        if (State == MagicEraItemState.Lock)
	        {
		        
		        ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.AgeMagic_05));
		        return;
	        }
	        
	        if (State != MagicEraItemState.NotGet)return;
	        if (cfg.OptionalReward is { Count: > 0 } && !Data.CheckSelectPacket(cfg.Id))
	        {
		        ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.Trigger08));
		        return;
	        }
	        var data = new ReqMagicAgeClaim();
	        data.Id = cfg.Id;
	        var result = await GameNetworkManager.Instance.SendAsync<RspMagicAgeClaim>(data);
	        NetHelper.CheckNetErrorMessage(result.rsp, true, () =>
	        {
		        Lodash.DealRewards(result.rsp.Reward.ToList());
		        UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList());
		        Data.GetAward(cfg.Id);
	        });
        }

        [Command]
        private async void CosOnClickBtn()
        {
	        if (State == MagicEraItemState.Lock)
	        {
		        ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.AgeMagic_05));
		        return;
	        }
	        if (State == MagicEraItemState.Get)return;
	        if (cfg.OptionalReward is { Count: > 0 } && !Data.CheckSelectPacket(cfg.Id))
	        {
		        ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.Trigger08));
		        return;
	        }
	        ShopManager.Instance.SendBuyPackageBuyRecharge(cfg.PackageId, null, 0, Data.GetSelectPacket(cfg.Id), 1,
		        (rewardList, costList) =>
		        {
			        Lodash.DealRewards(rewardList,costList);
			        UIHelper.OpenCommonRewardView(rewardList);
			        Data.GetAward(cfg.Id);
		        });
        }
        
        protected override void OnDispose()
        {
	        Data.PropertyChanged -= OptionalChange;
	        btnPriceVM?.Dispose();
	        base.OnDispose();
        }
    }
}