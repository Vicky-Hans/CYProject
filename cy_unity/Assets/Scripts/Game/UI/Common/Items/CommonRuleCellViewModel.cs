using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;

namespace DH.Game.ViewModels
{
    public partial class CommonRuleCellViewModel : ViewModelBase
    {
        
		[AutoNotify] private string titleStr;
		[AutoNotify] private string contentStr;
        [AutoNotify] private bool isShowBg = true;
        [AutoNotify] private string bgImgPath = "common[common_panel_5]";
        [Preserve]
        public CommonRuleCellViewModel(CommonRuleData data)
        {
            TitleStr = data.Title;
            ContentStr = data.Content;
            // IsShowBg = data.IsShowBg;
            // if (data.bgPath != null)
            // {
            //     BgImgPath = data.bgPath;
            // }
        }
    }
}