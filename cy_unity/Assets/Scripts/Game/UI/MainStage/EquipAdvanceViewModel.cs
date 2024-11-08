using System;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;


namespace DH.Game.ViewModels
{
    public partial class EquipAdvanceViewModel : ViewModelBase
    {
        
        [AutoNotify] private ObservableList<EquipAdvanceItemViewModel> equipScrollViewList = new();
        public Action<int> ChooseCallback;
        private int curEquipId;
        [Preserve]
        public EquipAdvanceViewModel(int equipId)
        {
            curEquipId = equipId;
            var cfg = ConfigCenter.EquipCfgColl.GetDataById(equipId);
            if (cfg == null || cfg.Model == null)
            {
                DHLog.Error($"EquipAdvanceViewModel: cfg is null, equipId:{equipId}");
                return;
            }
            
            var models = cfg.Model[^1];
            foreach (var model in models)
            {
                EquipAdvanceItemViewModel item = new(model, OnChooseEquipMode);
                EquipScrollViewList.Add(item);
            }
        }
        private void OnChooseEquipMode(int equipModelId)
        {
            if (GameDataManager.Instance.IsSecretFightData())
            {
                RequestSecretEquipAdvanceSelect(curEquipId, equipModelId).Forget();
            }
            else
            {
                RequestEquipAdvanceSelect(curEquipId, equipModelId).Forget();
            }

   
            GameDataManager.Instance.ChooseEquipAdvance(curEquipId, equipModelId);
            ChooseCallback?.Invoke(equipModelId);
            UIManager.Instance.CloseDialog<EquipAdvanceView>();
            GameTime.Instance.Pause = GameDataManager.Instance.WaveEnd;
            
            GameDataManager.Instance.NotifyGameUIWeaponRefresh();
        }
        
        public async UniTaskVoid RequestEquipAdvanceSelect(int equipId,int skillId)
        {
            ReqBattleEquipSkillSelect req = new();
            req.Uid = GameDataManager.Instance.Uid;
            req.EquipId = equipId;
            req.SkillId = skillId;
	        
            var result = await GameNetworkManager.Instance.SendAsync<RspBattleEquipSkillSelect>(req);
            if (result.rsp is not { Status: 0 })
            {
                if(result.rsp == null) return;
                var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
                ToastManager.Show(str);
                return;
            }
            DHLog.Debug($"技能选择成功 {skillId}");
        }
        public async UniTaskVoid RequestSecretEquipAdvanceSelect(int equipId,int skillId)
        {
            ReqSecretBattleEquipSkillSelect req = new();
            req.Uid = GameDataManager.Instance.Uid;
            req.EquipId = equipId;
            req.SkillId = skillId;
            var result = await GameNetworkManager.Instance.SendAsync<RspSecretBattleEquipSkillSelect>(req);
            if (result.rsp is not { Status: 0 })
            {
                if(result.rsp == null) return;
                var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
                ToastManager.Show(str);
                // 装备进阶后检查一下其他的是否可以合成
                GameManager.Instance.DelayCheckIsCanMerge().Forget();
                return;
            }
            DHLog.Debug($"技能选择成功 {skillId}");
        }
    }
}