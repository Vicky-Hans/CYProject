using System;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class EquipSkillCellItemView : BaseItemView
    {
        public override bool FullScreen => false;
		public DhImage icon;
		public DhButton opBtn;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<EquipSkillCellItemView, EquipSkillCellItemViewModel>();
            bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
            bindingSet.Bind(icon).For(v => v.Grag).To(vm => vm.IsLock);
			bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand).CommandParameter(() => new Tuple<Vector3, Vector3>(icon.transform.position, new Vector3(0,20,0)));
            bindingSet.Build();
        }
    }
}