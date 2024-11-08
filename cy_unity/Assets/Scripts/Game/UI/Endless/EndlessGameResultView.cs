using DH.Data;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using Spine;
using Spine.Unity;
using UnityEngine;
using Cysharp.Threading.Tasks;
namespace DH.Game.UIViews
{
    public partial class EndlessGameResultView : BaseView
    {
	    public override bool FullScreen => false;
	    public DhButton hurtBtn;
	    public DhButton closeBtn;
	    public DhText killCountText;
	    public DhText timeCountText;
	    public GameObject winNode;
	    public GameObject newRecordCoin;
	    public GameObject newRecordTime;
	    public GameObject rewardListNode;
	    public UICircularScrollView awardScrollView;
		[AssetPath]public string awardScrollViewCell;
		public ClickToClose clickToClose;
        public override async UniTask Create()
        {
			awardScrollView.PrefabPath = awardScrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<EndlessGameResultView, EndlessGameResultViewModel>();
			bindingSet.Bind(winNode).For(v => v.activeSelf).ToExpression(vm => PlayAction(vm.IsWin));
			bindingSet.Bind(awardScrollView).For(v => v.Collection).To(vm => vm.AwardScrollViewList);
			bindingSet.Bind(killCountText).For(v => v.text).To(vm => vm.KillCountTextStr);
			bindingSet.Bind(timeCountText).For(v => v.text).To(vm => vm.TimeCountTextStr);
			bindingSet.Bind(newRecordCoin).For(v => v.activeSelf).To(vm => vm.IsNewCoin);
			bindingSet.Bind(newRecordTime).For(v => v.activeSelf).To(vm => vm.IsNewTime);
			bindingSet.Bind(hurtBtn).For(v => v.onClick).To(vm => vm.OnClickHurtBtnCommand);
			bindingSet.Bind(clickToClose).For(v=>v.CloseCallback).To(vm=>vm.OnClickCloseBtn);
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtn);
			bindingSet.Bind(rewardListNode).For(v => v.activeSelf).To(vm => vm.IsShowAwardNode);
            bindingSet.Build();
        }
        private bool PlayAction(bool isWin)
        {
	        var aniComp = winNode.GetComponent<SkeletonGraphic>();
	        aniComp.AnimationState.SetAnimation(0, GameConst.MapAniName.Enter, false);
	        aniComp.AnimationState.Complete += OnWinComplete;
	        AudioManager.Instance.Play(AudioType.GameWin);
	        return isWin;
        }

        private void OnWinComplete(TrackEntry trackentry)
        {
	        var tempComp = winNode.GetComponent<SkeletonGraphic>();
	        tempComp.AnimationState.SetAnimation(0, GameConst.MapAniName.Idle, true);
	        tempComp.AnimationState.Complete -= OnWinComplete;
        }
        public override bool OnPhysicExit()
        {
	        return true;
        }
    }
}