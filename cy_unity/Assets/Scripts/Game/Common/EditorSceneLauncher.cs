using System;
using System.IO;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Data;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using DH.Game;
using DH.Game.UI;
using DH.Proto;
using Google.Protobuf;
using UnityEngine;

namespace DH.Game
{
    public class EditorSceneLauncher : MonoBehaviour
    {
        [AssetPath] public string levelRootPath;

        private void Start()
        {
            StartGameEditor().Forget();
        }

        private async UniTaskVoid StartGameEditor()
        {
            var levelRoot =
                await AssetsManager.InstantiateWithParamsAsync(levelRootPath, Vector3.zero, Quaternion.identity, null);
            var rootName = GetRootName();
            if (String.Compare(rootName, "LevelRoot", StringComparison.Ordinal) == 0)
            {
                AudioManager.Instance.Init();
            }else if (String.Compare(rootName, "FightingRoot", StringComparison.Ordinal) == 0)
            {
            }
        }
        
        private string GetRootName()
        {
            return Path.GetFileNameWithoutExtension(levelRootPath);
        }
    }
}