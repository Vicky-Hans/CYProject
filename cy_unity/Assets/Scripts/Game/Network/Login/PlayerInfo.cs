using System;
using DH.UIFramework.Observables;
using RoleDigest = DH.NativeCore.RoleDigest;

namespace DH.Game.Login
{
     public class PlayerInfo : ObservableObject, IComparable<PlayerInfo>
     {
         public int Account => protoMessage?.Account ?? NestMessage.account;
        public int RoleId => protoMessage?.RoleId ?? NestMessage.roleId;
        public int Sid => protoMessage?.Sid ?? NestMessage.sid;
        public string Name => protoMessage?.Name ?? NestMessage.name;
        public int Logo => protoMessage?.Logo ?? NestMessage.logo;
        public long Exp => protoMessage?.Exp ?? NestMessage.exp;
        public long VExp => protoMessage?.Vexp ?? NestMessage.vexp;
        public long Ctime => protoMessage?.Ctime ?? NestMessage.ctime;
        public long LastOnlineTime => protoMessage?.LastOnlineTime ?? NestMessage.last_online_time;
        public long LastOfflineTime => protoMessage?.LastOfflineTime ?? NestMessage.last_offline_time;
        public int HeadFrame => protoMessage?.HeadFrame ?? NestMessage.head_frame;

        public RoleDigest NestMessage => nestMessage;

        private readonly DH.NativeCore.RoleDigest nestMessage;
        private readonly DH.Proto.RoleDigest protoMessage;

        public PlayerInfo(DH.Proto.RoleDigest roleDigest)
        {
        }

        public PlayerInfo(DH.NativeCore.RoleDigest roleDigest)
        {
            this.nestMessage = roleDigest;
        }

        public int CompareTo(PlayerInfo other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var accountComparison = Account.CompareTo(other.Account);
            if (accountComparison != 0) return accountComparison;
            var roleIdComparison = RoleId.CompareTo(other.RoleId);
            if (roleIdComparison != 0) return roleIdComparison;
            var sidComparison = Sid.CompareTo(other.Sid);
            if (sidComparison != 0) return sidComparison;
            var nameComparison = string.Compare(Name, other.Name, StringComparison.Ordinal);
            if (nameComparison != 0) return nameComparison;
            var logoComparison = Logo.CompareTo(other.Logo);
            if (logoComparison != 0) return logoComparison;
            var expComparison = Exp.CompareTo(other.Exp);
            if (expComparison != 0) return expComparison;
            var vExpComparison = VExp.CompareTo(other.VExp);
            if (vExpComparison != 0) return vExpComparison;
            var ctimeComparison = Ctime.CompareTo(other.Ctime);
            if (ctimeComparison != 0) return ctimeComparison;
            var lastOnlineTimeComparison = LastOnlineTime.CompareTo(other.LastOnlineTime);
            if (lastOnlineTimeComparison != 0) return lastOnlineTimeComparison;
            var lastOfflineTimeComparison = LastOfflineTime.CompareTo(other.LastOfflineTime);
            if (lastOfflineTimeComparison != 0) return lastOfflineTimeComparison;
            return HeadFrame.CompareTo(other.HeadFrame);
        }
    }
}