using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
#if DISABLE_NET_MANAGER
using DH.Game.Network;
#else
using DH.Launch.Network;
#endif
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Game.Login;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using DH.Proto;
using DH.UNet;
using DHFramework;
using DHFramework.Json;
using Google.Protobuf;

namespace DH.Game
{
    public partial class GameNetworkManager
    {
        private class MessageInfo : IReference
        {
            public byte group;
            public byte cmd;
            public ReqGameMsg reqType;
            public RspGameMsg rspType;
            public IMessage requestData;
            public AutoResetUniTaskCompletionSource<IMessage> tcs;

            public override string ToString()
            {
                return
                    $"[Net Message Info]\ngroup: {group}\ncmd: {cmd}\nReqGameMsg: {reqType}\nRspGameMsg: {rspType}\nRequestData: {requestData}";
            }

            public void Clear()
            {
                tcs = null;
                requestData = null;
            }
        }

        private const string connectId = "Game";
        private const bool supportFastLogin = true;

        private readonly Dictionary<RspGameMsg, Queue<MessageInfo>> waitResponseHandlers =
            new Dictionary<RspGameMsg, Queue<MessageInfo>>();
        private readonly Dictionary<RspGameMsg, HashSet<INetworkHandler>> messageDispatcher =
            new Dictionary<RspGameMsg, HashSet<INetworkHandler>> ();

        private UniTaskCompletionSource<bool> connectTcs;

        public event Action onConnect;
        public event Action<byte[]> onFastConnect;
        public event Action onConnectFailed;
        public event Action onDisconnect;
        public event Action onServerKick;
        public event Action onOtherDeviceLogin;
        
        private static GameNetworkManager instance;

        public static GameNetworkManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameNetworkManager();
                }

                return instance;
            }
        }

        private GameNetworkManager()
        {

        }

        public bool IsConnect
        {
            get
            {
                return NetworkManager.Instance.CheckSendState(connectId);
            }
        }

        public void Init()
        {
            NetworkManager.Instance.RegisterHandleReceiveMessageEvent(connectId, HandleReceiveMessage);
            NetworkManager.Instance.RegisterConnectEvent(connectId, HandleConnectEvent);
            NetworkManager.Instance.RegisterDisconnectEvent(connectId, HandleDisconnectEvent);
            NetworkManager.Instance.RegisterFastConnectEvent(connectId, HandleFastConnectEvent);
        }

        public void Destroy()
        {
            onConnect = null;
            onDisconnect = null;
            onServerKick = null;
            onOtherDeviceLogin = null;
            ClearWaitResponse();
            ClearDispatcher();
            if (!NetworkManager.Instance) return;

            NetworkManager.Instance.UnregisterHandleReceiveMessageEvent(connectId, HandleReceiveMessage);
            NetworkManager.Instance.UnregisterConnectEvent(connectId, HandleConnectEvent);
            NetworkManager.Instance.UnregisterDisconnectEvent(connectId, HandleDisconnectEvent);
            NetworkManager.Instance.UnregisterFastConnectEvent(connectId, HandleFastConnectEvent);
            Close();
        }

        private void ClearDispatcher()
        {
            foreach (var item in messageDispatcher)
            {
                item.Value.Clear();
            }
            messageDispatcher.Clear();
        }
        
        private void ClearWaitResponse()
        {
            foreach (var handlers in waitResponseHandlers)
            {
                foreach (var messageInfo in handlers.Value)
                {
                    messageInfo.tcs?.TrySetResult(null);
                    messageInfo.tcs = null;
                    ReferencePool.Release(messageInfo);
                }
                
                handlers.Value.Clear();
            }
            
            waitResponseHandlers.Clear();
        }
        
        private static int GetRspMsgId(byte group, byte cmd)
        {
            return group * 100000 + cmd * 10 + 1;
        }

        private static void MsgId2GroupCmd<T>(T msgType, out byte group, out byte cmd) where T : Enum
        {
            int id = Unsafe.As<T, int>(ref msgType);;
            group = (byte)(id / 100000);
            cmd = (byte)(id % 100000 / 10);
        }
        
        private void HandleReceiveMessage(byte group, byte cmd, byte[] bytes)
        {
            int rspId = GetRspMsgId(group, cmd);
            RspGameMsg rspType = (RspGameMsg)rspId;
            if (!MsgType.RspGameMsgDict.TryGetValue(rspType,out var messageCreator))
            {
                DHLog.Error($"Can't find response type with id: {rspId}");
                return;
            }

            var responseData = messageCreator();
            responseData.MergeFrom(bytes);
            MessageInfo messageInfo = null;
            if (waitResponseHandlers.TryGetValue(rspType, out var messageInfos) && messageInfos.Count > 0)
            {
                messageInfo = messageInfos.Dequeue();
                messageInfo.tcs.TrySetResult(responseData);
            }
            
            DispatchMsg(rspType, responseData, messageInfo?.requestData);

            if (messageInfo != null)
            {
                ReferencePool.Release(messageInfo);
            }
        }

        private void HandleConnectEvent()
        {
            if (connectTcs != null)
            {
                connectTcs.TrySetResult(true);
                connectTcs = null;
            }
            else
            {
                onConnect?.Invoke();
            }
        }
        
        private void HandleFastConnectEvent(byte[] args)
        {
            onFastConnect?.Invoke(args);
            ActivityManager.Instance.Hide(WaitType.Net);
        }

        private void HandleDisconnectEvent(int errorCode)
        {
            if (connectTcs != null)
            {
                connectTcs.TrySetResult(false);
                connectTcs = null;
                ClearWaitResponse();
                Close();
                return;
            }
            
            ActivityManager.Instance.Hide(WaitType.Net,true);
            switch (errorCode)
            {
                case UNetErrorCode.ServerKickNtfClient:
                    ClearWaitResponse();
                    Close();
                    onServerKick?.Invoke();
                    return;
                
                case UNetErrorCode.OtherDeviceLogin:
                    ClearWaitResponse();
                    Close();
                    onOtherDeviceLogin?.Invoke();
                    return;
            }

            ClearWaitResponse();
            DHLog.Debug("[Net Disconnect] Net Disconnect.");
            onDisconnect?.Invoke();
            CheckNetworkDown<IMessage>(null);
        }

        public async UniTaskVoid ConnectAsync()
        {
            await Connect();
        }
        
        public async UniTask<bool> Connect()
        {
            if (IsConnect)
            {
                return true;
            }

            string url = Usdk.LoginCenterConfigDic.ReadValue<string>(Usdk.Config.UgateAddr);

#if UNITY_WEBGL || WECHAT_MINI
            string host = url;
            int port = 0;
#else
            var ipAddress = url.Split(':');
            if (ipAddress.Length != 2)
            {
                DHLog.Error($"获取到 ugate address {url} 不可用");
                return false;
            }

            string host = ipAddress[0];
            int port = int.Parse(ipAddress[1]);
#endif
            connectTcs = new UniTaskCompletionSource<bool>();
            NetworkManager.Instance.Connect(connectId, host, port);
            return await connectTcs.Task;
        }

        public bool Send<T>(T requestData) where T : IMessage
        {
            ReqGameMsg reqType = MsgType.ReqType[typeof(T)];
            if (!NetworkManager.Instance.CheckSendState(connectId))
            {
                return false;
            }

            MsgId2GroupCmd(reqType, out var group, out var cmd);
            var result = NetworkManager.Instance.Send(connectId, group, cmd, requestData.ToByteArray());
            return result;
        }

        /// <summary>
        /// 异步发送消息
        /// </summary>
        /// <param name="requestData"></param>
        /// <typeparam name="T">Response DataType</typeparam>
        /// <returns></returns>
        public async UniTask<(RspGameMsg type,T rsp)> SendAsync<T>(IMessage requestData) where T : IMessage
        {
            var rspType = MsgType.RspType[typeof(T)];
            var reqType = (ReqGameMsg)((int)rspType - 1);
            if (!NetworkManager.Instance.CheckSendState(connectId))
            {
                return (rspType,default(T));
            }

            MsgId2GroupCmd(reqType, out var group, out var cmd);
            var result = NetworkManager.Instance.Send(connectId, group, cmd,requestData.ToByteArray());
            if (!result)
            {
                return (rspType,default(T));
            }

            var messageInfo = ReferencePool.Acquire<MessageInfo>();
            messageInfo.group = group;
            messageInfo.cmd = cmd;
            messageInfo.reqType = reqType;
            messageInfo.rspType = rspType;
            messageInfo.requestData = requestData;
            messageInfo.tcs = AutoResetUniTaskCompletionSource<IMessage>.Create();
            if (!waitResponseHandlers.TryGetValue(messageInfo.rspType, out var messageInfos))
            {
                messageInfos = new Queue<MessageInfo>();
                waitResponseHandlers.Add(messageInfo.rspType,messageInfos);
            }
            messageInfos.Enqueue(messageInfo);
            var messageCmdName = $"{messageInfo.group}_{messageInfo.cmd}";
            if(!silentCmdDic.ContainsKey(messageCmdName)){
                ActivityManager.Instance.Show(WaitType.UI);
            }
            IMessage rspMessage;
            try
            {
                rspMessage = await messageInfo.tcs.Task;
            }
            finally
            {
                if (!silentCmdDic.ContainsKey(messageCmdName))
                {
                    ActivityManager.Instance.Hide(WaitType.UI);
                }
            } 
            return (rspType,(T)rspMessage);
        }

        public static bool CheckNetworkDown<T>(T rsp) where T : IMessage
        {
            if (rsp != null)
            {
                return false;
            }
            
            CommonMessageBoxViewModel messageBoxViewModel;
            // 服务器拒绝重连状态，取消重连按钮
            if (supportFastLogin || !NetworkManager.Instance.CheckServerRefuseReconnect(connectId))
            {
                var msgContent = LocalizeHelper.GetGlobal(GlobalLanguageId.NetworkFastLogin);
                messageBoxViewModel = CommonMessageBoxViewModel.CreateCommonMsgBox(msgContent, () =>
                    {
                        if (!NetworkManager.Instance.Reconnect(connectId, supportFastLogin))
                        {
                            LoginManager.Instance.ResetToLoginMenu();
                        }
                        else
                        {
                            ActivityManager.Instance.Show(WaitType.Net);
                        }
                    },
                    () => { LoginManager.Instance.ResetToLoginMenu(); });
            }
            else
            {
                var msgContent = LocalizeHelper.GetGlobal(GlobalLanguageId.NetworDisconnect);
                messageBoxViewModel = CommonMessageBoxViewModel.CreateCommonMsgBox(msgContent,
                    () => { LoginManager.Instance.ResetToLoginMenu(); }, null);
            }

            UIManager.Instance
                .OpenDialog<CommonMessageBox>(messageBoxViewModel, true)
                .Forget();
            return true;
        }
        
        public void AddListener<T>(Action<T> callback) where T : IMessage
        {
            var rspType = MsgType.RspType[typeof(T)];
            if (!messageDispatcher.TryGetValue(rspType, out var handlers))
            {
                handlers = new HashSet<INetworkHandler>();
                messageDispatcher[rspType] = handlers;
            }
            handlers.Add(new NetworkHandler<T>(callback));
        }

        public void RemoveListener<T>(Action<T> callback) where T : IMessage
        {
            var rspType = MsgType.RspType[typeof(T)];
            var dispatcher = messageDispatcher;
            if(dispatcher.Count == 0) return;
            dispatcher[rspType].Remove(new NetworkHandler<T>(callback));
        }
        
        public void DispatchMsg(RspGameMsg rspType, IMessage responseData, IMessage requestData)
        {
            var dispatcher = messageDispatcher;
            if (!dispatcher.TryGetValue(rspType, out var handlers))
            {
                return;
            }
            
            foreach (var handler in handlers)
            {
                handler.Execute(responseData);
            }
        }

        public void Close()
        {
            if (!NetworkManager.Instance) return;
            NetworkManager.Instance.Close(connectId);
        }

    }
}