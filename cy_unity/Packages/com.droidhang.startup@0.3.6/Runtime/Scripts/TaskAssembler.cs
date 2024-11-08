using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.HotService;
using DH.Log;
using DHFramework;
using UnityEngine;

namespace DH.Launch
{
    public class TaskAssembler
    {
        public enum TaskDescCode {
            CheckNetwork = 1, //检查网络
            PrepareConfig = 2, //准备配置
            InitSdk = 3, //初始化SDK
            CheckHotUpdate = 4, //检查热更新
            HotUpdate = 5, //热更新
            ApkDownload = 6, //下载apk
            LoadingGame = 7, //游戏加载中
        }

        public TaskManager TaskMgr => taskMgr;
        
        private TaskManager taskMgr { get; set; }

        public TaskAssembler()
        {
            taskMgr = new TaskManager();
        }
        
        /// <summary>
        /// 初始化阶段1启动入口
        /// </summary>
        /// <param name="hotUpdateComplete">热更新完成回调</param>
        public async UniTask LauncherTasks(Action hotUpdateComplete, Action noNeedUpdate = null)
        {
            taskMgr.Init();

            var loadMetadataTask = new LoadMetadataTask();
            loadMetadataTask.Init((int)TaskDescCode.PrepareConfig, 1);
            taskMgr.AddTask(loadMetadataTask);

            var loadConfigTask = new LoadConfigTask();
            loadConfigTask.Init((int)TaskDescCode.PrepareConfig, 1);
            taskMgr.AddTask(loadConfigTask);
            
#if !UNITY_WEBGL && !WECHAT_MINI
            var userAgreementTask = new UserAgreementTask();
            userAgreementTask.Init((int) TaskDescCode.PrepareConfig, 1);
            taskMgr.AddTask(userAgreementTask);
#endif
            
            var initSDKTask = new InitSDKTask();
            initSDKTask.Init((int)TaskDescCode.InitSdk, 1);
            taskMgr.AddTask(initSDKTask);

            var requestConfigTask = new RequestFullConfigTask();
            requestConfigTask.Init((int)TaskDescCode.PrepareConfig, 1);
            taskMgr.AddTask(requestConfigTask);

            var checkUpdateTask = new CheckHotUpdateTask();
            checkUpdateTask.Init((int)TaskDescCode.CheckHotUpdate, 1);
            taskMgr.AddTask(checkUpdateTask);
            
#if !UNITY_WEBGL && !WECHAT_MINI

            var hotUpdateTask = new HotUpdateTask();
            hotUpdateTask.Init((int)TaskDescCode.CheckHotUpdate, 1);
            taskMgr.AddTask(hotUpdateTask);

            var config = StartupEntry.Instance.StartupConfig;
            List<string> dllList = config.PreLoadDllList;
            if (dllList.Count > 0)
            {
                foreach (var dllName in dllList)
                {
                    var loadPreDllListTask = new LoadPreDllListTask();
                    loadPreDllListTask.SetLoadDllName(dllName);
                    loadPreDllListTask.Init((int)TaskDescCode.LoadingGame, 1);
                    taskMgr.AddTask(loadPreDllListTask);
                }
            }

            {
                var loadPreDllListTask = new LoadPreDllListTask();
                loadPreDllListTask.SetLoadDllName(config.StartDllName);
                loadPreDllListTask.Init((int)TaskDescCode.LoadingGame, 1);
                taskMgr.AddTask(loadPreDllListTask);
            }
#endif

            taskMgr.SetCallbacks(null, hotUpdateComplete, noNeedUpdate);
            taskMgr.RunTask();

            ULogClient.SetHotUpdateVersion(HotUpdateUtils.GetVersion());
        }

        /// <summary>
        /// 先行服分服热更新时再次启动初始化阶段1
        /// </summary>
        /// <param name="sid">服务器id</param>
        public void HotUpdateForSid(int sid, Action hotUpdateComplete)
        {
            taskMgr.Init();

            var loadConfigTask = new LoadConfigTask();
            loadConfigTask.Init((int)TaskDescCode.PrepareConfig, 1);
            taskMgr.AddTask(loadConfigTask);

            var requestConfigTask = new RequestFullConfigTask();
            requestConfigTask.Init((int)TaskDescCode.PrepareConfig, 1);
            taskMgr.AddTask(requestConfigTask);

            var checkUpdateTask = new CheckHotUpdateTask();
            checkUpdateTask.Init((int)TaskDescCode.CheckHotUpdate, 1);
            taskMgr.AddTask(checkUpdateTask);

#if !UNITY_WEBGL
            var hotUpdateTask = new HotUpdateTask();
            hotUpdateTask.Init((int)TaskDescCode.CheckHotUpdate, 1);
            taskMgr.AddTask(hotUpdateTask);
#endif

            taskMgr.SetCallbacks(null, hotUpdateComplete, null);
            taskMgr.RunTask();
        }

        /// <summary>
        /// 先行服分服热更新时再次启动初始化阶段1
        /// </summary>
        /// <param name="sid">服务器id</param>
        /// <param name="needUpdateCallback">需要热更新回调</param>
        /// <param name="noNeedUpdateCallback">不需要热更新回调</param>
        public void CheckUpdateForLogin(int sid, Action needUpdateCallback, Action noNeedUpdateCallback)
        {
            taskMgr.Init();

            var loadConfigTask = new LoadConfigTask();
            loadConfigTask.Init((int)TaskDescCode.PrepareConfig, 1);
            taskMgr.AddTask(loadConfigTask);

            HotUpdateManager.Instance.SetCacheSid(sid);
            var requestConfigTask = new RequestFullConfigTask();
            requestConfigTask.Init((int)TaskDescCode.PrepareConfig, 1);
            taskMgr.AddTask(requestConfigTask);

            var checkUpdateTask = new CheckHotUpdateTask();
            checkUpdateTask.Init((int)TaskDescCode.CheckHotUpdate, 1);
            taskMgr.AddTask(checkUpdateTask);

            taskMgr.SetCallbacks(needUpdateCallback, null, noNeedUpdateCallback);
            taskMgr.RunTask();
        }
        
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            taskMgr?.Update(elapseSeconds, realElapseSeconds);
        }
    }
}