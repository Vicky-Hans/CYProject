using DH.Config;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class EquipAttrItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private string bgPath;
		[AutoNotify] private string targetIconPath;
		[AutoNotify] private string targetValueStr;
		[AutoNotify] private string targetNameStr;
		[AutoNotify] private Color fontColor;
		[AutoNotify] private Color titleColor;
        [Preserve]
        public EquipAttrItemViewModel(WeaponAttr attr,bool isOwn=true,WeaponAttr addAttr=null)
        {
	        FontColor = UIHelper.HexColorStrToColor(isOwn ? "#FCF2E2" : "#D5D6DE");
	        TitleColor = UIHelper.HexColorStrToColor(isOwn ? "#5C3E22" : "#33344B");
	        BgPath = isOwn ? "equip[equip_panel_10]" : "equip[equip_panel_11]";
	        TargetIconPath = AttributesManager.Instance.GetAttrIcon(attr.Type);
		    TargetNameStr = AttributesManager.Instance.GetAttrName(attr.Type);
		    targetValueStr = AttributesManager.Instance.GetAttrDesc(attr, addAttr); //addAttr!=null ? $"{attr.Value}<color=#7CDE44>+{addAttr.Value}</color>" : attr.Value.ToString();
        }
        
        [Preserve]
        public EquipAttrItemViewModel(string iconPath,string name,string desc,bool isOwn=true)
        {
	        BgPath = isOwn ? "equip[equip_panel_10]" : "equip[equip_panel_11]";
	        FontColor = UIHelper.HexColorStrToColor(isOwn ? "#FCF2E2" : "#D5D6DE");
	        TitleColor = UIHelper.HexColorStrToColor(isOwn ? "#5C3E22" : "#33344B");
	        TargetIconPath = iconPath;
	        TargetNameStr = name;
	        TargetValueStr = desc;
        }
    }
}