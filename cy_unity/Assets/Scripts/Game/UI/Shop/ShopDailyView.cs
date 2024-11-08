using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;

namespace DH.Game.UIViews
{
    public partial class ShopDailyView : BaseItemView
    {
        public override bool FullScreen => false;
        public DhText nameText;
        public DhText endTImeValue;
        public ScrollRectExtend scrollViewLock;
        [AssetPath]public string scrollViewLockCell;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            scrollViewLock.PrefabPath = scrollViewLockCell;
            await base.Create();
            var bindSet = this.CreateBindingSet<ShopDailyView, ShopDailyViewModel>();
            bindSet.Bind(endTImeValue).For(v => v.text).To(vm => vm.EndTImeValueStr);
            bindSet.Bind(scrollViewLock).For(v => v.Collection).To(vm => vm.ScrollViewDailyList);
            bindSet.Bind(nameText).For(v => v.text).To(vm => vm.TitleName);
            bindSet.Build();
        }
    }
}