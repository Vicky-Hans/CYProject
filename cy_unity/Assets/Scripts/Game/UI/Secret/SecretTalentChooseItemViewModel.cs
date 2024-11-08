using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DHFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class SecretTalentChooseItemViewModel : ViewModelBase
    {

	    [AutoNotify] private bool isChoose;
		[AutoNotify] private TalentCellItemViewModel talentCellItemViewVm;
		[AutoNotify] private CellItemBaseViewModel cellItemBaseViewVm;
		[AutoNotify] private string descTextStr;
		[AutoNotify] private bool isShowTalent;
		[AutoNotify] private bool isShowTips;
		[AutoNotify] private string cellBgImgPath;
		private Tweener chooseAction;
		private Action<SecretTalentChooseItemViewModel> chooseCallback;
		[AutoNotify] private int curIndex;
		[AutoNotify] private int curTalentId;
		[AutoNotify] private Vector3 cellScale;
		[AutoNotify] private int curServerIndex;
        [Preserve]
        public SecretTalentChooseItemViewModel(int index, int serverIndex,int talentId, Action<SecretTalentChooseItemViewModel> callbcak)
        {
	        curIndex = index;
	        curServerIndex = serverIndex;
	        curTalentId	= talentId;
	        chooseCallback = callbcak;
	        InitPanel();
        }

        private void InitPanel()
        {
	        var cfg = ConfigCenter.SecretCopyTalentCfgColl.GetDataById(curTalentId);
	        if (cfg == null)
	        {
		        DHLog.Error($" 没有配置，请检查配置 SecretCopyTalent id is {curTalentId}");
		        return;
	        }
			var talentCfg = ConfigCenter.TalentCfgColl.GetDataById(cfg.Id);
			if (talentCfg == null)
			{
				DHLog.Error($" 没有配置，请检查配置 TalentCfg id is {cfg.Id}");
				return;
			}
			var index = (talentCfg.Quality < 2 ? 2 : talentCfg.Quality ) > 5? 5 : talentCfg.Quality;
			CellBgImgPath =  $"fight[fight_skillbg_{index - 1}]";
			
			IsShowTalent = cfg.Type != 3;
			// 天赋
	        if (IsShowTalent)
	        {
		        TalentCellItemViewVm = new TalentCellItemViewModel(cfg.Id, 1);
		        var languageCfg = ConfigCenter.TalentLanguageCfgColl.GetDataById(cfg.Id);
		        if (languageCfg == null)
		        {
			        DHLog.Error($"没有配置， 请检查 TalentLanguageCfg 配置 {curTalentId}");
			        return;
		        }
		        if (talentCfg.Value != null &&talentCfg.Value.Count > 0)
		        {
			        DescTextStr =  string.Format(languageCfg.Des,talentCfg.Value.ToArray());
		        }
		        else
		        {
			        DescTextStr = languageCfg.Des;
		        }
	        }
	        else  // 装备
	        {
		        var equipModelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(talentCfg.EquipModelId);
		        if (equipModelCfg == null)
		        {
			        DHLog.Error($" 没有配置，请检查配置 EquipModelCfg id is {talentCfg.EquipModelId}");
			        return;
		        }
		        cellItemBaseViewVm = new CellItemBaseViewModel(equipModelCfg.Id, (int)RewardType.Equip, 1,ECellItemSizeType.Size166X150,false,false);
		        cellItemBaseViewVm.OnClickEvent = (Tuple<Vector3, Vector3> info) =>
		        {
			        OnClickEquipInfoBtn(talentCfg.EquipModelId);
		        };
		        var equipModelLanguageCfg = ConfigCenter.EquipModelLanguageCfgColl.GetDataById(talentCfg.EquipModelId);
		        if (equipModelLanguageCfg == null)
		        {
			        DHLog.Error($" 没有配置，请检查配置 EquipModelLanguageCfg id is {talentCfg.EquipModelId}");
			        return;
		        }
		        DescTextStr = equipModelLanguageCfg.WeaponName;
	        }
	        IsChoose = false;
	        CheckIsShowTips();
	        DoShowAction().Forget();
        }

        [Command]
        private void OnClickTalentBtn()
        {
	        chooseCallback(this);
        }
        private async UniTaskVoid DoShowAction()
        {
	        CellScale = Vector3.zero * 0.9f;
	        var actionDuration1 = 0.5f;
	        var scale1 = Vector3.one * 0.85f;
	        await UniTask.Delay(curIndex * 200);
	        chooseAction = DOVirtual.Vector3(CellScale, scale1, actionDuration1, v =>
	        {
		        // 设置目标节点的anchoredPosition
		        CellScale = v;
	        }).SetEase(Ease.OutExpo);
        }

        private void CheckIsShowTips()
        {
	        if (IsShowTalent)
	        {
		        IsShowTips = false;
		        return;
	        }
	        var talentCfg = ConfigCenter.TalentCfgColl.GetDataById(curTalentId);
	        IsShowTips = GameDataManager.Instance.CheckEquipIsShowTips(talentCfg.EquipModelId);
        }

        private void OnClickEquipInfoBtn(int modelId)
        {
	        var equipModelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(modelId);
	        EquipDetailViewModel tmepVm = new(modelId, equipModelCfg.Equip);
			UIManager.Instance.OpenDialog<EquipDetailView>(tmepVm).Forget();	        
        }
    }
}