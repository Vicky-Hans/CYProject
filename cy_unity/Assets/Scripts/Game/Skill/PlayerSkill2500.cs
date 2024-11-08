using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerSkill2500 : BaseSkill
    {
        [SyncAssetPath] public string bulletPath1;
        public override async void Shoot(WeaponSkill weaponSkill)
        {
            var weaponModelId = weaponSkill.WeaponModelId;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var startPos = fightingManager.PlayerWorldPos;
            var atkNum = Lodash.RoundToInt(SkillData.attrMgr.Calc(AttributeType.AtkNum));
            var numCd = SkillData.attrMgr.Calc(AttributeType.AtkNumCd) * GameConst.TimeDivisor;
            var range = SkillData.Range();
            SkillData.SkillNum++;
            for (var i = 0; i < atkNum; i++)
            {
                var target = GetTarget(startPos, range);
                if(target == null) return;
                var targetPos = target.transform.position;
                var bullet = InstantiateObj(bulletPath1, startPos, Quaternion.identity, fightingManager.fightPanelTrans);
                var bulletComp = bullet.GetComponent<PlayerBullet2500>();
                var dmgArgs = CreateDamage();
                dmgArgs.weaponSkill = weaponSkill;
                dmgArgs.weaponModelId = weaponModelId;
                bulletComp.InitWithTarget(dmgArgs, GetConfigArgs, startPos, targetPos, this);
                if (!await PauseTask.Delay(numCd)) break;
            }
        }
        private MonsterController GetTarget(Vector3 pos, float range)
        {
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var target = fightingManager.GetRandMonsterInRange(pos, range);
            return target;
        }
    }
}