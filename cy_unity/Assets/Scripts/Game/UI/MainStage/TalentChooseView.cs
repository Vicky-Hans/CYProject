using Cysharp.Threading.Tasks;
using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class TalentChooseView : BaseView
    {
        public override bool FullScreen => false;

        public TextMeshProUGUI talentCanChooseText;
		public UICircularScrollView talentScrollView;
		[AssetPath]public string talentScrollViewCell;
		public DhButton refreshBtn;
		public DhImage adIcon;
		public DhText opBtnText;
		public DhText leftCountText;
		public CommonAdvIconView commonAdv;
		public GameObject effectNode;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
	        talentScrollView.PrefabPath = talentScrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<TalentChooseView, TalentChooseViewModel>();
			bindingSet.Bind(talentCanChooseText).For(v => v.text).To(vm => vm.PageTextStr);
			bindingSet.Bind(talentScrollView).For(v => v.Collection).To(vm => vm.TalentScrollViewList);
			bindingSet.Bind(refreshBtn.gameObject).For(v => v.activeSelf).To(vm => vm.IsCanClickRefreshBtn);
			bindingSet.Bind(refreshBtn).For(v => v.onClick).To(vm => vm.OnClickRefreshBtnCommand);
			bindingSet.Bind(refreshBtn).For(v => v.interactable).ToExpression(vm => SetRefreshBtn( vm.IsCanClickRefreshBtn));
			bindingSet.Bind(adIcon.transform.parent.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowAdIcon);
			bindingSet.Bind(leftCountText).For(v => v.text).To(vm => vm.LeftCountTextStr);
			// bindingSet.Bind(talentScrollView.gameObject).For(v=>v.activeSelf).ToExpression(vm => !vm.IsShowNoTipNode);
			bindingSet.Bind(commonAdv.BindingContext).For(v => v.DataContext).To(vm => vm.CommonAdvVm);
			bindingSet.Bind(effectNode).For(v => v.activeSelf).To(vm => vm.IsShowEffectNode);
            bindingSet.Build();
            AudioManager.Instance.Play(AudioType.OpenUi);

            talentScrollView.m_CanDragScrollView = false;

        }
        public bool SetRefreshBtn(bool isCanClick)
        {
	        if (isCanClick)
	        {
		        // UIHelper.SetGray(refreshBtn.gameObject,false,true,true);
		        // refreshBtn.SetGrayActive(false);
	        }
	        else
	        {
		        // UIHelper.SetGray(refreshBtn.gameObject,true,true,true);
		        // refreshBtn.SetGrayActive(true);
	        }
	        return isCanClick;
        }
    }
}