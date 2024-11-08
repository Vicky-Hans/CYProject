using System.Linq;
using DH.Data;
using DH.Game.UI;
using DH.Proto;
using DHFramework;


namespace DH.Game
{
    /// <summary>
    /// 这里主要是处理服务器主动推送的消息
    /// </summary>
    public class NotifyManager : Singleton<NotifyManager>
    {
        public void Init()
        {
            Clear();
            AddNetNotifyListener();
        }

        public void Clear()
        {
            RemoveNetNotifyListener();
        }

        /// <summary>
        /// 添加网络监听
        /// </summary>
        private void AddNetNotifyListener()
        {
            // 99 - 2  同步数据
            GameNetworkManager.Instance.AddListener<RspGmSync>(OnNotifyGmSync);
            // 邮件
            GameNetworkManager.Instance.AddListener<RspNewMail>(OnNotifyMailSync);
            GameNetworkManager.Instance.AddListener<RspNewSysMail>(OnNotifyMailSync);
            // 10-3 的提示
            GameNetworkManager.Instance.AddListener<RspNotifyMsg>(OnNotifyMsg);
            // 16-4 
            GameNetworkManager.Instance.AddListener<RspNotify>(OnNotifyInfo);
            GameNetworkManager.Instance.AddListener<RspAnnouncement>(OnBroadCostNotify);
            GameNetworkManager.Instance.AddListener<RspDiscordSync>(OnDiscordSync);
            GameNetworkManager.Instance.AddListener<RspInviteSync>(OnInviteSyncSync);
        }


        /// <summary>
        /// 移除网络监听
        /// </summary>
        private void RemoveNetNotifyListener()
        {
            GameNetworkManager.Instance.RemoveListener<RspGmSync>(OnNotifyGmSync);
            GameNetworkManager.Instance.RemoveListener<RspNewMail>(OnNotifyMailSync);
            GameNetworkManager.Instance.RemoveListener<RspNotifyMsg>(OnNotifyMsg);
            GameNetworkManager.Instance.RemoveListener<RspNotify>(OnNotifyInfo);
            GameNetworkManager.Instance.RemoveListener<RspAnnouncement>(OnBroadCostNotify);
            GameNetworkManager.Instance.RemoveListener<RspInviteSync>(OnInviteSyncSync);

        }
        private void OnDiscordSync(RspDiscordSync obj)
        {
            if (obj.Status != 0) return;
            DataCenter.charcaterData.SuccessfulSubscribe(obj.DiscordFlag);
        }

        private void OnInviteSyncSync(RspInviteSync obj)
        {
            if (obj.Status != 0) return;
            DataCenter.charcaterData.InvitedChangeCount = obj.InviteNumber - DataCenter.charcaterData.InviteNumber;
            DataCenter.charcaterData.InviteNumber = obj.InviteNumber;
        }

        private void OnBroadCostNotify(RspAnnouncement obj)
        {
            BroadCostManager.Instance.AddAnnouncement(obj);
        }

        private void OnNotifyGmSync(RspGmSync data)
        {
            DataCenter.Init(data.SyncInfo);
        }
        private void OnNotifyMailSync(RspNewMail data)
        {
            DataCenter.maildata.AddMail(data.Mails.ToList());
        }
        private void OnNotifyMailSync(RspNewSysMail data)
        {
        }
        private void OnNotifyMsg(RspNotifyMsg data)
        {
            if (data.Status != 0) return;
            var showStr = LocalizeHelper.GetGlobal(data.MsgKey);
            ToastManager.Show(showStr);
        }

        private void OnNotifyInfo(RspNotify data)
        {
            if(data.Dirty == null ||data.Dirty.Count == 0) return;
            foreach (var key in data.Dirty)
            {
                switch (key)
                {
                    case "shop" : DataCenter.shopData.MergeFrom(data.Shop,true);break;
                    case "mainStage" : DataCenter.mainStageData.MergeFrom(data.MainStage,true);break;
                    case "endless" : DataCenter.endlessData.MergeFrom(data.Endless,true);break;
                    case "passport" : DataCenter.allPassportData.MergeFrom(data.Passport,true);break;
                    case "dailyPack" : DataCenter.dailyPackData.MergeFrom(data.DailyPack,true);break;
                    case "luckDraw" : DataCenter.luckyDrawData.MergeFrom(data.LuckDraw,true);break;
                    case "dailyFight" : DataCenter.dailyFightData.MergeFrom(data.DailyFight,true);break;
                    case "secret" : DataCenter.secretData.MergeFrom(data.Secret,true);break;
                    case "magicDraw": DataCenter.magicDrawData.MergeFrom(data.MagicDraw, true); break;
                    case "triggerGift" :
                    {
                        DataCenter.triggerGiftData.MergeFrom(data.TriggerGift, true);
                        DataCenter.triggerGiftData.RefreshData();
                    }break;
                    case "school":DataCenter.collegeData.SyncData(data.School); break;
                    case "luckyTrip":DataCenter.luckyTravelData.MergeFrom(data.LuckyTrip,true); break;
                    case "luckyEgg":DataCenter.luckyEggData.MergeFrom(data.LuckyEgg,true); break;
                    case "bingo":DataCenter.mgicBingoData.MergeFrom(data.Bingo,true); break;
                    default:DHLog.Debug($" 没处理的推送类型 请及时处理 {key}");break;
                }
            }
        }
        
    }
}