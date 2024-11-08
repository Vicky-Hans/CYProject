using Cysharp.Threading.Tasks;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerSkill200 : BaseSkill
    {
        [SyncAssetPath] public string bulletPath201;
        [SyncAssetPath] public string bulletPath202;
        [SyncAssetPath] public string bulletPath203;
        [SyncAssetPath] public string bulletPath204;
        [SyncAssetPath] public string bulletPath205;
        [SyncAssetPath] public string bulletPath206;

        public override async UniTask Init(Skill data, UnitBase unitBase, BaseMonoUnit behaviour)
        {
            await base.Init(data, unitBase, behaviour);
        }

        private void ShootInternal(WeaponSkill weaponSkill, float dmgPercent = 1f)
        {
            var weaponModelId = weaponSkill.WeaponModelId;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var startPos = fightingManager.PlayerWorldPos;
            var target = GetTarget(startPos);
            if(target == null)
            {
                return;
            }
            SkillData.SkillNum++;
            var targetPos = target.GetHurtPos();
            var tmpBulletPath = GetBulletPath(weaponModelId);
            var bullet = InstantiateObj(tmpBulletPath, startPos, Quaternion.identity, fightingManager.fightPanelTrans);
            var bulletComp = bullet.GetComponent<PlayerBullet200>();
            var dmgArgs = CreateDamage();
            dmgArgs.weaponSkill = weaponSkill;
            dmgArgs.weaponModelId = weaponModelId;
            dmgArgs.dmgPercent = dmgPercent;
            bulletComp.InitWithTarget(dmgArgs, GetConfigArgs, startPos, targetPos, this);
        }
        
        public override void Shoot(WeaponSkill weaponSkill)
        {
            ShootInternal(weaponSkill);
        }

        public void ReHitShoot(WeaponSkill weaponSkill, float dmgPercent)
        {
            ShootInternal(weaponSkill, dmgPercent);
        }
        
        private string GetBulletPath(int weaponModelId)
        {
            switch (weaponModelId)
            {
                case 201: return bulletPath201;
                case 202: return bulletPath202;
                case 203: return bulletPath203;
                case 204: return bulletPath204;
                case 205: return bulletPath205;
                case 206: return bulletPath206;
            }
            return bulletPath201;
        }
        
        private MonsterController GetTarget(Vector3 pos)
        {
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var target = fightingManager.GetRandMonster();
            return target;
        }
    }
}