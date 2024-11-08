using DH.Game.ViewModels;
using DH.UIFramework;
using Spine.Unity;

namespace DH.Game.UIViews
{
    public partial class BossComingView : BaseView
    {
        public override bool FullScreen => false;
        public SkeletonGraphic actionNode;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<BossComingView, BossComingViewModel>();
            bindingSet.Bind(this).For(v => v.actionNode).To(vm => vm.ActionNode).OneWayToSource();
            bindingSet.Build();

            PlayAnimation();
            AudioManager.Instance.Play(AudioType.BossComing);
        }

        private void PlayAnimation()
        {
            actionNode.AnimationState.SetAnimation(0, "animation", false);
        }
    }
}