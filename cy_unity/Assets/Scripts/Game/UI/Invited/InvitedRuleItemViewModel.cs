using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;


namespace DH.Game.ViewModels
{
    public partial class InvitedRuleItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private string iconPath;
		[AutoNotify] private string descTextStr;
        [Preserve]
        public InvitedRuleItemViewModel(string path, string desc)
        {
            IconPath = path;
            DescTextStr = desc;
        }
    }
}