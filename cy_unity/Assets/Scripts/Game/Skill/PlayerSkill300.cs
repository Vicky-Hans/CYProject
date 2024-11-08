using Cysharp.Threading.Tasks;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class PlayerSkill300 : BaseSkill
    {
        [SyncAssetPath] public string bulletPath1;
        [SyncAssetPath] public string bulletPath2;
        [SyncAssetPath] public string bulletPath3;
        [SyncAssetPath] public string bulletPath4;
        [SyncAssetPath] public string bulletPath5;
        [SyncAssetPath] public string bulletPath6;
        [SyncAssetPath] public string rainBulletPath;

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
            var num = Lodash.RoundToInt(SkillData.attrMgr.Calc(AttributeType.Num));
            var spliAngle = SkillData.attrMgr.Calc(AttributeType.SpliAngle);
            var direction = targetPos - startPos;
            var angle = Lodash.Direction2Angle(direction);
            var factorX = num % 2 == 0 ? 2 : 1;
            var startAngle = angle - Mathf.FloorToInt((num- factorX)/2) * spliAngle;
            var tmpBulletPath = GetBulletPath(weaponModelId);
            for (int i = 0; i < num; i++)
            {
                var bulletAngle = startAngle + spliAngle * i;
                var bulletDirection = Lodash.Angle2Direction(bulletAngle);
                var tmpTargetPos = startPos + bulletDirection * 10f;
                var bullet = InstantiateObj(tmpBulletPath, startPos, Quaternion.identity, fightingManager.fightPanelTrans);
                var bulletComp = bullet.GetComponent<PlayerBullet300>();
                var dmgArgs = CreateDamage();
                dmgArgs.weaponSkill = weaponSkill;
                dmgArgs.weaponModelId = weaponModelId;
                bulletComp.InitWithTarget(dmgArgs, GetConfigArgs, startPos, tmpTargetPos, this);
            }
            ShootRain(weaponModelId, weaponSkill);
        }

        /// <summary>
        /// 发射箭雨
        /// </summary>
        private void ShootRain(int weaponModelId, WeaponSkill weaponSkill)
        {
            if(weaponModelId != 305)return;
            var target = GetTarget(cacheTransform.position);
            if(target == null)return;
            var trigger = SkillData.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.EnableRangeDmg);
            if(trigger == null)return;
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var startPos = target.transform.position;
            trigger.trigger.TryGetValue("prob", out int probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            prob += SkillData.attrMgr.Calc(AttributeType.BowProb);
            if(Lodash.RandRangeFloat(0, 1) > prob)return;
            var tmpBulletPath = rainBulletPath;
            var bullet = InstantiateObj(tmpBulletPath, startPos, Quaternion.identity, fightingManager.fightPanelTrans);
            var bulletComp = bullet.GetComponent<PlayerBullet300Rain>();
            var dmgArgs = CreateDamage();
            dmgArgs.weaponSkill = weaponSkill;
            dmgArgs.weaponModelId = weaponModelId;
            bulletComp.InitWithTarget(dmgArgs, GetConfigArgs, startPos, startPos, this);
        }
        
        private string GetBulletPath(int weaponModelId)
        {
            switch (weaponModelId)
            {
                case 301: return bulletPath1;
                case 302: return bulletPath2;
                case 303: return bulletPath3;
                case 304: return bulletPath4;
                case 305: return bulletPath5;
                case 306: return bulletPath6;
            }
            return bulletPath1;
        }
        
        private MonsterController GetTarget(Vector3 pos)
        {
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var target = fightingManager.GetRandMonster();
            return target;
        }
    }
}