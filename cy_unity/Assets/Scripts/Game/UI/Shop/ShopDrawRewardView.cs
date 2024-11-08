using System;
using DG.Tweening;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using DHFramework;
using Extend;
using Spine;
using Spine.Unity;
using UnityEngine;
using Event = Spine.Event;

namespace DH.Game.UIViews
{
    public partial class ShopDrawRewardView : BaseView
    {
        public override bool FullScreen => false;

        public DhButton btnClose;
		public UICircularScrollView scrollView;
		[AssetPath]public string scrollViewCell;
		public DhButton btnUseItem;
		public DhButton btnFreeItem;
		public ItemPriceNodeView itemPriceNodeView;
		
		public SkeletonGraphic skeletonGraphic;
		public CanvasGroup canvasGroup;

		public ParticleSystem effectTitle;

		[NonSerialized]
		public int endState;
		private bool isPlaying;
		public bool IsPlaying
		{
			get => isPlaying;
			set
			{
				GetPlayState(value);
				isPlaying = value;
			}
		}
        
        public SkeletonDataAsset newSkeletonData1;
        public SkeletonDataAsset newSkeletonData2;

        private TitleShowType titleShowType;
        public TitleShowType TitleShowType
        {
	        get => titleShowType;
	        set
	        {
		        titleShowType = value;
		        SetAnimation(titleShowType);
	        }
        }

        public BoneFollowerGraphic titleNameBone;
        public BoneFollowerGraphic buttonItemBone;
        public BoneFollowerGraphic buttonAdBone;
        public DhText titleName;

        public ParticleSystem mergeSucceed;
		public override async Cysharp.Threading.Tasks.UniTask Create()
        {
			scrollView.PrefabPath = scrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<ShopDrawRewardView, ShopDrawRewardViewModel>();
            
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
			bindingSet.Bind(btnUseItem).For(v => v.onClick).To(vm => vm.OnClickBtnUseItemCommand);
			bindingSet.Bind(itemPriceNodeView.BindingContext).For(v => v.DataContext).To(vm => vm.ItemPriceNodeModel);
			bindingSet.Bind(btnUseItem.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ShopBuyState == ShopBuyState.Item);
			bindingSet.Bind(btnFreeItem.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ShopBuyState == ShopBuyState.Adv || vm.ShopBuyState == ShopBuyState.Free);
			bindingSet.Bind(btnClose).For(v => v.onClick).To(vm => vm.OnClickBtnCloseCommand);
			bindingSet.Bind(btnFreeItem).For(v => v.onClick).To(vm => vm.OnClickBtnFreeCommand);
			bindingSet.Bind(this).For(v => v.IsPlaying).To(vm => ShopManager.Instance.IsShowDrawRewardAnimation);
			bindingSet.Bind(this).For(v => v.TitleShowType).To(vm => vm.TitleShowType);
			bindingSet.Bind(titleName).For(v => v.text).To(vm => vm.TitleName);
			bindingSet.Bind(this).For(v => v.mergeSucceed).To(vm => vm.MergeSucceed).OneWayToSource();
            bindingSet.Build();
    
        }
		
        private void OnAnimationComplete(TrackEntry trackentry)
        {
	        ShopManager.Instance.IsShowDrawRewardAnimation = false;
        }

        private bool GetPlayState(bool isPlay)
        {
	        if (isPlay)
	        {
		        PlaySpine();
	        }

	        return isPlay;
        }

        protected void OnDestroy()
        {
	        skeletonGraphic.AnimationState.Event -= HandleEvent;
	        skeletonGraphic.AnimationState.Complete -= OnAnimationComplete;

        }

        private void PlaySpine()
        {
	        AudioManager.Instance.PlayUIAudio("ui_common_rewardGot");
	        UIHelper.PlayEffect(effectTitle);
	        canvasGroup.alpha = 0;
	        skeletonGraphic.AnimationState.ClearTrack(0);
	        skeletonGraphic.AnimationState.SetAnimation(0, "animation",false);
        }

        private void HandleEvent(TrackEntry trackentry, Event e)
        {
	        DHLog.Debug("BoxUp HandleEvent"+e.Data.Name);
	        if (e.Data.Name == "chuchang")
	        {
		        ShopManager.Instance.IsShowDrawRewardAnimation = false;
		        ScrollAlpha();
	        }
        }

        private void ScrollAlpha()
        {
	        canvasGroup.alpha = 0;
	        // 延迟执行渐变
	        DOTween.To(() => 0, x =>
	        {
		        canvasGroup.alpha = x;
	        }, 1f, 0.3f);
        }

        private void SetAnimation(TitleShowType title)
        {
	        if(title == TitleShowType.Limit)
	        {
		        skeletonGraphic.skeletonDataAsset = newSkeletonData2;
	        }
	        else
	        {
		        skeletonGraphic.skeletonDataAsset = newSkeletonData1;
	        }
	        skeletonGraphic.Initialize(true);
	        titleNameBone.SkeletonGraphic = skeletonGraphic;
	        buttonItemBone.SkeletonGraphic = skeletonGraphic;
	        buttonAdBone.SkeletonGraphic = skeletonGraphic;
	        skeletonGraphic.AnimationState.SetAnimation(0, "animation", false);
	        
	        skeletonGraphic.AnimationState.Event += HandleEvent;
	        skeletonGraphic.AnimationState.Complete += OnAnimationComplete;
        }
    }
}