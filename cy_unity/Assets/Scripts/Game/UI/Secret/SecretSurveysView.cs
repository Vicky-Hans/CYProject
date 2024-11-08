using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using TMPro;

namespace DH.Game.UIViews
{
    public partial class SecretSurveysView : BaseView
    {
        public override bool FullScreen => false;
		public DhButton closeBtn;
		public TextMeshProUGUI content;
        public ClickTextComponent clickText;
        public UICircularScrollView rewardsScrollView;
        [AssetPath] public string rewardPrefab;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            rewardsScrollView.PrefabPath = rewardPrefab;
            await base.Create();
            var bindingSet = this.CreateBindingSet<SecretSurveysView, SecretSurveysViewModel>();
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtnCommand);
            bindingSet.Bind(content).For(v => v.text).To(vm => vm.ContentStr);
            bindingSet.Bind(this).For(v => v.clickText).To(vm => vm.ClickTextCmp).OneWayToSource();
            bindingSet.Bind(rewardsScrollView).For(v => v.Collection).To(vm => vm.RewardsScrollviewList);
            bindingSet.Build();
        }
    }
}