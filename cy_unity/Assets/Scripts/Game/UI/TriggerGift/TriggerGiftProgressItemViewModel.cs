using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DHFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class TriggerGiftProgressItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private string progressTextStr;
		[AutoNotify] private float progressValue;
        private StageRewardCfg cfg;
        [AutoNotify] private SelectCellItemEffectViewModel selectCellItemEffectVm;
        [Preserve]
        public TriggerGiftProgressItemViewModel(StageRewardCfg cfg)
        {
            this.cfg = cfg;
            InitUI();
            InitCellItem();
            DataCenter.triggerGiftData.PropertyChanged += DataChanged;
        }
        private void DataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName==nameof(DataCenter.triggerGiftData.OptionalRecord))
            {
                InitCellItem();
            }
        }
        
        protected override void OnDispose()
        {
            base.OnDispose();
            DataCenter.triggerGiftData.PropertyChanged -= DataChanged;
        }
        
        private void InitUI()
        {
            progressTextStr = cfg.Level.ToString();
            if (DataCenter.triggerGiftData.BuyDay >= cfg.Level)
            {
                ProgressValue = 1f;
            }
            else
            {
                var lastLevel = DataCenter.triggerGiftData.GetLastLevel(cfg.Id);
                var allValue = cfg.Level - lastLevel;
                var value = DataCenter.triggerGiftData.BuyDay - lastLevel;
                ProgressValue = value*1f / allValue*1f;
            }
        }

        #region 自选

        protected void InitCellItem()
        {
            if(cfg==null) return;
            if (cfg.OptionalReward is { Count: > 0 })
            {
                SelectCellItemEffectVm = new SelectCellItemEffectViewModel(cfg.OptionalReward,DataCenter.triggerGiftData.GetSelectPacket(cfg.Id),ECellItemSizeType.Size100X90);
                SelectCellItemEffectVm.ClickEvent = OnClickSelectReward;
                var state = GetState();
                SelectCellItemEffectVm.CellItemBaseViewVm.State = state;
                SelectCellItemEffectVm.CellItemBaseViewVm.SetClickAction(state == ECellItemState.Finish? null: OnClickSelectReward);
            }
            else if(cfg.Reward is { Count: > 0 })
            {
                SelectCellItemEffectVm = SelectCellItemEffectViewModel.Create(cfg.Reward[0],ECellItemSizeType.Size100X90);
                var state = GetState();
                SelectCellItemEffectVm.CellItemBaseViewVm.State = state;
                SelectCellItemEffectVm.CellItemBaseViewVm.SetClickAction((state is ECellItemState.Finish or ECellItemState.None)? null: OnClickSelectReward);
            }

            SelectCellItemEffectVm.CellItemBaseViewVm.CellItemBaseViewVm.IsOpenMask = true;

        }
        private void OnClickSelectReward(Tuple<Vector3, Vector3> info)
        {
            OnClickSelectReward();
        }

        private void OnClickSelectReward()
        {
            if(GetState()==ECellItemState.Finish) return;
            if (GetState()==ECellItemState.None || (cfg.OptionalReward is { Count: > 0 } && !DataCenter.triggerGiftData.CheckSelectPacket(cfg.Id)))
            {
                if(cfg.OptionalReward!=null && cfg.OptionalReward.Count>0)
                    UIManager.Instance.OpenDialog<CommonSelectRewardView>(new CommonSelectRewardViewModel(cfg.OptionalReward,
                        DataCenter.triggerGiftData.GetSelectPacket(cfg.Id),(selectIndex)=>
                    {
                        if (selectIndex >= 0)
                        {
                            DataCenter.triggerGiftData.SetSelectPacket(cfg.Id,selectIndex);
                        }
                    })).Forget();
                return;
            }
            
            if (GetState()==ECellItemState.GetIng)
            {
                GetAward();
            }
        }

        ECellItemState GetState()
        {
            if (DataCenter.triggerGiftData.IsGetProgressAward(cfg.Id))
            {
                return ECellItemState.Finish;
            }
            if (DataCenter.triggerGiftData.BuyDay >= cfg.Level)
            {
                return ECellItemState.GetIng;
            }
            return ECellItemState.None;
        }

        #endregion


        private async void GetAward()
        {
            if (GetState() != ECellItemState.GetIng)return;
            if (cfg.OptionalReward is { Count: > 0 } && !DataCenter.triggerGiftData.CheckSelectPacket(cfg.Id))
            {
                ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.Trigger08));
                return;
            }
            var data = new ReqTriggerGiftClaim();
            data.Id = cfg.Id;
            data.OptionalIndex = DataCenter.triggerGiftData.GetSelectPacket(cfg.Id);
            var result = await GameNetworkManager.Instance.SendAsync<RspTriggerGiftClaim>(data);
            NetHelper.CheckNetErrorMessage(result.rsp, true, () =>
            {
                Lodash.DealRewards(result.rsp.Reward.ToList());
                UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList());
                DataCenter.triggerGiftData.GetProgressAward(cfg.Id);
                InitCellItem();
            });
        }


    }
}