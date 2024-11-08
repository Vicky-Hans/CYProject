using System;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.ViewModels;
using UnityEngine;
using UnityEngine.Scripting;


namespace DH.Game.ViewModels
{
    public partial class MagicBingoProgressItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private string progressTextStr;
		[AutoNotify] private float progressValue;
        private StageRewardCfg cfg;
        [AutoNotify] private SelectCellItemEffectViewModel selectCellItemEffectVm;
        [AutoNotify] private bool showEffect;
        [Preserve]
        public MagicBingoProgressItemViewModel(StageRewardCfg cfg)
        {
            this.cfg = cfg;
            InitUI();
            InitCellItem();
            DataCenter.mgicBingoData.PropertyChanged += DataChanged;
        }
        private void DataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(DataCenter.mgicBingoData.OptionalRecord) 
                or nameof(DataCenter.mgicBingoData.Grid.StageClaimed))
            {
                InitCellItem();
            }
        }
        
        protected override void OnDispose()
        {
            base.OnDispose();
            DataCenter.mgicBingoData.PropertyChanged -= DataChanged;
        }
        
        private void InitUI()
        {
            progressTextStr = cfg.Level.ToString();
            if (DataCenter.mgicBingoData.Grid.BingoCount >= cfg.Level)
            {
                ProgressValue = 1f;
            }
            else
            {
                var lastLevel = DataCenter.mgicBingoData.GetLastLevel(cfg.Id);
                var allValue = cfg.Level - lastLevel;
                var value = DataCenter.mgicBingoData.Grid.BingoCount - lastLevel;
                ProgressValue = value*1f / allValue*1f;
            }
        }

        #region 自选

        protected void InitCellItem()
        {
            if(cfg==null) return;
            if (cfg.OptionalReward is { Count: > 0 })
            {
                SelectCellItemEffectVm = new SelectCellItemEffectViewModel(cfg.OptionalReward,DataCenter.mgicBingoData.GetOptionalSelectIndex(cfg.Id,1),
                    ECellItemSizeType.Size120X100);
                SelectCellItemEffectVm.ClickEvent = OnClickSelectReward;
                var state = GetState();
                SelectCellItemEffectVm.CellItemBaseViewVm.State = state == ECellItemState.GetIng ? ECellItemState.None : state;
                SelectCellItemEffectVm.CellItemBaseViewVm.SetClickAction(state == ECellItemState.Finish? null: OnClickSelectReward);
            }
            else if(cfg.Reward is { Count: > 0 })
            {
                SelectCellItemEffectVm = SelectCellItemEffectViewModel.Create(cfg.Reward[0],ECellItemSizeType.Size120X100);
                var state = GetState();
                SelectCellItemEffectVm.CellItemBaseViewVm.State = state == ECellItemState.GetIng ? ECellItemState.None : state;
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
            if (GetState()==ECellItemState.None || (cfg.OptionalReward is { Count: > 0 } && !DataCenter.mgicBingoData.IsSelectOptionalReward(cfg.Id,1)))
            {
                if(cfg.OptionalReward!=null && cfg.OptionalReward.Count>0)
                    UIManager.Instance.OpenDialog<CommonSelectRewardView>(new CommonSelectRewardViewModel(cfg.OptionalReward,
                        DataCenter.mgicBingoData.GetOptionalSelectIndex(cfg.Id,1),(selectIndex)=>
                    {
                        if (selectIndex >= 0)
                        {
                            DataCenter.mgicBingoData.SetOptionalSelectIndex(cfg.Id,selectIndex);
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
            ShowEffect = false;
            if (DataCenter.mgicBingoData.IsGetProgressAward(cfg.Id))
            {
                return ECellItemState.Finish;
            }
            if (DataCenter.mgicBingoData.Grid.BingoCount >= cfg.Level)
            {
                ShowEffect = true;
                return ECellItemState.GetIng;
            }
            return ECellItemState.None;
        }

        #endregion


        private async void GetAward()
        {
            if (GetState() != ECellItemState.GetIng)return;
            if (cfg.OptionalReward is { Count: > 0 } && !DataCenter.mgicBingoData.IsSelectOptionalReward(cfg.Id))
            {
                ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.Trigger08));
                return;
            }
            var data = new ReqBingoClaim();
            // data.Id = cfg.Id;
            // data.OptionalIndex = DataCenter.mgicBingoData.GetOptionalSelectIndex(cfg.Id);
            var result = await GameNetworkManager.Instance.SendAsync<RspBingoClaim>(data);
            NetHelper.CheckNetErrorMessage(result.rsp, true, () =>
            {
                Lodash.DealRewards(result.rsp.Reward.ToList());
                UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList());
                DataCenter.mgicBingoData.GetProgressAward();
            });
        }


    }
}