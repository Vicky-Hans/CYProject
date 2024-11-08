using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;


namespace DH.Game.ViewModels
{
    public partial class TalentAddOnCellItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private string addOnImgPath;
		[AutoNotify] private string addonTextStr;
        [AutoNotify] private bool isShowAddOnImg;
        [AutoNotify] private bool isShowAddOnText;

        [Preserve]
        public TalentAddOnCellItemViewModel(string text, string addOnImgPath)
        {
            AddonTextStr = text;
            AddOnImgPath = addOnImgPath;
            IsShowAddOnImg = !string.IsNullOrEmpty(addOnImgPath);
            IsShowAddOnText = !string.IsNullOrEmpty(text);
        }
        
        

        
    }
}