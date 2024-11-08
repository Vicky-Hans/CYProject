using System.Collections;
using DH.UIFramework;
using UnityEngine;

namespace DH.Game.UIViews
{
    public class UIWaitView : BaseView
    {
        [SerializeField]
        private GameObject objMask;
        private Coroutine waitForTimeout;
        private WaitForSeconds waitTime;

        private float timeOut = 1;
        private void Awake()
        {
            Canvas.overrideSorting = true;
            Canvas.sortingOrder = 30000;
        }
    
        public void ShowMask(float timeOut=1)
        {
            waitTime = new WaitForSeconds(timeOut);
            this.timeOut = timeOut;
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