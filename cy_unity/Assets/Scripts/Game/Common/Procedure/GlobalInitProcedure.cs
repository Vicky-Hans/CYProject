using System;
using System.Resources;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Config;
using DH.Game;
using DH.Game.UI;
using DH.Game.UIViews;
using DHFramework;
using DH.Game.ViewModels;
using DH.UIFramework;
using DHFramework.Localization;
using TF.Base;
using UnityEngine;

[ProcedureDeep(1)]
public class GlobalInitProcedure : ProcedureBase
{
    private bool bEnterLogin = false;
    private ShaderVariantCollection shaderVariantCollection;
    private GameObject systemFont;

    private async UniTask SetupUDownload()
    {
        var tcs = new UniTaskCompletionSource();
        UDownload.Setup(DHAssetsConfig.ReadWritePath, DHAssetsConfig.ReadOnlyPath, DHAssetsConfig.RuntimeDataPath,
            (result) => { tcs.TrySetResult(); });
        await tcs.Task;
    }

    protected override async UniTask Enter()
    {
        BaseView.LocalizeRegister += LocalizeHelper.RegisterLocalize;
        BaseView.LocalizeUnregister += LocalizeHelper.UnRegisterLocalize;
        MinMapMaskSettings.Enable = true;
        ActivityManager.Instance.Init();
        Localization.RegisterLocalize(GetHashCode(), OnLocalize);
        await ToastManager.Instance.Init();
        AudioManager.Instance.Init();
        await SetupUDownload();
        shaderVariantCollection = await AssetsManager.LoadAssetAsync<ShaderVariantCollection>("force_compile");
        shaderVariantCollection.WarmUp();
        // systemFont =
        //     await AssetsManager.InstantiateWithParentAsync("UI/SystemFont", AppGlobal.Instance.GlobalMono.transform,
        //         false);
        // 提前加载UI，防止出现黑屏
        var view = await UIManager.Instance.OpenDialog<StartGameMenu, StartGameViewModel>();
        
    }

    private async UniTask OnLocalize()
    {
        DataTableManager.LanguageChanged(Localization.GetCurrentLanguage());
        await ConfigCenter.GlobalLanguageCfgColl.LoadAsync();
        // 转接一下防止多语言文本未加载完成就直接通知UI
        await LocalizeHelper.NotifyOnLocalize();
    }

    protected override void Exit()
    {
        BaseView.LocalizeRegister -= LocalizeHelper.RegisterLocalize;
        BaseView.LocalizeUnregister -= LocalizeHelper.UnRegisterLocalize;
        Localization.UnRegisterLocalize(GetHashCode());
        UIManager.Instance.Release();
        ToastManager.Instance.Dispose();
        ActivityManager.Instance.Dispose();
        AssetsManager.Release(shaderVariantCollection);
        // AssetsManager.ReleaseInstance(systemFont);
    }

    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
        ToastManager.Instance.Update();
        AudioManager.Instance.Update(elapseSeconds);
         UIManager.Instance.OnUpdate();
        if (ProcedureManager.Instance.IsCurrent(ProcedureConfigKey))
        {
            if (!bEnterLogin && shaderVariantCollection != null && shaderVariantCollection.isWarmedUp)
                ProcedureManager.Instance.Change(nameof(LoginProcedure)).Forget();
        }
    }
}