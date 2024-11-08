using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.Config;
using DH.UIFramework;
namespace DH.Game.ViewModels
{
    public partial class ShopDrawClothesViewModel : ViewModelBase
    {
        
		[AutoNotify] private ShopBoxItemViewModel shopBoxItemViewVm;
        [AutoNotify] private string titleName;
        [Preserve]
        public ShopDrawClothesViewModel()
        {
            ShopBoxItemViewVm = new ShopBoxItemViewModel(ConfigCenter.EquipChestCfgColl.GetDataById(5));
            TitleName = ShopManager.Instance.GetTitleName(7);
        }
    }
}