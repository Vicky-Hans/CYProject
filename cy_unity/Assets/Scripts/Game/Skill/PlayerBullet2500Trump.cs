using System;
using DH.Config;
using DH.Data;
using UnityEngine;
namespace DH.Game
{
    public partial class PlayerBullet2500Trump : BaseBullet
    {
        
        private int dmgCount = 0;
        private float takeCd = 0f;
        private int weaponModelId;
        private float startTime = 0f;
        private float enWindDmg = 0f;
        private float enWindTime = 0f;
        private const float Interval = 0.35f;
        private const float Duration = 2.1f;
        [AssetPath] public string explodePath;
        [HideInInspector] public float dmgValue = 0;
        public override void InitWithTarget(DamageArgs damageArgs, Func<string, float> paramGetter,
            Vector3 startPosition, Vector3 targetPosition, IPool<GameObject> pool)
        {
            base.InitWithTarget(damageArgs, paramGetter, startPosition, targetPosition, pool);
            weaponModelId = damageArgs.weaponModelId;
            var trigger = damageArgs.skillData.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.EnableDeathEnwind);
            if (trigger == null) return;
            enWindTime = trigger.attrMgr.Calc(AttributeType.EnwindTime) * GameConst.TimeDivisor;
            enWindTime = (trigger.attrMgr.Calc(AttributeType.EnwindTime)+damageArgs.skillData.attrMgr.Calc(AttributeType.EnwindTime)) * GameConst.TimeDivisor;
            enWindDmg = damageArgs.skillData.CalcAtk();
            var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(WeaponModelId);
            if (modelCfg != null) enWindDmg = Mathf.FloorToInt(enWindDmg * modelCfg.Compose[0] * GameConst.AttributeDivisor + 0.5f);
            enWindDmg *= trigger.attrMgr.Calc(AttributeType.EnwindDmg);
        }
        private void ApplyDamageOnMonster()
        {
            takeCd = 0f;
            dmgCount++;
            var tmpStartPos = BattleManager.Instance.fightingManagerIns.playerCtrl.transform.position;
            var playerStartPos = new Vector3(tmpStartPos.x,tmpStartPos.y+0.6f,tmpStartPos.z);
            var effectStartPos = new Vector3((playerStartPos.x + 8) * 0.5f,playerStartPos.y);
            var rectSize = new Vector2(8,3.5f);
            var layerMask = LayerMask.GetMask("Enemy");
            var count = Physics2D.OverlapBoxNonAlloc(effectStartPos,rectSize,Lodash.Direction2Angle(Vector3.right),PhysicsUtility.CacheCollider,layerMask);
            for (var i = 0; i < count; ++i)
            {
                var target = PhysicsUtility.CacheCollider[i];
                if (target == null) continue;
                var monster = target.GetComponent<MonsterController>();
                if (monster == null || monster.CheckMonsterIsDead()) continue;
                PlayRangeDmgEffect(monster.transform.position);
                monster.DecHp(Lodash.RoundToInt(dmgValue), weaponModelId);
                if (monster.CheckMonsterIsDead() || enWindDmg <= 0 || enWindTime <= 0) continue;
                var enWindBuff = monster.Data.buffMgr.FindBuffById((int)AttributeType.Enwind);
                if (enWindBuff != null)
                {
                    enWindBuff.startTime = GameTime.Instance.GTime;
                }
                else
                {
                    monster.AddEnwindBuff(new Buff
                    {
                        id = (int)AttributeType.Enwind,
                        attrName = AttributeName.Enwind,
                        startTime = GameTime.Instance.GTime,
                        duration = enWindTime, multi = true,
                        value = enWindDmg,interval = 1, 
                        valueType = BuffValueType.Negative,
                    });
                }
            }
        }
        private void PlayRangeDmgEffect(Vector3 pos)
        {
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var obj = InstantiateObj(explodePath, pos, Quaternion.identity, fightingManager.fightPanelTrans);
            if (!obj) return;
            obj.transform.localPosition = new Vector3(obj.transform.localPosition.x,obj.transform.localPosition.y,obj.transform.localPosition.z);
            obj.transform.localScale = Vector3.one;
            fightingManager.AddAutoReleaseUnit(obj, 1f, this);
        }
        public override void OnUpdate(float elapseSeconds)
        {
            if(Recycled)return;
            takeCd += elapseSeconds;
            startTime += elapseSeconds;
            if (takeCd > Interval && dmgCount == 0) ApplyDamageOnMonster();
            if (startTime > Duration) Recycle();
        }
    }
}