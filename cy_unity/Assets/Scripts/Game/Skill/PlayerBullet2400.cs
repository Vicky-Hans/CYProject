using System;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerBullet2400 : BaseBullet
    {
        private float range;
        private Skill skillData;
        private Vector3 startPos;
        private Vector3 direction;
        private float bulletSpeed;
        public AttackBox attackBox;
        private float rotateSpd = 0f;
        private int dmgCount = 0;//当前伤害的个数
        private Vector3 eulerAngles;
        private float smallBulletStart = 0f;
        private float smallBulletTakeCd = 0f;
        private float smallBulletInterval = 0f;
        private float smallBulletDuration = 0f;
        private float smallBulleDmgValue = 0f;
        private float smallBulletCircleRadius = 0f;
        private float scaleIncrementValue = 0f;
        private DamageArgs curDamageArgs;
        private IPool<GameObject> curBulletPool;
        private Func<string, float> curParamGetter;
        private SkillTrigger enableStayTrigger;//金箍棒会停留在首个攻击的单位处
        private SkillTrigger enableThumpTrigger;//万钧一击
        [SyncAssetPath] public string thumBulletPath;
        public override void InitWithTarget(DamageArgs damageArgs, Func<string, float> paramGetter,
            Vector3 startPosition, Vector3 targetPosition, IPool<GameObject> pool)
        {
            base.InitWithTarget(damageArgs, paramGetter, startPosition, targetPosition, pool);
            smallBulletStart = 0f;
            smallBulletTakeCd = 0f;
            curBulletPool = pool;
            curDamageArgs = damageArgs;
            curParamGetter = paramGetter;
            startPos = startPosition;
            skillData = damageArgs.skillData;
            bulletSpeed = skillData.BulletSpeed();
            range = skillData.Range();
            direction = (targetPosition - startPos).normalized;
            bulletSize = skillData.BulletSize();
            scaleIncrementValue = skillData.KillNum*skillData.attrMgr.Calc(AttributeType.UpBulletSize);
            enableThumpTrigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.EnableThump);//万钧一击
            enableStayTrigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitFirstTarget, AttributeType.EnableStay);//金箍棒会停留在首个攻击的单位处
            if (enableStayTrigger!= null) //启用停留属性
            {
                smallBulletDuration = enableStayTrigger.attrMgr.Calc(AttributeType.StayTime);
                smallBulletDuration += skillData.attrMgr.Calc(AttributeType.StayTime);
                smallBulletDuration *= GameConst.TimeDivisor;
                smallBulletInterval = enableStayTrigger.attrMgr.Calc(AttributeType.StayDmgInterval)* GameConst.TimeDivisor;
            }
            var upBulletSizeTrigger = skillData.GetTriggerByNameAndComplete(AttributeName.KillTarget, AttributeType.UpBulletSize);
            if (upBulletSizeTrigger != null) 
            {
                scaleIncrementValue += skillData.KillNum*upBulletSizeTrigger.attrMgr.Calc(AttributeType.UpBulletSize);
                if (scaleIncrementValue > upBulletSizeTrigger.attrMgr.Calc(AttributeType.BulletSizeMax))
                {
                    scaleIncrementValue = upBulletSizeTrigger.attrMgr.Calc(AttributeType.BulletSizeMax);
                }
            }
            smallBulletCircleRadius = bulletSize*(1+scaleIncrementValue)*2.4f*0.5f;
            cacheTransform.eulerAngles = new Vector3(50, 0, 0f);
            cacheTransform.localScale = Vector3.one * (bulletSize*(1+scaleIncrementValue))/0.6f;
            attackBox.ResetData();
            attackBox.damageArgs = damageArgs;
            attackBox.Damage += AttackBoxOnDamage;
            rotateSpd = skillData.attrMgr.Calc(AttributeType.RotateSpd);
            eulerAngles = cacheTransform.eulerAngles;
        }
        private void AttackBoxOnDamage(DamageArgs arg1, IDamageable arg2, DamageStatus arg3)
        {
            if((arg3 & DamageStatus.Ignore) != 0)return;
            var unit = arg2 as MonsterController;
            if(unit == null)return;
            var unitPos = unit.transform.position;
            dmgCount = arg1.dmgCount;
            if (enableStayTrigger!= null)
            {
                smallBulleDmgValue = arg1.damagePoint*enableStayTrigger.attrMgr.Calc(AttributeType.StayDmg);
                if (enableThumpTrigger != null) EnableThumpAttr(unitPos,arg1.damagePoint);//启用万钧一击
            }
            else
            {
                if (enableThumpTrigger != null) EnableThumpAttr(unitPos,arg1.damagePoint);//启用万钧一击
                if (unit.MonsterData.ImmunePierce() || skillData.Pierce() < dmgCount) Recycle();
            }
        }
        /// <summary>
        /// 启用启用万钧一击属性
        /// </summary>
        private void EnableThumpAttr(Vector3 pos,long damagePoint)
        {
            enableThumpTrigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            probValue += Lodash.RoundToInt(skillData.attrMgr.Calc(AttributeType.ThumpProb));
            var prob = probValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1f) > prob) return;
            if (skillData.owner.buffMgr.FindBuffById((int)AttributeType.ThumpDmg) != null) return;
            skillData.owner.buffMgr.AddBuff(new Buff {  
                id = (int)AttributeType.ThumpDmg,
                attrName = AttributeName.ThumpDmg,
                startTime = GameTime.Instance.GTime,
                valueType = BuffValueType.Negative,
                duration = 2.5f, interval = 0, value = 0, multi = true});
            var playerStartPos = BattleManager.Instance.fightingManagerIns.playerCtrl.transform.position;
            var effectStartPos = new Vector3(playerStartPos.x+0.6f,playerStartPos.y,0);
            var bullet = curBulletPool.InstantiateObj(thumBulletPath, effectStartPos, Quaternion.identity, BattleManager.Instance.fightingManagerIns.fightPanelTrans);
            var bulletComp = bullet.GetComponent<PlayerBullet2400Thump>();
            bulletComp.dmgValue = damagePoint*enableThumpTrigger.attrMgr.Calc(AttributeType.ThumpDmg);;
            bulletComp.InitWithTarget(curDamageArgs.Clone(), curParamGetter, effectStartPos, pos, curBulletPool);
        }
        public override void OnUpdate(float elapseSeconds)
        {
            if(Recycled)return;
            if (dmgCount > 0 && dmgCount >= skillData.Pierce() && enableStayTrigger!= null && Vector3.Distance(startPos, cacheTransform.position) <= range)
            {
                smallBulletStart += elapseSeconds;
                smallBulletTakeCd += elapseSeconds;
                eulerAngles.z -= rotateSpd * elapseSeconds;
                if (eulerAngles.z < 360f) eulerAngles.z += 360f*10;
                cacheTransform.eulerAngles = eulerAngles;
                if (smallBulletStart > smallBulletDuration)
                {
                    Recycle();
                    return;
                };
                if (smallBulletTakeCd < smallBulletInterval) return;
                smallBulletTakeCd = 0f;
                TakeSmallBulletShootInternal();
            }
            else
            {
                eulerAngles.z -= rotateSpd * elapseSeconds;
                if (eulerAngles.z < 360f) eulerAngles.z += 360f;
                cacheTransform.eulerAngles = eulerAngles;
                var position = cacheTransform.position;
                position += direction * (bulletSpeed * elapseSeconds);
                cacheTransform.position = position;
                if (Vector3.Distance(startPos, position) > range) Recycle();
            }
        }
        private void TakeSmallBulletShootInternal()
        {
            var layerMask = LayerMask.GetMask("Enemy");
            var count = Physics2D.OverlapCircleNonAlloc(cacheTransform.position, smallBulletCircleRadius , PhysicsUtility.CacheCollider, layerMask);
            if (count == 0) return;
            for (var i = 0; i < count; i++)
            {
                var target = PhysicsUtility.CacheCollider[i];
                var monster = target.GetComponent<MonsterController>();
                if (monster == null) continue;
                if (monster.CheckMonsterIsDead()) continue;
                monster.DecHp(Lodash.RoundToInt(smallBulleDmgValue), WeaponModelId);
                if (monster.CheckMonsterIsDead()) curDamageArgs.skillData.KillNum++;
            }
        }
        protected override void Recycle()
        {
            attackBox.Damage -= AttackBoxOnDamage;
            base.Recycle();
        }
    }
}