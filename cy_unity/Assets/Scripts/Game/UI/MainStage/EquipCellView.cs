using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class EquipCellView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhImage bg;
		public DhImage icon;
		public DhText level;
		public DhImage typeIcon;
		public DhImage propertyIcon;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<EquipCellView, EquipCellViewModel>();
			bindingSet.Bind(bg).For(v => v.sprite).To(vm => vm.BgPath).WithConversion(this);
			bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
			bindingSet.Bind(level).For(v => v.text).To(vm => vm.LevelStr);
			bindingSet.Bind(typeIcon).For(v => v.sprite).To(vm => vm.TypeIconPath).WithConversion(this);
			// bindingSet.Bind(propertyIcon).For(v => v.sprite).To(vm => vm.PropertyIconPath).WithConversion(this);
            bindingSet.Build();
            // typeIcon.gameObject.SetActive(false);
            propertyIcon.gameObject.SetActive(false);
        }
    }
}