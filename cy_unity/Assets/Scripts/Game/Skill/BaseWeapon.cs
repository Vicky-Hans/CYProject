using DH.Config;
using DH.Data;

namespace DH.Game
{
    public class BaseWeapon
    {
        public WeaponSkill weaponData;
        private float timer;
        private float cd;
        private bool inited = false;
        private const float CheckTargetCd = 0.1f;
        private float checkTargetTimer;
        public float Progress => timer / cd;
        private BaseSkill skillIns;

        public void Init(BaseSkill skill, WeaponSkill wData)
        {
            weaponData = wData;
            skillIns = skill;
            CheckWeaponSkill();
            timer = 0f;
            cd = weaponData.SkillData.AttackInterval(wData);
            inited = true;
        }

        private void CheckWeaponSkill()
        {
            if (weaponData.WeaponModelId % 10 is not (5 or 6)) return;
            var equipSkillId = weaponData.modelCfg.Effect;
            if (equipSkillId <= 0) return;
            var equipSkillCfg = ConfigCenter.EquipSkillCfgColl.GetDataById(equipSkillId);
            if (equipSkillCfg == null) return;
            var skillId = equipSkillCfg.SkillId;
            if (skillId <= 0) return;
            var skillCfg = ConfigCenter.SkillCfgColl.GetDataById(skillId);
            if (skillCfg == null) return;
            skillIns.MonoOwner.Player.AddWeaponAttr(skillIns.SkillData, skillCfg);
        }
        public void OnUpdate(float deltaTime)
        {
            if(!inited)return;
            checkTargetTimer += deltaTime;
            timer += deltaTime;
            if (!(timer >= cd)) return;
            timer = 0f;
            cd = weaponData.SkillData.AttackInterval(weaponData);
            Shoot();
        }
        public bool CheckTarget()
        {
            if (checkTargetTimer < CheckTargetCd) return false;
            checkTargetTimer = 0;
            return skillIns.CheckTargetInRange();
        }

        public void Shoot()
        {
            if(skillIns == null) return;
            if(weaponData.GetAtkType() is EquipAtkType.Physic or EquipAtkType.Magic)
            {
                if (!CheckTarget()) return;
                if (weaponData.modelCfg.Equip != 5 && weaponData.modelCfg.Equip != 6)
                {
                    var playerCtrl = BattleManager.Instance.fightingManagerIns.playerCtrl;
                    playerCtrl.PlayAtk();
                }
                skillIns.Shoot(weaponData);
                FightingSoundHelper.Instance.PlayWeaponLaunch(weaponData.WeaponModelId);
            }
            else
            {
                skillIns.Shoot(weaponData);
            }
        }
    }
}