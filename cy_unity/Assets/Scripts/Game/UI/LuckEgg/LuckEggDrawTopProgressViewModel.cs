using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using Dh.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class LuckEggDrawTopProgressViewModel : ViewModelBase
    {
        public override bool AutoDispose => true;
        [AutoNotify] private int scoreValue;
        [AutoNotify] private StageRewardCfg cfg;
        [AutoNotify] private float progressValue;

        [AutoNotify] private SelectCellItemEffectViewModel selectItemViewModel;
        [AutoNotify] private TaskState state;
        [AutoNotify] private bool isEndPos;
        
        [Preserve]
        public LuckEggDrawTopProgressViewModel(StageRewardCfg cfg,bool isEnd = false)
        {
            IsEndPos = isEnd;
            Cfg = cfg;
            InitCellItem();
            RefreshValue();
            DataCenter.luckyEggData.PropertyChanged += DataChanged;
            DataCenter.luckyEggData.StageClaimed.CollectionChanged += ScoreListChanged;
            DataCenter.luckyEggData.OptionalRecord.CollectionChanged += OptionalChanged;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            DataCenter.luckyEggData.PropertyChanged -= DataChanged;
            DataCenter.luckyEggData.StageClaimed.CollectionChanged -= ScoreListChanged;
            DataCenter.luckyEggData.OptionalRecord.CollectionChanged -= OptionalChanged;
        }

        private void OptionalChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshCellItem();
        }

        private void DataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName==nameof(DataCenter.luckyEggData.Count))
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
                selectItemViewModel = new SelectCellItemEffectViewModel(Cfg.OptionalReward,DataCenter.luckyEggData.GetOptionalSelectIndex(Cfg.Id),ECellItemSizeType.Size90X80);
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
                selectItemViewModel.MergeSelect(DataCenter.luckyEggData.GetOptionalSelectIndex(Cfg.Id));
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
                    UIManager.Instance.OpenDialog<CommonSelectRewardView>(new CommonSelectRewardViewModel(cfg.OptionalReward,DataCenter.luckyEggData.GetOptionalSelectIndex(Cfg.Id),(selectIndex)=>
                    {
                        if (selectIndex >= 0)
                        {
                            DataCenter.luckyEggData.SetOptionalSelectIndex(cfg.Id,selectIndex);
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

            ActivityUIManager.Instance.SendLuckEggClaim(Cfg.Id,DataCenter.luckyEggData.GetOptionalSelectIndex(Cfg.Id)).Forget();
        }
        
        public bool IsNeedSelectReward(int id)
        {
            var proList = UIHelper.GetAllStageRewardList(ActivityStageType.LuckEgg);
            foreach (var item in proList)
            {
                if (item.Id < id && item.OptionalReward is { Count: > 0 } && !DataCenter.luckyEggData.IsSelectOptionalReward(item.Id))
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
            var score = DataCenter.luckyEggData.Count;
            var finish = DataCenter.luckyEggData.CheckProgressFinish(Cfg.Id);
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