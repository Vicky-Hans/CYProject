using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "AOTMetaAssemblyManifest", menuName = "HybridCLR/AOTMetaAssemblyManifest")]
public class AOTMetaAssemblyManifest : ScriptableObject
{
    [Header("提前的AOT 补充元数据dll列表")]
    public string[] DefaultAOTMetadataDlls = new string[] {"mscorlib", "System", "System.Core" };
    [Header("AOT 补充元数据dll列表")]
    public string[] AOTMetadataDlls = new string[] { };
}
