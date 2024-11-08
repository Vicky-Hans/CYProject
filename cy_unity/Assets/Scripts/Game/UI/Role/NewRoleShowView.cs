using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class NewRoleShowView : BaseView
    {
        public override bool FullScreen => false;
        
		public GameObject roleSpineGo;
		public DhText heroName;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<NewRoleShowView, NewRoleShowViewModel>();
            
            bindingSet.Bind(roleSpineGo).For(v => v.activeSelf).To(vm => vm.IsShowChapterEffectNode);
            bindingSet.Bind(this).For(v => v.roleSpineGo).To(vm => vm.EffectParentNode).OneWayToSource();
			bindingSet.Bind(heroName).For(v => v.text).To(vm => vm.HeroNameStr);

            bindingSet.Build();
        }
    }
}