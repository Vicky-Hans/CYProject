using System;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Config;
using DH.UIFramework.ViewModels;
using DH.UIFramework;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using Game.UI.MainUi;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Scripting;
namespace DH.Game.ViewModels
{
    public partial class WishItemViewModel : ViewModelBase
    {
	    [AutoNotify] private bool isShowWishNode = true;
	    [AutoNotify] private bool isShowWishItem;
		[AutoNotify] private string wishItemImgPath;
		[AutoNotify] private RectTransform wishNodeRect;
        [AutoNotify] private RectTransform iconEffectRect;
        [AutoNotify] private Vector3 wishNodeScale;
        [AutoNotify] private Image wishIcon;
		/// <summary>
		/// 是否解锁的标志位
		/// </summary>
		[AutoNotify] private bool isShowLockNode;
		/// <summary>
		///  点击心愿触发的回调
		/// </summary>
		private Action onClickWishCallback;
		private BackpackWeaponData weaponData;//武器数据
        private RectTransform weaponIconRect;
        private Sprite wishSprite;

        public GameManager Manager => GameManager.Instance;
        public BackpackWeaponData WeaponData
        {
            get => weaponData;
            set
            {
	            weaponData = value;
	            UpdateWishData();
            }
        }
        public Sprite WishSprite
        {
	        get => wishSprite;
	        set
	        {
		        wishSprite = value;
		        var spriteName = $"equip_icon[{wishSprite?.name.Replace("(Clone)","")}]";
		        if (spriteName == WishItemImgPath) return;
		        if (WishIcon == null || WeaponData == null) return;
		        WishIcon.sprite = AssetsManager.LoadSpriteSync(WishItemImgPath);
		        WishIcon.SetNativeSize();
	        }
        }
        [Preserve]
        public WishItemViewModel(BackpackWeaponData data, Action callback = null)
        {
	        WeaponData = data;
	        onClickWishCallback = callback;
	        //初始化
	        IsShowLockNode = !MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.Wish);
	        UpdateWishData();
	        UpdateWishBtnState();
	        var ratio = Lodash.GetResolutionRation();
	        if (ratio > 1)
	        {
		        WishNodeScale = Vector3.one * ratio;
	        }
	        else
	        {
		        WishNodeScale = Vector3.one;
	        }

	        InitAdFreePlus();
        }
        private void UpdateWishData()
        {
	        if (WeaponData != null)
	        {
		        IsShowWishItem = true;
		        var wishId = GameDataManager.Instance.GetWishIdByDataIdAndType(WeaponData.WeaponId, (EWishType)WeaponData.WeaponAttrType);
		        WishItemImgPath = GameManager.Instance.GetWishIconPath(wishId);
		        
	        }
	        else
	        {
		        IsShowWishItem = false;
		        WishItemImgPath = UIHelper.NoneImagePath();
	        }
	        UpdateMaxWeaponEffect().Forget();
        }
        public bool IsClickInArea(Vector2 screenPos)
        {
	        if (WishNodeRect == null) return false;
	        var worldPos = AppGlobal.Instance.UICamera.ScreenToWorldPoint(screenPos);
	        var tmpLocalPos = WishNodeRect.InverseTransformPoint(worldPos);
	        var transformVaild = false;
	        var containsVaild = false;
	        if (tmpLocalPos.x >= -WishNodeRect.rect.width * 0.5f && tmpLocalPos.x <= WishNodeRect.rect.width * 0.5f &&
	            tmpLocalPos.y >= -WishNodeRect.rect.height * 0.5f && tmpLocalPos.y <= WishNodeRect.rect.height * 0.5f)
	        {
		        transformVaild = true;
	        }
	        if (RectTransformUtility.RectangleContainsScreenPoint(WishNodeRect, screenPos,
		            AppGlobal.Instance.UICamera))
	        {
		        containsVaild = true;
	        }
	        return transformVaild || containsVaild;
        }

        public void UpdateWishBtnState()
        {
	        // 没解锁，不处理
	        if (!MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.Wish))
	        {
		        IsShowLockNode = true;
		        IsShowWishItem = false;
		        return;
	        }

	        IsShowLockNode = false;
	        var wishIds = GameDataManager.Instance.Wish;
	        if(wishIds == null || wishIds.Count == 0)
	        {
		        WishItemImgPath = UIHelper.NoneImagePath();
		        IsShowWishItem = false;
	        }
	        else
	        {
		        WishItemImgPath = GameManager.Instance.GetWishIconPath(wishIds[0]);
		        IsShowWishItem = true;
	        }
	        
	        UpdateMaxWeaponEffect().Forget();
        }

        public void OnClickWishBtn()
        {
	        if (IsShowAdFreePlus)
	        {
		        OnClickAdFreePlusBtnButton();
		        return;
	        }
	        
	        if (onClickWishCallback == null)
	        {
		        if (IsShowLockNode)
		        {
			        var str = MainUiManager.Instance.GetFunctionUnLockTips(EFunctionOpenType.Wish);
			        ToastManager.Show(str);
		        }
		        else
		        {
			        var str = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips43);
			        ToastManager.Show(str);
		        }
	        }
	        else
	        {
		        onClickWishCallback.Invoke();
	        }
        }
        
        public async UniTask UpdateMaxWeaponEffect()
        {
	        if(iconEffectRect ==null)return;
	        for (var i = iconEffectRect.childCount - 1; i >= 0; i--)
	        {
		        var childObj = iconEffectRect.GetChild(i);
		        AssetsManager.ReleaseInstance(childObj.gameObject);
	        }
	        if(weaponData ==null) return;
	        var cfg = ConfigCenter.EquipModelCfgColl.GetDataById(weaponData.WeaponId);
	        if (cfg?.HighEffect is null or "") return;
	        await AssetsManager.InstantiateWithParentAsync($"{cfg.HighEffect}_small",iconEffectRect, false);
        }

        #region 免广告plus
        [AutoNotify] private bool isShowAdFreePlus;

        private void InitAdFreePlus()
        {
	        var isOpenWish =  MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.Wish);
	        var isAdFree = DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.AdRreeReward) || 
	                       DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.ADFreeForever);

	        IsShowAdFreePlus = !isOpenWish && !isAdFree;
        }

        [Command]
        private void OnClickAdFreePlusBtnButton()
        {
	        UIManager.Instance.OpenDialog<AdFreePlusView,AdFreePlusViewModel>().Forget();
	    }
        
        #endregion

    }
}