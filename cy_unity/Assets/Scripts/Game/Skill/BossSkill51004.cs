using Cysharp.Threading.Tasks;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class BossSkill51004 : BaseSkill
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

        public override async void Shoot(WeaponSkill weaponSkill)
        {
            CurrentState = State.Idle;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var num = Lodash.RoundToInt(skillData.attrMgr.Calc(AttributeType.Num));
            var numCd = Lodash.RoundToInt(skillData.attrMgr.Calc(AttributeType.AtkNumCd)) * GameConst.TimeDivisor;
            var tmpBulletPath = bulletPath;
            for (int i = 0; i < num; i++)
            {
                var startPosition = monsterController.transform.position;
                var targetPosition = fightingManager.playerCtrl.transform.position;
                var bullet = InstantiateObj(tmpBulletPath, startPosition, Quaternion.identity,
                    fightingManager.fightPanelTrans);
                var bulletComp = bullet.GetComponent<BossTraceBullet>();
                bulletComp.TargetTrans = fightingManager.playerCtrl.transform;
                var dmgArgs = CreateDamage();
                bulletComp.InitWithTarget(dmgArgs, GetConfigArgs, startPosition, targetPosition, this);
                if (!await PauseTask.Delay(numCd))
                {
                    break;
                }
            }
        }
    }
}