using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class ShopBoxUpViewModel : ViewModelBase
    {
		[AutoNotify] private int levelValue;
		[AutoNotify] private ObservableList<ShopBoxRewardItemViewModel> scrollViewList = new();

        [Preserve]
        public ShopBoxUpViewModel(int lv)
        {
            LevelValue = lv;
            IntiScrollViewList();
        }

        private void IntiScrollViewList()
        {
            scrollViewList.Clear();
            var list = ShopManager.Instance.GetEquipChestList();
            foreach (var item in list)
            {
                scrollViewList.Add(new ShopBoxRewardItemViewModel(item.Id));
            }
        }
    }
}