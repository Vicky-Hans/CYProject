using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.NativeCore.Platform;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using Game.UI.MainUi;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class SubscribeGiftViewModel : ViewModelBase
    {
        
	    [AutoNotify] private ObservableList<CellItemBaseViewModel> awardScrollviewList = new();
		[AutoNotify] private string keyTextStr;

        [Preserve]
        public SubscribeGiftViewModel()
        {
	        KeyTextStr = DataCenter.charcaterData.DiscordCode;
	        var cfg = ConfigCenter.MailCfgColl.GetDataById(3);
	        int index = 0;
	        AwardScrollviewList.Clear();
	        foreach (var temp in cfg.Reward)
	        {
		        AwardScrollviewList.Add(CellItemBaseViewModel.Create(temp));
	        }
	        DHUnityUtil.PlayerPrefs.SetInt( DataCenter.charcaterData.DaysSubscribeGift,ServerTime.Instance.GetDay(ServerTime.Instance.GetNowTime()));
        }


        [Command]
        private void OnClickGOTOBtn()
        {
	        DeviceUtility.CopyToClipboard(DataCenter.charcaterData.DiscordCode);
	        var str = LocalizeHelper.GetGlobal(GlobalLanguageId.GiftCode_tips06);
	        ToastManager.Show(str);
	        // 使用默认浏览器打开链接
	        Application.OpenURL(GameConst.DiscordUrl);
        }

		public void Close()
		{
			UIManager.Instance.CloseDialog<SubscribeGiftView>();
		}

    }
}