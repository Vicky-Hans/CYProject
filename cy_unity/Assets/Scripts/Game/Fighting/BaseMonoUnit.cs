using System.Collections.Generic;
using DH.Data;
namespace DH.Game
{
    public class BaseMonoUnit : BaseAssetEntity
    {
        protected UnitBase data;
        protected readonly SkillController skillController = new();
        protected readonly Dictionary<string, bool> moveFlagContainer = new();
        private ShieldEffect shieldEffect;
        [DebugData] public BaseMonoUnit currentTarget;
        public UnitBase Data => data;
        public AttributeMgr AttributeMgr => data.attr;
        public SkillController SkillController => skillController;
        public Monster MonsterData => data as Monster;
        public Player Player => data as Player;
        private void RefreshMoveFlag()
        {
            var result = true;
            foreach (var item in moveFlagContainer) result = result && item.Value;
        }
        public void StopMove(string key)
        {
            moveFlagContainer[key] = false;
            RefreshMoveFlag();
        }
        public void ResumeMove(string key)
        {
            moveFlagContainer.Remove(key);
            RefreshMoveFlag();
        }
        public bool IsStucked()
        {
            return data.IsStucked();
        }
        public virtual bool ValidAttackTarget()
        {
            return !data.IsDead();
        }
    }
}