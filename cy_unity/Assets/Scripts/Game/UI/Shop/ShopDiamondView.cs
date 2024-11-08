using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;

namespace DH.Game.UIViews
{
    public partial class ShopDiamondView : BaseItemView
    {
        public override bool FullScreen => false;
        public DhText nameText;
        public ScrollRectExtend scrollViewGem;
        [AssetPath]public string scrollViewGemCell;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {

            scrollViewGem.PrefabPath = scrollViewGemCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<ShopDiamondView, ShopDiamondViewModel>();
            bindingSet.Bind(scrollViewGem).For(v => v.Collection).To(vm => vm.ScrollViewGemList);
            bindingSet.Bind(nameText).For(v => v.text).To(vm => vm.TitleName);
            bindingSet.Build();
        }
    }
}