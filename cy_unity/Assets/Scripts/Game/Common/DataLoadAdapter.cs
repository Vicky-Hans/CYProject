using System.Data;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Config;
using UnityEngine;

namespace DH.Game
{
    public class DataLoadAdapter : ILoadConfigAdapter
    {
        public byte[] LoadConfigBytes(string configPath)
        {
            var assets = AssetsManager.LoadAssetSync<TextAsset>(configPath);
            var bytes = assets.bytes;
            AssetsManager.Release(assets);
            return bytes;
        }

        public string LoadConfigText(string configPath)
        {
            var assets = AssetsManager.LoadAssetSync<TextAsset>(configPath);
            var text = assets.text;
            AssetsManager.Release(assets);
            return text;
        }

        public async UniTask<byte[]> LoadConfigBytesAsync(string configPath)
        {
            var assets = await AssetsManager.LoadAssetAsync<TextAsset>(configPath);
            var bytes = assets.bytes;
            AssetsManager.Release(assets);
            return bytes;
        }

        public async UniTask<string> LoadConfigTextAsync(string configPath)
        {
            var assets = await AssetsManager.LoadAssetAsync<TextAsset>(configPath);
            var text = assets.text;
            AssetsManager.Release(assets);
            return text;
        }
    }
}