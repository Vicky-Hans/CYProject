using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.UIFramework.Proxy;
using DHFramework;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace DH.UIFramework
{
    public partial class BaseView
    {
        private Dictionary<string, TaskItem<Material>> cacheMaterial = new();
        
        private void ReleaseMaterialCache()
        {
            foreach (var item in cacheMaterial)
            {
                if (item.Value.instance)
                {
                    AssetsManager.Release(item.Value.instance);
                    item.Value.instance = null;
                }

                ReferencePool.Release(item.Value);
            }

            cacheMaterial.Clear();
        }

        public void ConvertDirectly(string path, Graphic renderer)
        {
            if (cacheMaterial.TryGetValue(path, out var cache) && cache.instance)
            {
                renderer.material = cache.instance;
                return;
            }

            if (cache == null)
            {
                cache = ReferencePool.Acquire<TaskItem<Material>>();
#if UNITY_EDITOR
                cache.path = path;
#endif
                cacheMaterial.Add(path, cache);
            }

            ConvertDirectlyWrap(path, renderer, cache).Forget();
        }

        private async UniTaskVoid ConvertDirectlyWrap(string path, Graphic renderer, TaskItem<Material> taskItem)
        {
            var sprite = await LoadMaterial(path, taskItem);
            if (cts.IsCancellationRequested) return;
            renderer.material = sprite;
        }

        public void ConvertMaterial(string path, IModifiable target)
        {
            if (string.IsNullOrEmpty(path)) return;

            if (cts.IsCancellationRequested) return;

            if (cacheMaterial.TryGetValue(path, out var cache) && cache.instance)
            {
                target.SetValue(cache.instance);
                return;
            }

            if (cache == null)
            {
                cache = ReferencePool.Acquire<TaskItem<Material>>();
                cacheMaterial.Add(path, cache);
            }

            ConvertMaterialWrap(path, target, cache).Forget();
        }

        private async UniTaskVoid ConvertMaterialWrap(string path, IModifiable target, TaskItem<Material> taskItem)
        {
            var mat = await LoadMaterial(path, taskItem);
            if (cts.IsCancellationRequested) return;
            
            target.SetValue(mat);
        }

        private async UniTask<Material> LoadMaterial(string path, TaskItem<Material> taskData)
        {
            Material mat = null;
            var tcs = AutoResetUniTaskCompletionSource<Material>.Create();
            var taskItem = taskData;
            if (taskItem.pendingTasks == null)
            {
                taskItem.pendingTasks = ListPool<AutoResetUniTaskCompletionSource<Material>>.Get();
                mat = await AssetsManager.LoadAssetAsync<Material>(path);
                if (cts.IsCancellationRequested)
                {
                    AssetsManager.Release(mat);
                    return null;
                }

                taskItem.instance = mat;
                foreach (var task in taskItem.pendingTasks) task.TrySetResult(mat);
                ListPool<AutoResetUniTaskCompletionSource<Material>>.Release(taskItem.pendingTasks);
                taskItem.pendingTasks = null;
                return mat;
            }

            taskItem.pendingTasks.Add(tcs);
            mat = await tcs.Task;
            return mat;
        }
    }
}