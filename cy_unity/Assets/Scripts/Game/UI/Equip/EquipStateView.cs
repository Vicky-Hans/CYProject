using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class EquipStateView : BaseView
    {
        public override bool FullScreen => false;

        public RectTransform bgRectTf;
		public DhButton btnClose;
		public DhImage icon;
		public DhText name;
		public DhText stateName;
		public DhButton btnLeft;
		public DhButton btnRight;
		public GameObject elementNode;
		public DhImage elementIcon;
		public EquipSkillItemView equipSkillView;
		public GameObject maxState;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<EquipStateView, EquipStateViewModel>();

            bindingSet.Bind(btnClose).For(v => v.onClick).To(vm => vm.OnClickCloseCommand);
			bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
			bindingSet.Bind(name).For(v => v.text).To(vm => vm.NameStr);
			bindingSet.Bind(stateName).For(v => v.text).To(vm => vm.StateNameStr);
			bindingSet.Bind(btnLeft).For(v => v.onClick).To(vm => vm.OnClickLeftCommand);
			bindingSet.Bind(btnRight).For(v => v.onClick).To(vm => vm.OnClickRightCommand);
			bindingSet.Bind(equipSkillView.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowSkill);
			bindingSet.Bind(equipSkillView.BindingContext).For(v => v.DataContext).To(vm => vm.EquipSkillViewVm);
			bindingSet.Bind(elementIcon).For(v => v.sprite).To(vm => vm.ElementIconPath).WithConversion(this);
			bindingSet.Bind(btnLeft.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ShowPos>0);
			bindingSet.Bind(btnRight.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ShowPos<vm.ShowEquipList.Count-1);
			bindingSet.Bind(bgRectTf).For(v => v.sizeDelta).ToExpression(vm => GetBgSize(vm.IsShowSkill));
			bindingSet.Bind(maxState).For(v => v.activeSelf).To(vm => vm.IsMaxLevel);
            bindingSet.Build();
        }

        private Vector2 GetBgSize(bool IsShowSkill)
        {
	        return new Vector2(bgRectTf.sizeDelta.x,IsShowSkill?1172:1010);
        }
    }
}