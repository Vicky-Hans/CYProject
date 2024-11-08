using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Data;
using DH.UIFramework;
using DHFramework;
using Game.UI.CommonView;
using UnityEngine;
using UnityEngine.SceneManagement;
using PlayerPrefs = DHUnityUtil.PlayerPrefs;
using SceneManager = DH.UIFramework.SceneManager;

namespace DH.Game
{
    public class ProcedureManager : Singleton<ProcedureManager>
    {
        private struct Transition
        {
            public ProcedureBase procedureBase;
            public ProcedureState procedureState;

            public Transition(ProcedureBase procedureBase, ProcedureState procedureState)
            {
                this.procedureBase = procedureBase;
                this.procedureState = procedureState;
            }
        }

        private readonly Dictionary<int, ProcedureBase> procedureDeepStack = new();
        private ProcedureBase current;
        private readonly List<Transition> procedureFromQueue = new();
        private readonly List<Transition> procedureToQueue = new();
        private bool isLoading = false;
        private readonly List<int> pendingRemove;
        private int deepMax;
        private bool changingProcedure;

        public void Init()
        {
            procedureDeepStack.Clear();
            procedureFromQueue.Clear();
            procedureToQueue.Clear();
            foreach (var item in ProcedureConfig.DeepConfigs)
                if (item.Value > deepMax)
                    deepMax = item.Value;
        }

        protected override void Release()
        {
            for (var index = deepMax; index >= 1; index--)
            {
                if (!procedureDeepStack.TryGetValue(index, out var procedure)) continue;
                procedure.DoExit();
            }

            procedureDeepStack.Clear();
            procedureFromQueue.Clear();
            procedureToQueue.Clear();
            current = null;
        }

        /// <summary>
        /// 层级高->层级低
        /// </summary>
        /// <param name="procedureConfigKey"></param>
        /// <param name="showLoading">是否展示加载条</param>
        /// <param name="unloadAssets">若开启该选项，内部将先加载Empty场景，同时释放资源和GC，然后再加载目标场景
        /// 适用于游戏场景与主界面场景之间的切换</param>
        public async UniTaskVoid Change(string procedureConfigKey, bool showLoading = false,bool unloadAssets = false)
        {
            if (changingProcedure)
            {
                throw new Exception($"Try change to {procedureConfigKey} when changing procedure");
            }
            changingProcedure = true;
            await ChangeAsync(procedureConfigKey, showLoading,unloadAssets);
            changingProcedure = false;
        }

        public async UniTask ChangeAsync<T>(string procedureConfigKey, bool showLoading = false,
            bool unloadAssets = false) where T : BaseLoadingView
        {
            await ChangeAsyncInternal<T>(procedureConfigKey, showLoading, unloadAssets);
        }
        
        public async UniTask ChangeAsync(string procedureConfigKey, bool showLoading = false,
            bool unloadAssets = false)
        {
            await ChangeAsyncInternal<LoadingView>(procedureConfigKey, showLoading, unloadAssets);
        }
        
        /// <summary>
        /// async方式切换Procedure
        /// </summary>
        /// <param name="procedureConfigKey"></param>
        /// <param name="showLoading">是否展示加载条</param>
        /// <param name="unloadAssets">若开启该选项，内部将先加载Empty场景，同时释放资源和GC，然后再加载目标场景
        /// 适用于游戏场景与主界面场景之间的切换</param>
        private async UniTask ChangeAsyncInternal<T>(string procedureConfigKey, bool showLoading = false,
            bool unloadAssets = false) where T : BaseLoadingView
        {
            //已在流程中
            if (current != null && current.ProcedureConfigKey == procedureConfigKey)
            {
                showLoading = true;
                unloadAssets = true;
            }
            
            var targetDeep = ProcedureConfig.GetProcedureDeep(procedureConfigKey);
            ProcedureBase targetProcedure = null;

            //当前有流程
            if (current != null)
            {
                var currentDeep = ProcedureConfig.GetProcedureDeep(current.ProcedureConfigKey);
                // 退栈
                if (currentDeep >= targetDeep)
                {
                    for (var i = currentDeep; i >= targetDeep; --i)
                    {
                        var procedure = procedureDeepStack[i];
                        if (!current.Equals(procedure) && procedure.ProcedureConfigKey == procedureConfigKey)
                        {
                            targetProcedure = procedure;
                            continue;
                        }
                        
                        PushTransitionQueue(procedure, ProcedureState.Exit);
                        procedureDeepStack.Remove(i);
                        DHLog.Debug($"Exit procedure {procedure.ProcedureConfigKey}");
                    }
                }
                else
                {
                    PushTransitionQueue(current, ProcedureState.DeActive);
                }
            }

            if (targetProcedure == null)
            {
                targetProcedure = ProcedureConfig.GetProcedureClass(procedureConfigKey);
                procedureDeepStack.Add(targetDeep, targetProcedure);
                current = targetProcedure;
                PushTransitionQueue(current, ProcedureState.Enter);
            }
            // Procedure重启
            else if (current.Equals(targetProcedure))
            {
                PushTransitionQueue(current, ProcedureState.Enter);
            }
            else
            {
                current = targetProcedure;
                PushTransitionQueue(current, ProcedureState.Active);
            }

            await ProcessTransitionQueue<T>(showLoading,unloadAssets);
            DHLog.Debug($"Enter procedure {current.ProcedureConfigKey}");
        }

        private async UniTask LoadEmptyScene()
        {
            var tcs = AutoResetUniTaskCompletionSource.Create();
            var sceneAddress = "Scenes/Empty";
            if (!string.IsNullOrEmpty(sceneAddress))
            {
                SceneManager.Instance.LoadScene(sceneAddress, LoadSceneMode.Single, delegate(bool b) { tcs.TrySetResult(); }, null);
            }
            else
            {
                tcs.TrySetResult();
            }

            await tcs.Task;
        }
        
        private void UpdateLoadViewState(BaseView curLoadView,TransitionState state)
        {
            if(curLoadView == null) return;

            var tempView = curLoadView as BaseLoadingView;
            if(tempView == null) return;
            tempView.UpdateState(state);
        }

        private async UniTask DoProcessTransition<T>(bool showLoading,bool unloadAssets = false) where T : BaseLoadingView
        {
            isLoading = true;
            BaseView loadingView = null;
            if (showLoading)
            {
                loadingView = await UIManager.Instance.OpenDialog<T>();
            }
            
            UpdateLoadViewState(loadingView, TransitionState.UnLoad);

            
            for (var i = 0; i < procedureFromQueue.Count; ++i)
            {
                var procedure = procedureFromQueue[i].procedureBase;
                var state = procedureFromQueue[i].procedureState;
                await procedure.ChangeState(state);
            }

            // 使用Empty空场景作为两个场景切换的中间件，方便GC和资源释放
            if (unloadAssets)
            {
                await LoadEmptyScene();
                Resources.UnloadUnusedAssets();
                GC.Collect();
                AudioManager.Instance.ReleaseUnusedClip();
            }
            
            for (var i = 0; i < procedureToQueue.Count; ++i)
            {
                var procedure = procedureToQueue[i].procedureBase;
                var state = procedureToQueue[i].procedureState;
                await procedure.ChangeState(state, transitionState =>
                {
                    UpdateLoadViewState(loadingView, transitionState);
                });
            }

            if (showLoading)
            {
                UIManager.Instance.CloseDialog<T>();
            }

            procedureFromQueue.Clear();
            procedureToQueue.Clear();
            isLoading = false;
        }

        private async UniTask ProcessTransitionQueue<T>(bool showLoading,bool unloadAssets = false) where T : BaseLoadingView
        {
            await DoProcessTransition<T>(showLoading,unloadAssets);
        }

        public void PushTransitionQueue(ProcedureBase procedure, ProcedureState state)
        {
            if (state is ProcedureState.Enter or ProcedureState.Active)
                procedureToQueue.Add(new Transition(procedure, state));
            else
                procedureFromQueue.Add(new Transition(procedure, state));
        }

        public bool IsCurrent(string procedureConfigKey)
        {
            return current != null && current.ProcedureConfigKey == procedureConfigKey;
        }

        public bool IsInLoading()
        {
            return isLoading;
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (IsInLoading()) return;

            for (var index = 1; index <= deepMax; index++)
            {
                if (!procedureDeepStack.TryGetValue(index, out var procedure)) continue;

                procedure.Update(elapseSeconds, realElapseSeconds);
            }
        }
        //获取当前ProcedureKey
        public string GetCurrentProcedureKey()
        {
            string curKey = "";
            if (current != null) curKey = current.ProcedureConfigKey;
            return curKey;
        }
    }
}