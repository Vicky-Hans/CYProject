using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Config;
using DHFramework.Download;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.Scripting;

public class RefTypes
{
    [Preserve]
    private enum TestEnum
    {
        E1,
        E2,
    }
    [Preserve]
    private class RefInputClass : OnScreenControl
    {
        protected override string controlPathInternal { get; set; }

        [Preserve]
        protected void RefMethod()
        {
            base.SendValueToControl(new Vector2(0,0));
        }
    }
    
    [Preserve]
    private class RefCfgClass : LocalizeCollectionBase<RefCfgClass>
    {
        public void Add()
        {
            AddItem(new Dictionary<string,RefCfgClass>(),"",new RefCfgClass());
        }
    }
    
    [Preserve]
    public void MyAOTRefs()
    {
        _ = new ResourceComponent();
        _ = new DownloaderComponent();
        TestEnum tmpEnum = TestEnum.E1;
        _ = Unsafe.As<TestEnum, Int32>(ref tmpEnum);
        _ = new RefInputClass();
        new RefCfgClass().Add();
        Task().Forget();
        Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>("");
    }

    private async UniTaskVoid Task()
    {
        var result = await AssetsManager.LoadAssetAsync<TextAsset>("");
    }
}
