using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DH.UIFramework
{
    public class UIUtils
    {
        private static Vector3 outRangePos = new Vector3(9999, 9999, 0);

        public static void SetActive(GameObject obj, bool isActive)
        {
            if (!isActive && obj)
            {
                SetActive(obj.transform, false);
            }
        }
        
        public static void SetActive(Transform objTrans, bool isActive)
        {
            if (!isActive)
            {
                if (objTrans)
                {
                    objTrans.localPosition = outRangePos;
                }
            }
        }
    }

    public class PoolRecyclable : MonoBehaviour
    {
        private Dictionary<int, Stack<GameObject>> poolsObjDic = new Dictionary<int, Stack<GameObject>>();
        private Dictionary<int, int> activeObjPoolDic = new Dictionary<int, int>();

        //取出 cell
        protected virtual GameObject GetPoolsObj(GameObject prefab, Transform parent)
        {
            if (!prefab)
            {
                return null;
            }
            
            int instanceId = prefab.GetInstanceID();

            if (!poolsObjDic.TryGetValue(instanceId, out var poolsObj))
            {
                poolsObj = new Stack<GameObject>();
                poolsObjDic.Add(instanceId, poolsObj);
            }
            
            GameObject cell = null;
            if (poolsObj.Count > 0)
            {
                cell = poolsObj.Pop();
                UIUtils.SetActive(cell, true);
            }

            if (!cell)
            {
                cell = Instantiate(prefab, parent) as GameObject;
                cell.transform.localScale = Vector3.one;
                cell.transform.localPosition = Vector3.zero;
                OnPrefabInstantiate(cell);
            }

            int cellInstanceId = cell.GetInstanceID();

            if (!activeObjPoolDic.ContainsKey(cellInstanceId))
            {
                activeObjPoolDic.Add(cellInstanceId, instanceId);
            }
            
            return cell;
        }

        protected virtual void OnPrefabInstantiate(GameObject cell)
        {
        }
        
        protected virtual bool HasObjInPool(GameObject prefab)
        {
            int instanceId = prefab.GetInstanceID();

            if (!poolsObjDic.TryGetValue(instanceId, out var poolsObj))
            {
                poolsObj = new Stack<GameObject>();
                poolsObjDic.Add(instanceId, poolsObj);
            }

            return poolsObj.Count > 0;
        }

        protected virtual bool HasItemsInPoolByInstance(GameObject cell)
        {
            if (cell)
            {
                int cellInstanceId = cell.GetInstanceID();
                
                if (activeObjPoolDic.TryGetValue(cellInstanceId, out var poolInstanceId))
                {
                    poolsObjDic.TryGetValue(poolInstanceId, out var poolsObj);

                    return poolsObj != null && poolsObj.Count > 0;
                }
            }

            return false;
        }

        /// <summary>
        /// 回收Item
        /// </summary>
        /// <param name="cell"></param>
        protected virtual void SetPoolsObj(GameObject cell)
        {
            if (cell)
            {
                int cellInstanceId = cell.GetInstanceID();
                
                if (activeObjPoolDic.TryGetValue(cellInstanceId, out var poolInstanceId))
                {
                    if (!poolsObjDic.TryGetValue(poolInstanceId, out var poolsObj))
                    {
                        poolsObj = new Stack<GameObject>();
                        poolsObjDic.Add(poolInstanceId, poolsObj);
                    }
                    
                    poolsObj.Push(cell);
                    UIUtils.SetActive(cell, false);
                    
                    activeObjPoolDic.Remove(cellInstanceId);
                }
            }
        }
    }
}