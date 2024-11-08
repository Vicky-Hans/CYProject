using System;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Game.UI;
using DH.UIFramework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DH.Game
{
    public partial class UIManager
    {
        public class ViewItem
        {
            public bool uiState;
            public BaseView view;
            public Type viewType;
            public GameObject prefabCache;
            public object dataContext;
            public bool autoDispose;
            public Transform uiLayerRoot;

            public async UniTask PrepareView()
            {
                if (instance.lockCreateView)
                {
                    return;
                }
                
                if (view)
                {
                    view.SetDataContext(dataContext);
                    return;
                }

                if (!prefabCache)
                {
                    var config = UIConfig.GetUIConfigItem(viewType);
                    prefabCache = await AssetsManager.LoadAssetAsync<GameObject>(config.path);
                }
                
                var newInstance = Object.Instantiate(prefabCache, uiLayerRoot, false);
                view = newInstance.GetComponent<BaseView>();
                view.SetDataContext(dataContext);
                view.Canvas.enabled = false;
                await view.Create();
            }

            public void DeActiveView()
            {
                uiState = false;
                if (!view)
                {
                    return;
                }

                if (ReleaseWhenClose)
                {
                    view.Release();
                    Object.Destroy(view.gameObject);
                    if (ReleasePrefabWhenClose && prefabCache)
                    {
                        AssetsManager.Release(prefabCache);
                        prefabCache = null;
                    }
                }
                else
                {
                    view.Canvas.enabled = false;
                }
            }

            public void Active(bool isTop)
            {
                uiState = true;
                if (!view)
                {
                    throw new InvalidOperationException("Need prepare view first");
                }

                if (!view.Active && isTop)
                {
                    var audio = view.GetComponent<IViewAudio>();
                    audio?.PlayShowAudio();
                }
                view.Active = true;
            }

            /// <summary>
            /// 仅调用Dismiss用于保证代码逻辑顺序
            /// 在新界面打开后再操作View状态隐藏或者释放
            /// </summary>
            /// <param name="gotoMode"></param>
            public void DeActive(bool gotoMode = false)
            {
                if (!view)
                {
                    return;
                }

                if (gotoMode)
                {
                    var audio = view.GetComponent<IViewAudio>();
                    audio?.PlayDismissAudio();
                }
                
                view.DismissWithoutViewChange();
            }
        }
    }
}