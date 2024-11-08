using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class AttrItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhImage bg;
		public DhImage targetIcon;
		public DhText targetValue;
		public ParticleSystem upEffect;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<AttrItemView, AttrItemViewModel>();
            
			//bindingSet.Bind(bg).For(v => v.sprite).To(vm => vm.BgPath).WithConversion(this);
			bindingSet.Bind(targetIcon).For(v => v.sprite).To(vm => vm.TargetIconPath).WithConversion(this);
			bindingSet.Bind(targetValue).For(v => v.text).To(vm => vm.TargetValueStr);
			bindingSet.Bind(this).For(v => v.upEffect).To(vm => vm.UpEffect).OneWayToSource();
            bindingSet.Build();
        }
    }
}