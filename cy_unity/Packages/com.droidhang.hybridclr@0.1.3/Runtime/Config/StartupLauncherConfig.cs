using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StartupLauncherConfig", menuName = "HybridCLR/StartupLauncherConfig")]
public class StartupLauncherConfig : ScriptableObject
{
    [Header("挂载StartUI的CanvasRoot")]
    public string CanvsRoot;
    [Header("是否开启HybridCLR")]
    public bool EnableHybridCLR = true;
    [Header("是否启用global-metadata.dat加密")]
    public bool EnabledEncryptGlobalMetadata = true;
    [Header("依赖的热更新的dll列表（注意依赖顺序）")]
    public List<string> PreLoadDllList;
    [Header("启动的dll的名字")]
    public string StartDllName;
    [Header("类的名称")]
    public string StartTypeName;
    [Header("方法名称")]
    public string StartMethodName;
    [Header("是否自动释放StartupDlg")]
    public bool AutoDestroyStartupDlg = true;
    [Header("是否使用异步启动")]
    public bool UseAsyncAwait = false;
    //是否是正式包
    [Header("是否是Release")]
    public bool EnableRelease;
    [Header("Game Code")]
    public string GameCode;
    [Header("Game Name")]
    public string GameName;
    
    [Header("用户协议")]
    public string UserAgreement;
    [Header("隐私协议")]
    public string PrivacyAgreement;
    [Header("协议UI支持的语言")]
    public List<string> AgreementSupportLanguages;

    [Header("启动UI的地址")] public string StartupMainUIPath;
    [Header("用户协议UI的地址")] public string UserAgreementUIPath;
    [Header("二次确认框UI的地址")] public string MessageBoxUIPath;
    [Header("登录ui的地址")] public string LoginUIPath;
    [Header("启动过程多语言的地址（语言用{0}代码）")] public string StartupLanguageConfigPath;
    [Header("本地化配置的地址")] public string LocalizationConfigPath;
}
