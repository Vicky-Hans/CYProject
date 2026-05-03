using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.UIFramework.Converters;
using DH.UIFramework.Proxy;
using DH.UIFramework.Proxy.Targets;
using DHFramework;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace DH.UIFramework
{
    public partial class BaseView : IConverter
    {
        private class TaskItem<T> : IReference
        {
            public List<AutoResetUniTaskCompletionSource<T>> pendingTasks;
            public T instance;
#if UNITY_EDITOR
            public string path;
#endif

            public void Clear()
            {
                if (pendingTasks != null)
                {
                    ListPool<AutoResetUniTaskCompletionSource<T>>.Release(pendingTasks);
                    pendingTasks = null;
                }

#if UNITY_EDITOR
                path = "Cleared";
#endif
            }
        }

        protected Dictionary<Image, bool> imageNativeSizeDic = new ();
        private Dictionary<string, TaskItem<Sprite>> cacheSprite = new();
        protected CancellationTokenSource cts = new();

        private void ReleaseSpriteConverter()
        {
            cts.Cancel();
            foreach (var item in cacheSprite)
            {
                if (item.Value.instance)
                {
                    AssetsManager.Release(item.Value.instance);
                    item.Value.instance = null;
                }

                ReferencePool.Release(item.Value);
            }

            cacheSprite.Clear();
        }

        public void ConvertDirectly(string path, Image image)
        {
            if (cacheSprite.TryGetValue(path, out var cache) && cache.instance)
            {
                image.sprite = cache.instance;
                RefreshImageNativeSize(image);
                return;
            }

            if (cache == null)
            {
                cache = ReferencePool.Acquire<TaskItem<Sprite>>();
#if UNITY_EDITOR
                cache.path = path;
#endif
                cacheSprite.Add(path, cache);
            }

            ConvertDirectlyWrap(path, image, cache).Forget();
        }

        private async UniTaskVoid ConvertDirectlyWrap(string path, Image image, TaskItem<Sprite> taskItem)
        {
            // 如果Image没有赋值默认图片则需要在加载资源时隐藏图片
            if (!image.sprite)
            {
                image.enabled = false;
            }
            var sprite = await LoadSprite(path, taskItem);
            if (cts.IsCancellationRequested) return;
            image.enabled = true;
            image.sprite = sprite;
            RefreshImageNativeSize(image);
        }

        private void AdaptImageAlpha(IModifiable target)
        {
            if ((target is PropertyTargetProxy field) && (field.Target is Image image))
            {
                RefreshImageNativeSize(image);
                // var color = image.color;
                // color.a = 1;
                // image.color = color;
            }
        }

        public void Convert(object value, IModifiable target)
        {
            if (value == null) return;

            if (cts.IsCancellationRequested) return;

            var path = value.ToString();
            if (cacheSprite.TryGetValue(path, out var cache) && cache.instance)
            {
                target.SetValue(cache.instance);
                AdaptImageAlpha(target);
                return;
            }


            if (cache == null)
            {
                cache = ReferencePool.Acquire<TaskItem<Sprite>>();
                cacheSprite.Add(path, cache);
            }

            ConvertWrap(path, target, cache).Forget();
        }

        private async UniTask<Sprite> LoadSprite(string path, TaskItem<Sprite> taskData)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            
            Sprite sprite = null;
            var tcs = AutoResetUniTaskCompletionSource<Sprite>.Create();
            var taskItem = taskData;
            if (taskItem.pendingTasks == null)
            {
                taskItem.pendingTasks = ListPool<AutoResetUniTaskCompletionSource<Sprite>>.Get();
                AssetsManager.LoadSpriteAsync(path, (spriteItem) =>
                {
                    if (cts.IsCancellationRequested)
                    {
                        AssetsManager.Release(spriteItem);
                        return;
                    }

                    taskItem.instance = spriteItem;
                    foreach (var task in taskItem.pendingTasks) task.TrySetResult(spriteItem);
                    ListPool<AutoResetUniTaskCompletionSource<Sprite>>.Release(taskItem.pendingTasks);
                    taskItem.pendingTasks = null;
                });
            }

            taskItem.pendingTasks.Add(tcs);
            sprite = await tcs.Task;
            return sprite;
        }

        private async UniTaskVoid ConvertWrap(string path, IModifiable target, TaskItem<Sprite> taskItem)
        {
            Image image = null;
            if (target is ITargetProxy { Target: Image imageTarget } proxy && !imageTarget.sprite)
            {
                image = proxy.Target as Image;
                image.enabled = false;
            }
            
            // 如果Image没有赋值默认图片则需要在加载资源时隐藏图片
            var sprite = await LoadSprite(path, taskItem);
            if (cts.IsCancellationRequested) return;
            if (image)
            {
                image.enabled = true;
            }
            target.SetValue(sprite);
            AdaptImageAlpha(target);
        }

        public object ConvertBack(object value)
        {
            throw new NotImplementedException();
        }

        protected void SetImageIsNativeSize(Image img, bool native)
        {
            if (!imageNativeSizeDic.ContainsKey(img))
            {
                imageNativeSizeDic.Add(img, native);
            }
            else
            {
                imageNativeSizeDic[img] = native;
            }
        }

        private void RefreshImageNativeSize(Image img)
        {
            if (imageNativeSizeDic.TryGetValue(img, out var native) && native)
            {
                img.SetNativeSize();
            }
        }
    }
}