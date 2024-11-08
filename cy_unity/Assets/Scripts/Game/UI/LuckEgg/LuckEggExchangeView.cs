using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;

namespace DH.Game.UIViews
{
    public partial class LuckEggExchangeView : BaseItemView
    {
        public override bool FullScreen => false;
                
        public CommonTopView commonTopItems;
		public UICircularScrollView scrollView;
		[AssetPath]public string scrollViewCell;
        public DhText times;
        public DhImage cosImg;
        public DhText cosNums;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollView.PrefabPath = scrollViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<LuckEggExchangeView, LuckEggExchangeViewModel>();
            bindingSet.Bind(commonTopItems.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopItemsVm);
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
            bindingSet.Bind(times).For(v => v.text).To(vm => vm.TimeDes);
            bindingSet.Bind(cosNums).For(v => v.text).ToExpression(vm => vm.CosNums);
            bindingSet.Bind(cosImg).For(v => v.sprite).To(vm => vm.CosImgPath).WithConversion(this);
            bindingSet.Build();
        }
    }
}