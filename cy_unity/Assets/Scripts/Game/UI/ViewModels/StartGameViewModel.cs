using DH.Game.Login;
using DH.Game.UIViews;
using DH.Launch;
using DH.NativeCore;
using DH.UIFramework;
using DH.UIFramework.Commands;
using DH.UIFramework.ViewModels;
using UnityEngine;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class StartGameViewModel : ViewModelBase
    {
        public LoginManager Login => LoginManager.Instance;
        private readonly SimpleCommand<string> changeBtn;
        [AutoNotify] private bool showAccontNode;

        private ClickTextComponent clickTextCmp;

        public ClickTextComponent ClickTextCmp
        {
            get => null;
            set { 
                clickTextCmp = value;
                if (clickTextCmp != null)
                {
                    clickTextCmp.ClickCallback = OnClickLinkCallback;
                }
            }
        }

        [Preserve]
        public StartGameViewModel()
        {
            this.changeBtn = new SimpleCommand<string>(ClickChangeAccountBtn);
#if DH_DEBUG
            ShowAccontNode = true;
#else
            ShowAccontNode = false;
#endif
        }

        public ICommand OnChangeAccountnBtn
        {
            get { return this.changeBtn; }
        }

        public void ClickChangeAccountBtn(string account)
        {
            if (account != string.Empty && account != "")
            {
                // 切换账号
                Login.ChangeRoleAccount(account);
            }
            Login.LoginSdk(LoginType.Guest).Forget();
        }

        public bool ProtoMark
        {
            get
            {
                int isApply = DHUnityUtil.PlayerPrefs.GetInt("is_apply_user_protocol", 0);
                return isApply != 0;
            }
        }

        public void OnClickProto()
        {
            int isApply = DHUnityUtil.PlayerPrefs.GetInt("is_apply_user_protocol", 0);
            if (isApply == 0)
            {
                isApply = 1;
            }
            else
            {
                isApply = 0;
            }
            DHUnityUtil.PlayerPrefs.SetInt("is_apply_user_protocol", isApply);
            RaisePropertyChanged(nameof(ProtoMark));
        }

        public void OnClickUserProto()
        {
            var url = StartupEntry.Instance.GetUserAgreementUrl();
            Application.OpenURL(url);
        }

        public void OnClickPrivacy()
        {
            var url = StartupEntry.Instance.GetPrivacyAgreement();
            Application.OpenURL(url);
        }
        
        public void OnClickLinkCallback(string linkKey, Vector3 pos)
        {
            var opKey = int.TryParse(linkKey, out int id) ? id : 0;
            if (opKey == 0)return;
            if (opKey == 1)
            {
                OnClickUserProto();
            } else if (opKey == 2)
            {
                OnClickPrivacy();
            }
        }
    }
}