using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ShopDrawClothesView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public ShopBoxItemView shopBoxItemView;
        public DhText titleName;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<ShopDrawClothesView, ShopDrawClothesViewModel>();
            
			bindingSet.Bind(shopBoxItemView.BindingContext).For(v => v.DataContext).To(vm => vm.ShopBoxItemViewVm);
            bindingSet.Bind(titleName).For(v => v.text).To(vm => vm.TitleName);
            bindingSet.Build();
        }
    }
}