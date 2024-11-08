using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class EquipAdConfirmView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhText title;
		public DhText desc;
		public RandomItemView randomItem;
		public DhButton cancelBtn;
		public DhButton confirmBtn;
		public DhButton closeBtn;
		public DhImage adIcon;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<EquipAdConfirmView, EquipAdConfirmViewModel>();
			// bindingSet.Bind(title).For(v => v.text).To(vm => vm.TitleStr);
			// bindingSet.Bind(desc).For(v => v.text).To(vm => vm.DescStr);
			bindingSet.Bind(randomItem.BindingContext).For(v => v.DataContext).To(vm => vm.RandomItemVm);
			bindingSet.Bind(cancelBtn).For(v => v.onClick).To(vm => vm.OnClickCancelBtnCommand);
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCancelBtnCommand);
			bindingSet.Bind(confirmBtn).For(v => v.onClick).To(vm => vm.OnClickConfirmBtnCommand);
			bindingSet.Bind(adIcon.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowAdIcon);
			
            bindingSet.Build();
        }
    }
}