using System;
using System.Collections.Generic;
using Google.Protobuf;

namespace DH.Game
{
    public interface INetworkHandler
    {
        void Execute(IMessage message);
    }

    public readonly struct NetworkHandler<T> : INetworkHandler,IEquatable<NetworkHandler<T>> where T : IMessage
    {
        private readonly Action<T> callback;

        public NetworkHandler(Action<T> action)
        {
            callback = action;
        }

        public Action<T> Callback => callback;

        public void Execute(IMessage message)
        {
            callback((T)message);
        }

        public override int GetHashCode()
        {
            return callback.GetHashCode();
        }

        public override bool Equals(object? obj) => obj is NetworkHandler<T> other && this.Equals(other);

        public bool Equals(NetworkHandler<T> obj)
        {
            return callback == obj.callback;
        }
        
        public static bool operator ==(NetworkHandler<T> lhs, NetworkHandler<T> rhs)
        {
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(NetworkHandler<T> lhs, NetworkHandler<T> rhs) => !(lhs == rhs);
    }
}