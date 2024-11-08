using System;
using System.Collections.Generic;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;


namespace DH.Game.ViewModels
{
    public partial class SecretTalentChooseViewModel : ViewModelBase
    {
        
	    [AutoNotify] private ObservableDictionary<int,SecretTalentChooseItemViewModel> talentScrollViewDictionary = new();
		[AutoNotify] private string leftCountTextStr;
		[AutoNotify] private CommonAdvIconViewModel adVm;
		[AutoNotify] private bool isShowAdIcon;
		[AutoNotify] private string adLeftCountTextStr;
		[AutoNotify] private CommonAdvIconViewModel adRefreshVm;
		[AutoNotify] private CommonTopViewModel commonTopVm;
		[AutoNotify] private bool isShowRefreshBtn;
		private List<int> chooseTalentList = new();
		private List<int> baseTalentList = new();
		private const int ChooseLimitCount = 2;
		private Action<List<int>,List<int>> conformCallback;
		private Action chooseEndCallback;
		[AutoNotify]  private ESecretTalentRefreshType curRefreshType;
		
		[Preserve]
        public SecretTalentChooseViewModel(List<int> talentList, ESecretTalentRefreshType refreshType, Action<List<int>,List<int>> callback, Action chooseEndUpdateCallback)
        {
	        UpdateBaseTalentList(talentList);
	        curRefreshType = refreshType;
	        conformCallback = callback;
	        chooseEndCallback = chooseEndUpdateCallback;
	        InitPanel();
        }

        private void UpdateBaseTalentList(List<int> talentList)
        {
	        baseTalentList.Clear();
	        for (int i = 0; i < talentList.Count; i++)
	        {
		        var id = talentList[i];
		        var cfg = ConfigCenter.SecretCopyTalentCfgColl.GetDataById(id);
		        var tempId = id * 100  + i * 10 + cfg.Type;
		        baseTalentList.Add(tempId);
	        }
	        baseTalentList.Sort((a, b) =>
	        {
		        var sortA = a%10;
		        var sortB = b%10;
		        return sortB.CompareTo(sortA);
	        });
        }

        private void UpdateTalentScrollView()
        {
	        foreach (var item in talentScrollViewDictionary)
	        {
		        item.Value.Dispose();
	        }
	        talentScrollViewDictionary.Clear();
	        for (int i = 0; i < baseTalentList.Count; i++)
	        {
		        var tempId = baseTalentList[i] / 10;
		        var serverIndex = baseTalentList[i] % 100 / 10;
		        var talentId = baseTalentList[i] / 100;
		        SecretTalentChooseItemViewModel tempVm = new(i, serverIndex, talentId, OnClickTalentItem);
		        talentScrollViewDictionary.Add(i,tempVm);
	        }
        }

        protected override void OnDispose()
        {
	        if(adVm!=null)
		        adVm.Dispose();
	        if(adRefreshVm!=null)
		        adRefreshVm.Dispose();
	        if(CommonTopVm!=null)
	        {
		        commonTopVm.Dispose();
	        }
	        base.OnDispose();
        }

        private void InitPanel()
        {
	        UpdateLeftCountText();
	        List<GameConst.ItemIdCode> topList = new() {GameConst.ItemIdCode.AdFreeVouche};
	        commonTopVm = new(topList);
	        commonTopVm.SetIsShowAddIcon((int)GameConst.ItemIdCode.AdFreeVouche, false);
	        adRefreshVm = new CommonAdvIconViewModel();
	        CheckAndRefreshAdRefresh();
	        UpdateTalentScrollView();
        }
        [Command]
        private void OnClickConfirmBtn()
        {
	        // 检查选择个数
	        List<SecretTalentChooseItemViewModel> chooseItemList = new();
	        foreach (var item in talentScrollViewDictionary)
	        {
		        if (item.Value.IsChoose)
		        {
			        chooseItemList.Add(item.Value);
		        }
	        }

	        if (chooseItemList.Count != ChooseLimitCount)
	        {
		        // 给提示 选择数量不符合
		        // DHLog.Warning($"muzili warning 选择的天赋格个数不合法 限制选择个数 {ChooseLimitCount}  当前选择的个数是: {chooseItemList.Count}");
		        var str = LocalizeHelper.GetGlobal(GlobalLanguageId.Secret_02);
		        ToastManager.Show(str);
		        return;
	        }
	        
	        var idList = new List<int>();
	        var indexList = new List<int>();
	        foreach (var item in chooseItemList)
	        {
		        idList.Add(item.CurTalentId);
		        indexList.Add(item.CurServerIndex);
	        }
	        conformCallback(indexList,idList);
	        chooseEndCallback();
	        
	        GameManager.Instance.IsCanRandomTalent = true;
        }

        [Command]
        private void OnClickAdRefreshBtn()
        {
	        UIHelper.ShowRewardAds(() =>
	        {
		        GameManager.Instance.RequestSecretTalent(ESecretTalentRefreshType.AdRefresh,
			        (List<int> talentIds, ESecretTalentRefreshType refreshType) =>
			        {
				        UpdateBaseTalentList(talentIds);
				        UpdateTalentScrollView();
				        CheckAndRefreshAdRefresh();
				        UpdateLeftCountText();
			        },GameDataManager.Instance.Level, GameDataManager.Instance.Exp).Forget();
	        });
        }

        private void OnClickTalentItem(SecretTalentChooseItemViewModel item)
        {
	        if (item.IsChoose)
	        {
		        item.IsChoose = false;
	        }
	        else
	        {
		        if (CheckIsCanChoose())
		        {
			        item.IsChoose = true;
		        }
	        }
	        UpdateLeftCountText();
        }
        
        private bool CheckIsCanChoose()
        {
	        var chooseCount = 0;
	        foreach (var item in talentScrollViewDictionary)
	        {
		        if (item.Value.IsChoose) chooseCount += 1;
	        }
	        return chooseCount < ChooseLimitCount;
        }

        public void UpdatePanel(List<int> talentIds)
        {
	        chooseTalentList.Clear();
	        UpdateBaseTalentList(talentIds);
	        UpdateTalentScrollView();

	        UpdateLeftCountText();
        }
        private void UpdateLeftCountText()
        {
	        // var chooseCount = GetChooseCount();
	        var defCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_03);
	        if (defCfg == null || defCfg.Content == null || defCfg.Content.Count == 0)
	        {
		        DHLog.Error($" 配置文件 错误 请检查配置 DefinesCfg id {(int)DefineCfgId.Secret_03}");
		        return;
	        }
	        
	        var chooseCount = 0;
	        foreach (var item in talentScrollViewDictionary)
	        {
		        if (item.Value.IsChoose) chooseCount += 1;
	        }
	        if (chooseCount == ChooseLimitCount)
	        {
	         LeftCountTextStr = $"<color=#FFF8E4>{chooseCount}/{ChooseLimitCount}</color>";
	        }
	        else
	        {
	         LeftCountTextStr = $"<color=#F64C00>{chooseCount}</color><color=#FFF8E4>/{ChooseLimitCount}</color>";
	        }
        }
        public void CheckAndRefreshAdRefresh()
        {
	        var defCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_21);
	        var adTotalCount = 0;
	        adTotalCount = defCfg.Content[0];
	        var curCount = GameDataManager.Instance.TalentAdReFreshCount;
	        IsShowRefreshBtn = curCount > 0;
	        var str = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips39);
	        AdLeftCountTextStr = $"</color><color=#FFF8E4>{curCount}/{adTotalCount}</color>";	
        }
    }
}