using Cysharp.Threading.Tasks;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class BossSkill51003 : BaseSkill
    {
        [SyncAssetPath] public string bulletPath;
        private MonsterController monsterController;
        private Skill skillData;
        
        public override async UniTask Init(Skill data, UnitBase unitBase, BaseMonoUnit behaviour)
        {
            await base.Init(data, unitBase, behaviour);
            monsterController = behaviour as MonsterController;
            skillData = data;
        }
        
        public override void TakeSkill()
        {
            CurrentState = State.Taking;
            Shoot(null);
        }

        public override void Shoot(WeaponSkill weaponSkill)
        {
            CurrentState = State.Idle;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var startPosition = monsterController.transform.position;
            var targetPosition = fightingManager.playerCtrl.transform.position;
            var num = Lodash.RoundToInt(skillData.attrMgr.Calc(AttributeType.Num));
            var spliAngle = 360f / num;
            var tmpBulletPath = bulletPath;
            var direction = targetPosition - startPosition;
            var startAngle = Lodash.Direction2Angle(direction);
            for (int i = 0; i < num; i++)
            {
                var bulletAngle = startAngle + i * spliAngle;
                var bulletDirection = Lodash.Angle2Direction(bulletAngle);
                var bullet = InstantiateObj(tmpBulletPath, startPosition, Quaternion.identity,
                    fightingManager.fightPanelTrans);
                var bulletComp = bullet.GetComponent<EnemyBulletComm>();
                var dmgArgs = CreateDamage();
                bulletComp.InitWithTarget(dmgArgs, GetConfigArgs, startPosition, startPosition+bulletDirection, this);
            }
        }
    }
}