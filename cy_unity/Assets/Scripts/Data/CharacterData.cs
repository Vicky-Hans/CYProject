using System;
using DH.Config;
using DH.Proto;
using DH.UIFramework;
using DHFramework;

namespace DH.Data
{
    [ProtoWrap(typeof(CharacterSync))]
    public partial class CharacterData : BaseData
    {
        public long changeNameCdPullTime;
        private readonly int headParamOffset = 10000;
        [AutoNotify] private int invitedChangeCount;
    
        public override void Init()
        {
            base.Init();
        }

        protected override void ClearData()
        {
            UnlockHead.Clear();
            changeNameCdPullTime = 0;
            base.ClearData();
        }
        
        public long GetCreateRoleTime()
        {
            return RoleCreateTime;
        }

        /// <summary>
        /// 获取头像Id
        /// </summary>
        /// <param name="cfgId"></param>
        /// <param name="seasonId"></param>
        /// <returns></returns>
        public int GetHeadIdByCfgId(int cfgId, int seasonId)
        {
            var headId = seasonId * headParamOffset + cfgId;
            return headId;
        }

        /// <summary>
        /// 获取头像配置Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetHeadCfgId(int id)
        {
            var cfgId = id % headParamOffset;
            return cfgId;
        }

        /// <summary>
        ///  获取头像赛季id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetHeadSeasonById(int id)
        {
            var seasonId = id / headParamOffset;
            return seasonId;
        }

        public void AddTotalRecharge(double num)
        {
            if(num<=0) return;
            TotalRecharge += num;
        }

        public string GetPlayerHeadImgPath(int headId,bool isUnLock = true)
        {
            var cfgId = GetHeadCfgId(headId);
            var cfg = ConfigCenter.ProPictureCfgColl.GetDataById(cfgId);
            if (cfg == null) return "common[common_alpha]";
            if (cfg.ShowType == (int)EHeadShowType.HeadShowTypeStatic)
            {
                return  cfg.Type == (int)GameConst.EHeadCfgType.Head? $"playerinfo[{cfg.Icon}{(isUnLock?"_d":"_h")}]":$"playerinfo[{cfg.Icon}]";  
            }
            return $"playerinfo[{cfg.Basemap}]";
        }

        public int GetPlayerHeadQuality(int id)
        {
            var cfgId=  DataCenter.charcaterData.GetHeadCfgId(id);
            var pictureCfg = ConfigCenter.ProPictureCfgColl.GetDataById(cfgId);
            
            if(pictureCfg == null) return 0;
            if(pictureCfg.Quality == 0) return 0;
            return pictureCfg.Quality;
        }
        public string GetPlayerHeadBgPath(int id)
        {
            var cfgId=  DataCenter.charcaterData.GetHeadCfgId(id);
            var pictureCfg = ConfigCenter.ProPictureCfgColl.GetDataById(cfgId);
            
            if(pictureCfg == null) return "common[common_alpha]";
            if(pictureCfg.Quality == 0) return "common[commom_equipbg_4]";
            return $"common[commom_equipbg_{pictureCfg.Quality}]";
        }

        
        public string GetPlayerHeadFrameEffectPath(int headId)
        {
            var cfgId = GetHeadCfgId(headId);
            var cfg = ConfigCenter.ProPictureCfgColl.GetDataById(cfgId);
            if (cfg == null)
            {
                DHLog.Warning($" ProPictureCfgColl 没有处理对应数据={headId}，请及时处理");
                return  null;
            }
            return cfg.SpEffect;
        }
        

        public void AddHead(int id)
        {
            UnlockHead.Add(id);
        }
        
        public void AddAdTimes()
        {
            AdTimes += 1;
        }

        
        public string DaysSubscribeGift => Digest.RoleId + "DaysSubscribeGift";//关注有礼
        public bool IsShowSubscribeGiftRed()
        {
            var day = DHUnityUtil.PlayerPrefs.GetInt(DaysSubscribeGift);
            var now = ServerTime.Instance.GetDay(ServerTime.Instance.GetNowTime());
            return DiscordFlag == 0 && now > day;
        }
        public bool IsShowSubscribeGift()
        {
            return DiscordFlag == 0 ;
        }

        public void SuccessfulSubscribe(int flag)
        {
            DiscordFlag = flag;
        }
        
        #region 邀请信息

        /// <summary>
        /// 检查是否领取过过该奖励
        /// </summary>
        /// <param name="cfgId"></param>
        /// <returns></returns>
        public bool CheckIsClaimedInvitedReward(int cfgId)
        {
            return InviteClaim.Contains(cfgId);
        }

        public bool IsShowInvitedRedDot()
        {
            var ret = false;
            var cfgs = ConfigCenter.ShareRewardProgressCfgColl.DataItems;
            foreach (var cfg in cfgs)
            {
                if(cfg.Value1 <= InviteNumber && !CheckIsClaimedInvitedReward(cfg.Id))
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        /// <summary>
        /// 检查是否被邀请过
        /// </summary>
        public bool CheckIsInvited => IsInvited;
        
        #endregion

    }


    [ProtoWrap(typeof(Digest))]
    public partial class DigestData : BaseData
    {
        public long AddExp; 
        public bool IsEnough(int checkLv)
        {
            return Lv >= checkLv;
        }

        public void ChangeExp(long exp, bool isAdd = true)
        {
            var cfg = ConfigCenter.ProLevelCfgColl.GetDataById(Lv);
            if (cfg == null) return;
            if (isAdd)
            {
                AddExp = exp;
                RaisePropertyChanged(nameof(AddExp));
                Exp += exp;
                while (Exp >= cfg.Exp)
                {
                    Lv += 1;
                    Exp -= cfg.Exp;
                    cfg = ConfigCenter.ProLevelCfgColl.GetDataById(Lv);
                    if (cfg == null) return;
                }
            }
            else
            {
                DHLog.Debug("暂不支持减经验");
            }
        }

        public bool CheckEnoughLv(int lv)
        {
            return Lv >= lv;
        }

        public bool IsNewHead()
        {
            var ret = false;
            var unlockHeads = DataCenter.charcaterData.UnlockHead;
            foreach (var id in unlockHeads)
            {
                var cfgId = DataCenter.charcaterData.GetHeadCfgId(id);
                var cfg = ConfigCenter.ProPictureCfgColl.GetDataById(cfgId);
                if(cfg!=null && cfg.Type == (int)GameConst.EHeadCfgType.Head)
                {
                    if (DataCenter.charcaterData.NewHead.Contains(id))
                    {
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }
        public bool IsNewFrame()
        {
            var ret = false;
            var unlockHeads = DataCenter.charcaterData.UnlockHead;
            foreach (var id in unlockHeads)
            {
                var cfgId = DataCenter.charcaterData.GetHeadCfgId(id);
                var cfg = ConfigCenter.ProPictureCfgColl.GetDataById(cfgId);
                if(cfg!=null && cfg.Type == (int)GameConst.EHeadCfgType.Frame)
                {
                    if (DataCenter.charcaterData.NewHead.Contains(id))
                    {
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }


        public void AddHead(int id)
        {
            if (!DataCenter.charcaterData.UnlockHead.Contains(id))
            {
                DataCenter.charcaterData.UnlockHead.Add(id);
                DataCenter.charcaterData.NewHead.Add(id);
            }
        }
    }
}