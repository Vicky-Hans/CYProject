using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class MainStagePauseView : BaseView
    {
        public override bool FullScreen => false;
        
		public UICircularScrollView awardScrollView;
		[AssetPath]public string awardScrollViewCell;
		public DhButton continueBtn;
		public DhButton hurtBtn;
		public DhButton exitBtn;
		public DhButton effectBtn;
		public DhImage effectTipsImg;
		public DhButton musicBtn;
		public DhImage musicTipsImg;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
			awardScrollView.PrefabPath = awardScrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<MainStagePauseView, MainStagePauseViewModel>();
			bindingSet.Bind(awardScrollView).For(v => v.Collection).To(vm => vm.TalentScrollViewList);
			bindingSet.Bind(continueBtn).For(v => v.onClick).To(vm => vm.OnClickContinueBtnCommand);
			bindingSet.Bind(hurtBtn).For(v => v.onClick).To(vm => vm.OnClickHurtBtnCommand);
			bindingSet.Bind(exitBtn).For(v => v.onClick).To(vm => vm.OnClickExitBtnCommand);
			bindingSet.Bind(effectBtn).For(v => v.onClick).To(vm => vm.OnClickEffectBtnCommand);
			bindingSet.Bind(effectTipsImg.gameObject).For(v=>v.activeSelf).ToExpression(vm => !vm.EffectState);
			bindingSet.Bind(musicBtn).For(v => v.onClick).To(vm => vm.OnClickMusicBtnCommand);
			bindingSet.Bind(musicTipsImg.gameObject).For(v=>v.activeSelf).ToExpression(vm => !vm.MusicState);
            bindingSet.Build();
        }
    }
}