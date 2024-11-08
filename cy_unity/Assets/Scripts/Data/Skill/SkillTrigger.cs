using System.Collections.Generic;
using DH.UIFramework.Observables;

namespace DH.Data
{
    public class SkillTrigger : ObservableObject
    {
        public Dictionary<string, int> trigger;
        public AttributeMgr attrMgr;

        public SkillTrigger(Skill skill, Dictionary<string, int> trigger, Dictionary<string, int> complete)
        {
            this.trigger = trigger;
            attrMgr = new AttributeMgr(skill.owner);
            attrMgr.Modify(complete);
        }
    }
}