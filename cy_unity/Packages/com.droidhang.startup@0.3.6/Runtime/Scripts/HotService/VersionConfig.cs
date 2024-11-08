using System;
using UnityEngine;

namespace DH.HotService
{
    [Serializable]
    public class VersionConfig
    {
        //客户端游戏版本
        public string version;

        //abConfig文件的md5
        public string abConfigMd5;

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
}