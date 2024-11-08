using System;
using System.Collections.Generic;
using DHFramework;
using UnityEngine;

namespace DHBestHttp
{
    public class UnityHttpHelper : DHUtility.IHttpHelper
    {
        [RuntimeInitializeOnLoadMethod]
        internal static void Initialize()
        {
            Debug.Log("UnityHttpHelper Initialize...");
            
            DHUtility.Http.SetHttpHelper(new UnityHttpHelper());
        }
        
        public object SendRequest(DHFramework.HTTPMethods method, string url, Dictionary<string, string> header, string data, Action<int, string, string> callback,
            bool urlEncoded = true)
        {
            return UnityHttpRequestManager.Instance.SendRequest(method, url, header, data, callback, false);
        }
    }
}
