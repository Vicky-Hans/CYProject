using DH.Config;
using DH.Data;
using DH.Proto;
using DH.UIFramework;
using DHFramework;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game
{
    public class BroadCostView : BaseItemView
    {
        public RectTransform bgRect;
        public Text descText;
        public RectTransform descRectTransform;

        private float scrollSpeed = 400f;
        private bool isRolling;
        private int stopDuration = 2;
       
        private bool scrollingFirstText = true;
        private bool scrollingLastText;
        private long nextMoveTime;
        public bool IsStartScrolling { get; set; }

        public void Awake()
        {
            scrollSpeed = 1f;// ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.MarqueeSpeed).Content[0];
            ApplySortingOrder(10000);
            ResetBaseInfo();
        }

        private void Update()
        {
            // 是否开始滚动
            if(!IsStartScrolling) return;
            
            ScrollBroadCost();
        }
        private void ScrollBroadCost()
        {
            // 是否是最开始滚动 最右边
            if (scrollingFirstText)
            {
                // 滚动第一个文字到最左边
                descRectTransform.localPosition -= new Vector3(scrollSpeed * Time.deltaTime, 0f);
                if (descRectTransform.localPosition.x <= -bgRect.rect.width/2)
                {
                    scrollingFirstText = false;
                    scrollingLastText = false;
                    nextMoveTime = Lodash.GetUnixTime() + stopDuration;
                }
            }
            // 停留两秒
            else if (!scrollingFirstText && !scrollingLastText)
            {
                // 停留两秒
                var nowTime = Lodash.GetUnixTime();
                if (nowTime >= nextMoveTime)
                {
                    scrollingLastText = true;
                }
            }
            // 是否是最后滚动 最左边
            else if (scrollingLastText)
            {
                // 滚动最后一个文字到最左边
                descRectTransform.localPosition -= new Vector3(scrollSpeed * Time.deltaTime, 0f);
                if (descRectTransform.localPosition.x <= -(descRectTransform.rect.width + bgRect.rect.width /2))
                {
                    scrollingLastText = false;
                    ResetBaseInfo();
                    BroadCostManager.Instance.CheckAndShowBroadCast();
                }
            }
        }

        private void ResetBaseInfo()
        {
            // 将第一个文字移动到最右边
            descText.text = "";
            descRectTransform.localPosition = new Vector2(bgRect.rect.width/2, 0);
            IsStartScrolling = false;
            scrollingFirstText = true;
            scrollingLastText = false;
            nextMoveTime = 0;
        }
        
        /// <summary>
        /// 隐藏公告
        /// </summary>
        public void HideBroadCost()
        {
            bgRect.gameObject.SetActive(false);
        }
        

        private string ParseDescStr(int cfgId, string[] prams)
        {
            return "";
        }

        private string GetColorStr(string str, string colorStr)
        {
            string ret = $"<color={colorStr}>{str}</color>";
            return ret;
        }
    }
}