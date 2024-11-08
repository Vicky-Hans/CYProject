using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;

namespace DH.Game.ViewModels
{
    public partial class CollegeRankViewModel : ViewModelBase
    {
	    public override bool AutoDispose => true;
	    [AutoNotify] private RankTopCellViewModel rankTopCell1Vm = new RankTopCellViewModel(null);
		[AutoNotify] private RankTopCellViewModel rankTopCell2Vm = new RankTopCellViewModel(null);
		[AutoNotify] private RankTopCellViewModel rankTopCell3Vm = new RankTopCellViewModel(null);
		[AutoNotify] private string rankTextStr;
		[AutoNotify] private string playNameTextStr;
		[AutoNotify] private string maxLevelTextStr;
		[AutoNotify] private ObservableList<CollegeRankItemViewModel> scrollViewScoreList = new();
		[AutoNotify] private ObservableList<CollegeRankRewardItemViewModel> scrollViewRewardList = new();
		[AutoNotify] private CommonTopViewModel commonTopItemsVm;
		[AutoNotify] private CollegeRankItemViewModel collegeRankItemViewVm;
		[AutoNotify] private CollegeRankRewardItemViewModel collegeRankRewardItemViewVm;
		[AutoNotify] private TabBtnGroupTitleModel tabBtnGroupTitleViewVm;
		[AutoNotify] private int selectShopTitle;
        [Preserve]
        public CollegeRankViewModel()
        {
	        InitTopItems();
	        InitBtnGroup();
	        RefreshRewardInfo();
	        CollegeActivityManager.Instance.PropertyChanged += ManagerPropertyChanged;
        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        CollegeActivityManager.Instance.PropertyChanged -= ManagerPropertyChanged;
        }

        private void ManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(CollegeActivityManager.CurTab))
	        {
		        if (CollegeActivityManager.Instance.CurTab == CollegeActivity.CollegeRank)
		        {
			        RefreshRankInfo();
		        }
	        }
        }

        private void InitTopItems()
        {
	        commonTopItemsVm = new CommonTopViewModel(new List<GameConst.ItemIdCode>
	        {
		        GameConst.ItemIdCode.EnergyDrink,
		        GameConst.ItemIdCode.Stone,
		        GameConst.ItemIdCode.Money,
	        });
        }

        private void InitBtnGroup()
        {
	        List<TabBtnInfo> btnInfos = new List<TabBtnInfo>();
	        var btnInfo = new TabBtnInfo()
	        {
		        Pos = 0,
		        Name = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips12),
	        }; 
	        
	        var btnInfo1 = new TabBtnInfo()
	        {
		        Pos = 1,
		        Name = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips21),
	        }; 
	        btnInfos.Add(btnInfo);
	        btnInfos.Add(btnInfo1);
	        tabBtnGroupTitleViewVm = new TabBtnGroupTitleModel(btnInfos,(int)SelectShopTitle,ClickSelect);
        }
        
        private void ClickSelect(int index)
        {
	        SelectShopTitle = index;
	        RefreshTitleName();
        }

        private void RefreshTitleName()
        {
	        if (SelectShopTitle == 0)
	        {
		        RankTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips10);
		        PlayNameTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips11);
		        MaxLevelTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.mofaxy07);
	        }
	        else
	        {
		        RankTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips10);
		        PlayNameTextStr = string.Empty;
		        MaxLevelTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips21);
	        }
        }


        private void RefreshRewardInfo()
        {
	        ScrollViewRewardList.Clear();
	        var rewardList = ConfigCenter.ActivityRankingCfgColl.DataItems.ToList();
	        foreach (var item in rewardList)
	        {
		        ScrollViewRewardList.Add(new CollegeRankRewardItemViewModel(item));
	        }
        }

        private void RefreshRankInfo()
        {
	        RefreshInfoNone();
	        CollegeActivityManager.Instance.SendCollegeRank(RefreshInfo).Forget();
        }

        private void RefreshInfo(List<RankMember> topList,int score,long rank)
        {
	        RankTopCell1Vm = topList.Count >= 1 ? new RankTopCellViewModel(topList[0], (info) =>
	        {
		        CollegeActivityManager.Instance.SendPlayerInfo(info).Forget();
	        },true) : new RankTopCellViewModel(null);
		        
	        RankTopCell2Vm = topList.Count >= 2 ? new RankTopCellViewModel(topList[1], (info) =>
	        {
		        CollegeActivityManager.Instance.SendPlayerInfo(info).Forget();
	        },true) : new RankTopCellViewModel(null);
		        
	        RankTopCell3Vm = topList.Count >= 3 ? new RankTopCellViewModel(topList[2], (info) =>
	        {
		        CollegeActivityManager.Instance.SendPlayerInfo(info).Forget();
	        },true) : new RankTopCellViewModel(null);

	        CollegeRankItemViewVm = new CollegeRankItemViewModel(CollegeActivityManager.Instance.GetMySelfRankMember(score, rank));
	        CollegeRankRewardItemViewVm = new CollegeRankRewardItemViewModel(CollegeActivityManager.Instance.GetMySelfRankingCfg(rank));
	        var count = Math.Min(3, topList.Count);
	        for (int i = 0; i < count; i++)
	        {
		        topList.RemoveAt(0);
	        }
	        ScrollViewScoreList.Clear();
	        foreach (var item in topList)
	        {
		        ScrollViewScoreList.Add(new CollegeRankItemViewModel(item));
	        }
        }
        
        private void RefreshInfoNone()
        {
	        RankTopCell1Vm = new RankTopCellViewModel(null);
	        RankTopCell2Vm = new RankTopCellViewModel(null);
	        RankTopCell3Vm = new RankTopCellViewModel(null);
	        CollegeRankItemViewVm = new CollegeRankItemViewModel(CollegeActivityManager.Instance.GetMySelfRankMember(0, -1));
	        CollegeRankRewardItemViewVm = new CollegeRankRewardItemViewModel(null);
	        ScrollViewScoreList.Clear();
	       
        }
    }
}