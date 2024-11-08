using System.Collections.Generic;

namespace DH.Game
{
    public partial class AssetPoolEntity
    {
        private readonly Dictionary<string, int> sizeDic = new Dictionary<string, int>
        {
                {"Effects/HurtNum", 300},
                {"Effects/skill_buff_shield", 150},
                {"Effects/skill_buff_shield_broken", 150},
                {"Effects/deadFx", 150},
                {"Effects/dead_mid", 150},
                {"Effects/dead_small", 150},
                {"Effects/buff_frozen", 100},
                {"Effects/buff_firing", 100},
                {"Effects/buff_decelerate", 100},
                {"Effects/buff_fainting", 100},
                {"Effects/buff_hurt", 100},
                {"Effects/buff_vertigo", 100},
                {"Effects/buff_poison", 100},
                {"Effects/buff_electrify", 100},
                {"Effects/monster_revert", 100},
                {"Effects/monster_shield", 200}
        };
        
        public int GetPoolSize(string path)
        {
            if (path.Contains("Fighting/Enemy/Model/monster_")) return 100;
            return sizeDic.TryGetValue(path, out int size) ? size : 0;
        }
    }
}