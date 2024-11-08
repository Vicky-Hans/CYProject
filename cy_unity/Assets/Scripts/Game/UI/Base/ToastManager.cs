using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Config;
using DH.Game.UIViews;
using DHFramework;
using UnityEngine;

namespace DH.Game.UI
{
    public class ToastManager : Singleton<ToastManager>
    {
        private const int MaxItemCount = 10;
        
        private List<ToastItemView> toastItemViews;
        private List<ToastItemView> usingItemViews;
        private List<ToastItemView> pendingRemoveItems;
        private List<string> showToastMessages;
        private Transform toastRoot;
        
        public async UniTask Init()
        {
            usingItemViews = new List<ToastItemView>();
            toastItemViews = new List<ToastItemView>();
            pendingRemoveItems = new List<ToastItemView>();
            showToastMessages = new List<string>();
            toastRoot = UIManager.Instance.GetUILayerRoot(UILayersConfig.Toast);
            await InitInternal();
            await UniTask.CompletedTask;
        }

        protected override void Release()
        {
            while (toastItemViews.Count > 0)
            {
                var item = toastItemViews[0];
                toastItemViews.RemoveAt(0);
                AssetsManager.ReleaseInstance(item.gameObject);
            }
            toastItemViews.Clear();

            while (usingItemViews.Count > 0)
            {
                var item = usingItemViews[0];
                usingItemViews.RemoveAt(0);
                AssetsManager.ReleaseInstance(item.gameObject);
            }
            usingItemViews.Clear();
            showToastMessages.Clear();
            toastItemViews = null;
            toastRoot = null;
            
            while (pendingRemoveItems.Count > 0)
            {
                var item = pendingRemoveItems[0];
                pendingRemoveItems.RemoveAt(0);
                AssetsManager.ReleaseInstance(item.gameObject);
            }
            pendingRemoveItems.Clear();
        }

        public void Update()
        {
            foreach (var itemView in usingItemViews)
            {
                itemView.UpdatePosition(Time.deltaTime);

                if (itemView.IsComplete)
                {
                    pendingRemoveItems.Add(itemView);
                    UpdateShowToastMessages(itemView.desc.text);
                }
            }

            foreach (var pendingRemoveItem in pendingRemoveItems)
            {
                pendingRemoveItem.gameObject.SetActive(false);
                toastItemViews.Add(pendingRemoveItem);
                usingItemViews.Remove(pendingRemoveItem);
            }
            pendingRemoveItems.Clear();
        }

        public static void Show(string text,bool isPlayErrorAudio = true)
        {
           
            ToastManager.Instance.ShowInternal(text,isPlayErrorAudio).Forget();
        }
        
        /// <summary>
        /// 语言表id 通知
        /// </summary>
        /// <param name="id"></param>
        public static void ShowLanguage(string id,params object[] args)
        {
            ToastManager.Instance.ShowInternal(LocalizeHelper.GetGlobal(id,args)).Forget();
        }

        private bool CheckIsShow(string text)
        {
            var ret = true;
            foreach (var tempStr in showToastMessages)
            {
                if (String.Compare(tempStr, text, StringComparison.Ordinal) == 0)
                {
                    ret = false;
                    break;
                }
            }
            return ret;
        }

        private void UpdateShowToastMessages(string text)
        {
            var index = showToastMessages.FindIndex(tempStr=> String.Compare(tempStr, text, StringComparison.Ordinal) == 0);
            if (index != -1)
            {
                showToastMessages.RemoveAt(index);
            }
        }

        private async UniTaskVoid ShowInternal(string text,bool isPlayErrorAudio = true)
        {
            
            bool isShow = CheckIsShow(text);
            if(!isShow) return;
            
            showToastMessages.Add(text);
            ToastItemView toastItemView = null;
            if (toastItemViews.Count > 0)
            {
                toastItemView = toastItemViews[0];
                toastItemViews.RemoveAt(0);
            }
            else if (usingItemViews.Count > 0)
            {
                toastItemView = usingItemViews[0];
                usingItemViews.RemoveAt(0);
            }
            else
            {
                return;
            }
            
            toastItemView.UpdateInfo(text);
            toastItemView.transform.localPosition = Vector3.zero;
            toastItemView.gameObject.SetActive(true);
            if (isPlayErrorAudio)
                AudioManager.Instance.PlayWrongTips();

            usingItemViews.Add(toastItemView);
        }

        private async UniTask InitInternal()
        {
            for (int i = 0; i < MaxItemCount; i++)
            {
                var toastItem = await AssetsManager.InstantiateWithParentAsync("UI/Common/Items/ToastItem", toastRoot, false);
                toastItem.SetActive(false);
                var toastItemView = toastItem.GetComponent<ToastItemView>();
                toastItemViews.Add(toastItemView);
            }
        }
        
    }
}