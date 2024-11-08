using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerSkill1100 : BaseSkill
    {
        [SyncAssetPath] public string bulletPath1;
        [SyncAssetPath] public string bulletPath2;
        [SyncAssetPath] public string bulletPath3;
        [SyncAssetPath] public string bulletPath4;
        [SyncAssetPath] public string bulletPath5;
        [SyncAssetPath] public string bulletPath6;
        public override async void Shoot(WeaponSkill weaponSkill)
        {
            int weaponModelId = weaponSkill.WeaponModelId;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var startPos = fightingManager.PlayerWorldPos;
            var atkNum = Lodash.RoundToInt(SkillData.attrMgr.Calc(AttributeType.AtkNum));
            var numCd = SkillData.attrMgr.Calc(AttributeType.AtkNumCd) * GameConst.TimeDivisor;
            var range = SkillData.Range();
            SkillData.SkillNum++;
            bool canReturn = SkillData.SkillNum % GetReturnNum() == 0;
            for(var i=0; i<atkNum; i++)
            {
                var target = GetTarget(startPos, range);
                if(target == null)
                {
                    return;
                }
                var targetPos = target.GetHurtPos();
                var tmpBulletPath = GetBulletPath(weaponModelId);
                var bullet = InstantiateObj(tmpBulletPath, startPos, Quaternion.identity, fightingManager.fightPanelTrans);
                var bulletComp = bullet.GetComponent<PlayerBullet1100>();
                bulletComp.CanReturn = canReturn;
                var dmgArgs = CreateDamage();
                dmgArgs.weaponSkill = weaponSkill;
                dmgArgs.weaponModelId = weaponModelId;
                bulletComp.InitWithTarget(dmgArgs, GetConfigArgs, startPos, targetPos, this);
                
                if (!await PauseTask.Delay(numCd))
                {
                    break;
                }
            }
            
        }

        private int GetReturnNum()
        {
            var trigger = SkillData.GetTriggerByNameAndComplete(AttributeName.SkillNum, AttributeType.EnableCircle);
            if (trigger == null)
            {
                return 99999;
            }
            trigger.trigger.TryGetValue(AttributeName.SkillNum, out var skillNum);
            var decSkillNum = Lodash.RoundToInt(SkillData.attrMgr.Calc(AttributeType.DecSkillNum));
            return skillNum - decSkillNum;
        }
        
        private string GetBulletPath(int weaponModelId)
        {
            switch (weaponModelId)
            {
                case 1101: return bulletPath1;
                case 1102: return bulletPath2;
                case 1103: return bulletPath3;
                case 1104: return bulletPath4;
                case 1105: return bulletPath5;
                case 1106: return bulletPath6;
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