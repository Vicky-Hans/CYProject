using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DH.UIFramework;
using DHFramework;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using Object = UnityEngine.Object;

public static class AssetConfig
{
    private static List<string> FirstPackLua = new List<string>()
    {
        "System.lua",
        "Launcher.lua",
        "HotUpdateView.lua",
        "HotUpdateViewModel.lua"
    };
    
    public static void SetAddressableGroup(this Object obj, string groupName,string filePrefix)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
 
        if (settings)
        {
            var group = settings.FindGroup(groupName);
            if (!group)
                group = settings.CreateGroup(groupName, false, false, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
 
            var assetpath = AssetDatabase.GetAssetPath(obj);
            var guid = AssetDatabase.AssetPathToGUID(assetpath);
 
            var e = settings.CreateOrMoveEntry(guid, group, false, false);
            // flat path
            int index = assetpath.IndexOf(filePrefix,StringComparison.CurrentCultureIgnoreCase);
            e.address = $"{assetpath.Substring(index)}";
            e.SetLabel(groupName,true);
            var entriesAdded = new List<AddressableAssetEntry> {e};
 
            group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false, true);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true, false);
        }
    }

    private static bool CheckInFirstPack(string filePath)
    {
        foreach (var item in FirstPackLua)
        {
            if (filePath.Contains(item))
            {
                return true;
            }
        }

        return false;
    }

    [MenuItem("Tools/RenameLua")]
    public static void RenameLuaFiles()
    {
        int BaseLength = Application.dataPath.Length - "Assets".Length;
        List<string> firstPackFiles = new List<string>();
        
        foreach (string dir in Directory.GetDirectories(Application.dataPath, "LuaScript", SearchOption.AllDirectories))
        {
            var files = Directory.GetFiles(dir, "*lua.txt", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var target = file.Replace(".lua.txt", ".lua");
                if (File.Exists(target))
                {
                    File.Delete(target);
                }
                File.Move(file,target);
            }
        }
        
        AssetDatabase.Refresh();
    }

    //[MenuItem("Tools/AssetBundle/SetLuaScriptGroup")]
    public static void SetLuaScriptGroup()
    {
        const string BasePath = "Assets/com.droidhang.uiframework";
        int BaseLength = Application.dataPath.Length - "Assets".Length;
        List<string> firstPackFiles = new List<string>();
        
        foreach (string dir in Directory.GetDirectories(Application.dataPath, "LuaScript", SearchOption.AllDirectories))
        {
            var files = Directory.GetFiles(dir, "*lua", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var relativePath = file.Substring(BaseLength );
                if (CheckInFirstPack(relativePath))
                {
                    firstPackFiles.Add(relativePath);
                    continue;
                }
                
                var asset = AssetDatabase.LoadAssetAtPath<Object>(relativePath);
                asset.SetAddressableGroup("LuaScript","LuaScript");
            }
        }

        foreach (var item in firstPackFiles)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(item);
            asset.SetAddressableGroup("FirstLuaScript","LuaScript");
        }
    }
}
