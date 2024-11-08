using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Data;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class ShopDrawViewModel : ViewModelBase
    {
        
		[AutoNotify] private float sliderValue;
		[AutoNotify] private string progressValueStr;
		[AutoNotify] private int lvValueStr;
		[AutoNotify] private ObservableList<ShopBoxItemViewModel> shopBoxItemViewModels = new();
		[AutoNotify] private string titleName;
        [Preserve]
        public ShopDrawViewModel()
        {
	        InitBoxInfo();
	        RefreshLevelInfo();
	        LvValueStr = DataCenter.shopData.Recruit.Lv;
	        DataCenter.shopData.Recruit.PropertyChanged += RecruitChanged;
	        TitleName = ShopManager.Instance.GetTitleName(3);
        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        DataCenter.shopData.Recruit.PropertyChanged -= RecruitChanged;
	        UIHelper.ViewModelBaseOnDisposes(shopBoxItemViewModels);
        }

        private void RecruitChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(Recruit.Exp))
	        {
		        RefreshLevelInfo();
	        }
	        else if(e.PropertyName == nameof(Recruit.Lv))
	        {
		        LvValueStr = DataCenter.shopData.Recruit.Lv;
	        }
        }

        private void InitBoxInfo()
        {
	        shopBoxItemViewModels.ClearAndDispose();
	        var cfgList = ShopManager.Instance.GetEquipChestList();
	        foreach (var item in cfgList)
	        {
		        shopBoxItemViewModels.Add(new ShopBoxItemViewModel(item));
	        }
        }

        private void RefreshLevelInfo()
        {
	        var exp = DataCenter.shopData.Recruit.Exp;
	        var needExp = ShopManager.Instance.GetUpNeedExp(DataCenter.shopData.Recruit.Lv);
	        var isMax = ShopManager.Instance.IsEquipChestMax(DataCenter.shopData.Recruit.Lv);
	        if (isMax)
	        {
		        SliderValue = 1;
		        ProgressValueStr = "Max";
	        }
	        else
	        {
		        if (needExp == 0)
		        {
			        SliderValue = 0;
			        ProgressValueStr = string.Empty;
		        }
		        else
		        {
			        SliderValue = (float)exp / needExp;
			        ProgressValueStr = $"{exp}/{needExp}";
		        }
	        }

        }

        [Command]
        private void OnClickBtnProgressTips()
        {
	        //打开提示
	        UIManager.Instance.OpenDialog<ShopBoxRewardView>(new ShopBoxRewardViewModel()).Forget();
        }
    }
}