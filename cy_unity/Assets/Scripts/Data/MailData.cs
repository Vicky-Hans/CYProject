using System.Collections.Generic;
using DH.Config;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using System;
using Newtonsoft.Json.Linq;

namespace DH.Data
{
    [ProtoWrap(typeof(Proto.MailData))]
    public partial class MailData:BaseData
    {
        private ObservableDictionary<long, MailCfg> mailCfgDictionary = new();
        // 没有读或者没有领奖的邮件数
        [AutoNotify] private int redDotCount;

        private readonly List<string> replaceTierKey = new()
        {
            "tier",
            "tier1",
            "tier2"
        };
        private readonly List<int> replaceMailIds = new()
        {
            20,33
        };
        public override void Init()
        {
            base.Init();
            mailCfgDictionary.Clear();
            var mailCfgs = ConfigCenter.MailCfgColl.DataItems;
            foreach (var cfg in mailCfgs)
            {
                mailCfgDictionary.Add(cfg.Id, cfg);
            }
            UpdateRedDotCount();
        }

        protected override void ClearData()
        {
            mailCfgDictionary.Clear();
            
            Mails.Clear();
            base.ClearData();
        }

        /// <summary>
        /// 添加邮件
        /// </summary>
        /// <param name="mailInfo"></param>
        public void AddMail(MailInfo mailInfo)
        {
            if(Mails.Find(tempInfo => tempInfo.Id == mailInfo.Id)!= null) return;

            if (Mails != null)
            {
                Mails.Add(mailInfo);
                RedDotCount += 1;
            }
            
        }
        /// <summary>
        /// 添加邮件
        /// </summary>
        /// <param name="mailInfos"></param>
        public void AddMail(List<MailInfo> mailInfos)
        {
            foreach (var tempMail in mailInfos)
            {
                AddMail(tempMail);
            }
        }
        
        /// <summary>
        /// 获取邮件信息 通过邮件id
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        public MailInfo GetMailInfoByMailId(long mailId)
        {
            var index = Mails.FindIndex(tempInfo => tempInfo.Id == mailId);
            if (index != -1) 
            {
                return Mails[index];
            }
            return null;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="???"></param>
        public bool CheckIsHaveMailByMailId(long mailId)
        {
            return Mails.Find(tempInfo => tempInfo.Id == mailId) != null;
        }

        public void SetMailIsRead(long mailId)
        {
            var mailInfo = GetMailInfoByMailId(mailId);
            mailInfo.ReadFlag = (int) EMailReadState.MailStateRead;
            mailInfo.ReadTime = Lodash.GetUnixTime();
        }

        public EMailReadState GetMailIsRead(long mailId)
        {
            var mailInfo = GetMailInfoByMailId(mailId);
            return (EMailReadState)mailInfo.ReadFlag;
        }

        public bool GetMailIsHaveRewards(long mailId)
        {
            var rewards = GetMailAffixByMailId(mailId);
            if (rewards.Count == 0)
            {
                return false;
            }

            return true;
        }
        

        /// <summary>
        ///  获取邮件奖励
        /// </summary>
        /// <param name="???"></param>
        public List<MailRewards> GetMailAffixByMailId(long mailId)
        {
            List<MailRewards> ret = new();

            var mailInfo = GetMailInfoByMailId(mailId);
             if (mailInfo ==null || string.IsNullOrEmpty(mailInfo.Affix)) {
                 return ret;
             }
             ret = DHUtility.Json.ToObject<List<MailRewards>>(mailInfo.Affix);
             // 2024 01 03 不处理配置里的奖励，所有奖励走附件
             // var cfgRewards = GetRewardsByCfgId(mailInfo.CfgId);
             // if (cfgRewards.Count != 0)
             // {
             //     foreach (var item in cfgRewards)
             //     {
             //         MailRewards temp = new MailRewards((int)item.Type, item.Id,(int)item.Count);
             //         ret.Add(temp);
             //     }
             // }
             return ret;
        }
        
        /// <summary>
        /// 邮件是否领奖
        /// </summary>
        /// <param name="???"></param>
        /// <returns></returns>
        public EMailRewardState GetMailIsClaimRewardsByMailId(long mailId) { 
            var rewards = GetMailAffixByMailId(mailId);
            if (rewards.Count > 0)
            {
                var mailInfo = GetMailInfoByMailId(mailId);
                return (EMailRewardState)mailInfo.RewardFlag; 
            }

            return EMailRewardState.MailRewardStateClaim;
        }

        public void SetMailIsClaimRewardsByMailId(long mailId)
        {
            var mailInfo = GetMailInfoByMailId(mailId);
            mailInfo.RewardFlag = (int) EMailRewardState.MailRewardStateClaim;
        }

        public long GetMailSendTime(long mailId)
        {
            var mailInfo = GetMailInfoByMailId(mailId);
            return mailInfo.SendTime;
        }
        public long GetMailExpireTime(long mailId)
        {
            var mailInfo = GetMailInfoByMailId(mailId);
            return mailInfo.ExpireTime;
        }

        /// <summary>
        /// 检查邮件是否有效
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        public bool CheckMailIsVaild(long mailId)
        {
            if (!CheckIsHaveMailByMailId(mailId))
            {
                return false;
            }
            var mailInfo = GetMailInfoByMailId(mailId);
            var nowTime = Lodash.GetUnixTime();
            return mailInfo.ExpireTime > nowTime;
        }

        /// <summary>
        /// 领取所有可以奖励的邮件
        /// </summary>
        public List<long> GetAllCanClaimRewardMailList()
        {
            List<long> ret = new();
            
            foreach (var item in Mails)
            {
                var claimState = GetMailIsClaimRewardsByMailId(item.Id);
                if (claimState == EMailRewardState.MailRewardStateUnClaim)
                {
                    ret.Add(item.Id);
                }
            }
            return ret;
        }

        public string ReplaceAllArgs(string content, Dictionary<string,JToken> args, int mailId)
        { 
            string ret = content;
            foreach (var item in args)
            {
                var replaceStr = GetReplaceStr(item.Key, item.Value, mailId);
                ret = ret.Replace($"#{item.Key}#", replaceStr);
            }
            return ret;
        }

        private string GetReplaceStr(string key, JToken value, int mailId)
        {
            return value.ToString();
        }

        public List<long> GetCanDelMailList()
        {
            List<long> ret = new();
            foreach (var item in Mails)
            {
                var readState = GetMailIsRead(item.Id);
                var claimState = GetMailIsClaimRewardsByMailId(item.Id);
                if (readState == EMailReadState.MailStateRead &&
                    claimState == EMailRewardState.MailRewardStateClaim)
                {
                    ret.Add(item.Id);
                }
            }
            return ret;
        }

        /// <summary>
        /// 获取邮件标题
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        public string GetMailTitle(long mailId, int languageCode)
        {
            if (!CheckIsHaveMailByMailId(mailId)) return string.Empty;
            
            var mailInfo = GetMailInfoByMailId(mailId);
            if (mailInfo.Title is null or "")
            {
                var languageCfg = ConfigCenter.MailLanguageCfgColl.GetDataById(mailInfo.CfgId);
                if (languageCfg != null)
                {
                    return languageCfg.Name;
                }
                return string.Empty;
            }

            // 解析JSON数据
            List<Dictionary<string,string>> dataArray = DHUtility.Json.ToObject<List<Dictionary<string,string>>>(mailInfo.Title);
            string ret = String.Empty;
            foreach (var titleData in dataArray)
            {
                if (titleData.TryGetValue(languageCode.ToString(), out ret))
                {
                  break;
                }
            }
            return ret;
        }
        /// <summary>
        /// 获取邮件内容
        /// </summary>
        /// <param name="mailId"></param>
        /// <returns></returns>
        public string GetMailContent(long mailId, int languageCode)
        {
            if (!CheckIsHaveMailByMailId(mailId)) return string.Empty;
            var mailInfo = GetMailInfoByMailId(mailId);
            if (mailInfo.Content is null or "")
            {
                var languageCfg = ConfigCenter.MailLanguageCfgColl.GetDataById(mailInfo.CfgId);
                if (languageCfg != null)
                {
                    var args = DHUtility.Json.ToObject<Dictionary<string, JToken>>(mailInfo.Args);
                    var content = languageCfg.Content;
                    if (args != null && args.Count > 0)
                    {
                        content = ReplaceAllArgs(content, args, mailInfo.CfgId);
                    }
                    return content;
                }
                return string.Empty;
            }
            
            // 解析JSON数据
            List<Dictionary<string, JToken>> argsArray = DHUtility.Json.ToObject<List<Dictionary<string, JToken>>>(mailInfo.Args);
            List<Dictionary<string,JToken>> contentArray = DHUtility.Json.ToObject<List<Dictionary<string,JToken>>>(mailInfo.Content);
            string ret = String.Empty;
            foreach (var titleData in contentArray)
            {
                if (titleData.TryGetValue(languageCode.ToString(), out JToken tempRet))
                {
                    ret = tempRet.ToString();
                    break;
                }
            }

            if (ret == String.Empty)
            {
                return ret;
            }
            foreach (var item in argsArray)
            {
                ret = ReplaceAllArgs(ret, item, mailInfo.CfgId);
            }
            return ret;
        }
        
        private List<Reward> GetRewardsByCfgId(int cfgId)
        {
            List<Reward> ret = new();
            var info = ConfigCenter.MailCfgColl.GetDataById(cfgId);
            if (info is null || info.Reward == null) return ret;
            ret = info.Reward;
            return ret;
        }


        /// <summary>
        /// 删除邮件
        /// </summary>
        /// <param name="mailIdList"></param>
        public void DeleteMails(List<long> mailIdList)
        {
            foreach (var id in mailIdList)
            {
                var index = Mails.FindIndex(info=>info.Id == id);
                if (index == -1 ) continue;
                Mails.RemoveAt(index);
            }
        }

        /// <summary>
        /// 获取邮件的红点信息
        /// </summary>
        /// <returns></returns>
        public void UpdateRedDotCount()
        {
            RedDotCount = 0;
            foreach (var item in Mails)
            {
                var readState = GetMailIsRead(item.Id);
                var claimState = GetMailIsClaimRewardsByMailId(item.Id);
                if (readState == EMailReadState.MailStateUnRead ||
                    claimState == EMailRewardState.MailRewardStateUnClaim)
                {
                    //没有读或者没有领奖
                    RedDotCount += 1;
                    break;//红点暂时不需要显示个数
                }
            }
        }
    }
}