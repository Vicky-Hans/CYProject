using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using Spine.Unity;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class NewFunctionUnlockView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhImage icon;
		public DhText title;
		public DhButton gotoBut;
		public SkeletonGraphic spine;
		
	    public CanvasGroup cg0;
	    public CanvasGroup cg1;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<NewFunctionUnlockView, NewFunctionUnlockViewModel>();
            
			bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
			bindingSet.Bind(title).For(v => v.text).To(vm => vm.TitleStr);
			bindingSet.Bind(gotoBut).For(v => v.onClick).To(vm => vm.OnClickGotoButCommand);

            bindingSet.Build();
            
            PlayIdle();
            PlayAni();
        }
        private async void PlayIdle()
        {
	        if (spine == null)return;
	        spine.AnimationState.SetAnimation(0, "chuchang", false);
	        await UniTask.Delay(1000);
	        if (spine == null)return;
	        spine.AnimationState.SetAnimation(0, "idle", true);
        }

        public void PlayAni()
        {
	        // 定义起始值和目标值
	        float startValue = 0f;
	        float endValue = 1f;

	        // 创建插值动画，从起始值到目标值，在2秒内完成
	        DOTween.To(() => startValue, x => startValue = x, endValue, 0.27f)
		        .OnUpdate(() =>
		        {
			        cg0.alpha = startValue;
		        }).SetDelay(0.57f);
	        
	        float startValue2 = 0f;
	        DOTween.To(() => startValue2, x => startValue2 = x, endValue, 0.2f)
		        .OnUpdate(() =>
		        {
			        cg1.alpha = startValue;
		        }).SetDelay(0.8f);
        }
    }
}