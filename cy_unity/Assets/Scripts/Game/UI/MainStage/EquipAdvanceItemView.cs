using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class EquipAdvanceItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhImage icon;
		public DhImage skillIcon;
		public DhText nameText;
		public CellItemBaseView skillCellItemView;
		public DhText equipDescText;
		public ScrollRectExtend skillList;
		[AssetPath]public string skillCellViewPrefab;
		public DhButton opBtn;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
	        skillList.PrefabPath = skillCellViewPrefab;
            await base.Create();
            var bindingSet = this.CreateBindingSet<EquipAdvanceItemView, EquipAdvanceItemViewModel>();
			bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
			bindingSet.Bind(skillIcon).For(v => v.sprite).To(vm => vm.PropertyIconPath).WithConversion(this);
			bindingSet.Bind(nameText).For(v => v.text).To(vm => vm.NameTextStr);
			bindingSet.Bind(skillCellItemView.BindingContext).For(v => v.DataContext).To(vm => vm.SkillCellVm);
			bindingSet.Bind(equipDescText).For(v => v.text).To(vm => vm.EquipDescTextStr);
			bindingSet.Bind(skillList).For(v => v.Collection).To(vm => vm.SkillCellVmList);
			bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand);
            bindingSet.Build();
        }
    }
}