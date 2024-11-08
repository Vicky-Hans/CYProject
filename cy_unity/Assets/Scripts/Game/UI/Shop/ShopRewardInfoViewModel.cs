using DH.Config;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class ShopRewardInfoViewModel : ViewModelBase
    {
	    public Reward Reward;
		[AutoNotify] private string nameStr;
		[AutoNotify] private string descValueStr;
		 [AutoNotify] private ObservableList<CellItemBaseViewModel> scrollViewList = new();

        [Preserve]
        public ShopRewardInfoViewModel(Reward reward)
        {
	        Reward = reward;
	        NameStr = UIHelper.GetRewardName(reward);
	        InitReward();
        }


        private void InitReward()
        {
	        int unlockNum = 0;
	        scrollViewList.Clear();
	        var list = UIHelper.GetItemJackpotList(Reward);
	        foreach (var item in list)
	        {
		        var itemModel = CellItemBaseViewModel.Create(item);
		        itemModel.IsShowLock = true;
		        itemModel.IsShowNum = false;
		        scrollViewList.Add(itemModel);
		        if (!UIHelper.GetItemIsLock(item))
		        {
			        unlockNum++;
		        }
	        }

	        DescValueStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips10, Mathf.Floor(1f / unlockNum * 100));//$"每个装备碎片概率：{1f / unlockNum:P0}";
        }

        [Command]
        private void OnClickBtnClose()
        {
	        UIManager.Instance.CloseDialog<ShopRewardInfoView>();
        }

        [Command]
        private void OnClickBtnOk()
        {
	        UIManager.Instance.CloseDialog<ShopRewardInfoView>();
        }
    }
}