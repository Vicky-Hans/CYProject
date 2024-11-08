using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace DHBestHttp
{
    [ExecuteInEditMode]
    public class UnityHttpRequestManager : MonoBehaviour
    {
        private class RequestItem
        {
            public UnityWebRequest request;
            public Action<int, string, string> callback;
            public UnityWebRequestAsyncOperation asyncOP;

            public void Release()
            {
                asyncOP = null;
                request.Dispose();
                request = null;
                callback = null;
            }
        }

        private List<RequestItem> requestQueue = new List<RequestItem>();
        private List<RequestItem> finishRequestItems = new List<RequestItem>();
        
        private static UnityHttpRequestManager instance;
        public static UnityHttpRequestManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var obj = new GameObject(nameof(UnityHttpRequestManager));
                    instance = obj.AddComponent<UnityHttpRequestManager>();
                }

                return instance;
            }
        }

        private void Awake()
        {
            instance = this;

#if !UNITY_EDITOR
            DontDestroyOnLoad(gameObject);
#endif
        }

        private void OnDestroy()
        {
            while (finishRequestItems.Count > 0)
            {
                var finishItem = finishRequestItems[0];
                requestQueue.Remove(finishItem);
                finishRequestItems.RemoveAt(0);
                finishItem.Release();
            }
            
            while (requestQueue.Count > 0)
            {
                var item = requestQueue[0];
                requestQueue.RemoveAt(0);
                item.Release();
            }

            instance = null;
        }


        public object SendRequest(DHFramework.HTTPMethods method, string url, Dictionary<string, string> header,
            string data, Action<int, string, string> callback,
            bool urlEncoded = true)
        {
            UnityWebRequest webRequestItem = null;
            if (method == DHFramework.HTTPMethods.Get)
            {
                webRequestItem = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            }
            else if (method == DHFramework.HTTPMethods.Post)
            {
                if(header != null)
                {
                    var formData = new WWWForm();
                    formData.AddField("data", data);
                    webRequestItem = UnityWebRequest.Post(url, formData);
                    foreach (var item in header)
                    {
                        webRequestItem.SetRequestHeader(item.Key, item.Value);
                    }
                }
                else
                {
                    webRequestItem = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
                    webRequestItem.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
                    webRequestItem.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data));
                }
            }

            webRequestItem.downloadHandler = new DownloadHandlerBuffer();

            RequestItem requestItem = new RequestItem();
            requestItem.callback = callback;
            requestItem.request = webRequestItem;
            requestItem.asyncOP = requestItem.request.SendWebRequest();

            requestQueue.Add(requestItem);

            return null;
        }


        private void Update()
        {
            if (requestQueue.Count < 1)
            {
                return;
            }
            
            finishRequestItems.Clear();
            foreach (var requestItem in requestQueue)
            {
                if (requestItem.asyncOP.isDone)
                {
                    finishRequestItems.Add(requestItem);
                }
            }

            while (finishRequestItems.Count > 0)
            {
                var finishItem = finishRequestItems[0];
                requestQueue.Remove(finishItem);
                finishRequestItems.RemoveAt(0);
                HandleFinishHttpRequest(finishItem);
                finishItem.Release();
            }
        }

        private void HandleFinishHttpRequest(RequestItem requestItem)
        {
            int code = -1;
            string data = "";
            string message = "";
            
            
            if (requestItem.request.isHttpError)
            {
                message = requestItem.request.error;
            }
            else if (requestItem.request.isNetworkError)
            {
                message = requestItem.request.error;
            }
            else if (requestItem.request.isNetworkError)
            {
                message = requestItem.request.error;
            }
            else
            {
                code = 0;
                data = requestItem.request.downloadHandler.text;
                message = "";
            }

            requestItem.callback?.Invoke(code, data, message);
            
        }
        
    }
}