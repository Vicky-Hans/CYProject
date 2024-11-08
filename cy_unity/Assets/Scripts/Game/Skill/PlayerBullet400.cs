using System;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerBullet400 : BaseBullet
    {
        public AttackBox attackBox;
        private float bulletSpeed;
        private Vector3 startPos;
        private float range;
        private Vector3 direction;
        private Skill skillData;
        private int pierce;
        public int launchNum; //弹射次数
        public int launchCount; //已弹射次数
        private float launchNumDmg;
        
        public bool IsSplitBody { get; set; }
        public MonsterController originalMonster { get; set; }

        public override void InitWithTarget(DamageArgs damageArgs, Func<string, float> paramGetter,
            Vector3 startPosition, Vector3 targetPosition, IPool<GameObject> pool)
        {
            base.InitWithTarget(damageArgs, paramGetter, startPosition, targetPosition, pool);
            skillData = damageArgs.skillData;
            startPos = startPosition;
            bulletSpeed = skillData.BulletSpeed();
            range = skillData.Range();
            direction = targetPosition - startPos;
            bulletSize = skillData.attrMgr.Calc(AttributeType.BulletSize);
            pierce = skillData.Pierce();
            launchNum = Lodash.RoundToInt(skillData.attrMgr.Calc(AttributeType.LaunchNum));
            launchNumDmg = skillData.attrMgr.Calc(AttributeType.LaunchNumDmg);
            if (WeaponModelId == 406)
            {
                var windLaunchNum = Lodash.RoundToInt(skillData.attrMgr.Calc(AttributeType.WindLaunchNum));
                launchNum += windLaunchNum;
                launchNumDmg += skillData.attrMgr.Calc(AttributeType.WindLaunchNumDmg);
            }
            cacheTransform.up = direction;
            cacheTransform.localScale = Vector3.one * bulletSize;
            attackBox.ResetData();
            if (IsSplitBody)
            {
                if (originalMonster != null)
                {
                    attackBox.AddDamagable(originalMonster);
                }
            }
            attackBox.damageArgs = damageArgs;
            attackBox.Damage += AttackBoxOnDamage;
        }

        private void AttackBoxOnDamage(DamageArgs arg1, IDamageable arg2, DamageStatus arg3)
        {
            if((arg3 & DamageStatus.Ignore) != 0)return;
            if(launchNum > 0 && launchCount < launchNum)
            {
                Split(1, arg1, arg2);
                CheckLaunchSplit(arg1, arg2);
            }
            var unit = arg2 as MonsterController;
            if(unit == null)return;
            var immunePierce = unit.MonsterData.ImmunePierce();
            if (WeaponModelId == 405)
            {
                CheckHurt(unit);
            }
            // 穿透次数
            if (immunePierce || pierce < arg1.dmgCount)
            {
                Recycle();
            }
        }

        /// <summary>
        /// 弹射时，检查分裂
        /// </summary>
        /// <param name="args"></param>
        /// <param name="damageable"></param>
        private void CheckLaunchSplit(DamageArgs args, IDamageable damageable)
        {
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.Launch, AttributeType.SplitNum);
            if (trigger == null) return;
            var splitNum = Lodash.RoundToInt(trigger.attrMgr.Calc(AttributeType.SplitNum));
            if (splitNum <= 0) return;
            for(int i=0; i<splitNum; i++)
            {
                Split(1, args, damageable, true);
            }
        }

        private void CheckHurt(MonsterController monster)
        {
            if(monster.CheckMonsterIsDead())return;
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.Hurt);
            if (trigger == null) return;
            if(!trigger.trigger.TryGetValue("prob", out var probValue))return;
            var prob = probValue * GameConst.AttributeDivisor;
            prob += skillData.attrMgr.Calc(AttributeType.HurtProb);
            if(Lodash.RandRangeFloat(0, 1) > prob)return;
            var hurtTime = skillData.attrMgr.Calc(AttributeType.HurtTime) * GameConst.TimeDivisor;
            var hurtDmg = skillData.attrMgr.Calc(AttributeType.HurtDmg);
            var buff = new Buff
            {
                id = (int)AttributeType.Hurt,
                attrName = AttributeName.Hurt,
                value = hurtDmg,
                valueType = BuffValueType.Negative,
                startTime = GameTime.Instance.GTime,
                duration = hurtTime,
                multi = false,
            };
            // monster.Data.buffMgr.AddBuff(buff);
            monster.AddHurtBuff(buff);
        }
        
        public override void OnUpdate(float elapseSeconds)
        {
            var position = cacheTransform.position;
            position += cacheTransform.up * (bulletSpeed * elapseSeconds);
            cacheTransform.position = position;
            if (Vector3.Distance(startPos, position) > range) Recycle();
        }

        /// <summary>
        /// 弹射 or 分裂
        /// </summary>
        /// <param name="num"></param>
        /// <param name="args"></param>
        /// <param name="damageable"></param>
        private void Split(int num, DamageArgs args, IDamageable damageable, bool isSplit = false)
        {
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var startPosition = cacheTransform.position;
            var target = fightingManager.GetRandMonster();
            if (target == null) return;
            var tarPos = target.transform.position;
            var tmpBulletPath = GetSplitBulletPath(args);
            var bullet = InstantiateObj(tmpBulletPath, startPosition, Quaternion.identity, fightingManager.fightPanelTrans);
            var bulletComp = bullet.GetComponent<PlayerBullet400>();
            bulletComp.IsSplitBody = true;
            bulletComp.originalMonster = damageable as MonsterController;
            bulletComp.launchCount = launchCount + 1;
            var dmgArgs = args.Clone();
            dmgArgs.weaponModelId = WeaponModelId;
            if(isSplit)
            {
                dmgArgs.dmgPercent = 1f;
                bulletComp.launchCount = 99;
            }
            else
            {
                dmgArgs.dmgPercent = launchNumDmg;
            }
            bulletComp.InitWithTarget(dmgArgs, configGetter, startPosition, tarPos, this);
        }

        private string GetSplitBulletPath(DamageArgs args)
        {
            var weaponModelId = args.weaponModelId;
            var skillIns = args.skillIns;
            return (skillIns as PlayerSkill400).GetBulletPath(weaponModelId);
        }
        
        protected override void Recycle()
        {
            attackBox.Damage -= AttackBoxOnDamage;
            base.Recycle();
        }
        
    }
}