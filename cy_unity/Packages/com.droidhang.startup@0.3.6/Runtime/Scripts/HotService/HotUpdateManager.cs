using System;
using DH.Log;
using DH.UNet.Utility;
using UnityEngine;
using DHFramework;


#if !UNITY_WEBGL
using DHFramework.Download;
#endif


namespace DH.HotService
{
    public enum DownloadState
    {
        Complete = 0,
        /// <summary>
        /// 第一阶段下载，进入登陆界面前必须下载完成资源
        /// </summary>
        FirstPart = 1,
        /// <summary>
        /// 第二阶段下载，进入登陆界面前必须下载完成资源
        /// </summary>
        SecondPart = 2,
        /// <summary>
        /// 静默更新阶段，在条件允许的情况下可以进入游戏后进行更新
        /// </summary>
        ThirdPart = 3,
        /// <summary>
        /// 下载失败
        /// </summary>
        DownloadFailed = 4,
    }
    
    /// <summary>
    /// 提供进入游戏后的热更新状态维护
    /// </summary>
    public class HotUpdateManager : Singleton<HotUpdateManager>
    {
        public static readonly string HotUpdateVersionKey = "hotupdate_version";
        public static readonly string HotUpdateMd5Key = "hotupdate_md5";
        
        private readonly string cachedSidKey = "BPC_SID_KEY";

        private DownloadState state;
        private bool disposed;
        //服务器版本信息
        private NativeCore.Version rspVsn;
        
        public DownloadState State => state;

        /// <summary>
        /// 稳定版本热更新时，先尝试获取分服热更新缓存sid，若存在将使用上一次未完成的热更新sid
        /// </summary>
        /// <returns></returns>
        public int GetCacheSid()
        {
            int result = DHUnityUtil.PlayerPrefs.GetInt(cachedSidKey, 0);
            if (result != 0)
            {
                DHLog.Debug($"获取到缓存的分服热更新sid:{result}");
            }
            return result;
        }

        public void SetCacheSid(int sid)
        {
            if (sid != 0)
            {
                // 分服更新模式下缓存服务器ID，在热更新完成后清除
                DHUnityUtil.PlayerPrefs.SetInt(cachedSidKey,sid);
            }
        }

        public void ClearCacheSid()
        {
            if (!DHUnityUtil.PlayerPrefs.HasKey(cachedSidKey))
            {
                return;
            }
            
            DHUnityUtil.PlayerPrefs.DeleteKey(cachedSidKey);
            DHUnityUtil.PlayerPrefs.Save();
            DHLog.Debug("清理热更新缓存Sid数据");
        }

#if !UNITY_WEBGL
        /// <summary>
        /// 不能直接使用UDownload的Retry
        /// 调用后将重制状态
        /// </summary>
        public void RetryDownload()
        {
            state = DownloadState.FirstPart;
            UDownload.RetryDownload();
        }
        
        public void StartDownload(DH.NativeCore.Version vsn)
        {
            disposed = false;
            state = DownloadState.FirstPart;
            this.rspVsn = vsn;
            
            UDownload.OnDownloadCompleted += CompleteCallback;
            UDownload.OnGroupDownloadCompleted += GroupDownloadCompleted;
        }

        /// <summary>
        /// 辅助函数，用于获取Dictionary中的数据
        /// get_Item不支持key为int的
        /// </summary>
        /// <param name="info"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static GroupInfo GetGroupInfo(DetailInfo info, int group)
        {
            if (info.groupInfos.TryGetValue(group, out var item))
            {
                return item;
            }

            return null;
        }
#endif
        
        
        public void Shutdown()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
#if !UNITY_WEBGL
            UDownload.OnDownloadCompleted -= CompleteCallback;
            UDownload.OnGroupDownloadCompleted -= GroupDownloadCompleted;
            UDownload.Shutdown();
#endif
            DHLog.Debug("HotUpdate Manager release");
        }

        protected override void Release()
        {
            Shutdown();
        }
        
        private void GroupDownloadCompleted(int priority, bool success)
        {
            if (!success)
            {
                state = DownloadState.DownloadFailed;
                return;
            }
            
            var targetState = (DownloadState)(priority + 1);
            // 重试情况下会直接从后续段跳到第一段
            if (state > targetState && state != DownloadState.FirstPart)
            {
#if !(UNITY_WEBGL || WECHAT_MINI)
                Debug.LogException(new ArgumentException($"GroupDownloadCompleted with unexpected state {targetState}"));
#else
                Debug.LogException(new ArgumentException($"GroupDownloadCompleted with unexpected state {targetState}"));
#endif
                return;
            }

            state = targetState;
        }

        private void CompleteCallback(bool success, int code)
        {
            state = success ? DownloadState.Complete : DownloadState.DownloadFailed;

            if (success)
            {
                // 保存最新的版本信息到本地
                DHLog.Debug( "[热更] [HotUpdateTask CompleteCallback] 热更完成");
                //这些信息按理说应该在Lua侧保存,但是由于在热更过程中可能重启,导致Lua没有一个全局存在的可接受事件的类,所以在这里写入
                DHUnityUtil.PlayerPrefs.SetString(HotUpdateVersionKey,rspVsn.vsn);
                DHUnityUtil.PlayerPrefs.SetString(HotUpdateMd5Key,rspVsn.md5);
                // 清空Bundle丢失标记
                DHUnityUtil.PlayerPrefs.Save();
                ULogClient.SetHotUpdateVersion(rspVsn.vsn);
                Shutdown();
                ClearCacheSid();
            }
        }
    }
}