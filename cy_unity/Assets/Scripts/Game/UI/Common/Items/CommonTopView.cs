using DH.Game.ViewModels;
using DH.UIFramework;

namespace DH.Game.UIViews.ItemViews
{
    public partial class CommonTopView : BaseItemView
    {
        public UICircularScrollView items;
        [AssetPath] public string prefabPath;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            items.PrefabPath = prefabPath;
            var bindSet = this.CreateBindingSet<CommonTopView, CommonTopViewModel>();
            bindSet.Bind(items).For(v => v.Collection).To(vm => vm.ResItemsList);
            bindSet.Build();
        }
    }
}