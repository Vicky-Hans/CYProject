using System.Collections.Generic;
using System.ComponentModel;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class NewBieRewardCellItemViewModel : ViewModelBase
    {
		[AutoNotify] private string titleTextStr;
	    public NewBieData Data => DataCenter.newBieData;
	    [AutoNotify] private bool isToday;
	    
	    private int mIndex;
	    private List<Reward> mRewards;
	    
	    [AutoNotify] private CellItemViewModel cellItem1;
	    [AutoNotify] private CellItemViewModel cellItem2;
	    [AutoNotify] private CellItemViewModel cellItem3;
	    [AutoNotify] private CellItemViewModel cellItem4;
	    [AutoNotify] private int rewardNums;
        [Preserve]
        public NewBieRewardCellItemViewModel(List<Reward> rewards, int index)
        {
	        mIndex = index;
	        mRewards = rewards;
	        RewardNums = rewards.Count;
	        InitList();
	        DataCenter.newBieData.PropertyChanged += ProperChange;
	        PlayerInfoManager.Instance.PropertyChanged += ProperChange;
	        TitleTextStr = string.Format(ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.Xinshou_02).Name,mIndex);
        }

        private CellItemViewModel CellItemVM(Reward reward)
        {
	        var isGet = Data.IsGetAward(mIndex);
	        var cellModel = CellItemViewModel.Create(reward);
	        cellModel.SetSize(ECellItemSizeType.Size120X100);
	        if (Data.IsBuy)
	        {
		        if (isGet)
		        {
			        cellModel.State = ECellItemState.Finish;
		        }
		        else
		        {
			        if (GetNowDay() >= mIndex)
			        {
				        cellModel.State = ECellItemState.GetIng;
			        }
			        else
			        {
				        cellModel.State = ECellItemState.None;
			        }
		        }
	        }
	        else
	        {
		        cellModel.State = ECellItemState.None;
	        }
	        return cellModel;
        }

        public void ProperChange(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName is nameof(Data.Claim) or nameof(PlayerInfoManager.Instance.SecondDay))
	        {
		        InitList();
	        }
        }
        
        protected override void OnDispose()
        {
	        DataCenter.newBieData.PropertyChanged -= ProperChange;
	        PlayerInfoManager.Instance.PropertyChanged -= ProperChange;
	        base.OnDispose();
        }
        int GetNowDay()
        {
	        return ServerTime.Instance.GetTimeDay(Data.BuyStamp)+1;
        }

        private void InitList()
        {
	        switch (mRewards.Count)
	        {
		        case 1:
			        CellItem1 = CellItemVM(mRewards[0]);
			        break;
		        case 2:
			        CellItem1 = CellItemVM(mRewards[0]);
			        CellItem2 = CellItemVM(mRewards[1]);
			        break;
		        case 3:
			        CellItem1 = CellItemVM(mRewards[0]);
			        CellItem3 = CellItemVM(mRewards[1]);
			        CellItem4 = CellItemVM(mRewards[2]);
			        break;
		        default:
			        CellItem1 = CellItemVM(mRewards[0]);
			        CellItem2 = CellItemVM(mRewards[1]);
			        CellItem3 = CellItemVM(mRewards[2]);
			        CellItem4 = CellItemVM(mRewards[3]);
			        break;
	        }
        }
    }
}