using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Data;
using UnityEngine;
using UnityEngine.Pool;

namespace DH.Game
{
    public partial class PlayerSkill400 : BaseSkill
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
            var atkNum = Lodash.RoundToInt(SkillData.attrMgr.Calc(AttributeType.AtkNum));
            var numCd = SkillData.attrMgr.Calc(AttributeType.AtkNumCd) * GameConst.TimeDivisor;
            var targetsList = GetTarget(atkNum);
            if(targetsList == null || targetsList.Count == 0)
            {
                ListPool<MonsterController>.Release(targetsList);
                return;
            }
            var targetPoss = new List<Vector3>();
            foreach (var target in targetsList)
            {
                targetPoss.Add(target.GetHurtPos());
            }
            ListPool<MonsterController>.Release(targetsList);
            SkillData.SkillNum++;
            var tmpBulletPath = GetBulletPath(weaponModelId);
            for (int i = 0; i < atkNum; i++)
            {
                var tmpTargetPos = i < targetPoss.Count ? targetPoss[i] : targetPoss[0];
                var bullet = InstantiateObj(tmpBulletPath, startPos, Quaternion.identity, fightingManager.fightPanelTrans);
                var bulletComp = bullet.GetComponent<PlayerBullet400>();
                var dmgArgs = CreateDamage();
                dmgArgs.weaponSkill = weaponSkill;
                dmgArgs.weaponModelId = weaponModelId;
                bulletComp.InitWithTarget(dmgArgs, GetConfigArgs, startPos, tmpTargetPos, this);
                if (!await PauseTask.Delay(numCd))
                {
                    break;
                }
            }
        }
        
        public string GetBulletPath(int weaponModelId)
        {
            switch (weaponModelId)
            {
                case 401: return bulletPath1;
                case 402: return bulletPath2;
                case 403: return bulletPath3;
                case 404: return bulletPath4;
                case 405: return bulletPath5;
                case 406: return bulletPath6;
            }
            return bulletPath1;
        }
        
        private List<MonsterController> GetTarget(int count = 1)
        {
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var targets = fightingManager.GetRandMonstersInScreen(count);
            return targets;
        }
    }
}