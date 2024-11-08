using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using Cysharp.Threading.Tasks;
namespace DH.Game.UIViews
{
    public partial class SecretOpenView : BaseView
    {
        public override bool FullScreen => false;
        public UICircularScrollView rewardsScrollView;
        [AssetPath] public string rewardPrefab;
        public DhButton opBtn;
        public override async UniTask Create()
        {
            rewardsScrollView.PrefabPath = rewardPrefab;
            await base.Create();
            var bindingSet = this.CreateBindingSet<SecretOpenView, SecretOpenViewModel>();
            bindingSet.Bind(rewardsScrollView).For(v => v.Collection).To(vm => vm.RewardsScrollviewList);
            bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand);
            bindingSet.Build();
        }
    }
}