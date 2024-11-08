using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using Dh.Game.ViewModels;
using DH.UIFramework;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class CollegeActivityModel : ViewModelBase
    {
        public override bool AutoDispose => true;
        [AutoNotify] private CollegeActivity curSelect;
        public TabBtnGroupViewModel CollegeTabBtnModel;
        public CollegeActivityManager Manager = CollegeActivityManager.Instance;
        public CollegeRankViewModel CollegeRankView;
        public CollegeTaskModel CollegeTaskModel;
        public CollegeDiscountsModel CollegeDiscountsModel;
        public CollegeShopViewModel CollegeShopModel;

        [Preserve]  
        public CollegeActivityModel()
        {
          
            InitTabBtn();
            CurSelect = CollegeActivityManager.Instance.CurTab;
            CollegeTaskModel= new CollegeTaskModel();
            CollegeDiscountsModel = new CollegeDiscountsModel();
            CollegeRankView = new CollegeRankViewModel();
            CollegeShopModel= new CollegeShopViewModel();
            RefreshGroupTitleRed();
            CollegeActivityManager.Instance.PropertyChanged += CollegePropertyChanged;
            DataCenter.collegeData.PropertyChanged += TaskChanged;
            DataCenter.collegeData.TaskClaimed.CollectionChanged += TaskChanged;
            DataCenter.collegeData.ScoreClaimed.CollectionChanged += TaskChanged;
            DataCenter.collegeData.TaskProgress.CollectionChanged += TaskChanged;
            DataCenter.collegeData.FundClaimed.CollectionChanged += TaskChanged;
            DataCenter.collegeData.FundPlusClaimed.CollectionChanged += TaskChanged;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            CollegeActivityManager.Instance.PropertyChanged -= CollegePropertyChanged;
            DataCenter.collegeData.PropertyChanged -= TaskChanged;
            DataCenter.collegeData.TaskClaimed.CollectionChanged -= TaskChanged;
            DataCenter.collegeData.ScoreClaimed.CollectionChanged -= TaskChanged;
            DataCenter.collegeData.TaskProgress.CollectionChanged -= TaskChanged;
            DataCenter.collegeData.FundClaimed.CollectionChanged -= TaskChanged;
            DataCenter.collegeData.FundPlusClaimed.CollectionChanged -= TaskChanged;
       
        }

        private void TaskChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataCenter.collegeData.Score))
            {
                RefreshGroupTitleRed();
            }
        }

        private void TaskChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshGroupTitleRed();
        }

        private void CollegePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CollegeActivityManager.Instance.CurTab))
            {
                CurSelect = CollegeActivityManager.Instance.CurTab;
                CollegeTabBtnModel.CurPos = (int)CurSelect;
            }
        }
        
        private void RefreshGroupTitleRed()
        {
            CollegeTabBtnModel.RefreshRedDot((int)CollegeActivity.CollegeTask,CollegeActivityManager.Instance.CheckCollegeTaskRed());
            CollegeTabBtnModel.RefreshRedDot((int)CollegeActivity.CollegeFund,CollegeActivityManager.Instance.CheckCollegeFundRed());
            CollegeTabBtnModel.RefreshRedDot((int)CollegeActivity.CollegeShop,CollegeActivityManager.Instance.CheckCollegeShopRed());
        }

        private void InitTabBtn()
        {
            List<TabBtnInfo> btnInfos = new List<TabBtnInfo>();
            var btnInfo1 = new TabBtnInfo()
            {
                Pos = (int)CollegeActivity.CollegeTask,
                OnPath = $"school[school_icon_1]",
                OffPath = $"school[school_icon_1]",
                Name = LocalizeHelper.GetGlobal(GlobalLanguageId.mofaxy02),
            };
            var btnInfo2 = new TabBtnInfo()
            {
                Pos = (int)CollegeActivity.CollegeRank,
                OnPath = $"school[school_icon_2]",
                OffPath = $"school[school_icon_2]",
                Name = LocalizeHelper.GetGlobal(GlobalLanguageId.mofaxy03),
            };
            var btnInfo3 = new TabBtnInfo()
            {
                Pos = (int)CollegeActivity.CollegeFund,
                OnPath = $"school[school_icon_4]",
                OffPath = $"school[school_icon_4]",
                Name = LocalizeHelper.GetGlobal(GlobalLanguageId.mofaxy05),
            };
            var btnInfo4 = new TabBtnInfo()
            {
                Pos = (int)CollegeActivity.CollegeShop,
                OnPath = $"school[school_icon_3]",
                OffPath = $"school[school_icon_3]",
                Name = LocalizeHelper.GetGlobal(GlobalLanguageId.mofaxy04),
            };
            
            btnInfos.Add(btnInfo1);
            btnInfos.Add(btnInfo2);
            btnInfos.Add(btnInfo3);
            btnInfos.Add(btnInfo4);
            CollegeTabBtnModel = new TabBtnGroupViewModel(btnInfos,(int)CollegeActivity.CollegeTask,ClickSelect);
        }

        
        
        private void ClickSelect(int index)
        {
            CurSelect = (CollegeActivity)index;
            CollegeActivityManager.Instance.CurTab = CurSelect;
        }

        [Command]
        public void OnClose()
        {
            UIManager.Instance.CloseDialog<CollegeActivityView>();
        }
      
        private float time;
        public override void Update()
        {
            base.Update();
            if (!UIHelper.CalculateTime(ref time)) return;
            if (CollegeActivityManager.Instance.CheckEndTime())
            {
                UIManager.Instance.CloseDialog<CollegeActivityView>();
            }
        }
    }
}