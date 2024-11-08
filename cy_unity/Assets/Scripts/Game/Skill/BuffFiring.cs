using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class BuffFiring : BaseBuff
    {
        private float interval { get; set; }
        private float dmgTime = 0;
        public bool IsPlayer { get; set; }
        
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
        private void TakeDmg()
        {
            if (IsPlayer)
            {
                var player = baseMonoUnit as CharacterController;
                if (player == null) return;
                var dmg = (int)buff.value;
                player.DecArmor(dmg);
            }
            else
            {
                var monsterController = baseMonoUnit as MonsterController;
                if (monsterController == null) return;
                var firingDmgDown =
                    monsterController.MonsterData.attr.Calc(AttributeType.FiringDmgDown);
                var dmg = (int)buff.value;
                dmg = (int) (dmg * (1 - firingDmgDown));
                monsterController.DecHp(dmg, buff.equipModelId);
            }
        }
    }
}