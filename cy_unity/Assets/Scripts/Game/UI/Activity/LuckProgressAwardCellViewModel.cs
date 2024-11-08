using System;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using Vector3 = UnityEngine.Vector3;


namespace DH.Game.ViewModels
{
    public partial class LuckProgressAwardCellViewModel : ViewModelBase
    {
		[AutoNotify] private SelectCellItemEffectViewModel cellItemViewVm;
		[AutoNotify] private string progressTextStr;
		[AutoNotify] private float progressValue;
		[AutoNotify] private ECellItemState curState;
		[AutoNotify] private string progressImgPath;
		private readonly StageRewardCfg curCfg;
		private readonly Action<int> OnClickRecordItemCallback;
		private EDrawState curDrawState;
        [Preserve]
        public LuckProgressAwardCellViewModel(StageRewardCfg cfg, Action<int> clickRecordCallback,bool isEnd, EDrawState state)
        {
	        curCfg = cfg;
	        OnClickRecordItemCallback = clickRecordCallback;
	        curDrawState = state;
	        CreateRewardItem();
	        CellItemViewVm.CellItemBaseViewVm.SetSize(ECellItemSizeType.Size90X80);
	        ProgressTextStr = cfg.Level.ToString();
	        ProgressImgPath = isEnd ? "turntable[turntable_progress_2]" : "turntable[turntable_progress_1]";
	        UpdateState();
        }

        private void CreateRewardItem()
        {
	        
	        
	        if (curCfg.OptionalReward!=null && curCfg.OptionalReward.Count > 0)
	        {
		        var selectIndex = DataCenter.luckyDrawData.GetSelectIndex(curCfg.Id);
		        if (curDrawState == EDrawState.DrawMagic)
		        {
			        selectIndex = DataCenter.magicDrawData.GetSelectIndex(curCfg.Id);
		        }
		        CellItemViewVm = SelectCellItemEffectViewModel.Create(curCfg.OptionalReward, ECellItemSizeType.Size90X80, selectIndex: selectIndex);
		        CellItemViewVm.ClickEvent = OnClickSelectReward;
		        CellItemViewVm.CellItemBaseViewVm.State = curState;
		        CellItemViewVm.CellItemBaseViewVm.SetClickAction(curState == ECellItemState.Finish? null: OnClickSelectReward);
		        CellItemViewVm.CellItemBaseViewVm.CellItemBaseViewVm .IsOpenMask = true;
	        }
	        else
	        {
		        if (curCfg != null && curCfg.Reward != null && curCfg.Reward.Count > 0)
		        {
			        var reward = curCfg.Reward[0];
			        CellItemViewVm = SelectCellItemEffectViewModel.Create(reward,ECellItemSizeType.Size90X80);
			        CellItemViewVm.CellItemBaseViewVm.State = curState;
			        CellItemViewVm.CellItemBaseViewVm.CellItemBaseViewVm .IsOpenMask = true;
		        }
	        }
        }

        public void UpdateState()
        {
	        switch (curDrawState)
	        {
		        case EDrawState.DrawLucky:UpdateLuckyDrawState();break;
		        case EDrawState.DrawMagic:UpdateMagicDrawState();break;
		        
	        }
        }

        private void UpdateLuckyDrawState()
        {
	        if(curCfg ==null) return;
	        CurState = ECellItemState.None;
	        CellItemViewVm.CellItemBaseViewVm.SetClickAction(null);
	        // 已领取
	        if (DataCenter.luckyDrawData.CheckIsClaimed(curCfg.Id))
	        {
		        CurState = ECellItemState.Finish;
	        }
	        // 未领取 可以领取
	        else if (DataCenter.luckyDrawData.CheckIsCanClaim(curCfg.Id))
	        {
		        CurState = ECellItemState.GetIng;
		        CellItemViewVm.CellItemBaseViewVm.SetClickAction(OnClickRecordItemBtn);
	        }
	        CellItemViewVm.CellItemBaseViewVm.State = CurState;
	        ProgressValue = DataCenter.luckyDrawData.Progress >= curCfg.Level ? 1 : 0;
        }

        private void UpdateMagicDrawState()
        {
	        if(curCfg ==null) return;
	        CurState = ECellItemState.None;
	       
	        // 已领取
	        if (DataCenter.magicDrawData.CheckIsClaimed(curCfg.Id))
	        {
		        CurState = ECellItemState.Finish;
		        CellItemViewVm.CellItemBaseViewVm.SetClickAction(null);
	        }
	        // 未领取 可以领取
	        else if (DataCenter.magicDrawData.CheckIsCanClaim(curCfg.Id))
	        {
		        CurState = ECellItemState.GetIng;
		        CellItemViewVm.CellItemBaseViewVm.SetClickAction(OnClickRecordItemBtn);
	        } 

	        CellItemViewVm.CellItemBaseViewVm.State = CurState;
	        ProgressValue = DataCenter.magicDrawData.Progress >= curCfg.Level ? 1 : 0;
        }

        private void OnClickRecordItemBtn(Tuple<Vector3, Vector3> info)
        {
	        OnClickRecordItemCallback.Invoke(curCfg.Id);
        }

        private void OnClickSelectReward(Tuple<Vector3, Vector3> info)
        {
	        OnClickSelectReward();
        }
        private void OnClickSelectReward()  
        {
	        if(curCfg == null || curCfg.OptionalReward == null || curCfg.OptionalReward.Count==0) return;
	        var initPos =  DataCenter.luckyDrawData.GetSelectIndex(curCfg.Id);
	        if (curDrawState == EDrawState.DrawMagic)
	        {
		        initPos =  DataCenter.magicDrawData.GetSelectIndex(curCfg.Id);
	        }

	        UIManager.Instance.OpenDialog<CommonSelectRewardView>(new CommonSelectRewardViewModel(curCfg.OptionalReward,initPos
		        ,(selectIndex)=> {
			        if (curDrawState == EDrawState.DrawMagic)
			        {
				        DataCenter.magicDrawData.SetSelectIndex(curCfg.Id,selectIndex);
				        CellItemViewVm.MergeSelect(selectIndex);
			        }
			        else
			        {
				        DataCenter.luckyDrawData.SetSelectIndex(curCfg.Id,selectIndex);
				        CellItemViewVm.MergeSelect(selectIndex);
			        }
		        })).Forget();
        }
    }
}