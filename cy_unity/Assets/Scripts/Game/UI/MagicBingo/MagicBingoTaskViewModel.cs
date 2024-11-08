using System;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DH.Data;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class MagicBingoTaskViewModel : ViewModelBase
    {
        public override bool AutoDispose => true;
        [AutoNotify] private CommonTopViewModel commonTopItemsVm;
        [AutoNotify] private ObservableList<MagicBingoTaskItemViewModel> dailyScrollViewList = new();
        [AutoNotify] private ObservableList<MagicBingoTaskItemViewModel> activityScrollViewList = new();
        private MagicBingoData Data => DataCenter.mgicBingoData;
        [AutoNotify] private string timeDes;
        [Preserve]
        public MagicBingoTaskViewModel()
        {
            InitTopItems();
            InitUI();
            RefreshTimeDesc();
            Data.TaskClaimed.CollectionChanged += TaskChanged;
            Data.TaskProgress.CollectionChanged += TaskChanged;
            PlayerInfoManager.Instance.PropertyChanged  += SecondDay;
        }
        
        private void TaskChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            InitUI();
        }

        private void SecondDay(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            InitUI();
        }
        
        protected override void OnDispose()
        {
            base.OnDispose();
            Data.TaskClaimed.CollectionChanged -= TaskChanged;
            Data.TaskProgress.CollectionChanged -= TaskChanged;
            PlayerInfoManager.Instance.PropertyChanged  -= SecondDay;
        }
        
        private void InitTopItems()
        {
            CommonTopItemsVm = new CommonTopViewModel(new List<GameConst.ItemIdCode>
            {
                GameConst.ItemIdCode.BinGoPoint,
                GameConst.ItemIdCode.Stone,
            });
        }
        
        
        void InitUI()
        {
            InitDailyTaskList();
            InitActivityTaskList();
        }
        private void InitDailyTaskList()
        {
            DailyScrollViewList.Clear();
            var temp = Data.GetAllRewardList((int)ETaskType.MagicBingo,1);
            var tempList = new List<MagicBingoTaskItemViewModel>();
            for (int i = 0; i < temp.Count; i++)
            {
                tempList.Add(new MagicBingoTaskItemViewModel(temp[i]));
            }

            tempList = tempList.OrderBy(o => o.State == LuckEggTaskItemState.Finish)
                .ThenByDescending(o => o.State == LuckEggTaskItemState.NotGetAward).ToList();
            DailyScrollViewList.AddRange(tempList);
        }
        private void InitActivityTaskList()
        {
            ActivityScrollViewList.Clear();
            var temp = Data.GetAllRewardList((int)ETaskType.MagicBingo,2);
            var tempList = new List<MagicBingoTaskItemViewModel>();
            for (int i = 0; i < temp.Count; i++)
            {
                tempList.Add(new MagicBingoTaskItemViewModel(temp[i]));
            }

            tempList = tempList.OrderBy(o => o.State == LuckEggTaskItemState.Finish)
                .ThenByDescending(o => o.State == LuckEggTaskItemState.NotGetAward).ToList();
            ActivityScrollViewList.AddRange(tempList);
        }
        
        #region 倒计时相关
        private float interval;
        private void RefreshTimeDesc()
        {
            var times = Math.Max(0, DataCenter.mgicBingoData.EndStamp - ServerTime.Instance.GetNowTime());
            TimeDes =  ServerTime.Instance.SecondsDHAndMS(times);
        }
        public override void Update()
        {
            if (UIHelper.CalculateTime(ref interval))
            {
                RefreshTimeDesc();
            }
        }
        

        #endregion
    }
}