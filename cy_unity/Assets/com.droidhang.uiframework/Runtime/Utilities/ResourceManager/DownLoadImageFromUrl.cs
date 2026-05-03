using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using DHFramework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
#if UNITY_2018_3_OR_NEWER
using UnityEngine.Networking;

#endif

namespace DH.UIFramework
{
    public class DownLoadImageFromUrl : MonoBehaviour
    {
        private class ReferenceTexture2D
        {
            public Texture2D Texture2D = null;
            public int ReferenceCount = 0;
        }

        /// <summary>
        /// 不再使用LoadFromCache标记，本地缓存也使用UnityWebRequest加载
        /// 由Unity底层进行Texture的创建
        /// </summary>
        public enum DownloadType
        {
            LoadFromCache = 1,
            LoadFromUrl = 2,
        }

        private static readonly byte[] PreAllocBuffer = new byte[4 * 1024];

        private static Dictionary<string, DownLoadImageFromUrl> underDownloadProcesses =
            new Dictionary<string, DownLoadImageFromUrl>();

        private static string filePath = $"{Application.persistentDataPath}/DownLoadImage/";

        /// <summary>
        /// 图片引用字典容器
        /// </summary>
        private static Dictionary<string, ReferenceTexture2D> loadedTextureDic =
            new Dictionary<string, ReferenceTexture2D>();

        private bool enableLog = false;
        private bool cached = true;
        private string uniqueHash;
        private int progress;
        private bool success = false;
        private string absoluteFilePath;
        private string url;

        private Action onStartAction;
        private Action<Texture2D> onDownloadedAction; //这个是下载完图片了
        private Action<Texture> onLoadedAction; //这个是图片加载显示完成
        private Action<int> loadStatusAction; //是本地缓存还是网络下载 
        private Action<string> onErrorAction;


        /// <summary>
        /// Set image url for download.
        /// </summary>
        /// <param name="url">Image Url</param>
        /// <returns></returns>
        public DownLoadImageFromUrl Load(string url)
        {
            if (enableLog)
                Debug.Log("[DownLoadImageFromUrl] Url set : " + url);
            this.url = url;
            return this;
        }

        #region Actions

        public DownLoadImageFromUrl WithStartAction(Action action)
        {
            this.onStartAction = action;

            if (enableLog)
                Debug.Log("[DownLoadImageFromUrl] On start action set : " + action);

            return this;
        }

        public DownLoadImageFromUrl WithDownloadedAction(Action<Texture2D> action)
        {
            this.onDownloadedAction = action;

            if (enableLog)
                Debug.Log("[DownLoadImageFromUrl] On downloaded action set : " + action);

            return this;
        }

        public DownLoadImageFromUrl WithDownloadStatusAction(Action<int> action)
        {
            this.loadStatusAction = action;

            return this;
        }

        public DownLoadImageFromUrl WithLoadedAction(Action<Texture> action)
        {
            this.onLoadedAction = action;

            if (enableLog)
                Debug.Log("[DownLoadImageFromUrl] On loaded action set : " + action);

            return this;
        }

        public DownLoadImageFromUrl WithErrorAction(Action<string> action)
        {
            this.onErrorAction = action;

            if (enableLog)
                Debug.Log("[DownLoadImageFromUrl] On OnError action set : " + action);

            return this;
        }

        #endregion

        /// <summary>
        /// Enable cache
        /// </summary>
        /// <returns></returns>
        public DownLoadImageFromUrl SetCached(bool cached)
        {
            this.cached = cached;

            if (enableLog)
                Debug.Log("[DownLoadImageFromUrl] Cache enabled : " + cached);

            return this;
        }

        /// <summary>
        /// Start DownLoadImageFromUrl process.
        /// </summary>
        public void StartLoad()
        {
            if (url == null)
            {
                OnError("Url has not been set. Use 'Load' funtion to set image url.");
                return;
            }

            try
            {
                Uri uri = new Uri(url);
                this.url = uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                OnError("Url is not correct.");
                return;
            }

            if (enableLog)
                Debug.Log("[DownLoadImageFromUrl] Start Working.");

            if (onStartAction != null)
                onStartAction.Invoke();

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            uniqueHash = CreateMD5(url);
            if (loadedTextureDic.TryGetValue(uniqueHash, out var item))
            {
                item.ReferenceCount++;
                if (item.Texture2D)
                {
                    OnLoadedTexture(item.Texture2D);
                    return;
                }

                Assert.IsTrue(underDownloadProcesses.ContainsKey(uniqueHash), "UnExpanding downloaded state");
            }
            else
            {
                loadedTextureDic.Add(uniqueHash, new ReferenceTexture2D()
                {
                    ReferenceCount = 1,
                });
            }

            absoluteFilePath = DHUtility.Path.GetRegularPath(Path.Combine(filePath, $"{uniqueHash}.png"));

            if (underDownloadProcesses.ContainsKey(uniqueHash))
            {
                loadStatusAction?.Invoke((int) DownloadType.LoadFromUrl);

                DownLoadImageFromUrl sameProcess = underDownloadProcesses[uniqueHash];
                sameProcess.onDownloadedAction += OnDownloadedData;
                return;
            }

            if (File.Exists(absoluteFilePath))
            {
                url = DHUtility.Path.GetRemotePath(absoluteFilePath);
                loadStatusAction?.Invoke((int) DownloadType.LoadFromUrl);
                underDownloadProcesses.Add(uniqueHash, this);
                StopAllCoroutines();
                StartCoroutine(Downloader(true));
            }
            else
            {
                loadStatusAction?.Invoke((int) DownloadType.LoadFromUrl);
                underDownloadProcesses.Add(uniqueHash, this);
                StopAllCoroutines();
                StartCoroutine(Downloader(false));
            }
        }

        private IEnumerator Downloader(bool isLocal)
        {
            if (enableLog)
                Debug.Log("[DownLoadImageFromUrl] Download started.");

            Texture2D texture = null;
#if UNITY_2018_3_OR_NEWER
            using (var www = UnityWebRequestTexture.GetTexture(url))
            {
#else
            using (var www = new WWW(url))
            {
#endif
                www.SendWebRequest();
                yield return null;
                while (!www.isDone)
                {
                    if (www.error != null)
                    {
                        OnError("Error while downloading the image : " + www.error);
                        yield break;
                    }

#if UNITY_2018_3_OR_NEWER
                    progress = Mathf.FloorToInt(www.downloadProgress * 100);
#else
                    progress = Mathf.FloorToInt(www.progress * 100);
#endif
                    if (enableLog)
                        Debug.Log("[DownLoadImageFromUrl] Downloading progress : " + progress +
                                  "%");
                    yield return null;
                }

                if (www.isHttpError || www.isNetworkError)
                {
                    File.Delete(absoluteFilePath);
                }
                else
                {
                    var handler = www.downloadHandler as DownloadHandlerTexture;
                    texture = handler?.texture;
                    var sw = Stopwatch.StartNew();
                    //压缩图片在内存中的空间占用
                    texture?.Compress(false);
                    sw.Stop();
                    if (handler != null && !isLocal)
                    {
                        File.WriteAllBytes(absoluteFilePath, handler.data);
                    }
                }
            }

            underDownloadProcesses.Remove(uniqueHash);
            OnDownloadedData(texture);
        }

        private void OnDownloadedData(Texture2D texture)
        {
            onDownloadedAction?.Invoke(texture);
            if (loadedTextureDic.TryGetValue(uniqueHash, out var textureItem))
            {
                //如果引用计数为0，则是被释放了，直接销毁该下载的图片且不回调
                if (textureItem.ReferenceCount == 0)
                {
                    Destroy(texture);
                    loadedTextureDic.Remove(uniqueHash);
                    OnFinish();
                    return;
                }

                textureItem.Texture2D = texture;
                OnLoadedTexture(textureItem.Texture2D);
            }
            else
            {
                OnError("Loading image file has been failed.");
            }
        }

        private void OnLoadedTexture(Texture2D texture2D)
        {
            try
            {
                onLoadedAction?.Invoke(texture2D);
                if (enableLog)
                    Debug.Log("[DownLoadImageFromUrl] Image has been loaded.");

                success = true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                OnFinish();
            }
        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            // 需要保证字符串长度不超过1024
            // 此处按设计url长度不会超过1024
            // 此处代码为定点优化，请勿使用于其他未知长度的场合
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                int count = Encoding.ASCII.GetBytes(input, 0, input.Length, PreAllocBuffer, 0);
                byte[] hashBytes = md5.ComputeHash(PreAllocBuffer, 0, count);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }

        private void OnError(string message)
        {
            success = false;

            if (enableLog)
                Debug.LogError("[DownLoadImageFromUrl] Error : " + message);

            onErrorAction?.Invoke(message);
            OnFinish();
        }

        private void OnFinish()
        {
            if (enableLog)
                Debug.Log("[DownLoadImageFromUrl] Operation has been finished.");

            if (!cached)
            {
                try
                {
                    File.Delete(absoluteFilePath);
                }
                catch (Exception ex)
                {
                    if (enableLog)
                        Debug.LogError($"[DownLoadImageFromUrl] Error while removing cached file: {ex.Message}");
                }
            }

            Destroy(gameObject, .5f);
        }


        /// <summary>
        /// Get instance of DownLoadImageFromUrl class
        /// </summary>
        public static DownLoadImageFromUrl Get()
        {
            return new GameObject("DownLoadImageFromUrl").AddComponent<DownLoadImageFromUrl>();
        }

        public static void DestroyTexture2D(Texture2D texture2D)
        {
            Destroy(texture2D);
        }

        /// <summary>
        /// Clear a certain cached file with its url
        /// </summary>
        /// <param name="url">Cached file url.</param>
        /// <returns></returns>
        public static void ClearCache(string url)
        {
            try
            {
                string urlKey = CreateMD5(url);
                //Debug.Log($"count->{count},url->{urlKey}");
                bool isSuccess = loadedTextureDic.TryGetValue(urlKey, out var item);
                // 排除错误的对象
                if (!isSuccess)
                {
                    Debug.LogError($"not include this object,urlKey->{urlKey},item->{item}");
                    return;
                }

                item.ReferenceCount--;
                if (item.ReferenceCount < 0)
                {
                    Debug.LogError($"item.ReferenceCount can't be negative,check reference->{item.ReferenceCount}");
                    return;
                }

                if (item.ReferenceCount > 0)
                {
                    return;
                }

                // 还未被下载下来的图片直接不做销毁，且下载完成后自动释放
                if (!item.Texture2D)
                {
                }
                else
                {
                    Destroy(item.Texture2D);
                    loadedTextureDic.Remove(urlKey);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DownLoadImageFromUrl] Error while removing cached file: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear all DownLoadImageFromUrl cached files
        /// </summary>
        /// <returns></returns>
        public static void ClearAllCachedFiles()
        {
            try
            {
                if (loadedTextureDic.Count > 0)
                {
                    Debug.LogError($"[{nameof(DownLoadImageFromUrl)}] ClearAllCachedFiles Count is 0");
                }

                foreach (var item in loadedTextureDic)
                {
                    item.Value.ReferenceCount = 0;
                    Destroy(item.Value.Texture2D);
                    item.Value.Texture2D = null;
                }

                loadedTextureDic.Clear();
                Directory.Delete(filePath, true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DownLoadImageFromUrl] Error while removing cached file: {ex.Message}");
            }
        }

        public static void ClearAllLoadedSpriteCached()
        {
            try
            {
                foreach (var item in loadedTextureDic)
                {
                    item.Value.ReferenceCount = 0;
                    Destroy(item.Value.Texture2D);
                    item.Value.Texture2D = null;
                }

                loadedTextureDic.Clear();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DownLoadImageFromUrl] Error while removing cached file: {ex.Message}");
            }
        }

        private void OnDestroy()
        {
            onStartAction = null;
            onDownloadedAction = null; //这个是下载完图片了
            onLoadedAction = null; //这个是图片加载显示完成
            loadStatusAction = null; //是本地缓存还是网络下载 
            onErrorAction = null;
            StopAllCoroutines();
        }
    }
}