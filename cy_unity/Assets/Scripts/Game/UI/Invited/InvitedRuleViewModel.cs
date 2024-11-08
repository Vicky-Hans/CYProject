using System.Collections.Generic;
using DH.Config;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class InvitedRuleViewModel : ViewModelBase
    {
        
		[AutoNotify] private ObservableList<InvitedRuleItemViewModel> scrollViewList = new();
        List<string> iconList = new()
        {
            "invite[invite_img_2]",
            "invite[invite_img_3]",
            "invite[invite_img_1]"
        };

        private List<string> descList = new()
        {
            LocalizeHelper.GetGlobal(GlobalLanguageId.Share_Tips_06),
            LocalizeHelper.GetGlobal(GlobalLanguageId.Share_Tips_07),
            LocalizeHelper.GetGlobal(GlobalLanguageId.Share_Tips_08),
        };
        [Preserve]
        public InvitedRuleViewModel()
        {
            for (int i = 0; i < iconList.Count; i++)
            {
                var tempVm = new InvitedRuleItemViewModel(iconList[i], descList[i]);
                scrollViewList.Add(tempVm);
            }
        }

        [Command]
        private void OnClickCloseBtn()
        {
	        UIManager.Instance.CloseDialog<InvitedRuleView>();
        }

        
    }
}