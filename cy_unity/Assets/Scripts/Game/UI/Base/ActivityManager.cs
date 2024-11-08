using System.Collections.Generic;
using DH.Asset;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DHFramework;
using UnityEngine;

namespace DH.Game.UI
{

    public enum WaitType
    {
        Net,
        UI,
    }
    public class ActivityManager : Singleton<ActivityManager>
    {
        private NetWaitView netWaitDlg;
        private UIWaitView uiWaitDlg;
        private int netRefCount = 0;
        private int uiRefCount = 0;

        public void Init()
        {
            var rootParent = UIManager.Instance.GetUILayerRoot(UILayersConfig.Wait);
            AssetsManager.InstantiateWithParentAsyncWithCallback("UI/Wait/NetWaitDlg", rootParent, false,
                delegate(GameObject obj)
                {
                    netWaitDlg = obj.GetComponent<NetWaitView>();
                    ForceRefresh(WaitType.Net);
                });
            
            AssetsManager.InstantiateWithParentAsyncWithCallback("UI/Wait/UIWaitDlg", rootParent, false,
                delegate(GameObject obj)
                {
                    uiWaitDlg = obj.GetComponent<UIWaitView>();
                    ForceRefresh(WaitType.UI);
                });
        }

        protected override void Release()
        {
            uiRefCount = 0;
            netRefCount = 0;
            netWaitDlg.HideMask();
            uiWaitDlg.HideMask();
            AssetsManager.ReleaseInstance(netWaitDlg.gameObject);
            AssetsManager.ReleaseInstance(uiWaitDlg.gameObject);
        }

        public bool ResponseEscapeBtn()
        {
            return !IsShow();
        }
        
        public bool IsShow()
        {
            return netRefCount > 0 || uiRefCount > 0;
        }

        public bool IsShowNet()
        {
            return netRefCount > 0;
        }

        public bool IsShowUI()
        {
            return uiRefCount > 0;
        }

        public void Show(WaitType type,float timeOut = 1)
        {
            if (type == WaitType.Net)
            {
                netRefCount++;
                if (netRefCount == 1 && netWaitDlg)
                {
                    netWaitDlg.ShowMask();
                }
            }
            else if (type == WaitType.UI)
            {
                uiRefCount++;
                if (uiRefCount == 1 && uiWaitDlg)
                {
                    uiWaitDlg.ShowMask(timeOut);
                }
            }
        }

        public void Hide(WaitType type, bool bForce = false)
        {
            if (type == WaitType.Net)
            {
                if (bForce)
                    netRefCount = 0;
                else
                    netRefCount--;
            
                netRefCount = netRefCount < 0 ? 0 : netRefCount;
                if (netRefCount == 0 && netWaitDlg)
                {
                    netWaitDlg.HideMask();
                }
            }
            else if (type == WaitType.UI)
            {
                if (bForce)
                    uiRefCount = 0;
                else
                    uiRefCount--;
            
                uiRefCount = uiRefCount < 0 ? 0 : uiRefCount;
                if (uiRefCount == 0 && uiWaitDlg)
                {
                    uiWaitDlg.HideMask();
                }
            }
        }

        public void ForceRefresh(WaitType type)
        {
            if (type == WaitType.Net)
            {
                if (netRefCount > 0)
                {
                    netWaitDlg.ShowMask();
                }
                else
                {
                    netWaitDlg.HideMask();
                }
            }
            else if (type == WaitType.UI)
            {
                if (uiRefCount > 0)
                {
                    uiWaitDlg.ShowMask();
                }
                else
                {
                    uiWaitDlg.HideMask();
                }
            }
        }



    }
}