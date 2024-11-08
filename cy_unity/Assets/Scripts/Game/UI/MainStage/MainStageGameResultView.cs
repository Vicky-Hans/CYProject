using DH.Data;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class MainStageGameResultView : BaseView
    {
        public override bool FullScreen => false;
		public DhText killText;
		public GameObject winNode;
		public GameObject failNode;
		public UICircularScrollView awardScrollView;
		[AssetPath]public string awardScrollViewCell;
		public DhButton doubleBtn;
		public DhText leftCountText;
		public DhButton hurtBtn;
		public ClickToClose clickToClose;
		public DhButton closeBtn;
		public CommonAdvIconView commonAdv;
		public DhButton confirmBtn;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
			awardScrollView.PrefabPath = awardScrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<MainStageGameResultView, MainStageGameResultViewModel>();
			bindingSet.Bind(killText).For(v => v.text).To(vm => vm.KillNumStr);
			bindingSet.Bind(killText.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowDouble);
			bindingSet.Bind(winNode).For(v => v.activeSelf).ToExpression(vm => PlayAction(vm.IsWin,true));
			bindingSet.Bind(failNode).For(v => v.activeSelf).ToExpression(vm => PlayAction(vm.IsWin, false));
			bindingSet.Bind(awardScrollView).For(v => v.Collection).To(vm => vm.AwardScrollViewList);
			bindingSet.Bind(doubleBtn.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowDouble);
			bindingSet.Bind(doubleBtn).For(v => v.onClick).To(vm => vm.OnClickDoubleBtnCommand);
			bindingSet.Bind(doubleBtn).For(v => v.interactable).To(vm => vm.IsCanClickDoubleBtn);
			bindingSet.Bind(doubleBtn.image).For(v => v.sprite).To(vm => vm.DoubleBtnSpritePath).WithConversion(this);
			bindingSet.Bind(leftCountText).For(v => v.text).To(vm => vm.LeftCountTextStr);
			bindingSet.Bind(hurtBtn).For(v => v.onClick).To(vm => vm.OnClickHurtBtnCommand);
			bindingSet.Bind(clickToClose).For(v=>v.CloseCallback).To(vm=>vm.OnClickCloseBtn);
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtn);
			bindingSet.Bind(commonAdv.BindingContext).For(v => v.DataContext).To(vm => vm.CommonAdvVm);
			bindingSet.Bind(confirmBtn).For(v => v.onClick).To(vm => vm.OnClickConfirmBtnCommand);
			bindingSet.Bind(confirmBtn.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowDouble);
			bindingSet.Bind(closeBtn.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowDouble);
			
            bindingSet.Build();
        }
        public bool PlayAction(bool isWin, bool isWinNode)
        {

	        if (isWinNode)
	        {
		        var aniComp = winNode.GetComponent<SkeletonGraphic>();
		        aniComp.AnimationState.SetAnimation(0, GameConst.MapAniName.Enter, false);
		        aniComp.AnimationState.Complete += OnWinComplete;
		        
		        
		        return isWin;
	        }
	        else
	        {
		        var aniComp = failNode.GetComponent<SkeletonGraphic>();
		        aniComp.AnimationState.SetAnimation(0, GameConst.MapAniName.Enter, false);
		        aniComp.AnimationState.Complete += OnFailComplete;
		        
		        return !isWin;	
	        }
        }

        private void OnWinComplete(TrackEntry trackentry)
        {
	        var tempComp = winNode.GetComponent<SkeletonGraphic>();
	        tempComp.AnimationState.SetAnimation(0, GameConst.MapAniName.Idle, true);
	        tempComp.AnimationState.Complete -= OnWinComplete;
        }
        private void OnFailComplete(TrackEntry trackentry)
        {
	        var tempComp = failNode.GetComponent<SkeletonGraphic>();
	        tempComp.AnimationState.SetAnimation(0, GameConst.MapAniName.Idle, true);
	        tempComp.AnimationState.Complete -= OnFailComplete;
        }
        
        public override bool OnPhysicExit()
        {
	        return true;
        }
    }
}