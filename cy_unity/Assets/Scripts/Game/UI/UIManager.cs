using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Game;
using DH.Game.UI;
using DH.UIFramework;
using DH.UIFramework.ViewModels;
using DHFramework;
using Game.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace DH.Game
{
    public partial class UIManager
    {
        private static readonly bool ReleaseWhenClose = true;
        private static readonly bool ReleasePrefabWhenClose = true;
        private static UIManager instance;
        private Dictionary<UILayersConfig, Transform> uiLayerRootDic;
        private Dictionary<UILayersConfig, List<ViewItem>> uiStackDic;
        private EventSystem eventSystem;
        private readonly HashSet<string> inputFlag = new HashSet<string>();
        private readonly List<ViewItem> pendingCloseViews = new();
        private readonly List<ViewItem> pendingOpenViews = new();
        private readonly List<ViewItem> gotoOpenViews = new();
        private readonly List<ViewItem> gotoCloseViews = new();
        private bool lockCreateView;
        private bool doingExit = false;

        public static UIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UIManager
                    {
                        eventSystem = Object.FindObjectOfType<EventSystem>()
                    };
                }

                return instance;
            }
        }

        #region Public Function

        public void Init()
        {
            uiLayerRootDic = new Dictionary<UILayersConfig, Transform>();
            uiStackDic = new Dictionary<UILayersConfig, List<ViewItem>>();
            for (UILayersConfig i = UILayersConfig.Min + 1; i < UILayersConfig.Max; ++i)
            {
                uiStackDic[i] = new List<ViewItem>();
                uiLayerRootDic[i] = AppGlobal.Instance.UIRoots[(int)i - 1];
            }
        }

        public void Release()
        {
            foreach (var uiStack in uiStackDic)
            {
                foreach (var item in uiStack.Value)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    if (item.view)
                    {
                        item.view.Release();
                        if (item.autoDispose)
                        {
                            (item.view.GetDataContext() as ViewModelBase)?.Dispose();
                        }

                        Object.Destroy(item.view.gameObject);
                    }

                    AssetsManager.Release(item.prefabCache);
                }
            }

            inputFlag.Clear();
            uiStackDic.Clear();
            uiStackDic = null;
            uiLayerRootDic.Clear();
            uiLayerRootDic = null;
            if (eventSystem)
            {
                eventSystem.enabled = true;
            }
        }

        public void SetInputState(string tag, bool flag)
        {
            if (!flag)
            {
                inputFlag.Add(tag);
            }
            else
            {
                inputFlag.Remove(tag);
            }

            if (eventSystem)
            {
                bool inputState = eventSystem.enabled;
                bool newState = inputFlag.Count == 0;
                if (inputState == newState)
                {
                    return;
                }

                eventSystem.enabled = newState;
                if (newState)
                {
                    eventSystem.SetSelectedGameObject(null);
                }
            }
        }

        public bool CheckViewIsTopMost(BaseView view, UILayersConfig layersConfig = UILayersConfig.UI)
        {
            if (!uiStackDic.TryGetValue(layersConfig, out var views) || views.Count == 0)
            {
                return false;
            }

            return views[^1].view == view;
        }

        public async UniTask<T> OpenDialog<T, TSource>()
            where T : BaseView where TSource : new()
        {
            var viewItem = await OpenDialogInternal<T, TSource>();
            var view = viewItem.view;
            if (!view)
            {
                return null;
            }

            return view as T;
        }

        public async UniTask<T> OpenDialog<T>(object source, bool autoDispose = true)
            where T : BaseView
        {
            var viewItem = await OpenDialogInternal<T>(source, autoDispose);
            var view = viewItem.view;
            if (!view)
            {
                return null;
            }
            
            return view as T;
        }

        public async UniTask<T> OpenDialog<T>() where T : BaseView
        {
            var item = await OpenDialogInternal<T>(null, false);
            return item.view as T;
        }

        public void CloseDialog<T>() where T : BaseView
        {
            var baseNode = GetDialogInternal<T>();
            if (baseNode == null)
            {
                return;
            }

            CloseDialogVoid(baseNode).Forget();
        }

        public void CloseDialog(Type uiConfigKey)
        {
            ViewItem baseNode = null;
            var configItem = UIConfig.GetUIConfigItem(uiConfigKey);
            if (configItem == null) return;
            var uiStack = GetUIStack(configItem.layer);
            if (uiStack == null) return;
            for (int i = uiStack.Count - 1; i >= 0; --i)
            {
                if (uiStack[i].viewType == uiConfigKey)
                    baseNode = uiStack[i];
            }

            if (baseNode == null)
            {
                return;
            }

            CloseDialogVoid(baseNode).Forget();
        }

        public async UniTask CloseDialogWithAnim<T>() where T : BaseView
        {
            var baseNode = GetDialogInternal<T>();
            if (baseNode == null)
            {
                return;
            }

            if (baseNode.view) ;
            {
                await baseNode.view.PlayCloseAnimation();
            }
            await CloseDialogAsync(baseNode);
        }
        
        public bool CheckIsTopDlg<T>(UILayersConfig uiLayer = UILayersConfig.UI ) where T : BaseView
        {
            return CheckIsTopDlg(typeof(T), uiLayer);
        }

        public Transform GetUILayerRoot(UILayersConfig uiLayersConfig)
        {
            Transform root;
            if (!uiLayerRootDic.TryGetValue(uiLayersConfig, out root))
            {
                DHLog.Error("Try to get a error layer , layerconfig is : " + uiLayersConfig);
            }

            return root;
        }

        public async UniTaskVoid CloseTopDialog(UILayersConfig uiLayersConfig = UILayersConfig.UI,
            bool withExitAnim = false)
        {
            doingExit = true;
            try
            {
                var topDlg = GetTopDialog(uiLayersConfig);
                if (withExitAnim && topDlg?.view)
                {
                    await topDlg.view.PlayCloseAnimation();
                }

                if (topDlg != null)
                {
                    await CloseDialogAsync(topDlg);
                }
            }
            finally
            {
                doingExit = false;
            }
        }

        public void CloseAllTopDialog(bool keepRootDlg = true)
        {
            CloseAllTopUIWithGoto(keepRootDlg).Forget();
        }

        public async UniTask CloseAllTopDialogAsync(bool keepRootDlg = true)
        {
            await SafeGotoMenu(null,keepRootDlg:keepRootDlg);
        }

        /// <summary>
        /// 直接跳转到指定界面
        /// 跳转界面的逻辑由用户单独实现
        /// </summary>
        /// <param name="func"></param>
        /// <param name="closeAllDialog"></param>
        /// <param name="keepRootDlg"></param>
        public async UniTask SafeGotoMenu(Func<UniTask> func, bool closeAllDialog = true, bool keepRootDlg = true)
        {
            using var safeGotoScope = new SafeGotoScope();
            var uiStack = GetUIStack(UILayersConfig.UI);
            if (uiStack == null) return;
            int bottom = keepRootDlg ? 1 : 0;
            gotoOpenViews.Clear();
            gotoCloseViews.Clear();

            if (closeAllDialog)
            {
                for (int i = uiStack.Count - 1; i >= bottom; --i)
                {
                    var item = uiStack[i];
                    CloseDialogWhenGotoDirectly(item);
                    gotoCloseViews.Add(item);
                }
            }

            // 1. 执行跳转逻辑，不执行View的修改
            // 2. 执行需要打开的View显示
            // 3. 执行需要关闭的View隐藏或者销毁
            if (func != null)
            {
                using (new GotoScope())
                {
                    await func();
                }
            }

            {
                for (int i = uiStack.Count - 1; i >= 0; --i)
                {
                    var item = uiStack[i];
                    var config = UIConfig.GetUIConfigItem(item.viewType);
                    gotoOpenViews.Insert(0, item);
                    if (config.isFullScreen)
                    {
                        break;
                    }
                }

                await RefreshPendingViewState(UILayersConfig.UI,gotoOpenViews,gotoCloseViews,closeItem:null,releaseData:true);
            }
        }

        public T GetDialog<T>() where T : BaseView
        {
            return GetDialogInternal<T>()?.view as T;
        }

        public bool IsOpen<T>() where T : BaseView
        {
            return IsOpen(typeof(T));
        }

        public void OnUpdate()
        {
            if (doingExit || !eventSystem.enabled)
            {
                return;
            }

            // if (MainMenuTutorialManager.Instance.DoingTutorial)
            // {
            //     return;
            // }

            if (!Input.GetKeyDown(KeyCode.Escape))
            {
                return;
            }

            if (!ActivityManager.Instance.ResponseEscapeBtn())
            {
                return;
            }

            CloseTopDialog(true).Forget();
        }

        #endregion

        #region Private Function

        private async UniTaskVoid CloseAllTopUIWithGoto(bool keepRootDlg)
        {
            await SafeGotoMenu(null,keepRootDlg:keepRootDlg);
        }

        private async UniTask<ViewItem> OpenDialogInternal<T, TSource>() where T : BaseView where TSource : new()
        {
            try
            {
                SetInputState("OpenDialogInternal", false);
                var uiConfigKey = typeof(T);
                var configItem = UIConfig.GetUIConfigItem(uiConfigKey);
                if (configItem == null) return null;
                var uiLayerRoot = GetUILayerRoot(configItem.layer);
                if (uiLayerRoot == null) return null;
                var uiStack = GetUIStack(configItem.layer);
                if (uiStack == null) return null;
                if (IsOpen(uiConfigKey) && !configItem.isMulti)
                {
                    var viewItem = TakeUI2Top<T>();
                    var source = viewItem.view.GetDataContext();
                    if (source is not TSource)
                    {
                        throw new InvalidOperationException();
                    }
                    if (!lockCreateView)
                    {
                        await RefreshUIStackStateInternal(configItem.layer, null);
                        RefreshViewLayerOrder(configItem.layer);
                    }
                    return viewItem;
                }
                else
                {
                    var prefab = await AssetsManager.LoadAssetAsync<GameObject>(configItem.path);
                    var viewItem = new ViewItem()
                    {
                        prefabCache = prefab,
                        viewType = uiConfigKey,
                        uiLayerRoot = uiLayerRoot,
                        autoDispose = true,
                        dataContext = new TSource(),
                    };
                    PushUI2Stack(viewItem);
                    if (!lockCreateView)
                    {
                        await RefreshUIStackStateInternal(configItem.layer, null);
                        RefreshViewLayerOrder(configItem.layer);
                    }
                    return viewItem;
                }
            }
            finally
            {
                SetInputState("OpenDialogInternal", true);
            }
        }

        private async UniTask<ViewItem> OpenDialogInternal<T>(object dataContext, bool autoDispose) where T : BaseView
        {
            try
            {
                SetInputState("OpenDialogInternal", false);
                var uiConfigKey = typeof(T);
                var configItem = UIConfig.GetUIConfigItem(uiConfigKey);
                if (configItem == null) return null;
                var uiLayerRoot = GetUILayerRoot(configItem.layer);
                if (uiLayerRoot == null) return null;
                var uiStack = GetUIStack(configItem.layer);
                if (uiStack == null) return null;

                if (IsOpen(uiConfigKey) && !configItem.isMulti)
                {
                    var viewItem = TakeUI2Top<T>();
                    if (!lockCreateView)
                    {
                        await RefreshUIStackStateInternal(configItem.layer, null);
                        RefreshViewLayerOrder(configItem.layer);
                    }
                    return viewItem;
                }
                else
                {
                    var prefab = await AssetsManager.LoadAssetAsync<GameObject>(configItem.path);
                    var viewItem = new ViewItem()
                    {
                        prefabCache = prefab,
                        viewType = uiConfigKey,
                        uiLayerRoot = uiLayerRoot,
                        autoDispose = autoDispose,
                        dataContext = dataContext,
                    };
                    PushUI2Stack(viewItem);
                    if (!lockCreateView)
                    {
                        await RefreshUIStackStateInternal(configItem.layer, null);
                        RefreshViewLayerOrder(configItem.layer);
                    }
                    return viewItem;
                }
            }
            finally
            {
                SetInputState("OpenDialogInternal", true);
            }
        }

        protected ViewItem GetDialogInternal<T>() where T : BaseView
        {
            var uiConfigKey = typeof(T);
            var configItem = UIConfig.GetUIConfigItem(uiConfigKey);
            if (configItem == null) return null;
            var uiStack = GetUIStack(configItem.layer);
            if (uiStack == null) return null;
            for (int i = uiStack.Count - 1; i >= 0; --i)
            {
                if (uiStack[i].viewType == uiConfigKey)
                    return uiStack[i];
            }

            return null;
        }

        private async UniTaskVoid CloseTopDialog(bool withExitAnim = false)
        {
            doingExit = true;
            try
            {
                for (int index = (int)UILayersConfig.Max - 1; index > (int)UILayersConfig.Min; index--)
                {
                    var layer = (UILayersConfig)index;
                    var topDlg = GetTopDialog(layer);
                    if (topDlg == null)
                    {
                        continue;
                    }

                    // 自定义退出行为
                    if (topDlg.view && topDlg.view.OnPhysicExit())
                    {
                        break;
                    }

                    if (withExitAnim && topDlg.view)
                    {
                        await topDlg.view.PlayCloseAnimation();
                    }

                    await CloseDialogAsync(topDlg);
                    break;
                }
            }
            finally
            {
                doingExit = false;
            }
        }

        private bool IsOpen(Type uiConfigKey)
        {
            var configItem = UIConfig.GetUIConfigItem(uiConfigKey);
            if (configItem == null) return false;
            var uiStack = GetUIStack(configItem.layer);
            if (uiStack == null) return false;

            for (int i = 0; i < uiStack.Count; ++i)
            {
                if (uiStack[i].viewType == uiConfigKey)
                    return true;
            }

            return false;
        }

        private List<ViewItem> GetUIStack(UILayersConfig uiLayersConfig)
        {
            if (!uiStackDic.TryGetValue(uiLayersConfig, out var uiStack))
            {
                DHLog.Error("Try to get a error uistack , layerconfig is : " + uiLayersConfig);
            }

            return uiStack;
        }

        private void RefreshViewLayerOrder(UILayersConfig uiLayersConfig)
        {
            var uiStack = GetUIStack(uiLayersConfig);
            if (uiStack == null) return;
            for (int i = 0; i < uiStack.Count; ++i)
            {
                int orderInLayer = uiLayersConfig == UILayersConfig.Scene
                    ? (int)uiLayersConfig * 400
                    : (int)uiLayersConfig * 400 + i * 20;
                var view = uiStack[i].view;
                if (!view)
                {
                    continue;
                }

                uiStack[i].view.ApplySortingOrder(orderInLayer);
            }
        }

        private async UniTask RefreshUIStackStateInternal(UILayersConfig uiLayer, ViewItem closeItem)
        {
            SetInputState(nameof(RefreshUIStackStateInternal),false);
            var uiStack = GetUIStack(uiLayer);
            if (uiStack == null) return;
            bool findFullScreen = false;
            bool topUI = false;

            pendingCloseViews.Clear();
            pendingOpenViews.Clear();
            if (closeItem != null)
            {
                pendingCloseViews.Add(closeItem);
            }
            
            for (int i = uiStack.Count - 1; i >= 0; --i)
            {
                topUI = i == uiStack.Count - 1;
                
                var item = uiStack[i];
                var uiConfigKey = item.viewType;
                var configItem = UIConfig.GetUIConfigItem(uiConfigKey);
                if (!findFullScreen)
                {
                    findFullScreen = configItem.isFullScreen;
                    pendingOpenViews.Insert(0,item);
                    if (topUI)
                    {
                        if (item.view != null)
                        {
                            item.view.OnTopUIShow();
                        }
                    }
                }
                else if(item.uiState || item.view)
                {
                    pendingCloseViews.Add(item);
                }
            }

            await RefreshPendingViewState(uiLayer,pendingOpenViews,pendingCloseViews,closeItem);
            SetInputState(nameof(RefreshUIStackStateInternal),true);
        }

        private async UniTask RefreshPendingViewState(UILayersConfig uiLayer, List<ViewItem> open,List<ViewItem> close,ViewItem closeItem,bool releaseData = false)
        {
            //  Collection was modified; enumeration operation may not execute
            List<ViewItem> tempOp = new();
            foreach (var item in open)
            {
                tempOp.Add(item);
            }
            foreach (var view in tempOp)
            {
                await view.PrepareView();
            }
            
            foreach (var view in close)
            {
                view.DeActive();
            }

            if (open.Count > 0)
            {
                var last = open[^1];
                foreach (var view in open)
                {
                    view.Active(view == last);
                }
            }

            if (closeItem != null)
            {
                ReleaseViewData(closeItem);
            }
            
            foreach (var view in close)
            {
                if (releaseData)
                {
                    ReleaseViewData(view);
                }
                view.DeActiveView();
            }
            RefreshViewLayerOrder(uiLayer);
            if (open.Count > 0)
            {
                var last = open[^1];
                await last.view.PlayOpenAnimation();
            }
        }

        private ViewItem TakeUI2Top<T>() where T : BaseView
        {
            var type = typeof(T);
            var configItem = UIConfig.GetUIConfigItem(type);
            if (configItem == null) return null;
            var uiStack = GetUIStack(configItem.layer);
            if (uiStack == null) return null;
            for (int i = uiStack.Count - 1; i >= 0; --i)
            {
                if (uiStack[i].viewType != type)
                {
                    continue;
                }

                var comp = uiStack[i];
                uiStack.RemoveAt(i);
                uiStack.Add(comp);
                return comp;
            }

            return null;
        }

        private void PushUI2Stack(ViewItem viewItem)
        {
            var configItem = UIConfig.GetUIConfigItem(viewItem.viewType);
            if (configItem == null) return;
            var uiStack = GetUIStack(configItem.layer);
            if (uiStack == null) return;
            uiStack.Add(viewItem);
        }

        private UIConfigItem PopUIFromStack(ViewItem viewItem)
        {
            var configItem = UIConfig.GetUIConfigItem(viewItem.viewType);
            if (configItem == null) return null;
            var uiStack = GetUIStack(configItem.layer);
            if (uiStack == null) return null;
            uiStack.Remove(viewItem);
            return configItem;
        }

        private ViewItem GetTopDialog(UILayersConfig uiLayersConfig = UILayersConfig.UI)
        {
            var uiStack = GetUIStack(uiLayersConfig);
            if (uiStack == null) return null;
            return uiStack.Count > 0 ? uiStack[^1] : null;
        }

        private void CloseDialogWhenGotoDirectly(ViewItem baseNode)
        {
            var configItem = UIConfig.GetUIConfigItem(baseNode.viewType);
            if (configItem == null) return;
            var uiStack = GetUIStack(configItem.layer);
            if (uiStack == null) return;
            uiStack.Remove(baseNode);
            // 此处忽略界面激活或者隐藏逻辑，有外部统一处理
        }

        private async UniTaskVoid CloseDialogVoid(ViewItem baseNode)
        {
            await CloseDialogAsync(baseNode);
        }

        private async UniTask CloseDialogAsync(ViewItem baseNode)
        {
            var configItem = PopUIFromStack(baseNode);
            await RefreshUIStackStateInternal(configItem.layer, baseNode);
        }

        private void ReleaseViewData(ViewItem baseNode)
        {
            if (baseNode.view)
            {
                baseNode.view.Release();
                if (baseNode.autoDispose)
                {
                    var dataContext = baseNode.view.GetDataContext();
                    baseNode.view.SetDataContext(null);
                    (dataContext as ViewModelBase)?.Dispose();
                }

                AssetsManager.Release(baseNode.prefabCache);
                Object.Destroy(baseNode.view.gameObject);
                baseNode.view = null;
                baseNode.prefabCache = null;
            }
        }
        
        /// <summary>
        /// 检查是界面堆栈的栈顶是否是某个界面
        /// </summary>
        /// <param name="uiConfigKey"></param>
        /// <param name="uiLayersConfig"></param>
        /// <returns></returns>
        private bool CheckIsTopDlg(Type uiConfigKey,UILayersConfig uiLayersConfig)
        {
            var configItem = UIConfig.GetUIConfigItem(uiConfigKey);
            if (configItem == null) return false;
            var uiStack = GetUIStack(configItem.layer);
            if (uiStack == null) return false;
        
            var topDlg = GetTopDialog(uiLayersConfig);
            if (topDlg == null) return false;
            return topDlg.viewType == uiConfigKey;
        }

        #endregion
    }
}