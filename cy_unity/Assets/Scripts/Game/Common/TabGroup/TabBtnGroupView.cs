using Cysharp.Threading.Tasks;
using DH.Game.ViewModels;
using DH.UIFramework;
namespace DH.Game.UIViews.ItemViews
{
    public partial class TabBtnGroupView : BaseItemView
    {
        [AssetPath] public string btnPrefabPath;
        public ScrollRectExtend scrollView;

        public override async UniTask Create()
        {
            await base.Create();
            scrollView.PrefabPath = btnPrefabPath;
            var bindSet = this.CreateBindingSet<TabBtnGroupView, TabBtnGroupViewModel>();
            bindSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.BtnModels);
            bindSet.Build();
        }
    }
}