using System.Collections.Generic;
using System.Linq;
using DH.Config;
using DH.Data;

namespace DH.Game
{
    public partial class ItemManager : ObservableSingleton<ItemManager>
    {
        
        public List<Reward> GetItemJackpotList(Reward reward)
        {
            List<Reward> list = new();
            if (reward.Type == RewardType.Item)
            {
                var cfg = ConfigCenter.ItemCfgColl.GetDataById(reward.Id);
                if (cfg.Type==(int)GameConst.ItemType.EquipFragmentRandom && cfg is { JackpotId: { Count: > 0 } })
                {
                    var jockCfg = ConfigCenter.JackpotCfgColl.GetDataById(cfg.JackpotId[0]);
                    if (jockCfg is { RandomReward: {Count: >0 } })
                    {
                        foreach (var item in jockCfg.RandomReward)
                        {
                            list.Add(UIHelper.RandomRewardToReward(item));
                        }
                    }
                }
            }
            return list;
        }
        
        public bool GetItemIsLock(int type,int id)
        {
            if (type == (int)RewardType.Item)
            {
                var cfg = ConfigCenter.ItemCfgColl.GetDataById(id);
                if (cfg != null)
                {
                    if (cfg.Type == (int)GameConst.ItemType.EquipFragment)
                    {
                        var equipCfg = ConfigCenter.EquipCfgColl.GetDataById(cfg.TypeValue);
                        if (equipCfg != null)
                        {
                            return !DataCenter.mainStageData.IsPassChapter(equipCfg.Unlock);
                        }
                    }
                }
            }
            return false;
        }
        
        public bool GetItemIsLock(ResourceData reward)
        {
            return GetItemIsLock(reward.Type,reward.Id);
        }
        
        public bool GetItemIsLock(Reward reward)
        {
            return GetItemIsLock((int)reward.Type,reward.Id);
        }

        public List<Reward> GetRewardsByType(int type,bool isOwn=false)
        {
            List<Reward> list = new List<Reward>();
            var itemList = ConfigCenter.ItemCfgColl.DataItems.ToList();
            foreach (var item in itemList)
            {
                if (item.Type != type) continue;
                var reward = new Reward(RewardType.Item, item.Id, 1);
                if (isOwn)
                {
                    if ( UIHelper.CheckRewardIsEnough(reward))
                    {
                        list.Add(reward);
                    }
                }else
                {
                    list.Add(reward);
                }
            }
            return list;
        }
    }
}
