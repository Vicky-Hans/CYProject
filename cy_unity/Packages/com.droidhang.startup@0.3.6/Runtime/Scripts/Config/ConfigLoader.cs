using DH.Config;
using UnityEngine;
using System;
using System.IO;
using Cysharp.Threading.Tasks;
using DH.Asset;

namespace DH.Launch
{
    public class ConfigLoader : ILoadConfigAdapter
    {
        public byte[] LoadConfigBytes(string configPath)
        {
            TextAsset configFile = AssetsManager.LoadAssetSync<TextAsset>(configPath);

            if (!configFile)
            {
                throw new Exception($"{configPath} 的配置读取失败");
            }

            return configFile.bytes;
        }

        public string LoadConfigText(string configPath)
        {
            throw new Exception($"{configPath} 的配置读取失败 在启动流程里默认就使用bson文件");
        }
        
        public async UniTask<byte[]> LoadConfigBytesAsync(string configPath)
        {
            TextAsset configFile = await AssetsManager.LoadAssetAsync<TextAsset>(configPath);

            if (!configFile)
            {
                throw new Exception($"{configPath} 的配置读取失败");
            }

            var bytes = configFile.bytes;
            AssetsManager.Release(configFile);
            return bytes;
        }

        public async UniTask<string> LoadConfigTextAsync(string configPath)
        {
            throw new Exception($"{configPath} 的配置读取失败 在启动流程里默认就使用bson文件");
        }
        
    }
}