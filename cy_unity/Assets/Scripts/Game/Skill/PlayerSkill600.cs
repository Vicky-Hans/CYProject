using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;

namespace DH.Game
{
    public partial class PlayerSkill600 : BaseSkill
    {

        public override async UniTask Init(Skill data, UnitBase unitBase, BaseMonoUnit behaviour)
        {
            await base.Init(data, unitBase, behaviour);
        }

        public override async void Shoot(WeaponSkill weaponSkill)
        {
            var weaponModelId = weaponSkill.WeaponModelId;
            var equipModelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(weaponModelId);
            var compose = equipModelCfg.Compose[1] * GameConst.AttributeDivisor;
            var cdArmor = SkillData.attrMgr.Calc(AttributeType.CdArmor);
            cdArmor *= compose;
            var upArmor = SkillData.attrMgr.Calc(AttributeType.UpArmor);
            upArmor += SkillData.GetAboutArmor(weaponSkill);
            upArmor += SkillData.GetPeerArmor(weaponSkill);
            upArmor += owner.attr.Calc(AttributeType.UpArmor);
            cdArmor *= (1+upArmor);
            owner.resource.AddArmor(Lodash.RoundToInt(cdArmor));
        }
        
    }
}