using Cysharp.Threading.Tasks;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class BossSkill51001 : BaseSkill
    {
        [SyncAssetPath] public string bulletPath;
        private MonsterController monsterController;
        
        
        public override async UniTask Init(Skill data, UnitBase unitBase, BaseMonoUnit behaviour)
        {
            await base.Init(data, unitBase, behaviour);
            monsterController = behaviour as MonsterController;
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
            var startPos = monsterController.transform.position;
            
            var targetPos = startPos;
            var tmpBulletPath = bulletPath;
            var bullet = InstantiateObj(tmpBulletPath, startPos, Quaternion.identity, fightingManager.fightPanelTrans);
            var bulletComp = bullet.GetComponent<BossSkill51001Bullet>();
            bulletComp.MonsterController = monsterController;
            // bulletComp.SkillIns = this;
            var dmgArgs = CreateDamage();
            bulletComp.InitWithTarget(dmgArgs, GetConfigArgs, startPos, targetPos, this);
        }
        
    }
}