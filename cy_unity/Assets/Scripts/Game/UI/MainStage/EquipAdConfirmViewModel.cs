using System;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;


namespace DH.Game.ViewModels
{
    public partial class EquipAdConfirmViewModel : ViewModelBase
    {
		[AutoNotify] private string titleStr;
		[AutoNotify] private string descStr;
		[AutoNotify] private RandomItemViewModel randomItemVm;
		[AutoNotify] private bool isShowAdIcon;
		private readonly Action<bool> opCallback;
		/// <summary>
		/// 看广告的确认弹窗
		/// </summary>
		/// <param name="weaponData"></param>
		/// <param name="gridAddData"></param>
		/// <param name="callback">回调，不管看不看都会调用，成功回调里面是true，否则 false</param>
        [Preserve]
        public EquipAdConfirmViewModel(BackpackWeaponData weaponData,GridAddData gridAddData, Action<bool> callback)
        {
	        opCallback = callback;
	        randomItemVm = new RandomItemViewModel(weaponData,gridAddData);
	        IsShowAdIcon = DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.AdRreeReward) ||
	        DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.ADFreeForever);
        }

        [Command]
        private void OnClickCancelBtn()
        {
	        opCallback(false);
	        UIManager.Instance.CloseDialog<EquipAdConfirmView>();
	        
        }

        [Command]
        private void OnClickConfirmBtn()
        {
	        UIHelper.ShowRewardAds(() =>
	        {
		        opCallback(true);
		        UIManager.Instance.CloseDialog<EquipAdConfirmView>();
	        });
        }
    }
}