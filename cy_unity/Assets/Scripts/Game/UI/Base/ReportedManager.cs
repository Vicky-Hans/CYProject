using System;
using System.Collections.Generic;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Proto;
using DHFramework;
using DHUnityUtil;

namespace DH.Game
{
    public enum ReportedType
    {
        RechargeNewBie,//购买新手礼包
        RechargeMonthcard,//购买月卡
        Remain1,//1日留存
        Remain3,//3日留存
        AdvCnt15,//广告15次
        AdvCnt30,//广告30次
        AdvCnt50,//广告50次
        UpLv5,//升级5
        UpLv10,//升级10
        UpLv30,//升级30
    }

    public struct ReportedInfo
    {
        public string Name;//名称
        public bool Only;//是否首次发送
    }

    public class ReportedManager : Singleton<ReportedManager>
    {
        public Dictionary<ReportedType,ReportedInfo> ReportedTypeNameDic= new ()
        {
            {ReportedType.RechargeNewBie,new ReportedInfo{Name = "purchase_newbie",Only = false}},
            {ReportedType.RechargeMonthcard,new ReportedInfo{Name = "purchase_monthcard",Only = false}},
            {ReportedType.Remain1,new ReportedInfo{Name = "retention_day_1",Only = true}},
            {ReportedType.Remain3,new ReportedInfo{Name = "retention_day_3",Only = true}},
            {ReportedType.AdvCnt15,new ReportedInfo{Name = "view_15_ads",Only = true}},
            {ReportedType.AdvCnt30,new ReportedInfo{Name = "view_30_ads",Only = true}},
            {ReportedType.AdvCnt50,new ReportedInfo{Name = "view_50_ads",Only = true}},
            {ReportedType.UpLv5,new ReportedInfo{Name = "lv_5",Only = true}},
            {ReportedType.UpLv10,new ReportedInfo{Name = "lv_10",Only = true}},
            {ReportedType.UpLv30,new ReportedInfo{Name = "lv_30",Only = true}},
        };

        //重新进入游戏初始化
        public void Init()
        {
            RoleTimeChange();
        }

        protected override void Initialization()
        {
            base.Initialization();
            DataCenter.charcaterData.PropertyChanged += ChapterDataChanged;
            DataCenter.charcaterData.Digest.PropertyChanged += ChapterDigestDataChanged;
        }


        protected override void Release()
        {
            base.Release();
            DataCenter.charcaterData.PropertyChanged -= ChapterDataChanged;
            DataCenter.charcaterData.Digest.PropertyChanged -= ChapterDigestDataChanged;
        }

        #region 充值部分

        public void RechargeCallBack(int packId)
        {
            var pCfg = ConfigCenter.PackageCfgColl.GetDataById(packId);
            if (pCfg ==null) return;
            switch (pCfg.Rule)
            {
                case 4:
                    SendReported(ReportedType.RechargeMonthcard);
                    break;
                case 5:
                    SendReported(ReportedType.RechargeNewBie);
                    break;
            }

        }
        #endregion

        #region 留存率部分
        private void RoleTimeChange()
        {
            var day = ServerTime.Instance.GetTimeDay(DataCenter.charcaterData.RoleCreateTime);
            day += 1;
            if (day >= 1)
            {
                SendReported(ReportedType.Remain1);
            }
            if (day >= 3)
            {
                SendReported(ReportedType.Remain3);
            }
        }
        #endregion

        #region 等级部分
        private void ChapterDigestDataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataCenter.charcaterData.Digest.Lv))
            {
                LvChanged();
            }
        }

        private void LvChanged()
        {
            if (DataCenter.charcaterData.Digest.Lv == 0) return;
            switch (DataCenter.charcaterData.Digest.Lv)
            {
                case 5: SendReported(ReportedType.UpLv5); break;
                case 10: SendReported(ReportedType.UpLv10); break;
                case 30: SendReported(ReportedType.UpLv30); break;
            }
        }

        #endregion

        #region 广告部分
        private void ChapterDataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataCenter.charcaterData.AdTimes))
            {
                AddAdTimes();
            }
        }


        //广告次数变化事件
        private void AddAdTimes()
        {
            if (DataCenter.charcaterData.AdTimes!=0)
            {
                switch (DataCenter.charcaterData.AdTimes)
                {
                    case 15: SendReported(ReportedType.AdvCnt15); break;
                    case 30: SendReported(ReportedType.AdvCnt30); break;
                    case 50: SendReported(ReportedType.AdvCnt50); break;
                }
            }
        }
        #endregion

        #region 数据埋点上传
        private string GetKey(ReportedType type)
        {
            return $"{ReportedTypeNameDic[type].Name}_{DataCenter.charcaterData.Digest.RoleId}_Reported";
        }
        
        private string GetFirstKey(ReportedType type)
        {
            return $"{ReportedTypeNameDic[type].Name}_{DataCenter.charcaterData.Digest.RoleId}_First";
        }
        
        public void SendReported(ReportedType type)
        {
            if (ReportedTypeNameDic[type].Only)
            {
                var isSubmit = PlayerPrefs.GetInt(GetFirstKey(type));
                if(isSubmit==1) return;
            }
            
            DHLog.Debug($"SendReported: {type}   key:{ReportedTypeNameDic.GetValueOrDefault(type).Name}   isFirst:{ReportedTypeNameDic.GetValueOrDefault(type).Only}");
            Usdk.ThirdEvent.TrackEvent(ReportedTypeNameDic.GetValueOrDefault(type).Name, new[] { Usdk.ThirdEvent.Firebase,Usdk.ThirdEvent.Appsflyer});
            if (ReportedTypeNameDic[type].Only)
            {
                PlayerPrefs.SetInt(GetFirstKey(type), 1);
            }
        }
        #endregion

        #region 网络相关
        /// <summary>
        /// 广告上报
        /// </summary>
        /// <param name="isLook">是否真实观看</param>
        /// <param name="count">观看数量</param>
        public async UniTask SendAdUpload(bool isLook=true,int count = 1,Action sendAdUploadCb = null)
        {
            var req = new ReqAdUpload
            {
                Count = count,
                Look = isLook,
            };
            await GameNetworkManager.Instance.SendAsync<RspAdUpload>(req);
            sendAdUploadCb?.Invoke();
        }
        #endregion
      
    }
}
