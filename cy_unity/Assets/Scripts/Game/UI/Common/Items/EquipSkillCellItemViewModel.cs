using System;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DHFramework;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class EquipSkillCellItemViewModel : ViewModelBase
    {
		[AutoNotify] private string iconPath;
		[AutoNotify] private bool isLock;
		private EquipSkillCfg curCfg;
		private int equipSkillCfgId;
		private Action<int>clickEquipCellCallback;

		[Preserve]
		public EquipSkillCellItemViewModel(int equipSkillCfgId, Action<int> callback = null)
		{
			IconPath = EquipManager.Instance.GetEquipSkillIcon(equipSkillCfgId);
			curCfg = ConfigCenter.EquipSkillCfgColl.GetDataById(equipSkillCfgId);
			var itemData = DataCenter.equipData.GetEquipItemData(curCfg.EquipId);
			if (curCfg == null)
			{
				DHLog.Error($"没有装备技能配置 请检查配置在 EquipSkillCfg id is  {equipSkillCfgId}");
				return;
			}

			// 解锁了看天赋是否选择过
			var talentIds = GameDataManager.Instance.GetChooseTalent(ETalentType.TalentTypeEquip);
			IsLock = curCfg.SkillType != 4;
			foreach (var item in talentIds)
			{
				var talendId = item.Key;
				var talentCfg = ConfigCenter.TalentCfgColl.GetDataById(talendId);
				if (talentCfg == null)
				{
					DHLog.Error($"没有天赋配置 请检查配置在 TalentCfg id is  {talendId}");
					continue;
				}

				if (talentCfg.EquipSkillId == equipSkillCfgId)
				{
					IsLock = false;
					break;
				}
			}
        }

        [Command]
        private void OnClickOpBtn(Tuple<Vector3, Vector3> info)
        {
	        if (clickEquipCellCallback == null)
	        {
		        var desc = "";
		        if (IsLock)
		        {
			        // desc = $"<color=#FF0000> {LocalizeHelper.GetGlobal(GlobalLanguageId.Equip_08,curCfg.LvUnlockId)}</color>{EquipManager.Instance.GetEquipSkillDesc(curCfg.Id)}";
			        desc = $"{EquipManager.Instance.GetEquipSkillDesc(curCfg.Id)}";
		        }
		        else
		        {
			        desc = EquipManager.Instance.GetEquipSkillDesc(curCfg.Id);
		        }
		        UIHelper.OpenCommonTips("",desc,info.Item1,info.Item2);
	        }
	        else
	        {
		        clickEquipCellCallback?.Invoke(equipSkillCfgId);   
	        }
        }
    }
}