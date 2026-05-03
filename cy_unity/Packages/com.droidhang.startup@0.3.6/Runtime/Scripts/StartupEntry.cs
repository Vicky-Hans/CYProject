using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Config;
using DH.NativeCore.MonoSingleton;
using DH.NativeCore.Platform;
using DHFramework;
using DHFramework.Localization;
using DHHybridCLR.Scripts;
using UnityEngine;

namespace DH.Launch
{
    /// <summary>
    /// 负责lua环境的初始化及释放、管理启动界面UI、启动初始化阶段1
    /// </summary>
    public partial class StartupEntry : MonoSingleton<StartupEntry>
    {
        public GameObject StartupUIRoot { get; private set; }
        public TaskAssembler TaskEntry { get; private set; }
        public StartupLanguageCfgCollection LanguageConfig = new StartupLanguageCfgCollection();
        public Transform CanvasRootTrans{get;set;} //用于挂启动UI
        
        //用于分服热更新
        private int sid = 0;
        private bool beginStartGame = false;
        private Action releaseFunc;
        private Coroutine luaCoroutine;
        private bool autoDestroyStartupDlg;

#if UNITY_WEBGL || WECHAT_MINI
        public Action WebGLStartGame = null;
#endif

        /// <summary>
        /// 只能全局只调用一次，程序的入口函数
        /// </summary>
        public async UniTaskVoid Startup()
        {
            StartupConfig = GameRoot.Instance.StartupConfig;
            var canvasRootGo = GameObject.Find(StartupConfig.CanvsRoot);
            autoDestroyStartupDlg = StartupConfig.AutoDestroyStartupDlg;
            CanvasRootTrans = canvasRootGo ? canvasRootGo.transform : null;
            DHLog.Debug("[Startup] AssetsManager.Instance.Init");
            AssetsManager.Instance.Init(UniTask.Action(async () =>
            {
                await StartLauncherTask();
            }));
        }
    
        /// <summary>
        /// 选择一个服务器后发现有热更新，调用此接口
        /// </summary>
        /// <param name="sid">服务器id</param>
        /// <param name="releaseFunc">当前lua环境的的release函数</param>
        public async void HotUpdateForSid(int sid, Action releaseFunc)
        {
            DHLog.Debug("[BeginStartGame] begin HotUpdateForSid");
            await InitStartupPage();
            this.releaseFunc = releaseFunc;
            this.sid = sid;
            StartCoroutine(BeginHotUpdateForSid(sid));
        }

        private async UniTask StartLauncherTask()
        {
            await Localization.InitSimple(GameRoot.Instance.StartupConfig.LocalizationConfigPath,DeviceUtility.GetLanguage);
            await InitConfig();
            TaskEntry = new TaskAssembler();
            await InitStartupPage();
            await TaskEntry.LauncherTasks(OnComplete, OnComplete);
        }
        /// <summary>
        /// 初始化读startup配置
        /// </summary>
        private async UniTask InitConfig()
        {
            var languageCode = Localization.GetCurrentLanguage();
            DataTableManager.LanguageChanged(languageCode);
            
            await LanguageConfig.LoadAsync();
        }

        /// <summary>
        /// 启动阶段完成的回调，需要更新catalog
        /// </summary>
        private void OnComplete()
        {
            AssetsManager.UpdateLocationData(() =>
            {
                beginStartGame = true;
            });
        }

        /// <summary>
        /// 从登录界面来的热更新完成事件
        /// </summary>
        private void OnCompleteFromGameLogic()
        {
            TaskEntry.TaskMgr.TipRestartGame();
        }
        
        private void OnUpdate()
        {
            if (TaskEntry != null)
            {
                TaskEntry.Update(Time.deltaTime, Time.unscaledDeltaTime);
            }
            if (!beginStartGame) return;
            beginStartGame = false;
            if (StartupConfig.UseAsyncAwait)
            {
                ExecuteStartGameAsync().Forget();
            }
            else
            {
                ExecuteStartGame();
            }
        }
        /// <summary>
        /// 选服后发现需要热更新，释放上一次lua环境，重新启动初始化阶段1
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected IEnumerator BeginHotUpdateForSid(int sid)
        {
            yield return new WaitForEndOfFrame();
            
            releaseFunc?.Invoke();
            this.releaseFunc = null;
        
            yield return new WaitForEndOfFrame();

            TaskEntry.HotUpdateForSid(sid, OnCompleteFromGameLogic);
        }

        #region 启动界面

        /// <summary>
        /// 显示启动界面，用于显示初始化的文字描述和进度条
        /// </summary>
        private async UniTask InitStartupPage()
        {
            if (!StartupUIRoot)
            {
                var path = StartupConfig.StartupMainUIPath;
                StartupUIRoot = await AssetsManager.InstantiateWithParentAsync(path, CanvasRootTrans, false);
                if (StartupUIRoot)
                {
                    RectTransform rect = StartupUIRoot.GetComponent<RectTransform>();
                    rect.anchoredPosition = Vector2.zero;
                }
            }
        }

        /// <summary>
        /// 删除启动界面
        /// </summary>
        public void DestroyStartupPage()
        {
            if (StartupUIRoot)
            {
                AssetsManager.Release(StartupUIRoot);
                StartupUIRoot = null;
            }
        }
    
        #endregion

        #region 启动游戏

        private void ExecuteStartGame()
        {
            StartGame();
            if (autoDestroyStartupDlg)
            {
                DestroyStartupPage();
            }
        }

        private void StartGame()
        {
            using (new Benchmark("[Startup] StartGame"))
            {
                Debug.Log("[GameRoot::RunDll] Begin");

#if UNITY_WEBGL || WECHAT_MINI
                WebGLStartGame?.Invoke();
                WebGLStartGame = null;
#else
                System.Reflection.Assembly gameDll = AppDomain.CurrentDomain.GetAssemblies()
                    .First(assembly => assembly.GetName().Name == StartupConfig.StartDllName);

                if (gameDll == null)
                {
                    Debug.LogError($"[GameRoot::RunDll] {StartupConfig.StartDllName} is null");
                    return;
                }

                var appType = gameDll.GetType(StartupConfig.StartTypeName);
                if (appType == null)
                {
                    Debug.LogError($"[GameRoot::RunDll] {StartupConfig.StartTypeName} is null");
                    return;
                }


                var mainMethod = appType.GetMethod(StartupConfig.StartMethodName);
                if (mainMethod == null)
                {
                    Debug.LogError($"[GameRoot::RunDll] {StartupConfig.StartMethodName} is null");
                    return;
                }

                mainMethod.Invoke(null, null);
#endif

            }
        }

        #endregion
    }
}
