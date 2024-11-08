using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Log;
using DHFramework;
using UnityEngine;

namespace DH.Game
{
    public class AdController : Singleton<AdController>
    {
        #region json 序列化/反序列化所用的对象。字段不做规范化

        public class AdsParam : IReference
        {
            public string advertising_type;
            public string advertising_id;
            public string button_id;
            public string placements;

            public void Clear()
            {
            }
        }

        public class DataInMsg
        {
            public string advertising_type;
            public string advertising_id;
            public string method_name;
        }

        public class RetMsg
        {
            public int statusCode;
            public string data;
            public string message;
        }

        #endregion

        private enum AdsStatus
        {
            Loading = 1, //加载中
            Loaded = 2, //已加载
        }

        public readonly List<string> AdsIdList = new List<string>()
        {
#if UNITY_IOS
            //"ca-app-pub-3940256099942544/1712485313" //测试id
            "ca-app-pub-1022820009422209/4821109770",
            "ca-app-pub-1022820009422209/4678489609"
#else
            //"ca-app-pub-3940256099942544/6978759866" //测试id
            "ca-app-pub-1022820009422209/6000743353",
            "ca-app-pub-1022820009422209/2061498348"
#endif
        };

        private readonly string placements = "";

        //保存已经加载成功的广告id列表
        private Dictionary<string, AdsStatus> adsStatusDic = new Dictionary<string, AdsStatus>(); //记录加载中或已加载好的Ads
        private Dictionary<string, bool> canRewardDic = new Dictionary<string, bool>(); //对应的广告id是否可以给奖励
        private const string AdShowCallback = "Advertising_showAdCallback";
        private const string AdCallback = "Advertising_addAdCallback";
        private AutoResetUniTaskCompletionSource<bool> tcs;

        public void Init()
        {
            // 监听加载广告
            Usdk.Subscribe(AdCallback, OnAddAdResult);
            // 监听展示广告
            Usdk.Subscribe(AdShowCallback, OnShowAdResult);
            
            InitLoadRewardAds();
        }

        protected override void Release()
        {
            //取消绑定监听
            Usdk.Unsubscribe(AdCallback, OnAddAdResult);
            Usdk.Unsubscribe(AdShowCallback, OnShowAdResult);
        }

        public void ShowRewardAds(Action rewardCallback)
        {
            ShowRewardAds(string.Empty,rewardCallback);
        }
        
        /// <summary>
        /// 展示广告
        /// </summary>
        /// <param name="buttonId">激励点位，自定义字符串</param>
        /// <param name="rewardCallback">给奖励的回调</param>
        public void ShowRewardAds(string buttonId, Action rewardCallback,Action sendAdUploadCb = null)
        {
            HandleAd(buttonId, rewardCallback,sendAdUploadCb).Forget();
        }

        private async UniTask HandleAd(string buttonId, Action rewardCallback,Action sendAdUploadCb = null)
        {
            var result = await ShowRewardAds(buttonId);
            if (!result)
            {
                return;
            }
            rewardCallback?.Invoke();
            //添加广告看完后的计数
            if (Application.platform != RuntimePlatform.IPhonePlayer)
            {
                DataCenter.charcaterData.AddAdTimes();
                ReportedManager.Instance.SendAdUpload(sendAdUploadCb:sendAdUploadCb).Forget();
            }
        }
        
        /// <summary>
        /// 展示广告
        /// </summary>
        /// <param name="buttonId">激励点位，自定义字符串</param>
        private async UniTask<bool> ShowRewardAds(string buttonId)
        {
            var showSuccess = false;
            var adsCount = AdsIdList.Count;
            tcs = AutoResetUniTaskCompletionSource<bool>.Create();

            for (int i = 0; i < adsCount; ++i)
            {
                var adsId = AdsIdList[0];
                AdsIdList.RemoveAt(0);
                AdsIdList.Add(adsId); //放到最后面去，避免一直播放第一个广告id

                bool success = ShowRewardAds(adsId, buttonId);
                if (success)
                {
                    showSuccess = true;
                    break;
                }
            }

            if (!showSuccess)
            {
                string str = "播放广告失败，请稍候再试"; //ConfigCenter.GetGlobalString(GlobalLanguageId.ConfAdLoading); //TODO
                str = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.Shop_tips13).Name;
                ToastManager.Show(str);
                tcs.TrySetResult(false);
            }

            var result = await tcs.Task;
            tcs = null;
            return result;
        }

        /// <summary>
        /// 加载广告
        /// </summary>
        /// <param name="adsIdList">广告位id列表</param>
        private void InitLoadRewardAds()
        {
            foreach (var adsId in AdsIdList)
            {
                LoadRewardAds(adsId);
            }
        }

        /// <summary>
        /// 加载广告
        /// </summary>
        /// <param name="adsId">广告位id</param>
        private void LoadRewardAds(string adsId)
        {
            if (adsStatusDic.ContainsKey(adsId))
            {
                DHLog.Debug($"[Ads]LoadRewardAds {adsId} is loading");
                return;
            }

            DHLog.Debug($"[Ads]LoadRewardAds begin load {adsId}");
            adsStatusDic.Add(adsId, AdsStatus.Loading);

            AdsCallService(adsId, "Advertising_addAd");
        }

        /// <summary>
        /// 展示广告
        /// </summary>
        /// <param name="adsId">广告位id</param>
        /// <param name="buttonId">激励点位，自定义字符串</param>
        /// <param name="rewardCallback">给奖励的回调</param>
        private bool ShowRewardAds(string adsId, string buttonId)
        {
            DHLog.Debug($"[Ads]ShowRewardAds for {adsId}");
#if UNITY_EDITOR
            tcs?.TrySetResult(true);
            return true;
#endif
            if (!adsStatusDic.ContainsKey(adsId) || adsStatusDic.TryGetValue(adsId, out var status) && status != AdsStatus.Loaded)
            {
                LoadRewardAds(adsId); //没有加载过需要重新加载
                ULogClient.ReportInfo("ad_notloading",string.Empty,string.Empty);
                return false;
            }

            adsStatusDic.Remove(adsId);
            ActivityManager.Instance.Show(WaitType.UI);
            AdsCallService(adsId, "Advertising_showAd", buttonId);

            return true;
        }

        private void AdsCallService(string adsId, string serviceName, string buttonId = "")
        {
            var param = ReferencePool.Acquire<AdsParam>();
            param.advertising_id = adsId;
            param.advertising_type = "reward";
            param.button_id = buttonId;
            param.placements = placements;

            var paramJson = DHUtility.Json.ToJson(param);
            Usdk.CallService(serviceName, paramJson);
            ReferencePool.Release(param);
        }

        private void OnShowAdsFinish(string adsId)
        {
            ActivityManager.Instance.Hide(WaitType.UI);
            LoadRewardAds(adsId);
        }

        #region 监听回调

        public void OnAddAdResult(string msg)
        {
            DHLog.Debug($"[Ads]OnAddAdResult: {msg}");

            try
            {
                var ret = DHUtility.Json.ToObject<RetMsg>(msg);
                var data = DHUtility.Json.ToObject<DataInMsg>(ret.data);
                var adsId = data.advertising_id;

                if (ret.statusCode == 0)
                {
                    if (!adsStatusDic.ContainsKey(adsId))
                    {
                        adsStatusDic.Add(adsId, AdsStatus.Loaded);
                    }
                    else
                    {
                        adsStatusDic[adsId] = AdsStatus.Loaded;
                    }
                }
                else
                {
                    if (adsStatusDic.ContainsKey(adsId))
                    {
                        adsStatusDic.Remove(adsId);
                    }
                }
            }
            catch (Exception e)
            {
                adsStatusDic.Clear();
                DHLog.Error($"[Ads] OnAddAdResult {e.Message}\n{e.StackTrace}");
            }
        }

        private void OnShowAdResult(string msg)
        {
            DHLog.Debug($"[Ads]OnShowAdResult: {msg}");

            try
            {
                var ret = DHUtility.Json.ToObject<RetMsg>(msg);
                var data = DHUtility.Json.ToObject<DataInMsg>(ret.data);
                var adsId = data.advertising_id;
                var methodName = data.method_name;

                if (ret.statusCode == 0)
                {
                    if (methodName == "onEarnedReward")
                    {
                        if (canRewardDic.ContainsKey(adsId))
                        {
                            canRewardDic[adsId] = true;
                        }
                        else
                        {
                            canRewardDic.Add(adsId, true);
                        }
                    }
                    else if (methodName == "onAdClosed")
                    {
                        canRewardDic.TryGetValue(adsId, out var canReward);
                        OnShowAdsFinish(adsId);
                        tcs?.TrySetResult(canReward);
                        canRewardDic.Clear();
                    }
                }
                else
                {
                    string str = "广告正在加载中"; //ConfigCenter.GetGlobalString(GlobalLanguageId.ConfAdShowFail); //TODO
                    str = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.Shop_tips13).Name;
                    ToastManager.Show(str);
                    OnShowAdsFinish(adsId);
                    tcs?.TrySetResult(false);
                }
            }
            catch (Exception e)
            {
                ActivityManager.Instance.Hide(WaitType.UI);
                tcs?.TrySetResult(false);
                DHLog.Error($"[Ads] OnShowAdResult {e.Message}\n{e.StackTrace}");
            }
        }

        #endregion
    }
}