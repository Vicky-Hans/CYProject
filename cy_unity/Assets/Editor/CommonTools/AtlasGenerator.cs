using System;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine.AssetGraph;
using UnityEngine.U2D;

namespace DH.Editor
{
    [System.Serializable]
    [CustomAssetGenerator("Atlas Generator", "v1.0", 1)]
    public class AtlasGenerator : IAssetGenerator
    {
        public void OnValidate()
        {
           
        }

        public string GetAssetExtension(AssetReference asset)
        {
            return ".spriteatlasv2";
        }

        public Type GetAssetType(AssetReference asset)
        {
            return typeof(Object);
        }

        public bool CanGenerateAsset(AssetReference asset)
        {
            return AssetDatabase.IsValidFolder(asset.path);
        }

        public bool GenerateAsset(AssetReference asset, string generateAssetPath)
        {
            var path = generateAssetPath;
            SpriteAtlasAsset atlas = SpriteAtlasAsset.Load(path);
            if (!atlas)
            {
                atlas = new SpriteAtlasAsset();
            }

            atlas.Add(new UnityEngine.Object[]{AssetDatabase.LoadAssetAtPath(asset.path,typeof(Object))});
            SpriteAtlasAsset.Save(atlas,generateAssetPath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(generateAssetPath);
            return true;
        }

        public void OnInspectorGUI(Action onValueChanged)
        {
            
        }

        public bool NeedGenerateAsset(string path)
        {
            return !File.Exists(path);
        }
    }
}