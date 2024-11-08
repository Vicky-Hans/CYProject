using System;
using Cysharp.Threading.Tasks;
using DH.ComponentUI;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using DH.HotService;
#if DISABLE_NET_MANAGER
using DH.Game.Network;
#else
using DH.Launch.Network;
#endif
using DH.NativeCore;
using DH.Launch;
using DH.NativeCore.Platform;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using DHUnityUtil;
using Google.Protobuf;
using RoleDigest = DH.NativeCore.RoleDigest;

namespace DH.Game.Login
{
    public partial class LoginManager : ObservableSingleton<LoginManager>
    {
        public enum LoginState
        {
            None,
            Logout,
            LoginSdk,
            ReqServerInfos,
            AccountAuth,
            CreateOrLoginRole,
            Success,
        }

#if UNITY_EDITOR
        private const LoginType LoginType = NativeCore.LoginType.Guest;
#else
        private const LoginType LoginType = NativeCore.LoginType.Domestic;
#endif
        
        
        private string curDhToken;
        private GameNetworkManager gameNetwork;
        private LoginListener loginListener;
        private LoginState currentState;
        [AutoNotify]
        private readonly AnnouncementViewModel notice = new AnnouncementViewModel();
        
        public int CurrentAccountId { get; private set; }

        public bool EnableStartGame => CurrentState == LoginState.ReqServerInfos && CurrentServer != null;

        public ObservableDictionary<int, ServerInfo> ServerInfos => serverInfos;

        public LoginState CurrentState
        {
            get => currentState;
            set
            {
                if (!Set(ref currentState, value))
                {
                    return;
                }
                
                RaisePropertyChanged(nameof(EnableStartGame));
            }
        }

        public void Init()
        {
            loginListener = new LoginListener();
            ULogin.SetULoginListener(loginListener);

            gameNetwork = GameNetworkManager.Instance;
            gameNetwork.onDisconnect += OnNetworkDisConnect;
            gameNetwork.onFastConnect += OnFastLoginReply;
            gameNetwork.onServerKick += OnNetworkKickOut;
            gameNetwork.onOtherDeviceLogin += OnOtherDeviceLogin;
            NetworkManager.Instance.SerializerFastRoleLoginHandler = CreateFastLoginRequest;

#if DH_DEBUG
            LoginSdk(LoginType.Guest).Forget();
#else
            LoginSdk(LoginType.Domestic).Forget();
#endif
        }

        public void Destroy()
        {
            gameNetwork.onDisconnect -= OnNetworkDisConnect;
            gameNetwork.onFastConnect -= OnFastLoginReply;
            gameNetwork.onServerKick -= OnNetworkKickOut;
            gameNetwork.onOtherDeviceLogin -= OnOtherDeviceLogin;
            notice.Dispose();
            Reset();
            loginListener = null;
            // 防止报错
            //ULogin.SetULoginListener(null);
        }

        public void Reset()
        {
            CurrentServer = null;
            CurrentAccountId = 0;
        }

        public void StartGame()
        {
            int isApply = DHUnityUtil.PlayerPrefs.GetInt("is_apply_user_protocol", 0);
            if (isApply == 0)
            {
                ToastManager.Show(ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.signin_tips05).Name);
                return;
            }

            if (CurrentState != LoginState.ReqServerInfos)
            {
                return;
            }
            
            
            DoLoginLogicServer().Forget();
        }

        /// <summary>
        /// 登出
        /// </summary>
        public async UniTask Logout()
        {
            ActivityManager.Instance.Show(WaitType.Net);
            var loginResult = await loginListener.Logout();
            ActivityManager.Instance.Hide(WaitType.Net);
            if (HandleLoginSDKResult(loginResult.result))
            {
                GameNetworkManager.Instance.Close();
                Reset();
                ProcedureManager.Instance.Change(nameof(LoginProcedure),true).Forget();
                CurrentState = LoginState.Logout;
                LoginSdk(LoginType).Forget();
            }
        }

        /// <summary>
        /// 重新回到登录界面
        /// </summary>
        public void ResetToLoginMenu()
        {
            DataCenter.clothesData.SaveClothesRedInfo();
            ResetToLoginMenuIntertal().Forget();
        }
        private async UniTaskVoid ResetToLoginMenuIntertal()
        {
            GameNetworkManager.Instance.Close();
            await UIManager.Instance.CloseAllTopDialogAsync(false);
            Reset();
            ProcedureManager.Instance.Change(nameof(LoginProcedure),true).Forget();
            CurrentState = LoginState.LoginSdk;
            RequestServersAndRoles().Forget();
        }

        public async UniTaskVoid RestartGame()
        {
            // ActivityManager.Instance.Show(WaitType.Net);
            // var loginResult = await loginListener.Logout();
            // ActivityManager.Instance.Hide(WaitType.Net);
            Reset();
            gameNetwork.Close();
            ProcedureManager.Instance.Dispose();
        }

        public void RoleEnterGame()
        {
            Usdk.RoleEnterGame(CreateRoleInfo());
        }
        
        /// <summary>
        /// 登录Sdk
        /// </summary>
        /// <param name="loginType"></param>
        public async UniTaskVoid LoginSdk(LoginType loginType,bool autoLoginRole = false)
        {
            ActivityManager.Instance.Show(WaitType.Net);
            CurrentState = LoginState.LoginSdk;
            var loginResult = await loginListener.Login(loginType);
            ActivityManager.Instance.Hide(WaitType.Net);
            if (!HandleLoginSDKResult(loginResult.result))
            {
                return;
            }
            
            curDhToken = loginResult.dhToken;
            if (autoLoginRole)
            {
                await RequestServersAndRoles();
                StartGame();
            }
            else
            {
                RequestServersAndRoles().Forget();
            }
        }

        private async UniTaskVoid DoLoginLogicServer()
        {
            
            
            CurrentState = LoginState.AccountAuth;
            ActivityManager.Instance.Show(WaitType.Net);
            DHLog.Debug($"DoLoginLogicServer {currentState}");
            try
            {
                var tcs = new UniTaskCompletionSource<bool>();
                var entry = StartupEntry.Instance;
                entry.TaskEntry.CheckUpdateForLogin(currentServer.Sid,
                    () =>
                    {
                        tcs.TrySetResult(true);
                        entry.HotUpdateForSid(currentServer.Sid,()=>RestartGame().Forget());
                    },
                    () =>
                    {
                        tcs.TrySetResult(false);
                    });
                var needUpdate = await tcs.Task;
                if (needUpdate)
                {
                    return;
                }

                var result = await GameNetworkManager.Instance.Connect();
                if (!result)
                {
                    CurrentState = LoginState.ReqServerInfos;
                    var msgContent = LocalizeHelper.GetGlobal(GlobalLanguageId.NetworDisconnect);
                    var messageBox = CommonMessageBoxViewModel.CreateConfirmButtonOnly(msgContent, DoLoginLogicServer);
                    UIManager.Instance
                        .OpenDialog<CommonMessageBox>(messageBox, true)
                        .Forget();
                    return;
                }
                await SendAccountAuth();
            }
            finally
            {
                ActivityManager.Instance.Hide(WaitType.Net);
            }
        }

        private void OnNetworkDisConnect()
        {
            CurrentState = LoginState.ReqServerInfos;
            ActivityManager.Instance.Hide(WaitType.Net, true);
        }

        private void OnFastLoginReply(byte[] data)
        {
            var rsp = new RspRoleFastLogin();
            rsp.MergeFrom(data);
            if (rsp.ErrCode == 0)
            {
                DHLog.Debug("快速登录成功");
                // 登录成功
            }
            else
            {
                DHLog.Debug($"快速登录失败:code {rsp.ErrCode} msg {rsp.ErrMsg}");
                // 重新登录
                ResetToLoginMenu();
            }
        }

        private void OnNetworkKickOut()
        {
            CurrentState = LoginState.ReqServerInfos;
            ActivityManager.Instance.Hide(WaitType.Net, true);

            var msgContent = LocalizeHelper.GetGlobal(GlobalLanguageId.NetworkKickOut);
            var messageBox = CommonMessageBoxViewModel.CreateConfirmButtonOnly(msgContent, ResetToLoginMenu);
            UIManager.Instance
                .OpenDialog<CommonMessageBox>(messageBox, true)
                .Forget();
        }

        private void OnOtherDeviceLogin()
        {
            CurrentState = LoginState.ReqServerInfos;
            ActivityManager.Instance.Hide(WaitType.Net, true);

            var msgContent = LocalizeHelper.GetGlobal(GlobalLanguageId.OtherDeviceLogin);
            var messageBox = CommonMessageBoxViewModel.CreateConfirmButtonOnly(msgContent, ResetToLoginMenu);
            UIManager.Instance
                .OpenDialog<CommonMessageBox>(messageBox, true)
                .Forget();
        }

        private void ShowMaintainTip()
        {
            var span = TimeSpan.FromSeconds(lastMaintainFinishTime);
            var time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + span;
            time = time.ToLocalTime();

            var msgContent = LocalizeHelper.GetGlobal(GlobalLanguageId.ServerMaintainTip);
            var tipStr = DHUtility.Format(msgContent,
                time.ToString("yyyy-MM-dd HH:mm:ss"));

            var messageBox = CommonMessageBoxViewModel.CreateConfirmButtonOnly(tipStr,
                () => { DHGameActivityUtil.ExitSuccess(); });

            UIManager.Instance
                .OpenDialog<CommonMessageBox>(messageBox, true)
                .Forget();
        }
        
        private bool HandleLoginSDKResult(ULoginResult loginResult)
        {
            string errorStr;
            switch (loginResult)
            {
                case ULoginResult.Success:
                    return true;
                case ULoginResult.Cancel:
                    errorStr = LocalizeHelper.GetGlobal(GlobalLanguageId.ULoginResult_Cancel);
                    break;
                case ULoginResult.Failed:
                    errorStr = LocalizeHelper.GetGlobal(GlobalLanguageId.ULoginResult_Failed);
                    break;
                case ULoginResult.NetworkRequestError:
                    errorStr = LocalizeHelper.GetGlobal(GlobalLanguageId.ULoginResult_NetworkRequestError);
                    break;
                case ULoginResult.BindInvalidThirdType:
                    errorStr = LocalizeHelper.GetGlobal(GlobalLanguageId.ULoginResult_BindInvalidThirdType);
                    break;
                case ULoginResult.ServerResponseError:
                    errorStr = LocalizeHelper.GetGlobal(GlobalLanguageId.ULoginResult_ServerResponseError);
                    break;
                default:
                    errorStr = $"ULogin错误码:{loginResult}";
                    break;
            }

            DHLog.Error($"[Login] errorCode:{loginResult}\n LogError:{errorStr}");

            var messageBox = CommonMessageBoxViewModel.CreateConfirmButtonOnly(errorStr, ()=> LoginSdk(LoginType).Forget());
            UIManager.Instance
                .OpenDialog<CommonMessageBox>(messageBox, true)
                .Forget();
            return false;
        }

        /// <summary>
        /// 登录帐号
        /// </summary>
        private async UniTask SendAccountAuth()
        {
            ClientInfo info = CreateClientInfo();

            var req = new ReqAccountAuth
            {
                Token = ByteString.CopyFromUtf8(curDhToken),
                Client = info,
                ClusterType = Usdk.RspFullCfg.cluster_type
            };
            var result = await gameNetwork.SendAsync<RspAccountAuth>(req);
            if (!HandleLoginGameError(result.rsp))
            {
                CurrentAccountId = result.rsp.Accountid;
                await LoginOrCreateRole();
            }
        }

        private async UniTask LoginRole()
        {
            var req = new ReqLoginRole
            {
                RoleId = GetCurServerPlayerInfo().RoleId,
                Logicid = CurrentServer.Sid
            };
            var result = await gameNetwork.SendAsync<RspLoginRole>(req);
            if (!HandleLoginGameError(result.rsp))
            {
                HandleLoginFinish();
            }
        }

        /// <summary>
        /// 登录选择的服务器
        /// </summary>
        private async UniTask LoginOrCreateRole()
        {
            CurrentState = LoginState.CreateOrLoginRole;
            if (HasRoleInCurrentServer())
            {
                await LoginRole();
            }
            else
            {
                var req = new ReqCreateRole
                {
                    Logo = 0,
                    Name = string.Empty,
                    Sid = CurrentServer.Sid,
                };
                var result = await gameNetwork.SendAsync<RspCreateRole>(req);
                if (!HandleLoginGameError(result.rsp))
                {
                    if (!playerInfos.ContainsKey(CurrentServer.Sid))
                    {
                        playerInfos.Add(CurrentServer.Sid, new PlayerInfo(new RoleDigest()
                        {
                            roleId = result.rsp.RoleId,
                            name = result.rsp.Name,
                            account = CurrentAccountId,
                        }));
                    }
                    RoleCreate();
                    await LoginRole();
                }
            }
        }

        private byte[] CreateFastLoginRequest()
        {
            var playerInfo = GetCurServerPlayerInfo();
            return new ReqRoleFastLogin()
            {
                Client = CreateClientInfo(),
                Token = ByteString.CopyFromUtf8(curDhToken),
                ClusterType = Usdk.RspFullCfg.cluster_type,
                RoleId = playerInfo.RoleId,
                Sid = CurrentServer.Sid,
                Vsn = HotUpdateUtils.GetVersion(),
                Md5 = HotUpdateUtils.GetHotUpdateMD5(),
                AppName = DeviceUtility.GetBundleId(),
            }.ToByteArray();
        }

        private bool HandleLoginGameError(IMessage rsp)
        {
            string errorStr;
            if (rsp == null)
            {
                errorStr = LocalizeHelper.GetGlobal(GlobalLanguageId.NetworkConnectError);
            }
            else
            {
                var errorNo = (int)(rsp.Descriptor.FindFieldByName("errno")?.Accessor.GetValue(rsp) ?? -1);
                if (errorNo == 0)
                {
                    return false;
                }
                //var errorMsg = rsp.Descriptor.FindFieldByName("errmsg")?.Accessor.GetValue(rsp);
                var errorMsg = GetErrMsg(errorNo);
                errorStr = $"{errorMsg}(code:{errorNo})";
            }
            GameNetworkManager.Instance.Close();
            var msgBox = CommonMessageBoxViewModel.CreateConfirmButtonOnly(errorStr, DoLoginLogicServer);
            UIManager.Instance
                .OpenDialog<CommonMessageBox>(msgBox, true)
                .Forget();
            return true;
        }

        /// <summary>
        /// 角色登录成功即表示可以进入游戏了
        /// </summary>
        private void HandleLoginFinish()
        {
            CurrentState = LoginState.Success;
            ProcedureManager.Instance.Change(nameof(MainGameProcedure),true).Forget();
        }

        /// <summary>
        /// 生成客户端信息
        /// </summary>
        /// <returns></returns>
        private ClientInfo CreateClientInfo()
        {
            return new ClientInfo
            {
                Adid = DeviceUtility.GetGoogleAdvertisingID(),
                Idfv = DeviceUtility.GetIDFV(),
                Dhid = DeviceUtility.GetShuMeiDroidHangID(),
                Imei = DeviceUtility.GetIMEI(),
                AndroidId = DeviceUtility.GetAndroidId(),
                AppsflyerId = DeviceUtility.GetAppflyerID(),
                DeviceToken = DeviceUtility.GetGoogleDeviceToken(),
                MacAddress = DeviceUtility.GetMacAddress(),
                DeviceModel = DeviceUtility.GetDeviceModel(),
                DviceName = DeviceUtility.GetDeviceName(),
                OsVersion = DeviceUtility.GetOS(),
                Language = DeviceUtility.GetLanguage(),
                NetworkType = DeviceUtility.GetNetworkType().ToString(),
                Reserved2 = DeviceUtility.GetOAID(),
                BundleId = DeviceUtility.GetBundleId(),
                Country = DeviceUtility.GetCountry(),
                AppVersion = DeviceUtility.GetAppVersionName(),
                Platform = DeviceUtility.GetPlatform(),
                Channel = "UnityDev",
                Att = Usdk.CallFunction("Utils_checkATTOpened", "") ?? "",
                SubPackage = "",
                ClientSessionId = DH.Log.ULogClient.Instance.ClientSessionID, //BI日志前后端串连标识
            };
        }

        private void RoleCreate()
        {
            Usdk.RoleCreated(CreateRoleInfo());
        }

        private RoleInfo CreateRoleInfo()
        {
            var serverInfo = GetCurrentServerInfo();
            var role = GetCurServerPlayerInfo();
            var info = new RoleInfo
            {
                accountId = role.Account.ToString(),
                sessionId = role.Sid.ToString(),
                sid = serverInfo.Sid.ToString(),
                serverName = serverInfo.Name,
                userId = role.RoleId.ToString(),
                userName = role.Name,
                level = role.Exp.ToString(),
                vip = role.VExp.ToString(),
                avatar = role.Logo.ToString(),
                token = curDhToken,
            };
            
            return info;
        }


        public void ChangeRoleAccount(string account)
        {
            PlayerPrefs.SetString("ULogin_Auto_Login_Key", "");
            
            PlayerPrefs.SetString("CurrentDevAccount_ID", account);
            PlayerPrefs.SetString("ShuMeiID", "");
            PlayerPrefs.Save();
        }

        public void ChangeServer()
        {
            var tmpServer = GetUse1w();
            tmpServer = tmpServer == 0 ? 1 : 0;
            SetUse1w(tmpServer);
            ToastManager.Show("hah~~");
            //ResetToLoginMenu();
        }

        public int GetUse1w()
        {
            return PlayerPrefs.GetInt("login_server_10000", 0);
        }

        public void SetUse1w(int flag)
        {
            PlayerPrefs.SetInt("login_server_10000", flag);
            PlayerPrefs.Save();
        }

        public string GetErrMsg(int errCode)
        {
            var msg = LocalizeHelper.GetGlobal(GlobalLanguageId.NetworkConnectError);
            switch (errCode)
            {
                case -206002:
                case -206040:
                    msg = LocalizeHelper.GetGlobal(GlobalLanguageId.ServerMaintainTip);
                    break;
            }
            return msg;
        }

    }
}