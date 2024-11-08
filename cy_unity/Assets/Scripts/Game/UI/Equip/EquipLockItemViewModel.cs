using DH.Config;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
namespace DH.Game.ViewModels
{
    public partial class EquipLockItemViewModel : ViewModelBase
    {
	    [AutoNotify] private int id;
	    [AutoNotify] private string bgPath;
		[AutoNotify] private string iconPath;
		[AutoNotify] private string typeIconPath;
		[AutoNotify] private int unLockChapterId;
		[AutoNotify] private int attrNum;
        [Preserve]
        public EquipLockItemViewModel(int id)
        {
	        Id = id;
	        BaseInfo();
        }
        
        private void BaseInfo()
        {
	        BgPath = EquipManager.Instance.GetBgPath(Id);
	        IconPath = EquipManager.Instance.GetIconPath(Id);
	        var cfg = ConfigCenter.EquipCfgColl.GetDataById(Id);
	        TypeIconPath = EquipManager.Instance.GetEquipType(cfg?.GridType??0);
	        unLockChapterId = ConfigCenter.EquipCfgColl.GetDataById(id)?.Unlock??0;
	        AttrNum = EquipManager.Instance.GetEquipAttrNum(Id);
        }

        [Command]
        private void OnClickOpen()
        {
	        UIManager.Instance.OpenDialog<EquipInfoView>(new EquipInfoViewModel(Id));
        }

    }
}