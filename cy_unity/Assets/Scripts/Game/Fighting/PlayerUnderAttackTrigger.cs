using DH.Data;

namespace DH.Game
{
    public class PlayerUnderAttackTrigger
    {
        private Player player;
        private FightingBaseManager fightingBaseManager;
        
        public bool IsActiveUnderAttackMiss { get; set; }
        private float missProb;
        public bool IsActiveUnderAttackReHit { get; set; }
        private float reHitProb;
        private float reHitDmg;

        public void Init(Player p)
        {
            player = p;
            fightingBaseManager = BattleManager.Instance.fightingManagerIns;
            CheckUnderAttackMiss();
            CheckUnderAttackReHit();
        }

        private void CheckUnderAttackMiss()
        {
            var trigger = player.triggerSkill.GetTriggerByNameAndComplete("underAttack", AttributeType.Miss);
            if(trigger == null)return;
            trigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            missProb = probValue * GameConst.AttributeDivisor;
            IsActiveUnderAttackMiss = true;
        }

        private void CheckUnderAttackReHit()
        {
            var trigger = player.triggerSkill.GetTriggerByNameAndComplete("underAttack",
                AttributeType.ReHit);
            if(trigger == null)return;
            trigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            reHitProb = probValue * GameConst.AttributeDivisor;
            reHitDmg = trigger.attrMgr.Calc(AttributeType.ReHitDmg);
            IsActiveUnderAttackReHit = true;
        }

        public float MissRate => IsActiveUnderAttackMiss ? missProb : 0f;

        public void TriggerReHit()
        {
            if(!IsActiveUnderAttackReHit)return;
            if(Lodash.RandRangeFloat(0, 1) > reHitProb)return;
            BattleManager.Instance.fightingManagerIns.ShootWeaponMaxLevel(2, reHitDmg);
        }
    }
}