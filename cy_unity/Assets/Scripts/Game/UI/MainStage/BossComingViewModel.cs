using Cysharp.Threading.Tasks;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.ViewModels;
using Spine.Unity;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class BossComingViewModel : ViewModelBase
    {

        private readonly int delayTime = 1500; //ms
        [AutoNotify] private SkeletonGraphic actionNode;
        [Preserve]
        public BossComingViewModel()
        {
            DelayClose().Forget();
        }
        
        private async UniTaskVoid DelayClose()
        {
            await UniTask.Delay(delayTime);
            UIManager.Instance.CloseDialog<BossComingView>();
        }
        

        
    }
}