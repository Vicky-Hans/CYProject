using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class LuckDrawRatioCellView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhImage bg;
		public DhText descText;
		public DhText valueText;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<LuckDrawRatioCellView, LuckDrawRatioCellViewModel>();
			bindingSet.Bind(bg).For(v => v.sprite).To(vm => vm.BgPath).WithConversion(this);
			bindingSet.Bind(descText).For(v => v.text).To(vm => vm.DescTextStr);
			bindingSet.Bind(valueText).For(v => v.text).To(vm => vm.ValueTextStr);
            bindingSet.Build();
        }
    }
}