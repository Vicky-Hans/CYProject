using System;
using System.Collections.Generic;
using DHFramework;
using UnityEngine;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace DH.Game
{
    public class LifeTimeManager : GameModule
    {
        public class Unit : IReference
        {
            public GameObject target;
            public float lifeTime;
            public float releaseTime;
            public IPool<GameObject> pool;
            public Action completed;

            public void Destroy()
            {
                if (!target)
                {
                    return;
                }
                
                if (pool != null)
                {
                    pool.ReleaseObj(target);
                }
                else
                {
                    Object.Destroy(target);
                }
                completed?.Invoke();
                target = null;
            }

            public void Clear()
            {
                // 防止在对象已经释放到对象池的情况下重复释放
                if (target && target.activeSelf)
                {
                    Destroy();
                }
                
                target = null;
                pool = null;
                completed = null;
            }
        }

        private Dictionary<GameObject, Unit> units = new Dictionary<GameObject, Unit>();
        private List<GameObject> pendingList = new List<GameObject>();

        [Preserve]
        public LifeTimeManager()
        {
            
        }

        public void RemoveUnit(GameObject target)
        {
            if (units.TryGetValue(target, out var unit))
            {
                unit.target = null;
                unit.lifeTime = 0;
                unit.releaseTime = 0;
                unit.pool = null;
                unit.completed = null;
                ReferencePool.Release(unit);
                units.Remove(target);
            }
        }
        
        public void AddAutoReleaseUnit(GameObject target, float lifeTime, IPool<GameObject> pool,Action completed = null)
        {
            if(target == null)return;
            if (units.TryGetValue(target, out var unit))
            {
                unit.target = target;
                unit.lifeTime = lifeTime;
                unit.releaseTime = Time.time + lifeTime;
                unit.completed = completed;
            }
            else
            {
                unit = ReferencePool.Acquire<Unit>();
                unit.target = target;
                unit.lifeTime = lifeTime;
                unit.releaseTime = Time.time + lifeTime;
                unit.pool = pool;
                unit.completed = completed;
                units.Add(target,unit);
            }
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            var current = Time.time;
            foreach (var pair in units)
            {
                if (pair.Value.releaseTime > current)
                {
                    continue;
                }
                
                pendingList.Add(pair.Key);
                ReferencePool.Release(pair.Value);
            }

            if (pendingList.Count == 0)
            {
                return;
            }

            foreach (var item in pendingList)
            {
                units.Remove(item);
            }
            pendingList.Clear();
        }

        public override void Shutdown()
        {
            foreach (var pair in units)
            {
                ReferencePool.Release(pair.Value);
            }
            units.Clear();
        }
    }
}