using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;

namespace DH.Game
{
    public partial class PlayerSkill1500 : BaseSkill
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
            var revertPer = SkillData.attrMgr.Calc(AttributeType.RevertPer);
            revertPer *= compose;
            var upRevert = SkillData.attrMgr.Calc(AttributeType.UpRevert);
            var clothesUpRevert = owner.clothesSkillAttr.Calc(AttributeType.UpRevert);
            revertPer *= (1+upRevert+clothesUpRevert);
            owner.resource.AddHpPer(revertPer);
        }
    }
}