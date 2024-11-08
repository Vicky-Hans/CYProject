using DH.Config;
using DH.Data;
namespace DH.Game
{
    public partial class PlayerSkill2000 : BaseSkill
    {
        public override async void Shoot(WeaponSkill weaponSkill)
        {
            var weaponModelId = weaponSkill.WeaponModelId;
            var equipModelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(weaponModelId);
            var compose = equipModelCfg.Compose[1] * GameConst.AttributeDivisor;
            var revertPer = SkillData.attrMgr.Calc(AttributeType.RevertPer);
            revertPer *= compose;
            owner.resource.AddHpPer(revertPer);
        }
    }
}