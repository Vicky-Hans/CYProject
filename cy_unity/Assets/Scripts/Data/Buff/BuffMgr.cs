using System.Collections.Generic;
using DH.UIFramework.Observables;

namespace DH.Data
{
    public class BuffMgr : ObservableObject
    {
        private UnitBase owner;
        private readonly Dictionary<int,List<Buff>> buffMap = new Dictionary<int, List<Buff>>();

        public BuffMgr(UnitBase owner)
        {
            this.owner = owner;
        }

        public Buff FindBuffById(int id)
        {
            return (buffMap.TryGetValue(id,out var buffs) && buffs.Count > 0) ? buffs[0] : null;
        }

        public List<Buff> FindBuffsById(int id)
        {
            buffMap.TryGetValue(id, out var buffs);
            return buffs;
        }
        
        public int GetBuffCountById(int id)
        {
            return FindBuffsById(id)?.Count ?? 0;
        }

        public float GetBuffsValue(int id)
        {
            var v = 0f;
            var list = FindBuffsById(id);
            if (list != null)
            {
                // 考虑正负 ？
                foreach (var buff in list)
                {
                    v+=buff.value;
                }
            }
            return v;
        }

        public float GetBuffsMaxValue(int id)
        {
            var v = 0f;
            var list = FindBuffsById(id);
            if (list != null)
            {
                // 考虑正负 ？
                foreach (var buff in list)
                {
                    v = v > buff.value ? v : buff.value; // Mathf.Max(v, buff.value);
                }
            }
            return v;
        }

        public Buff AddBuff(Buff buff)
        {
            if (buff.multi)
            {
                AddBuffInternal(buff);
                return buff;
            }

            var tmpBuff = FindBuffById(buff.id);
            if (tmpBuff == null)
            {
                AddBuffInternal(buff);
                return buff;
            }

            if (buff.startTime + buff.duration > tmpBuff.startTime + tmpBuff.duration)
            {
                tmpBuff.startTime = buff.startTime;
                tmpBuff.duration = buff.duration;
            }

            if (buff.value > tmpBuff.value)
            {
                tmpBuff.value = buff.value;
            }

            return tmpBuff;
        }

        private void AddBuffInternal(Buff buff)
        {
            if (buffMap.TryGetValue(buff.id, out var buffs))
            {
                buffs.Add(buff);
            }
            else
            {
                buffMap.Add(buff.id, new List<Buff>() { buff });
            }
        }

        public void RemoveBuff(Buff buff)
        {
            if (buffMap.TryGetValue(buff.id, out var buffs))
            {
                buffs.Remove(buff);
            }
        }

        private readonly List<Buff> pendingList = new List<Buff>();
        public void CheckValid(float currentTime)
        {
            foreach (var pair in buffMap)
            {
                var buffs = pair.Value;
                foreach (var buff in buffs)
                {
                    if (buff.IsValid(currentTime))
                    {
                        continue;
                    }
                    
                    pendingList.Add(buff);
                }

                foreach (var item in pendingList)
                {
                    buffs.Remove(item);
                }
                pendingList.Clear();
            }
        }
    }
}