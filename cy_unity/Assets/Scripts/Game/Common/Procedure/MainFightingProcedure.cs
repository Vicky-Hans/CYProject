using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using UnityEngine;

namespace DH.Game
{
    [ProcedureDeep(4)]
    public class MainFightingProcedure:ProcedureBase
    {
        private readonly string fightingRootPath = "Player/MainFightingRoot";
        public override string GetSceneConfig()
        {
            return "Scenes/MainFighting";
        }

        protected override async UniTask Enter()
        {
           
        }

        protected override async UniTask Active()
        {
            UIManager.Instance.CloseAllTopDialog(false);
            var root = await AssetsManager.InstantiateWithParamsAsync(fightingRootPath, Vector3.zero, Quaternion.identity, null);
            await root.GetComponent<MainFightingManager>().Init();
            
            await UIManager.Instance.OpenDialog<MainStageGameUiView, MainStageGameUiViewModel>();
            
        }

        protected override void Exit()
        {
            UIManager.Instance.CloseDialog<MainStageGameUiView>();
        }
    }
}