using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Asset;
using DH.Config;
using DH.Game.UI;
using DH.Data;
using DH.UIFramework;
using UnityEngine;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using UnityEngine.UI;
namespace DH.Game.ViewModels
{
    public partial class WeaponItemViewModel : ViewModelBase
    {
        [AutoNotify] private float alpha;//透明度
        [AutoNotify] private int adState;//广告状态 0~不为广告 1~是广告但未解锁 2~是广告已解锁
        [AutoNotify] private float cdTime;
        [AutoNotify] private Vector3 localPos;
        [AutoNotify] private Vector2 iconSize;
        [AutoNotify] private Vector2 weaponSize;
        [AutoNotify] private Vector2 tipsEffectPos;
        [AutoNotify] private Image weaponIcon;
        [AutoNotify] private string iconPathStr;
        [AutoNotify] private bool isShowGainTips;//是否显示增益提示
        [AutoNotify] private Vector2Int posIndex;//武器位置信息
        [AutoNotify] private RectTransform weaponNodeRect;//武器对象Rect
        private bool loadMaxEffect;
        private bool isAddMaxEffect;
        private Sprite weaponSprite;
        private float curCdTime;
        private Tweener breathingTweener;
        private Tweener popTweener;
        private BackpackWeaponData weaponData;//武器数据
        public GameManager Manager = GameManager.Instance;
        private RectTransform weaponIconRect;
        public RectTransform WeaponIconRect
        {
            get => weaponIconRect;
            set
            {
                weaponIconRect = value;
                if (weaponIconRect != null && IsAddMaxEffect && WeaponData != null && WeaponData.HighEffect != "")
                {
                    UpdateMaxWeaponEffect(true,WeaponData.HighEffect).Forget();
                }
            }
        }
        private bool IsAddMaxEffect
        {
            get => isAddMaxEffect;
            set
            {
                isAddMaxEffect = value;
                if (WeaponIconRect != null && isAddMaxEffect && WeaponData!=null && WeaponData.HighEffect != "")
                {
                    UpdateMaxWeaponEffect(true,WeaponData.HighEffect).Forget();
                } else if (!isAddMaxEffect && WeaponIconRect != null)
                {
                    UpdateMaxWeaponEffect(false,WeaponData?.HighEffect).Forget();
                }
            }
        }
        public Sprite WeaponSprite
        {
            get => weaponSprite;
            set
            {
                weaponSprite = value;
                var spriteName = $"fight_icon[{weaponSprite?.name.Replace("(Clone)","")}]";
                if (spriteName == WeaponData?.IconPath) return;
                if (WeaponIcon != null && WeaponData != null) WeaponIcon.sprite = AssetsManager.LoadSpriteSync(WeaponData?.IconPath);
            }
        }
        public BackpackWeaponData WeaponData
        {
            get => weaponData;
            set => weaponData = value;
        }
        [Preserve]
        public WeaponItemViewModel(BackpackWeaponData data)
        {
            UpdateWeaponData(data);
        }
        public void UpdateWeaponData(BackpackWeaponData data)
        {
            Alpha = 1;
            WeaponData = data;
            WeaponSize = Vector2.zero;
            IconSize = Vector2.zero;
            WeaponSize = new Vector2(WeaponData.Width*Manager.CellSize,WeaponData.Height*Manager.CellSize);
            IconSize = new Vector2(WeaponData.Width * Manager.CellSize - 10,WeaponData.Height * Manager.CellSize - 10);
            IconPathStr = "";
            IconPathStr = WeaponData.IconPath;
            CdTime = 0;
            if (breathingTweener != null)
            {
                WeaponNodeRect?.DOKill();
                breathingTweener.Kill();
                breathingTweener = null;
            }
            if (WeaponNodeRect!= null) WeaponNodeRect.localScale = Vector3.one;
            if (WeaponData.HighEffect != "")//最高阶
            {
                IsAddMaxEffect = true;
                if (WeaponIconRect!=null && WeaponIconRect.childCount == 0) UpdateMaxWeaponEffect(true,WeaponData.HighEffect).Forget();
            } else if (WeaponData !=null )
            {
                IsAddMaxEffect = false;
                if (WeaponIconRect!=null && WeaponIconRect.childCount > 0) UpdateMaxWeaponEffect(false,WeaponData.HighEffect).Forget();
            }
            CalculationTipEffectPos(WeaponData.ShapType);
        }

        public async UniTask UpdateMaxWeaponEffect(bool isAdd,string weaponEffect)
        {
            if (isAdd && CheckIsAddEffect(weaponEffect) && !loadMaxEffect)
            {
                if (weaponEffect is null or "") return;
                loadMaxEffect = true;
                await AssetsManager.InstantiateWithParentAsync(weaponEffect,WeaponIconRect, false);
                loadMaxEffect = false;
            }
            else if (WeaponIconRect!= null && WeaponIconRect.childCount > 0)
            {
                for (var i = WeaponIconRect.childCount-1; i >= 0; i--)
                {
                    var childObj = WeaponIconRect.GetChild(i);
                    AssetsManager.ReleaseInstance(childObj.gameObject);
                }
            }
        }
        private bool CheckIsAddEffect(string effectPath)
        {
            if (WeaponIconRect == null) return false;
            if (WeaponIconRect.childCount == 0) return true;
            var childObj = WeaponIconRect.GetChild(0);
            var path = $"UIEffects/{childObj.name.Replace("(Clone)","")}";
            if (path != effectPath) return true;
            for (var i = WeaponIconRect.childCount-1; i >= 0; i--)
            {
                var tmpChildObj = WeaponIconRect.GetChild(i);
                AssetsManager.ReleaseInstance(tmpChildObj.gameObject);
            }
            return true;
        }
        public bool IsClickInArea(Vector2 screenPos)
        {
            if (WeaponNodeRect == null) return false;
            var worldPos = AppGlobal.Instance.UICamera.ScreenToWorldPoint(screenPos);
            var tmpLocalPos = WeaponNodeRect.InverseTransformPoint(worldPos);
            var transformVaild = false;
            var containsVaild = false;
            if (tmpLocalPos.x >= -WeaponNodeRect.rect.width * 0.5f && tmpLocalPos.x <= WeaponNodeRect.rect.width * 0.5f &&
                tmpLocalPos.y >= -WeaponNodeRect.rect.height * 0.5f && tmpLocalPos.y <= WeaponNodeRect.rect.height * 0.5f)
            {
                transformVaild = true;
            }
            if (RectTransformUtility.RectangleContainsScreenPoint(WeaponNodeRect, screenPos,
                    AppGlobal.Instance.UICamera))
            {
                containsVaild = true;
            }
            if (!transformVaild && !containsVaild) return false;
            var cellY = Mathf.FloorToInt((tmpLocalPos.x + WeaponNodeRect.rect.width * WeaponNodeRect.anchorMax.x) / Manager.CellSize);
            var cellX = Mathf.FloorToInt((WeaponNodeRect.rect.height - tmpLocalPos.y + WeaponNodeRect.rect.yMin) / Manager.CellSize);
            var isExist = false;
            var tmpOccupyList = WeaponData.OccupyList;
            if (WeaponData.ShapType == GridType.LzThreeNum)
            {
                tmpOccupyList = GameDataManager.Instance.GetSpecialOccupyList();
            }
            for (var i = 0; i < tmpOccupyList.Count; i++) 
            {
                var occupyY = tmpOccupyList[i].x;
                var occupyX = tmpOccupyList[i].y;
                if (cellX == occupyY && cellY == occupyX) 
                {
                    isExist = true;
                    break;
                }
            }
            return isExist;
        }
        /// <summary>
        /// 刷新武器的位置
        /// </summary>
        public void UpdatePosition()
        {
            if (WeaponNodeRect && weaponData!= null)
            {
                WeaponNodeRect.localPosition = CalculationLocalPos(PosIndex, weaponData);
            }
        }
        /// <summary>
        /// 获取武器当前占用的实际格子位置信息
        /// </summary>
        /// <returns></returns>
        public List<Vector2Int> GetRunningCellOccupy()
        {
            var retOccupy = new List<Vector2Int>(weaponData.OccupyList.Count);
            for (var i = 0; i < weaponData.OccupyList.Count; i++)
            {
                var curPos = weaponData.OccupyList[i];
                var ret = new Vector2Int { x = curPos.x + PosIndex.x, y = curPos.y + PosIndex.y};
                retOccupy.Add(ret);
            }
            return retOccupy;
        }
        /// <summary>
        /// 刷新武器图标的信息
        /// </summary>
        public void UpdateIcon()
        {
            IconPathStr = WeaponData.IconPath;
        }
        /// <summary>
        /// 播放武器升级特效
        /// </summary>
        public async UniTask PlayWeaponUpgradeEffect()
        {
            if (WeaponNodeRect == null) return;
            var path = $"UIEffects/ui_effect_equipmixed";
            var tempNode = await AssetsManager.InstantiateWithParentAsync(path,WeaponNodeRect, false);
            if(tempNode == null) return;
            await UniTask.Delay(3000);
            AssetsManager.ReleaseInstance(tempNode);
        }
        /// <summary>
        /// 播放武器增益特效
        /// </summary>
        public async UniTask PlayWeaponGainEffect()
        {
            if (WeaponNodeRect == null) return;
            var path = $"UIEffects/ui_effect_equipment_gain";
            var tempNode = await AssetsManager.InstantiateWithParentAsync(path,WeaponNodeRect, false);
            if(tempNode == null) return;
            await UniTask.Delay(3000);
            AssetsManager.ReleaseInstance(tempNode);
        }
        public override void Update()
        {
            base.Update();
            if (WeaponData is { HighEffect: "" } && WeaponIconRect!=null && WeaponIconRect.childCount > 0)
            {
                UpdateMaxWeaponEffect(false,WeaponData.HighEffect).Forget();
            }
            if (GameDataManager.Instance.WaveEnd && Manager.DragState != EDragState.Weapon)
            {
                if (CdTime > 0) CdTime = 0;
                if (WeaponNodeRect != null && WeaponNodeRect.localScale.x > 1) WeaponNodeRect.localScale = Vector3.one;
                return;
            }
            if (WeaponData != null && breathingTweener == null && !GameDataManager.Instance.WaveEnd)
            {
                CdTime = BattleManager.Instance.fightingManagerIns.GetWeaponProgress((int)WeaponData.Uid);
                if (Mathf.Abs(CdTime-curCdTime) > Mathf.Epsilon)//不相等，但是CdTime=0
                {
                    if (CdTime <= 0 && WeaponNodeRect != null)
                    {
                        WeaponNodeRect.DOScale(1.2f, 0.05f).SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo);
                    }
                    curCdTime = CdTime;
                }
            }
            else if (WeaponNodeRect != null && WeaponNodeRect.localScale.x > 1 && Manager.DragState != EDragState.Weapon)
            {
                WeaponNodeRect.localScale = Vector3.one;
                CdTime = 0;
            }
            if (WeaponData != null && Manager.BlockState == EBlockState.Normal && Manager.DragState == EDragState.Weapon)
            {
                if (Manager.DragWeaponId == WeaponData.WeaponId && Manager.DragUid != WeaponData.Uid && !Manager.CheckWeaponMaxLevel(WeaponData.WeaponId,WeaponData.EquipId))
                {
                    if ((WeaponData.NextInfo.Count <= 1 || !Manager.CheckWeaponDoubleAttrUnlocked(WeaponData.EquipId)) && WeaponData.NextInfo.Count != 1) return;
                    CdTime = 0;
                    if (breathingTweener == null)
                    {
                        breathingTweener = WeaponNodeRect.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
                    } else if (!breathingTweener.IsPlaying())
                    {
                        WeaponNodeRect.localScale = Vector3.one;
                        breathingTweener = WeaponNodeRect.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
                    }
                }
                else
                {
                    ResetAnimationTweener();
                    if (WeaponNodeRect != null && WeaponNodeRect.localScale.x > 1) WeaponNodeRect.localScale = Vector3.one;
                }
            }
            else
            {
                ResetAnimationTweener();
                if (WeaponNodeRect != null && WeaponNodeRect.localScale.x > 1) WeaponNodeRect.localScale = Vector3.one;
            }
        }
        /// <summary>
        /// 重置武器的动画效果
        /// </summary>
        private void ResetAnimationTweener()
        {
            if (WeaponNodeRect != null) WeaponNodeRect.DOKill();
            if (breathingTweener == null) return;
            breathingTweener.Kill();
            breathingTweener = null;
        }
        public Vector3 CalculationLocalPos(Vector2Int idxPos,BackpackWeaponData weaponDataParam)
        {
            if (weaponDataParam == null) return Vector3.zero;
            if (weaponDataParam.LocationType == ELocationType.Godown)
            {
                var curPos = WeaponNodeRect != null ? new Vector3(WeaponNodeRect.localPosition.x,0,0) : Vector3.zero;
                return curPos;
            }
            var rowCenter = (GameDataManager.Instance.GridBorderRect.xMax+GameDataManager.Instance.GridBorderRect.xMin) * 0.5f;
            var columnCenter = (GameDataManager.Instance.GridBorderRect.yMax+GameDataManager.Instance.GridBorderRect.yMin) * 0.5f;
            var rowDiff = idxPos.x+1 - rowCenter;
            var columnDiff = idxPos.y+1 - columnCenter;
            var tmpLocalX = columnDiff * GameManager.Instance.CellSize;
            var tmpLocalY = -rowDiff * GameManager.Instance.CellSize;
            switch (weaponDataParam.ShapType)
            {
                case GridType.LTwoNum://横排2格 sizeInfo = new Vector2Int(2,1);
                {
                    tmpLocalX += GameManager.Instance.CellSize * 0.5f;
                } break; 
                case GridType.HTwoNum://竖排2格 sizeInfo = new Vector2Int(1,2);
                {
                    tmpLocalY -=GameManager.Instance.CellSize * 0.5f;
                } break; 
                case GridType.LThreeNum://横排3格 sizeInfo = new Vector2Int(3,1);
                {
                    tmpLocalX += GameManager.Instance.CellSize;
                } break; 
                case GridType.HThreeNum://竖排3格 sizeInfo = new Vector2Int(1,3);
                {
                    tmpLocalY -=GameManager.Instance.CellSize;
                } break; 
                case GridType.LtThreeNum: //L型3格（向下）sizeInfo = new Vector2Int(2,2);
                {
                    tmpLocalX += GameManager.Instance.CellSize * 0.5f;
                    tmpLocalY -=GameManager.Instance.CellSize * 0.5f;
                } break;
                case GridType.LzThreeNum: //L型3格（向上）sizeInfo = new Vector2Int(2,2);
                {
                    tmpLocalX -= GameManager.Instance.CellSize * 0.5f;
                    tmpLocalY -=GameManager.Instance.CellSize * 0.5f;
                } break;
                case GridType.DFourNum://丁字4格 sizeInfo = new Vector2Int(3,2);
                {
                    tmpLocalX += GameManager.Instance.CellSize;
                    tmpLocalY -=GameManager.Instance.CellSize * 0.5f;
                } break; 
                case GridType.LtFourNum://L型4格 sizeInfo = new Vector2Int(2,3);
                {
                    tmpLocalX += GameManager.Instance.CellSize * 0.5f;
                    tmpLocalY -=GameManager.Instance.CellSize;
                } break;
                case GridType.SFourNum://4方格 sizeInfo = new Vector2Int(2,2);
                {
                    tmpLocalX += GameManager.Instance.CellSize * 0.5f;
                    tmpLocalY -=GameManager.Instance.CellSize * 0.5f;
                } break; 
            }
            var resultLocalPos = new Vector3(tmpLocalX,tmpLocalY,0);
            return resultLocalPos;
        }
        private void CalculationTipEffectPos(GridType gridType)
        {
            TipsEffectPos = gridType switch
            {
                GridType.LtThreeNum or GridType.LtFourNum => new Vector2(-62, 0),//L型4格/L型3格（向下）
                GridType.LzThreeNum => new Vector2(62, 0),//L型3格（向上)
                _ => Vector2.zero
            };
        }
    }
}