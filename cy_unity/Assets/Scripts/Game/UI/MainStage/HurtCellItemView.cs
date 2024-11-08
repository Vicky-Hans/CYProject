using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine.UI;
using Extend;
namespace DH.Game.UIViews
{
    public partial class HurtCellItemView : BaseItemView
    {
        public override bool FullScreen => false;
		public CanClaimItemView canClaimItem;
		public Slider progress;
		public DhText progressText;
		public DhText nameText;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
	        progress.interactable = false;
            await base.Create();
            var bindingSet = this.CreateBindingSet<HurtCellItemView, HurtCellItemViewModel>();
			bindingSet.Bind(canClaimItem.BindingContext).For(v => v.DataContext).To(vm => vm.CanClaimItemVm);
			bindingSet.Bind(progress).For(v => v.value).To(vm => vm.ProgressValue);
			bindingSet.Bind(progressText).For(v => v.text).To(vm => vm.ProgressTextStr);
			bindingSet.Bind(nameText).For(v => v.text).To(vm => vm.NameTextStr);

            bindingSet.Build();
        }
    }
}