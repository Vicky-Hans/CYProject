#if DISABLE_NET_MANAGER
using System;
using System.Collections.Generic;
using System.Net;
using DH.NativeCore.MonoSingleton;
using DH.UNet;
using DHFramework;
using UnityEngine;

namespace DH.Game.Network
{
    public class NetworkManager : MonoSingleton<NetworkManager>
    {
        private class MessagePeer : IReference
        {
            public ConnectWrap Wrap;
            public IUNetIncomingMessage Message;

            public static MessagePeer Create(ConnectWrap wrap, IUNetIncomingMessage message)
            {
                var item = ReferencePool.Acquire<MessagePeer>();
                item.Wrap = wrap;
                item.Message = message;
                return item;
            }

            public void Clear()
            {
                Wrap = null;
                Message = null;
            }
        }

        private static readonly byte FastReconnectReqGroup = 1;
        private static readonly byte FastReconnectReqCmd = 3;
        
        private UNetClient netClient;
        private Dictionary<string, ConnectWrap> networkConnectDic;
        private Dictionary<string, ConnectWrapEvent> connectWrapEventDic;
        private UnityThreadModule threadModule;
        private List<MessagePeer> messagePool = new List<MessagePeer>(10);
        private Func<byte[]> serializerFastRoleLoginHandler;

        public UNetClient NetClient => netClient;

        public Func<byte[]> SerializerFastRoleLoginHandler
        {
            get => serializerFastRoleLoginHandler;
            set => serializerFastRoleLoginHandler = value;
        }

        protected override void Init()
        {
            threadModule = new UnityThreadModule();
            networkConnectDic = new Dictionary<string, ConnectWrap>();
            connectWrapEventDic = new Dictionary<string, ConnectWrapEvent>();
            netClient = UNetClient.Instance;
            netClient.Init();

#if UNITY_WEBGL || WECHAT_MINI
            UNetConfiguration configuration = new UNetConfiguration
            {
                ServiceType = UNetServiceType.WebsocketService,
                ThreadName = "NetworkManager"
            };
#else
            UNetConfiguration configuration = new UNetConfiguration
            {
                ServiceType = UNetServiceType.TCPService,
                ThreadName = "NetworkManager"
            };
#endif

            netClient.SetupService(configuration);
            netClient.Start();
        }

        protected override void Release()
        {
            networkConnectDic.Clear();
            networkConnectDic = null;

            netClient.Shutdown();
            netClient = null;
        }

        private void Update()
        {
            if(networkConnectDic == null)return;
            foreach (var connect in networkConnectDic)
            {
                UpdateConnectWrap(connect.Value);
            }

            // 缓存消息后进行处理，防止迭代器失效
            if (messagePool.Count > 0)
            {
                foreach (var item in messagePool)
                {
                    try
                    {
                        if (networkConnectDic.ContainsKey(item.Wrap.ConnectId))
                        {
                            HandleIncomingMessage(item.Wrap, item.Message);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    finally
                    {
                        netClient.RecycleMessage(item.Wrap.ConnectId, item.Message);

                        ReferencePool.Release(item);
                    }
                }

                messagePool.Clear();
            }

            threadModule.Update(Time.deltaTime, Time.unscaledTime);
        }

        private void UpdateConnectWrap(ConnectWrap connectWrap)
        {
            IUNetIncomingMessage incomingMessage = null;
            while ((incomingMessage = netClient.ReadMessageAsync(connectWrap.ConnectId)) != null)
            {
                messagePool.Add(MessagePeer.Create(connectWrap, incomingMessage));
            }
        }

        private void HandleIncomingMessage(ConnectWrap connectWrap, IUNetIncomingMessage message)
        {
            switch (message.MessageType)
            {
                case UNetIncomingMessageType.DebugMessage:
                case UNetIncomingMessageType.WarningMessage:
                    DHLog.Debug(
                        $"Debug Info :connectId: {connectWrap.ConnectId},  {message.ErrorCode} : {message.ErrorMsg}");
                    break;

                case UNetIncomingMessageType.ErrorMessage:
                    //HandleIncomingMessageError(connectWrap, message);
                    // 忽略错误信息，只关注连接状态
                    DHLog.Debug(
                        $"Debug Info :connectId: {connectWrap.ConnectId},  {message.ErrorCode} : {message.ErrorMsg}");
                    // DNS解析错误时，连接状态还未建立，无法在StatusChange中进行处理，需要单独处理
                    if (message.ErrorCode == UNetErrorCode.DNSParseDomainNameError || message.ErrorCode == UNetErrorCode.PeerDisconnect)
                    {
                        connectWrap.CanSendMsg = false;
                        connectWrap.Reconnecting = false;
                        var connectWrapEvent = this.GetConnectWrapEvent(connectWrap.ConnectId);
                        DHLog.Debug("[NetworkComponent] DNS解析错误,等待下次发送消息时,再次自动重连");
                        connectWrapEvent?.RaiseDisconnectEvent(message.ErrorCode);
                    }
                    break;

                case UNetIncomingMessageType.Data:
                    HandleIncomingMessageData(connectWrap, message.Group, message.CMD, message.Data);
                    break;

                case UNetIncomingMessageType.StatusChange:
                    HandleConnectStatusChange(connectWrap, message);
                    break;

                case UNetIncomingMessageType.HeartBeatMessage:
                    UpdateHeartbeatSyncServerTimestamp(connectWrap, message);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void HandleIncomingMessageData(ConnectWrap connectWrap, byte group, byte cmd, byte[] data)
        {
            //data根据协议应该使用 PB 反序列化
            DHLog.Debug($"HandleIncomingMessageData group {group} cmd {cmd}");
            var connectWrapEvent = GetConnectWrapEvent(connectWrap.ConnectId);
            if(group == FastReconnectReqGroup && cmd == FastReconnectReqCmd)
            {
                connectWrapEvent?.RaiseHandleFastConnectedEvent(data);
                return;
            }
            
            connectWrapEvent?.RaiseHandleReceiveMessageEvent(group, cmd, data);
        }

        private void UpdateHeartbeatSyncServerTimestamp(ConnectWrap connectWrap, IUNetIncomingMessage message)
        {
            //DHLog.Debug($"Sync Server Timestamp : {connectWrap.ConnectId}, {message.HeartbeatSyncTimestamp} ms");

            connectWrap.ServerTimestamp = message.HeartbeatSyncTimestamp;
        }

        /// <summary>
        /// 如果上一次是在Connecting，VerifyConnecting，Connected任何一种状态，当断开时，都应该调用DisconnectEvent事件
        /// </summary>
        /// <returns></returns>
        private bool LastConnectStatusIsTryOrConnect(ConnectWrap connectWrap)
        {
            return connectWrap.LastConnectStatus == UNetConnectStatus.Connected ||
                   connectWrap.LastConnectStatus == UNetConnectStatus.Connecting ||
                   connectWrap.LastConnectStatus == UNetConnectStatus.VerifyConnecting ||
                   connectWrap.LastConnectStatus == UNetConnectStatus.Reconnecting;
        }

        private void HandleConnectStatusChange(ConnectWrap connectWrap, IUNetIncomingMessage message)
        {
            DHLog.Debug(" HandleConnectStatusChange : " + message.ConnectStatus);

            UNetConnectStatus status = message.ConnectStatus;

            DHLog.Debug(
                $"connectId: {connectWrap.ConnectId}, LastConnectStatus : {connectWrap.LastConnectStatus}, ConnectStatus : {status}");

            if (status == connectWrap.LastConnectStatus)
            {
                return;
            }

            if (connectWrap.LastConnectStatus == UNetConnectStatus.Reconnecting &&
                status == UNetConnectStatus.Connected)
            {
                //可以关闭重连的UI
                DHLog.Debug($"connectId: {connectWrap.ConnectId}, 重连完成了，可以关闭网络UI了...");
                connectWrap.CanSendMsg = true;
                connectWrap.Reconnecting = false;
            }
            else if (connectWrap.LastConnectStatus != UNetConnectStatus.Connected &&
                     status == UNetConnectStatus.Connected)
            {
                DHLog.Debug("ConnectedEvent?.Invoke();");
                // 连接成功
                connectWrap.CanSendMsg = true;
                connectWrap.Reconnecting = false;
                
                if(connectWrap.SendFastReconnect)
                {
                    // 发送快速登录请求
                    connectWrap.SendFastReconnect = false;
                    connectWrap.ServerRefuse = false;
                    DHLog.Debug("[NetworkComponent] 快速登录连接成功，自动发送登录消息");   
                    if(this.SerializerFastRoleLoginHandler != null){
                        var data = this.SerializerFastRoleLoginHandler();
                        this.netClient.Send(connectWrap.ConnectId, FastReconnectReqGroup, FastReconnectReqCmd, data);
                    }
                }
                else
                {
                    var connectWrapEvent = this.GetConnectWrapEvent(connectWrap.ConnectId);
                    connectWrapEvent?.RaiseConnectedEvent();
                }
            }
            else if (LastConnectStatusIsTryOrConnect(connectWrap) && status == UNetConnectStatus.Disconnected)
            {
                connectWrap.CanSendMsg = false;
                connectWrap.Reconnecting = false;
                var connectWrapEvent = this.GetConnectWrapEvent(connectWrap.ConnectId);
                if(message.ServerRefuse)
                {
                    // 快速重连，业务层无感
                    this.netClient.Close(connectWrap.ConnectId);
                    // 标记需要发送快速登录请求
                    connectWrap.SendFastReconnect = true;
                    connectWrap.ServerRefuse = true;   
                    DHLog.Debug("[NetworkComponent] 服务器拒绝重连，等待下次发送消息时，自动启动快速登录逻辑");   
                    connectWrapEvent?.RaiseDisconnectEvent(message.ErrorCode);
                }
                else
                {
                    DHLog.Debug("[NetworkComponent] 等待下次发送消息时,再次自动重连");
                    connectWrapEvent?.RaiseDisconnectEvent(message.ErrorCode);
                }
            }
            else if (connectWrap.LastConnectStatus != UNetConnectStatus.Reconnecting &&
                     status == UNetConnectStatus.Reconnecting)
            {
                //可以打开重连的UI
                DHLog.Debug($"connectId: {connectWrap.ConnectId}开始重连了...");

                connectWrap.CanSendMsg = false;
                connectWrap.Reconnecting = true;
            }

            connectWrap.LastConnectStatus = status;
        }

        public string GetConnectIP(string connectId)
        {
            if (!networkConnectDic.TryGetValue(connectId, out var connectWrap))
            {
                return "";
            }

            return netClient.GetConnectIP(connectId);
        }

        public long GetServerTimestamp(string connectId)
        {
            if (!networkConnectDic.TryGetValue(connectId, out var connectWrap))
            {
                return 0;
            }

            return connectWrap.ServerTimestamp;
        }

        public bool CheckServerRefuseReconnect(string connectId)
        {
            if (!networkConnectDic.TryGetValue(connectId, out var connectWrap))
            {
                return false;
            }

            return connectWrap.ServerRefuse;
        }
        
        public bool Reconnect(string connectId,bool supportFastLogin)
        {
            if (!networkConnectDic.TryGetValue(connectId, out var connectWrap))
            {
                return false;
            }

            if (!supportFastLogin)
            {
                this.netClient.Reconnect(connectId);
                return false;
            }

            // 若支持快速登录，直接快速登录，不再检查服务器拒绝重连与否
            connectWrap.ServerRefuse = false;
            connectWrap.SendFastReconnect = true;
            connectWrap.LastConnectStatus = UNetConnectStatus.InvalidConnect;
            this.netClient.Close(connectWrap.ConnectId);
            this.netClient.ConnectAsync(connectWrap.Config, connectWrap.Address, connectWrap.Port);

            return true;
        }

        public void Connect(string connectId, string ip, int port)
        {
            if (networkConnectDic.ContainsKey(connectId))
            {
                Debug.Log($" already connect {connectId}");
                return;
            }

#if UNITY_WEBGL || WECHAT_MINI
        UNetConnectConfiguration configuration =
            new UNetConnectConfiguration(UNetServiceType.WebsocketService, connectId);
#else
            UNetConnectConfiguration configuration =
                new UNetConnectConfiguration(UNetServiceType.TCPService, connectId);
#endif

            configuration.EnableMessageType(UNetIncomingMessageType.DebugMessage);
            configuration.EnableMessageType(UNetIncomingMessageType.WarningMessage);
            configuration.EnableMessageType(UNetIncomingMessageType.ErrorMessage);
            configuration.EnableSilenceReconnect = true;
            configuration.HeartBeatInterval = 10;
            configuration.MaxHeartBeatMissCount = 3;
            configuration.ConnectTimeout = 10;

            Connect(configuration, ip, port);
        }

        public void Connect(UNetConnectConfiguration configuration, string ip, int port)
        {
            var connectId = configuration.UniqueIdentifier;

            if (networkConnectDic.ContainsKey(connectId))
            {
                Debug.Log($" already connect {connectId}");
                return;
            }

            DHLog.Debug(ip);

            if (IPAddress.TryParse(ip, out IPAddress ipAddress))
            {
                netClient.Connect(configuration, ipAddress, port);
            }
            else
            {
                netClient.ConnectAsync(configuration, ip, port);
            }

            //netClient.Connect(configuration, "ih-dev-gate.dhgames.cn", 18887);
            //netClient.Connect(configuration, "www.baidu.com", 18887);
            //netClient.Connect(configuration, "ugate-clienttest.dhgames.cn", 18889);

            var container = networkConnectDic;
            threadModule.RunOnNextFrame(() => { container.Add(connectId, new ConnectWrap(connectId,ip,port,configuration)); });
        }

        public bool CheckSendState(string uniqueIdentifier)
        {
            if (!networkConnectDic.TryGetValue(uniqueIdentifier,out var connectWrap)) 
            {
                return false;
            }

            if (!connectWrap.CanSendMsg)
            {
                if(connectWrap.ServerRefuse)
                {
                    DHLog.Debug("[NetworkComponent] 服务器拒绝重连,需要让玩家手动重连");   
                }
                else if(!connectWrap.Reconnecting)
                {
                    DHLog.Debug("[NetworkComponent] 需要让玩家手动重连");   
                }
                else
                {
                    DHLog.Debug($"[NetworkComponent] 当前无法发送消息,连接已经断开,当前重连状态${connectWrap.Reconnecting}");   
                }
            }
            
            return connectWrap.CanSendMsg;
        }

        public bool Send(string uniqueIdentifier, byte group, byte cmd, byte[] data)
        {
            if (!networkConnectDic.TryGetValue(uniqueIdentifier,out var connectWrap)) 
            {
                return false;
            }
            
            if(!connectWrap.CanSendMsg)
            {
                if(connectWrap.ServerRefuse)
                {
                    DHLog.Debug("[NetworkComponent] 服务器拒绝重连,需要让玩家手动重连");   
                }
                else if(!connectWrap.Reconnecting)
                {
                    DHLog.Debug("[NetworkComponent] 需要让玩家手动重连");   
                }
                else
                {
                    DHLog.Debug($"[NetworkComponent] 当前无法发送消息,连接已经断开,当前重连状态${connectWrap.Reconnecting}");   
                }
                return false;
            }

            var result = netClient.Send(uniqueIdentifier, group, cmd, data);
            return result != UNetSendResult.FailedNotConnected;
        }

        public void Close(string connectId, Action closeCallback = null)
        {
            DHLog.Debug($" Close {connectId}");

            netClient.Close(connectId);
            networkConnectDic.Remove(connectId);
            threadModule.RunOnNextFrame(() => { threadModule.RunOnNextFrame(() => { closeCallback?.Invoke(); }); });
        }

        public void Release(string connectId)
        {
            if (networkConnectDic.ContainsKey(connectId))
            {
                netClient.Close(connectId);
                networkConnectDic.Remove(connectId);
            }

            if (connectWrapEventDic.ContainsKey(connectId))
            {
                connectWrapEventDic.Remove(connectId);
            }
        }

        public void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                return;
            }

            foreach (var item in networkConnectDic)
            {
                netClient.ForceProbeNetwork(item.Key);
            }
        }

        #region Event 注册回调事件，如果有其他事件，统一添加

        internal ConnectWrapEvent GetConnectWrapEvent(string connectId)
        {
            if (connectWrapEventDic.TryGetValue(connectId, out var connectWrapEvent))
            {
                return connectWrapEvent;
            }

            DHLog.Debug($"GetConnectWrapEvent {connectId} is null..");

            return null;
        }
        
        public void RegisterFastConnectEvent(string connectId, Action<byte[]> onConnectedEvent)
        {
            if (!connectWrapEventDic.TryGetValue(connectId, out var connectWrap))
            {
                connectWrap = new ConnectWrapEvent(connectId);
                connectWrapEventDic.Add(connectId, connectWrap);
            }

            connectWrap.FastConnectedEvent += onConnectedEvent;
        }

        public void UnregisterFastConnectEvent(string connectId, Action<byte[]> onConnectedEvent)
        {
            if (!connectWrapEventDic.TryGetValue(connectId, out var connectWrap))
            {
                return;
            }

            connectWrap.FastConnectedEvent -= onConnectedEvent;
        }

        public void RegisterConnectEvent(string connectId, Action onConnectedEvent)
        {
            if (!connectWrapEventDic.TryGetValue(connectId, out var connectWrap))
            {
                connectWrap = new ConnectWrapEvent(connectId);
                connectWrapEventDic.Add(connectId, connectWrap);
            }

            connectWrap.ConnectedEvent += onConnectedEvent;
        }

        public void UnregisterConnectEvent(string connectId, Action OnConnectedEvent)
        {
            if (!connectWrapEventDic.TryGetValue(connectId, out var connectWrap))
            {
                return;
            }

            connectWrap.ConnectedEvent -= OnConnectedEvent;
        }

        public void RegisterDisconnectEvent(string connectId, Action<int> onDisconnectEvent)
        {
            if (!connectWrapEventDic.TryGetValue(connectId, out var connectWrap))
            {
                connectWrap = new ConnectWrapEvent(connectId);
                connectWrapEventDic.Add(connectId, connectWrap);
            }

            connectWrap.DisconnectEvent += onDisconnectEvent;
        }

        public void UnregisterDisconnectEvent(string connectId, Action<int> OnDisconnectEvent)
        {
            if (!connectWrapEventDic.TryGetValue(connectId, out var connectWrap))
            {
                return;
            }

            connectWrap.DisconnectEvent -= OnDisconnectEvent;
        }

        public void RegisterHandleReceiveMessageEvent(string connectId,
            Action<byte, byte, byte[]> OnHandleReceiveMessageEvent)
        {
            if (!connectWrapEventDic.TryGetValue(connectId, out var connectWrap))
            {
                connectWrap = new ConnectWrapEvent(connectId);
                connectWrapEventDic.Add(connectId, connectWrap);
            }

            connectWrap.HandleReceiveMessageEvent += OnHandleReceiveMessageEvent;
        }

        public void UnregisterHandleReceiveMessageEvent(string connectId,
            Action<byte, byte, byte[]> OnHandleReceiveMessageEvent)
        {
            if (!connectWrapEventDic.TryGetValue(connectId, out var connectWrap))
            {
                return;
            }

            connectWrap.HandleReceiveMessageEvent -= OnHandleReceiveMessageEvent;
        }
        #endregion
    }
}
#endif