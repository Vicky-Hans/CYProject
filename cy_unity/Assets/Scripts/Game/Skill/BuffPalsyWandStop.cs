using DH.Data;
using UnityEngine;
namespace DH.Game
{
    public partial class BuffPalsyWandStop : BaseBuff
    {
        private MonsterController targetMonster;
        private SkillTrigger palsyWandStopEndTrigger;
        public override void InitWithTarget(BaseMonoUnit unit, Buff buffParam, IPool<GameObject> pool)
        {
            base.InitWithTarget(unit, buffParam, pool);
            targetMonster = unit as MonsterController;
            palsyWandStopEndTrigger = buffParam.skillData.GetTriggerByNameAndComplete(AttributeName.PalsyWandStopEnd, AttributeType.Decelerate);
        }
        public override void OnUpdate(float deltaTime)
        {
            if(Recycled)return;
            if (buff == null)
            {
                Recycle();
                return;
            }
            time += deltaTime;
            if (buff.IsValid(GameTime.Instance.GTime)) return;
            if (palsyWandStopEndTrigger != null) AddDecelerateAndDownAtkSpd();
            Recycle();
        }
        private void AddDecelerateAndDownAtkSpd()
        {
            var downSpd = palsyWandStopEndTrigger.attrMgr.Calc(AttributeType.DownSpd);
            var downAtkSpd = palsyWandStopEndTrigger.attrMgr.Calc(AttributeType.DownAtkSpd);
            var decelerateTime = palsyWandStopEndTrigger.attrMgr.Calc(AttributeType.DecelerateTime)*GameConst.TimeDivisor;
            var downAtkSpdTime = palsyWandStopEndTrigger.attrMgr.Calc(AttributeType.DownAtkSpdTime)*GameConst.TimeDivisor;
            if (targetMonster != null) targetMonster.Data.buffMgr.AddBuff(new Buff
            {
                id = (int)AttributeType.DownAtkSpd,
                attrName = AttributeName.DownAtkSpd,
                startTime = GameTime.Instance.GTime,
                duration = downAtkSpdTime,interval = 0,
                value = downAtkSpd,multi = true,
                valueType = BuffValueType.Negative
            });
            var buffDownSpd = new Buff
            {
                id = (int)AttributeType.Decelerate,
                attrName = AttributeName.Decelerate,
                startTime = GameTime.Instance.GTime,
                duration = decelerateTime,interval = 0,
                value = downSpd,multi = true,
                valueType = BuffValueType.Negative
            };
            if (targetMonster != null) targetMonster.AddDecelerateBuff(buffDownSpd);
        }
    }
}