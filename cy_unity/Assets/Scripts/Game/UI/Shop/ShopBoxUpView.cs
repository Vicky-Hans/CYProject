using DH.Game.ViewModels;
using DH.UIFramework;
using DHFramework;
using Extend;
using Spine;
using Spine.Unity;
using UnityEngine;
using Event = Spine.Event;
using DG.Tweening;
namespace DH.Game.UIViews
{
    public partial class ShopBoxUpView : BaseView
    {
        public override bool FullScreen => false;
		public DhText levelValue;

		public SkeletonGraphic skeletonGraphic;
		public CanvasGroup canvasGroup;
		public ShopBoxRewardItemView shopBoxRewardItemView1;
		public ShopBoxRewardItemView shopBoxRewardItemView2;
		public ShopBoxRewardItemView shopBoxRewardItemView3;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<ShopBoxUpView, ShopBoxUpViewModel>();
			bindingSet.Bind(levelValue).For(v => v.text).To(vm => vm.LevelValue);
			bindingSet.Bind(shopBoxRewardItemView1.BindingContext).For(v => v.DataContext).To(vm => vm.ScrollViewList[0]);
			bindingSet.Bind(shopBoxRewardItemView2.BindingContext).For(v => v.DataContext).To(vm => vm.ScrollViewList[1]);
			bindingSet.Bind(shopBoxRewardItemView3.BindingContext).For(v => v.DataContext).To(vm => vm.ScrollViewList[2]);
            bindingSet.Build();

            ScrollAlpha();
            skeletonGraphic.AnimationState.Event += HandleEvent;
        }

        private void HandleEvent(TrackEntry trackentry, Event e)
        {
	        DHLog.Debug("BoxUp HandleEvent"+e.Data.Name);
        }

        private void ScrollAlpha()
        {
	        AudioManager.Instance.PlayUIAudio("ui_common_levelUp");
	        canvasGroup.alpha = 0;
	        // 延迟执行渐变
	        DOTween.To(() => 0, x =>
		        {
			        canvasGroup.alpha = x;
		        }, 1f, 0.3f)
		        .SetDelay(0.7f);
        }
    }
}