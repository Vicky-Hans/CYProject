using System;
using System.Collections.Generic;
using DH.Game.UI;
using UnityEngine;
using DH.UIFramework;
using DH.UIFramework.ViewModels;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using Game.UI.Guide;
using DH.UIFramework.Observables;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace DH.Game.ViewModels
{
    public partial class MainMergeViewModel : ViewModelBase
    {
        [AutoNotify] private EDragState dragState;
        [AutoNotify] private Vector2Int gridSize;
        [AutoNotify] private RectTransform gridListRect;
        [AutoNotify] private RectTransform dragAreaRect;
        [AutoNotify] private RectTransform batchItemRect;
        [AutoNotify] private RectTransform weaponItemRect;
        [AutoNotify] private RectTransform gridAddItemRect;
        [AutoNotify] private RectTransform flyEndRect;
        [AutoNotify] private DragBatchItemViewModel dragBatchItemVm;
        [AutoNotify] private WeaponItemViewModel dragWeaponItemVm;
        [AutoNotify] private GridAddItemViewModel dragGridAddItemVm;
        [AutoNotify] private ObservableList<GridItemViewModel> gridList = new();
        [AutoNotify] private ObservableList<WeaponItemViewModel> weaponList = new();
        [AutoNotify] private ObservableList<RandomItemViewModel> randomBagList = new();
        private WishItemView wishView;
        [AutoNotify] private WishItemViewModel wishVm;
        public WishItemView WishView
        {
            get => wishView;
            set
            {
                wishView = value;
                if (value != null) onBindWishNodeCallback?.Invoke(value);
            }
        }
        public PointerEventData DragStartData;
        public Action<PointerEventData> PointerClickFunc;
        public Action<PointerEventData> DragFunc;
        public Action<PointerEventData> EndDragFunc;
        public Action<PointerEventData> BeginDragFunc;
        public Action<PointerEventData> PointerClickDownFunc;
        public Action<PointerEventData> PointerClickUpFunc;
        
        private Vector2 dragStartLocalPos = Vector2.zero;//拖动起点本地坐标
        private Vector3 dragRealTimeWorldPos = Vector3.zero;//拖动实时世界坐标
        private Vector2Int startIdxPos = Vector2Int.zero;
        private Tweener gridAniTweener;
        private bool isCanDrag;//是否可拖动
        private long playingAdUid;//正在播放广告的Uid
        public GameManager Manager => GameManager.Instance;
        private GameDataManager DataManager => GameDataManager.Instance;
        private List<Vector2Int> blockAddOccupyCells;
        private Action<WishItemView> onBindWishNodeCallback;
        private int eventPointerId;
        public MainMergeViewModel(Action<WishItemView> bingWishNodeCallback)
        {
            onBindWishNodeCallback = bingWishNodeCallback;
            GameManager.Instance.mainMergeVMInstance = this;
            PointerClickFunc = PointerClickHandle;
            PointerClickDownFunc = PointerClickDownHandle;
            PointerClickUpFunc = PointerClickUpHandle;
            BeginDragFunc = OnBeginDragHandle;
            DragFunc = OnDragHandle;
            EndDragFunc = OnEndDragHandle;
            Input.multiTouchEnabled = false;
            GameManager.Instance.FreeAdTipsCnt = 0;
            InitData();
            if (DataManager.Wish is { Count: > 0 })
            {
                var wishDataId = DataManager.ParseWishDataId(DataManager.Wish[0]);
                var wishType = DataManager.ParseWishType(DataManager.Wish[0]);
                var wishBackpackData = DataManager.CreateBackpackData(wishDataId * 10 + 1, (int)wishType, ELocationType.WishPool);
                WishVm = new WishItemViewModel(wishBackpackData);
            }
            else
            {
                WishVm = new WishItemViewModel(null);
            }
        }
        /// <summary>
        /// 数据初始化
        /// </summary>
        private void InitData()
        {
            DataManager.GetAreaBorder();
            //初始化武器列表
            foreach (var weaponData in DataManager.BackpackWeaponList)
            {
                var tmpVm = new WeaponItemViewModel(weaponData);
                tmpVm.PosIndex = new Vector2Int{x = weaponData.RowIdx,y = weaponData.ColumnIdx};
                tmpVm.UpdatePosition();
                WeaponList.Add(tmpVm);
            }
            RefreshGridList();
            RefreshWeaponList();
            GridSize = Manager.GridSize;
            if (WeaponList.Count > 0) BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
            DataManager.PropertyChanged += EquipDataPropertyChanged;
            Manager.PropertyChanged += BackpackDataPropertyChanged;
        }
        /// <summary>
        /// 刷新格子列表
        /// </summary>
        private void RefreshGridList()
        {
            GridList.Clear();
            Manager.UpdateGridSize();
            for (var i = 0; i < DataManager.GMaxRow; i++) 
            {
                for (var j = 0; j < DataManager.GMaxColumn; j++)
                {
                    if (DataManager.GridData[i][j] <= 0 && Manager.BlockState != EBlockState.AddCell) continue;
                    var blockUnit = new GridItemViewModel(i, j);
                    blockUnit.LocalPos = Manager.CalculationGridLocalPos(i, j);
                    GridList.Add(blockUnit);
                    blockUnit.SetBlockSolidState(DataManager.GridData[i][j]);
                }
            }
            foreach (var weaponItem in WeaponList)
            {
                weaponItem?.UpdatePosition();
            }
        }
        /// <summary>
        /// 刷新随机库列表
        /// </summary>
        private void RefreshWeaponList()
        {
            RandomBagList.Clear();
            foreach (var weaponParamData in DataManager.RandomWeaponDataList)
            {
                var weaponCfg = ConfigCenter.EquipModelCfgColl.GetDataById(weaponParamData.WeaponId/10);
                var weaponCopyCfg = ConfigCenter.CopyEquipWeightsCfgColl.GetDataById(weaponParamData.WeaponId/10);
                if (weaponCfg == null) continue;
                var adType = 0;
                var adIconPath = "common[common_alpha]";
                if (weaponCopyCfg is { Adv: > 0 } && weaponCopyCfg.Frame != "")
                {
                    adType = weaponCopyCfg.Adv;
                    adIconPath = weaponCopyCfg.Frame;
                } 
                switch (weaponCfg.Type)
                {
                    case 1://武器
                    {
                        var weaponData = DataManager.CreateBackpackData(weaponParamData.WeaponId,weaponParamData.WeaponAttrType,ELocationType.Godown);
                        if (weaponData == null) continue;
                        RandomBagList.Add(new RandomItemViewModel(weaponData,null));
                        if (DragWeaponItemVm != null) continue;
                        UpdateDragWeaponItemVm(weaponData);
                        if (DragWeaponItemVm != null) DragWeaponItemVm.Alpha = 0;
                        break;
                    }
                    case 2://格子
                    {
                        var tmData = DataManager.CreateGridAddData(weaponCfg, adType, adIconPath);
                        tmData.IsUnlocked = adType != 1 || weaponParamData.WeaponId%10 != 0;
                        tmData.GridParamId = weaponParamData.WeaponId;
                        RandomBagList.Add(new RandomItemViewModel(null,tmData));
                        if (DragGridAddItemVm != null) continue;
                        UpdateDragGridAddItemVm(tmData);
                        if (DragGridAddItemVm != null) DragGridAddItemVm.Alpha = 0;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 点击按下事件
        /// </summary>
        /// <param name="eventData"></param>
        private void PointerClickDownHandle(PointerEventData eventData)
        {
            if (Manager.DragState != EDragState.None) return;
            //候选列表
            var curClickRandomInfo = GetClickRandomVmIndex(eventData.position);
            if (curClickRandomInfo == null) return;
            UpdateGridInfoByWeapon();
            AudioManager.Instance.PlayButtonClick();
            Manager.DragState = EDragState.BlockAdd;
            Manager.BlockState = EBlockState.AddCell;
            UpdateDragGridAddItemVm(curClickRandomInfo.GridAddData);
            if (DragGridAddItemVm != null) DragGridAddItemVm.Alpha = 0;
            ParseDragItem(eventData.position);
            DataManager.GridBorderRect = Manager.GetMaxGridBorder();
            RefreshGridList();
        }
        /// <summary>
        /// 点击弹起事件
        /// </summary>
        /// <param name="eventData"></param>
        private void PointerClickUpHandle(PointerEventData eventData)
        {
            if (Manager.DragState != EDragState.BlockAdd) return;
            //候选列表
            var curClickRandomInfo = GetClickRandomVmIndex(eventData.position);
            if (curClickRandomInfo == null) return;
            if (curClickRandomInfo.GridAddData.AdType <= 0 || curClickRandomInfo.GridAddData.IsUnlocked) return;
            AudioManager.Instance.PlayButtonClick();
            if (playingAdUid != 0) return;
            playingAdUid = curClickRandomInfo.GridAddData.Uid;
            var tmpVm = new EquipAdConfirmViewModel(null,curClickRandomInfo.GridAddData, isOk =>
            {
                if (isOk)
                {
                    curClickRandomInfo.GridAddData.IsUnlocked = true;
                    curClickRandomInfo.UpdateAdState();
                }
                playingAdUid = 0;
            });
            UIManager.Instance.OpenDialog<EquipAdConfirmView>(tmpVm).Forget();
        }
        /// <summary>
        /// 触发点击事件
        /// </summary>
        /// <param name="eventData"></param>
        private void PointerClickHandle(PointerEventData eventData)
        {
            if (GuideManager.Instance.IsTriggerLevelGuide) return;
            if (Manager.DragState != EDragState.None) return;
            //候选列表
            var curClickRandomInfo = GetClickRandomVmIndex(eventData.position,true);
            if (curClickRandomInfo != null)
            {
                AudioManager.Instance.PlayButtonClick();
                var equipVm = new EquipDetailViewModel(curClickRandomInfo.WeaponData.WeaponId, curClickRandomInfo.WeaponData.EquipId);
                UIManager.Instance.OpenDialog<EquipDetailView>(equipVm).Forget();
                return;
            }
            if (Manager.DragState != EDragState.None) return;
            //场上的武器
            var clickIndex = GetClickWeaponItemIndex(eventData.position);
            if (clickIndex >= 0 && WeaponList[clickIndex] != null)
            {
                AudioManager.Instance.PlayButtonClick();
                var equipVm = new EquipDetailViewModel(WeaponList[clickIndex].WeaponData.WeaponId, WeaponList[clickIndex].WeaponData.EquipId);
                UIManager.Instance.OpenDialog<EquipDetailView>(equipVm).Forget();
                return;
            }
            //心愿
            if (!WishVm.IsShowWishItem && WishVm.IsClickInArea(eventData.position)) WishVm.OnClickWishBtn();
        }
        /// <summary>
        /// 物品拖拽中
        /// </summary>
        /// <param name="eventData"></param>
        private void OnDragHandle(PointerEventData eventData)
        {
            if (eventPointerId != eventData.pointerId) return;
            if (!Manager.CheckScreenPosValidity(dragAreaRect,eventData.position) || !isCanDrag) return;
            dragRealTimeWorldPos = AppGlobal.Instance.UICamera.ScreenToWorldPoint(eventData.position);
            if (Manager.DragState == EDragState.Weapon && DragWeaponItemVm != null && //拖动武器
                WeaponItemRect != null && DragWeaponItemVm.WeaponNodeRect != null)
            {
                var localPos = WeaponItemRect.InverseTransformPoint(dragRealTimeWorldPos);
                localPos.z = 0;
                DragWeaponItemVm.WeaponNodeRect.localPosition = localPos;
                ParseDragItem(eventData.position);
            }
            else if (Manager.DragState == EDragState.BlockAdd && DragGridAddItemVm != null //增加空格子
                 && GridAddItemRect != null && DragGridAddItemVm.GridNodeRect != null) 
            {
                if (DragGridAddItemVm.GridAddData.LocationType == ELocationType.Backpack && //新增格子时背包里的不能拖出来
                    !RectTransformUtility.RectangleContainsScreenPoint(GridListRect,eventData.position,AppGlobal.Instance.UICamera)) return;
                var localPos = GridAddItemRect.InverseTransformPoint(dragRealTimeWorldPos);
                localPos.z = 0;
                DragGridAddItemVm.GridNodeRect.localPosition = localPos;
                ParseDragItem(eventData.position);
            }
            else if (Manager.DragState == EDragState.DragBatch && DragBatchItemVm != null && BatchItemRect != null 
                 && DragBatchItemVm.BatchRect != null && DragBatchItemVm.GridList.Count > 0)//拖动整个地图
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(GridListRect,eventData.position,AppGlobal.Instance.UICamera)) return;
                var localPos = BatchItemRect.InverseTransformPoint(dragRealTimeWorldPos);
                localPos.z = 0;
                DragBatchItemVm.BatchRect.localPosition = localPos;
                var occupyInfo = GetBatchDragOccupyInfo(eventData.position);
                RefreshGridState(occupyInfo.Item2,occupyInfo.Item1,true);// 刷新格子显示状态
                ParseDragItem(eventData.position);
            }
        }
        /// <summary>
        /// 刷新格子显示状态
        /// </summary>
        /// <param name="occupyCells"></param>
        /// <param name="isAllMatch"></param>
        /// <param name="isDash"></param>
        private void RefreshGridState(List<Vector2Int> occupyCells,bool isAllMatch,bool isDash)
        {
            foreach (var gridItem in GridList)
            {
                var isEqual = false;
                foreach (var itemInfo in occupyCells)
                {
                    if (gridItem.IndexPos.x != itemInfo.x || gridItem.IndexPos.y != itemInfo.y) continue;
                    isEqual = true;
                    break;
                }
                if (isDash)
                {
                    gridItem.SetGridDashValidState(isEqual,isAllMatch);
                }
                else
                {
                    gridItem.SetGridSolidValidState(isEqual,isAllMatch);
                }
            }
        }
        /// <summary>
        /// 武器升级逻辑处理
        /// </summary>
        /// <param name="toUpgradeWeapon"></param>
        /// <param name="nextId"></param>
        private void WeaponUpgradeLogic(WeaponItemViewModel toUpgradeWeapon,int nextId)
        {
            //此处需要判断拖动的是背包里的还是随机库里的
            Manager.RemoveWeaponFromBackpack(toUpgradeWeapon.WeaponData);//移除两个旧的
            if (DragWeaponItemVm.WeaponData.LocationType == ELocationType.Backpack)
            {
                Manager.RemoveWeaponFromBackpack(DragWeaponItemVm.WeaponData); //将拖动升级的武器格子状态刷新为1
                UpdateGridDataValueByWeaponData(DragWeaponItemVm.WeaponData.OccupyList,DragWeaponItemVm.WeaponData.RowIdx, DragWeaponItemVm.WeaponData.ColumnIdx, 1,-1);
            }
            else if (DragWeaponItemVm.WeaponData.LocationType == ELocationType.WishPool)//如果是从心愿池拖出来，移除心愿池数据
            {
                Manager.RemoveWeaponFromWishPool(DragWeaponItemVm.WeaponData.WeaponId,DragWeaponItemVm.WeaponData.WeaponAttrType);
            }
            else
            {
                Manager.RemoveRandomWeaponData(DragWeaponItemVm.WeaponData.WeaponParamId,DragWeaponItemVm.WeaponData.WeaponAttrType);
            }
            var newWeaponData = DataManager.CreateBackpackData(nextId*10+1,1,toUpgradeWeapon.WeaponData.LocationType);
            newWeaponData.RowIdx = toUpgradeWeapon.PosIndex.x;
            newWeaponData.ColumnIdx = toUpgradeWeapon.PosIndex.y;
            toUpgradeWeapon.UpdateWeaponData(newWeaponData);
            AudioManager.Instance.PlayAudio($"SFX_UI/ui_game_merge0{newWeaponData.WeaponLev}");
            toUpgradeWeapon.PlayWeaponUpgradeEffect().Forget();//播放升级特效
            DataManager.TotalMergeNum += 1;
            DataManager.MergeWeaponUid = newWeaponData.Uid;
            if (DataManager.GetEquipCfgData(newWeaponData.EquipId)?.AtkType == (int)EquipAtkType.Magic && GameDataManager.Instance.MagicEquipMergedGainValue > 0)//魔法武器
            {
                DataManager.MagicMergeNum += 1;
                UIEffectManager.Instance.PlayerGameCoinAction(toUpgradeWeapon.WeaponNodeRect.position,1, true,null,false).Forget();
                GameManager.Instance.CheckMagicEquipMergedGain();
            }
            else if (DataManager.GetEquipCfgData(newWeaponData.EquipId)?.AtkType == (int)EquipAtkType.Physic && GameDataManager.Instance.PhyEquipMergedGainValue > 0)//物理武器
            {
                DataManager.PhysicsMergeNum += 1;
                UIEffectManager.Instance.PlayerGameCoinAction(toUpgradeWeapon.WeaponNodeRect.position,1, true,null,false).Forget();
                GameManager.Instance.CheckPhyEquipMergedGain();
            }
            if (GameDataManager.Instance.CheckIsAttributeGainWeapon(toUpgradeWeapon.WeaponData.EquipId))
            {
                PlayerGainWeaponEffectLogic(toUpgradeWeapon.WeaponData,toUpgradeWeapon.GetRunningCellOccupy());
            }
            Manager.AddWeaponToBackpack(toUpgradeWeapon.WeaponData);//新增一个新的
        }
        /// <summary>
        /// 获取当前点击的格子数
        /// </summary>
        /// <param name="screenPos"></param>
        /// <returns></returns>
        private Vector2Int GetCurClickGridIdx(Vector2 screenPos)
        {
            var clickIdxInfo = new Vector2Int(-1, -1);
            for (var i = GridList.Count - 1; i >= 0; i--)
            {
                if (!GridList[i].IsClickInArea(screenPos)) continue;
                clickIdxInfo = GridList[i].IndexPos;
                break;
            }
            return clickIdxInfo;
        }
        /// <summary>
        /// 获取当前点击到武器库物品的索引，没有返回-1
        /// </summary>
        /// <param name="screenPos"></param>
        /// <returns></returns>
        private int GetClickWeaponItemIndex(Vector2 screenPos)
        {
            var resultIdx = -1;
            for (var i = WeaponList.Count-1; i >=0; i--)
            {
                if (WeaponList[i] == null || !WeaponList[i].IsClickInArea(screenPos)) continue;
                resultIdx = i;
                break;
            }
            return resultIdx;
        }
        /// <summary>
        /// 设置随机库武器透明度
        /// </summary>
        /// <param name="pOpacity"></param>
        private void MakeWeaponTransparent(float pOpacity) 
        {
            foreach (var randomBagItem in RandomBagList)
            {
                if (randomBagItem?.WeaponData != null) randomBagItem.Alpha = pOpacity;
            }
            foreach (var weaponItem in WeaponList)
            {
                weaponItem.Alpha = pOpacity;
            }
        }
        /// <summary>
        /// 解析拖动的对象
        /// </summary>
        /// <param name="dragPos"></param>
        private void ParseDragItem(Vector2 dragPos)
        {
            List<Vector2Int> occupyCells = null;
            var allMath = false;
            if (Manager.DragState == EDragState.BlockAdd && DragGridAddItemVm != null) 
            {
                var occupyInfo = GetDragItemOccupyCells(dragPos,false);
                occupyCells = occupyInfo.Item2;
                allMath = occupyInfo.Item1;
            }
            else if (Manager.DragState == EDragState.Weapon && DragWeaponItemVm != null) 
            {
                var occupyInfo = GetDragItemOccupyCells(dragPos,true);
                occupyCells = occupyInfo.Item2;
                allMath = occupyInfo.Item1;
                if (allMath && occupyCells.Count > 0 && GameDataManager.Instance.CheckIsAttributeGainWeapon(DragWeaponItemVm.WeaponData.EquipId))
                {
                    var tmpList = GameDataManager.Instance.GetAttributeGainWeaponList(DragWeaponItemVm.WeaponData,occupyCells);
                    foreach (var weaponItem in WeaponList)//武器增益特效显示逻辑处理
                    {
                        weaponItem.IsShowGainTips = tmpList.Contains(weaponItem.WeaponData.Uid);
                    }
                }
            }
            if (occupyCells == null|| occupyCells.Count == 0) return;
            RefreshGridState(occupyCells,allMath,false);// 刷新格子显示状态
        }
        /// <summary>
        /// 批量拖拽的格子占用信息
        /// </summary>
        /// <returns></returns>
        private (bool,List<Vector2Int>) GetBatchDragOccupyInfo(Vector2 screenPos)
        {
            var offsetV2Int = GetDragItemStartIdxPos(DragBatchItemVm.GridListRect,screenPos);
            var occupyCells = new List<Vector2Int>();
            if (offsetV2Int.x == -1 || offsetV2Int.y == -1) return (false, occupyCells);
            foreach (var curIdxPos in DragBatchItemVm.OccupyList)
            {
                occupyCells.Add(new Vector2Int { x = offsetV2Int.x + curIdxPos.x,y = offsetV2Int.y + curIdxPos.y});
            }
            var isMatch = IsAllMatch(occupyCells,false);
            if (blockAddOccupyCells is not { Count: > 0 } || !isMatch) return (isMatch, occupyCells);
            foreach (var curBlockInfo in blockAddOccupyCells)
            {
                foreach (var tmpOccupyInfo in occupyCells)
                {
                    if (curBlockInfo.x != tmpOccupyInfo.x || curBlockInfo.y != tmpOccupyInfo.y) continue;
                    isMatch = false;
                    break;
                }
                if (!isMatch) break;
            }
            return (isMatch, occupyCells);
        }
        /// <summary>
        /// 获取拖拽物品的占用数据
        /// </summary>
        /// <param name="screenPos"></param>
        /// <param name="isWeapon"></param>
        /// <returns></returns>
        private (bool,List<Vector2Int>) GetDragItemOccupyCells(Vector2 screenPos, bool isWeapon)
        {
            var occupyCells = new List<Vector2Int>();
            var isMatch = false;
            if (isWeapon && DragWeaponItemVm != null && DragWeaponItemVm.WeaponNodeRect != null)
            {
                var offsetV2Int = GetDragItemStartIdxPos(DragWeaponItemVm.WeaponNodeRect,screenPos);
                if (offsetV2Int is not { x: >= 0, y: >= 0 }) return (false, occupyCells);
                var leftTopBlockPos = new Vector3(-(Manager.GridSize.y+1)*Manager.CellSize*0.5f,(Manager.GridSize.x+1)*Manager.CellSize*0.5f);//左上
                if (DragWeaponItemVm.WeaponData.ShapType == GridType.LzThreeNum)//类型7的特殊处理
                {
                    leftTopBlockPos.x += Manager.CellSize;
                }
                var tmpV2I = new Vector2Int(-1,-1);
                for (var i = 0; i < GridList.Count; i++)
                {
                    var tmpLocalPos = GridList[i].GridRect.localPosition;
                    var tmpleftTopX = tmpLocalPos.x - GridList[i].GridRect.sizeDelta.x * 0.5f;
                    var tmpleftTopY = tmpLocalPos.y + GridList[i].GridRect.sizeDelta.y * 0.5f;
                    var tmpIndexY = Mathf.FloorToInt((tmpleftTopX - leftTopBlockPos.x) / Manager.CellSize);
                    var tmpIndexX = Mathf.FloorToInt((leftTopBlockPos.y - tmpleftTopY) / Manager.CellSize);
                    if (tmpIndexY != offsetV2Int.y || tmpIndexX != offsetV2Int.x) continue;
                    tmpV2I = GridList[i].IndexPos;
                    break;
                }
                if (tmpV2I is { x: >= 0, y: >= 0 })
                {
                    foreach (var curIdxPos in DragWeaponItemVm.WeaponData.OccupyList)
                    {
                        occupyCells.Add(new Vector2Int { x = tmpV2I.x + curIdxPos.x,y = tmpV2I.y + curIdxPos.y});
                    }
                }
                isMatch = IsAllMatch(occupyCells,true);
            }
            else if (DragGridAddItemVm != null && DragGridAddItemVm.GridNodeRect != null)
            {
                var offsetV2Int = GetDragItemStartIdxPos(DragGridAddItemVm.GridNodeRect,screenPos);
                if (offsetV2Int is { x: >= 0, y: >= 0 })
                {
                    var tmpOccupyList = DragGridAddItemVm.GridAddData.OccupyList;
                    if (DragGridAddItemVm.GridAddData.ShapType == GridType.LzThreeNum)//类型7的特殊处理
                    {
                        tmpOccupyList = DataManager.GetSpecialOccupyList();
                    }
                    foreach (var curIdxPos in tmpOccupyList)
                    {
                        occupyCells.Add(new Vector2Int { x = offsetV2Int.x + curIdxPos.x,y = offsetV2Int.y + curIdxPos.y});
                    }
                }
                isMatch = IsAllMatch(occupyCells,false);
            }
            return (isMatch,occupyCells);
        }
        /// <summary>
        /// 获取拖拽物品的起始占位坐标
        /// </summary>
        /// <param name="itemRect"></param>
        /// <param name="screenPos"></param>
        /// <returns></returns>
        private Vector2Int GetDragItemStartIdxPos(RectTransform itemRect,Vector2 screenPos)
        {
            var offsetV2Int = new Vector2Int {x = -1,y = -1};
            if (GridListRect == null || itemRect == null) return offsetV2Int;
            if (!Manager.CheckScreenPosValidity(GridListRect,screenPos)) return offsetV2Int;
            var worldPos = AppGlobal.Instance.UICamera.ScreenToWorldPoint(screenPos);
            var curLocalPos = GridListRect.InverseTransformPoint(worldPos);
            var leftTopBlockPos = new Vector3(-(Manager.GridSize.y+1)*Manager.CellSize*0.5f,(Manager.GridSize.x+1)*Manager.CellSize*0.5f);//左上
            var rightBottomBlockPos = new Vector3((Manager.GridSize.y+1)*Manager.CellSize*0.5f,-(Manager.GridSize.x+1)*Manager.CellSize*0.5f);//右下
            var leftTop = new Vector3(curLocalPos.x - itemRect.sizeDelta.x * 0.5f, curLocalPos.y + itemRect.sizeDelta.y * 0.5f, 0);
            var rightBottom = new Vector3(curLocalPos.x + itemRect.sizeDelta.x * 0.5f, curLocalPos.y- itemRect.sizeDelta.y * 0.5f, 0);
            if (leftTop.x >= leftTopBlockPos.x && leftTop.y <= leftTopBlockPos.y 
              && rightBottom.x <= rightBottomBlockPos.x && rightBottom.y >= rightBottomBlockPos.y)//上下左右都不超
            {
                offsetV2Int.x = Mathf.FloorToInt((leftTopBlockPos.y - leftTop.y) / Manager.CellSize);
                offsetV2Int.y = Mathf.FloorToInt((leftTop.x - leftTopBlockPos.x) / Manager.CellSize);
            }
            else if (leftTop.x < leftTopBlockPos.x && leftTop.y <= leftTopBlockPos.y 
              && rightBottom.x <= rightBottomBlockPos.x && rightBottom.y >= rightBottomBlockPos.y)//仅仅左边超框
            {
                offsetV2Int.x = Mathf.FloorToInt((leftTopBlockPos.y - leftTop.y) / Manager.CellSize);
            }
            else if (leftTop.x >= leftTopBlockPos.x && leftTop.y > leftTopBlockPos.y 
               && rightBottom.x <= rightBottomBlockPos.x && rightBottom.y >= rightBottomBlockPos.y)//仅仅上边超框
            {
            }
            else if (leftTop.x >= leftTopBlockPos.x && leftTop.y <= leftTopBlockPos.y
               && rightBottom.x > rightBottomBlockPos.x && rightBottom.y >= rightBottomBlockPos.y) //仅仅右边超框
            {
                var detalx = rightBottom.x - rightBottomBlockPos.x;
                leftTop.x -= detalx;
                offsetV2Int.y = Mathf.FloorToInt((leftTop.x - leftTopBlockPos.x) / Manager.CellSize);
                offsetV2Int.x = Mathf.FloorToInt((leftTopBlockPos.y - leftTop.y) / Manager.CellSize);
            }
            else if (leftTop.x >= leftTopBlockPos.x && leftTop.y <= leftTopBlockPos.y 
              && rightBottom.x <= rightBottomBlockPos.x && rightBottom.y < rightBottomBlockPos.y)//仅仅下边超框
            {
                var detalY = rightBottomBlockPos.y - rightBottom.y;
                leftTop.y += detalY;
                offsetV2Int.y = Mathf.FloorToInt((leftTop.x - leftTopBlockPos.x) / Manager.CellSize);
                offsetV2Int.x = Mathf.FloorToInt((leftTopBlockPos.y - leftTop.y) / Manager.CellSize);
            }
            else if (leftTop.x < leftTopBlockPos.x && leftTop.y > leftTopBlockPos.y 
              && rightBottom.x <= rightBottomBlockPos.x && rightBottom.y >= rightBottomBlockPos.y)//左上都超
            {
            }
            else if (leftTop.x >= leftTopBlockPos.x && leftTop.y <= leftTopBlockPos.y 
             && rightBottom.x > rightBottomBlockPos.x && rightBottom.y < rightBottomBlockPos.y)//右下都超
            {
                var detalx = rightBottom.x - rightBottomBlockPos.x;
                var detalY = rightBottomBlockPos.y - rightBottom.y;
                leftTop.x -= detalx;
                leftTop.y += detalY;
                offsetV2Int.y = Mathf.FloorToInt((leftTop.x - leftTopBlockPos.x) / Manager.CellSize);
                offsetV2Int.x = Mathf.FloorToInt((leftTopBlockPos.y - leftTop.y) / Manager.CellSize);
            }
            return offsetV2Int;
        }
        //判断是否有效
        private bool IsAllMatch(List<Vector2Int> cellList,bool isWeapon)
        {
            var allMath = true;
            for (var i = cellList.Count - 1; i >= 0; i--)
            {
                var isEqual = false;
                GridItemViewModel targetCtrl = null;
                for (var j = GridList.Count - 1; j >= 0; j--)
                {
                    if (GridList[j].IndexPos.x != cellList[i].x ||
                        GridList[j].IndexPos.y != cellList[i].y) continue;
                    targetCtrl = GridList[j];
                    isEqual = true;
                    break;
                }
                if (isWeapon)
                {
                    if (isEqual && targetCtrl.OccupyState != EBlockOccupyState.Virtual) continue;
                    allMath = false;
                    break;
                }
                if (isEqual && targetCtrl.OccupyState != EBlockOccupyState.Solid) continue;
                allMath = false;
                break;
            }
            return allMath;
        }
        /// <summary>
        /// 播放增益武器特效
        /// </summary>
        /// <param name="weaponData"></param>
        /// <param name="occupyCells"></param>
        private void PlayerGainWeaponEffectLogic(BackpackWeaponData weaponData,List<Vector2Int> occupyCells)
        {
            var showList = GameDataManager.Instance.GetAttributeGainWeaponList(weaponData,occupyCells);
            foreach (var weaponItem in WeaponList)
            {
                weaponItem.IsShowGainTips = false;
                if (showList.Contains(weaponItem.WeaponData.Uid)) weaponItem.PlayWeaponGainEffect().Forget();
            }
        }
        /// <summary>
        /// 重置所有武器的增益特效显示
        /// </summary>
        private void ResetGainWeaponEffectLogic()
        {
            foreach (var weaponItem in WeaponList)
            {
                weaponItem.IsShowGainTips = false;
            }
        }
        /// <summary>
        /// 根据拖拽的武器数据，更新格子数据
        /// </summary>
        /// <param name="occupyList"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <param name="gridValue"></param>
        /// <param name="threshold"></param>
        private void UpdateGridDataValueByWeaponData(List<Vector2Int> occupyList,int offsetX, int offsetY,int gridValue,int threshold)
        {
            if (occupyList == null || occupyList.Count == 0) return;
            for (var i = occupyList.Count - 1; i >= 0; i--)
            {
                var gridCellX = offsetX + occupyList[i].x;
                var gridCellY = offsetY + occupyList[i].y;
                if (gridCellX < 0 || gridCellY < 0 ||gridCellX >= DataManager.GMaxRow || gridCellY >= DataManager.GMaxColumn) continue;
                if (DataManager.GridData[gridCellX][gridCellY] <= threshold) continue;
                DataManager.GridData[gridCellX][gridCellY] = gridValue;
            }
        }
        /// <summary>
        /// 检测所传入的占位信息是否有冲突
        /// </summary>
        /// <param name="originalOccupys"></param>
        /// <param name="newOccupys"></param>
        /// <returns></returns>
        private bool CheckOccupyConflict(List<Vector2Int> originalOccupys, List<Vector2Int> newOccupys)
        {
            var isConflict = false;
            for (var i = 0; i < originalOccupys.Count; i++)
            {
                var curOccupy = originalOccupys[i];
                for (var j = 0; j < newOccupys.Count; j++)
                {
                    if (curOccupy.x != newOccupys[j].x || curOccupy.y != newOccupys[j].y) continue;
                    isConflict = true;
                    break;
                }
                if (isConflict) break;
            }
            return isConflict;
        }
        /// <summary>
        /// 地块扩展确认逻辑
        /// </summary>
        public void ExtendConfirmLogic()
        {
            Manager.BlockState = EBlockState.Normal;
            if (blockAddOccupyCells is { Count: > 0 })
            {
                //获取blockAddOccupyCells中每个元素的上下左右格子数据信息
                var nearblockOccupyCellsInfo = new List<Vector2Int>();
                foreach (var blockItem in blockAddOccupyCells)
                {
                    if (blockItem.y + 1 < DataManager.GMaxColumn)//上
                    {
                        nearblockOccupyCellsInfo.Add(new Vector2Int{ x = blockItem.x, y = blockItem.y+1});
                    }
                    if (blockItem.y - 1 >= 0)//下
                    {
                        nearblockOccupyCellsInfo.Add(new Vector2Int{ x = blockItem.x, y = blockItem.y-1});
                    }
                    if (blockItem.x - 1 >= 0)//左
                    {
                        nearblockOccupyCellsInfo.Add(new Vector2Int{ x = blockItem.x - 1, y = blockItem.y});
                    }
                    if (blockItem.x + 1 < DataManager.GMaxRow)//右
                    {
                        nearblockOccupyCellsInfo.Add(new Vector2Int{ x = blockItem.x + 1, y = blockItem.y});
                    }
                }
                //踢出重复的
                for (var i = nearblockOccupyCellsInfo.Count-1; i >= 0; i--)
                {
                    var isHave = false;
                    for (var j = blockAddOccupyCells.Count - 1; j >= 0; j--)
                    {
                        if (nearblockOccupyCellsInfo[i].x != blockAddOccupyCells[j].x ||
                            nearblockOccupyCellsInfo[i].y != blockAddOccupyCells[j].y) continue;
                        isHave = true;
                        break;
                    }
                    if (isHave) nearblockOccupyCellsInfo.RemoveAt(i);
                }
                var expandSuccess = false;
                for (var i = nearblockOccupyCellsInfo.Count - 1; i >= 0; i--)
                {
                    var tempVec = nearblockOccupyCellsInfo[i];
                    if (DataManager.GridData[tempVec.x][tempVec.y] <= 0) continue;
                    expandSuccess = true;
                    break;
                }
                if (expandSuccess)
                {
                    DataManager.AddSloidBlocks(blockAddOccupyCells);
                }
                else
                {
                    DragGridAddItemVm.GridAddData.LocationType = ELocationType.Godown;
                    var tempViewMode = new RandomItemViewModel(null, DragGridAddItemVm.GridAddData);
                    RandomBagList.Add(tempViewMode);
                }
                BattleManager.Instance.fightingManagerIns.OnWeaponChanged();//武器变动要调用
                if (DragBatchItemVm != null) DragBatchItemVm.Alpha = 0;
                if (DragGridAddItemVm != null) DragGridAddItemVm.Alpha = 0;
            }
            //武器格子占用情况要刷新
            foreach (var weaponItem in WeaponList)
            {
                UpdateGridDataValueByWeaponData(weaponItem.WeaponData.OccupyList,weaponItem.PosIndex.x,weaponItem.PosIndex.y,weaponItem.WeaponData.WeaponId,-1);
            }
            DataManager.GetAreaBorder();
            RefreshGridList();
            MakeWeaponTransparent(1);
            blockAddOccupyCells = null;
            Manager.DragState = EDragState.None;
        }
        /// <summary>
        /// 获取点击位置的随机库物品(格子/武器)
        /// </summary>
        /// <param name="screenPos"></param>
        /// <param name="isWeapon"></param>
        /// <returns></returns>
        private RandomItemViewModel GetClickRandomVmIndex(Vector2 screenPos,bool isWeapon=false)
        {
            foreach (var randomBagItem in RandomBagList)
            {
                if (!randomBagItem.IsClickInArea(screenPos)) continue;
                if ((isWeapon && randomBagItem.WeaponData!= null)||(!isWeapon && randomBagItem.GridAddData!= null)) return randomBagItem;
            }
            return null;
        }
        /// <summary>
        /// 任意升级棋盘上的武器（升级可升级的）
        /// </summary>
        public void RandomUpgradeWeapon()
        {
            var wList = ListPool<WeaponItemViewModel>.Get();
            foreach (var weaponItem in WeaponList)
            {
                if (Manager.CheckWeaponMaxLevel(weaponItem.WeaponData.WeaponId,weaponItem.WeaponData.EquipId)) continue;//最大等级
                if (weaponItem.WeaponData.NextInfo.Count > 1) continue;//双属性不合成
                wList.Add(weaponItem);
            }
            if (wList.Count == 0) return;
            var randomSeed =Lodash.RandRange(0, wList.Count);
            if (wList[randomSeed] == null) return;
            var nextWeaponId = wList[randomSeed].WeaponData.NextInfo[0];
            WeaponUpgradeLogic(wList[randomSeed], nextWeaponId);
            BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
            ListPool<WeaponItemViewModel>.Release(wList);
        }
        /// <summary>
        /// 检测是否在拖拽中
        /// </summary>
        public bool CheckIsDraging()
        {
            var isDraging = DragWeaponItemVm is { Alpha: > 0 } || (DragGridAddItemVm is { Alpha: > 0 } && isCanDrag) ||DragBatchItemVm is { Alpha: > 0};
            return isDraging;
        }
        /// <summary>
        /// 刷新当前拖动的武器对象数据
        /// </summary>
        /// <param name="weaponData"></param>
        private void UpdateDragWeaponItemVm(BackpackWeaponData weaponData)
        {
            if (weaponData == null) return;
            if (DragWeaponItemVm == null)
            {
                DragWeaponItemVm = new WeaponItemViewModel(weaponData);
            }
            else
            {
                DragWeaponItemVm.UpdateWeaponData(weaponData);
            }
        }
        /// <summary>
        /// 刷新当前拖动的新增格子对象
        /// </summary>
        /// <param name="gridAddData"></param>
        private void UpdateDragGridAddItemVm(GridAddData gridAddData)
        {
            if (DragGridAddItemVm == null)
            {
                DragGridAddItemVm = new GridAddItemViewModel(gridAddData);
            }
            else
            {
                DragGridAddItemVm.UpdateGridAddData(gridAddData);
            }
        }
        /// <summary>
        /// 武器格子占用情况要刷新
        /// </summary>
        private void UpdateGridInfoByWeapon()
        {
            //重置
            for (var i = 0; i < GameDataManager.Instance.GMaxRow; i++)
            {
                for (var j = 0; j < GameDataManager.Instance.GMaxColumn; j++)
                {
                    GameDataManager.Instance.GridData[i][j] = 0;
                }
            }
            //解锁
            foreach (var gridInfo in GridList)
            {
                GameDataManager.Instance.GridData[gridInfo.IndexPos.x][gridInfo.IndexPos.y] = 1;
            }
            foreach (var weaponItem in WeaponList)
            {
                UpdateGridDataValueByWeaponData(weaponItem.WeaponData.OccupyList,weaponItem.PosIndex.x,weaponItem.PosIndex.y,weaponItem.WeaponData.WeaponId,-1);
            }
        }
        /// <summary>
        /// 背包数据变化监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackpackDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Manager.DragState))
            {
                DragState = Manager.DragState;
            }
            else if (e.PropertyName == nameof(Manager.GridSize))
            {
                if (Manager.GridSize.x == GridSize.x && Manager.GridSize.y == GridSize.y) return;
                // 播放格子放大缩小动画
                var startSize = GridSize;
                var endSize = Manager.GridSize;
                gridAniTweener?.Kill();
                gridAniTweener = DOTween.To(()=>startSize, paramVec =>
                {
                    GridSize = new Vector2Int((int)paramVec.x,(int)paramVec.y);
                },endSize, 0.1f).OnComplete(() => {
                    GridSize = Manager.GridSize;
                });
            }
        }
        /// <summary>
        /// 装备数据变化监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EquipDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataManager.RandomRefreshCount))
            {
                RefreshWeaponList();
            } 
            else if (e.PropertyName == nameof(DataManager.Wish))
            {
                if (DataManager.Wish.Count == 0)
                {
                    WishVm.WeaponData = null;
                }
                else
                {
                    var curWeaponId = DataManager.ParseWishDataId(DataManager.Wish[0]);
                    var curWeaponAttrType = DataManager.ParseWishType(DataManager.Wish[0]);
                    var weaponData =  DataManager.CreateBackpackData(curWeaponId*10+1,(int)curWeaponAttrType,ELocationType.WishPool);
                    WishVm.WeaponData = weaponData;
                }
            }
            else if (DataManager != null && e.PropertyName == nameof(DataManager.WaveEnd))//波次结算播放背包中钱袋飞金币动效
            {
                if (!DataManager.WaveEnd)
                {
                    if (WeaponList.Count > 0) BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
                }
                else
                {
                    if (DataManager.CurStageType != EStateType.StageTypeMainStage) return;
                    var copyCfg = ConfigCenter.CopyCfgColl.GetDataById(Manager.CurChapterId);
                    if (DataManager.Wave >= copyCfg.LevelWaves) return;
                    foreach (var weaponItem in WeaponList)
                    {
                        if (weaponItem.WeaponData.EquipId != Manager.MoneyBagEquipId) continue; //如果不是钱袋
                        var moneyCfg = ConfigCenter.EquipModelCfgColl.GetDataById(weaponItem.WeaponData.WeaponId);
                        if (moneyCfg is not { CoinNum: > 0 } || weaponItem.WeaponNodeRect == null) continue;
                        var flyCoinNum = moneyCfg.CoinNum > 30 ? 30 : moneyCfg.CoinNum;
                        UIEffectManager.Instance.PlayerGameCoinAction(weaponItem.WeaponNodeRect.position,flyCoinNum, true,null,false).Forget();
                    }
                }
            }
        }
        protected override void OnDispose()
        {
            Input.multiTouchEnabled = true;
            GameManager.Instance.mainMergeVMInstance = null;
            DataManager.PropertyChanged -= EquipDataPropertyChanged;
            Manager.PropertyChanged -= BackpackDataPropertyChanged;
            base.OnDispose();
        }
        private void NotifyGuideStopAction()
        {
            GuideManager.Instance.StopGuideAction();
        }
    }
}