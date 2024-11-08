using System.Collections;
using DH.UIFramework;
using UnityEngine;

namespace DH.Game.UIViews
{
    public class NetWaitView : BaseView
    {
        [SerializeField]
        private GameObject objMask;
        [SerializeField]
        private GameObject icon;

        private Coroutine waitForTimeout;
        private WaitForSeconds waitTime = new WaitForSeconds(timeOut);

        private const float timeOut = 3;
    
        private void Awake()
        {
            Canvas.overrideSorting = true;
            Canvas.sortingOrder = 30000;
        }
        
        public void ShowMask()
        {
            gameObject.SetActive(true);
            waitForTimeout = StartCoroutine(WaitForTimeOut());
        }

        public void HideMask()
        {
            if (waitForTimeout != null)
            {
                StopCoroutine(waitForTimeout);
            }
            objMask.SetActive(false);
            gameObject.SetActive(false);
        }

        private IEnumerator WaitForTimeOut()
        {
            yield return waitTime;
            objMask.SetActive(true);
            waitForTimeout = null;
        }
    }
}