using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using DH.Config;
using DH.Data;
using DH.Game;
using DH.Game.UI;
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using Game.UI.MainUi;

namespace Dh.Game.ViewModels
{
    public enum CollegeTaskTitle
    {
        Clothes=1,
        Draw=2,
        Else=3,
    }

    public partial class CollegeTaskModel : ViewModelBase
    {
        public override bool AutoDispose => true;
        [AutoNotify] private ObservableList<CollegeTopProgressModel> topProgressModels=new();
        [AutoNotify] private ObservableList<CollegeTaskItemModel> taskItemModels=new();
        [AutoNotify] private TabBtnGroupTitleModel tabBtnGroupTitle;
        [AutoNotify] private int curTitleType;
        public CommonTopViewModel CommonTopItemsModel;
        [AutoNotify] private string endTImeValueStr;
        private float IntervalTime = 0;
        
        private float SecTime;
        [AutoNotify] private int score;
        [AutoNotify] private int topIndex;

        [AutoNotify] public int startMoveState = -1;
        public CollegeTaskModel()
        {
            InitTopProgressModels();
            InitTabGroup();
            InitTopItems();
            RefreshTopIndexPos();
            Score = DataCenter.collegeData.GetScore();
            RefreshTime();
            RefreshTabBtnGroupTitleRed();
            CollegeActivityManager.Instance.PropertyChanged += CollegePropertyChanged;
            DataCenter.collegeData.PropertyChanged += DataChanged;
            DataCenter.collegeData.TaskClaimed.CollectionChanged += TaskChanged;
            DataCenter.collegeData.TaskProgress.CollectionChanged += TaskProgressChanged;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            CollegeActivityManager.Instance.PropertyChanged -= CollegePropertyChanged;
            DataCenter.collegeData.PropertyChanged -= DataChanged;
            DataCenter.collegeData.TaskClaimed.CollectionChanged -= TaskChanged;
            DataCenter.collegeData.TaskProgress.CollectionChanged -= TaskProgressChanged;
        }
        private void CollegePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CollegeActivityManager.Instance.CurTab))
            {
                RefreshTopIndexPos();
            }
        }

        private void TaskProgressChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshTabBtnGroupTitleRed();
        }

        private void TaskChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshTaskList();
            RefreshTabBtnGroupTitleRed();
        }

        private void DataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName==nameof(DataCenter.collegeData.Score))
            {
                Score = DataCenter.collegeData.GetScore();
            }
        }

        private void InitTopItems()
        {
            CommonTopItemsModel = new CommonTopViewModel(new List<GameConst.ItemIdCode>
            {
                GameConst.ItemIdCode.Stone,
                GameConst.ItemIdCode.Money,
            });
        }

        private void InitTopProgressModels()
        {
            var list = CollegeActivityManager.Instance.GetAllRewardList();
            for (int i = 0; i < list.Count; i++)
            {
                topProgressModels.Add(new CollegeTopProgressModel(list[i],i==list.Count-1));
            }
         
        }

        private void RefreshTopIndexPos()
        {
            topIndex = 0;
            var list = CollegeActivityManager.Instance.GetAllRewardList();
            for (int i = 0; i < list.Count; i++)
            {
                if (!DataCenter.collegeData.CheckProgressFinish(list[i].Id) )
                {
                    TopIndex = i;
                    break;
                }
            }
        }

        //初始化任务类型标签
        private void InitTabGroup()
        {
            List<TabBtnInfo> btnInfos = new List<TabBtnInfo>();
            var btnInfo1 = new TabBtnInfo()
            {
                Pos = (int)CollegeTaskTitle.Clothes,
                Name = MainUiManager.Instance.GetFunctionName(EFunctionOpenType.FunctionClothes)
            };
            var btnInfo2 = new TabBtnInfo()
            {
                Pos = (int)CollegeTaskTitle.Draw,
                Name = LocalizeHelper.GetGlobal(GlobalLanguageId.mofaxy16)
            };
            var btnInfo3 = new TabBtnInfo()
            {
                Pos = (int)CollegeTaskTitle.Else,
                Name = LocalizeHelper.GetGlobal(GlobalLanguageId.mofaxy12),//其他
            };
     
            btnInfos.Add(btnInfo1);
            btnInfos.Add(btnInfo2);
            btnInfos.Add(btnInfo3);

            var initSelect = CollegeTaskTitle.Clothes;
            if (CollegeActivityManager.Instance.CheckCollegeTypeRed(CollegeTaskTitle.Clothes))
            {
                initSelect = CollegeTaskTitle.Clothes;
            }
            else if (CollegeActivityManager.Instance.CheckCollegeTypeRed(CollegeTaskTitle.Draw))
            {
                initSelect = CollegeTaskTitle.Draw;
            }else if (CollegeActivityManager.Instance.CheckCollegeTypeRed(CollegeTaskTitle.Else))
            {
                initSelect = CollegeTaskTitle.Else;
            }

            TabBtnGroupTitle = new TabBtnGroupTitleModel(btnInfos,(int)initSelect, (pos) =>
            {
                CurTitleType = pos;
                RefreshTaskList();
            });
        }

        private void RefreshTabBtnGroupTitleRed()
        {
            TabBtnGroupTitle.RefreshRedDot((int)CollegeTaskTitle.Clothes,CollegeActivityManager.Instance.CheckCollegeTypeRed(CollegeTaskTitle.Clothes));
            TabBtnGroupTitle.RefreshRedDot((int)CollegeTaskTitle.Draw,CollegeActivityManager.Instance.CheckCollegeTypeRed(CollegeTaskTitle.Draw));
            TabBtnGroupTitle.RefreshRedDot((int)CollegeTaskTitle.Else,CollegeActivityManager.Instance.CheckCollegeTypeRed(CollegeTaskTitle.Else));
        }

        private void RefreshTaskList()
        {
            TaskItemModels.Clear();
            var taskList = CollegeActivityManager.Instance.GetAllRewardList(CurTitleType);
            if (taskList != null)
            {
                for (int i = 0; i < taskList.Count; i++)
                {
                    var itemModel = new CollegeTaskItemModel(taskList[i]);
                    itemModel.StartMoveAction = StartMove;
                    TaskItemModels.Add(itemModel);
                }
            }
        }

        private void StartMove(int taskId)
        {
            for (int i = 0; i < taskItemModels.Count; i++)
            {
                if (taskItemModels[i].Cfg.Id == taskId)
                {
                    startMoveState = i;
                    RaisePropertyChanged(nameof(StartMoveState));
                    return;
                }
            }
        }
        
        private void RefreshTime()
        {
            EndTImeValueStr = UIHelper.GetRefreshDayTime(DataCenter.collegeData.EndStamp);//ServerTime.Instance.Seconds2Hhmmss(ServerTime.Instance.RemainTime(DataCenter.shopData.RefreshStamp));
        }

        [Command]
        private void OnClickRule()
        {
            //规则提示
            UIHelper.OpenCommonRule(LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips13),LocalizeHelper.GetGlobal(GlobalLanguageId.mofaxy11));
        }
  
        public override void Update()
        {
            if(!UIHelper.CalculateTime(ref IntervalTime)) return;
            RefreshTime();
        }

    }
}