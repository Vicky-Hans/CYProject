using Cysharp.Threading.Tasks;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerSkill1300 : BaseSkill
    {
        [SyncAssetPath] public string bulletPath1;
        [SyncAssetPath] public string bulletPath2;
        [SyncAssetPath] public string bulletPath3;
        [SyncAssetPath] public string bulletPath4;
        [SyncAssetPath] public string bulletPath5;
        [SyncAssetPath] public string bulletPath6;

        public override async UniTask Init(Skill data, UnitBase unitBase, BaseMonoUnit behaviour)
        {
            await base.Init(data, unitBase, behaviour);
        }

        public override void Shoot(WeaponSkill weaponSkill)
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
            var bulletComp = bullet.GetComponent<PlayerBullet1300>();
            var dmgArgs = CreateDamage();
            dmgArgs.weaponSkill = weaponSkill;
            dmgArgs.weaponModelId = weaponModelId;
            bulletComp.InitWithTarget(dmgArgs, GetConfigArgs, startPos, targetPos, this);
        }
        
        private string GetBulletPath(int weaponModelId)
        {
            switch (weaponModelId)
            {
                case 1301: return bulletPath1;
                case 1302: return bulletPath2;
                case 1303: return bulletPath3;
                case 1304: return bulletPath4;
                case 1305: return bulletPath5;
                case 1306: return bulletPath6;
            }
            return bulletPath1;
        }
        
        private MonsterController GetTarget(Vector3 pos)
        {
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var target = fightingManager.GetNearestMonster(pos);
            return target;
        }
    }
}