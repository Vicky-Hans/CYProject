using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;


namespace DH.Game.ViewModels
{
    public partial class EquipDetailViewModel : ViewModelBase
    {
        
		[AutoNotify] private string titleTextStr;
		[AutoNotify] private string tipsTextStr;
		[AutoNotify] private string equipIconPath;
	    [AutoNotify] private ObservableList<EquipAttrItemViewModel> infoScrollViewList = new();
	    [AutoNotify] private ObservableList<EquipSkillCellItemViewModel> equipSkillScrollViewList = new();
	    [AutoNotify] private string equipTypeIconPath;
	    [AutoNotify] private bool isShowEquipTypeIcon;
	    private int curEquipModelId;
	    private int curEquipId;
        [Preserve]
        public EquipDetailViewModel(int equipModelId, int equipId)
        {
	        curEquipModelId = equipModelId;
	        curEquipId = equipId;       
	        EquipIconPath = EquipManager.Instance.GetModelIconPath(equipModelId);
	        TitleTextStr = EquipManager.Instance.GetModelName(equipModelId);
	        TipsTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Equip_13, EquipManager.Instance.GetEquipModelLevel(equipModelId));
	        UpdateEquipArr();
	        UpdateEquipSkillList();
	        var cfg = ConfigCenter.EquipCfgColl.GetDataById(equipId);
	        if (cfg != null)
	        {
		        IsShowEquipTypeIcon = cfg.AtkType != 4;
		        EquipTypeIconPath = EquipManager.Instance.GetEquipAtkTypeIcon(cfg);
	        }
        }

        [Command]
        private void OnClickOpBtn()
        {
	        UIManager.Instance.CloseDialog<EquipDetailView>();
        }

        private void UpdateEquipArr()
        {
	        var cfg = ConfigCenter.EquipModelCfgColl.GetDataById(curEquipModelId);
	        if (cfg.Equip == 9)
	        {
		        InfoScrollViewList.Add(new EquipAttrItemViewModel("equip[common_icon_target]", LocalizeHelper.GetGlobal(GlobalLanguageId.Equip_09),EquipManager.Instance.GetTargetDesc(curEquipId)));
		        var allAttrList = EquipManager.Instance.GetEquipAttrListByModelId(curEquipModelId);
		        foreach (var item in allAttrList)
		        {
			        InfoScrollViewList.Add(new EquipAttrItemViewModel(new WeaponAttr(item.Key,item.Value)));
		        }
		        InfoScrollViewList.Add(new EquipAttrItemViewModel("equip[common_icon_coin]", LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips40),cfg.CoinNum.ToString()));
		        
	        }
	        else
	        {
		        InfoScrollViewList.Add(new EquipAttrItemViewModel("equip[common_icon_target]", LocalizeHelper.GetGlobal(GlobalLanguageId.Equip_09),EquipManager.Instance.GetTargetDesc(curEquipId)));
		        var allAttrList = EquipManager.Instance.GetEquipAttrListByModelId(curEquipModelId);
		        foreach (var item in allAttrList)
		        {
			        InfoScrollViewList.Add(new EquipAttrItemViewModel(new WeaponAttr(item.Key,item.Value)));
		        }
		        
	        }
        }

        private void UpdateEquipSkillList()
        {

	        foreach (var item in EquipSkillScrollViewList)
	        {
		        item.Dispose();
	        }
	        EquipSkillScrollViewList.Clear();
	        
	        var equipCfg = ConfigCenter.EquipCfgColl.GetDataById(curEquipId);
	        if (equipCfg == null)
	        {
		        DHLog.Error($" 没有配置或者装备技能， 请检查配置 EquipCfg id is {curEquipId }");
		        return;
	        }

	        if (equipCfg.DoubleUnlock == 0|| equipCfg.EquipSkillId == null ||equipCfg.EquipSkillId.Count == 0)
	        {
		        // DHLog.Error($" 没有配置或者装备技能， 请检查配置 EquipCfg id is {curEquipId }");
		        return;
	        }
	        
	        // 检查是否选过
	        var advanceId = GameDataManager.Instance.GetEquipAdvanceEquipModelIdByEquipId(curEquipId);
	        var itemData = DataCenter.equipData.GetEquipItemData(curEquipId);
	        if(itemData == null)return;
	        foreach (var skillId in equipCfg.EquipSkillId)
	        {
		        if (GameDataManager.Instance.CurStageType == EStateType.StageTypeSecret)
		        {
			        var tempCfg = ConfigCenter.EquipSkillCfgColl.GetDataById(skillId);
			        if (tempCfg == null)
			        {
				        // DHLog.Error($"没有配置 EquipSkillCfg 请检查配置 id {modelCfg.Effect} ");
				        continue;
			        }
			        if (tempCfg.IfShow == 1)
			        {
				        continue;
			        }   
		        }
		        
		        var equipSkillCfg = ConfigCenter.EquipSkillCfgColl.GetDataById(skillId);
		        if(equipSkillCfg.LvUnlockId > itemData.Lv) continue;
		        // 没合成
		        if (advanceId == 0)
		        {
			        if (equipSkillCfg.LvUnlockId <= equipCfg.DoubleUnlock)
			        {
				        EquipSkillCellItemViewModel tempVm = new(skillId);
				        EquipSkillScrollViewList.Add(tempVm);
			        }
		        }
		        else
		        {
			        if(equipSkillCfg.SkillType == 4) continue;
			        if (equipSkillCfg.LvUnlockId <= equipCfg.DoubleUnlock)
			        {
				        EquipSkillCellItemViewModel tempVm = new(skillId);
				        EquipSkillScrollViewList.Add(tempVm);
			        }
			        else
			        {
				        // 这里判断 选择对应的是否是同类型的
				        var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(advanceId);
				        if (modelCfg == null || modelCfg.Effect == 0)
				        {
					        continue;
				        }
				        var tempEquipSkillCfg = ConfigCenter.EquipSkillCfgColl.GetDataById(modelCfg.Effect);
				        if(tempEquipSkillCfg == null || tempEquipSkillCfg.PreviewSkill == null ||tempEquipSkillCfg.PreviewSkill.Count ==0) continue;
			        
				        if(!tempEquipSkillCfg.PreviewSkill.Contains(skillId)) continue;
				        
				        EquipSkillCellItemViewModel tempVm = new(skillId);
				        EquipSkillScrollViewList.Add(tempVm);
			        }
		        }
	        }
        }
    }
}