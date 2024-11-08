using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;

namespace DH.Game.UIViews
{
    public partial class MagicBingoExchangeView : BaseItemView
    {
        public override bool FullScreen => false;
                
        public CommonTopView commonTopItems;
        public UICircularScrollView scrollView;
        [AssetPath]public string scrollViewCell;
        public DhText times;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
            scrollView.PrefabPath = scrollViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<MagicBingoExchangeView, MagicBingoExchangeViewModel>();
            bindingSet.Bind(commonTopItems.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopItemsVm);
            bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
            bindingSet.Bind(times).For(v => v.text).To(vm => vm.TimeDes);
            bindingSet.Build();
        }
    }
}