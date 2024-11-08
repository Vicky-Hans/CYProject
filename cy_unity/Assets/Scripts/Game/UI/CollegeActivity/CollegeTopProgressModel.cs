using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.UIFramework.ViewModels;
using UnityEngine;
using UnityEngine.Scripting;

namespace Dh.Game.ViewModels
{
    public enum TaskState
    {
        None,
        UnLock,
        UnFinish,
        GoWay,
        Get,
        Finish,
    }

    public partial class CollegeTopProgressModel : ViewModelBase
    {
        [AutoNotify] private int scoreValue;
        [AutoNotify] private StageRewardCfg cfg;
        [AutoNotify] private float progressValue;

        [AutoNotify] private SelectCellItemEffectViewModel selectItemViewModel;
        [AutoNotify] private TaskState state;
        [AutoNotify] private bool isEndPos;
        
        [Preserve]
        public CollegeTopProgressModel(StageRewardCfg cfg,bool isEnd = false)
        {
            IsEndPos = isEnd;
            Cfg = cfg;
            InitCellItem();
            RefreshValue();
            DataCenter.collegeData.PropertyChanged += DataChanged;
            DataCenter.collegeData.ScoreClaimed.CollectionChanged += ScoreListChanged;
            DataCenter.collegeData.OptionalRecord.CollectionChanged += OptionalChanged;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            DataCenter.collegeData.PropertyChanged -= DataChanged;
            DataCenter.collegeData.ScoreClaimed.CollectionChanged -= ScoreListChanged;
            DataCenter.collegeData.OptionalRecord.CollectionChanged -= OptionalChanged;
        }

        private void OptionalChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshCellItem();
        }

        private void DataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName==nameof(DataCenter.collegeData.Score))
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
                selectItemViewModel = new SelectCellItemEffectViewModel(Cfg.OptionalReward,DataCenter.collegeData.GetOptionalSelectIndex(Cfg.Id),ECellItemSizeType.Size120X100);
                selectItemViewModel.ClickEvent = OnClickSelectReward;
                selectItemViewModel.CellItemBaseViewVm.SetClickAction(OnClickSelectReward);
            }
            else if(Cfg.Reward is { Count: > 0 })
            {
                selectItemViewModel = SelectCellItemEffectViewModel.Create(Cfg.Reward[0],ECellItemSizeType.Size120X100);
                selectItemViewModel.CellItemBaseViewVm.SetClickAction(OnClickSelectReward);
            }

            selectItemViewModel.CellItemBaseViewVm.CellItemBaseViewVm.IsOpenMask = true;
        }
        
        protected void RefreshCellItem()
        {
            if (Cfg?.OptionalReward is { Count: > 0 })
            {
                selectItemViewModel.MergeSelect(DataCenter.collegeData.GetOptionalSelectIndex(Cfg.Id));
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
                    UIManager.Instance.OpenDialog<CommonSelectRewardView>(new CommonSelectRewardViewModel(cfg.OptionalReward,DataCenter.collegeData.GetOptionalSelectIndex(Cfg.Id),(selectIndex)=>
                    {
                        if (selectIndex >= 0)
                        {
                            DataCenter.collegeData.SetOptionalSelectIndex(cfg.Id,selectIndex);
                        }
                    })).Forget();
                
            }
            else if (State == TaskState.Get)
            {
                SendCollegeScoreClaim();
            }
        }

        private void SendCollegeScoreClaim()
        {
            if (CollegeActivityManager.Instance.IsNeedSelectReward(Cfg.Id))
            {
                ToastManager.ShowLanguage(GlobalLanguageId.Trigger08);
                return;
            }

            CollegeActivityManager.Instance.SendCollegeScoreClaim(Cfg.Id,DataCenter.collegeData.GetOptionalSelectIndex(Cfg.Id)).Forget();
        }

        private void RefreshValue()
        {
            if(Cfg==null) return;
            ScoreValue = Cfg.Level;
            var score = DataCenter.collegeData.GetScore();
            var finish = DataCenter.collegeData.CheckProgressFinish(Cfg.Id);
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
    }
}