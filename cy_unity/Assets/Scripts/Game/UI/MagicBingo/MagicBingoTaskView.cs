using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;

namespace DH.Game.UIViews
{
    public partial class MagicBingoTaskView : BaseItemView
    {
        public override bool FullScreen => false;
        public CommonTopView commonTopItems;
        public ScrollRectExtend dailyScrollView;
        public ScrollRectExtend activityScrollView;
        [AssetPath]public string scrollViewCell;
        public DhText times;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            dailyScrollView.PrefabPath = scrollViewCell;
            activityScrollView.PrefabPath = scrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<MagicBingoTaskView, MagicBingoTaskViewModel>();
            bindingSet.Bind(commonTopItems.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopItemsVm);
            bindingSet.Bind(dailyScrollView).For(v => v.Collection).To(vm => vm.DailyScrollViewList);
            bindingSet.Bind(activityScrollView).For(v => v.Collection).To(vm => vm.ActivityScrollViewList);
            bindingSet.Bind(times).For(v => v.text).To(vm => vm.TimeDes);
            bindingSet.Build();
        }
    }
}