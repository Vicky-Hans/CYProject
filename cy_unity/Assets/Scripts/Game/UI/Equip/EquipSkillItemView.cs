using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class EquipSkillItemView : BaseItemView
    {
        public override bool FullScreen => true;

        public DhImage bg;
        public DhImage icon;
        public DhText desc;
        public GameObject lockMask;
        public ClickTextComponent clickTextComponent;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<EquipSkillItemView, EquipSkillItemViewModel>();
            bindingSet.Bind(bg).For(v => v.sprite).To(vm => vm.BgPath).WithConversion(this);
            bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.TargetIconPath).WithConversion(this);
            bindingSet.Bind(desc).For(v => v.text).To(vm => vm.Desc);
            bindingSet.Bind(icon).For(v => v.Grag).ToExpression(vm => !vm.IsOwn || vm.IsLock);
            bindingSet.Bind(this).For(v => v.clickTextComponent).To(vm => vm.ClickTextCmp).OneWayToSource();
            bindingSet.Build();
        }
    }
}