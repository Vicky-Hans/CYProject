using DH.Game.UI;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class SecretTalentChooseView : BaseView
    {
        public override bool FullScreen => false;
        
		public UICircularScrollView talentScrollView;
		[AssetPath]public string talentScrollViewCell;
		public DhButton refreshBtn;
		public CommonAdvIconView adView;
		public DhText leftCountText;
		public DhButton adRefreshBtn;
		public DhText adLeftCountText;
		public CommonAdvIconView adRefreshView;
		public CommonTopView commonTopView;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
			talentScrollView.PrefabPath = talentScrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<SecretTalentChooseView, SecretTalentChooseViewModel>();
			bindingSet.Bind(talentScrollView).For(v => v.Collection).To(vm => vm.TalentScrollViewDictionary);
			bindingSet.Bind(refreshBtn).For(v => v.onClick).To(vm => vm.OnClickConfirmBtnCommand);
			bindingSet.Bind(adView.BindingContext).For(v => v.DataContext).To(vm => vm.AdVm);
			bindingSet.Bind(adView.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowAdIcon);
			bindingSet.Bind(leftCountText).For(v => v.text).To(vm => vm.LeftCountTextStr);
			bindingSet.Bind(adRefreshBtn).For(v=>v.onClick).To(vm=>vm.OnClickAdRefreshBtnCommand);
			bindingSet.Bind(adLeftCountText).For(v => v.text).To(vm => vm.AdLeftCountTextStr);
			bindingSet.Bind(adRefreshView.BindingContext).For(v => v.DataContext).To(vm => vm.AdRefreshVm);
			bindingSet.Bind(adRefreshBtn.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowRefreshBtn);
			bindingSet.Bind(commonTopView.BindingContext).For(v=>v.DataContext).To(vm=>vm.CommonTopVm);
            bindingSet.Build();
            talentScrollView.m_CanDragScrollView = false;
            
            AudioManager.Instance.Play(AudioType.OpenUi);
            
        }
    }
}