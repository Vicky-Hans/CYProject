using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.Login;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework.Localization;
using Game.UI.MainUi;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class SystemSettingViewModel : ViewModelBase
    {
	    [AutoNotify] private string pidNumsStr;
		[AutoNotify] private string languageTitleStr;

		[AutoNotify] private  string inputStr;
		public PlayerInfoManager Manager => PlayerInfoManager.Instance;
		
		
		
		#region 音乐 音效
		
		private bool effectState = DataCenter.userinfo.EffectState;

		public bool EffectState
		{
			get => effectState;
			set
			{
				DataCenter.userinfo.EffectState = value;
				AudioManager.Instance.AudioMute = !value;
				Set(ref effectState, value);
			}
		}

		private bool musicState = DataCenter.userinfo.MusicState;

		public bool MusicState
		{
			get => musicState;
			set
			{
				DataCenter.userinfo.MusicState = value;
				AudioManager.Instance.MusicMute = !value;
				Set(ref musicState, value);
			}
		}
		[Command]
		private void OnClickMusic()
		{
			MusicState = !MusicState;
		}

		[Command]
		private void OnClickAudio()
		{
			EffectState = !EffectState;
	        
		}

		#endregion
		
		[AutoNotify] private ObservableList<CellItemViewModel> awardItemsList = new ();
		[AutoNotify] private string kayString;
        [Preserve]
        public SystemSettingViewModel()
        {
	        PidNumsStr = DataCenter.charcaterData.Digest.RoleId.ToString();
	        var currentCode = Localization.GetCurrentLanguage();
	        languageTitleStr = Manager.supportLanguage[currentCode];
        }

        

		[Command]
		private void OnClickExit()
		{           
			var str= LocalizeHelper.GetGlobal(GlobalLanguageId.Net_Logout_tips);
			var messageBox = CommonMessageBoxViewModel.CreateCommonMsgBox(str, LoginManager.Instance.ResetToLoginMenu,() => { }); 
			UIManager.Instance.OpenDialog<CommonMessageBox>(messageBox, true).Forget();
		}

		[Command]
		private void OnClickCloseBtn()
		{
			UIManager.Instance.CloseDialog<SystemSettingView>();
		}
        
		[Command]
		private void OnClickLanguageBut()
		{
			UIManager.Instance.OpenDialog<SelectLanguageView,SelectLanguageViewModel>().Forget();
		}
		[Command]
		private async void OnClickCdkeyBut()
		{
			if (!CheckIsCDKeyIsOpen())
			{
				var cfg = ConfigCenter.FunctionOpenCfgColl.GetDataById((int)EFunctionOpenType.RedeemCode);
				var str = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips04);
				ToastManager.Show(string.Format(str,cfg.Stage));
				return;
			}
			if(InputStr == "")
			{
				AudioManager.Instance.PlayWrongTips();
				return;
			}
			var cdKey = InputStr;
			//这里请求服务器验证
			var req = new ReqRedeemExchange();
			req.Code = cdKey;

			var result = await GameNetworkManager.Instance.SendAsync<RspRedeemExchange>(req);
			if (result.rsp is not { Status: 0 })
			{
				if(result.rsp == null) return;
				var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
				ToastManager.Show(str);
				AudioManager.Instance.PlayWrongTips();
				return;
			}
			Lodash.DealRewards(result.rsp.Rewards.ToList());
			UIHelper.OpenCommonRewardView(result.rsp.Rewards.ToList());
		}
		private bool CheckIsCDKeyIsOpen()
		{
			return  MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.RedeemCode);;
		}
		[Command]
		private async void OnClickCommunityBut()
		{
			Application.OpenURL(GameConst.DiscordUrl);
		}
		
		
		
		// [Command]
		// private async void OnClickGoCommunityBut()
		// {
		// 	DeviceUtility.CopyToClipboard(DataCenter.charcaterData.DiscordCode);
		// 	var str = LocalizeHelper.GetGlobal(GlobalLanguageId.GiftCode_tips06);
		// 	ToastManager.Show(str);
		// 	// 使用默认浏览器打开链接
		// 	Application.OpenURL(GameConst.DiscordUrl);
		// }

    }
}