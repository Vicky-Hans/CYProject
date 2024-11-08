using System.ComponentModel;
using System.Runtime.CompilerServices;
using Data.Exp;
using DH.UIFramework.Observables;
using UnityEngine;

namespace DH.Data
{
    public class UnitBase : ObservableObject
    {
        public AttributeMgr attr;
        public BuffMgr buffMgr;
        public BattleResource resource;
        public ExpMgr exp;
        public SkillMgr skill;
        public Skill triggerSkill;   // 触发技能, 挂载所有的全局触发效果
        public AttributeMgr clothesSkillAttr;//服饰技能属性
        public Skill clothesTriggerSkill;   // 服饰触发技能, 挂载所有的全局服饰触发效果
        public readonly ClothesResource ClothesResource;   // 服饰技能相关数据
        public Vector3 position;     // 记录当前位置
        public virtual bool IsMonster => false;
        public float SpdFactor = 1f;
        public long Id { get; set; }
        public int DeadPoisonCount { get; set; }
        public int FrozenCount => buffMgr.GetBuffCountById((int)AttributeType.Frozen);
        public int DecelerateCount => buffMgr.GetBuffCountById((int)AttributeType.Decelerate);
        public int FiringCount => buffMgr.GetBuffCountById((int)AttributeType.Firing);
        public int PoisonCount => buffMgr.GetBuffCountById((int)AttributeType.Poisoning);
        private int VertigoCount => buffMgr.GetBuffCountById((int)AttributeType.Vertigo);
        private int StopTimeCount => buffMgr.GetBuffCountById((int)AttributeType.StopTime); 
        private int EnwindCount => buffMgr.GetBuffCountById((int)AttributeType.Enwind); //缠绕次数
        public UnitBase()
        {
            attr = new AttributeMgr(this);
            buffMgr = new BuffMgr(this);
            exp = new ExpMgr(this);
            resource = new BattleResource(this);
            skill = new SkillMgr(this);
            exp.PropertyChanged += ExpMod;
            triggerSkill = new Skill(1, 1, this);
            clothesSkillAttr = new AttributeMgr(this);
            clothesTriggerSkill = new Skill(1, 1, this);
            ClothesResource = new ClothesResource(this);
            position = Vector3.one * 9999f;
        }
        public virtual void ExpMod(object sender, PropertyChangedEventArgs e) { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDead()
        {
            return resource.IsDead();
        }
        public bool IsStucked()
        {
            if(FrozenCount > 0) return true;
            if(VertigoCount > 0) return true;
            if(StopTimeCount > 0) return true;
            if(EnwindCount > 0) return true;
            return false;
        }
        public bool CheckProb(SkillTrigger trigger)
        {
            if(trigger.trigger.TryGetValue(AttributeName.Prob, out var prob))
            {
                return Lodash.RandRangeFloat(0, 1) < prob * GameConst.AttributeDivisor;
            }
            return false;
        }
    }
}