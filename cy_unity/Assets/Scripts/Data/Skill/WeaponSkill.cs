using DH.Config;
using DH.UIFramework.Observables;

namespace DH.Data
{
    public class WeaponSkill : ObservableObject
    {
        public BackpackWeaponData backData;
        public UnitBase owner;
        public EquipModelCfg modelCfg;
        public EquipCfg equipCfg;
        public Skill SkillData { get; set; }
        
        public long WeaponUid => backData.Uid;  // 武器唯一id,前端生成
        public int EquipId => backData.EquipId;
        public int WeaponModelId => backData.WeaponId;
        
        public WeaponSkill(UnitBase unitBase, BackpackWeaponData backpackWeaponData)
        {
            owner = unitBase;
            backData = backpackWeaponData;
            modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(WeaponModelId);
            equipCfg = ConfigCenter.EquipCfgColl.GetDataById(EquipId);
            if (modelCfg != null && modelCfg.Equip != 9 && modelCfg.InitialEffect is { Count: > 0 })
            {
                SkillData = owner.skill.GetSkill(modelCfg.InitialEffect[0]);
            }
        }
        public EquipAtkType GetAtkType()
        {
            return (EquipAtkType)equipCfg.AtkType;
        }
        public EquipAttrType GetAttrType()
        {
            return (EquipAttrType)modelCfg.AttrType;
        }
    }
}