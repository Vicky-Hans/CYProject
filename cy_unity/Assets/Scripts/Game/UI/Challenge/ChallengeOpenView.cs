using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using Cysharp.Threading.Tasks;
namespace DH.Game.UIViews
{
    public partial class ChallengeOpenView : BaseView
    {
        public override bool FullScreen => false;
        public ScrollRectExtend challengeAwardScroll;
        public DhButton btnClose;
        [AssetPath] public string itemPrefab;
        public override async UniTask Create()
        {
            challengeAwardScroll.PrefabPath = itemPrefab;
            await base.Create();
            var bindingSet = this.CreateBindingSet<ChallengeOpenView, ChallengeOpenViewModel>();
            bindingSet.Bind(challengeAwardScroll).For(v => v.Collection).To(vm => vm.ChallengeAwardList);
            bindingSet.Bind(btnClose).For(v => v.onClick).To(vm => vm.OnClickCloseCommand);
            bindingSet.Build();
        }
    }
}