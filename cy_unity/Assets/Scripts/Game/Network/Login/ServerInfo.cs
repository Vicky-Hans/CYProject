using System;
using DH.NativeCore;
using DH.UIFramework.Commands;
using DH.UIFramework.Observables;

namespace DH.Game.Login
{
    public class ServerInfo : ObservableObject, IComparable<ServerInfo>, IComparable
    {
        public int Sid => nestMessage.sid;
        public string Name => nestMessage.name;
        public uint Ctime => nestMessage.ctime;
        public uint Status => nestMessage.status;
        public int Recommend => nestMessage.recommend;

        private Server nestMessage;
        private RoleDigest digest;
        private bool selected;

        public bool NewServer => (Recommend & 1) == 1;
        public bool IsRecommend => (Recommend & 2) == 2;

        public SimpleCommand SelectCmd { get; set; }

        public RoleDigest Digest
        {
            get => digest;
            set => Set(ref digest, value);
        }

        public bool Selected
        {
            get => selected;
            set => Set(ref selected, value);
        }

        public ServerInfo(Server server)
        {
            nestMessage = server;
        }

        public int CompareTo(ServerInfo other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var sidComparison = Sid.CompareTo(other.Sid);
            if (sidComparison != 0) return sidComparison;
            var nameComparison = string.Compare(Name, other.Name, StringComparison.Ordinal);
            if (nameComparison != 0) return nameComparison;
            var ctimeComparison = Ctime.CompareTo(other.Ctime);
            if (ctimeComparison != 0) return ctimeComparison;
            var statusComparison = Status.CompareTo(other.Status);
            if (statusComparison != 0) return statusComparison;
            return Recommend.CompareTo(other.Recommend);
        }

        public int CompareTo(object obj)
        {
            return CompareTo((ServerInfo)obj);
        }
    }
}