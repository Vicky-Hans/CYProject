using System.Collections.Generic;
using DH.Config;
using DH.Data;
using DH.Game;
using DHFramework;
using UnityEngine;
using UnityEngine.Pool;

namespace DH.Game
{
    public class EnemyManager
    {
        private List<MonsterController> enemyList = new();
        private List<MonsterController> pendingList = new();
        private List<MonsterController> removeList = new();

        private List<float> sqrDistanceTempList = new List<float>();
        
        public EnemyManager()
        {
        }

        public List<MonsterController> EnemyList => enemyList;
        
        private float GetCapsuleDis(Vector3 pos, CapsuleCollider2D capsule)
        {
            return Lodash.DistanceToCapsule(pos, capsule);
        }

        public void Update(float deltaTime)
        {
            // remove
            foreach (var unitObj in removeList)
            {
                enemyList.Remove(unitObj);
            }
            removeList.Clear();
            
            foreach (var unitObj in enemyList)
            {
                if (unitObj.MonsterData.IsDead())
                {
                    removeList.Add(unitObj);
                    continue;
                }
                unitObj.OnUpdate(deltaTime);
            }
            
            foreach (var unitObj in pendingList) enemyList.Add(unitObj);
            pendingList.Clear();
        }

        public void Add(MonsterController obj)
        {
            pendingList.Add(obj);
        }

        public void Remove(MonsterController obj)
        {
            removeList.Add(obj);
        }

        public MonsterController GetNearest(Vector3 pos)
        {
            var list = ListPool<MonsterController>.Get();
            NearestMonster(list, pos, 1);
            if (list.Count <= 0)
            {
                ListPool<MonsterController>.Release(list);
                return null;
            }
            var obj = list[0];
            ListPool<MonsterController>.Release(list);
            return obj;
        }
        public void NearestMonster(List<MonsterController> monster, Vector3 pos, long count, float maxDistance = 12.5f)
        {
            if (enemyList.Count == 0)
            {
                return;
            }
            
            sqrDistanceTempList.Clear();
            var maxDisSqr = maxDistance * maxDistance;
            for (int i = 0; i < enemyList.Count; i++)
            {
                var obj = enemyList[i];
                if (!obj.ValidAttackTarget())
                {
                    continue;
                }

                if (!BattleManager.Instance.fightingManagerIns.TargetInScreen(obj))
                {
                    continue;
                }
                
                var dis = (obj.transform.position - pos).sqrMagnitude;
                if (dis > maxDisSqr)
                {
                    continue;
                }
                
                var currentCount = monster.Count;
                int selectedIndex = 0;
                // 根据距离降序排列
                for (int index = 0; index < currentCount; index++)
                {
                    if (sqrDistanceTempList[index] > dis)
                    {
                        // 小于当前对象的距离，插入到当前对象后一个位置
                        selectedIndex = index + 1;
                    }
                    else
                    {
                        // 大于当前对象的距离，插入到当前对象前一个位置
                        selectedIndex = index;
                        break;
                    }
                }
                
                // 大于目标个数，移除最远的对象
                if (currentCount >= count)
                {
                    // 目标个数最大，同时距离大于第一个对象，忽略添加
                    if (selectedIndex == 0)
                    {
                        continue;
                    }
                    
                    monster.RemoveAt(0);
                    sqrDistanceTempList.RemoveAt(0);
                    selectedIndex--;
                }

                monster.Insert(selectedIndex,obj);
                sqrDistanceTempList.Insert(selectedIndex,dis);
            }
        }
        public void NearestMonsterInRect(List<MonsterController> monster, Vector3 pos, long count, float maxDistance = 12.5f)
        {
            if (enemyList.Count == 0)
            {
                return;
            }
            
            sqrDistanceTempList.Clear();
            for (int i = 0; i < enemyList.Count; i++)
            {
                var obj = enemyList[i];
                if (!obj.ValidAttackTarget())
                {
                    continue;
                }

                if (!BattleManager.Instance.fightingManagerIns.TargetInScreen(obj))
                {
                    continue;
                }

                var dis = Mathf.Abs((obj.transform.position - pos).y);
                if (dis > maxDistance)
                {
                    continue;
                }
                
                var currentCount = monster.Count;
                int selectedIndex = 0;
                // 根据距离降序排列
                for (int index = 0; index < currentCount; index++)
                {
                    if (sqrDistanceTempList[index] > dis)
                    {
                        // 小于当前对象的距离，插入到当前对象后一个位置
                        selectedIndex = index + 1;
                    }
                    else
                    {
                        // 大于当前对象的距离，插入到当前对象前一个位置
                        selectedIndex = index;
                        break;
                    }
                }
                
                // 大于目标个数，移除最远的对象
                if (currentCount >= count)
                {
                    // 目标个数最大，同时距离大于第一个对象，忽略添加
                    if (selectedIndex == 0)
                    {
                        continue;
                    }
                    
                    monster.RemoveAt(0);
                    sqrDistanceTempList.RemoveAt(0);
                    selectedIndex--;
                }

                monster.Insert(selectedIndex,obj);
                sqrDistanceTempList.Insert(selectedIndex,dis);
            }
        }
        
        public MonsterController FarestMonsterInScreen()
        {
            if(enemyList.Count == 0)return null;
            sqrDistanceTempList.Clear();
            var maxDisSqr = 0f;
            int currentIdx = -1;
            for (int i = 0; i < enemyList.Count; i++)
            {
                var obj = enemyList[i];
                if (!obj.ValidAttackTarget())
                {
                    continue;
                }

                if (!BattleManager.Instance.fightingManagerIns.TargetInScreen(obj))
                {
                    continue;
                }
                var dis = obj.transform.position.sqrMagnitude;
                if (dis > maxDisSqr)
                {
                    maxDisSqr = dis;
                    currentIdx = i;
                }
            }
            return currentIdx == -1 ? null : enemyList[currentIdx];
        }

        public MonsterController YFarestMonsterInScreen()
        {
            if(enemyList.Count == 0)return null;
            sqrDistanceTempList.Clear();
            var maxDisSqr = 0f;
            int currentIdx = -1;
            for (int i = 0; i < enemyList.Count; i++)
            {
                var obj = enemyList[i];
                if (!obj.ValidAttackTarget())
                {
                    continue;
                }

                if (!BattleManager.Instance.fightingManagerIns.TargetInScreen(obj))
                {
                    continue;
                }
                var dis = Mathf.Abs(obj.transform.position.y);
                if (dis > maxDisSqr)
                {
                    maxDisSqr = dis;
                    currentIdx = i;
                }
            }
            return currentIdx == -1 ? null : enemyList[currentIdx];
        }

        public MonsterController RandMonsterInRect(Vector3 pos, float range)
        {
            if(enemyList.Count == 0)return null;
            var list = ListPool<MonsterController>.Get();
            for (int i = 0; i < enemyList.Count; i++)
            {
                var obj = enemyList[i];
                if (!obj.ValidAttackTarget())
                {
                    continue;
                }

                if (!BattleManager.Instance.fightingManagerIns.TargetInScreen(obj))
                {
                    continue;
                }
                var mPos = obj.transform.position;
                if (Mathf.Abs(mPos.y-pos.y) <= range)
                {
                    list.Add(obj);
                }
            }
            
            MonsterController mObj = null;
            if (list.Count > 0)
            {
                mObj = list.RandomOneInList();
            }
            ListPool<MonsterController>.Release(list);
            return mObj;
        }

        public void RandMonstersInRect(List<MonsterController> monster, Vector3 pos, long count,
            float maxDistance = 12.5f)
        {
            if (enemyList.Count == 0)
            {
                return;
            }
            var list = ListPool<MonsterController>.Get();
            var maxDisSqr = maxDistance * maxDistance;
            for (int i = 0; i < enemyList.Count; i++)
            {
                var obj = enemyList[i];
                if (!obj.ValidAttackTarget())
                {
                    continue;
                }
                if (!BattleManager.Instance.fightingManagerIns.TargetInScreen(obj))
                {
                    continue;
                }
                var dis = (obj.transform.position - pos).sqrMagnitude;
                if (dis > maxDisSqr)
                {
                    continue;
                }
                list.Add(obj);
            }
            for (int i = 0; i < count && list.Count > 0; i++)
            {
                var tmpObj = list.RandomOneInList();
                monster.Add(tmpObj);
                list.Remove(tmpObj);
            }
            ListPool<MonsterController>.Release(list);
        }
        public void GetMonstersInRect(List<MonsterController> monster, Vector3 pos, long count = 0,float maxDistance = 12.5f)
        {
            if (enemyList.Count == 0) return;
            var list = ListPool<MonsterController>.Get();
            var maxDisSqr = maxDistance * maxDistance;
            for (var i = 0; i < enemyList.Count; i++)
            {
                var obj = enemyList[i];
                if (!obj.ValidAttackTarget()) continue;
                if (!BattleManager.Instance.fightingManagerIns.TargetInScreen(obj)) continue;
                var dis = (obj.transform.position - pos).sqrMagnitude;
                if (dis > maxDisSqr || (Mathf.Abs(obj.transform.position.x-pos.x) <= Mathf.Epsilon&& Mathf.Abs(obj.transform.position.y-pos.y) <= Mathf.Epsilon)) continue;
                list.Add(obj);
            }
            if (list.Count == 0) return;
            if (count == 0) count = list.Count;
            for (var i = 0; i < count; i++)
            {
                var tmpObj = list.RandomOneInList();
                monster.Add(tmpObj);
                list.Remove(tmpObj);
            }
            ListPool<MonsterController>.Release(list);
        }

        /// <summary>
        /// 上半区或者下半区随机一个怪物
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public MonsterController RandMonsterInSemiScreen(Vector3 pos)
        {
            if(enemyList.Count == 0)return null;
            var list = ListPool<MonsterController>.Get();
            for (int i = 0; i < enemyList.Count; i++)
            {
                var obj = enemyList[i];
                if (!obj.ValidAttackTarget())
                {
                    continue;
                }

                if (!BattleManager.Instance.fightingManagerIns.TargetInScreen(obj))
                {
                    continue;
                }
                var mPos = obj.transform.position;
                if (pos.y * mPos.y > 0)
                {
                    list.Add(obj);
                }
            }
            MonsterController mObj = null;
            if (list.Count > 0)
            {
                mObj = list.RandomOneInList();
            }
            ListPool<MonsterController>.Release(list);
            return mObj;
        }

        /// <summary>
        /// screen范围内随机一个目标
        /// </summary>
        /// <returns></returns>
        public MonsterController RandMonsterInScreen()
        {
            if(enemyList.Count == 0)return null;
            var list = ListPool<MonsterController>.Get();
            for (int i = 0; i < enemyList.Count; i++)
            {
                var obj = enemyList[i];
                if (!obj.ValidAttackTarget())
                {
                    continue;
                }

                if (!BattleManager.Instance.fightingManagerIns.TargetInScreen(obj))
                {
                    continue;
                }
                list.Add(obj);
            }
            MonsterController mObj = null;
            if (list.Count > 0)
            {
                mObj = list.RandomOneInList();
            }
            ListPool<MonsterController>.Release(list);
            return mObj;
        }

        /// <summary>
        /// 获取所有存活怪
        /// </summary>
        /// <param name="monsters"></param>
        /// <param name="selectBoss">是否选择 boss</param>
        public void GetAllMonsters(List<MonsterController> monsters, bool selectBoss = false)
        {
            if (enemyList.Count == 0) return;
            foreach (var obj in enemyList)
            {
                if (!obj.ValidAttackTarget()) continue;
                if(obj.MonsterData.cfg.MonsterType == (int)MonsterType.Boss && !selectBoss)
                {
                    continue;
                }
                monsters.Add(obj);
            }
        }

        /// <summary>
        /// 随机count个目标
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="count"></param>
        /// <param name="selectBoss">是否能选择boss</param>
        public void GetRandMonstersInScreen(List<MonsterController> monster, long count = 0, bool selectBoss = true)
        {
            if (enemyList.Count == 0) return;
            var list = ListPool<MonsterController>.Get();
            for (var i = 0; i < enemyList.Count; i++)
            {
                var obj = enemyList[i];
                if (!obj.ValidAttackTarget()) continue;
                if (!selectBoss)
                {
                    if(obj.MonsterData.cfg.MonsterType == (int)MonsterType.Boss)continue;
                }
                if (!BattleManager.Instance.fightingManagerIns.TargetInScreen(obj)) continue;
                list.Add(obj);
            }
            if (list.Count == 0) return;
            if (count == 0) count = list.Count;
            for (var i = 0; i < count && list.Count > 0; i++)
            {
                var tmpObj = list.RandomOneInList();
                monster.Add(tmpObj);
                list.Remove(tmpObj);
            }
            ListPool<MonsterController>.Release(list);
        }

        /// <summary>
        /// 在range范围内随机一个目标
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public MonsterController RandMonsterInRange(Vector3 pos, float range)
        {
            if(enemyList.Count == 0)return null;
            var list = ListPool<MonsterController>.Get();
            for (int i = 0; i < enemyList.Count; i++)
            {
                var obj = enemyList[i];
                if (!obj.ValidAttackTarget())
                {
                    continue;
                }

                if (!BattleManager.Instance.fightingManagerIns.TargetInScreen(obj))
                {
                    continue;
                }
                var mPos = obj.transform.position;
                if ((mPos - pos).sqrMagnitude < range * range)
                {
                    list.Add(obj);
                }
            }
            MonsterController mObj = null;
            if (list.Count > 0)
            {
                mObj = list.RandomOneInList();
            }
            ListPool<MonsterController>.Release(list);
            return mObj;
        }

        public void RandMonstersInRange(Vector3 pos, float range,List<MonsterController> monster)
        {
            if(enemyList.Count == 0)return;
            foreach (var obj in enemyList)
            {
                if (!obj.ValidAttackTarget()) continue;
                if (!BattleManager.Instance.fightingManagerIns.TargetInScreen(obj)) continue;
                var mPos = obj.transform.position;
                if ((mPos - pos).sqrMagnitude < range * range) monster.Add(obj);
            }
        }
        // 获取屏幕内所有怪物
        public void GetAllMonstersInScreen(List<MonsterController> monster, bool selectBoss = true)
        {
            if (enemyList.Count == 0) return;
            for (var i = 0; i < enemyList.Count; i++)
            {
                var obj = enemyList[i];
                if (!obj.ValidAttackTarget()) continue;
                if (!selectBoss)
                {
                    if(obj.MonsterData.cfg.MonsterType == (int)MonsterType.Boss)continue;
                }
                if (!BattleManager.Instance.fightingManagerIns.TargetInScreen(obj)) continue;
                monster.Add(obj);
            }
        }
       
    }
}