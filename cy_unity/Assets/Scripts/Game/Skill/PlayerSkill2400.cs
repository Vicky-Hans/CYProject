using Cysharp.Threading.Tasks;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerSkill2400 : BaseSkill
    {
        [SyncAssetPath] public string bulletPath1;
        [SyncAssetPath] public string bulletPath2;
        [SyncAssetPath] public string bulletPath3;
        [SyncAssetPath] public string bulletPath4;
        [SyncAssetPath] public string bulletPath5;

        public override async UniTask Init(Skill data, UnitBase unitBase, BaseMonoUnit behaviour)
        {
            await base.Init(data, unitBase, behaviour);
            SkillData.KillNum = GameDataManager.Instance.TridentKillNum;
        }
        public override async void Shoot(WeaponSkill weaponSkill)
        {
            var weaponModelId = weaponSkill.WeaponModelId;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var startPos = fightingManager.PlayerWorldPos;
            var range = SkillData.Range();
            SkillData.SkillNum++;
            var target = GetTarget(startPos, range);
            if(target == null) return;
            var targetPos = target.transform.position;
            var tmpBulletPath = GetBulletPath(weaponModelId);
            var bullet = InstantiateObj(tmpBulletPath, startPos, Quaternion.identity, fightingManager.fightPanelTrans);
            var bulletComp = bullet.GetComponent<PlayerBullet2400>();
            var dmgArgs = CreateDamage();
            dmgArgs.weaponSkill = weaponSkill;
            dmgArgs.weaponModelId = weaponModelId;
            bulletComp.InitWithTarget(dmgArgs, GetConfigArgs, startPos, targetPos, this);
        }
        private string GetBulletPath(int weaponModelId)
        {
            switch (weaponModelId)
            {
                case 2401: return bulletPath1;
                case 2402: return bulletPath2;
                case 2403: return bulletPath3;
                case 2404: return bulletPath4;
                case 2405: return bulletPath5;
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