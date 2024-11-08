using System;
using DH.Config;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class TalentCellItemViewModel : ViewModelBase
    {

	    [AutoNotify] private Vector2 cellSize = Vector2.one * 180;
		[AutoNotify] private string bgPath;
		[AutoNotify] private string tagBgPath;
		[AutoNotify] private bool isShowEffectNode;
		[AutoNotify] private string iconPath;
	    [AutoNotify] private ObservableList<TalentAddOnCellItemViewModel> addOnScrollViewList = new();
	    [AutoNotify] private bool isShowBg;
	    [AutoNotify] private Vector3 effectScale;
	    [AutoNotify] private string chooseCountTextStr;
	    private Vector2 defaultScale = Vector2.one * 130;
	    

	    private int curTalentId;
        [Preserve]
        public TalentCellItemViewModel(int talentId, int count)
        {
	        
	        curTalentId = talentId;
	        if (talentId == -1)
	        {
		        IsShowBg = false;
		        ChooseCountTextStr = "";
	        }
	        else
	        {
		        IsShowBg = true;
		        var cfg = ConfigCenter.TalentCfgColl.GetDataById(talentId);
		        IconPath = cfg.Icon;
		        ChooseCountTextStr = count >1 ? count.ToString(): "";
	        }
        }
        
        public void SetSize(Vector2 size)
        {
	        CellSize = size;
	        EffectScale = Vector3.one * (size.x / defaultScale.x);
        }
        [Command]
        private void OnClickOpBtn(Tuple<Vector3, Vector3> info)
        {
	        if(curTalentId == -1) return;
	        var languageCfg = ConfigCenter.TalentLanguageCfgColl.GetDataById(curTalentId);
	        var title = LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_Title);;
	        var cfg = ConfigCenter.TalentCfgColl.GetDataById(curTalentId);
	        var desc= languageCfg.Des;
	        if (cfg is { Value: not null })
	        {
		        desc =  string.Format(languageCfg.Des,cfg.Value.ToArray());
	        }
	        UIHelper.OpenItemTips(title,desc, info);
        }
    }
}