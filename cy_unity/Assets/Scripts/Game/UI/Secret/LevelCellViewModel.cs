using System;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class LevelCellViewModel : ViewModelBase
    {

	    [AutoNotify] private string levelOpBtnImgPath;
		[AutoNotify] private string levelInfoTextStr;
		[AutoNotify] private bool isChoose;
		[AutoNotify] private EBoxState boxState;
		[AutoNotify] private RectTransform boxRectTransform;
		[AutoNotify] private Color levelInfoTextColor;
		[AutoNotify] private CopySecretCfg curCfg;
		[AutoNotify] private bool isShowTipsNode;
		private Action<LevelCellViewModel> clickLevelCallback;
		private Action<int, LevelCellViewModel> clickBoxCallback;

		private Color[] levelInfoTextColors = new Color[]
		{
			// 绿色
			UIHelper.HexColorStrToColor("#8AEA69"),
			//蓝色
			UIHelper.HexColorStrToColor("#3DC6FF"),
			// 紫色
			UIHelper.HexColorStrToColor("#CF7FF8"),
			// 橙色
			UIHelper.HexColorStrToColor("#FF6A48")
		};
		private string[] levelInfoTextArray = new string[]
		{
			// 绿色
			GlobalLanguageId.Secret_09,
			GlobalLanguageId.Secret_10,
			GlobalLanguageId.Secret_11,
			GlobalLanguageId.Secret_12
		};
			
		[Preserve]
        public LevelCellViewModel(CopySecretCfg cfg, Action<LevelCellViewModel> callback, Action<int, LevelCellViewModel> boxCallback)
        {
            curCfg = cfg;
            clickLevelCallback = callback;
            clickBoxCallback = boxCallback;
            InitPanel();
        }
        [Command]
        private void OnClickLevelBtn()
        {

	        if (CurCfg.Id > DataCenter.secretData.CurrStage)
	        {
		        var levelStr = GetLevelInfoText(CurCfg.Id - 1);
		        var str = LocalizeHelper.GetGlobal(GlobalLanguageId.Secret_16,levelStr);
		        ToastManager.Show(str);
		        return;
	        }

	        if (!IsChoose)
	        {
				clickLevelCallback?.Invoke(this);
	        }
        }

        [Command]
        private void OnClickBoxBtn()
        {
	        clickBoxCallback?.Invoke(1, this);
        }
        
        private void InitPanel()
        {
	        LevelInfoTextStr = GetLevelInfoText( curCfg.Id);
	        bool isUnlock = CurCfg.Id <= DataCenter.secretData.CurrStage;
	        LevelOpBtnImgPath = isUnlock ? "secret[secret_panel_1]": "secret[secret_panel_2]";
	        LevelInfoTextColor = GetLevelInfoTextColor(curCfg.Id);
	        UpdateBoxState();
	        UpdateIsShowTipsNode();
        }

        private int GetDifficulty(int cfgId)
        {
	        var difficultyCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_29);
	        if (difficultyCfg?.Content == null || difficultyCfg.Content.Count == 0) return 0;
	        var difficultyCount = difficultyCfg.Content.Count;

	        var ret = difficultyCount;
	        for (int i = 0; i < difficultyCfg.Content.Count; i++)
	        {
		        if (difficultyCfg.Content[i] >= cfgId - DataCenter.secretData.BeginStage + 1)
		        {
			        ret = i;
			        break;
		        }
	        }
	        return ret;
        }

        private int GetCurLevelShowIndex(int cfgId)
        {
	        var difficultyCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_29);
	        if (difficultyCfg?.Content == null || difficultyCfg.Content.Count == 0) return 0;
	        var difficultyCount = difficultyCfg.Content.Count;

	        var ret = difficultyCount;
	        for (int i = 0; i < difficultyCfg.Content.Count; i++)
	        {
		        if (difficultyCfg.Content[i] >= cfgId - DataCenter.secretData.BeginStage + 1)
		        {
			        ret = i;
			        break;
		        }
	        }
	        var offset = 0;
	        if (ret > 0)
	        {
		        offset = difficultyCfg.Content[ret - 1];
	        }
	        return cfgId - DataCenter.secretData.BeginStage - offset + 1;
        }

        private Color GetLevelInfoTextColor(int cfgId)
        {
	        var tempDifficulty =  GetDifficulty(cfgId);
	        if (tempDifficulty >= levelInfoTextColors.Length)
	        {
		        tempDifficulty = levelInfoTextColors.Length - 1;
	        }
			return levelInfoTextColors[tempDifficulty];
	        // return UIHelper.HexColorStrToColor("#FCF2E2");
        }
        private string GetLevelInfoText(int cfgId)
        {
	        var tempDifficulty =  GetDifficulty(cfgId);
	        
	        if (tempDifficulty >= levelInfoTextColors.Length)
	        {
		        tempDifficulty = levelInfoTextColors.Length - 1;
	        }
	        var str = LocalizeHelper.GetGlobal(GlobalLanguageId.Secret_04, GetCurLevelShowIndex(cfgId));
	        
	        return LocalizeHelper.GetGlobal(levelInfoTextArray[tempDifficulty], str);
        }

        public void UpdateBoxState()
        {
            if (curCfg == null) return;
            int chapterId = curCfg.Id;
            
            BoxState = DataCenter.secretData.GetBoxStateByChapterId(chapterId);
        }

        public void UpdateIsShowTipsNode()
        {
	        if (DataCenter.secretData.CurrStage == CurCfg.Id)
	        {
		        var stageInfo = DataCenter.secretData.GetStageInfo(curCfg.Id);
		        if(stageInfo ==null) return;
		        if (!stageInfo.Pass)
		        {
			        IsShowTipsNode = true;
			        return;
		        }
	        }

	        IsShowTipsNode = false;
        }
    }
}