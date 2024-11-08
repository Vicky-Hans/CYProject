using DH.UIFramework.Observables;

namespace DH.Data
{
    public class ClothesResource : ObservableObject
    {
        private UnitBase owner;
        private Player player => owner as Player;
        public int ImmuneEveryCount  { get; set; }//免疫伤害次数
        public int BleedTimeSuperposeCount  { get; set; }//流血时长叠加次数
        public float BleedSuperposeTime  { get; set; }//流血叠加时长
        public int SecKillCount  { get; set; }//秒杀概率叠加次数
        public bool IsActiveRevive  { get; set; }//是否开启复活技能
        public float RelifeHp  { get; set; }//复活血量百分比
        public float ReviveAttackBonus { get; set; }  //复活后攻击加成
        public float MissAttackBonus { get; set; }  //闪避后攻击加成
        public float ReviveDownDmg { get; set; }
        
        //skill 910071 50%以上，目标血量越高，伤害越高
        public float S910071HpMore { get; set; }
        public float S910071ExtraHp { get; set; }
        public float S910071UpDmg { get; set; }
        //skill 910073 对满血敌人造成额外伤害
        public float S910073ExtraDmg { get; set; }
        
        public ClothesResource(UnitBase owner)
        {
            this.owner = owner;
        }

        public void InitTriggers()
        {
            CheckReviveTrigger();
            CheckReviveBonusTrigger();
            CheckS910071Trigger();
            CheckS910073Trigger();
        }
        
        private void CheckReviveTrigger()
        {
            if (player == null) return;
            var trigger = player.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.HeroDead, AttributeType.EnableRelife);
            if(trigger == null) return;
            IsActiveRevive = true;
            RelifeHp = trigger.attrMgr.Calc(AttributeType.RelifeHp);
        }

        private void CheckReviveBonusTrigger()
        {
            if (player == null) return;
            var trigger = player.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.Relife, AttributeType.AttackBonus);
            if(trigger == null) return;
            ReviveAttackBonus = trigger.attrMgr.Calc(AttributeType.AttackBonus);
            ReviveDownDmg = trigger.attrMgr.Calc(AttributeType.DownDmg);
        }

        private void CheckS910071Trigger()
        {
            if (player == null) return;
            var trigger = player.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.HitMonsterHpValueMore, AttributeType.ExtraHp);
            if(trigger == null) return;
            trigger.trigger.TryGetValue(AttributeName.HitMonsterHpValueMore,
                out var s910071HpMoreVal);
            S910071HpMore = s910071HpMoreVal * GameConst.AttributeDivisor;
            S910071ExtraHp = trigger.attrMgr.Calc(AttributeType.ExtraHp);
            S910071UpDmg = trigger.attrMgr.Calc(AttributeType.UpDmg);
        }

        private void CheckS910073Trigger()
        {
            if (player == null) return;
            var trigger = player.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.HitMonsterHpValueMore, AttributeType.ExtraDmg);
            if(trigger == null) return;
            S910073ExtraDmg = trigger.attrMgr.Calc(AttributeType.ExtraDmg);
        }
    }
}