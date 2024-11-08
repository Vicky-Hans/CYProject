using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public class MonsterLowHpRecovery
    {
        private float revertPercent;
        private float hpRevert;
        private float revertTime;
        private const float PerRevertCd = 1f;
        private float perRevertTime;
        private float curRevertTime;
        private float revertCd;
        private float curRevertCd;
        private float stopTime;
        private float immuneDmgTime;

        private bool recovering;
        private MonsterController monsterController;
        private float underDmgRevertProb;//怪物受到伤害后有概率恢复其攻击力100%的生命值
        private float underDmgRevertValue;//怪物受到伤害后有概率恢复其攻击力100%的生命值
        public void Init(MonsterController monster)
        {
            monsterController = monster;
            CheckLowHpRecoveryTrigger();
            var underDmgRevertTrigger = monster.Data.triggerSkill.GetTriggerByNameAndComplete(AttributeName.UnderDmg, AttributeType.RevertAtk);
            if (underDmgRevertTrigger == null) return;
            underDmgRevertTrigger.trigger.TryGetValue("prob", out var probValue);
            underDmgRevertProb = probValue * GameConst.AttributeDivisor;
            underDmgRevertValue = underDmgRevertTrigger.attrMgr.Calc(AttributeType.RevertAtk);
        }

        private void CheckLowHpRecoveryTrigger()
        {
            if(monsterController == null)return;
            var trigger =
                monsterController.MonsterData.triggerSkill.GetTriggerByNameAndComplete(
                    AttributeName.MonsterHpValueTh, AttributeType.StopTime);
            if (trigger == null) return;
            trigger.trigger.TryGetValue(AttributeName.MonsterHpValueTh, out var revertPercentValue);
            revertPercent = revertPercentValue * GameConst.AttributeDivisor;
            hpRevert = trigger.attrMgr.Calc(AttributeType.HpRevert);
            revertTime = trigger.attrMgr.Calc(AttributeType.RevertTime) * GameConst.TimeDivisor;
            revertCd = trigger.attrMgr.Calc(AttributeType.RevertCd) * GameConst.TimeDivisor;
            stopTime = trigger.attrMgr.Calc(AttributeType.StopTime) * GameConst.TimeDivisor;
            immuneDmgTime = trigger.attrMgr.Calc(AttributeType.ImmuneDmgTime) * GameConst.TimeDivisor;
            curRevertCd = revertCd + 1f;
        }

        public void TriggerHpPercent()
        {
            if(recovering)return;
            if (curRevertCd < revertCd) return;
            if(monsterController == null || monsterController.CheckMonsterIsDead())return;
            var percent = monsterController.MonsterData.resource.Progress;
            if(percent >= revertPercent)return;
            monsterController.PlayStopRevertFx(stopTime, 1f);
            AddStopBuff();
            AddImmuneDmgBuff();
            recovering = true;
        }
        /// <summary>
        /// 怪物受到伤害后有概率恢复其攻击力100%的生命值
        /// </summary>
        public void CheckHpRecoveryTrigger()
        {
            if (Lodash.RandRangeFloat(0, 1) > underDmgRevertProb) return;
            if(monsterController == null || monsterController.CheckMonsterIsDead()) return;
            var revertValue = monsterController.attackBox.damageArgs.damagePoint * underDmgRevertValue;
            var entityPool = BattleManager.Instance.fightingManagerIns.entityPool;
            var assetPath = "Player/MonsterStopRevert";
            entityPool.LoadAssetSync(assetPath);
            var obj = entityPool.InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, monsterController.transform);
            if (obj == null) return;
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = monsterController.GetFrozenPos();
            BattleManager.Instance.fightingManagerIns.AddAutoReleaseGObj(obj, 1);
            monsterController.HealDelta(Lodash.RoundToInt(revertValue));
        }
        private void AddStopBuff()
        {
            if(recovering)return;
            var buff = new Buff
            {
                id = (int)AttributeType.StopTime,
                attrName = AttributeName.StopTime,
                startTime = GameTime.Instance.GTime,
                duration = stopTime,
                interval = 999f,
                value = 0f,
                valueType = BuffValueType.Negative,
                multi = false,
            };
            monsterController.AddStopBuff(buff);
        }
        
        private void AddImmuneDmgBuff()
        {
            if(recovering)return;
            var buff = new Buff
            {
                id = (int)AttributeType.ImmuneDmg,
                attrName = AttributeName.ImmuneDmg,
                startTime = GameTime.Instance.GTime,
                duration = immuneDmgTime,
                interval = 999f,
                value = 0f,
                valueType = BuffValueType.Positive,
                multi = false,
            };
            monsterController.AddImmuneDmgBuff(buff);
        }

        public void OnUpdate(float deltaTime)
        {
            if (!recovering)
            {
                if(curRevertCd < revertCd)
                {
                    curRevertCd += deltaTime;
                }
                return;
            }
            if (monsterController == null || monsterController.CheckMonsterIsDead()) return;
            perRevertTime += deltaTime;
            curRevertTime += deltaTime;
            if (perRevertTime >= PerRevertCd)
            {
                perRevertTime = 0f;
                monsterController.HealPercent(hpRevert);
            }
            if (curRevertTime >= revertTime)
            {
                curRevertTime = 0f;
                curRevertCd = 0f;
                recovering = false;
            }
        }
    }
}