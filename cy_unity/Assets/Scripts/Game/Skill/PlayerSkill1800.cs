using DH.Data;
using UnityEngine;
namespace DH.Game
{
    public partial class PlayerSkill1800  : BaseSkill
    {
        [SyncAssetPath] public string bulletPath1;
        [SyncAssetPath] public string bulletPath2;
        [SyncAssetPath] public string bulletPath3;
        [SyncAssetPath] public string bulletPath4;
        public override async void Shoot(WeaponSkill weaponSkill)
        {
            var weaponModelId = weaponSkill.WeaponModelId;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var startPos = fightingManager.PlayerWorldPos;
            var range = SkillData.Range();
            var target = GetTarget(startPos,range);
            if(target == null) return;
            SkillData.SkillNum++;
            var targetPos = target.GetHurtPos();
            var tmpBulletPath = GetBulletPath(weaponModelId);
            var bullet = InstantiateObj(tmpBulletPath, startPos, Quaternion.identity, fightingManager.fightPanelTrans);
            var bulletComp = bullet.GetComponent<PlayerBullet1800>();
            var dmgArgs = CreateDamage();
            dmgArgs.weaponSkill = weaponSkill;
            dmgArgs.weaponModelId = weaponModelId;
            bulletComp.InitWithTarget(dmgArgs, GetConfigArgs, startPos, targetPos, this);
        }
        
        private string GetBulletPath(int weaponModelId)
        {
            switch (weaponModelId)
            {
                case 1801: return bulletPath1;
                case 1802: return bulletPath2;
                case 1803: return bulletPath3;
                case 1804: return bulletPath4;
            }
            return bulletPath1;
        }
        
        private MonsterController GetTarget(Vector3 pos,float range)
        {
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var target = fightingManager.GetRandMonsterInRange(pos, range);
            return target;
        }
    }
}