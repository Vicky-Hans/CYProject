using Cysharp.Threading.Tasks;
using DH.Game;
using Dh.Game.ViewModels;
using DH.UIFramework;

namespace Dh.Game.UIViews.ItemViews
{
    public partial class CommonItemListView : BaseItemView
    {
        public ScrollRectExtend scrollRect;
        [AssetPath] public string prefabPath;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            scrollRect.PrefabPath = prefabPath;
            var bindSet = this.CreateBindingSet<CommonItemListView, CommonItemListModel>();
            bindSet.Bind(scrollRect).For(v => v.Collection).To(vm => vm.ItemListModels);
            bindSet.Build();
        }
    }
}
