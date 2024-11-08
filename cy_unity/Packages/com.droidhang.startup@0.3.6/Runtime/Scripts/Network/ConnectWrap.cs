using DH.UNet;
// 取消使用Startup组件内的NetworkManager
#if !DISABLE_NET_MANAGER
namespace DH.Launch.Network
{
    /// <summary>
    /// 根据 ConnectId 唯一确定一个连接，包含相关的状态；
    /// 每个连接都有自己的 LastConnectStatus 和 PendingTimeoutTasks 超时任务
    /// </summary>
    public class ConnectWrap
    {
        public string ConnectId { get; private set; }
        public UNetConnectStatus LastConnectStatus { get; set; }
        public long ServerTimestamp { get; set; }
        public UNetConnectConfiguration Config { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public bool CanSendMsg { get; set; }
        public bool Reconnecting { get; set; }
        public bool SendFastReconnect { get; set; }
        public bool ServerRefuse { get; set; }

        public ConnectWrap(string connectId,string address,int port,UNetConnectConfiguration config)
        {
            ConnectId = connectId;
            Address = address;
            Port = port;
            Config = config;
        }
    }

}
#endif