using Cysharp.Threading.Tasks;
using DH.Data;
using DH.Game.Login;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using DH.Proto;
using Game.UI;
using Game.UI.Guide;
using Game.UI.MainUi;
using UnityEngine;

namespace DH.Game
{
    [ProcedureDeep(3)]
    public class MainGameProcedure : ProcedureBase
    {
        protected override async UniTask Enter()
        {
            var reqSync = new ReqSync();
            var result = await GameNetworkManager.Instance.SendAsync<RspSync>(reqSync);
            if (GameNetworkManager.CheckNetworkDown(result.rsp)) return;

            PayController.Instance.Init();
            AdController.Instance.Init();
            LoginManager.Instance.RoleEnterGame();
            DataCenter.Init(result.rsp);
            ReportedManager.Instance.Init();
            // z这里正常是进主线 
            BroadCostManager.Instance.Init().Forget();
            NotifyManager.Instance.Init();
            GlobalSchedule.Instance.SetTimeScale(GameConst.TimeDefaultScale);
            PlayerInfoManager.Instance.Init();
            MainUiManager.Instance.Init();
            PopUpManager.Instance.Init();
            GuideManager.Instance.Init();
            UIEffectManager.Instance.Init();

            // PlayerPrefs.SetInt(GameConst.EnterGameCount, 0);
            // // 记录进入次数
            // int enterCount = PlayerPrefs.GetInt(GameConst.EnterGameCount, 0);
            //
            // if (enterCount == 0 && DataCenter.mainStageData.CurrChapter == 1)
            // {
            //     OnEnterGame().Forget();
            // }
            // else
            // {
            //     PlayerPrefs.SetInt(GameConst.EnterGameCount, 1);
            //     await UIManager.Instance.OpenDialog<MainUiView, MainUiViewModel>();
            // }
            
            // if (!AppGlobal.Instance.UIEffectRoot.TryGetComponent<PointerEffectAid>(out _))
            // {
            //     AppGlobal.Instance.UIEffectRoot.gameObject.AddComponent<PointerEffectAid>();
            // }

            // await UIManager.Instance.OpenDialog<MainUiView, MainUiViewModel>();
        }

        private async UniTask OnEnterGame()
        {
            await UniTask.Delay(100);
            await MainUiManager.Instance.RequestEnterGame(1);
        }

        protected override async UniTask Active()
        {
            await UIManager.Instance.OpenDialog<MainUiView, MainUiViewModel>();
        }
        protected override void DeActive()
        {
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            AudioManager.Instance.Update(elapseSeconds);
        }

        protected override void Exit()
        {
            PayController.Instance.Dispose();
            AdController.Instance.Dispose();
            UIManager.Instance.CloseDialog<MainUiView>();
            // int enterCount = PlayerPrefs.GetInt(GameConst.EnterGameCount, 0);
            // if (enterCount < 1)
            // {
            //     PlayerPrefs.SetInt(GameConst.EnterGameCount, enterCount + 1);
            // }
        }

        public override string GetSceneConfig()
        {
            return "Scenes/MainMenu";
        }
    }
}