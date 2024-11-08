using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DHFramework;
using UnityEngine;
using UnityEngine.Pool;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace DH.Game
{
    public class BaseAssetEntity : ObservableMonoBehavior, IPool<GameObject>
    {
        private class DependencyItem : IReference
        {
            public bool clear;
            public bool loaded;
            public readonly List<BaseAssetEntity> onCompleted = new();
            public readonly List<AutoResetUniTaskCompletionSource> tasks = new();

            public void Invoke()
            {
                foreach (var entity in onCompleted)
                {
                    if (!entity || entity.released)
                    {
                        continue;
                    }
                    
                    entity.OnAssetsLoaded();
                }

                foreach (var action in tasks) action.TrySetResult();
            }

            public void Cancel()
            {
                foreach (var action in tasks) action.TrySetCanceled();
            }

            public void Clear()
            {
                clear = true;
                onCompleted.Clear();
                tasks.Clear();
            }
        }

        /// <summary>
        /// 强制取消资源自主管理
        /// </summary>
        internal bool forceDisableChildrenManage;

        /// <summary>
        /// 所有资源加载行为虽然由异步，当时都在Unity主线程触发，只需要使用一个List用于统一的缓存即可
        /// HashSet保证Path唯一
        /// </summary>
        private static readonly HashSet<string> AsyncPathCache = new();

        protected readonly Dictionary<string, Object> assetsContainer = new();

        /// <summary>
        /// 防止同时加载同一个资源，导致逻辑错误
        /// </summary>
        private readonly Dictionary<string, List<AutoResetUniTaskCompletionSource>> assetLoadingTasks = new();

        protected readonly Dictionary<string, Object> syncAssetsContainer = new();

        /// <summary>
        /// 逻辑父节点托管了资源加载、实例化、释放等功能
        /// </summary>
        [NonSerialized] private BaseAssetEntity assetManager;

        private bool assetLoadCompleted;
        private bool loadedAssets;
        private bool released;

        /// <summary>
        /// 等待资源加载完成对象
        /// </summary>
        private readonly Dictionary<string, DependencyItem> loadingDepActions = new();

        protected readonly HashSet<BaseAssetEntity> pendingAssetsTargets = new();

        protected CancellationTokenSource cts = new();

        /// <summary>
        /// 是否具备管理资源的资格，否则将由逻辑父节点管理资源，自身不具备资源管理资格
        /// </summary>
        protected virtual bool ManageAssets => assetManager == null;

        /// <summary>
        /// 所有资源加载行为虽然由异步，当时都在Unity主线程触发，只需要使用一个List用于统一的缓存即可
        /// HashSet保证Path唯一
        /// </summary>
        public static HashSet<string> PathCache => AsyncPathCache;


        private static void GetTargetAssetsPath(BaseAssetEntity target,
            BaseAssetEntity root,
            HashSet<string> asyncPaths,
            HashSet<string> syncPaths,
            Dictionary<string, Object> syncContainer)
        {
            var hashSet = HashSetPool<string>.Get();
            target.ProduceSyncPath(hashSet);
            foreach (var path in hashSet)
            {
                if (syncContainer.ContainsKey(path))
                    continue;
                if (syncPaths != null) syncPaths.Add(path);

                var asset = AssetsManager.LoadAssetSync(path);
                syncContainer.Add(path, asset);
                var gameObj = asset as GameObject;
                if (!gameObj) continue;
                var child = gameObj.GetComponent<BaseAssetEntity>();
                if (!child)
                {
                    throw new Exception($"{path}使用了同步资源加载，必须包含继承BaseAssetEntity的组件");
                }
                if (root.forceDisableChildrenManage || !child.ManageAssets)
                    GetTargetAssetsPath(child, root, asyncPaths, syncPaths, syncContainer);
            }

            HashSetPool<string>.Release(hashSet);

            target.ProduceAsyncPath(asyncPaths);
        }

        protected virtual void ProduceSyncPath(HashSet<string> syncPath)
        {
        }

        protected virtual void ProduceAsyncPath(HashSet<string> asyncPath)
        {
        }

        protected async UniTask LoadAssets(string path)
        {
            if (assetLoadingTasks.TryGetValue(path, out var tasks))
            {
                var tcs = AutoResetUniTaskCompletionSource.Create();
                tasks.Add(tcs);
                await tcs.Task;
                return;
            }
            else
            {
                assetLoadingTasks.Add(path, new List<AutoResetUniTaskCompletionSource>());
            }

            var result = await AssetsManager.LoadAssetAsync<Object>(path, cts.Token);
            if (cts == null || cts.IsCancellationRequested) return;

            if (assetLoadingTasks.TryGetValue(path, out tasks))
                foreach (var task in tasks)
                    task.TrySetResult();

            assetsContainer.Add(path, result);
            assetLoadingTasks.Remove(path);
        }

        protected async UniTask LoadAssets()
        {
            released = false;
            
            if (!ManageAssets) return;

            if (loadedAssets)
            {
                Debug.LogError("Invalid operation for load assets");
                return;
            }

            loadedAssets = true;
            cts = new CancellationTokenSource();

            var tasks = new List<UniTask>();
            PathCache.Clear();
            GetTargetAssetsPath(this, this, PathCache, null, syncAssetsContainer);
            foreach (var path in PathCache) tasks.Add(LoadAssets(path));
            PathCache.Clear();
            await UniTask.WhenAll(tasks);
            if (cts.IsCancellationRequested)
            {
                pendingAssetsTargets.Clear();
            }
            else
            {
                OnAssetsLoaded();
            }
        }

        /// <summary>
        /// 仅用于手动管理资源加载模式，加载指定的资源
        /// 只用于Logic资源同步加载，同时不存在任何定义了[AssetPath][SyncPath]的资源
        /// </summary>
        /// <param name="path"></param>
        public GameObject LoadAssetSync(string path)
        {
            if (assetManager) return assetManager.LoadAssetSync(path);
            
            if (syncAssetsContainer.TryGetValue(path, out var prefab)) return (GameObject)prefab;
            var asset = AssetsManager.LoadAssetSync<GameObject>(path);
            var entity = asset.GetComponent<BaseAssetEntity>();
            syncAssetsContainer.Add(path, asset);
            if (entity == null)
            {
                Debug.Log("null");
            }
            LoadDependencyAsync(entity, path).Forget();
            return asset;
        }

        private async UniTaskVoid LoadDependencyAsync(BaseAssetEntity entity, string assetPath)
        {
            var tasks = new List<UniTask>();
            var asyncPath = HashSetPool<string>.Get();
            var syncPath = HashSetPool<string>.Get();
            GetTargetAssetsPath(entity, this, asyncPath, syncPath, syncAssetsContainer);
            foreach (var cachePath in asyncPath)
            {
                if (assetsContainer.ContainsKey(cachePath)) continue;
                tasks.Add(LoadAssets(cachePath));
            }

            // 由于多Key映射同一个dep对象，需要置位clear位
            var dep = ReferencePool.Acquire<DependencyItem>();
            dep.loaded = false;
            dep.clear = false;
            loadingDepActions.Add(assetPath, dep);
            foreach (var path in syncPath) loadingDepActions.Add(path, dep);

            HashSetPool<string>.Release(asyncPath);
            HashSetPool<string>.Release(syncPath);

            await UniTask.WhenAll(tasks);
            OnAssetsDepLoaded(assetPath);
        }

        private void OnAssetsDepLoaded(string asyncPath)
        {
            if (!loadingDepActions.TryGetValue(asyncPath, out var action)) return;

            action.loaded = true;
            action.Invoke();
            action.onCompleted.Clear();
            ReferencePool.Release(action);
        }

        protected virtual void OnAssetsLoaded()
        {
            assetLoadCompleted = true;
            foreach (var item in pendingAssetsTargets)
            {
                if (!item || item.released)
                {
                    continue;
                }
                item.OnAssetsLoaded();
            };
            pendingAssetsTargets.Clear();
        }

        protected T GetAsset<T>(string assetPath) where T : Object
        {
            if (assetManager) return assetManager.GetAsset<T>(assetPath);

            return assetsContainer[assetPath] as T;
        }

        protected T GetSyncAsset<T>(string assetPath) where T : Object
        {
            if (assetManager) return assetManager.GetSyncAsset<T>(assetPath);

            return syncAssetsContainer[assetPath] as T;
        }

        [Conditional("DH_DEBUG")]
        private void AssetDebug(string message, Component target)
        {
            Debug.Log(message, target);
        }

        protected void SetAssetEntity(BaseAssetEntity component, string assetPath)
        {
            if (!component) return;

            if (!forceDisableChildrenManage && component.ManageAssets) return;

            component.assetManager = assetManager ? assetManager : this;
            var deps = assetManager ? assetManager.loadingDepActions : loadingDepActions;
            if (deps.TryGetValue(assetPath, out var dependencyItem))
            {
                if (dependencyItem.loaded)
                    component.OnAssetsLoaded();
                else
                    dependencyItem.onCompleted.Add(component);
            }
            else
            {
                if (assetLoadCompleted)
                {
                    component.OnAssetsLoaded();
                }
                else
                {
                    pendingAssetsTargets.Add(component);
                    AssetDebug("Add pending assets target", component);
                }
            }
        }

        /// <summary>
        /// 实例化对象，该函数只能被对象池派生时重写
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public virtual GameObject InstantiateObj(string assetPath, Vector3 position, Quaternion rotation,
            Transform parent)
        {
            if (assetManager) return assetManager.InstantiateObj(assetPath, position, rotation, parent);

            Object prefab;
            if (syncAssetsContainer.TryGetValue(assetPath, out prefab))
            {
                var result = Instantiate(prefab, position, rotation, parent) as GameObject;
                var component = result.GetComponent<BaseAssetEntity>();
                SetAssetEntity(component, assetPath);
                return result;
            }

            if (assetsContainer.TryGetValue(assetPath, out prefab))
                return Instantiate(prefab, position, rotation) as GameObject;

            return null;
        }

        public virtual GameObject InstantiateObj(string assetPath, Transform parent,
            bool instantiateInWorldSpace = false)
        {
            if (assetManager) return assetManager.InstantiateObj(assetPath, parent, instantiateInWorldSpace);

            Object prefab;
            if (syncAssetsContainer.TryGetValue(assetPath, out prefab))
            {
                var prefabEntity = ((GameObject)prefab).GetComponent<BaseAssetEntity>();
                var result = Instantiate(prefab, parent, instantiateInWorldSpace) as GameObject;
                var component = result.GetComponent<BaseAssetEntity>();
                SetAssetEntity(component, assetPath);
                return result;
            }

            if (assetsContainer.TryGetValue(assetPath, out prefab))
                return Instantiate(prefab, parent, instantiateInWorldSpace) as GameObject;

            return null;
        }

        public virtual void ReleaseObj(GameObject obj)
        {
            if (assetManager)
            {
                assetManager.ReleaseObj(obj);
                return;
            }

            BaseAssetEntity behavior = null;
            if (pendingAssetsTargets.Count > 0 && (behavior = obj.GetComponent<BaseAssetEntity>()))
                pendingAssetsTargets.Remove(behavior);

            Destroy(obj);
        }

        public void ReleaseAssets()
        {
            released = true;
            // 不具备自主管理资源的对象，不需要释放资源
            if (!ManageAssets) return;

            if (cts == null) return;

            cts.Cancel();
            cts.Dispose();
            cts = null;

            foreach (var item in assetsContainer)
            {
                if (!item.Value) continue;

                AssetsManager.Release(item.Value);
            }

            assetsContainer.Clear();


            foreach (var item in syncAssetsContainer)
            {
                if (!item.Value) continue;

                AssetsManager.Release(item.Value);
            }

            foreach (var item in loadingDepActions)
            {
                if (item.Value.clear) continue;

                ReferencePool.Release(item.Value);
            }

            syncAssetsContainer.Clear();
            pendingAssetsTargets.Clear();
            loadingDepActions.Clear();
            assetLoadingTasks.Clear();
            assetManager = null;
        }

        [ContextMenu("DumpCacheAssets")]
        private void DumpCacheAssets()
        {
            var stringBuilder = new StringBuilder();
            foreach (var item in assetsContainer) stringBuilder.AppendLine(item.Key);

            Debug.Log(stringBuilder.ToString());
        }
    }
}