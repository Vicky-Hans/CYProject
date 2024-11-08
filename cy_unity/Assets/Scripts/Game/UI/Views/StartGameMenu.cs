using System.Threading;
using Cysharp.Threading.Tasks;
using DH.ComponentUI;
using DH.Config;
using DH.Game.Login;
using DH.UIFramework;
using DH.Game.ViewModels;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PlayerPrefs = DHUnityUtil.PlayerPrefs;

namespace DH.Game.UIViews
{
    public partial class StartGameMenu : BaseView
    {
        public Button startButton;
        public TMP_InputField accountInputField;
        public Button changeAccountBtn;
        public UICircularScrollView serverItems;
        [AssetPath] public string serverItemPrefab;
        public GameObject btnMark;
        public Button protocolBtn;
        public Button noticeBtn;
        // public Button userProtoBtn;
        // public Button privacyBtn;
        public GameObject accountNode;
        public TextMeshProUGUI protoDesc;
        public ClickTextComponent clickTextCmp;

        [SerializeField] private SkeletonGraphic logoEffect;

        public override bool FullScreen => true;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            serverItems.PrefabPath = serverItemPrefab;
            var bindingSet = this.CreateBindingSet<StartGameMenu, StartGameViewModel>();
            bindingSet.Bind(startButton).For(v => v.onClick).To(vm => vm.Login.StartGame);
            bindingSet.Bind(startButton).For(v => v.interactable).To(vm => vm.Login.EnableStartGame);
            bindingSet.Bind(this).For(v => v.serverItems).To(vm => vm.Login.CollectionView).OneWayToSource();
            bindingSet.Bind(serverItems).For(v => v.Collection).To(vm => vm.Login.ServerInfos);
            bindingSet.Bind(changeAccountBtn).For(v => v.onClick).To(vm => vm.OnChangeAccountnBtn).CommandParameter(() => accountInputField.text);
            bindingSet.Bind(btnMark).For(v => v.activeSelf).To(vm => vm.ProtoMark);
            bindingSet.Bind(protocolBtn).For(v => v.onClick).To(vm => vm.OnClickProto);
            // bindingSet.Bind(userProtoBtn).For(v => v.onClick).To(vm => vm.OnClickUserProto);
            // bindingSet.Bind(privacyBtn).For(v => v.onClick).To(vm => vm.OnClickPrivacy);
            bindingSet.Bind(this).For(v => v.clickTextCmp).To(vm => vm.ClickTextCmp).OneWayToSource();
            // bindingSet.Bind(noticeBtn).For(v => v.onClick).ToExpression(vm => PullNotice(false));
            bindingSet.Bind(accountNode).For(v => v.activeSelf).To(vm => vm.ShowAccontNode);
            bindingSet.Build();
            accountInputField.text = PlayerPrefs.GetString("CurrentDevAccount_ID");
            var str1 = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.signin_tips01).Name;
            var str2 = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.signin_tips02).Name;
            var str3 = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.signin_tips03).Name;
            var str4 = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.signin_tips04).Name;

            // MutexStr = LocalizeHelper.GetGlobal(GlobalLanguageId.challenge_19, $" <link={nameCfg.Id}><u>{nameCfg.Name}</u></link> ", $" {cellData.Cfg.LvUnlockId} ");
            protoDesc.text = $"{ str1} <link={1}><u>{str2}</u></link>{str3} <link={2}><u>{str4}</u></link>";
            
            logoEffect.AnimationState.SetAnimation(0, "ui_login_01_01", false);
            
            logoEffect.AnimationState.Complete += OnLogoEffectComplete;
            AudioManager.Instance.PlayMusic("BGM/cy_bgm_ui",1f,1f);
            noticeBtn.onClick.AddListener(() =>
            {
                LoginManager.Instance.noticeCts ??= new CancellationTokenSource();
                var noticeCts = LoginManager.Instance.noticeCts;
                if (noticeCts == null || noticeCts.IsCancellationRequested)
                {
                    return;
                }
                
                LoginManager.Instance.Notice.Open(false,noticeCts?.Token ?? CancellationToken.None).Forget();
            });
        }

        public override void Release()
        {
            if (LoginManager.Instance.noticeCts != null)
            {
                LoginManager.Instance.noticeCts.Cancel();
                LoginManager.Instance.noticeCts = null;
            }
            base.Release();
        }

        private void OnLogoEffectComplete(TrackEntry trackentry)
        {
            if (trackentry.Animation.Name == "ui_login_01_01")
            {
                logoEffect.AnimationState.SetAnimation(0, "ui_login_01_02", true);
            }
        }

        public override bool OnPhysicExit()
        {
            return true;
        }
    }
}