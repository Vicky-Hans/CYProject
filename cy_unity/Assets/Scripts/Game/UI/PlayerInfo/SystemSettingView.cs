using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.NativeCore.Platform;
using UnityEngine.UI;
using UnityEngine;
using Extend;
using TMPro;

namespace DH.Game.UIViews
{
    public partial class SystemSettingView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhText pidNums;
		public DhButton pidBut;
		public DhButton music;
		public DhButton audio;
		public DhButton exit;
		public Text languageTitle;
		public DhButton changeLanguageBut;
		public Button closeBtn;
		public GameObject musicGo;
		public GameObject audioGo;
		
		public TMP_InputField input;
		public Button cdKeyBtn;
		public Button commUnityBtn;
		
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<SystemSettingView, SystemSettingViewModel>();
			bindingSet.Bind(pidNums).For(v => v.text).To(vm => vm.PidNumsStr);
			bindingSet.Bind(music).For(v => v.onClick).To(vm => vm.OnClickMusicCommand);
			bindingSet.Bind(audio).For(v => v.onClick).To(vm => vm.OnClickAudioCommand);
			bindingSet.Bind(exit).For(v => v.onClick).To(vm => vm.OnClickExitCommand);
			bindingSet.Bind(languageTitle).For(v => v.text).To(vm => vm.LanguageTitleStr);
			bindingSet.Bind(changeLanguageBut).For(v => v.onClick).To(vm => vm.OnClickLanguageButCommand);
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtnCommand);
			bindingSet.Bind(musicGo).For(v => v.activeSelf).ToExpression(vm => !vm.MusicState);
			bindingSet.Bind(audioGo).For(v => v.activeSelf).ToExpression(vm => !vm.EffectState);
			bindingSet.Bind(input).For(v => v.text, v => v.onValueChanged).To(vm => vm.InputStr).OneWayToSource();
			bindingSet.Bind(cdKeyBtn).For(v => v.onClick).To(vm => vm.OnClickCdkeyButCommand);
			bindingSet.Bind(commUnityBtn).For(v => v.onClick).To(vm => vm.OnClickCommunityButCommand);
			
            bindingSet.Build();
            if (GameConst.IsIosAuditState)
            {
	            cdKeyBtn.transform.parent.gameObject.SetActive(false);
            }
            pidBut.onClick.AddListener(CopyTextToClipboard);
        }
        public void CopyTextToClipboard()
        {
	        DeviceUtility.CopyToClipboard($"{DataCenter.charcaterData.Digest.RoleId}");
	        var str = LocalizeHelper.GetGlobal(GlobalLanguageId.GiftCode_tips06);
	        ToastManager.Show(str);
        }
        
    }
}