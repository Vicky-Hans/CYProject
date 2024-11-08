using System;
using DH.Data;
using UnityEngine;
namespace DH.Game
{
    public partial class PlayerClothesBullet910043 : BaseBullet
    {
        public AttackBox attackBox;
        [AssetPath] public string rangeDmgPath;
        private float bulletSpeed;
        private Vector3 startPos;
        private float range;
        private float pierce;
        private Vector3 direction;
        private UnitBase ownerData;
        public override void InitWithTarget(DamageArgs damageArgs, Func<string, float> paramGetter,
            Vector3 startPosition, Vector3 targetPosition, IPool<GameObject> pool)
        {
            base.InitWithTarget(damageArgs, paramGetter, startPosition, targetPosition, pool);
            startPos = startPosition;
            ownerData = damageArgs.sender.Data;
            SkillTrigger shockWaveTrigger = null;
            if (ownerData.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.HeroUnderAttack, AttributeType.ShockWaveDmg) != null)
            {
                shockWaveTrigger = ownerData.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.HeroUnderAttack, AttributeType.ShockWaveDmg);
            }
            else if (ownerData.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.Reverting, AttributeType.ShockWaveDmg) != null)
            {
                shockWaveTrigger = ownerData.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.Reverting, AttributeType.ShockWaveDmg);
            }
            if (shockWaveTrigger != null)
            {
                pierce = shockWaveTrigger.attrMgr.Calc(AttributeType.Pierce);
                bulletSpeed = shockWaveTrigger.attrMgr.Calc(AttributeType.ShockWaveSpeed);
                bulletSize = shockWaveTrigger.attrMgr.Calc(AttributeType.ShockWaveSize);
            }
            range = Vector3.Distance(targetPosition,startPosition);
            direction = (targetPosition - startPos).normalized;
            var angle = Lodash.Direction2Angle(targetPosition - startPos);
            cacheTransform.localEulerAngles = new Vector3(0,0,-90f+angle);
            cacheTransform.localScale = Vector3.one*(bulletSize*0.5f);
            attackBox.ResetData();
            attackBox.damageArgs = damageArgs;
            attackBox.Damage += AttackBoxOnDamage;
        }
        private void AttackBoxOnDamage(DamageArgs arg1, IDamageable arg2, DamageStatus arg3)
        {
            if((arg3 & DamageStatus.Ignore) != 0) return;
            var unit = arg2 as MonsterController;
            if(unit == null) return;
            var unitPos = unit.transform.position;
            PlayRangeDmgEffect(1, unitPos);
            //检测击退
            if (ownerData.clothesSkillAttr.Calc(AttributeType.ShockWaveRepel) > 0)
            {
                if(unit.CheckMonsterIsDead()) return;
                var repelRange = ownerData.clothesSkillAttr.Calc(AttributeType.ShockWaveRepel);
                unit.Repeled(repelRange);
            }
            // 穿透次数
            if (pierce < arg1.dmgCount) Recycle();
        }
        private void PlayRangeDmgEffect(float range, Vector3 pos)
        {
            var effectRadius = 2f;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var obj = InstantiateObj(rangeDmgPath, pos, Quaternion.identity, fightingManager.fightPanelTrans);
            if (!obj) return;
            obj.transform.localScale = Vector3.one * range/effectRadius;
            fightingManager.AddAutoReleaseUnit(obj, 2f, this);
        }
        public override void OnUpdate(float elapseSeconds)
        {
            var position = cacheTransform.position;
            position += direction * (bulletSpeed * elapseSeconds);
            cacheTransform.position = position;
            if (Vector3.Distance(startPos, position) > range) Recycle();
        }
        protected override void Recycle()
        {
            attackBox.Damage -= AttackBoxOnDamage;
            base.Recycle();
        }
    }
}