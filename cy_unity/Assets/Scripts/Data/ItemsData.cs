using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using DH.Config;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;

namespace DH.Data
{
    [ProtoWrap(typeof(ItemBag))]
    public partial class ItemsData : BaseData
    {
        [AutoNotify] private readonly ObservableDictionary<int, ResourceData> resourceDatas = new();
        [AutoNotify] private ObservableList<ResourceData> materialList = new();
        public delegate void ItemUpdateDelegate(ResourceData data);
        public event ItemUpdateDelegate OnItemUpdate;
        protected override void InitData()
        {
            var items = ConfigCenter.ItemCfgColl.DataItems;
            foreach (var item in items)
            {
                var resource = new ResourceData
                {
                    Type = (int)RewardType.Item,
                    Count =0,
                    Id = item.Id
                };
                resource.PropertyChanged += ItemChanged;
                ResourceDatas.Add(item.Id, resource);
                CheckAddMaterial(resource);
            
            }
            foreach (var item in itemsAll)
            {
                if (ResourceDatas.TryGetValue(item.Key, out var value))
                {
                    value.Count = item.Value;
                }
            }

            itemsAll.CollectionChanged += ItemsAllOnCollectionChanged;
        }

        private void ItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if(sender is ResourceData data)
             OnItemUpdate?.Invoke(data);
        }

        protected override void ClearData()
        {
            ItemsAll.CollectionChanged -= ItemsAllOnCollectionChanged;
            ItemsAll.Clear();
            ResourceDatas.Clear();
            MaterialList.Clear();
            base.ClearData();
        }

        private void ItemsAllOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            var newItems = eventArgs.NewItems;
            var oldItems = eventArgs.OldItems;
            switch (eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var listItem in newItems)
                    {
                        var item = (KeyValuePair<int, long>)listItem;
                        var resource = new ResourceData
                        {
                            Type =(int) RewardType.Item,
                            Count = item.Value,
                            Id = item.Key
                        };
                        resourceDatas.Add(item.Key, resource);

                        CheckAddMaterial(resource);
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var listItem in oldItems)
                    {
                        var item = (KeyValuePair<int, long>)listItem;
                        resourceDatas.Remove(item.Key);
                    }

                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (var index = 0; index < oldItems.Count; index++)
                    {
                        var item = (KeyValuePair<int, long>)oldItems[index];
                        var newItem = (KeyValuePair<int, long>)newItems[index];
                        if (!resourceDatas.TryGetValue(item.Key, out var resource)) continue;

                        resource.Id = newItem.Key;
                        resource.Count = newItem.Value;
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    resourceDatas.Clear();
                    break;
            }
        }

        public void CheckAddMaterial(ResourceData data)
        {
            var itemCfg = ConfigCenter.ItemCfgColl.GetDataById(data.Id);
            if (itemCfg.Type == (int)GameConst.ItemType.Paper ||
                itemCfg.Type == (int)GameConst.ItemType.Stone)
            {
                materialList.Add(data);
            }
        }

        /// <summary>
        ///  获取指定的 item数量；
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public long GetItemCountById(int itemId)
        {
            return GetResourceDataById(itemId)?.Count ?? 0;
        }

        /// <summary>
        /// 设置 / 获取 金币
        /// </summary>
        public long Money
        {
            get
            {
                return resourceDatas.TryGetValue((int)GameConst.ItemIdCode.Money, out var ret)?ret.Count : 0;
            }
        }

        /// <summary>
        /// 设置 / 获取 砖石
        /// </summary>
        public long Stone
        {
            get
            {
                return resourceDatas.TryGetValue(2, out var ret)? ret.Count: 0;
            }
        }

        /// <summary>
        /// 设置 / 获取 砖石
        /// </summary>
        public long GameCoin
        {
            get => resourceDatas.TryGetValue((int)GameConst.ItemIdCode.GameCoin, out var ret) ? ret.Count : 0;
            set
            {
                if (ResourceDatas.TryGetValue((int)GameConst.ItemIdCode.GameCoin, out var item))
                {
                    item.Count = value;
                }
            }
        }

        private long stone = 0;


        public void ChangeItemsCount(int id, long count, bool isAdd)
        {
            if (isAdd)
            {
                if (ResourceDatas.TryGetValue(id, out var item))
                {
                    item.Count += count;
                }
            }
            else
            {
                if (ResourceDatas.TryGetValue(id, out var item))
                {
                    item.Count -= count;
                }
            }
        }
        public bool CheckItemIsEnough(Reward reward,int num)
        {
            return reward!=null && CheckItemIsEnough(reward.Id,reward.Count*num);
        }
        
        public bool CheckItemIsEnough(Reward reward)
        {
            return reward!=null && CheckItemIsEnough(reward.Id,reward.Count);
        }

        public bool CheckItemIsEnough(int id, long count)
        {
            long itemCount = 0;
            if (ResourceDatas.TryGetValue(id, out var item))
            {
                itemCount = item.Count;
            }
            // DHLog.Debug("muzili log CheckItemIsEnough  id == " + id + "  need count is " + count +
            //           "   own count is " + itemCount);
            return itemCount >= count;
        }

        public long GetItemCount(int id)
        {
            long count = 0;
            if (ResourceDatas.TryGetValue(id, out var item))
            {
                count = item.Count;
            }
            return count;
        }

        // public List<ResourceData> Materials
        // {
        //     get{
        //     var list = new List<ResourceData>();
        //     foreach (var item in itemsAll)
        //     {
        //         var cfg = ConfigCenter.ItemCfgColl.GetDataById(item.Key);
        //         if (cfg.Type == (int)GameConst.ItemType.Paper ||
        //             cfg.Type == (int)GameConst.ItemType.Stone)
        //             list.Add(ResourceDatas[item.Key]);
        //     }
        //     return list;
        //     }
        // }

        public List<ResourceData> GetStones()
        {
            var list = new List<ResourceData>();
            foreach (var item in ResourceDatas)
            {
                var cfg = ConfigCenter.ItemCfgColl.GetDataById(item.Key);
                if (cfg.Type == (int)GameConst.ItemType.Stone)
                    list.Add(ResourceDatas[item.Key]);
            }
            return list; 
        }

        public string GetItemNameById(int id)
        {
            var cfg = ConfigCenter.ItemLanguageCfgColl.GetDataById(id);
            if (cfg == null)
            {
                DHLog.Warning($"请检查 多语言表 itemlanuage 没有配置 {id}");
                return "";
            }
            return cfg.Name;
        }

        public string GetItemIconPathById(int id)
        {
            var cfg = ConfigCenter.ItemCfgColl.GetDataById(id);
            if (cfg == null) return $"icon[dj_4]";
            return cfg.Icon;
        }

        public int GetItemQuality(int id)
        {
            var cfg = ConfigCenter.ItemCfgColl.GetDataById(id);
            if (cfg != null)
            {
                return cfg.Quality;
            }

            return 0;
        }

        public string GetItemBgPathById(int id)
        {
            var cfg = ConfigCenter.ItemCfgColl.GetDataById(id);
            if (cfg != null)
            {
                var quaCfg = ConfigCenter.QuaCfgColl.GetDataById(cfg.Quality);
                if (quaCfg != null)
                {
                    return quaCfg.ItemBg;
                }
            }
            return $"common[commom_equipbg_1]";
        }
        
        public ResourceData GetResourceDataById(int id)
        {
            if (ResourceDatas.ContainsKey(id))
            {
                return ResourceDatas[id];
            }

            return null;
        }
    }
}