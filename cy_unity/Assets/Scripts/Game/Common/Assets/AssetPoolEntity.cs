using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace DH.Game
{
    public partial class AssetPoolEntity : BaseAssetEntity
    {
        private class ObjectPool
        {
            public readonly List<GameObject> recycledItems = new List<GameObject>();
            public readonly List<GameObject> usingItems = new List<GameObject>();
            public string assetPath;
            public GameObject prefab;
            public float nextReleaseTime;
            public int capacity;
            public AssetPoolEntity owner;

            public GameObject Acquire(Vector3 position, Quaternion rotation,Transform parent)
            {
                if (capacity > 100 && usingItems.Count >= capacity)
                {
                    return null;
                }
                
                if (recycledItems.Count > 0)
                {
                    var obj = recycledItems[0];
#if UNITY_EDITOR
                    // 仅用于编辑器模式排查故障
                    if (!obj)
                    {
                        Debug.LogError($"Asset {assetPath} use incorrect release mode");
                        return null;
                    }
#endif
                    
                    usingItems.Add(obj);
                    recycledItems.RemoveAt(0);
                    obj.SetActive(true);
                    obj.transform.SetParent(parent);   
                    obj.transform.SetPositionAndRotation(position,rotation);
                    obj.transform.localScale = Vector3.one;
                    owner.instancePoolMap.Add(obj,this);
                    owner.releasedPoolMap.Remove(obj);
                    return obj;
                }
                else
                {
                    var obj = Instantiate(prefab, position, rotation,parent) as GameObject;
                    usingItems.Add(obj);
                    owner.instancePoolMap.Add(obj,this);
                    return obj;
                }
            }
            
            public GameObject Acquire(Transform parent, bool worldPositionStays = false)
            {
                if (capacity > 100 && usingItems.Count >= capacity)
                {
                    return null;
                }
                
                if (worldPositionStays)
                {
                    throw new Exception("Not support this mode when use object pool");
                }
                
                if (recycledItems.Count > 0)
                {
                    var obj = recycledItems[0];
#if UNITY_EDITOR
                    if (!obj)
                    {
                        Debug.LogError($"Asset {assetPath} use incorrect release mode");
                        return null;
                    }
#endif
                    
                    usingItems.Add(obj);
                    recycledItems.RemoveAt(0);
                    
                    obj.SetActive(true);
                    obj.transform.SetParent(parent,false);
                    obj.transform.SetLocalPositionAndRotation(Vector3.zero,Quaternion.identity);
                    obj.transform.localScale = Vector3.one;
                    owner.instancePoolMap.Add(obj,this);
                    owner.releasedPoolMap.Remove(obj);
                    return obj;
                }
                else
                {
                    var obj = Instantiate(prefab, parent,false) as GameObject;
                    usingItems.Add(obj);
                    owner.instancePoolMap.Add(obj,this);
                    return obj;
                }
            }

            public void Recycle(GameObject gameObject)
            {
                owner.instancePoolMap.Remove(gameObject);
                
                if (capacity == 0 || recycledItems.Count >= capacity)
                {
                    usingItems.Remove(gameObject);
                    Destroy(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
#if UNITY_EDITOR
                    gameObject.transform.SetParent(owner.transform);
#else
                    gameObject.transform.SetParent(null);
#endif
                    recycledItems.Add(gameObject);
                    usingItems.Remove(gameObject);
                }
            }
        }
        
        private readonly Dictionary<string, ObjectPool> poolContainer = new Dictionary<string,ObjectPool>();
        /// <summary>
        /// 实例对象对应的对象池
        /// </summary>
        private readonly Dictionary<GameObject, ObjectPool> instancePoolMap = new Dictionary<GameObject,ObjectPool>();
        private readonly HashSet<GameObject> releasedPoolMap = new HashSet<GameObject>();
        /// <summary>
        /// 跟踪编辑器模式下资源加载堆栈
        /// 保存最近五次该对象的实例化堆栈
        /// </summary>
#if UNITY_EDITOR
        private readonly Dictionary<GameObject,List<string>> trackInstanceStack = new Dictionary<GameObject,List<string>>();
        private readonly Dictionary<GameObject,List<string>> trackReleaseStack = new Dictionary<GameObject,List<string>>();
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ObjectPool GetPool(string path,GameObject prefab)
        {
            if (!poolContainer.TryGetValue(path, out var pool))
            {
                pool = new ObjectPool()
                {
                    prefab = prefab,
                    assetPath = path,
                    capacity = GetPoolSize(path),
                    owner = this,
                };
                
                poolContainer.Add(path,pool);
            }

            return pool;
        }

        public async UniTask PreLoadAssets(List<string> assetsPath)
        {
            var tasks = ListPool<UniTask>.Get();
            foreach (var item in assetsPath)
            {
                tasks.Add(LoadAssets(item));
            }

            await UniTask.WhenAll(tasks);
            ListPool<UniTask>.Release(tasks);
        }

        public T GetCacheAsset<T>(string path) where T : Object
        {
            return GetAsset<T>(path);
        }

        public override GameObject InstantiateObj(string assetPath, Vector3 position, Quaternion rotation,Transform parent)
        {
            ObjectPool pool;
            if (syncAssetsContainer.TryGetValue(assetPath, out var prefab))
            {
                pool = GetPool(assetPath, (GameObject)prefab);
                var result = pool.Acquire(position,rotation,parent);
                if (!result)
                {
                    return null;
                }
                
                var component = result.GetComponent<BaseAssetEntity>();
                SetAssetEntity(component, assetPath);
#if UNITY_EDITOR
                TrackAllocStack(result);
#endif
                return result;
            }

            if (assetsContainer.TryGetValue(assetPath, out prefab))
            {
                pool = GetPool(assetPath, (GameObject)prefab);
                var result = pool.Acquire(position, rotation,parent);
                if (!result)
                {
                    return null;
                }
#if UNITY_EDITOR
                TrackAllocStack(result);
#endif
                return result;
            }

            return null;
        }

        public override GameObject InstantiateObj(string assetPath, Transform parent, bool instantiateInWorldSpace = false)
        {
            ObjectPool pool;
            if (syncAssetsContainer.TryGetValue(assetPath, out var prefab))
            {
                pool = GetPool(assetPath, (GameObject)prefab);
                var result = pool.Acquire(parent, instantiateInWorldSpace);
                var component = result.GetComponent<BaseAssetEntity>();
                SetAssetEntity(component, assetPath);
#if UNITY_EDITOR
                TrackAllocStack(result);
#endif
                return result;
            }

            if (assetsContainer.TryGetValue(assetPath, out prefab))
            {
                pool = GetPool(assetPath, (GameObject)prefab);
                var result = pool.Acquire(parent, instantiateInWorldSpace);
#if UNITY_EDITOR
                TrackAllocStack(result);
#endif
                return result;
            }

            return null;
        }

        public override void ReleaseObj(GameObject obj)
        {
            if (releasedPoolMap.Contains(obj))
            {
#if UNITY_EDITOR
                Debug.LogError($"Invalid pool item {obj.name}");
                DumpAllocStack(obj);
#endif
                return;
            }
            
            BaseAssetEntity behavior = null;
            if (pendingAssetsTargets.Count > 0 && (behavior = obj.GetComponent<BaseAssetEntity>()))
            {
                pendingAssetsTargets.Remove(behavior);
            }
            
#if UNITY_EDITOR
            TrackFreeStack(obj);
#endif

            if (instancePoolMap.TryGetValue(obj,out var pool))
            {
                pool.Recycle(obj);
                releasedPoolMap.Add(obj);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Invalid pool item {obj.name}");
#endif
                Destroy(obj); 
            }
        }

#if UNITY_EDITOR
        [Conditional("DH_TRACE_DEBUG")]
        private void DumpAllocStack(GameObject obj)
        {
            if (trackInstanceStack.TryGetValue(obj, out var list))
            {
                var stringBuilder = new StringBuilder();
                foreach (var item in list)
                {
                    stringBuilder.AppendLine(item);
                }
                Debug.Log(stringBuilder.ToString());
            }

            if (trackReleaseStack.TryGetValue(obj, out list))
            {
                var stringBuilder = new StringBuilder();
                foreach (var item in list)
                {
                    stringBuilder.AppendLine(item);
                }
                Debug.Log(stringBuilder.ToString());
            }
        }

        [Conditional("DH_TRACE_DEBUG")]
        private void TrackAllocStack(GameObject obj)
        {
            if (trackInstanceStack.TryGetValue(obj, out var list))
            {
                if (list.Count > 10)
                {
                    list.RemoveAt(0);
                }
            }
            else
            {
                trackInstanceStack.Add(obj,list = new List<string>());
            }
            list.Add(UnityEngine.StackTraceUtility.ExtractStackTrace());
        }
        [Conditional("DH_TRACE_DEBUG")]
        private void TrackFreeStack(GameObject obj)
        {
            if (trackReleaseStack.TryGetValue(obj, out var list))
            {
                if (list.Count > 10)
                {
                    list.RemoveAt(0);
                }
            }
            else
            {
                trackReleaseStack.Add(obj,list = new List<string>());
            }
            list.Add(UnityEngine.StackTraceUtility.ExtractStackTrace());
        }
#endif
    }
}