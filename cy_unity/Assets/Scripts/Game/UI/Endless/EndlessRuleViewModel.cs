using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;

namespace DH.Game.ViewModels
{
    public partial class EndlessRuleViewModel : ViewModelBase
    {
        [Preserve]
        public EndlessRuleViewModel() { }
        [Command]
        private void OnClickCloseBtn()
        {
            UIManager.Instance.CloseDialog<EndlessRuleView>();
        }
    }
}