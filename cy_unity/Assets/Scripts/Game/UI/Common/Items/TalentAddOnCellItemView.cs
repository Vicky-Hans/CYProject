using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class TalentAddOnCellItemView : BaseItemView
    {
        public override bool FullScreen => true;
        
		public DhImage addOnImg;
		public DhText addonText;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<TalentAddOnCellItemView, TalentAddOnCellItemViewModel>();
			bindingSet.Bind(addOnImg).For(v => v.sprite).To(vm => vm.AddOnImgPath).WithConversion(this);
            bindingSet.Bind(addOnImg.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowAddOnImg);
            bindingSet.Bind(addonText).For(v => v.text).To(vm => vm.AddonTextStr);
            bindingSet.Bind(addonText.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowAddOnText);
            bindingSet.Build();
        }
    }
}