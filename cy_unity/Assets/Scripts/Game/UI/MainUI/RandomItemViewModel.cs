using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Config;
using DH.Game.UI;
using DH.Data;
using DH.UIFramework;
using UnityEngine;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class RandomItemViewModel : ViewModelBase
    {
        [AutoNotify] private float alpha;//透明度
        [AutoNotify] private int adState;//广告状态 0~不为广告 1~是广告但未解锁 2~是广告已解锁
        [AutoNotify] private string iconPathStr;
        [AutoNotify] private string adIconPathStr;
        [AutoNotify] private int randomItemID;
        [AutoNotify] private int adShowType;//0=右上角 1=左上角
        [AutoNotify] private Vector2 randomSize;
        [AutoNotify] private Vector2 adIconSize;
        [AutoNotify] private Vector2 randomIconSize;
        [AutoNotify] private Vector2Int posIndex;//武器位置信息
        [AutoNotify] private GridAddData gridAddData;
        [AutoNotify] private BackpackWeaponData weaponData;
        [AutoNotify] private RectTransform randomNodeRect;
        private bool isAddMaxEffect;
        private bool isShowMaxEffect = true;
        private bool loadMaxEffect;
        public CommonAdvIconViewModel RCommonAdvVm;
        public CommonAdvIconViewModel LCommonAdvVm;
        private RectTransform randomIconRect;
        public RectTransform RandomIconRect
        {
            get => randomIconRect;
            set
            {
                randomIconRect = value;
                if (randomIconRect == null || !IsAddMaxEffect || WeaponData == null) return;
                RandomIconRect.gameObject.SetActive(isShowMaxEffect);
                UpdateMaxWeaponEffect(true,WeaponData.HighEffect).Forget();
            }
        }
        public bool IsAddMaxEffect
        {
            get => isAddMaxEffect;
            set
            {
                isAddMaxEffect = value;
                if (RandomIconRect != null && isAddMaxEffect && WeaponData != null && WeaponData.HighEffect != "")
                {
                    UpdateMaxWeaponEffect(true,WeaponData.HighEffect).Forget();
                } 
                else if (!isAddMaxEffect && WeaponData != null && RandomIconRect != null)
                {
                    UpdateMaxWeaponEffect(false,WeaponData.HighEffect).Forget();
                }
            }
        }

        public bool IsShowMaxEffect
        {
            get => isShowMaxEffect;
            set
            {
                isShowMaxEffect = value;
                if (RandomIconRect != null)
                {
                    RandomIconRect.gameObject.SetActive(isShowMaxEffect);
                }
            }
        }
        [Preserve]
        public RandomItemViewModel(BackpackWeaponData weaponParam,GridAddData gridAddParam,float alphaParam = 1)
        {
            UpdateRandomData(weaponParam,gridAddParam,alphaParam);
            RCommonAdvVm = new CommonAdvIconViewModel();
            LCommonAdvVm = new CommonAdvIconViewModel();
        }

        public void UpdateRandomData(BackpackWeaponData weaponParam,GridAddData gridAddParam,float alphaParam = 1)
        {
            Alpha = alphaParam;
            WeaponData = weaponParam;
            GridAddData = gridAddParam;
            if (WeaponData != null)
            {
                IconPathStr = WeaponData.IconPath;
                RandomItemID = WeaponData.WeaponId;
                if (WeaponData.HighEffect != "")//最高阶
                {
                    IsAddMaxEffect = true;
                    if (RandomIconRect != null && RandomIconRect.childCount == 0)
                    {
                        UpdateMaxWeaponEffect(true, WeaponData.HighEffect).Forget();
                    }
                } else
                {
                    IsAddMaxEffect = false;
                    if (RandomIconRect != null && RandomIconRect.childCount > 0)
                    {
                        UpdateMaxWeaponEffect(false,WeaponData.HighEffect).Forget();
                    }
                }
                CalculationOccupyData(WeaponData.ShapType,false);
            }
            else if (GridAddData != null)
            {
                IconPathStr = GridAddData.IconPath;
                RandomItemID = GridAddData.GridId;
                CalculationOccupyData(GridAddData.ShapType,true);
            }
            UpdateAdState();
            RCommonAdvVm?.Dispose();
            LCommonAdvVm?.Dispose();
            RCommonAdvVm = new CommonAdvIconViewModel();
            LCommonAdvVm = new CommonAdvIconViewModel();
        }
        public override void Update()
        {
            base.Update();
            if (WeaponData is { HighEffect: "" } && RandomIconRect!= null && RandomIconRect.childCount > 0)
            {
                UpdateMaxWeaponEffect(false,WeaponData.HighEffect).Forget();
            }
        }
        /// <summary>
        /// 设置Icon对象的显隐状态
        /// </summary>
        /// <param name="isShow"></param>
        public void SetRandomIconHide(bool isShow)
        {
            IsShowMaxEffect = isShow;
            if (RandomIconRect!= null) RandomIconRect.gameObject.SetActive(isShow);
        }
        private async UniTask UpdateMaxWeaponEffect(bool isAdd,string weaponEffect)
        {
            if (isAdd && CheckIsAddEffect(weaponEffect) && !loadMaxEffect)
            {
                if (weaponEffect is null or "") return;
                loadMaxEffect = true;
                await AssetsManager.InstantiateWithParentAsync(weaponEffect,RandomIconRect, false);
                loadMaxEffect = false;
            }
            else if (RandomIconRect!= null && RandomIconRect.childCount > 0)
            {
                for (var i = RandomIconRect.childCount-1; i >= 0; i--)
                {
                    var childObj = RandomIconRect.GetChild(i);
                    AssetsManager.ReleaseInstance(childObj.gameObject);
                }
            }
        }
        private bool CheckIsAddEffect(string effectPath)
        {
            if (RandomIconRect == null) return false;
            if (RandomIconRect.childCount == 0) return true;
            var childObj = RandomIconRect.GetChild(0);
            var path = $"UIEffects/{childObj.name.Replace("(Clone)","")}";
            if (path != effectPath) return true;
            for (var i = RandomIconRect.childCount-1; i >= 0; i--)
            {
                var tmpChildObj = RandomIconRect.GetChild(i);
                AssetsManager.ReleaseInstance(tmpChildObj.gameObject);
            }
            return true;
        }
        /// <summary>
        /// 刷新广告格子状态
        /// </summary>
        public void UpdateAdState()
        {
            if (WeaponData != null)
            {
                if (WeaponData.AdType > 0)
                {
                    if (WeaponData.IsUnlocked)
                    {
                        AdState = 2;
                        AdIconPathStr = UIHelper.NoneImagePath();
                    }
                    else
                    {
                        AdState = 1;
                        AdIconPathStr = $"fight_icon[{WeaponData.AdIconPath}]";
                    }
                }
                else
                {
                    AdState = 0;
                    AdIconPathStr = UIHelper.NoneImagePath();
                }
            }
            else if (GridAddData != null)
            {
                if (GridAddData.AdType > 0)
                {
                    if (GridAddData.IsUnlocked)
                    {
                        AdState = 2;
                        AdIconPathStr = UIHelper.NoneImagePath();
                    }
                    else
                    {
                        AdState = 1;
                        AdIconPathStr = $"fight_icon[{GridAddData.AdIconPath}]";
                    }
                }
                else
                {
                    AdState = 0;
                    AdIconPathStr = UIHelper.NoneImagePath();
                }
            }
        }
        public bool IsClickInArea(Vector2 screenPos)
        {
            if (RandomNodeRect == null) return false;
            var worldPos = AppGlobal.Instance.UICamera.ScreenToWorldPoint(screenPos);
            var tmpLocalPos = RandomNodeRect.InverseTransformPoint(worldPos);
            
            var transformVaild = false;
            var containsVaild = false;
            if (tmpLocalPos.x >= -RandomNodeRect.rect.width * 0.5f && tmpLocalPos.x <= RandomNodeRect.rect.width * 0.5f &&
                tmpLocalPos.y >= -RandomNodeRect.rect.height * 0.5f && tmpLocalPos.y <= RandomNodeRect.rect.height * 0.5f)
            {
                transformVaild = true;
            }
            if (RectTransformUtility.RectangleContainsScreenPoint(RandomNodeRect,screenPos,AppGlobal.Instance.UICamera))
            {
                containsVaild = true;
            }
            return transformVaild || containsVaild;
        }
        /// <summary>
        /// 播放武器升级特效
        /// </summary>
        public async UniTask PlayRandomUpgradeEffect()
        {
            if (RandomNodeRect == null) return;
            var path = $"UIEffects/ui_effect_equipmixed";
            var tempNode = await AssetsManager.InstantiateWithParentAsync(path,RandomNodeRect, false);
            if(tempNode == null) return;
            await UniTask.Delay(3000);
            AssetsManager.ReleaseInstance(tempNode);
        }
        private void CalculationOccupyData(GridType gridType,bool isGrid)
        {
            switch (gridType)
            {
                case GridType.OneNum://1格
                {
                    RandomSize = new Vector2(135, 135);
                    RandomIconSize = new Vector2(124, 124);
                    AdIconSize = Vector2.one;
                    AdShowType = 0;
                } break; 
                case GridType.LTwoNum://横排2格 sizeInfo = new Vector2Int(2,1); 2 = icon(262,124) adIcon(312,162) 
                {
                    RandomSize = new Vector2(2*GameManager.Instance.CellSize, GameManager.Instance.CellSize);
                    RandomIconSize = new Vector2(2*GameManager.Instance.IconSize, GameManager.Instance.IconSize);
                    AdIconSize = new Vector2(2*GameManager.Instance.IconSize, GameManager.Instance.IconSize);
                    if (isGrid)
                    {
                        RandomIconSize = new Vector2(262,124);
                        AdIconSize = new Vector2(312,162);
                    }
                    AdShowType = 0;
                } break; 
                case GridType.HTwoNum://竖排2格 sizeInfo = new Vector2Int(1,2);3 = icon(124,262) adIcon(163,313)
                {
                    AdShowType = 0;
                    RandomSize = new Vector2(GameManager.Instance.CellSize, 2*GameManager.Instance.CellSize);
                    RandomIconSize = new Vector2(GameManager.Instance.IconSize, 2*GameManager.Instance.IconSize);
                    AdIconSize = new Vector2(GameManager.Instance.IconSize, 2*GameManager.Instance.IconSize);
                    if (isGrid)
                    {
                        RandomIconSize = new Vector2(124,262);
                        AdIconSize = new Vector2(163,313);
                    }
                } break; 
                case GridType.LThreeNum://横排3格 sizeInfo = new Vector2Int(3,1);4 = icon(400,124) adIcon(448,162) 
                {
                    AdShowType = 0;
                    RandomSize = new Vector2(3*GameManager.Instance.CellSize, GameManager.Instance.CellSize);
                    RandomIconSize = new Vector2(3*GameManager.Instance.IconSize, GameManager.Instance.IconSize);
                    AdIconSize = new Vector2(3*GameManager.Instance.IconSize, GameManager.Instance.IconSize);
                    if (isGrid)
                    {
                        RandomIconSize = new Vector2(400,124);
                        AdIconSize = new Vector2(448,162);
                    }
                } break; 
                case GridType.HThreeNum://竖排3格 sizeInfo = new Vector2Int(1,3);5 = icon(124,400) adIcon(162,448)
                {
                    AdShowType = 0;
                    RandomSize = new Vector2(GameManager.Instance.CellSize, 3*GameManager.Instance.CellSize);
                    RandomIconSize = new Vector2(GameManager.Instance.IconSize, 3*GameManager.Instance.IconSize);
                    AdIconSize = new Vector2(GameManager.Instance.IconSize, 3*GameManager.Instance.IconSize);
                    if (isGrid)
                    {
                        RandomIconSize = new Vector2(124,400);
                        AdIconSize = new Vector2(162,448);
                    }
                } break; 
                case GridType.LtThreeNum: //L型3格（向下）sizeInfo = new Vector2Int(2,2);广告左上角6 = icon(262,261) adIcon(308,308)
                {
                    AdShowType = 1;
                    RandomSize = new Vector2(2*GameManager.Instance.CellSize, 2*GameManager.Instance.CellSize);
                    RandomIconSize = new Vector2(2*GameManager.Instance.IconSize, 2*GameManager.Instance.IconSize);
                    AdIconSize = new Vector2(2*GameManager.Instance.IconSize, 2*GameManager.Instance.IconSize);
                    if (isGrid)
                    {
                        RandomIconSize = new Vector2(262,261);
                        AdIconSize = new Vector2(308,308);
                    }
                } break;
                case GridType.LzThreeNum: //L型3格（向上）sizeInfo = new Vector2Int(2,2);7 = icon(262,261) adIcon(308,308)
                {
                    AdShowType = 0;
                    RandomSize = new Vector2(2*GameManager.Instance.CellSize, 2*GameManager.Instance.CellSize);
                    RandomIconSize = new Vector2(2*GameManager.Instance.IconSize, 2*GameManager.Instance.IconSize);
                    AdIconSize = new Vector2(2*GameManager.Instance.IconSize, 2*GameManager.Instance.IconSize);
                    if (isGrid)
                    {
                        RandomIconSize = new Vector2(262,261);
                        AdIconSize = new Vector2(308,308);
                    }
                } break;
                case GridType.DFourNum://丁字4格 sizeInfo = new Vector2Int(3,2);8 = icon(400,261) adIcon(448,312) 
                {
                    AdShowType = 0;
                    RandomSize = new Vector2(3*GameManager.Instance.CellSize, 2*GameManager.Instance.CellSize);
                    RandomIconSize = new Vector2(3*GameManager.Instance.IconSize, 2*GameManager.Instance.IconSize);
                    AdIconSize = new Vector2(3*GameManager.Instance.IconSize, 2*GameManager.Instance.IconSize);
                    if (isGrid)
                    {
                        RandomIconSize = new Vector2(400,261);
                        AdIconSize = new Vector2(448,312);
                    }
                } break; 
                case GridType.LtFourNum://L型4格 sizeInfo = new Vector2Int(2,3);广告左上角9 = icon(262,399) adIcon(308,448)
                {
                    AdShowType = 1;
                    RandomSize = new Vector2(2*GameManager.Instance.CellSize, 3*GameManager.Instance.CellSize);
                    RandomIconSize = new Vector2(2*GameManager.Instance.IconSize, 3*GameManager.Instance.IconSize);
                    AdIconSize = new Vector2(2*GameManager.Instance.IconSize, 3*GameManager.Instance.IconSize);
                    if (isGrid)
                    {
                        RandomIconSize = new Vector2(262,399);
                        AdIconSize = new Vector2(308,448);
                    }
                } break;
                case GridType.SFourNum://4方格 sizeInfo = new Vector2Int(2,2);10 = icon(262,261) adIcon(309,308) 
                {
                    AdShowType = 0;
                    RandomSize = new Vector2(2*GameManager.Instance.CellSize, 2*GameManager.Instance.CellSize);
                    RandomIconSize = new Vector2(2*GameManager.Instance.IconSize, 2*GameManager.Instance.IconSize);
                    AdIconSize = new Vector2(2*GameManager.Instance.IconSize, 2*GameManager.Instance.IconSize);
                    if (isGrid)
                    {
                        RandomIconSize = new Vector2(262,261);
                        AdIconSize = new Vector2(309,308);
                    }
                } break; 
                default:
                    AdShowType = 0;
                    RandomSize = new Vector2(GameManager.Instance.CellSize, GameManager.Instance.CellSize);
                    RandomIconSize = new Vector2(GameManager.Instance.IconSize, GameManager.Instance.IconSize);
                    AdIconSize = Vector2.one;
                    break;
            }
        }
        protected override void OnDispose()
        {
            RCommonAdvVm?.Dispose();
            LCommonAdvVm?.Dispose();
            base.OnDispose();
        }
    }
}