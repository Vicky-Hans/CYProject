using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.UIFramework.Builder;
using DH.UIFramework.Contexts;
using DH.UIFramework.ViewModels;
using DHFramework.Localization;
using UnityEngine;

namespace DH.UIFramework
{
    public partial class BaseView : MonoBehaviour
    {
        public static event Action<int, Func<UniTask>> LocalizeRegister; 
        public static event Action<int> LocalizeUnregister; 

        private bool initialized;
        private BindingContextLifecycle bindingContextLifecycle;
        private Canvas canvas;
        private SortingOrderGroup group;

        private bool autoDispose;
        private GameObject prefabCache;
        private bool viewActive;

        public virtual bool FullScreen => false;

        private void Awake()
        {
            var c = GetComponent<Canvas>();
            if (c)
                viewActive = c.enabled;
        }

        public bool Active
        {
            set
            {
                if (viewActive == value)
                {
                    return;
                }

                viewActive = value;
                var canvasLocal = Canvas;
                if (canvasLocal.enabled != value)
                {
                    canvasLocal.enabled = value;
                }

                if (value)
                {
                    OnFocus();
                }
                else
                {
                    OnDismiss();
                }
            }

            get => viewActive;
        }

        public BindingContextLifecycle BindingContext
        {
            get
            {
                if (bindingContextLifecycle) return bindingContextLifecycle;

                bindingContextLifecycle = GetComponent<BindingContextLifecycle>();
                if (!bindingContextLifecycle)
                {
                    this.BindingContext();
                    bindingContextLifecycle = GetComponent<BindingContextLifecycle>();
                }

                return bindingContextLifecycle;
            }
        }

        public Canvas Canvas
        {
            get
            {
                if (!canvas) canvas = GetComponent<Canvas>();

                return canvas;
            }
        }

        public SortingOrderGroup Group
        {
            get
            {
                if (!group) group = GetComponent<SortingOrderGroup>();

                return group;
            }
        }

        protected virtual async UniTask OnLocalize()
        {
            (BindingContext.BindingContext as BindingContext)?.OnLocalize();
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// 仅用于数据绑定优化代码生成逻辑
        /// </summary>
        public BindingSetBase bindingSetBase;
        /// <summary>
        /// 仅用于数据绑定优化代码生成逻辑
        /// </summary>
        public virtual void InitializeBinding()
        {
        }

        public virtual async UniTask Create()
        {
            if (initialized) throw new Exception("Already initialized");

            initialized = true;
            if (LocalizeRegister == null)
            {
                Localization.RegisterLocalize(GetInstanceID(), OnLocalize);
            }
            else
            {
                LocalizeRegister?.Invoke(GetInstanceID(), OnLocalize);
            }

            await UniTask.CompletedTask;
        }

        public virtual void ApplySortingOrder(int order)
        {
            Canvas.overrideSorting = true;
            Canvas.sortingOrder = order;
            if (!Group) return;
            Group.ApplySortingOrderModifier(order);
        }

        /// <summary>
        /// 物理返回键自定义响应
        /// 如主界面对于物理返回键应该响应弹出退出游戏界面而不是退出主界面
        /// </summary>
        /// <returns>false表示无自定义行为，true表示存在自定义关闭行为，上层不要进行统一处理</returns>
        public virtual bool OnPhysicExit()
        {
            return false;
        }

        /// <summary>
        /// 直接跳转时，只执行Dismiss逻辑代码，不执行View隐藏
        /// 防止还未进入新界面时发生界面闪烁
        /// </summary>
        public void DismissWithoutViewChange()
        {
            viewActive = false;
            OnDismiss();
        }

        public virtual void Release()
        {
            bindingContextLifecycle = GetComponent<BindingContextLifecycle>();
            if (bindingContextLifecycle)
            {
                var ctx = bindingContextLifecycle.BindingContext;
                ctx.Unbind();
                ctx.Clear();
            }
            RemoveAllChildList();
            ReleaseSpriteConverter();
            ReleaseMaterialCache();
            if (LocalizeUnregister == null)
            {
                Localization.UnRegisterLocalize(GetInstanceID());
            }
            else
            {
                LocalizeUnregister?.Invoke(GetInstanceID());
            }
        }

        #region state changed

        public void OnTopUIShow()
        {
            OnShow();
        }

        protected virtual void OnFocus()
        {
            
        }

        protected virtual void OnDismiss()
        {
            
        }
    
        protected virtual void OnShow()
        {
            
        }

        #endregion

        #region Child ui

        private List<BaseView> childBaseViewList = new();

        protected async UniTask<T> CreateChildView<T, TKSource>(string path, Transform parent, TKSource source)
            where T : BaseView where TKSource : new()
        {
            var prefab = await AssetsManager.LoadAssetAsync<GameObject>(path);
            var newInstance = Instantiate(prefab, parent, false);
            var comp = newInstance.GetComponent<T>();
            comp.prefabCache = prefab;

            childBaseViewList.Add(comp);

            if (source == null)
            {
                source = new TKSource();
                comp.autoDispose = true;
            }

            comp.SetDataContext(source);

            return comp;
        }

        protected void RemoveChildView(BaseView childNode)
        {
            if (childBaseViewList.Remove(childNode))
            {
                childNode.Release();

                if (childNode.autoDispose) (childNode.GetDataContext() as ViewModelBase)?.Dispose();

                var childNodePrefabCache = childNode.prefabCache;
                ((RectTransform)(childNode.transform)).SetParent(null);
                Destroy(childNode.gameObject);
                AssetsManager.Release(childNodePrefabCache);
            }
        }

        protected void RemoveAllChildList()
        {
            if (childBaseViewList.Count > 0)
                for (var i = childBaseViewList.Count - 1; i >= 0; --i)
                    RemoveChildView(childBaseViewList[i]);
        }

        #endregion
    }
}