using DH.Config;
using DH.Data;
namespace DH.Game
{
    public partial class PlayerSkill1900 : BaseSkill
    {
        public override async void Shoot(WeaponSkill weaponSkill)
        {
            var weaponModelId = weaponSkill.WeaponModelId;
            var equipModelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(weaponModelId);
            var compose = equipModelCfg.Compose[1] * GameConst.AttributeDivisor;
            var armorPer = SkillData.attrMgr.Calc(AttributeType.ArmorPer);
            armorPer *= compose;
            var upArmor = SkillData.attrMgr.Calc(AttributeType.UpArmor);
            upArmor += owner.attr.Calc(AttributeType.UpArmor);
            armorPer *= (1+upArmor);
            owner.resource.AddArmorPer(armorPer);
        }
    }
}