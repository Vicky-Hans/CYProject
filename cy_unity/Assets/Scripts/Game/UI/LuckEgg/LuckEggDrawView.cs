using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Config;
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using UnityEngine;
using Extend;
using Spine.Unity;

namespace DH.Game.UIViews
{
    public partial class LuckEggDrawView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhButton btnTips;
		public DhText timeDesc;
		public DhButton skipBtn;
		public GameObject skipSelect;
		public DhButton btnDrawFree;
		public DhButton btnDrawOne;
		public ItemPriceNodeView oneBtnPriceView;
		public DhButton btnDrawOneUnUse;
		public ItemPriceNodeView oneUnBtnPriceView;
		public DhButton btnDrawBatch;
		public ItemPriceNodeView tenBtnPriceView;
		public DhButton btnDrawBatchUnUse;
		public ItemPriceNodeView tenUnBtnPriceView;
		public DhText rewardAllProgress;
		public UICircularScrollView rewardScrollView;
		[AssetPath]public string rewardScrollViewCell;
		public DhText limitDrawCnt;
		public GameObject drawIng;
		public GameObject skeletonGraphic;
		public CommonTopView commonTopView;
		public DhText drawDescOne;
		public DhText drawDescTen;
		public DhText drawDescOneUnUse;
		public DhText drawDescTenUnUse;

		public SkeletonGraphic drawAnimation;

		public ParticleSystem luckDrawBlue;
		public ParticleSystem luckDrawPurple;
		public ParticleSystem luckDrawRoseate;
		public ParticleSystem luckDrawYellow;
		
		public ParticleSystem luckDrawMove;
		public ParticleSystem luckDrawMoveEnd;
		
		[NonSerialized] public bool moveState;
		public Transform startMovePos;
		public Transform endMovePos;
		private Tween tweenPos;

		public int playEffectIndex;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			rewardScrollView.PrefabPath = rewardScrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<LuckEggDrawView, LuckEggDrawViewModel>();
            bindingSet.Bind(commonTopView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopViewModel);
			bindingSet.Bind(btnTips).For(v => v.onClick).To(vm => vm.OnClickBtnTipsCommand);
			bindingSet.Bind(timeDesc).For(v => v.text).To(vm => vm.TimeDescStr);
			bindingSet.Bind(skipBtn).For(v => v.onClick).To(vm => vm.OnClickSkipBtnCommand);
			bindingSet.Bind(skipSelect).For(v => v.activeSelf).To(vm => vm.IsSkipAnimation);
			bindingSet.Bind(btnDrawFree).For(v => v.onClick).To(vm => vm.OnClickBtnDrawFreeCommand);
			bindingSet.Bind(btnDrawOne).For(v => v.onClick).To(vm => vm.OnClickBtnDrawOneCommand);
			bindingSet.Bind(oneBtnPriceView.BindingContext).For(v => v.DataContext).To(vm => vm.OneBtnPriceViewVm);
			bindingSet.Bind(btnDrawOneUnUse).For(v => v.onClick).To(vm => vm.OnClickBtnDrawOneUnUseCommand);
			
			bindingSet.Bind(btnDrawOne.gameObject).For(v => v.activeSelf).To(vm => vm.IsDrawOne);
			bindingSet.Bind(btnDrawOneUnUse.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsDrawOne);
			
			bindingSet.Bind(oneUnBtnPriceView.BindingContext).For(v => v.DataContext).To(vm => vm.OneBtnPriceViewVm);
			bindingSet.Bind(btnDrawBatch).For(v => v.onClick).To(vm => vm.OnClickBtnDrawBatchCommand);
			bindingSet.Bind(tenBtnPriceView.BindingContext).For(v => v.DataContext).To(vm => vm.TenBtnPriceViewVm);
			bindingSet.Bind(btnDrawBatchUnUse).For(v => v.onClick).To(vm => vm.OnClickBtnDrawBatchUnUseCommand);
			bindingSet.Bind(tenUnBtnPriceView.BindingContext).For(v => v.DataContext).To(vm => vm.TenBtnPriceViewVm);
			
			bindingSet.Bind(btnDrawBatch.gameObject).For(v => v.activeSelf).To(vm => vm.IsDrawTen);
			bindingSet.Bind(btnDrawBatchUnUse.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsDrawTen);
			
			bindingSet.Bind(rewardAllProgress).For(v => v.text).To(vm => vm.RewardAllProgressStr);
			bindingSet.Bind(rewardScrollView).For(v => v.Collection).To(vm => vm.RewardScrollViewList);
			bindingSet.Bind(rewardScrollView).For(v => v.DefaultJumpIndex).To(vm => vm.StartPos);
			bindingSet.Bind(limitDrawCnt).For(v => v.text).To(vm => vm.LimitDrawCntStr);
			bindingSet.Bind(drawIng).For(v => v.activeSelf).To(vm => vm.DrawIng);
			bindingSet.Bind(skeletonGraphic).For(v => v.activeSelf).To(vm => vm.IsShowSkeletonGraphic);

			bindingSet.Bind(drawDescOne).For(v => v.text).ToExpression(vm => GetDrawDesc(vm.OneBtnPriceViewVm.Reward.Count));
			bindingSet.Bind(drawDescTen).For(v => v.text).ToExpression(vm => GetDrawDesc(vm.TenBtnPriceViewVm.Reward.Count));
			bindingSet.Bind(drawDescOneUnUse).For(v => v.text).ToExpression(vm => GetDrawDesc(vm.OneBtnPriceViewVm.Reward.Count));
			bindingSet.Bind(drawDescTenUnUse).For(v => v.text).ToExpression(vm => GetDrawDesc(vm.TenBtnPriceViewVm.Reward.Count));
			bindingSet.Bind(this).For(v => v.drawAnimation).To(vm => vm.SkeletonGraphic).OneWayToSource();
			bindingSet.Bind(this).For(v => v.moveState).ToExpression(vm => MoveAnimationStart(vm.StartMoveAnimation));
			bindingSet.Bind(this).For(v => v.playEffectIndex).ToExpression(vm => PlayAnimationIndex(vm.RandomShowType));
            bindingSet.Build();
        }

        private string GetDrawDesc(long cnt)
        {
		    return LocalizeHelper.GetGlobal(GlobalLanguageId.niudan02, cnt);
        }

        private int PlayAnimationIndex(int index)
        {
	        if (index == 1)
	        {
		        UIHelper.PlayEffect(luckDrawPurple);
	        }else if (index == 2)
	        {
		        UIHelper.PlayEffect(luckDrawYellow);
	        }else if (index == 3)
	        {
		        UIHelper.PlayEffect(luckDrawBlue);
	        }else if (index == 4)
	        {
		        UIHelper.PlayEffect(luckDrawRoseate);
	        }
	        return index;
        }

        private bool MoveAnimationStart(bool state)
        {
	        if (state)
	        {
		        MoveAnimation().Forget();
	        }
	        return state;
        }

        private async UniTaskVoid MoveAnimation()
        {
	        if (tweenPos != null && tweenPos.IsActive())
	        {
		        tweenPos.Kill();
	        }
	        luckDrawMove.transform.position = startMovePos.position;
	        UIHelper.PlayEffect(luckDrawMove);
	        tweenPos = luckDrawMove.transform.DOMove(endMovePos.position, 0.5f);
	        await UniTask.Delay(500);
	        UIHelper.StopEffect(luckDrawMove);
	        UIHelper.PlayEffect(luckDrawMoveEnd);
        }
    }
}