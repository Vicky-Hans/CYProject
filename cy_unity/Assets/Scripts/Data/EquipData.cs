using System.Collections.Generic;
using DH.Proto;
using DH.UIFramework.Observables;

namespace DH.Data
{
    [ProtoWrap(typeof(EquipBag))]
    public partial class EquipData : BaseData
    {
        protected override void InitData()
        {
            //Items.Clear();
            // items.Add();
        }
        
        public int GetBattleEquipCount()
        {
            int battleNum = 0;
            if (Formations.TryGetValue(CurrWearFormation, out var value))
            {
                foreach (var item in value.Data)
                {
                    if (item.Value > 0) battleNum++;
                }
            }
            return battleNum;
        }
        
        /// <summary>
        /// 获取当前阵容未上阵的装备
        /// </summary>
        /// <returns></returns>
        public List<int> GetOwnUnUseEquipList()
        {
            List<int> ownList = new();
            EquipWearFormation formation = null;
            if (Formations.TryGetValue(CurrWearFormation, out var value))
            {
                formation = value;
            }
            
            foreach (var item in Items)
            {
                if (item.Value.Lv > 0 && (formation==null || !formation.Data.ContainsKey(item.Value.Id) || (formation.Data.ContainsKey(item.Value.Id) && formation.Data[item.Value.Id]==0)))
                {
                    ownList.Add(item.Key);
                }
            }
            
            return ownList;
        }
        
        /// <summary>
        /// 获取当前阵容上阵的装备
        /// </summary>
        /// <returns></returns>
        public List<int> GetOwnUseEquipList()
        {
            List<int> useList = new();
            if (Formations.TryGetValue(CurrWearFormation, out var value))
            {
                foreach (var equipUse in value.Data)
                {
                    if (equipUse.Value != 0)
                    {
                        useList.Add(equipUse.Key);
                    }
                }
            }
            
            return useList;
        }

        public bool IsUseIng(int id)
        {
            if (id == 0) return false;
            if (Formations.TryGetValue(CurrWearFormation, out var value))
            {
                return value.Data.ContainsKey(id) && value.Data[id]>0;
            }

            return false;
        }

        public EquipItemData GetEquipItemData(int id)
        {
            if (Items.TryGetValue(id, out EquipItemData value))
            {
                return value;
            }
            return null;
        }
        
        /// <summary>
        /// 获取装备等级
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetEquipLevel(int id)
        {
            var data = GetEquipItemData(id);
            if(data == null) return 0;
            return data.Lv;
        }
        
        public bool IsOwn(int id)
        {
            return Items.ContainsKey(id);
        }
        public void AddEquipData(int id,bool isRefresh = true)
        {
            if(IsOwn(id)) return;
            Items.Add(id,new EquipItemData{Id = id,Lv = 1,WearId = 0});
            if(isRefresh) RaisePropertyChanged(nameof(Items));
        }
        
        public void ChangeEquipData(Equip data,bool isRefresh = true)
        {
            if (Items.TryGetValue(data.Id, out EquipItemData item))
            {
                item.MergeFrom(data);
                if(isRefresh)RaisePropertyChanged(nameof(Items));
            }
        }
        
        public void ChangeWearFormationData(EquipWearFormation data)
        {
            Formations[CurrWearFormation] = data;
        }

        // public void RemoveBattle(int oldId,bool isRefresh = true)
        // {
        //     if (Items.TryGetValue(oldId, out EquipItemData data))
        //     {
        //         data.WearId = 0;
        //         if(isRefresh)RaisePropertyChanged(nameof(Items));
        //     }
        // }

        protected override void ClearData()
        {
            MergeFrom(new EquipBag(),true);
        }
    }
    
    [ProtoWrap(typeof(Equip))]
    public partial class EquipItemData : BaseData
    {
        
    }
}
