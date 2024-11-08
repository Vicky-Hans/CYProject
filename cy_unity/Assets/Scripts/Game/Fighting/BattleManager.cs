using DH.Data;

namespace DH.Game
{
    public class BattleManager : ObservableSingleton<BattleManager>
    {
        public FightingBaseManager fightingManagerIns;
        public MapFightingManager MapFightingManager => fightingManagerIns  as MapFightingManager;

        public bool IsStageForest()
        {
            return fightingManagerIns.FightType == (int)EStateType.StageTypeSecret;
        }
    }
}