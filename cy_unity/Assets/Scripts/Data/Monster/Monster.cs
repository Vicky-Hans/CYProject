using DH.Config;
using DHFramework;
using UnityEngine.PlayerLoop;

namespace DH.Data
{
    public class Monster : UnitBase
    {
        public MonsterCfg cfg;
        public MonsterModelCfg modelCfg;

        public override bool IsMonster => true;
        
        public bool IsSplitBody { get; set; } // 是否是分身，分身不提供exp，不继承怪物特性

        public float AtkBonus { get; set; }
        public int AtkBonusCount { get; set; }
        public float BreakArmorCount { get; set; }//怪物区域破甲次数
        public float BreakArmorDmg { get; set; }//怪物破甲伤害特性
        /// <summary>
        /// 攻速提升圈属性
        /// </summary>
        public float DownCd { get; set; }
        public int DownCdCount { get; set; }

        public Monster(MonsterCfg monsterCfg, bool isSplitBody = false)
        {
            Id = monsterCfg.Id;
            cfg = monsterCfg;
            modelCfg = ConfigCenter.MonsterModelCfgColl.GetDataById(monsterCfg.ModelId);
            IsSplitBody = isSplitBody;
            attr.Modify(monsterCfg.MonsterPro);
            GetSpdFactor();
            if(cfg.AtkId != null){
                foreach (var item in cfg.AtkId)
                {
                    skill.AddSkill(item.Skill, SkillType.Auto);
                }
            }
            if(IsSplitBody){}
            else
            {
                if(cfg.CharacterId != null){
                    cfg.CharacterId.ForEach(AddFeature);
                }
                if (GameDataManager.Instance.CurStageType == EStateType.StageTypeChallenge)
                {
                    //先刷新基础属性：血量+攻击力
                    var baseHp = GameDataManager.Instance.GetMonsterHpByMonsterId((int)Id);
                    var baseAtk = GameDataManager.Instance.GetMonsterAtkByMonsterId((int)Id);
                    if (baseHp > 0) ReInitHp(baseHp);
                    if (baseAtk > 0) ReInitAtk(baseAtk);
                    foreach (var attrInfo in attr.AllAttributes)
                    {
                        if ((AttributeType)attrInfo.Key == AttributeType.Atk||(AttributeType)attrInfo.Key == AttributeType.Hp 
                            || (AttributeType)attrInfo.Key == AttributeType.Cd || (AttributeType)attrInfo.Key == AttributeType.Spd) continue;
                        attrInfo.Value.ValueBase = 0f;
                    }
                    var buffList = DataCenter.dailyFightData.Buff;
                    if (buffList != null)
                    {
                        foreach (var buffId in buffList)
                        {
                            var buffCfg = ConfigCenter.DailyStageBuffCfgColl.GetDataById(buffId);
                            if (buffCfg == null)continue;
                            var skillId = buffCfg.SkillId;
                            AddFeature(skillId);
                        }
                    }
                }
                else if (GameDataManager.Instance.CurStageType is EStateType.StageTypeEndless or EStateType.StageTypeSecret)//无尽关卡怪物基础血量+攻击力单独取值
                {
                    var baseAttract= GameDataManager.Instance.GetEndlessMonsterBaseAttr((int)Id);
                    if (baseAttract.x > 1) ReInitHp((long)baseAttract.x);
                    if (baseAttract.y > 1) ReInitAtk((long)baseAttract.y);
                }
            }
            resource.Init();
        }
        /// <summary>
        /// 检测当前是否为无尽模式
        /// </summary>
        /// <returns></returns>
        private void GetSpdFactor()
        {
            SpdFactor = 1f;
        }
        
        public void AddFeature(int skillId)
        {
            var skillCfg = ConfigCenter.SkillCfgColl.GetDataById(skillId);
            if (skillCfg == null) return;
            if (skillCfg?.Result != null)
            {
                attr.Modify(skillCfg.Result);
            }
            triggerSkill.AddAllTrigger(skillCfg);
        }

        public bool IsBoss()
        {
            if (IsSplitBody) return false;
            return cfg.MonsterType == (int)MonsterType.Boss;
        }

        public bool ImmuneHurt()
        {
            return attr.Calc(AttributeType.ImmuneHurt) > 0.5f;
        }
        
        public bool ImmuneFrozen()
        {
            return attr.Calc(AttributeType.ImmuneFrozen) > 0.5f;
        }
        
        public bool ImmuneVertigo()
        {
            return attr.Calc(AttributeType.ImmuneVertigo) > 0.5f;
        }

        public bool ImmuneRepel()
        {
            var immuneRepel = attr.Calc(AttributeType.ImmuneRepel);
            if (immuneRepel > 0.5f) return true;
            var trigger = triggerSkill.GetTriggerByNameAndComplete(AttributeName.UnderAttack,
                AttributeType.ImmuneRepel);
            if (trigger == null) return false;
            return CheckProb(trigger);
        }

        public bool ImmunePierce()
        {
            var immunePierce = attr.Calc(AttributeType.ImmunePierce);
            return immunePierce > 0.5f;
        }

        public bool ImmuneDmg()
        {
            return buffMgr.GetBuffCountById((int)AttributeType.ImmuneDmg) > 0;
        }
        public bool ImmuneStop()
        {
            return buffMgr.GetBuffCountById((int)AttributeType.ImmuneStop) > 0;
        }
        public void ReInitAtk(long atk)
        {
            attr.SetAttr(AttributeType.Atk, atk);
        }

        public void ReInitHp(long hp)
        {
            attr.SetAttr(AttributeType.Hp, hp);
            resource.Init();
        }
    }
}