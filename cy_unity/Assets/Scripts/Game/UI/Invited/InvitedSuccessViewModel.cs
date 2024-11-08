using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Game.UIViews;
using DH.UIFramework;


namespace DH.Game.ViewModels
{
    public partial class InvitedSuccessViewModel : ViewModelBase
    {
        
		[AutoNotify] private string tipsDescStr;

        [Preserve]
        public InvitedSuccessViewModel(int count)
        {
	        TipsDescStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Share_Tips_11, count);
        }
        
        [Command]
        private void OnClickOpBtn()
        {
	        UIManager.Instance.OpenDialog<InvitedView,InvitedViewModel>().Forget();
        }

        
    }
}