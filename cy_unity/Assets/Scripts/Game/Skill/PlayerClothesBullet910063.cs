using System;
using DH.Data;
using UnityEngine;
namespace DH.Game
{
    public partial class PlayerClothesBullet910063 : BaseBullet
    {
        private Vector3 direction;
        private Vector3 startPos;
        private UnitBase ownerData;
        private float lightningDmg;
        private float startTime = 0f;
        private float durationTime = 0.2f;
        public MonsterController TargetMonster { get; set; }
        [AssetPath] public string boomPath;
        public override void InitWithTarget(DamageArgs damageArgs, Func<string, float> paramGetter,
            Vector3 startPosition, Vector3 targetPosition, IPool<GameObject> pool)
        {
            base.InitWithTarget(damageArgs, paramGetter, startPosition, targetPosition, pool);
            startPos = startPosition;
            direction = (targetPosition - startPos).normalized;
            ownerData = damageArgs.sender.Data;
            var len = (targetPosition - startPos).magnitude;
            var eulerAngle = cacheTransform.eulerAngles;
            eulerAngle.z =  Lodash.Direction2Angle(direction) - 90;
            cacheTransform.eulerAngles = eulerAngle;
            cacheTransform.localScale = new Vector3(1, len/9, 1);
            lightningDmg = damageArgs.damagePoint;
            ApplyDamageOnMonster();
        }
        private void ApplyDamageOnMonster()
        {
            if(TargetMonster == null || TargetMonster.CheckMonsterIsDead())return;
            var unitPos = TargetMonster.transform.position;
            PlayRangeDmgEffect(1, unitPos);
            TargetMonster.DecHp(Mathf.RoundToInt(lightningDmg));
            if(TargetMonster == null|| TargetMonster.CheckMonsterIsDead()) return;
            var palsyTrigger = ownerData.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.MomsterLightning, AttributeType.Palsy);
            if (palsyTrigger == null) return;
            var palsyTime = palsyTrigger.attrMgr.Calc(AttributeType.PalsyTime)*GameConst.TimeDivisor;
            var downAtkSpd = palsyTrigger.attrMgr.Calc(AttributeType.DownAtkSpd);
            var downSpd = palsyTrigger.attrMgr.Calc(AttributeType.DownSpd);
            var buff = new Buff
            {
                id = (int)AttributeType.Palsy,
                attrName = AttributeName.Palsy,
                startTime = GameTime.Instance.GTime,
                duration = palsyTime,
                interval = 0,
                value = 0,
                skillData = ownerData.clothesTriggerSkill,
                valueType = BuffValueType.Negative,
                multi = true,
                clothesId = 1,
            };
            if (TargetMonster != null) TargetMonster.AddClothesPalsyBuff(buff);
            var buffDecAtkSpd = new Buff
            {
                id = (int)AttributeType.DownAtkSpd,
                attrName = AttributeName.DownAtkSpd,
                startTime = GameTime.Instance.GTime,
                duration = palsyTime,
                interval = 0,
                value = downAtkSpd,
                valueType = BuffValueType.Negative,
                multi = true,
                clothesId = 1,
            };
            if (TargetMonster != null) TargetMonster.Data.buffMgr.AddBuff(buffDecAtkSpd);
            var buffDownSpd = new Buff
            {
                id = (int)AttributeType.Decelerate,
                attrName = AttributeName.Decelerate,
                startTime = GameTime.Instance.GTime,
                duration = palsyTime,
                interval = 0,
                value = downSpd,
                valueType = BuffValueType.Negative,
                multi = true,
                clothesId = 1,
            };
            if (TargetMonster != null) TargetMonster.AddDecelerateBuff(buffDownSpd);
        }
        public override void OnUpdate(float elapseSeconds)
        {
            if(Recycled)return;
            startTime += elapseSeconds;
            if (startTime > durationTime) Recycle();
        }

        private void PlayRangeDmgEffect(float range, Vector3 pos)
        {
            var effectRadius = 2f;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var obj = InstantiateObj(boomPath, pos, Quaternion.identity, fightingManager.fightPanelTrans);
            if (!obj) return;
            obj.transform.localScale = Vector3.one * range/effectRadius;
            fightingManager.AddAutoReleaseUnit(obj, 2f, this);
        }
    }
}