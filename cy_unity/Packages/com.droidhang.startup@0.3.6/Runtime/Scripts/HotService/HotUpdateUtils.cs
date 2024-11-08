using System.Collections;
using System.Collections.Generic;
using DH.NativeCore.Platform;
using DHFramework;
using DHFramework.Json;
using UnityEngine;

namespace DH.HotService
{
    public class HotUpdateUtils
    {
        public enum ClusterType {
            Normal = 1, //正式服
            Review = 2, //审核服
            Test = 3, //测试服,加了ip白名单,现在没用
        }
        
        public enum UpdateType
        {
            Update = 1, //小版本更新
            Store = 2, //商店更新
            NoNeedUpdate = 3, //不需要热更新
        }

        public static UpdateType GetUpdateType(string curVersion, string newVersion)
        {
            if (string.IsNullOrEmpty(newVersion))
            {
                return UpdateType.NoNeedUpdate;
            }
            
            var curV = GetSplitVersion(curVersion);
            var newV = GetSplitVersion(newVersion);

            if (newVersion == DeviceUtility.GetAppVersionName())
            {
                return UpdateType.NoNeedUpdate;
            }
            
            if (curV[0] == newV[0] && curV[1] == newV[1])
            {
                
                return UpdateType.Update;
            }

            return UpdateType.Store;
        }

        public static string GetVersion()
        {
            var appVersion = DeviceUtility.GetAppVersionName();
            var hotUpdateVersion = DHUnityUtil.PlayerPrefs.GetString(HotUpdateManager.HotUpdateVersionKey, "");
            
            if (string.IsNullOrEmpty(hotUpdateVersion))
            {
                return appVersion;
            }

            var res = CompareVersion(appVersion, hotUpdateVersion);
            
            if (res > 0)
            {
                //大版本比热更版本高
                return appVersion;
            }
            
            if(res < 0)
            {
                //大版本比热更版本低:如果a.b不相等,则直接返回appVersion,用于兼容内网包覆盖安装
                var appList = GetSplitVersion(appVersion);
                var hotUpdateList = GetSplitVersion(hotUpdateVersion);
                
                if (appList[0] != hotUpdateList[0] || appList[1] != hotUpdateList[1])
                {
                    return appVersion;
                }
                
                return hotUpdateVersion;
            }

            return appVersion;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns>v1 > v2: 返回1  v1 == v2:返回0  v1 < v2:返回-1</returns>
        public static int CompareVersion(string v1, string v2)
        {
            var version1 = GetSplitVersion(v1);
            var version2 = GetSplitVersion(v2);

            for (int i = 0; i < version1.Length; ++i)
            {
                var s1 = version1[i];
                var s2 = version2[i];

                if (s1 > s2)
                {
                    return 1;
                }else if (s1 < s2)
                {
                    return -1;
                }
            }

            return 0;
        }
        
        public static string GetHotUpdateMD5()
        {
            return DHUnityUtil.PlayerPrefs.GetString(HotUpdateManager.HotUpdateMd5Key, "");
        }

        public static ClusterType GetClusterType()
        {
            var clusterType = (int)Usdk.LoginCenterConfigDic.ReadValue<long>(DH.LoginCenterDefine.ClusterType);
            return (ClusterType) clusterType;
        }
        
        public static bool IsReviewCluster()
        {
            return GetClusterType() == ClusterType.Review;
        }
        
        public static bool IsEmulator()
        {
            return Usdk.CallFunction("Utils_isEmulator", "") == "true";
        }

        private static int[] GetSplitVersion(string version)
        {
            var trimStr = version.Trim();
            string[] vLists = trimStr.Split('.');
            int[] vIntValues = new int[vLists.Length];

            for (int i = 0; i < vIntValues.Length; ++i)
            {
                int.TryParse(vLists[i], out var num);
                vIntValues[i] = num;
            }
            
            return vIntValues;
        }
    }
}
