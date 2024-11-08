using System.Collections.Generic;
using DH.UIFramework;

namespace DH.Data
{
    public partial class PlayerStats : ObservableSingleton<PlayerStats>
    {
        /// <summary>
        /// 击杀数
        /// </summary>
        [AutoNotify] private int killCount;
        
        /// <summary>
        /// 击杀boss的个数
        /// </summary>
        public int KillBossCount { get; set; }

        /// <summary>
        /// 飞船血量
        /// </summary>
        [AutoNotify] private long hp;
        /// <summary>
        /// 飞船最大血量
        /// </summary>
        [AutoNotify] private long maxHp;

        /// <summary>
        /// 防御力
        /// </summary>
        [AutoNotify] private long def;

        /// <summary>
        /// 最大防御力
        /// </summary>
        [AutoNotify] private long maxDef;
        
        [AutoNotify] private long killExp;
        [AutoNotify] private long totalSkillHurts;//所有技能输出的总伤害
        /// <summary>
        /// 技能伤害统计 key 装备模型ID value 技能造成的伤害值
        /// </summary>
        public Dictionary<long, long> SkillHurtsDic;
        private readonly Dictionary<int, long> items = new ();
        public Dictionary<int, long> Items => items;
        public void Init()
        {
            KillCount = GameDataManager.Instance.Kills;
            KillBossCount = 0;
            KillExp = GameDataManager.Instance.Exp;
            TotalSkillHurts = 0;
            SkillHurtsDic ??= new();
            SkillHurtsDic.Clear();
            SkillHurtsDic = GameDataManager.Instance.SkillHurtDic;
            Hp = GameDataManager.Instance.Hp;
            MaxHp = GameDataManager.Instance.MaxHp;
            Items.Clear();
        }
        public void AddHurt(long weaponModelId, long hurt, UnitBase unit=null)
        {
            if(weaponModelId < 100)return;
            if (!SkillHurtsDic.TryGetValue(weaponModelId, out long sum))
            {
                SkillHurtsDic.Add(weaponModelId, hurt);
                CalculateDamageData();
                return;
            }
            sum += hurt;
            SkillHurtsDic[weaponModelId] = sum;
            CalculateDamageData();
        }
        /// <summary>
        /// 计算当前所有技能造成的伤害值
        /// </summary>
        private void CalculateDamageData()
        {
            long tmp = 0;
            foreach (var skillInfo in SkillHurtsDic)
            {
                tmp += skillInfo.Value;
            }
            TotalSkillHurts = tmp;
        }
        /// <summary>
        /// 伤害还原
        /// </summary>
        public void ReviveLastWaveData()
        {
            KillCount = GameDataManager.Instance.killedCount;
            SkillHurtsDic.Clear();
            //技能伤害统计 key 技能ID value 技能造成的伤害值
            foreach (var skillhurtInfo in GameDataManager.Instance.HurtStat)
            {
                AddHurt(skillhurtInfo.Key, skillhurtInfo.Value);
            }
        }
    }
}