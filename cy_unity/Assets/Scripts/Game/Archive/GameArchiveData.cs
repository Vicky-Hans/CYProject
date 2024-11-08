using DH.Data;
using System.Collections.Generic;
using Google.Protobuf;

namespace DH.Game
{
    //挑战模式商店随机存档数据
    public class ChallengeRandomArchiveData
    {
        public int Id;
        public int Price;//价格
        public bool IsBuy;//是否购买过
        public bool IsTreasure;//是否是宝藏
    }
    //游戏存档数据
    public class GameArchiveData
    {
        public int chapterID;//章节ID
        public string modelName;//模式名称（MiningProcedure/FightingProcedure）
        public float gTime;//时间戳
        public float coastTime;//已消耗时间
        public int gameExpLevel;//等级
        public int adRefreshCount;//已消耗时间
        public int refreshCount;//等级
        public float showTimeViewLeftTime;
        public FightArchiveData fightArchive;
        public PlayerArchiveData playerData;
        public PlayerStatsArchiveData playerStatsData;
        public Dictionary<int, ChallengeRandomArchiveData> shopArchiveDataDictionary;//技能商店数据
        public List<int> UnlockTechnologyList;// 解锁的 科技列表 key 【武器ID  weapon id，其他就是科技id】 包含默认的
        public string TechnologyInfo;//科技数据
        public Dictionary<int, int> UnlockTechnologySkillDictionary;//科技技能数据
        public Dictionary<int, int> UnlockOtherTechnologySkillDictionary;//非科技技能数据
    }
    //战斗武器存档数据
    public class WeaponFightArchiveData
    {
        public int Lv;//武器等级
        public Dictionary<string, int> Attr;//属性map，key-属性k，v-对应数值或者比例【万分比】
        public List<int> Skills;//技能【武器专属】，武器升级解锁的；
        public List<int> ChipSkills;//技能【武器专属】，芯片专属武器的技能；
    }
    //战斗存档数据
    public class FightArchiveData
    {
        public long Uid;//战斗唯一id;
        public long seed; //随机种子
        public int currSkin;//当前皮肤
        public int DefaultWeapon;//默认武器;
        public List<int> LearnWeapon;//初始习得技能
        public List<int> EvoEffect; //特殊效果；进化的特殊效果集合；
        public List<int> GlobalSkills;//全局技能触发效果
        public Dictionary<int, WeaponFightArchiveData> Weapon; //武器数据
        public Dictionary<string, int> ShipAttr;//飞船属性map，key-属性k，v-对应数值或者比例【万分比】
    }
    
    //技能存档数据
    public class SkillArchiveData
    {
        public int id;//ID
        public long Lv;//技能等级
    }
    //经验存档数据
    public class ExpArchiveData
    {
        public long Exp;
        public long Lv;
    }
    //战斗资源存储数据
    public class BattleResourceArchiveData
    {
        public long Hp;
        public long MaxHp;
        public long GoldCoin;
    }
    //玩家数据存档
    public class PlayerArchiveData
    {
        public long Id;
        public long KillFromSkillId;
        public int ImmuneDmgCount; //已免疫伤害次数
        public int unlockSkillCount; //解锁技能次数
        
        public ExpArchiveData expData;
        public List<SkillArchiveData> activeSkillsArchive;
        public List<SkillArchiveData> autoSkillsArchive;
        public Dictionary<long, float> attrDic;
        public List<SkillArchiveData> passiveSkillsArchive;
        public BattleResourceArchiveData resourceArchiveData;
        public Dictionary<long, Dictionary<string, long>> skillAbilityArchive;
    }
}