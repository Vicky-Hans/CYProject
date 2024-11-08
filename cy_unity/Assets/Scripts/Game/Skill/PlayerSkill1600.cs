using Cysharp.Threading.Tasks;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerSkill1600 : BaseSkill
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

        public override async void Shoot(WeaponSkill weaponSkill)
        {
            var weaponModelId = weaponSkill.WeaponModelId;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var startPos = fightingManager.PlayerWorldPos;
            var range = SkillData.Range();
            var target = GetTarget(startPos, range);
            if(target == null)
            {
                return;
            }
            SkillData.SkillNum++;
            var targetPos = target.GetHurtPos();
            var tmpBulletPath = GetBulletPath(weaponModelId);
            var bullet = InstantiateObj(tmpBulletPath, startPos, Quaternion.identity, fightingManager.fightPanelTrans);
            var bulletComp = bullet.GetComponent<PlayerBullet1600>();
            var dmgArgs = CreateDamage();
            dmgArgs.weaponSkill = weaponSkill;
            dmgArgs.weaponModelId = weaponModelId;
            bulletComp.InitWithTarget(dmgArgs, GetConfigArgs, startPos, targetPos, this);
        }
        
        private string GetBulletPath(int weaponModelId)
        {
            switch (weaponModelId)
            {
                case 1601: return bulletPath1;
                case 1602: return bulletPath2;
                case 1603: return bulletPath3;
                case 1604: return bulletPath4;
                case 1605: return bulletPath5;
                case 1606: return bulletPath6;
            }
            return bulletPath1;
        }
        
        private MonsterController GetTarget(Vector3 pos, float range)
        {
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var target = fightingManager.GetRandMonsterInRange(pos, range);
            return target;
        }
    }
}