using System;
using DH.Config;
using DH.Data;
using UnityEngine;
using UnityEngine.Pool;

namespace DH.Game
{
    public partial class PlayerBullet1200 : BaseBullet
    {
        public AttackBox attackBox;
        private float bulletSpeed;
        private Vector3 startPos;
        private float range;
        private Vector3 direction;
        private Skill skillData;
        private int pierce;
        private RotateSelf rotateSelfComp;

        public override void InitWithTarget(DamageArgs damageArgs, Func<string, float> paramGetter,
            Vector3 startPosition, Vector3 targetPosition, IPool<GameObject> pool)
        {
            base.InitWithTarget(damageArgs, paramGetter, startPosition, targetPosition, pool);
            skillData = damageArgs.skillData;
            startPos = startPosition;
            bulletSpeed = skillData.BulletSpeed();
            range = skillData.Range();
            pierce = skillData.Pierce();
            direction = (targetPosition - startPos).normalized;
            bulletSize = skillData.BulletSize();
            // cacheTransform.up = direction;
            cacheTransform.localScale = Vector3.one * bulletSize;
            attackBox.ResetData();
            attackBox.damageArgs = damageArgs;
            attackBox.Damage += AttackBoxOnDamage;
            
            var rotateSpd = skillData.attrMgr.Calc(AttributeType.RotateSpd);
            rotateSelfComp = gameObject.AddComponent<RotateSelf>();
            rotateSelfComp.BulletIns = this;
            rotateSelfComp.Spd = rotateSpd;
        }

        private void AttackBoxOnDamage(DamageArgs arg1, IDamageable arg2, DamageStatus arg3)
        {
            if((arg3 & DamageStatus.Ignore) != 0)return;
            var unit = arg2 as MonsterController;
            if(unit == null)return;
            var immunePierce = unit.MonsterData.ImmunePierce();
            var unitPos = unit.transform.position;
            // 重伤
            CheckHurt(unit);
            // 1205 叠盾
            CheckDmgArmor(arg1);
            // 1206 吸血
            CheckDmgRevert(arg1);
            // 穿透次数
            if (immunePierce || pierce < arg1.dmgCount)
            {
                Recycle();
            }
        }

        private void CheckHurt(MonsterController monster)
        {
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.Hurt);
            if (trigger == null) return;
            trigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1f) > prob)return;
            var hurtDmg = skillData.attrMgr.Calc(AttributeType.HurtDmg);
            var hurtTime = skillData.attrMgr.Calc(AttributeType.HurtTime);
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
            monster.AddHurtBuff(buff);
        }

        private void CheckDmgArmor(DamageArgs args)
        {
            if(WeaponModelId != 1205)return;
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.DmgArmor);
            if (trigger == null) return;
            if(!trigger.trigger.TryGetValue(AttributeName.Prob, out var probValue))return;
            var prob = probValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1) > prob)return;
            var dmg = args.damagePoint;
            var dmgArmor = trigger.attrMgr.Calc(AttributeType.DmgArmor);
            var everyDmgArmor = skillData.attrMgr.Calc(AttributeType.DmgArmor);
            if (everyDmgArmor > 0.01f)
            {
                // 四周防具
                var weaponSkill = args.weaponSkill;
                var weaponUid = weaponSkill.WeaponUid;
                var wList = ListPool<BackpackWeaponData>.Get();
                GameDataManager.Instance.GetWeaponNearbyList(weaponUid, wList);
                var count = 0;
                foreach (var w in wList)
                {
                    var equipId = w.EquipId;
                    var equipCfg = ConfigCenter.EquipCfgColl.GetDataById(equipId);
                    if(equipCfg == null)continue;
                    if (equipCfg.AtkType == (int)EquipAtkType.Defender)
                    {
                        count++;
                    }
                }
                ListPool<BackpackWeaponData>.Release(wList);
                dmgArmor += count * everyDmgArmor;
            }
            var armor = dmg * dmgArmor;
            skillData.owner.resource.AddArmor(Lodash.RoundToInt(armor));
        }

        private void CheckDmgRevert(DamageArgs args)
        {
            if(WeaponModelId != 1206)return;
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.DmgRevert);
            if (trigger == null) return;
            if(!trigger.trigger.TryGetValue(AttributeName.Prob, out var probValue))return;
            var prob = probValue * GameConst.AttributeDivisor;
            prob += GetDmgRevertProb();
            if(Lodash.RandRangeFloat(0, 1f) > prob)return;
            var dmg = args.damagePoint;
            var dmgRevert = trigger.attrMgr.Calc(AttributeType.DmgRevert);
            var revert = dmg * dmgRevert;
            skillData.owner.resource.AddHp(Lodash.RoundToInt(revert));
        }

        private float GetDmgRevertProb()
        {
            var trigger = skillData.GetTriggerByNameAndComplete(AttributeName.AsHpValueTh, AttributeType.DmgRevertProb);
            if (trigger == null) return 0f;
            trigger.trigger.TryGetValue(AttributeName.AsHpValueTh, out var asHpValueTh);
            if(skillData.owner.resource.Progress > asHpValueTh * GameConst.AttributeDivisor)
            {
                return 0f;
            }
            return trigger.attrMgr.Calc(AttributeType.DmgRevertProb);
        }
        
        public override void OnUpdate(float elapseSeconds)
        {
            if(Recycled)return;
            rotateSelfComp.OnUpdate(elapseSeconds);
            var position = cacheTransform.position;
            position += direction * (bulletSpeed * elapseSeconds);
            cacheTransform.position = position;
            if (Vector3.Distance(startPos, position) > range)
            {
                Recycle();
            }
        }
        
        protected override void Recycle()
        {
            attackBox.Damage -= AttackBoxOnDamage;
            base.Recycle();
        }
        
    }
}