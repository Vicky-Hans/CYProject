using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HybridCLR;
using UnityEngine;

namespace DHHybridCLR.Scripts
{
    public partial class GameRoot
    {
#if !(UNITY_WEBGL || WECHAT_MINI)
        public IEnumerator LoadMetadataForAotAssembly(string fileName, string password)
        {
            DHFramework.DHLog.Debug("[GameRoot::LoadMetadataForAotAssembly] Start");
            yield return LoadData(fileName, password, OnLoadMetadataData);
        }
        
        private static unsafe void OnLoadMetadataData(Dictionary<string, byte[]> dllBytes)
        {
            if (dllBytes == null)
            {
                dllBytes = new Dictionary<string, byte[]>();
            }
            
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            DHFramework.DHLog.Debug($"[GameRoot::LoadMetadataForAotAssembly] Count:{dllBytes.Count}");
            foreach (var dll in dllBytes)
            {
                DHFramework.DHLog.Debug($"[GameRoot::LoadMetadataForAotAssembly] load {dll.Key}");

                try
                {
                    var err = (LoadImageErrorCode) RuntimeApi.LoadMetadataForAOTAssembly(dll.Value, mode);
                    if (err != LoadImageErrorCode.OK)
                    {
                        DHFramework.DHLog.Debug($"[GameRoot::LoadMetadataForAotAssembly] load {dll.Key} ret:{err}");
                    }
                }
                catch (Exception ex)
                {
                    DHFramework.DHLog.Debug($"[GameRoot::LoadMetadataForAotAssembly] load {dll.Key} exception");
                    Debug.LogException(ex);
                }
            }

            DHFramework.DHLog.Debug($"[GameRoot::LoadMetadataForAotAssembly] finished!");
        }
#endif
    }
}
