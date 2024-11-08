using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DH.Data;
using DH.Game.UIViews;
using Game.UI.Guide;
using UnityEngine.EventSystems;

namespace DH.Game.ViewModels
{
    public partial class MainMergeViewModel
    {
        /// <summary>
        /// 物品拖拽开始
        /// </summary>
        /// <param name="eventData"></param>
        private void OnBeginDragHandle(PointerEventData eventData)
        {
            if (GridListRect == null || isCanDrag) return;
            DragStartData = eventData;
            eventPointerId = eventData.pointerId;
            dragStartLocalPos = eventData.position;
            if (Manager.BlockState == EBlockState.AddCell)
            {
                BeginDragOnAddCellModel();
            } 
            else if (Manager.BlockState == EBlockState.Normal)
            {
                var clickIndex = GetClickRandomItemIndex(dragStartLocalPos);//候选列表
                if (clickIndex >= 0 && RandomBagList[clickIndex] != null)//点击到了随机库中的武器/新格子
                {
                    var clickRandomItemVm = RandomBagList[clickIndex];
                    if (clickRandomItemVm.GridAddData != null && clickRandomItemVm.GridAddData.AdType > 0 && !clickRandomItemVm.GridAddData.IsUnlocked)
                    {
                        if (playingAdUid != 0) return;
                        playingAdUid = clickRandomItemVm.GridAddData.Uid;
                        var tmpVm = new EquipAdConfirmViewModel(null,clickRandomItemVm.GridAddData, (isOk) =>
                        {
                            if (isOk)
                            {
                                clickRandomItemVm.GridAddData.IsUnlocked = true;
                                clickRandomItemVm.UpdateAdState();
                                playingAdUid = 0;
                            }
                            else
                            {
                                playingAdUid = 0;
                            }
                        });
                        UIManager.Instance.OpenDialog<EquipAdConfirmView>(tmpVm).Forget();
                        return;
                    }
                    if (clickRandomItemVm.WeaponData != null)
                    {
                        Manager.DragState = EDragState.Weapon;
                        UpdateDragWeaponItemVm(clickRandomItemVm.WeaponData);
                        Manager.DragWeaponId = DragWeaponItemVm.WeaponData.WeaponId;
                        Manager.DragUid = DragWeaponItemVm.WeaponData.Uid;
                        if (clickRandomItemVm.RandomNodeRect!=null && DragWeaponItemVm.WeaponNodeRect!= null)
                        {
                            var tmpWorldPos = clickRandomItemVm.RandomNodeRect.position;
                            var tmpLocalPos = DragWeaponItemVm.WeaponNodeRect.parent.InverseTransformPoint(tmpWorldPos);
                            tmpLocalPos.z = 0;
                            DragWeaponItemVm.WeaponNodeRect.localPosition = tmpLocalPos;
                        }
                    }
                    else if (clickRandomItemVm.GridAddData != null)
                    {
                        Manager.DragState = EDragState.BlockAdd;
                        Manager.BlockState = EBlockState.AddCell;
                        UpdateDragGridAddItemVm(clickRandomItemVm.GridAddData);
                        Manager.DragWeaponId = clickRandomItemVm.GridAddData.GridId;
                        Manager.DragUid = clickRandomItemVm.GridAddData.Uid;
                        if (clickRandomItemVm.RandomNodeRect != null && DragGridAddItemVm.GridNodeRect != null)
                        {
                            var tmpWorldPos = clickRandomItemVm.RandomNodeRect.position;
                            var tmpLocalPos = DragGridAddItemVm.GridNodeRect.parent.InverseTransformPoint(tmpWorldPos);
                            tmpLocalPos.z = 0;
                            DragGridAddItemVm.GridNodeRect.localPosition = tmpLocalPos;
                        }
                        DataManager.GridBorderRect = Manager.GetMaxGridBorder();
                        RefreshGridList();
                    }
                    isCanDrag = true;
                    AudioManager.Instance.PlayAudio("SFX_UI/ui_game_drag");
                    ParseDragItem(dragStartLocalPos);
                    RandomBagList.RemoveAt(clickIndex);
                    NotifyGuideStopAction();
                }
                if (Manager.DragState == EDragState.BlockAdd) MakeWeaponTransparent(0.55f);
                if (Manager.DragState != EDragState.None) return;
                clickIndex = GetClickWeaponItemIndex(dragStartLocalPos);
                if (clickIndex >= 0 && WeaponList[clickIndex] != null) //点击到了背包中的武器/新格子
                {
                    var clickWeaponItemVm = WeaponList[clickIndex];
                    Manager.DragState = EDragState.Weapon;
                    UpdateDragWeaponItemVm(clickWeaponItemVm.WeaponData);
                    DragWeaponItemVm.PosIndex = clickWeaponItemVm.PosIndex;
                    Manager.DragWeaponId = DragWeaponItemVm.WeaponData.WeaponId;
                    Manager.DragUid = DragWeaponItemVm.WeaponData.Uid;
                    if (clickWeaponItemVm.WeaponNodeRect!=null && DragWeaponItemVm.WeaponNodeRect!= null)
                    {
                        var tmpWorldPos = clickWeaponItemVm.WeaponNodeRect.position;
                        var tmpLocalPos = DragWeaponItemVm.WeaponNodeRect.InverseTransformPoint(tmpWorldPos);
                        tmpLocalPos.z = 0;
                        DragWeaponItemVm.WeaponNodeRect.localPosition = tmpLocalPos;
                    }
                    isCanDrag = true;
                    AudioManager.Instance.PlayAudio("SFX_UI/ui_game_drag");
                    startIdxPos = clickWeaponItemVm.PosIndex;
                    ParseDragItem(dragStartLocalPos);
                    WeaponList.RemoveAt(clickIndex);
                    NotifyGuideStopAction();
                } 
                else if (!WishVm.IsShowLockNode && WishVm.WeaponData != null && WishVm.IsClickInArea(dragStartLocalPos))
                {
                    Manager.DragState = EDragState.Weapon;
                    UpdateDragWeaponItemVm(WishVm.WeaponData);
                    Manager.DragWeaponId = DragWeaponItemVm.WeaponData.WeaponId;
                    Manager.DragUid = DragWeaponItemVm.WeaponData.Uid;
                    if (WishVm.WishNodeRect!=null && DragWeaponItemVm.WeaponNodeRect!= null)
                    {
                        var tmpWorldPos = WishVm.WishNodeRect.position;
                        var tmpLocalPos = DragWeaponItemVm.WeaponNodeRect.InverseTransformPoint(tmpWorldPos);
                        tmpLocalPos.z = 0;
                        DragWeaponItemVm.WeaponNodeRect.localPosition = tmpLocalPos;
                    }
                    isCanDrag = true;
                    AudioManager.Instance.PlayAudio("SFX_UI/ui_game_drag");
                    ParseDragItem(dragStartLocalPos);
                    Manager.RemoveWeaponFromWishPool(WishVm.WeaponData.WeaponId,WishVm.WeaponData.WeaponAttrType);
                    WishVm.WeaponData = null;
                    NotifyGuideStopAction();
                }
            }
        }
        /// <summary>
        /// 格子扩建模式下的开始拖动,只能点击随机库中的格子，不能点击武器
        /// </summary>
        private void BeginDragOnAddCellModel()
        {
            if (DragGridAddItemVm == null || DragGridAddItemVm.Alpha == 0)
            {
                var clickIndex = GetClickRandomItemIndex(dragStartLocalPos);//候选列表
                if (clickIndex >= 0 && RandomBagList[clickIndex] != null && RandomBagList[clickIndex].GridAddData != null)//点击到了随机库中的新格子
                {
                    var clickRandomItemVm = RandomBagList[clickIndex];
                    if (clickRandomItemVm.GridAddData != null && clickRandomItemVm.GridAddData.AdType > 0 && !clickRandomItemVm.GridAddData.IsUnlocked)
                    {
                        if (playingAdUid != 0) return;
                        playingAdUid = clickRandomItemVm.GridAddData.Uid;
                        var tmpVm = new EquipAdConfirmViewModel(null,clickRandomItemVm.GridAddData, (isOk) =>
                        {
                            if (isOk)
                            {
                                clickRandomItemVm.GridAddData.IsUnlocked = true;
                                clickRandomItemVm.UpdateAdState();
                                playingAdUid = 0;
                            }
                            else
                            {
                                playingAdUid = 0;
                            }
                        });
                        UIManager.Instance.OpenDialog<EquipAdConfirmView>(tmpVm).Forget();
                        return;
                    }
                    if (clickRandomItemVm is { GridAddData: not null })
                    {
                        Manager.DragState = EDragState.BlockAdd;
                        Manager.BlockState = EBlockState.AddCell;
                        Manager.DragWeaponId = clickRandomItemVm.GridAddData.GridId;
                        Manager.DragUid = clickRandomItemVm.GridAddData.Uid;
                        UpdateDragGridAddItemVm(clickRandomItemVm.GridAddData);
                        if (DragGridAddItemVm != null && clickRandomItemVm.RandomNodeRect != null && DragGridAddItemVm.GridNodeRect != null)
                        {
                            var tmpWorldPos = clickRandomItemVm.RandomNodeRect.position;
                            var tmpLocalPos = DragGridAddItemVm.GridNodeRect.parent.InverseTransformPoint(tmpWorldPos);
                            tmpLocalPos.z = 0;
                            DragGridAddItemVm.GridNodeRect.localPosition = tmpLocalPos;
                        }
                        DataManager.GridBorderRect = Manager.GetMaxGridBorder();
                        RefreshGridList();
                        isCanDrag = true;
                        ParseDragItem(dragStartLocalPos);
                        RandomBagList.RemoveAt(clickIndex);
                        NotifyGuideStopAction();
                        return;
                    }
                }
            }
            //棋盘的武器或者空格子点击响应逻辑 点击到哪个格子
            var clickIdxInfo = GetCurClickGridIdx(dragStartLocalPos);
            var isNewAdd = false;//移动场上固定格子
            //是否为新增的格子
            for (var i = 0; i < blockAddOccupyCells?.Count; i++)
            {
                var aOccupyCell = blockAddOccupyCells[i];
                if (aOccupyCell.x != clickIdxInfo.x || aOccupyCell.y != clickIdxInfo.y) continue;
                isNewAdd = true;
                break;
            }
            if (isNewAdd)
            {
                isCanDrag = true;
                AudioManager.Instance.PlayAudio("SFX_UI/ui_game_drag");
                Manager.DragState = EDragState.BlockAdd;
                if (DragWeaponItemVm?.WeaponData == null) return;
                Manager.DragWeaponId = DragWeaponItemVm.WeaponData.WeaponId;
                Manager.DragUid = DragWeaponItemVm.WeaponData.Uid;
            }
            else if(CheckBatchGridClickVaild(clickIdxInfo,dragStartLocalPos) && GuideManager.Instance.CurGuideId != 112)//点在格子内且点在已解锁格子内
            {
                isCanDrag = true;
                AudioManager.Instance.PlayAudio("SFX_UI/ui_game_drag");
                Manager.DragState = EDragState.DragBatch;
                var tmpWeaponList = new List<WeaponItemViewModel>();
                foreach (var weaponItem in WeaponList)
                {
                    tmpWeaponList.Add(weaponItem);
                }
                WeaponList.Clear();
                //整体拖动
                var tmpGridList = new List<GridItemViewModel>();
                DataManager.GetAreaBorder();
                for (var i = 0; i < DataManager.GMaxRow; i++) 
                {
                    for (var j = 0; j < DataManager.GMaxColumn; j++)
                    {
                        var idx = i * DataManager.GMaxColumn + j;
                        if (DataManager.GridData[i][j] <= 0 || idx >= GridList.Count) continue;
                        var blockUnit = new GridItemViewModel(i, j);
                        blockUnit.LocalPos = Manager.CalculationGridLocalPos(i, j);
                        blockUnit.SetBlockSolidState(1);
                        tmpGridList.Add(blockUnit);
                        GridList[idx].SetBlockSolidState(0);
                    }
                }
                if (DragBatchItemVm != null)
                {
                    DragBatchItemVm.UpdateGridList(false,tmpGridList,tmpWeaponList);
                }
                else
                {
                    DragBatchItemVm = new DragBatchItemViewModel(false,tmpGridList,tmpWeaponList);
                }
            }
        }
        /// <summary>
        /// 获取当前点击到随机库物品的索引，没有返回-1
        /// </summary>
        /// <param name="screenPos"></param>
        /// <returns></returns>
        private int GetClickRandomItemIndex(Vector2 screenPos)
        {
            var resultIdx = -1;
            for (var i = RandomBagList.Count-1; i >=0; i--)
            {
                if (RandomBagList[i] == null) continue;
                var isClick = RandomBagList[i].IsClickInArea(screenPos);
                if (!isClick) continue;
                resultIdx = i;
                break;
            }
            return resultIdx;
        }
    }
}