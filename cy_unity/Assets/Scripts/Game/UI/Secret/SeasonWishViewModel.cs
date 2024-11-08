using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;


namespace DH.Game.ViewModels
{
    public partial class SeasonWishViewModel : ViewModelBase
    {
		[AutoNotify] private string wishTextStr;
		public SecretData BaseData=>DataCenter.secretData;
        [Preserve]
        public SeasonWishViewModel()
        {
	        var cfg = ConfigCenter.SecretSeasonCfgColl.GetDataById(BaseData.Season);
	        if (cfg != null)
	        {
		        WishTextStr = LocalizeHelper.GetGlobal(cfg.Rule);
	        }
        }

        [Command]
        private void OnClickCloseBtn()
        {
	        UIManager.Instance.CloseDialog<SeasonWishView>();
        }
    }
}