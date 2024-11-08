using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class ShopBoxRewardItemViewModel : ViewModelBase
    {

	    [AutoNotify] private int chestId;
		[AutoNotify] private string bgPath;
		[AutoNotify] private string iconPath;
		[AutoNotify] private ObservableList<ShopRewardAddItemViewModel> scrollViewList = new();
		private int selectLv;

		public int SelectLv
		{
			get => selectLv;
			set
			{
				if(!Set(ref selectLv,value))return;
				RefreshReward();
			}
		}

		[Preserve]
        public ShopBoxRewardItemViewModel(int chestId,int lv=-1)
        {
	        ChestId = chestId;
	        SelectLv = lv != -1 ? lv : DataCenter.shopData.GetBoxInfoLv();
	        BgPath = ShopManager.Instance.GetEquipChestBg(chestId);
	        IconPath = ShopManager.Instance.GetEquipChestIconPath(chestId);
        }

        private void RefreshReward()
        {
	        ScrollViewList.Clear();
	        var rewardList = ShopManager.Instance.GetBoxAddReward(ChestId, SelectLv);
	        foreach (var item in rewardList)
	        {
		        ScrollViewList.Add(new ShopRewardAddItemViewModel(chestId,item,rewardList.Count==1));
	        }
        }
    }
}