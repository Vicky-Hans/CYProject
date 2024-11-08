using System;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;

namespace DH.Game.ViewModels
{
    public partial class DailySpecialSelectItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private string equipIconPath;
		[AutoNotify] private string nameStr;
		[AutoNotify] private string qualityImg;
		
		[AutoNotify] private bool isSelect;
		[AutoNotify] private bool isGray;
		[AutoNotify] private bool isSelectGray;
		
		public int EquipId;
		private readonly Action<int> callback;
        [Preserve]
        public DailySpecialSelectItemViewModel(int mEquipId , Action<int> mCallback)
        {
	        EquipId = mEquipId;
	        callback = mCallback;
	        var cfg = ConfigCenter.EquipCfgColl.GetDataById(EquipId);
	        var qlt = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.DailySpecial_WeaponQua).Content[0]-1;
	        EquipIconPath =EquipManager.Instance.GetModelIconPath(cfg.Model[qlt][0]);
	        NameStr =EquipManager.Instance.GetModelName(cfg.Model[qlt][0]);
	        QualityImg = GetQualityImg(cfg.Quality);
	        IsGray = !DataCenter.equipData.IsOwn(EquipId);
        }
        
        [Command]
        private void OnClickSelectBuy()
        {
	        //if (DataCenter.equipData.IsOwn(EquipId))
	        //{
		        callback?.Invoke(EquipId);
	        //}
        }

        private string GetQualityImg(int qlt)
        {
	        var temp = qlt switch
	        {
		        2 => "gratia[gratia_panel_12]",
		        3 => "gratia[gratia_panel_13]",
		        4 => "gratia[gratia_panel_14]",
		        _ => "gratia[gratia_panel_12]"
	        };

	        return temp;
        }
    }
}