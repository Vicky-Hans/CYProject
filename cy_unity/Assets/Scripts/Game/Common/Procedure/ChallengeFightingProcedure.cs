using DH.Asset;
using UnityEngine;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using Cysharp.Threading.Tasks;
namespace DH.Game
{
    [ProcedureDeep(4)]
    public class ChallengeFightingProcedure :ProcedureBase
    {
        private readonly string fightingRootPath = "Player/ChallengeFightingRoot";
        public override string GetSceneConfig()
        {
            return "Scenes/MainFighting";
        }
        protected override async UniTask Enter() { }
        protected override async UniTask Active()
        {
            await UIManager.Instance.CloseAllTopDialogAsync(false);
            var root = await AssetsManager.InstantiateWithParamsAsync(fightingRootPath, Vector3.zero, Quaternion.identity, null);
            await root.GetComponent<ChallengeFightingManager>().Init();
            await UIManager.Instance.OpenDialog<ChallengeGameView, ChallengeGameViewModel>();
        }

        protected override void Exit()
        {
            UIManager.Instance.CloseDialog<ChallengeGameView>();
        }
    }
}