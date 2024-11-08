using System;
#if DISABLE_NET_MANAGER
namespace DH.Game.Network
{
    /// <summary>
    /// 和ConnectWrap 对应的注册事件，ConnectId和ConnectWrap相同
    /// </summary>
    internal class ConnectWrapEvent
    {
        public string ConnectId { get; private set; }
        public event Action ConnectedEvent;
        public event Action<int> DisconnectEvent;
        public event Action<byte[]> FastConnectedEvent; 
        public event Action<byte, byte, byte[]> HandleReceiveMessageEvent;

        public ConnectWrapEvent(string connectId)
        {
            ConnectId = connectId;
        }
            
        public void RaiseConnectedEvent()
        {
            ConnectedEvent?.Invoke();
        }
    
        public void RaiseDisconnectEvent(int errorCode)
        {
            DisconnectEvent?.Invoke(errorCode);
        }
        
        public void RaiseHandleReceiveMessageEvent(byte group, byte cmd, byte[] data)
        {
            HandleReceiveMessageEvent?.Invoke(group, cmd, data);
        }
        
        public void RaiseHandleFastConnectedEvent(byte[] data)
        {
            FastConnectedEvent?.Invoke(data);
        }
    }
}
#endif