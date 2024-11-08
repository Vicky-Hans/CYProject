using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;

namespace DH.Game
{
    public partial class PlayerSkill500 : BaseSkill
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
            if (weaponSkill.WeaponModelId == 506)
            {
                var skillData = weaponSkill.SkillData;
                var loseHp = skillData.attrMgr.Calc(AttributeType.LoseHp);
                owner.resource.DecHp(Lodash.RoundToInt(loseHp));
                return;
            }
            var cdRevert = SkillData.attrMgr.Calc(AttributeType.CdRevert);
            cdRevert *= compose;
            var upRevert = SkillData.attrMgr.Calc(AttributeType.UpRevert);
            upRevert += owner.attr.Calc(AttributeType.UpRevert);
            upRevert += owner.clothesSkillAttr.Calc(AttributeType.UpRevert);
            cdRevert *= (1+upRevert);
            owner.resource.AddHp(Lodash.RoundToInt(cdRevert));
        }
    }
}