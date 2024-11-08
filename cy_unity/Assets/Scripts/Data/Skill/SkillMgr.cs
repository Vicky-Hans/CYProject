using System;
using System.Linq;
using DH.Config;
using DH.UIFramework.Observables;
namespace DH.Data
{
    /// <summary>
    /// 数据层技能管理
    /// </summary>
    public class SkillMgr
    {
        private UnitBase owner;
        public readonly ObservableList<Skill> AutoSkills;
        public SkillMgr(UnitBase owner)
        {
            this.owner = owner;
            AutoSkills = new ObservableList<Skill>();
        }
        public void AddSkill(int id, SkillType skillType = SkillType.Auto, long lv = 0)
        {
            switch (skillType)
            {
                case SkillType.Auto:
                    if (GetSkill(id) != null) return;
                    AutoSkills.Add(new Skill(id, lv, owner));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null);
            }
        }
        public Skill GetSkill(long id)
        {
            return AutoSkills.FirstOrDefault(sk => sk.id == id);
        }
    }
}