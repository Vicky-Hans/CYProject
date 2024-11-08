using System;
using System.Collections.Specialized;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.UIFramework.ViewModels;

namespace Dh.Game.ViewModels
{
    public partial class CollegeTaskItemModel : ViewModelBase
    {
        [AutoNotify]
        private ActivityTaskCfg cfg;
        [AutoNotify]
        private CommonItemListModel itemListModel;
        [AutoNotify] 
        private float progressValue;
        [AutoNotify] 
        private string progressDesc;
        [AutoNotify] private TaskState state;
        [AutoNotify] private string taskDesc;

        public Action<int> StartMoveAction;
        public CollegeTaskItemModel(ActivityTaskCfg cfg)
        {
            Cfg = cfg;
            TaskDesc = CollegeActivityManager.Instance.GetTaskDesc(Cfg);
            RefreshState();
            InitItemList();
            DataCenter.collegeData.TaskClaimed.CollectionChanged += TaskChanged;
            DataCenter.collegeData.TaskProgress.CollectionChanged += TaskChanged;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            DataCenter.collegeData.TaskClaimed.CollectionChanged -= TaskChanged;
            DataCenter.collegeData.TaskProgress.CollectionChanged -= TaskChanged;
        }

        private void TaskChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshState();
        }

        private void InitItemList()
        {
            itemListModel = new CommonItemListModel(Cfg.Reward);
        }

        private void RefreshState()
        {
            var finish = DataCenter.collegeData.IsFinishTask(Cfg.Id);
            var all = Cfg.EventLoad;
            var value = DataCenter.collegeData.GetTaskProgress(Cfg.Id);
            ProgressDesc = $"{value}/{all}";
            ProgressValue = (float)value / all;
            RefreshRewardState(false);
            if (all > value)
            {
                if (Cfg.TaskList != 0)
                {
                    State = TaskState.GoWay;
                }
                else
                {
                    State = TaskState.UnFinish;
                }

            }else
            {
                if (finish)
                {
                    State = TaskState.Finish;   
                }
                else
                {
                    State = TaskState.Get;
                    RefreshRewardState(true);
                }
            }
        }

        public void RefreshRewardState(bool show)
        {
            if (itemListModel is { ItemListModels: not null })
            {
                foreach (var item in itemListModel.ItemListModels)
                {
                    item.State = ECellItemState.GetIng;
                }
            }
        }

        [Command]
        private void OnClickGet()
        {
            CollegeActivityManager.Instance.SendCollegeTaskClaim(Cfg.Id, () =>
            {
                StartMoveAction?.Invoke(cfg.Id);
            }).Forget();
        }
        
        [Command]
        private void OnGoWay()
        {
            JumpManager.Instance.Jump(cfg.TaskList);
        }
    }
}