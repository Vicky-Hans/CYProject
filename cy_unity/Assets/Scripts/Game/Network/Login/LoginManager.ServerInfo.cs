using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using DH.NativeCore;
using DH.UIFramework.Commands;
using DH.UIFramework.Observables;

namespace DH.Game.Login
{
    public class ServerInfoCompare : IComparer
    {
        public static readonly ServerInfoCompare Default = new();

        public int Compare(object x, object y)
        {
            if (!(x is KeyValuePair<int, ServerInfo> lhs) || !(y is KeyValuePair<int, ServerInfo> rhs)) return 0;

            return lhs.Value.CompareTo(rhs.Value);
        }
    }


    public partial class LoginManager
    {
        private readonly ObservableDictionary<int, ServerInfo> serverInfos = new();
        private readonly ObservableDictionary<int, PlayerInfo> playerInfos = new();

        private bool serverMaintaining = false;
        private float requestAgainTime = 0;
        private long lastMaintainFinishTime = 0;
        private ServerInfo currentServer;
        public CancellationTokenSource noticeCts;

        public IComparer ServerComparison => ServerInfoCompare.Default;

        public ICollectionView CollectionView
        {
            get => null;
            set
            {
                if (value == null) return;
                value.Comparer = ServerInfoCompare.Default;
            }
        }

        public ServerInfo CurrentServer
        {
            get => currentServer;
            set
            {
                var oldValue = currentServer;
                if (!Set(ref currentServer, value)) return;

                if (oldValue != null) oldValue.Selected = false;

                if (currentServer != null) currentServer.Selected = true;

                RaisePropertyChanged(nameof(EnableStartGame));
            }
        }

        public int CompareServer(IComparable left, IComparable right)
        {
            return left.CompareTo(right);
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (serverMaintaining)
            {
                requestAgainTime -= realElapseSeconds;

                if (requestAgainTime < 0)
                {
                    serverMaintaining = false;
                    RequestServersAndRoles().Forget();
                    //关闭服务器维护的提示框
                    UIManager.Instance.CloseDialog<CommonMessageBox>();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasRoleInCurrentServer()
        {
            return playerInfos.ContainsKey(CurrentServer?.Sid ?? -1);
        }

        private async UniTask RequestServersAndRoles()
        {
            var result = await Usdk.GetServerList();
            HandleServersAndRoles(result);
            noticeCts ??= new CancellationTokenSource();
            notice.Open(true,noticeCts.Token).Forget();
        }

        private void HandleServersAndRoles(RspServers data)
        {
            if (data.errno != 0)
            {
                var msgContent = LocalizeHelper.GetGlobal(GlobalLanguageId.NetworkConnectError);
                var messageBox = CommonMessageBoxViewModel.CreateConfirmButtonOnly(msgContent, () => RequestServersAndRoles().Forget());
                UIManager.Instance.OpenDialog<CommonMessageBox>(messageBox, true).Forget();
                return;
            }

            if (data.maintain_open)
            {
                serverMaintaining = true;
                lastMaintainFinishTime = data.maintain_finish_time;
                requestAgainTime = 30f;

                ShowMaintainTip();
                return;
            }

            if (data.servers.Length == 0)
            {
                var msgContent = LocalizeHelper.GetGlobal(GlobalLanguageId.ServersAndRolesEmpty);
                var messageBox = CommonMessageBoxViewModel.CreateConfirmButtonOnly(msgContent, () => RequestServersAndRoles().Forget());
                UIManager.Instance.OpenDialog<CommonMessageBox>(messageBox, true).Forget();
                return;
            }

            playerInfos.Clear();
            ServerInfos.Clear();
            CurrentState = LoginState.ReqServerInfos;
            RoleDigest lastLoginRole = null;
            foreach (var roleInfo in data.roles)
            {
                if ((lastLoginRole == null || lastLoginRole.last_online_time < roleInfo.last_online_time)&&roleInfo.sid!=10000)
                    lastLoginRole = roleInfo;

                if (!playerInfos.ContainsKey(roleInfo.sid)) playerInfos.Add(roleInfo.sid, new PlayerInfo(roleInfo));
            }

            var recommendLevel = 0;
            uint cTime = 0;
            var recommendSid = -1;
            foreach (var serverInfo in data.servers)
            {
                if (!ServerInfos.TryGetValue(serverInfo.sid, out var serverInfoModel))
                {
                    serverInfoModel = new ServerInfo(serverInfo);
                    serverInfoModel.SelectCmd = new SimpleCommand(() => { OnSelectServer(serverInfoModel); });
                    ServerInfos.Add(serverInfo.sid, serverInfoModel);
                }

                if (playerInfos.TryGetValue(serverInfo.sid, out var digest))
                    serverInfoModel.Digest = digest.NestMessage;

                if (serverInfo.recommend > recommendLevel ||
                    (serverInfo.recommend == recommendLevel && serverInfo.ctime > cTime))
                {
                    recommendSid = serverInfo.sid;
                    recommendLevel = serverInfo.recommend;
                    cTime = serverInfo.ctime;
                }
            }

            var recordServerId = lastLoginRole?.sid ?? -1;
            var serverId = -1;
            if (recordServerId < 0 || !ServerInfos.ContainsKey(recordServerId))
                serverId = recommendSid > 0 ? recommendSid : -1;
            else if (CurrentServer == null) serverId = recordServerId;

            if (ServerInfos.TryGetValue(serverId, out var server)) CurrentServer = server;

            if (GetUse1w() > 0)
            {
                if (serverInfos.TryGetValue(10000, out var info))
                {
                    CurrentServer = info;
                    // SetUse1w(0);
                }
                else
                {
                    SetUse1w(0);
                }
                
            }

        }

        private void OnSelectServer(ServerInfo serverInfo)
        {
            CurrentServer = serverInfo;
        }

        private bool GetServerInfo(int sid, out ServerInfo serverInfo)
        {
            return ServerInfos.TryGetValue(sid, out serverInfo);
        }

        public ServerInfo GetCurrentServerInfo()
        {
            if (!GetServerInfo(CurrentServer.Sid, out var serverInfo))
                throw new Exception($"Can't get current server information with sid {CurrentServer.Sid}.");

            return serverInfo;
        }

        public bool GetPlayerInfo(int sid, out PlayerInfo playerInfo)
        {
            return playerInfos.TryGetValue(sid, out playerInfo);
        }

        public PlayerInfo GetCurServerPlayerInfo()
        {
            if (!GetPlayerInfo(CurrentServer.Sid, out var playerInfo))
                throw new Exception($"Can't get current server player information with sid {CurrentServer.Sid}.");

            return playerInfo;
        }
    }
}