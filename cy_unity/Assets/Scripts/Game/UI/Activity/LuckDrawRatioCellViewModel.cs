using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;


namespace DH.Game.ViewModels
{
    public partial class LuckDrawRatioCellViewModel : ViewModelBase
    {
        
		[AutoNotify] private string bgPath;
		[AutoNotify] private string descTextStr;
		[AutoNotify] private string valueTextStr;
		
        [Preserve]
        public LuckDrawRatioCellViewModel(int index, string desc, string value)
        {
	        BgPath = index % 2 == 0 ? "common[common_panel_18]" : "common[common_panel_19]";
	        DescTextStr = desc;
	        ValueTextStr = value;
        }
        
        

        
    }
}