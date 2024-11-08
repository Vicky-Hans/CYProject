using DH.Data;
namespace DH.Game
{
    public partial class BuffBleed  : BaseBuff
    {
        private float dmgTime = 0;
        public override void OnUpdate(float deltaTime)
        {
            if(Recycled)return;
            if (buff == null)
            {
                Recycle();
                return;
            }
            dmgTime += deltaTime;
            if (dmgTime > buff.interval)
            {
                dmgTime = 0;
                TakeDmg();
            }
            if (!buff.IsValid(GameTime.Instance.GTime)) Recycle();
        }
        private void TakeDmg()
        {
            var monsterController = baseMonoUnit as MonsterController;
            if (monsterController == null) return;
            var dmg = (int)buff.value;
            monsterController.DecHp(dmg, buff.equipModelId);
        }
    }
}