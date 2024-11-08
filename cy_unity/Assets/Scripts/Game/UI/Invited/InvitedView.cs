using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class InvitedView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhButton invitedBtn;
		public DhButton facebookInvitedBtn;
		public TMP_InputField inputField;
		public DhButton sendBtn;
		public GameObject inputInvitedIdNode;
		public DhText progressText;
		public Slider progressSlider;
		public StaticItemsBindComponent rewardScrollviewList;
		public DhButton closeBtn;
		public DhButton infoBtn;
		
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<InvitedView, InvitedViewModel>();
            bindingSet.Bind(rewardScrollviewList).For(v=>v.BindDictionaryGetValueFunc).To(vm=>vm.GetRewardScrollviewListValue);
            bindingSet.Bind(rewardScrollviewList).For(v => v.Collection).To(vm => vm.RewardScrollviewList);
			bindingSet.Bind(invitedBtn).For(v => v.onClick).To(vm => vm.OnClickInvitedBtnCommand);
			bindingSet.Bind(facebookInvitedBtn).For(v => v.onClick).To(vm => vm.OnClickFacebookInvitedBtnCommand);
			bindingSet.Bind(this).For(v => v.inputField).To(vm => vm.InputField).OneWayToSource();
			bindingSet.Bind(sendBtn).For(v=>v.onClick).To(vm=>vm.OnClickSendBtnCommand);
			bindingSet.Bind(progressText).For(v => v.text).To(vm => vm.ProgressTextStr);
			bindingSet.Bind(progressSlider).For(v=>v.value).To(vm=>vm.ProgressSliderValue);
			bindingSet.Bind(inputInvitedIdNode).For(v => v.activeSelf).To(vm => vm.IsShowInputInvitedIdNode);
			bindingSet.Bind(closeBtn).For(v=>v.onClick).To(vm=>vm.OnCLickCloseBtnCommand);
			bindingSet.Bind(infoBtn).For(v=>v.onClick).To(vm=>vm.OnClickInfoBtnCommand);
            bindingSet.Build();
        }
    }
}