using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public class InRangeTrigger
    {
        private Player player;
        private FightingBaseManager fightingBaseManager;
        private Vector3 playerPos;
        private Transform playerTrans;
        private Transform fxTrans;
        
        public bool IsActiveInRange { get; set; }
        private float inRange;
        private float roleHpDmg;
        private float downAtkSpd;
        private float triggerCd = 1f;
        private float triggerTime = 0f;
        private readonly float inRangeFxRadius = 10.8f;
        
        public void Init(Player p)
        {
            player = p;
            fightingBaseManager = BattleManager.Instance.fightingManagerIns;
            playerPos = fightingBaseManager.PlayerWorldPos;
            playerTrans = fightingBaseManager.playerCtrl.transform;
            CheckInRange();
            ShowRangeFx();
        }

        private void CheckInRange()
        {
            if(player.triggerSkill == null)
            {
                IsActiveInRange = false;
                return;
            }
            var trigger = player.triggerSkill.GetTriggerByNameAndComplete(AttributeName.InRange,
                AttributeType.EnableRoleHpDmg);
            if (trigger == null)
            {
                IsActiveInRange = false;
                return;
            }
            IsActiveInRange = true;
            trigger.trigger.TryGetValue(AttributeName.InRange, out var inRangeValue);
            inRange = inRangeValue * GameConst.AttributeDivisor;
            roleHpDmg = trigger.attrMgr.Calc(AttributeType.RoleHpDmg);
            downAtkSpd = trigger.attrMgr.Calc(AttributeType.DownAtkSpd);
        }

        private void ShowRangeFx()
        {
            if(!IsActiveInRange)return;
            var rangeFx = Object.Instantiate(fightingBaseManager.playerCtrl.inRangeEffect, playerPos, Quaternion.identity, fightingBaseManager.fightPanelTrans);
            if(rangeFx == null)return;
            fxTrans = rangeFx.transform;
            var scale = inRange / inRangeFxRadius;
            rangeFx.transform.localScale = new Vector3(scale, scale, 1f);
            rangeFx.SetActive(true);
        }

        private void TriggerInRange()
        {
            var rangePos = playerPos;
            if (BattleManager.Instance.IsStageForest())
            {
                rangePos = playerTrans.position;
            }
            var layerMask = LayerMask.GetMask("Enemy");
            var count = Physics2D.OverlapCircleNonAlloc(rangePos, inRange , PhysicsUtility.CacheCollider, layerMask);
            for (var i = 0; i < count; ++i)
            {
                var target = PhysicsUtility.CacheCollider[i];
                if (target == null) continue;
                var monster = target.GetComponent<MonsterController>();
                if (monster == null) continue;
                if(monster.CheckMonsterIsDead())continue;
                var buff = new Buff
                {
                    id = (int)AttributeType.DownAtkSpd,
                    attrName = AttributeName.DownAtkSpd,
                    startTime = GameTime.Instance.GTime,
                    duration = triggerCd,
                    interval = 0,
                    value = downAtkSpd,
                    valueType = BuffValueType.Negative,
                    multi = true,
                };
                monster.Data.buffMgr.AddBuff(buff);
                var roleHp = player.resource.Hp;
                var dmg = roleHp * roleHpDmg;
                monster.DecHp(Lodash.RoundToInt(dmg));
            }
        }

        public void OnUpdate(float dt)
        {
            if(!IsActiveInRange)return;
            if (BattleManager.Instance.IsStageForest())
            {
                if(fxTrans!=null && playerTrans != null)
                {
                    fxTrans.position = playerTrans.position;
                }
            }
            triggerTime += dt;
            if (!(triggerTime > triggerCd)) return;
            triggerTime = 0f;
            TriggerInRange();
        }
    }
}