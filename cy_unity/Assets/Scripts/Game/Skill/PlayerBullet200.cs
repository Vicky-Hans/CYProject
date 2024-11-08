using System;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerBullet200 : BaseBullet
    {
        public AttackBox attackBox;
        [AssetPath] public string rangePoisonPath;
        private float bulletSpeed;
        private Vector3 startPos;
        private float range;
        private Vector3 direction;
        private Skill skillData;
        private Rect panelRect;
        private int cattapultCount = 0;
        private int pierce = 0;

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
            if(WeaponModelId == 205)
            {
                pierce += Lodash.RoundToInt(skillData.attrMgr.Calc(AttributeType.WindPierce));
            }
            cacheTransform.up = direction;
            cacheTransform.localScale = Vector3.one * bulletSize;
            attackBox.ResetData();
            attackBox.damageArgs = damageArgs;
            attackBox.Damage += AttackBoxOnDamage;
            var panelTrans = BattleManager.Instance.fightingManagerIns.fightPanelTrans.GetComponent<RectTransform>();
            panelRect = panelTrans.rect;
        }

        private void AttackBoxOnDamage(DamageArgs arg1, IDamageable arg2, DamageStatus arg3)
        {
            if((arg3 & DamageStatus.Ignore) != 0)return;
            var unit = arg2 as MonsterController;
            if(unit == null)return;
            var immunePierce = unit.MonsterData.ImmunePierce();
            var unitPos = unit.transform.position;
            if ((arg3 & DamageStatus.Dead) == 0)
            {
                // 对同一目标，伤害提升
                var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitSameTarget, AttributeType.UpDaggerDmg);
                if (trigger != null)
                {
                    var buff = new Buff
                    {
                        id = (int)AttributeType.UpDaggerDmg,
                        attrName = AttributeName.UpDaggerDmg,
                        value = 1,
                        valueType = BuffValueType.Negative,
                        startTime = GameTime.Instance.GTime,
                        duration = 99999,
                        multi = true,
                    };
                    unit.Data.buffMgr.AddBuff(buff);
                }
                // 206 中毒
                if (WeaponModelId == 206)
                {
                    var poisoningTime = skillData.attrMgr.Calc(AttributeType.PoisoningTime) * GameConst.TimeDivisor;
                    var poisonDmg = skillData.attrMgr.Calc(AttributeType.PoisonDmg);
                    var poisonInterval = skillData.attrMgr.Calc(AttributeType.PoisonInterval) * GameConst.TimeDivisor;
                    var atk = skillData.CalcWeaponAtk(WeaponModelId);
                    var buff = new Buff
                    {
                        id = (int)AttributeType.Poisoning,
                        attrName = AttributeName.Poisoning,
                        startTime = GameTime.Instance.GTime,
                        duration = poisoningTime,
                        interval = poisonInterval,
                        value = atk * poisonDmg,
                        valueType = BuffValueType.Negative,
                        multi = true,
                        equipModelId = WeaponModelId,
                    };
                    unit.AddPoisonBuff(buff);
                }
            }
            else
            {
                //206 毒爆
                CheckKillPoison(unit, unitPos);
            }

            // 穿透次数
            if (immunePierce || pierce < arg1.dmgCount)
            {
                Recycle();
            }
        }
        
        public override void OnUpdate(float elapseSeconds)
        {
            var position = cacheTransform.position;
            position += cacheTransform.up * (bulletSpeed * elapseSeconds);
            cacheTransform.position = position;
            CheckCatapult(cacheTransform.localPosition);
            if (Vector3.Distance(startPos, position) > range) Recycle();
        }

        private void CheckCatapult(Vector3 pos)
        {
            if(BattleManager.Instance.IsStageForest())return;
            if(WeaponModelId != 205)return;
            if(skillData.attrMgr.Calc(AttributeType.EnableCatapult) < 1)return;
            var cattapultNum = Lodash.RoundToInt(skillData.attrMgr.Calc(AttributeType.CatapultNum));
            if(cattapultCount >= cattapultNum)
            {
                Recycle();
                return;
            }

            if (!panelRect.Contains(pos))
            {
                var oldUp = cacheTransform.up;
                if(pos.x < panelRect.xMin || pos.x > panelRect.xMax)
                {
                    oldUp.x = -oldUp.x;
                }
                if(pos.y < panelRect.yMin || pos.y > panelRect.yMax)
                {
                    oldUp.y = -oldUp.y;
                }
                cacheTransform.up = oldUp;
                cattapultCount++;
            }
        }
        
        private void PlayRangePoisonEffect(float range, Vector3 pos)
        {
            var effectRadius = 2f;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var obj = InstantiateObj(rangePoisonPath, pos, Quaternion.identity, fightingManager.fightPanelTrans);
            if (obj)
            {
                obj.transform.localScale = Vector3.one * range/effectRadius;
                fightingManager.AddAutoReleaseUnit(obj, 2f, this);
            }
        }

        private void CheckKillPoison(MonsterController unit, Vector3 unitPos)
        {
            if(WeaponModelId!=206)return;
            if(unit.Data.DeadPoisonCount <= 0)return;
            var rangeDmg = skillData.attrMgr.Calc(AttributeType.RangeDmg);
            var rangeDmgDis = skillData.attrMgr.Calc(AttributeType.RangeDmgDis);
            PlayRangePoisonEffect(rangeDmgDis, unitPos);
            var dmgValue = skillData.CalcWeaponAtk(WeaponModelId);
            dmgValue *= rangeDmg;
            var layerMask = LayerMask.GetMask("Enemy");
            int count = Physics2D.OverlapCircleNonAlloc(unitPos, rangeDmgDis,
                PhysicsUtility.CacheCollider, layerMask);
            for (int index = 0; index < count; index++)
            {
                var target = PhysicsUtility.CacheCollider[index];
                if (target == null) continue;
                var monster = target.GetComponent<MonsterController>();
                if (monster == null || monster == unit) continue;
                monster.DecHp((int)dmgValue, WeaponModelId);
                AddRangePoisonBuff(monster);
            }
        }

        private void AddRangePoisonBuff(MonsterController monsterController)
        {
            if(monsterController == null || monsterController.CheckMonsterIsDead())return;
            if(skillData.attrMgr.Calc(AttributeType.EnableRangePoison) < 0.5f)return;
            var poisoningTime = skillData.attrMgr.Calc(AttributeType.RangePoisonTime) * GameConst.TimeDivisor;
            var poisonDmg = skillData.attrMgr.Calc(AttributeType.RangePoisonDmg);
            var poisonInterval = skillData.attrMgr.Calc(AttributeType.RangePoisonInterval) * GameConst.TimeDivisor;
            var atk = skillData.CalcWeaponAtk(WeaponModelId);
            var buff = new Buff
            {
                id = (int)AttributeType.Poisoning,
                attrName = AttributeName.Poisoning,
                startTime = GameTime.Instance.GTime,
                duration = poisoningTime,
                interval = poisonInterval,
                value = atk * poisonDmg,
                valueType = BuffValueType.Negative,
                multi = true,
                equipModelId = WeaponModelId,
            };
            monsterController.AddPoisonBuff(buff);
        }
        
        protected override void Recycle()
        {
            attackBox.Damage -= AttackBoxOnDamage;
            base.Recycle();
        }
        
    }
}