using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;

namespace DH.Game.ViewModels
{
    public partial class HurtCellItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private CanClaimItemViewModel canClaimItemVm;
		[AutoNotify] private float progressValue;
		[AutoNotify] private string progressTextStr;
		[AutoNotify] private string nameTextStr;

		/// <summary>
		///  伤害数据
		/// </summary>
		/// <param name="equipId"></param>
		/// <param name="hurtRatio"></param>
        [Preserve]
        public HurtCellItemViewModel(int equipId, float hurtRatio)
        {
	        // 这里 再初始化一下装备的icon
	        ProgressValue = hurtRatio;
	        ProgressTextStr = hurtRatio.ToString("P");
	        NameTextStr = EquipManager.Instance.GetEquipName(equipId);
	        
	        MailRewards res = new()
	        {
		        id = equipId,
		        count = 1,
		        type = (int)RewardType.Equip,
	        };
	        CanClaimItemVm = new CanClaimItemViewModel(res, false);
	        CanClaimItemVm.IsShowCount = false;

        }
    }
}