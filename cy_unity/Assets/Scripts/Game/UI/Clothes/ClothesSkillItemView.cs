using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class ClothesSkillItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhImage ownIcon;
		public DhImage lockNode;
		public DhText desc;
		public ClickTextComponent clickText;
		public GameObject mySelf;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<ClothesSkillItemView, ClothesSkillItemViewModel>();
            
			bindingSet.Bind(ownIcon).For(v => v.sprite).To(vm => vm.OwnIconPath).WithConversion(this);
			bindingSet.Bind(lockNode).For(v => v.sprite).To(vm => vm.LockPath).WithConversion(this);
			bindingSet.Bind(ownIcon.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsLock);
			bindingSet.Bind(lockNode.gameObject).For(v => v.activeSelf).To(vm => vm.IsLock);
			bindingSet.Bind(desc).For(v => v.text).To(vm => vm.DescStr);
			bindingSet.Bind(desc).For(v => v.alpha).ToExpression(vm => GetDescAlpha(vm.IsLock));
			bindingSet.Bind(this).For(v => v.clickText).To(vm => vm.ClickTextCmp).OneWayToSource();
			bindingSet.Bind(this).For(v => v.mySelf).To(vm => vm.SelfNode).OneWayToSource();
            bindingSet.Build();
        }

        private float GetDescAlpha(bool isLock)
        {
	        return isLock ? 0.5f : 1f;
        }
    }
}