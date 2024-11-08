using System.Collections.Generic;
using UnityEngine.Pool;

namespace DH.Game
{
    public interface IGamePlayElement
    {
        bool Recycled { get; }
        void OnUpdate(float elapseSeconds);
    }

    public class GamePlayManager : GameModule
    {
        private readonly HashSet<IGamePlayElement> elements = new();
        private readonly HashSet<IGamePlayElement> pendingList = new();
        private readonly HashSet<IGamePlayElement> pendingAddList = new();

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (pendingList.Count != 0)
            {
                foreach (var item in pendingList) elements.Remove(item);
                pendingList.Clear();
            }

            // 同一帧同时添加和删除一个元素，将以添加行为优先
            if (pendingAddList.Count != 0)
            {
                foreach (var item in pendingAddList) elements.Add(item);
                pendingAddList.Clear();
            }

            foreach (var item in elements)
            {
                if (item.Recycled)
                {
                    pendingList.Add(item);
                    continue;
                }

                item.OnUpdate(elapseSeconds);
            }
        }

        public void AddElement(IGamePlayElement element)
        {
            pendingAddList.Add(element);
        }

        public void RemoveElement(IGamePlayElement element)
        {
            pendingList.Remove(element);
        }
        /// <summary>
        /// 清除所有子弹
        /// </summary>
        public void ClearAllBullets()
        {
            var tmpList = ListPool<IGamePlayElement>.Get();
            foreach (var element in elements)
            {
                if(element is BaseBullet bullet)
                {
                    tmpList.Add(bullet);
                }
                else if (element is PlayerPoisonCircle poisonCircle)
                {
                    tmpList.Add(poisonCircle);
                }
                else if (element is PlayerSandCircle superBullet)
                {
                    tmpList.Add(superBullet);
                }
            }
            foreach (var element in pendingAddList)
            {
                if(element is BaseBullet bullet)
                {
                    tmpList.Add(bullet);
                }
                else if (element is PlayerPoisonCircle poisonCircle)
                {
                    tmpList.Add(poisonCircle);
                }
                else if (element is PlayerSandCircle superBullet)
                {
                    tmpList.Add(superBullet);
                }
            }

            foreach (var element in tmpList)
            {
                pendingAddList.Remove(element);
                (element as BaseBullet)?.ForceDestroy();
                (element as PlayerPoisonCircle)?.ForceDestroy();
                (element as PlayerSandCircle)?.ForceDestroy();
                
            }
            ListPool<IGamePlayElement>.Release(tmpList);
        }
        public override void Shutdown()
        {
            elements.Clear();
        }
    }
}