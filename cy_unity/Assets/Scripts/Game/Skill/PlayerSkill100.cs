using Cysharp.Threading.Tasks;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerSkill100 : BaseSkill
    {
        [SyncAssetPath] public string bulletPath101;
        [SyncAssetPath] public string bulletPath102;
        [SyncAssetPath] public string bulletPath103;
        [SyncAssetPath] public string bulletPath104;
        [SyncAssetPath] public string bulletPath105;
        [SyncAssetPath] public string bulletPath106;

        public override async UniTask Init(Skill data, UnitBase unitBase, BaseMonoUnit behaviour)
        {
            await base.Init(data, unitBase, behaviour);
        }

        public override void Shoot(WeaponSkill weaponSkill)
        {
            int weaponModelId = weaponSkill.WeaponModelId;
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
            var bulletComp = bullet.GetComponent<PlayerBullet100>();
            var dmgArgs = CreateDamage();
            dmgArgs.weaponSkill = weaponSkill;
            dmgArgs.weaponModelId = weaponModelId;
            bulletComp.InitWithTarget(dmgArgs, GetConfigArgs, startPos, targetPos, this);
        }
        
        private string GetBulletPath(int weaponModelId)
        {
            switch (weaponModelId)
            {
                case 101: return bulletPath101;
                case 102: return bulletPath102;
                case 103: return bulletPath103;
                case 104: return bulletPath104;
                case 105: return bulletPath105;
                case 106: return bulletPath106;
            }
            return bulletPath101;
        }
        
        private MonsterController GetTarget(Vector3 pos)
        {
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var target = fightingManager.GetNearestMonster(pos);
            return target;
        }
    }
}