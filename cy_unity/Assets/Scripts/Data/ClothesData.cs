using System.Collections.Generic;
using System.Linq;
using System.Text;
using DH.Config;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;

namespace DH.Data
{
    [ProtoWrap(typeof(HeroEquipBag))]
    public partial class ClothesData : BaseData
    {
        public string RedNew = "RedNew";
        public string RedNewEnd = "RedNewEnd";
        [AutoNotify] private ObservableList<int> clothesNewList = new();
        [AutoNotify] private ObservableList<int> clothesNewEndList = new();

        public override void Init()
        {
            clothesNewList = LoadListFromPlayerPrefs(RedNew);
            clothesNewEndList = LoadListFromPlayerPrefs(RedNewEnd);
        }

        //记录新获得红点
        public void AddNewClothes(long uid)
        {
            if (CheckClothesItemIsOwnRed(uid))
            {
                return;
            }

            var id = GetClothesRedId(uid);
            if (!ClothesNewList.Contains(id) && !ClothesNewEndList.Contains(id))
            {
                ClothesNewList.Add(id);
            }
        }

        public bool CheckClothesItemIsOwnRed(long uid)
        {
            var id = GetClothesRedId(uid);
            foreach (var item in Items)
            {
                if (id!=0 && uid != item.Key && id == GetClothesRedId(item.Value))
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckNewRed(long uid)
        {
            var id = GetClothesRedId(uid);
            return ClothesNewList.Contains(id) && !ClothesNewEndList.Contains(id);
        }
        
        public void DelNewRed(long uid)
        {
            var id = GetClothesRedId(uid);
            if (ClothesNewList.Contains(id))
            {
                ClothesNewList.Remove(id);
            }
            
            if (!clothesNewEndList.Contains(id))
            {
                clothesNewEndList.Add(id);
            }
        }
        public List<HeroEquipData> GetAllHeroEquipList()
        {
            return Items.Values.ToList();
        }

        public HeroEquipData GetHeroEquipDataByUid(long uid)
        {
            return Items[uid];
        }

        public void ChangedClothes(int pos,long uid,bool isUse = true,bool inherit=false)
        {
            if (isUse)
            {
                if (inherit && IsUseIngByPart(pos))
                {
                    var oldUid = Wear[pos];
                    var oldData = GetHeroEquipDataByUid(oldUid);
                    var data = GetHeroEquipDataByUid(uid);
                    if (oldData != null && data != null)
                    {
                        (oldData.Lv, data.Lv) = (data.Lv, oldData.Lv);
                    }
                    Wear[pos] = uid;
                }
                else
                {
                    Wear[pos] = uid;

                }

            }
            else
            {
                if (Wear.TryGetValue(pos, out var value))
                {
                    if (value == uid)
                    {
                        Wear.Remove(pos);
                    }
                }
            }
        }

        public int GetHeroEquipPos(long uid)
        {
            foreach (var item in Wear)
            {
                if (item.Value == uid)
                {
                    return item.Key;
                }
            }

            return -1;
        }

        public HeroEquipData GetHeroEquipDataByPart(int partId)
        {
            if (Wear.TryGetValue(partId, out var uid))
            {
                return Items[uid];
            }
            return null;
        }

        public bool IsUseIng(long uid)
        {
            foreach (var item in Wear)
            {
                if (item.Value == uid)
                {
                    return true;
                }
            }
            return false;
        }
        
        public bool IsUseIngByPart(int part)
        {
            return Wear.ContainsKey(part) && Wear[part]!=0;
        }
        
        
        public bool IsOwn(long uid)
        {
            return Items.ContainsKey(uid);
        }

        public List<HeroEquipData> GetUnWearList()
        {
            List<HeroEquipData> unWearList = new();
            foreach (var item in Items)
            {
                if (!IsUseIng(item.Key))
                {
                    unWearList.Add(item.Value);
                }
            }
            return unWearList;
        }

        public void AddHeroEquipList(List<HeroEquip> heroEquipDatas)
        {
            foreach (var item in heroEquipDatas)
            {
                Items[item.Uid] = new HeroEquipData(item);
                AddNewClothes(item.Uid);
            }
            RaisePropertyChanged(nameof(Items));
        }
        
        public void AddHeroEquip(HeroEquipData heroEquipData)
        {
            if (heroEquipData == null || heroEquipData.Uid == 0)
            {
                DHLog.Error("Add heroEquipData is Null");
                return;
            }
            Items[heroEquipData.Uid] = heroEquipData;
            AddNewClothes(heroEquipData.Uid);
            RaisePropertyChanged(nameof(Items));
        }

        public void ChangeLevel(long uid,int lv)
        {
            if (Items.TryGetValue(uid, out var data))
            {
                data.Lv = lv;
            }
            RaisePropertyChanged(nameof(Items));
        }
        
        public void ChangeQua(long uid,int qua)
        {
            if (Items.TryGetValue(uid, out var data))
            {
                data.QuaId = qua;
            }
            RaisePropertyChanged(nameof(Items));
        }

        public void UseClothes(int pos,long uid,bool inherit = false)
        {
            if (Wear.ContainsKey(pos))
            {
                Wear[pos] = uid;
            }
        }

        public bool IsNoneClothes(int pos)
        {
            return !Wear.ContainsKey(pos) || !IsOwn(Wear[pos]);
        }

        public void ResetLv(long uid)
        {
            ChangeLevel(uid, 1);
        }
        
        public void ResetQua(long uid,int baseQua)
        {
            if(baseQua<=0) return;
            ChangeQua(uid, baseQua);
        }

        public void RemoveHeroEquip(List<long> removeList,bool isRefresh = true)
        {
            for (int i = 0; i < removeList.Count; i++)
            {
                if (IsUseIng(removeList[i]))
                {
                    Wear.Remove(GetHeroEquipPos(removeList[i]));
                }
                Items.Remove(removeList[i]);
            }
            if(isRefresh)
                RaisePropertyChanged(nameof(Items));
        }
        
        #region 记录本地红点

        public void SaveClothesRedInfo()
        {
            SaveListToPlayerPrefs(RedNew,ClothesNewList);
            SaveListToPlayerPrefs(RedNewEnd,ClothesNewEndList);
        }

        public int GetClothesRedId(long uid)
        {
            var data = GetHeroEquipDataByUid(uid);
            return GetClothesRedId(data);
        }
        
        public int GetClothesRedId(HeroEquipData data)
        {
            if (data != null)
            {
                return data.Id * 100 + data.QuaId;
            }
            return 0;
        }

        private void SaveListToPlayerPrefs(string redType,ObservableList<int> list)
        {
            if(list == null) return;
            var buffer = new StringBuilder();
            bool isfirst = true;
            foreach (var item in list)
            {
                if (isfirst)
                {
                    buffer.Append(item);
                    isfirst = false;
                }
                else
                {
                    buffer.Append("/"+item);
                }
            }
            DHUnityUtil.PlayerPrefs.SetString($"Save_{DataCenter.charcaterData.Digest.RoleId}_Clothes{redType}",buffer.ToString());
        }

        private ObservableList<int> LoadListFromPlayerPrefs(string redType)
        {
            var info = DHUnityUtil.PlayerPrefs.GetString($"Save_{DataCenter.charcaterData.Digest.RoleId}_Clothes{redType}",string.Empty);
            var stringArray = info.Split("/");
            ObservableList<int> intList = new ObservableList<int>();
            foreach (string str in stringArray)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    int intValue;
                    if (int.TryParse(str, out intValue))
                    {
                        intList.Add(intValue);
                    }
                }
            }
            return intList;
        }
        
        #endregion
        
    }
    
    [ProtoWrap(typeof(HeroEquip))]
    public partial class HeroEquipData : BaseData
    {
   
    }
}
