using DH.Config;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;


namespace DH.Game.ViewModels
{
    public partial class EquipCellViewModel : ViewModelBase
    {
        
		[AutoNotify] private string bgPath;
		[AutoNotify] private string iconPath;
		[AutoNotify] private string levelStr;
		[AutoNotify] private string typeIconPath;
		[AutoNotify] private string propertyIconPath;

        [Preserve]
        public EquipCellViewModel(int equipId, int level)
        {
	        BgPath = EquipManager.Instance.GetBgPathByEquipId(equipId);
	        var cfg = ConfigCenter.EquipCfgColl.GetDataById(equipId);
	        TypeIconPath = EquipManager.Instance.GetEquipType(cfg?.GridType??0);
	        IconPath = EquipManager.Instance.GetIconPath(equipId, level);
	        LevelStr =  $"Lv.{level}";
        }
    }
}