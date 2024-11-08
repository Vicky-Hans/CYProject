using DH.Config;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;

namespace DH.Game.ViewModels
{
    public partial class EquipStateViewModel : ViewModelBase
    {
	    [AutoNotify] private int id;
	    [AutoNotify] private EquipCfg cfg;
		[AutoNotify] private string iconPath;
		[AutoNotify] private string nameStr;
		[AutoNotify] private string stateNameStr;
		[AutoNotify] private string elementIconPath;
		
		[AutoNotify] private bool isShowSkill;

		
		[AutoNotify] private EquipSkillItemViewModel equipSkillViewVm;

		[AutoNotify] private ObservableList<int> showEquipList=new();
		[AutoNotify] private int showPos;

		[AutoNotify] private bool isMaxLevel;
		private int InitModelId;
        [Preserve]
        public EquipStateViewModel(int id,int modelId = 0)
        {
	        Id = id;
	        Cfg = ConfigCenter.EquipCfgColl.GetDataById(id);
	        InitModelId = modelId;
	        InitShowList();
	        RefreshShow();
        }
        
        private void InitShowList()
        {
	        var showModelId =InitModelId==0?EquipManager.Instance.GetEquipIconShowModelId(id):InitModelId;
	        showEquipList.Clear();
	        if(Cfg==null) return;
	        foreach (var item in Cfg.Model)
	        {
		        showEquipList.AddRange(item);
	        }

	        showPos = 0;
	        for (int i = 0; i < showEquipList.Count; i++)
	        {
		        if (showEquipList[i] == showModelId)
		        {
			        showPos = i;
		        }
	        }
	        
        }

        private void RefreshShow()
        {
	        if (showPos < showEquipList.Count)
	        {
		        var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(showEquipList[showPos]);
		        IconPath = EquipManager.Instance.GetModelIconPath(showEquipList[showPos]);
		        NameStr = EquipManager.Instance.GetModelName(showEquipList[showPos]);
		        StateNameStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Equip_13,modelCfg?.Class ?? 0);// $"合成{modelCfg?.Class ?? 0}阶段";
		        ElementIconPath = EquipManager.Instance.GetEquipAttrTypeIcon(modelCfg?.AttrType ?? 0);
		        IsShowSkill = modelCfg?.Effect != 0;
		        EquipSkillViewVm = new EquipSkillItemViewModel(modelCfg?.Effect ?? 0);
		        IsMaxLevel = false; //(modelCfg?.Class ?? 0) == EquipManager.Instance.EquipMaxLv;

	        }
	        
        }

        [Command]
        private void OnClickClose()
        {
	        UIManager.Instance.CloseDialog<EquipStateView>();
        }
        
        [Command]
        private void OnClickLeft()
        {
	        if (ShowPos > 0)
	        {
		        ShowPos -= 1;
		        RefreshShow();
	        }
        }
        
        [Command]
        private void OnClickRight()
        {
	        if (ShowPos < showEquipList.Count-1)
	        {
		        ShowPos += 1;
		        RefreshShow();
	        }
        }

    }
}