using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class AutumnSpecialOfferView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhText timesDes;
		public UICircularScrollView scrollView;
		[AssetPath]public string scrollViewCell;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollView.PrefabPath = scrollViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<AutumnSpecialOfferView, AutumnSpecialOfferViewModel>();
            
			bindingSet.Bind(timesDes).For(v => v.text).To(vm => vm.TimesDesStr);
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);

            bindingSet.Build();
        }
    }
}