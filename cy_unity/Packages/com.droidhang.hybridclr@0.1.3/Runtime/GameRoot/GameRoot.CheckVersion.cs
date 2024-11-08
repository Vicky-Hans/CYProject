using UnityEngine;

namespace DHHybridCLR.Scripts
{
    public partial class GameRoot
    {
        private static readonly string HotUpdateVersionKey = "hotupdate_version";
        private static readonly string HotUpdateMd5Key = "hotupdate_md5";
        
        public bool IsBaseVersion()
        {
            var curVersion = GetVersion();
            if (curVersion == Application.version)
            {
                return true;
            }

            return false;
        }
        
        public static string GetVersion()
        {
            var appVersion = Application.version;
            var hotUpdateVersion = DHUnityUtil.PlayerPrefs.GetString(HotUpdateVersionKey, "");
            
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
