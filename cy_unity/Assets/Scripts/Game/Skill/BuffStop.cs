using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class BuffStop : BaseBuff
    {
        [AssetPath] public string endFxPath;
        private float interval { get; set; }
        private float dmgTime = 0;
        public override void InitWithTarget(BaseMonoUnit unit, Buff buff, IPool<GameObject> pool)
        {
            base.InitWithTarget(unit, buff, pool);
            interval = buff.interval;
            dmgTime = 0;
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
            dmgTime += deltaTime;
            if (dmgTime > interval)
            {
                dmgTime = 0;
                TakeDmg();
            }
            if (time > buff.duration) Recycle();
        }
        private void PlayEndFx()
        {
            var fxPos = transform.position;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var obj = InstantiateObj(endFxPath, fxPos, Quaternion.identity, fightingManager.fightPanelTrans);
            if(obj!=null)
            {
                fightingManager.AddAutoReleaseUnit(obj, 2, this);
            }
        }
        
        private void TakeDmg()
        {
            var monsterController = baseMonoUnit as MonsterController;
            if (monsterController == null) return;
            var dmg = (int)buff.value;
            monsterController.DecHp(dmg);
        }

        public override void Recycle()
        {
            PlayEndFx();
            base.Recycle();
        }
    }
}