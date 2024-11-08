using System;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using Game.UI.MainStage;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class InvitedRewardItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private CellItemViewModel cellItemViewVm;
		[AutoNotify] private string progressTextStr;
		[AutoNotify] private bool isShowIconBtn;
		private ShareRewardProgressCfg curCfg;
		[AutoNotify] private ECellItemState curState;
		private Action<ShareRewardProgressCfg> claimRewardCallback;
        [Preserve]
        public InvitedRewardItemViewModel(ShareRewardProgressCfg cfg, Action<ShareRewardProgressCfg> callback)
        {
	        curCfg = cfg;
	        claimRewardCallback = callback;
	        UpdateProgress();
        }


        [Command]
        private void OnClickShowIconBtn(Tuple<Vector3, Vector3> info)
        {
	        // 这里判断领取没
	        if (curState != ECellItemState.GetIng)
	        {
		        // 这里显示
		        var preModel = new ChapterBoxPreviewViewModel(curCfg.Reward);
		        preModel.NodePos = info.Item1;
		        UIManager.Instance.OpenDialog<ChapterBoxPreviewView>(preModel).Forget();
	        }
	        else
	        {
		        claimRewardCallback(curCfg);
	        }
        }

        public void UpdateState()
        {
	        curState = ECellItemState.None;
	        if (curCfg.Value1 <= DataCenter.charcaterData.InviteNumber)
	        {
		        curState = DataCenter.charcaterData.CheckIsClaimedInvitedReward(curCfg.Id) ? ECellItemState.Finish : ECellItemState.GetIng;
	        }
	        cellItemViewVm.State = curState;
	        if (!IsShowIconBtn && curState != ECellItemState.GetIng)
	        {
		        cellItemViewVm.SetClickAction(null);
	        }
	        else
	        {
		        cellItemViewVm.SetClickAction(OnClickShowIconBtn);
	        }

	     
        }

        private void UpdateProgress()
		{
			// 不显示奖励，显示指定的图
			IsShowIconBtn = curCfg.Reward.Count > 1;
			ProgressTextStr = curCfg.Value1.ToString();
			cellItemViewVm = CellItemViewModel.Create(curCfg.Reward[0], ECellItemSizeType.Size90X80);
			UpdateState();
		}
    }
}