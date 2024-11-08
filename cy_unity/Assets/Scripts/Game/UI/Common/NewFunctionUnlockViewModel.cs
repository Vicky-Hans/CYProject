using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using Game.UI;


namespace DH.Game.ViewModels
{
    public partial class NewFunctionUnlockViewModel : ViewModelBase
    {
        
		[AutoNotify] private string iconPath;
		[AutoNotify] private string titleStr;
		private FunctionOpenCfg cfg;
        [Preserve]
        public NewFunctionUnlockViewModel(EFunctionOpenType type)
        {
			cfg = ConfigCenter.FunctionOpenCfgColl.GetDataById((int)type);
			IconPath = $"mainui[{cfg.Icon}]";
			TitleStr = ConfigCenter.FunctionOpenLanguageCfgColl.GetDataById(cfg.Id).Name;
        }


        [Command]
        private void OnClickGotoBut()
        {
	        if (cfg.ObtainId!=0)
	        {
		        JumpManager.Instance.Jump(cfg.ObtainId);
	        }
        }
    }
}