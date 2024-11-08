using DH.UIFramework;

namespace DH.Game.UIViews
{
    public partial class DebugView : BaseView
    {
        public override bool FullScreen => false;
        [AssetPath]
        public string debugCellPrefab;
        public UICircularScrollView scrollview;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            scrollview.PrefabPath = debugCellPrefab;
            await base.Create();
            var bindingSet = this.CreateBindingSet<DebugView,DebugViewModel>();
            bindingSet.Bind(scrollview).For(v => v.Collection).To(vm => vm.DebugList);
            bindingSet.Build();
        }
    }
}