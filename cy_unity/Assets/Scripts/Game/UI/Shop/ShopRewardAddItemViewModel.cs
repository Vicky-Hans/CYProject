using DH.Config;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class ShopRewardAddItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private string bgPath;
		[AutoNotify] private string addCntStr;
        [AutoNotify] private CellItemBaseViewModel cellItemBaseViewModel;
        [AutoNotify] private Vector2 itemSize;
        [Preserve]
        public ShopRewardAddItemViewModel(int chestId,Reward reward,bool isOnly)
        {
            CellItemBaseViewModel = CellItemBaseViewModel.Create(reward, ECellItemSizeType.Size90X80, false, false);
            AddCntStr = $"+{reward?.Count}";
            itemSize = isOnly ? new Vector2(289, 127): new Vector2(226, 116);
            bgPath = ShopManager.Instance.GetEquipChestItemBg(chestId);
        }
    }
}