using System;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;

namespace DH.Game.ViewModels
{
    public partial class EquipAdvanceItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private string iconPath;
		[AutoNotify] private string propertyIconPath;
		[AutoNotify] private string nameTextStr;
		[AutoNotify] private CellItemBaseViewModel skillCellVm;
		[AutoNotify] private string equipDescTextStr;
		[AutoNotify] private ObservableList<CellItemViewModel> skillCellVmList = new();

		private int equipModelId;
		private Action<int> onClickEquipModelBtn;
        [Preserve]
        public EquipAdvanceItemViewModel(int modelId, Action<int> callaback)
        {
	        equipModelId = modelId;
	        onClickEquipModelBtn = callaback;
	        UpdatePanelInfo();
	        UpdateSkillList();
        }

        [Command]
        private void OnClickOpBtn()
        {
	        onClickEquipModelBtn?.Invoke(equipModelId);
        }

        private void UpdatePanelInfo()
        {
	        var cfg = ConfigCenter.EquipModelCfgColl.GetDataById(equipModelId);
	        var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(equipModelId);
	        var skillCfg = ConfigCenter.EquipSkillCfgColl.GetDataById(cfg.Effect);
	        IconPath = EquipManager.Instance.GetModelIconPath(equipModelId);
	        NameTextStr = EquipManager.Instance.GetModelName(equipModelId);
	        PropertyIconPath = EquipManager.Instance.GetEquipAttrTypeIcon(modelCfg.AttrType);
	        EquipDescTextStr = EquipManager.Instance.GetEquipSkillDesc(cfg.Effect);

	        // foreach (var skillId in skillCfg.PreviewSkill)
	        // {
		       skillCellVm = new CellItemBaseViewModel(cfg.Effect, (int)RewardType.Skill,1, ECellItemSizeType.Size166X150, false, false);
	        // }
	        
        }

        private void UpdateSkillList()
        {
	        
	        var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(equipModelId);
	        if (modelCfg == null)
	        {
		        DHLog.Error($"没有配置 EquipModelCfg 请检查配置 id {equipModelId} ");
		        return;
	        }
	        var equipSkillCfg = ConfigCenter.EquipSkillCfgColl.GetDataById(modelCfg.Effect);
	        if (equipSkillCfg == null)
	        {
		        DHLog.Error($"没有配置 EquipSkillCfg 请检查配置 id {modelCfg.Effect} ");
		        return;
	        }

	        foreach (var skillId in equipSkillCfg.PreviewSkill)
	        {
		        if (GameDataManager.Instance.CurStageType == EStateType.StageTypeSecret)
		        {
			        var tempCfg = ConfigCenter.EquipSkillCfgColl.GetDataById(skillId);
			        if (tempCfg == null)
			        {
				        DHLog.Error($"没有配置 EquipSkillCfg 请检查配置 id {modelCfg.Effect} ");
				        continue;
			        }

			        if (tempCfg.IfShow == 1)
			        {
				        continue;
			        }   
		        }
		        CellItemViewModel tempVm = new (skillId, (int)RewardType.Skill,1, ECellItemSizeType.Size90X80, false, false);
		        tempVm.CellItemBaseViewVm.IsShowLock = !EquipManager.Instance.CheckEquipSkillUnlock(skillId); 
		        SkillCellVmList.Add(tempVm);
	        }
	        
        }
    }
}