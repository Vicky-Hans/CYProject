using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public class PlayerSandCircle : BaseAssetEntity,IGamePlayElement
    {
        private float hpDmg = 0f;//飞沙阵血量降低百分比
        private float downSpd = 0f;//移速减少比例
        private float extraDmg = 0f;//额外伤害值
        private float startTime = 0f;//进行时间
        private float durationTime = 0f;//总的持续时间
        private float decelerateTime = 0f;//移速减少持续时间
        private IPool<GameObject> sandCirclePool;
        public bool Recycled { get; private set; }
        public void InitWithTarget(UnitBase data,float range,IPool<GameObject> pool)
        {
            sandCirclePool = pool;
            transform.localScale = Vector3.one* (range / 6f);
            hpDmg = data.attr.Calc(AttributeType.HpDmg);
            var passSandCircleTrigger = data.triggerSkill.GetTriggerByNameAndComplete(AttributeName.PassSandCircle, AttributeType.HpDmg);
            if (passSandCircleTrigger != null)
            {
                hpDmg += passSandCircleTrigger.attrMgr.Calc(AttributeType.HpDmg);
                downSpd = passSandCircleTrigger.attrMgr.Calc(AttributeType.DownSpd);
                decelerateTime = passSandCircleTrigger.attrMgr.Calc(AttributeType.DecelerateTime)* GameConst.TimeDivisor;
            }
            var hitCircleTargetTrigger = data.triggerSkill.GetTriggerByNameAndComplete(AttributeName.HitCircleTarget, AttributeType.ExtraDmg);
            if (hitCircleTargetTrigger != null) extraDmg = hitCircleTargetTrigger.attrMgr.Calc(AttributeType.ExtraDmg);
            durationTime = data.attr.Calc(AttributeType.SandCircleTime) * GameConst.TimeDivisor;
            BattleManager.Instance.fightingManagerIns.gamePlayManager.AddElement(this);
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            var monsterObj = other.GetComponent<MonsterController>();
            if (monsterObj == null || monsterObj.Data.IsDead()) return;
            monsterObj.DecHpValue(Lodash.RoundToInt(monsterObj.Data.resource.Hp*hpDmg));//立即减少15%最大血量
            if (monsterObj.Data.IsDead()) return;
            if (decelerateTime  > 0 && downSpd > 0)
            {
                var buff = new Buff
                {
                    duration = decelerateTime,
                    id = (int)AttributeType.Decelerate,
                    attrName = AttributeName.Decelerate,
                    startTime = GameTime.Instance.GTime,
                    valueType = BuffValueType.Negative,
                    interval = 0, value = downSpd,multi = true
                };
                monsterObj.AddDecelerateBuff(buff);
            }
            if (!(extraDmg > 0)) return; 
            //挂载飞沙阵buff，造成额外伤害
            monsterObj.Data.buffMgr.AddBuff(new Buff
            {
                duration = 3600f,
                id = (int)AttributeType.PassSandCircle,
                attrName = AttributeName.PassSandCircle,
                startTime = GameTime.Instance.GTime,
                valueType = BuffValueType.Negative,
                interval = 0, value = extraDmg, multi = true
            });
        }
        /// <summary>
        /// 离开飞沙阵区域
        /// </summary>
        /// <param name="other"></param>
        private void OnCollisionExit2D(Collision2D other)
        {
            var monsterObj = other.gameObject.GetComponent<MonsterController>();
            if (monsterObj == null || monsterObj.Data.IsDead()) return;
            var buffList = monsterObj.Data.buffMgr.FindBuffsById((int)AttributeType.PoisonPassSecDmg);
            if (buffList is not { Count: > 0 }) return;
            foreach (var buffItem in buffList)
            {
                monsterObj.Data.buffMgr.RemoveBuff(buffItem);
            }
        }
        public void OnUpdate(float elapseSeconds)
        {
            if(Recycled)return;
            startTime += elapseSeconds;
            if (startTime > durationTime) Recycle();
        }
        public void ForceDestroy()
        {
            Recycle();
        }
        private void Recycle()
        {
            if (Recycled) return;
            Recycled = true;
            BattleManager.Instance.fightingManagerIns.gamePlayManager.RemoveElement(this);
            if (gameObject != null)
            {
                if (sandCirclePool != null)
                {
                    sandCirclePool.ReleaseObj(gameObject);
                }
                else
                {
                    ReleaseObj(gameObject);
                }
            }
            ReleaseAssets();
        }
    }
}