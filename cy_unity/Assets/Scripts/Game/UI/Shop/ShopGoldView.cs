using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;

namespace DH.Game.UIViews
{
    public partial class ShopGoldView : BaseItemView
    {
        public override bool FullScreen => false;
        public DhText nameText;
        public ScrollRectExtend scrollViewCoin;
        [AssetPath]public string scrollViewCoinCell;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {

            scrollViewCoin.PrefabPath = scrollViewCoinCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<ShopGoldView, ShopGoldViewModel>();
            bindingSet.Bind(scrollViewCoin).For(v => v.Collection).To(vm => vm.ScrollViewCoinList);
            bindingSet.Bind(nameText).For(v => v.text).To(vm => vm.TitleName);
            bindingSet.Build();
        }
    }
}