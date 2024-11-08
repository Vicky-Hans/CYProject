using Cysharp.Threading.Tasks;
using DH.Game.ViewModels;
using DH.UIFramework;
namespace DH.Game.UIViews.ItemViews
{
    public partial class TabBtnGroupTitleView : BaseItemView
    {
        [AssetPath] public string btnPrefabPath;
        public UICircularScrollView scrollView;

        public override async UniTask Create()
        {
            await base.Create();
            scrollView.PrefabPath = btnPrefabPath;
            var bindSet = this.CreateBindingSet<TabBtnGroupTitleView, TabBtnGroupTitleModel>();
            bindSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.BtnModels);
            bindSet.Build();
        }
    }
}