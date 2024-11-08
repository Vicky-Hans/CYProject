using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;


namespace DH.Game.ViewModels
{
    public partial class EquipUnOwnItemViewModel : ViewModelBase
    {
        [AutoNotify] private int id;
        [AutoNotify] private EquipCfg cfg;
        //数据状态
        [AutoNotify] private string iconPath;
        
        [AutoNotify] private string typeIconPath;
        [AutoNotify] private string attrIconPath;
        [AutoNotify] private int attrNum;
        [AutoNotify] private string progressDesc;

        [Preserve]
        public EquipUnOwnItemViewModel(int cfgId,bool show = true,bool isReplace=false)
        {
            Id = cfgId;
            Cfg = ConfigCenter.EquipCfgColl.GetDataById(Id);
            IconPath = EquipManager.Instance.GetIconPath(Cfg);
            AttrIconPath = EquipManager.Instance.GetAttrIconPath(Cfg);
            TypeIconPath = EquipManager.Instance.GetEquipType(Cfg?.GridType??0);
            AttrNum = EquipManager.Instance.GetEquipAttrNum(Id);
            var ownItemNum = DataCenter.itemsData.GetItemCountById(Cfg?.ItemId??0);
            ProgressDesc = $"{ownItemNum}";
        }

        protected override void OnDispose()
        {
            base.OnDispose();
        }
    }
}