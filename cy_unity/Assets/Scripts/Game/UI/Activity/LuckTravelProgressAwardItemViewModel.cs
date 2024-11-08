using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using Dh.Game.ViewModels;
using DH.Proto;
using DH.UIFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class LuckTravelProgressAwardItemViewModel : ViewModelBase
    {
        public override bool AutoDispose => true;
        [AutoNotify] private int scoreValue;
        [AutoNotify] private StageRewardCfg cfg;
        [AutoNotify] private float progressValue;

        [AutoNotify] private SelectCellItemEffectViewModel selectItemViewModel;
        [AutoNotify] private TaskState state;
        [AutoNotify] private bool isEndPos;
        
        [Preserve]
        public LuckTravelProgressAwardItemViewModel(StageRewardCfg cfg,bool isEnd = false)
        {
            IsEndPos = isEnd;
            Cfg = cfg;
            InitCellItem();
            RefreshValue();
            DataCenter.luckyTravelData.PropertyChanged += DataChanged;
            DataCenter.luckyTravelData.ClaimRecord.CollectionChanged += ScoreListChanged;
            DataCenter.luckyTravelData.OptionalRecord.CollectionChanged += OptionalChanged;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            DataCenter.luckyTravelData.PropertyChanged -= DataChanged;
            DataCenter.luckyTravelData.ClaimRecord.CollectionChanged -= ScoreListChanged;
            DataCenter.luckyTravelData.OptionalRecord.CollectionChanged -= OptionalChanged;
        }

        private void OptionalChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshCellItem();
        }

        private void DataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName==nameof(DataCenter.luckyTravelData.Progress))
            {
                RefreshValue();
            }
        }

        private void ScoreListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshValue();
        }

        protected void InitCellItem()
        {
            if(Cfg==null) return;
            if (Cfg.OptionalReward is { Count: > 0 })
            {
                selectItemViewModel = new SelectCellItemEffectViewModel(Cfg.OptionalReward,DataCenter.luckyTravelData.GetOptionalSelectIndex(Cfg.Id),ECellItemSizeType.Size90X80);
                selectItemViewModel.ClickEvent = OnClickSelectReward;
                selectItemViewModel.CellItemBaseViewVm.SetClickAction(OnClickSelectReward);
            }
            else if(Cfg.Reward is { Count: > 0 })
            {
                selectItemViewModel = SelectCellItemEffectViewModel.Create(Cfg.Reward[0],ECellItemSizeType.Size90X80);
                selectItemViewModel.CellItemBaseViewVm.SetClickAction(OnClickSelectReward);
            }

            selectItemViewModel.CellItemBaseViewVm.CellItemBaseViewVm.IsOpenMask = true;
        }
        
        protected void RefreshCellItem()
        {
            if (Cfg?.OptionalReward is { Count: > 0 })
            {
                selectItemViewModel.MergeSelect(DataCenter.luckyTravelData.GetOptionalSelectIndex(Cfg.Id));
            }
        }

        private void OnClickSelectReward(Tuple<Vector3, Vector3> info)
        {
            OnClickSelectReward();
        }

        private void OnClickSelectReward()
        {
            if(state==TaskState.Finish) return;
            if (selectItemViewModel.SelectType == SelectItemType.Select && (selectItemViewModel.IsSelect || State != TaskState.Get))
            {
                if(Cfg.OptionalReward!=null && Cfg.OptionalReward.Count>0)
                    UIManager.Instance.OpenDialog<CommonSelectRewardView>(new CommonSelectRewardViewModel(cfg.OptionalReward,DataCenter.luckyTravelData.GetOptionalSelectIndex(Cfg.Id),(selectIndex)=>
                    {
                        if (selectIndex >= 0)
                        {
                            DataCenter.luckyTravelData.SetOptionalSelectIndex(cfg.Id,selectIndex);
                        }
                    })).Forget();
                
            }
            else if (State == TaskState.Get)
            {
                SendLuckProgressClaim();
            }
        }

        private void SendLuckProgressClaim()
        {
            if (IsNeedSelectReward(Cfg.Id))
            {
                ToastManager.ShowLanguage(GlobalLanguageId.Trigger08);
                return;
            }

            SendLuckProgressClaim(Cfg.Id,DataCenter.luckyTravelData.GetOptionalSelectIndex(Cfg.Id)).Forget();
        }
        
        public bool IsNeedSelectReward(int id)
        {
            var proList = UIHelper.GetAllStageRewardList(ActivityStageType.LuckTravel);
            foreach (var item in proList)
            {
                if (item.Id < id && item.OptionalReward is { Count: > 0 } && !DataCenter.luckyTravelData.IsSelectOptionalReward(item.Id))
                {
                    return true;
                }
            }
            return false;
        }

        private void RefreshValue()
        {
            if(Cfg==null) return;
            ScoreValue = Cfg.Level;
            var score = DataCenter.luckyTravelData.Progress;
            var finish = DataCenter.luckyTravelData.CheckProgressFinish(Cfg.Id);
            if (score >= scoreValue)
            {
                ProgressValue = 1;
                if (finish)
                {
                    State = TaskState.Finish;
                    selectItemViewModel.CellItemBaseViewVm.SetClickAction(null); //任务清空选择事件，提示本身的道具
                    selectItemViewModel.CellItemBaseViewVm.State = ECellItemState.Finish;
                }
                else
                {
                    State = TaskState.Get;
                    selectItemViewModel.CellItemBaseViewVm.SetClickAction(OnClickSelectReward);
                    selectItemViewModel.CellItemBaseViewVm.State = ECellItemState.GetIng;
                }
            } 
            else
            {
                var lastScore = CollegeActivityManager.Instance.GetLastRewardValue(cfg.Id);
                if (score < lastScore)
                {
                    ProgressValue = 0;
                }
                else
                {
                    ProgressValue = (float)(score - lastScore) / (scoreValue - lastScore);
                }
                State = TaskState.UnFinish;
                selectItemViewModel.CellItemBaseViewVm.State = ECellItemState.None;
                if (Cfg.OptionalReward is { Count: > 0 })
                {
                    selectItemViewModel.CellItemBaseViewVm.SetClickAction(OnClickSelectReward);
                }
                else
                {
                    selectItemViewModel.CellItemBaseViewVm.SetClickAction(null);
                }
            }

        }

        /// <summary>
        /// 领取记录
        /// </summary>
        /// <param name="id">记录id</param>
        /// <param name="index">自选奖励</param>
        public async UniTaskVoid SendLuckProgressClaim(int id,int index = -1)
        {
            if(!ActivityUIManager.Instance.CheckLuckTravelOpen()) return;
            var req = new ReqLuckyTripClaim()
            {
                Id = id,
                OptionalIndex = index
            };
            var result =await GameNetworkManager.Instance.SendAsync<RspLuckyTripClaim>(req);
            
            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                Lodash.DealRewards(result.rsp.Reward.ToList());
                DataCenter.luckyTravelData.SetScoreClaimed(id);
                UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList());
            }
        }

        
    }
}