using System.Collections.Generic;

namespace DH.Data
{
    //背包存档数据
    public class MapBagArchiveData
    {
        public int bagTitaniumCount;//获取钛金矿
        public int bagActionBallCount;//获取能量球
        public int bagUraniumCount;//获取铀矿
        public int bagMoneyCount;
        public Dictionary<int, long> items;
    }
    //玩家状态存档数据
    public class PlayerStatsArchiveData
    {
        public int KillCount;//击杀数
        public int TileCount;//摧毁个数
        public float MoveDistance;// 移动距离
        public int ResCount;//开采资源
        public int TreasureCount;// 宝藏
        public int KillBossCount;// 击杀boss的个数
        public Dictionary<long, long> SkillHurtsDic;// 技能伤害统计 key 技能ID value 技能造成的伤害值
        public Dictionary<int, long> Items;//挖到的宝藏
        
        public long killExp;
        public long maxHp;
        public long hp;
        public List<int> unlockWeaponList;//解锁武器数据
    }
}