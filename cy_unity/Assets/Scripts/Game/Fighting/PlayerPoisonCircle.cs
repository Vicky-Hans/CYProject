using DH.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace DH.Game
{
    public sealed class PlayerPoisonCircle : BaseAssetEntity,IGamePlayElement
    {
        private IPool<GameObject> poisonPool;
        private Buff basebuff;
        private float dmgTime = 0;
        private float poisonRange;//毒圈范围
        public GameObject poisonCircleEffectObj;
        private UnitBase playerData { get; set; }
        private bool recycled;
        public bool Recycled => recycled;
        public void InitWithTarget(UnitBase data,Buff buff,float range,IPool<GameObject> pool)
        {
            poisonPool = pool;
            basebuff = buff;
            poisonRange = range;
            poisonCircleEffectObj.transform.localScale = Vector3.one* (range / 4.5f);
            playerData = data;
            BattleManager.Instance.fightingManagerIns.gamePlayManager.AddElement(this);
        }
        public void OnUpdate(float elapseSeconds)
        {
            if(Recycled)return;
            if (basebuff == null)
            {
                Recycle();
                return;
            }
            dmgTime += elapseSeconds;
            if (dmgTime > basebuff.interval)
            {
                dmgTime = 0;
                TakeDmg();
            }
            if (!basebuff.IsValid(GameTime.Instance.GTime)) Recycle();
        }
        private void TakeDmg()
        {
            var monsterList = BattleManager.Instance.fightingManagerIns.GetRandMonstersInRange(transform.position,poisonRange);
            if (monsterList.Count == 0) 
            {
                ListPool<MonsterController>.Release(monsterList);
                return;
            }
            var poisonTrigger = playerData.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.PassPoisonCircle, AttributeType.Poison);
            var isAddPoisonTrigger = false;
            if (poisonTrigger != null)
            {
                poisonTrigger.trigger.TryGetValue(AttributeName.Prob,out var propValue);
                if (Lodash.RandRangeFloat(0, 1f) <= propValue * GameConst.AttributeDivisor) isAddPoisonTrigger = true;
            }
            foreach (var monsterCtrl in monsterList)
            {
                monsterCtrl.DecHp((int)basebuff.value);
                if (poisonTrigger != null && isAddPoisonTrigger)
                {
                    var poisonBuffs = monsterCtrl.Data.buffMgr.FindBuffsById((int)AttributeType.Poison);
                    if (poisonBuffs is { Count: > 0 }) continue;
                    var poisonTime = poisonTrigger.attrMgr.Calc(AttributeType.PoisonTime)*GameConst.TimeDivisor;
                    var poisonDmgProp = poisonTrigger.attrMgr.Calc(AttributeType.PoisonDmgSec);
                    var poisonDmg = playerData.attr.Calc(AttributeType.Atk) * poisonDmgProp;
                    var buff = new Buff
                    {
                        id = (int)AttributeType.Poison,
                        attrName = AttributeName.Poison,
                        startTime = GameTime.Instance.GTime,
                        duration = poisonTime,
                        interval = 1,
                        value = poisonDmg,
                        valueType = BuffValueType.Negative,
                        multi = true,
                        clothesId = 2,
                    };
                    monsterCtrl.AddPoisonBuff(buff);
                }
            }
            ListPool<MonsterController>.Release(monsterList);
        }
        public void ForceDestroy()
        {
            Recycle();
        }

        private void Recycle()
        {
            if (recycled) return;
            recycled = true;
            var figManager = BattleManager.Instance.fightingManagerIns;
            figManager.gamePlayManager.RemoveElement(this);
            if (gameObject != null)
            {
                if (poisonPool != null)
                {
                    poisonPool.ReleaseObj(gameObject);
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