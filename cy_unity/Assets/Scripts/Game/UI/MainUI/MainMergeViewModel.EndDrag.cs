using System.Collections.Generic;
using DH.Game.UI;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Asset;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DH.Game.ViewModels
{
    public partial class MainMergeViewModel
    {
        /// <summary>
        /// 物品拖拽结束
        /// </summary>
        /// <param name="eventData"></param>
        private void OnEndDragHandle(PointerEventData eventData)
        {
            if (eventData == null) return;
            if (eventPointerId != eventData.pointerId) return;
            Manager.LaseDragModelId = Manager.DragWeaponId;
            Manager.DragWeaponId = 0;
            Manager.DragUid = 0;
            if (!isCanDrag) return;
            dragRealTimeWorldPos = AppGlobal.Instance.UICamera.ScreenToWorldPoint(eventData.position);
            if (Manager.DragState == EDragState.BlockAdd && DragGridAddItemVm != null)//拖动新增格子
            {
                AudioManager.Instance.PlayAudio("SFX_UI/ui_game_drop");
                DragEndGridAddLogicHandle(eventData.position);
                ResetBlocks();
            }
            else if (Manager.DragState == EDragState.DragBatch && DragBatchItemVm != null && DragBatchItemVm.GridList.Count > 0)  //批量拖动格子，进行放置
            {
                AudioManager.Instance.PlayAudio("SFX_UI/ui_game_drop");
                Manager.BlockState = EBlockState.AddCell;
                DragEndBatchLogicHandle(eventData.position);
                DragBatchItemVm.WeaponList.Clear();
                DragBatchItemVm.GridList.Clear();
                DragBatchItemVm.Alpha = 0;
            }
            else if (Manager.DragState == EDragState.Weapon && DragWeaponItemVm != null) //拖动后放置武器
            {
                AudioManager.Instance.PlayAudio("SFX_UI/ui_game_drop");
                Manager.DragState = EDragState.None;
                DragEndWeaponLogicHandle(eventData.position);
                DragWeaponItemVm.Alpha = 0;
                DragWeaponItemVm.UpdateMaxWeaponEffect(false, "").Forget();
                ResetBlocks();
                isCanDrag = false;
            }
        }
         /// <summary>
        /// 新增格子拖拽结束后的逻辑处理
        /// 1、由随机库到棋盘上放置且放置的位置是否合法
        /// 2、由随机库回到随机库
        /// 3、由棋盘到棋盘位置移动且判断是否合法
        /// </summary>
        /// <param name="screenPos"></param>
        private void DragEndGridAddLogicHandle(Vector2 screenPos)
        {
            var occupyInfo = GetDragItemOccupyCells(screenPos,false);
            if (occupyInfo.Item1 && occupyInfo.Item2.Count > 0) //必然在棋盘上
            {
                blockAddOccupyCells = occupyInfo.Item2;
                if (DragGridAddItemVm.GridAddData.LocationType == ELocationType.Godown)//移除数据层的数据
                {
                    Manager.RemoveRandomWeaponData(DragGridAddItemVm.GridAddData.GridParamId,1);
                    DragGridAddItemVm.GridAddData.LocationType = ELocationType.Backpack;
                }
                var tmpLocalPos = Manager.CalculationLocalPos(occupyInfo.Item2[0],DragGridAddItemVm.GridAddData.ShapType);
                var worldPos = GridListRect.TransformPoint(tmpLocalPos);
                var resultLocalPos= GridAddItemRect.InverseTransformPoint(worldPos);
                resultLocalPos.z = 0;
                DragGridAddItemVm.PosIndex = occupyInfo.Item2[0];
                DragGridAddItemVm.GridNodeRect.localPosition = resultLocalPos;
            }
            else
            {
                blockAddOccupyCells?.Clear();
                //在棋盘上但是不满足放置条件
                if (DragGridAddItemVm.GridAddData.LocationType == ELocationType.Backpack)//回到棋盘上次位置
                {
                    var tmpLocalPos = Manager.CalculationLocalPos(DragGridAddItemVm.PosIndex,
                        DragGridAddItemVm.GridAddData.ShapType);
                    var worldPos = GridListRect.TransformPoint(tmpLocalPos);
                    var resultLocalPos= GridAddItemRect.InverseTransformPoint(worldPos);
                    resultLocalPos.z = 0;
                    var tmpOccupyCells = new List<Vector2Int>(35);
                    for (var i = 0; i < DragGridAddItemVm.GridAddData.OccupyList.Count; i++)
                    {
                        var tmpInfo = new Vector2Int{x = DragGridAddItemVm.GridAddData.OccupyList[i].x+DragGridAddItemVm.PosIndex.x,
                            y = DragGridAddItemVm.GridAddData.OccupyList[i].y+DragGridAddItemVm.PosIndex.y};
                        tmpOccupyCells.Add(tmpInfo);
                    }
                    blockAddOccupyCells = tmpOccupyCells;
                    DragGridAddItemVm.GridNodeRect.localPosition = resultLocalPos;
                }
                else if (DragGridAddItemVm.GridAddData.LocationType == ELocationType.Godown)
                {
                    var tmVm = new RandomItemViewModel(null, DragGridAddItemVm.GridAddData);
                    RandomBagList.Add(tmVm);
                    DragGridAddItemVm.Alpha = 0;
                }
            }
            isCanDrag = false;
        }
        /// <summary>
        /// 批量拖拽格子结束后操作逻辑处理
        /// </summary>
        /// <param name="screenPos"></param>
        private void DragEndBatchLogicHandle(Vector2 screenPos)
        {
            var occupyInfo = GetBatchDragOccupyInfo(screenPos);
            if (occupyInfo.Item1 && occupyInfo.Item2.Count > 0)//批量拖动匹配有效且占用格子数为不0
            {
                //满足放置条件，而且有位置移动
                var offsetX = occupyInfo.Item2[0].x-DragBatchItemVm.GridList[0].IndexPos.x;
                var offsetY = occupyInfo.Item2[0].y-DragBatchItemVm.GridList[0].IndexPos.y;
                UpdateGridValueByBatchGrid(0);
                UpdateGridDataValueByWeaponData(occupyInfo.Item2,0,0,1,-1);
                DataManager.GridBorderRect = Manager.GetMaxGridBorder();
                UpdateWeaponList(offsetX,offsetY);
                DragBatchItemVm.WeaponList.Clear();
                MakeWeaponTransparent(0.55f);
            }
            else
            {
                DataManager.GridBorderRect = Manager.GetMaxGridBorder();
                UpdateGridValueByBatchGrid(1);
                if (DragBatchItemVm != null && DragBatchItemVm.WeaponList.Count > 0)
                {
                    WeaponList.Clear();
                    foreach (var weaponItem in DragBatchItemVm.WeaponList)
                    {
                        var tempWeaponVm = new WeaponItemViewModel(weaponItem.WeaponData);
                        tempWeaponVm.PosIndex = new Vector2Int { x = tempWeaponVm.WeaponData.RowIdx,y = tempWeaponVm.WeaponData.ColumnIdx};
                        tempWeaponVm.UpdatePosition();
                        WeaponList.Add(tempWeaponVm);
                    }
                }
            }
            ResetBlocks();
            isCanDrag = false;
        }
        /// <summary>
        /// 武器拖动结束后逻辑处理，一下几种情况要处理
        /// 1、由随机库到棋盘上放置或合成
        /// 2、由棋盘到随机库卸下或合成
        /// 3、由棋盘到棋盘位置移动或合成
        /// 4、由随机库到随机库位置移动或合成
        /// </summary>
        /// <param name="screenPos"></param>
        private void DragEndWeaponLogicHandle(Vector2 screenPos)
        {
            var dragWeaponData = DragWeaponItemVm.WeaponData;
            var occupyInfo = GetDragItemOccupyCells(screenPos,true);
            if (occupyInfo.Item1 && occupyInfo.Item2.Count > 0) //必定在棋盘内即：随机库+棋盘拖动到棋盘的情况已考虑（1、3情况）
            {
                //是否升级
                WeaponItemViewModel toUpgradeWeapon = null;
                foreach (var weaponItem in WeaponList)
                {
                    var curWeaponData = weaponItem.WeaponData;
                    var realOccupy = weaponItem.GetRunningCellOccupy();
                    if (curWeaponData.WeaponId == dragWeaponData.WeaponId && curWeaponData.WeaponLev == dragWeaponData.WeaponLev && !Manager.CheckWeaponMaxLevel(curWeaponData.WeaponId,curWeaponData.EquipId)
                        && occupyInfo.Item2[0].x == realOccupy[0].x && occupyInfo.Item2[0].y == realOccupy[0].y && curWeaponData.NextInfo.Count > 0)  //且不能是最高级
                    {
                        toUpgradeWeapon = weaponItem;
                        break;
                    }
                }
                if (toUpgradeWeapon != null && toUpgradeWeapon.WeaponData.NextInfo.Count > 1 && Manager.CheckWeaponDoubleAttrUnlocked(toUpgradeWeapon.WeaponData.EquipId))//双属性合成
                {
                    //判断是否已选
                    var selectId = DataManager.GetEquipAdvanceEquipModelIdByEquipId(toUpgradeWeapon.WeaponData.EquipId);
                    if (selectId > 0)
                    {
                        WeaponUpgradeLogic(toUpgradeWeapon, selectId);
                    }
                    else
                    {
                        var equipAdvanceVm = new EquipAdvanceViewModel(toUpgradeWeapon.WeaponData.EquipId);
                        equipAdvanceVm.ChooseCallback = (tmpWeaponId) =>
                        {
                            if (GameDataManager.Instance.CurStageType == EStateType.StageTypeEndless)//无尽模式下动态添加玩家武器天赋
                            {
                                var tmpTalentId = DataManager.GetEndlessTalentId(tmpWeaponId);
                                if (tmpTalentId > 0)
                                {
                                    GameDataManager.Instance.AddChooseTalent(tmpTalentId);
                                    BattleManager.Instance.fightingManagerIns.playerCtrl.Player.AddTalent(tmpTalentId);
                                }
                            }
                            WeaponUpgradeLogic(toUpgradeWeapon, tmpWeaponId);
                            BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
                        };
                        UIManager.Instance.OpenDialog<EquipAdvanceView>(equipAdvanceVm).Forget();
                        GameTime.Instance.Pause = true;
                    }
                } 
                else if (toUpgradeWeapon != null && toUpgradeWeapon.WeaponData.NextInfo.Count == 1)
                {
                    var nextWeaponId = toUpgradeWeapon.WeaponData.NextInfo[0];
                    WeaponUpgradeLogic(toUpgradeWeapon, nextWeaponId);
                }
                else
                {
                    if (toUpgradeWeapon != null && !Manager.CheckWeaponDoubleAttrUnlocked(toUpgradeWeapon.WeaponData.EquipId))
                    {
                        // 这里给提示
                        ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips42));
                    }
                    RemoveWeapons(occupyInfo.Item2);//移除对应格子的武器
                    UpdateGridDataValueByWeaponData(occupyInfo.Item2,0,0,DragWeaponItemVm.WeaponData.WeaponId,-1);
                    if (DragWeaponItemVm.WeaponData.LocationType == ELocationType.Godown)//如果是从随机库中拖出来的，需要把随机库中的数据删除
                    {
                        Manager.RemoveRandomWeaponData(DragWeaponItemVm.WeaponData.WeaponParamId,DragWeaponItemVm.WeaponData.WeaponAttrType);
                    } else if (DragWeaponItemVm.WeaponData.LocationType == ELocationType.WishPool)//如果是从心愿池拖出来，移除心愿池数据
                    {
                        Manager.RemoveWeaponFromWishPool(DragWeaponItemVm.WeaponData.WeaponId,DragWeaponItemVm.WeaponData.WeaponAttrType);
                    }
                    DragWeaponItemVm.WeaponData.LocationType = ELocationType.Backpack;
                    var weaponVm = new WeaponItemViewModel(DragWeaponItemVm.WeaponData);
                    weaponVm.PosIndex = occupyInfo.Item2[0];
                    weaponVm.WeaponData.RowIdx = occupyInfo.Item2[0].x;
                    weaponVm.WeaponData.ColumnIdx = occupyInfo.Item2[0].y;
                    WeaponList.Add(weaponVm);
                    weaponVm.UpdateIcon();
                    if (GameDataManager.Instance.CheckIsAttributeGainWeapon(weaponVm.WeaponData.EquipId))
                    {
                        PlayerGainWeaponEffectLogic(weaponVm.WeaponData,occupyInfo.Item2);
                    }
                    Manager.AddWeaponToBackpack(weaponVm.WeaponData);
                }
                BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
            }
            else
            {
                DragEndWeaponOutOfGridLogicHandle(screenPos);//拖动到棋盘外的逻辑处理
            }
        }
        /// <summary>
        /// 刷新批量格子的数据
        /// </summary>
        /// <param name="gridValue"></param>
        private void UpdateGridValueByBatchGrid(int gridValue)
        {
            if (DragBatchItemVm == null || DragBatchItemVm.GridList.Count == 0) return;
            for (var i = DragBatchItemVm.GridList.Count - 1; i >= 0; i--)
            {
                var tmpIdxPos = DragBatchItemVm.GridList[i].IndexPos;
                DataManager.GridData[tmpIdxPos.x][tmpIdxPos.y] = gridValue;
            }
        }
        /// <summary>
        /// 武器拖动结束于棋盘外的逻辑处理
        /// </summary>
        /// <param name="screenPos"></param>
        private void DragEndWeaponOutOfGridLogicHandle(Vector2 screenPos)
        {
            var dragWeaponData = DragWeaponItemVm.WeaponData;
            RandomItemViewModel refreshUpgradeWeapon = null;
            var upgradeIdx = -1;
            for (var i = 0; i < RandomBagList.Count; i++)
            {
                if (RandomBagList[i] == null) continue;
                if (RandomBagList[i].GridAddData != null) continue;
                if (!RandomBagList[i].IsClickInArea(screenPos)) continue;
                //放到武器刷新区域内
                if (dragWeaponData.WeaponId == RandomBagList[i].WeaponData.WeaponId && !Manager.CheckWeaponMaxLevel(RandomBagList[i].WeaponData.WeaponId,RandomBagList[i].WeaponData.EquipId)
                    && RandomBagList[i].WeaponData.NextInfo.Count > 0 && dragWeaponData.WeaponLev == RandomBagList[i].WeaponData.WeaponLev) //武器合并升级
                {
                    upgradeIdx = i;
                    refreshUpgradeWeapon = RandomBagList[i];
                    break;
                }
            }
            if (refreshUpgradeWeapon is { WeaponData: not null } && refreshUpgradeWeapon.WeaponData.NextInfo.Count > 1 && upgradeIdx >= 0
                && Manager.CheckWeaponDoubleAttrUnlocked(refreshUpgradeWeapon.WeaponData.EquipId))  //武器刷新区升级
            {
                //判断是否已选
                var selectId = DataManager.GetEquipAdvanceEquipModelIdByEquipId(refreshUpgradeWeapon.WeaponData.EquipId);
                if (selectId > 0)
                {
                    RandomWeaponUpgradeLogic(refreshUpgradeWeapon, selectId,upgradeIdx);
                }
                else
                {
                    var equipAdvanceVm = new EquipAdvanceViewModel(refreshUpgradeWeapon.WeaponData.EquipId);
                    equipAdvanceVm.ChooseCallback = (tmpWeaponId) =>
                    {
                        RandomWeaponUpgradeLogic(refreshUpgradeWeapon, tmpWeaponId,upgradeIdx);
                        BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
                    };
                    UIManager.Instance.OpenDialog<EquipAdvanceView>(equipAdvanceVm).Forget();
                    GameTime.Instance.Pause = true;
                }
            } 
            else if (refreshUpgradeWeapon is { WeaponData: not null } && refreshUpgradeWeapon.WeaponData.NextInfo.Count == 1 && upgradeIdx >= 0)  //武器刷新区升级
            {
                var weaponId = refreshUpgradeWeapon.WeaponData.NextInfo[0];
                var weaponCfg = ConfigCenter.EquipModelCfgColl.GetDataById(weaponId);
                if (weaponCfg == null) return;
                var tmpWeaponData = DataManager.CreateBackpackData(weaponId*10+1,1,ELocationType.Godown);
                if (tmpWeaponData == null) return;
                Manager.RemoveRandomWeaponData(refreshUpgradeWeapon.WeaponData.WeaponParamId,refreshUpgradeWeapon.WeaponData.WeaponAttrType);
                if (DragWeaponItemVm.WeaponData.LocationType == ELocationType.Backpack)//移除当前拖动的数据
                {
                    Manager.RemoveWeaponFromBackpack(DragWeaponItemVm.WeaponData);
                    UpdateGridDataValueByWeaponData(DragWeaponItemVm.WeaponData.OccupyList,DragWeaponItemVm.PosIndex.x,DragWeaponItemVm.PosIndex.y,1,1);
                }
                else if (DragWeaponItemVm.WeaponData.LocationType == ELocationType.WishPool)//如果是从心愿池拖出来，移除心愿池数据
                {
                    Manager.RemoveWeaponFromWishPool(DragWeaponItemVm.WeaponData.WeaponId,DragWeaponItemVm.WeaponData.WeaponAttrType);
                }
                else
                {
                    Manager.RemoveRandomWeaponData(DragWeaponItemVm.WeaponData.WeaponParamId,DragWeaponItemVm.WeaponData.WeaponAttrType);
                }
                if (DataManager.GetEquipCfgData(tmpWeaponData.EquipId)?.AtkType == (int)EquipAtkType.Magic && GameDataManager.Instance.MagicEquipMergedGainValue > 0)//魔法武器
                {
                    DataManager.MagicMergeNum += 1;
                    UIEffectManager.Instance.PlayerGameCoinAction(
                        RandomBagList[upgradeIdx].RandomNodeRect.position,1, true,null,false).Forget();
                    GameManager.Instance.CheckMagicEquipMergedGain();
                }
                else if (DataManager.GetEquipCfgData(tmpWeaponData.EquipId)?.AtkType == (int)EquipAtkType.Physic && GameDataManager.Instance.PhyEquipMergedGainValue > 0)//物理武器
                {
                    DataManager.PhysicsMergeNum += 1;
                    UIEffectManager.Instance.PlayerGameCoinAction(
                        RandomBagList[upgradeIdx].RandomNodeRect.position,1, true,null,false).Forget();
                    GameManager.Instance.CheckPhyEquipMergedGain();
                }
                RandomBagList[upgradeIdx].UpdateRandomData(tmpWeaponData,null);
                RandomBagList[upgradeIdx].PlayRandomUpgradeEffect().Forget();
                ResetGainWeaponEffectLogic();
                AudioManager.Instance.PlayAudio($"SFX_UI/ui_game_merge0{tmpWeaponData.WeaponLev}");
                BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
            }
            else
            {
                if (DragWeaponItemVm.WeaponData.LocationType == ELocationType.Backpack)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(GridListRect,screenPos,AppGlobal.Instance.UICamera))//如果在棋盘区域，就返回棋盘的原始位置
                    {
                        var weaponVm = new WeaponItemViewModel(DragWeaponItemVm.WeaponData);
                        weaponVm.PosIndex = startIdxPos;
                        weaponVm.WeaponData.RowIdx = startIdxPos.x;
                        weaponVm.WeaponData.ColumnIdx = startIdxPos.y;
                        WeaponList.Add(weaponVm);
                        weaponVm.UpdateIcon();
                        ResetGainWeaponEffectLogic();
                        BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
                    }
                    else if (RectTransformUtility.RectangleContainsScreenPoint(WishVm.WishNodeRect,screenPos,AppGlobal.Instance.UICamera) && !WishVm.IsShowLockNode && WishVm.WeaponData == null)
                    {
                        WishVm.WeaponData = DragWeaponItemVm.WeaponData;
                        Manager.AddWeaponToWishPool(DragWeaponItemVm.WeaponData.WeaponId,DragWeaponItemVm.WeaponData.WeaponAttrType);
                        ResetGainWeaponEffectLogic();
                        UpdateGridDataValueByWeaponData(DragWeaponItemVm.WeaponData.OccupyList,DragWeaponItemVm.PosIndex.x,DragWeaponItemVm.PosIndex.y,1,1);
                        Manager.RemoveWeaponFromBackpack(DragWeaponItemVm.WeaponData);
                        BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
                    } 
                    else if (RectTransformUtility.RectangleContainsScreenPoint(WishVm.WishNodeRect, screenPos, AppGlobal.Instance.UICamera) && !WishVm.IsShowLockNode && WishVm.WeaponData != null)
                    {
                        var actionIconPath = WishVm.WeaponData.IconPath;
                        Manager.RemoveWeaponFromWishPool(WishVm.WeaponData.WeaponId, WishVm.WeaponData.WeaponAttrType);
                        // 检查是否可以升级
                        if (dragWeaponData.WeaponId == WishVm.WeaponData.WeaponId && !Manager.CheckWeaponMaxLevel(WishVm.WeaponData.WeaponId, WishVm.WeaponData.EquipId) && WishVm.WeaponData.NextInfo.Count > 0 && dragWeaponData.WeaponLev == WishVm.WeaponData.WeaponLev)
                        {
                            // 双属性检查
                            if (Manager.CheckWeaponDoubleAttrUnlocked(WishVm.WeaponData.EquipId) && WishVm.WeaponData.NextInfo.Count > 1)
                            {
                                //判断是否已选
                                var selectId = DataManager.GetEquipAdvanceEquipModelIdByEquipId(WishVm.WeaponData.EquipId);
                                if (selectId > 0)
                                {
                                    WeaponUpgradeWishLogic(WishVm.WeaponData, selectId);
                                }
                                else
                                {
                                    var equipAdvanceVm = new EquipAdvanceViewModel(WishVm.WeaponData.EquipId);
                                    equipAdvanceVm.ChooseCallback = (tmpWeaponId) =>
                                    {
                                        if (GameDataManager.Instance.CurStageType == EStateType.StageTypeEndless)//无尽模式下动态添加玩家武器天赋
                                        {
                                            var tmpTalentId = DataManager.GetEndlessTalentId(tmpWeaponId);
                                            if (tmpTalentId > 0)
                                            {
                                                GameDataManager.Instance.AddChooseTalent(tmpTalentId);
                                                BattleManager.Instance.fightingManagerIns.playerCtrl.Player.AddTalent(tmpTalentId);
                                            }
                                        }
                                        WeaponUpgradeWishLogic(WishVm.WeaponData, tmpWeaponId);
                                        BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
                                    };
                                    UIManager.Instance.OpenDialog<EquipAdvanceView>(equipAdvanceVm).Forget();
                                    GameTime.Instance.Pause = true;
                                }
                            }
                            else
                            {
                                // 直接更新
                                WeaponUpgradeWishLogic(WishVm.WeaponData, WishVm.WeaponData.NextInfo[0]);
                            }
                        }
                        else
                        {
                            var randomItemVm = new RandomItemViewModel(WishVm.WeaponData,null,0);
                            WishVm.WeaponData = DragWeaponItemVm.WeaponData;
                            Manager.AddWeaponToWishPool(DragWeaponItemVm.WeaponData.WeaponId,DragWeaponItemVm.WeaponData.WeaponAttrType);
                            //棋盘格子恢复
                            UpdateGridDataValueByWeaponData(DragWeaponItemVm.WeaponData.OccupyList,DragWeaponItemVm.PosIndex.x,DragWeaponItemVm.PosIndex.y,1,1);
                            randomItemVm.WeaponData.LocationType = ELocationType.Godown;
                            randomItemVm.SetRandomIconHide(false);
                            randomItemVm.WeaponData.WeaponParamId = randomItemVm.WeaponData.WeaponId * 10 + 1;
                            RandomBagList.Insert(0,randomItemVm);
                            PlayWeaponToRandom(WishVm.WishNodeRect.position,actionIconPath,randomItemVm).Forget();//播放移除动画
                        }
                        ResetGainWeaponEffectLogic();
                        Manager.RemoveWeaponFromBackpack(DragWeaponItemVm.WeaponData);
                        BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
                    }
                    else//否则移入随机库，即卸下
                    {
                        if (refreshUpgradeWeapon != null && refreshUpgradeWeapon.WeaponData!= null && !Manager.CheckWeaponDoubleAttrUnlocked(refreshUpgradeWeapon.WeaponData.EquipId))
                        {
                            // 这里给提示
                            ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips42));
                        }
                        var randomItemVm = new RandomItemViewModel(DragWeaponItemVm.WeaponData,null);
                        //棋盘格子恢复
                        UpdateGridDataValueByWeaponData(DragWeaponItemVm.WeaponData.OccupyList,DragWeaponItemVm.PosIndex.x,DragWeaponItemVm.PosIndex.y,1,1);
                        randomItemVm.WeaponData.LocationType = ELocationType.Godown;
                        randomItemVm.WeaponData.WeaponParamId = randomItemVm.WeaponData.WeaponId * 10 + 1;
                        RandomBagList.Add(randomItemVm);
                        ResetGainWeaponEffectLogic();
                        Manager.AddRandomWeaponData(randomItemVm.WeaponData.WeaponId*10+1,1);
                        Manager.RemoveWeaponFromBackpack(randomItemVm.WeaponData);
                        BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
                    }
                }
                else if (DragWeaponItemVm.WeaponData.LocationType == ELocationType.Godown && RectTransformUtility.RectangleContainsScreenPoint(WishVm.WishNodeRect,screenPos,AppGlobal.Instance.UICamera) && !WishVm.IsShowLockNode && WishVm.WeaponData == null)
                {
                    WishVm.WeaponData = DragWeaponItemVm.WeaponData;
                    Manager.AddWeaponToWishPool(DragWeaponItemVm.WeaponData.WeaponId,DragWeaponItemVm.WeaponData.WeaponAttrType);
                    ResetGainWeaponEffectLogic();
                    Manager.RemoveRandomWeaponData(DragWeaponItemVm.WeaponData.WeaponId,DragWeaponItemVm.WeaponData.WeaponAttrType);
                    BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
                } 
                else if (DragWeaponItemVm.WeaponData.LocationType == ELocationType.Godown && RectTransformUtility.RectangleContainsScreenPoint(WishVm.WishNodeRect, screenPos, AppGlobal.Instance.UICamera) && !WishVm.IsShowLockNode && WishVm.WeaponData != null)
                {
                    var actionIconPath = WishVm.WeaponData.IconPath;
                    Manager.RemoveWeaponFromWishPool(WishVm.WeaponData.WeaponId, WishVm.WeaponData.WeaponAttrType);
                    // 检查是否可以升级
                    if (dragWeaponData.WeaponId == WishVm.WeaponData.WeaponId && !Manager.CheckWeaponMaxLevel(WishVm.WeaponData.WeaponId, WishVm.WeaponData.EquipId) && WishVm.WeaponData.NextInfo.Count > 0 && dragWeaponData.WeaponLev == WishVm.WeaponData.WeaponLev)
                    {
                        // 双属性检查
                        if (Manager.CheckWeaponDoubleAttrUnlocked(WishVm.WeaponData.EquipId) 
                            && WishVm.WeaponData.NextInfo.Count > 1)
                        {
                            //判断是否已选
                            var selectId = DataManager.GetEquipAdvanceEquipModelIdByEquipId(WishVm.WeaponData.EquipId);
                            if (selectId > 0)
                            {
                                WeaponUpgradeWishLogic(WishVm.WeaponData, selectId);
                            }
                            else
                            {
                                var equipAdvanceVm = new EquipAdvanceViewModel(WishVm.WeaponData.EquipId);
                                equipAdvanceVm.ChooseCallback = (tmpWeaponId) =>
                                {
                                    if (GameDataManager.Instance.CurStageType == EStateType.StageTypeEndless)//无尽模式下动态添加玩家武器天赋
                                    {
                                        var tmpTalentId = DataManager.GetEndlessTalentId(tmpWeaponId);
                                        if (tmpTalentId > 0)
                                        {
                                            GameDataManager.Instance.AddChooseTalent(tmpTalentId);
                                            BattleManager.Instance.fightingManagerIns.playerCtrl.Player.AddTalent(tmpTalentId);
                                        }
                                    }
                                    WeaponUpgradeWishLogic(WishVm.WeaponData, tmpWeaponId);
                                    BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
                                };
                                UIManager.Instance.OpenDialog<EquipAdvanceView>(equipAdvanceVm).Forget();
                                GameTime.Instance.Pause = true;
                            }
                        }
                        else
                        {
                            // 直接更新
                            WeaponUpgradeWishLogic(WishVm.WeaponData, WishVm.WeaponData.NextInfo[0]);
                        }
                    }
                    else
                    {
                        var randomItemVm = new RandomItemViewModel( WishVm.WeaponData,null,0);
                        WishVm.WeaponData = DragWeaponItemVm.WeaponData;
                        Manager.AddWeaponToWishPool(DragWeaponItemVm.WeaponData.WeaponId,DragWeaponItemVm.WeaponData.WeaponAttrType);
                        randomItemVm.WeaponData.LocationType = ELocationType.Godown;
                        randomItemVm.SetRandomIconHide(false);
                        randomItemVm.WeaponData.WeaponParamId = randomItemVm.WeaponData.WeaponId * 10 + 1;
                        RandomBagList.Insert(0,randomItemVm);
                        PlayWeaponToRandom(WishVm.WishNodeRect.position,actionIconPath,randomItemVm).Forget();//播放移除动画
                    }
                    ResetGainWeaponEffectLogic();
                    BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
                    
                }
                else if (DragWeaponItemVm.WeaponData.LocationType == ELocationType.WishPool)
                {
                    if (!RectTransformUtility.RectangleContainsScreenPoint(WishVm.WishNodeRect,screenPos,AppGlobal.Instance.UICamera)) //移入随机库，即卸下
                    {
                        var randomItemVm = new RandomItemViewModel(DragWeaponItemVm.WeaponData,null);
                        randomItemVm.WeaponData.LocationType = ELocationType.Godown;
                        randomItemVm.WeaponData.WeaponParamId = randomItemVm.WeaponData.WeaponId * 10 + 1;
                        RandomBagList.Add(randomItemVm);
                        ResetGainWeaponEffectLogic();
                        Manager.AddRandomWeaponData(randomItemVm.WeaponData.WeaponId*10+1,1);
                        BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
                    }
                    else//回到心愿池
                    {
                        WishVm.WeaponData = DragWeaponItemVm.WeaponData;
                        Manager.AddWeaponToWishPool(DragWeaponItemVm.WeaponData.WeaponId,DragWeaponItemVm.WeaponData.WeaponAttrType);
                        ResetGainWeaponEffectLogic();
                        BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
                    }
                }
                else//随机库没有变动
                {
                 
                    if (refreshUpgradeWeapon is { WeaponData: not null } && !Manager.CheckWeaponDoubleAttrUnlocked(refreshUpgradeWeapon.WeaponData.EquipId))
                    {
                        ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips42));  // 这里给提示
                    }
                    ResetGainWeaponEffectLogic();
                    DragWeaponItemVm.WeaponData.LocationType = ELocationType.Godown;
                    var randomItemVm = new RandomItemViewModel(DragWeaponItemVm.WeaponData, null);
                    RandomBagList.Add(randomItemVm);
                }
            }
        }
        /// <summary>
        /// 移除武器
        /// </summary>
        /// <param name="occupyCells"></param>
        private void RemoveWeapons(List<Vector2Int> occupyCells)
        {
            var conflictList = new List<long>(35);
            foreach (var curWeaponVm in WeaponList)
            {
                if (!CheckOccupyConflict(curWeaponVm.GetRunningCellOccupy(),occupyCells)) continue;//不冲突继续检测
                curWeaponVm.Alpha = 0;
                conflictList.Add(curWeaponVm.WeaponData.Uid);
            }
            if (conflictList.Count > 0)
            {
                DragWeaponItemVm.Alpha = 0;
                foreach (var conflictItem in conflictList)
                {
                    var curIdx = GetWeaponItemIndexByUid(conflictItem);
                    if (curIdx < 0) continue;
                    var curWeaponVm = WeaponList[curIdx];
                    if (curWeaponVm == null) continue;
                    curWeaponVm.UpdateMaxWeaponEffect(false, "").Forget();
                    UpdateGridDataValueByWeaponData(curWeaponVm.WeaponData.OccupyList,curWeaponVm.PosIndex.x,curWeaponVm.PosIndex.y,1,1);
                    curWeaponVm.WeaponData.LocationType = ELocationType.Godown;
                    var curRandomItemVm = new RandomItemViewModel(curWeaponVm.WeaponData, null,0);
                    curRandomItemVm.SetRandomIconHide(false);
                    curRandomItemVm.WeaponData.WeaponParamId = curRandomItemVm.WeaponData.WeaponId * 10 + 1;
                    RandomBagList.Add(curRandomItemVm);
                    Manager.AddRandomWeaponData(curRandomItemVm.WeaponData.WeaponId*10+1,1);//添加到随机库
                    Manager.RemoveWeaponFromBackpack(curRandomItemVm.WeaponData);//从背包中移除
                    PlayWeaponToRandom(curWeaponVm.WeaponNodeRect.position,curWeaponVm.IconPathStr,curRandomItemVm).Forget();//播放移除动画
                    WeaponList.RemoveAt(curIdx);
                }
            }
            //如果当前武器是棋盘数据，需要把原来位置格子数据置为1
            if (DragWeaponItemVm.WeaponData.LocationType != ELocationType.Backpack) return;
            var oldStartPosX = DragWeaponItemVm.WeaponData.RowIdx;
            var oldStartPosY = DragWeaponItemVm.WeaponData.ColumnIdx;
            UpdateGridDataValueByWeaponData(DragWeaponItemVm.WeaponData.OccupyList,oldStartPosX,oldStartPosY,1,-1);
        }
        /// <summary>
        /// 随机库武器升级逻辑处理
        /// </summary>
        /// <param name="toUpgradeWeapon"></param>
        /// <param name="nextId"></param>
        /// <param name="upgradeIdx"></param>
        private void RandomWeaponUpgradeLogic(RandomItemViewModel toUpgradeWeapon,int nextId,int upgradeIdx)
        {
            var weaponCfg = ConfigCenter.EquipModelCfgColl.GetDataById(nextId);
            if (weaponCfg == null) return;
            var tmpWeaponData = DataManager.CreateBackpackData(nextId*10+1,1,ELocationType.Godown);
            if (tmpWeaponData == null) return;
            Manager.RemoveRandomWeaponData(toUpgradeWeapon.WeaponData.WeaponParamId,toUpgradeWeapon.WeaponData.WeaponAttrType);
            if (DragWeaponItemVm.WeaponData.LocationType == ELocationType.Backpack)//移除当前拖动的数据
            {
                Manager.RemoveWeaponFromBackpack(DragWeaponItemVm.WeaponData);
                UpdateGridDataValueByWeaponData(DragWeaponItemVm.WeaponData.OccupyList,DragWeaponItemVm.PosIndex.x,DragWeaponItemVm.PosIndex.y,1,1);
            }
            else
            {
                Manager.RemoveRandomWeaponData(DragWeaponItemVm.WeaponData.WeaponParamId,DragWeaponItemVm.WeaponData.WeaponAttrType);
            }
            if (DataManager.GetEquipCfgData(tmpWeaponData.EquipId)?.AtkType == (int)EquipAtkType.Magic && GameDataManager.Instance.MagicEquipMergedGainValue > 0)//魔法武器
            {
                DataManager.MagicMergeNum += 1;
                UIEffectManager.Instance.PlayerGameCoinAction(RandomBagList[upgradeIdx].RandomNodeRect.position,1, true,null,false).Forget();
                GameManager.Instance.CheckMagicEquipMergedGain();
            }
            else if (DataManager.GetEquipCfgData(tmpWeaponData.EquipId)?.AtkType == (int)EquipAtkType.Physic && GameDataManager.Instance.PhyEquipMergedGainValue > 0)//物理武器
            {
                DataManager.PhysicsMergeNum += 1;
                UIEffectManager.Instance.PlayerGameCoinAction(RandomBagList[upgradeIdx].RandomNodeRect.position,1, true,null,false).Forget();
                GameManager.Instance.CheckPhyEquipMergedGain();
            }
            RandomBagList[upgradeIdx].UpdateRandomData(tmpWeaponData,null);
            RandomBagList[upgradeIdx].PlayRandomUpgradeEffect().Forget();
            ResetGainWeaponEffectLogic();
            AudioManager.Instance.PlayAudio($"SFX_UI/ui_game_merge0{tmpWeaponData.WeaponLev}");
            BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
        }
        /// <summary>
        /// 武器升级逻辑处理
        /// </summary>
        /// <param name="weaponData"></param>
        /// <param name="nextId"></param>
        private void WeaponUpgradeWishLogic(BackpackWeaponData weaponData,int nextId)
        {
            //此处需要判断拖动的是背包里的还是随机库里的
            Manager.RemoveWeaponFromBackpack(weaponData);//移除两个旧的
            if (DragWeaponItemVm.WeaponData.LocationType == ELocationType.Backpack)
            {
                Manager.RemoveWeaponFromBackpack(DragWeaponItemVm.WeaponData);
                var oldStartPosX = DragWeaponItemVm.WeaponData.RowIdx;//将拖动升级的武器格子状态刷新为1
                var oldStartPosY = DragWeaponItemVm.WeaponData.ColumnIdx;
                UpdateGridDataValueByWeaponData(DragWeaponItemVm.WeaponData.OccupyList,oldStartPosX, oldStartPosY, 1,-1);
            }
            else
            {
                Manager.RemoveRandomWeaponData(DragWeaponItemVm.WeaponData.WeaponParamId,DragWeaponItemVm.WeaponData.WeaponAttrType);
            }
            
            var newWeaponData = DataManager.CreateBackpackData(nextId*10+1,1,weaponData.LocationType);
            DataManager.TotalMergeNum += 1;
            DataManager.MergeWeaponUid = newWeaponData.Uid;

            if (DataManager.GetEquipCfgData(newWeaponData.EquipId)?.AtkType == (int)EquipAtkType.Magic && GameDataManager.Instance.MagicEquipMergedGainValue > 0)//魔法武器
            {
                DataManager.MagicMergeNum += 1;
                UIEffectManager.Instance.PlayerGameCoinAction(WishVm.WishNodeRect.position,1, true,null,false).Forget();
                GameManager.Instance.CheckMagicEquipMergedGain();
            }
            else if (DataManager.GetEquipCfgData(newWeaponData.EquipId)?.AtkType == (int)EquipAtkType.Physic && GameDataManager.Instance.PhyEquipMergedGainValue > 0)//物理武器
            {
                DataManager.PhysicsMergeNum += 1;
                UIEffectManager.Instance.PlayerGameCoinAction(WishVm.WishNodeRect.position,1, true,null,false).Forget();
                GameManager.Instance.CheckPhyEquipMergedGain();
            }
            WishVm.WeaponData = newWeaponData;
            Manager.AddWeaponToWishPool(newWeaponData.WeaponId,newWeaponData.WeaponAttrType);
        }
        //重置格子状态
        private void ResetBlocks()
        {
            for (var i = GridList.Count - 1; i >= 0; i--)
            {
                var indexPos = GridList[i].IndexPos;
                GridList[i].SetBlockSolidState(DataManager.GridData[indexPos.x][indexPos.y]);
            }
        }
        /// <summary>
        /// 根据Uid获取对应武器的索引值
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        private int GetWeaponItemIndexByUid(long uid)
        {
            var curIndex = -1;
            for (var i = 0; i < WeaponList.Count; i++)
            {
                if (WeaponList[i] == null) continue;
                if (WeaponList[i].WeaponData.Uid != uid) continue;
                curIndex = i;
                break;
            }
            return curIndex;
        }
        /// <summary>
        /// 检测拖动的格子是否有效
        /// </summary>
        /// <param name="gridIdx"></param>
        /// <param name="screenPos"></param>
        /// <returns></returns>
        private bool CheckBatchGridClickVaild(Vector2Int gridIdx,Vector2 screenPos)
        {
            var isVaild = gridIdx is { x: >= 0, y: >= 0 } && gridIdx.x < GameDataManager.Instance.GMaxRow
                                                          && gridIdx.y < GameDataManager.Instance.GMaxColumn;
            if (!isVaild) return false;
            if (DragBatchItemVm != null && DragBatchItemVm.GridList.Count > 0)
            {
                isVaild = DragBatchItemVm.IsClickInArea(screenPos);
            }
            else
            {
                isVaild = GameDataManager.Instance.GridData[gridIdx.x][gridIdx.y] > 0;
            }
            return isVaild;
        }
         private readonly string actionItemPrefabPath = "UI/Common/Items/ActionItem";
        /// <summary>
        /// 播放武器移除动画
        /// </summary>
        /// <param name="worldPos"></param>
        /// <param name="path"></param>
        /// <param name="endItemVm"></param>
        private async UniTask PlayWeaponToRandom(Vector3 worldPos,string path,RandomItemViewModel endItemVm)
        {
            var playTime = 0.3f;
            var localPos = FlyEndRect.InverseTransformPoint(worldPos);
            var actionItem = await GetActionItem(FlyEndRect,path);
            if(actionItem == null) return;
            var endLocalPos = Vector3.zero;
            if (endItemVm != null && endItemVm.RandomNodeRect!= null)
            {
                endLocalPos = FlyEndRect.InverseTransformPoint(endItemVm.RandomNodeRect.position);
            }
            actionItem.transform.localPosition = localPos;
            actionItem.gameObject.SetActive(true);
            var moveTween = actionItem.transform.DOLocalMove(endLocalPos, playTime).SetEase(Ease.Linear).OnComplete(() =>
            {
                if (endItemVm != null)
                {
                    endItemVm.Alpha = 1;
                    endItemVm.SetRandomIconHide(true);
                }
                AssetsManager.ReleaseInstance(actionItem);
            });
        }
        private async UniTask<GameObject> GetActionItem(Transform parent, string iconPath)
        {
            var actionItem = await AssetsManager.InstantiateWithParentAsync(actionItemPrefabPath, parent, false);
            if (actionItem == null) return null;
            if (parent == null)
            {
                AssetsManager.ReleaseInstance(actionItem);
                return null;
            }
            actionItem.gameObject.SetActive(false);
            var itemImg = actionItem.transform.Find("Icon").gameObject.GetComponent<Image>();
            itemImg.sprite = AssetsManager.LoadSpriteSync(iconPath);
            itemImg.SetNativeSize();
            return actionItem;
        }
        /// <summary>
        /// 武器列表刷新
        /// </summary>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        private void UpdateWeaponList(int offsetX, int offsetY)
        {
            foreach (var dragWeaponVm in DragBatchItemVm.WeaponList)
            {
                var weaponVm = new WeaponItemViewModel(dragWeaponVm.WeaponData);
                weaponVm.PosIndex = new Vector2Int
                { 
                    x = dragWeaponVm.PosIndex.x + offsetX, 
                    y = dragWeaponVm.PosIndex.y + offsetY 
                };
                weaponVm.WeaponData.RowIdx = weaponVm.PosIndex.x;
                weaponVm.WeaponData.ColumnIdx = weaponVm.PosIndex.y;
                weaponVm.UpdatePosition();
                WeaponList.Add(weaponVm);
                Manager.UpdateWeaponDataInBackpack(weaponVm.WeaponData);
            }
        }
    }
}