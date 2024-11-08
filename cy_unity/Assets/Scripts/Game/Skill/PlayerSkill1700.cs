using DH.Data;
using DHFramework;
using UnityEngine;
namespace DH.Game
{
    public partial class PlayerSkill1700  : BaseSkill
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
            var target = GetTarget(startPos, range);
            if(target == null) return;
            SkillData.SkillNum++;
            var targetPos = target.GetHurtPos();
            var minNum = Lodash.RoundToInt(SkillData.attrMgr.Calc(AttributeType.MinNum));
            var maxNum = Lodash.RoundToInt(SkillData.attrMgr.Calc(AttributeType.MaxNum));
            var num = Lodash.RandRange(minNum, maxNum+1);
            if (num <= 0) num = 1;
            var spliAngle = SkillData.attrMgr.Calc(AttributeType.SpliAngle);
            var direction = targetPos - startPos;
            var angle = Lodash.Direction2Angle(direction);
            var factorX = num % 2 == 0 ? 2 : 1;
            var startAngle = angle -  Lodash.RoundToInt((num- factorX)*0.5f) * spliAngle;
            var tmpBulletPath = GetBulletPath(weaponModelId);
            for (int i = 0; i < num; i++)
            {
                var bulletAngle = startAngle + spliAngle * i;
                var bulletDirection = Lodash.Angle2Direction(bulletAngle);
                var tmpTargetPos = startPos + bulletDirection * 10f;
                var bullet = InstantiateObj(tmpBulletPath, startPos, Quaternion.identity, fightingManager.fightPanelTrans);
                var bulletComp = bullet.GetComponent<PlayerBullet1700>();
                var dmgArgs = CreateDamage();
                dmgArgs.weaponSkill = weaponSkill;
                dmgArgs.weaponModelId = weaponModelId;
                bulletComp.InitWithTarget(dmgArgs, GetConfigArgs, startPos, tmpTargetPos, this);
            }
        }
        private string GetBulletPath(int weaponModelId)
        {
            switch (weaponModelId)
            {
                case 1701: return bulletPath1;
                case 1702: return bulletPath2;
                case 1703: return bulletPath3;
                case 1704: return bulletPath4;
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