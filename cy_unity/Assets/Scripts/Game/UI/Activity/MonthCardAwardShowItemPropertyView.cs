using DG.Tweening;
using DH.Game.ViewModels;
using DH.UIFramework;
using DHFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class MonthCardAwardShowItemPropertyView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhText text;
        public Transform trans;
        public int Index;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<MonthCardAwardShowItemPropertyView, MonthCardAwardShowItemPropertyViewModel>();
			bindingSet.Bind(text).For(v => v.text).To(vm => vm.TextStr);
            bindingSet.Bind(this).For(v => v.trans).To(vm => vm.Trans);
            bindingSet.Bind(this).For(v => v.Index).To(vm => vm.MIndex);
            bindingSet.Build();
            PlayAni();
        }

        void PlayAni()
        {
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                DOTween.To(() => 0, x => canvasGroup.alpha = x, 1, 0.1f).SetDelay(0.33f +(Index*0.07f));
        }
    }
}