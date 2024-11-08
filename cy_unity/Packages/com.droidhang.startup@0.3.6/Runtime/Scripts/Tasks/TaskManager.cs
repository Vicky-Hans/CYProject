using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.NativeCore;
using DHFramework;
using UnityEngine;

#if !UNITY_WEBGL
using DHFramework.Download;
#endif

namespace DH.Launch
{
    public partial class TaskManager
    {
        private Queue<TaskBase> tasks = new Queue<TaskBase>();
        private TaskBase curTask;
        private object userData;
        private float totalWeight = 0; //用于登录界面显示进度条
        private float completeWeight = 0; //用于登录界面显示进度条
        private bool init = false;

        private Action needUpdateCallback;
        private Action noNeedUpdateCallback;
        private Action hotUpdateCompleteCallback;
        
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            if (init)
            {
                Reset();
                return;
            }

            init = true;
            Reset();
        }

        /// <summary>
        /// 把一个任务加到执行列表里
        /// </summary>
        /// <param name="task"></param>
        public void AddTask(TaskBase task)
        {
            tasks.Enqueue(task);

            if (task.Weight > 0)
            {
                totalWeight += task.Weight;
            }
        }

        /// <summary>
        /// 开始执行任务
        /// </summary>
        public void RunTask()
        {
            StartNextTask();
        }

        public void SetCallbacks(Action needUpdate, Action updateComplete, Action noNeedUpdate)
        {
            this.needUpdateCallback = needUpdate;
            this.noNeedUpdateCallback = noNeedUpdate;
            this.hotUpdateCompleteCallback = updateComplete;
        }

        public void TipRestartGame()
        {
            OpenMessageBox(LanguageId.DialogTitle, LanguageId.RestartGameContent, 
                yesCallback:Application.Quit, 
                yesBtnText:LanguageId.Confirm);
        }
        
        #region 启动UI

#if !UNITY_WEBGL
        /// <summary>
        /// 下载过程中的回调
        /// </summary>
        /// <param name="info"></param>
        public void OnDownloadProgress(DetailInfo info)
        {
            string baseTextKey = "";
            switch (info.state)
            {
                case 1:
                    baseTextKey = LanguageId.DownloadState1;
                    break;
                case 2:
                    baseTextKey = LanguageId.DownloadState2;
                    break;
                case 3:
                    baseTextKey = LanguageId.DownloadState3;
                    break;
                case 4:
                    baseTextKey = LanguageId.DownloadState4;
                    break;
                case 5:
                    baseTextKey = LanguageId.DownloadState5;
                    break;
                case 6:
                    baseTextKey = LanguageId.DownloadState6;
                    break;
                case 7:
                    baseTextKey = LanguageId.DownloadState7;
                    break;
                case 8:
                    baseTextKey = LanguageId.DownloadState8;
                    break;
                case 9:
                    baseTextKey = LanguageId.DownloadState9;
                    break;
                case 10:
                    baseTextKey = LanguageId.DownloadState10;
                    break;
                case 11:
                    baseTextKey = LanguageId.DownloadState11;
                    break;
                case 12:
                    baseTextKey = LanguageId.DownloadState12;
                    break;
                case 13:
                    baseTextKey = LanguageId.DownloadState13;
                    break;
                case 14:
                    baseTextKey = LanguageId.DownloadState14;
                    break;
                case 15:
                    baseTextKey = LanguageId.DownloadState15;
                    break;
                case 16:
                    baseTextKey = LanguageId.DownloadState16;
                    break;
                case 17:
                    baseTextKey = LanguageId.DownloadState17;
                    break;
                case 18:
                    baseTextKey = LanguageId.DownloadState18;
                    break;
            }

            bool showSpeed = info.state == 5 || info.state == 11 || info.state == 14 || info.state == 12;
            var curSizeText = UDownload.GetReadableSize(info.currentLength);
            var maxSizeText = UDownload.GetReadableSize(info.totalLength);
            var speedText = UDownload.GetReadableSpeed(info.speed);
            
            RefreshProgress(info.progress);

            var baseText = StartupEntry.Instance.LanguageConfig.GetDataById(baseTextKey);
            if (showSpeed)
            {
                RefreshText(DHUtility.Format("{0}[{1}/{2}] {3}", baseText, curSizeText, maxSizeText, speedText));                
            }
            else
            {
                RefreshText(baseText);
            }
        }
#endif


        /// <summary>
        /// 刷新登录界面进度条
        /// </summary>
        /// <param name="progress"></param>
        private void RefreshProgress(float progress)
        {
            if (StartupMainDlg.Instance)
            {
                StartupMainDlg.Instance.RefreshProgress(progress);
            }
        }

        /// <summary>
        /// 刷新登录界面文本显示
        /// </summary>
        /// <param name="progress"></param>
        private void RefreshText(string descText)
        {
            if (StartupMainDlg.Instance)
            {
                StartupMainDlg.Instance.RefreshText(descText);
            }
        }

        /// <summary>
        /// 刷新登录界面的状态显示
        /// </summary>
        /// <param name="progress"></param>
        private void RefreshStatus(int state)
        {
            string textKey = "";
            switch ((TaskAssembler.TaskDescCode)state)
            {
                case TaskAssembler.TaskDescCode.CheckNetwork:
                    textKey = LanguageId.CheckNetwork;
                    break;
                case TaskAssembler.TaskDescCode.PrepareConfig:
                    textKey = LanguageId.LoadingConfig;
                    break;
                case TaskAssembler.TaskDescCode.InitSdk:
                    textKey = LanguageId.InitSdk;
                    break;
                case TaskAssembler.TaskDescCode.CheckHotUpdate:
                    textKey = LanguageId.CheckHotUpdate;
                    break;
                case TaskAssembler.TaskDescCode.LoadingGame:
                    textKey = LanguageId.LoadingConfig;
                    break;
            }

            if (!string.IsNullOrEmpty(textKey))
            {
                RefreshText(StartupEntry.Instance.LanguageConfig.GetDataById(textKey));
            }
        }
        
        #endregion
        
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (!init || curTask == null)
            {
                return;
            }

            if (!curTask.IsDone)
            {
                curTask.UpdateTask(elapseSeconds, realElapseSeconds);
                return;
            }

            OnOneTaskEnd(curTask);
            var result = curTask.GetResult();
            CheckCurTaskResult(result);

            if (tasks.Count == 0)
            {
                OnAllTaskEnd(result);
                return;
            }
            
            StartNextTask();
        }

        public void Shutdown()
        {
            Reset();
        }

        /// <summary>
        /// 当一个任务开始的事件
        /// </summary>
        /// <param name="task"></param>
        private void OnOneTaskStart(TaskBase task)
        {
            if (task != null)
            {
                RefreshStatus(task.State);
            }
        }

        /// <summary>
        /// 当一个任务结束的事件
        /// </summary>
        /// <param name="task"></param>
        private void OnOneTaskEnd(TaskBase task)
        {
            if (task != null && task.Weight > 0)
            {
                completeWeight += task.Weight;
                RefreshProgress(completeWeight / totalWeight);
            }
        }

        /// <summary>
        /// 当所有任务结束的事件
        /// </summary>
        /// <param name="result"></param>
        private void OnAllTaskEnd(object result)
        {
            curTask = null;

            if (result != null && result is HotUpdateResult updateResult)
            {
                if (updateResult.UpdateComplete)
                {
                    hotUpdateCompleteCallback?.Invoke();
                }
                else
                {
                    if (updateResult.NeedUpdate)
                    {
                        needUpdateCallback?.Invoke();
                    }
                    else
                    {
                        noNeedUpdateCallback?.Invoke();
                    }
                }
            }
            
            needUpdateCallback = null;
            noNeedUpdateCallback = null;
            hotUpdateCompleteCallback = null;
        }

        private void StartNextTask()
        {
            curTask = tasks.Dequeue();
            curTask.Start(this, userData);
            userData = null;
            
            OnOneTaskStart(curTask);
        }

        private void CheckCurTaskResult(object result)
        {
            userData = result;
            if (userData is HotUpdateResult checkResult)
            {
#if !UNITY_WEBGL
                if (tasks.Count != 0)
                {
                    return;
                }

                if (checkResult.NeedUpdate && checkResult.AddApkDownloadTask)
                {
                    //添加apk download的task
                    var apkDownloadTask = new ApkDownloadTask();
                    apkDownloadTask.Init((int)TaskAssembler.TaskDescCode.CheckHotUpdate, 1);
                    tasks.Enqueue(apkDownloadTask);
                }
#endif
            }
        }
        
        private void Reset()
        {
            tasks.Clear();
            curTask = null;
            userData = null;
            totalWeight = 0;
            completeWeight = 0;
            needUpdateCallback = null;
            noNeedUpdateCallback = null;
            hotUpdateCompleteCallback = null;
        }
    }
}
