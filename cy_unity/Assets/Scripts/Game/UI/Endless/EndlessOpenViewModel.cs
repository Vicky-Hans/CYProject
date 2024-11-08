using Cysharp.Threading.Tasks;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.Game.UIViews;
using DH.UIFramework;
using Game.UI.MainUi;

namespace DH.Game.ViewModels
{
    public partial class EndlessOpenViewModel : ViewModelBase
    {
        public override bool AutoDispose => true;
        [Preserve]
        public EndlessOpenViewModel() { }
        [Command]
        private void OnClickClose()
        {
            UIManager.Instance.CloseDialog<EndlessOpenView>();
            MainUiManager.Instance.CurTabType = ETabType.TabTypeActivity;
            UIManager.Instance.OpenDialog<EndlessActivityView, EndlessActivityViewModel>().Forget();
        }
    }
}