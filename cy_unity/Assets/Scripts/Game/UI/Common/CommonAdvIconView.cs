using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class CommonAdvIconView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhImage icon;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<CommonAdvIconView, CommonAdvIconViewModel>();
            
			bindingSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);

            bindingSet.Build();
        }
    }
}