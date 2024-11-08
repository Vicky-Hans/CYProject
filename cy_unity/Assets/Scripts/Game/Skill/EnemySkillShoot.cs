using Cysharp.Threading.Tasks;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class EnemySkillShoot : BaseSkill
    {
        [SyncAssetPath] public string bulletPath;
        private MonsterController monsterController;
        private Transform skillPointTrans;
        private readonly Vector3 targetOffset = Vector3.up;

        public override async UniTask Init(Skill data, UnitBase unitBase, BaseMonoUnit behaviour)
        {
            await base.Init(data, unitBase, behaviour);
            monsterController = behaviour as MonsterController;
            if (monsterController != null)
            {
                skillPointTrans = monsterController.transform.Find("SkillPoint");
            }
        }
        
        public override void TakeSkill()
        {
            CurrentState = State.Taking;
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            if (currentState != State.Taking) return;
            Shoot(null);
        }
        
        public override void Shoot(WeaponSkill weaponSkill)
        {
            CurrentState = State.Idle;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var startPos = skillPointTrans?skillPointTrans.position:monsterController.transform.position;
            var target = GetTarget(startPos);
            if(target == null)
            {
                return;
            }
            var targetPos = target.transform.position + targetOffset;
            var tmpBulletPath = GetBulletPath();
            var bullet = InstantiateObj(tmpBulletPath, startPos, Quaternion.identity, fightingManager.fightPanelTrans);
            var bulletComp = bullet.GetComponent<EnemyBulletComm>();
            var dmgArgs = CreateDamage();
            bulletComp.InitWithTarget(dmgArgs, GetConfigArgs, startPos, targetPos, this);
        }
        
        private string GetBulletPath()
        {
            return bulletPath;
        }
        
        private CharacterController GetTarget(Vector3 pos)
        {
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var target = fightingManager.playerCtrl;
            return target;
        }
    }
}