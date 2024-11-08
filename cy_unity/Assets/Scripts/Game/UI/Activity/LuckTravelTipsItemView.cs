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
    public partial class LuckTravelTipsItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhText desc;
        public GameObject bg;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<LuckTravelTipsItemView, LuckTravelTipsItemViewModel>();
            bindingSet.Bind(bg).For(v => v.activeSelf).To(vm => vm.IsShowBg);
			bindingSet.Bind(desc).For(v => v.text).To(vm => vm.DescStr);
            bindingSet.Bind(desc).For(v => v.alpha).To(vm => vm.AlphaValue);
            bindingSet.Bind(desc.transform).For(v => v.localScale).To(vm => vm.LocalScale);
            bindingSet.Build();
        }
    }
}