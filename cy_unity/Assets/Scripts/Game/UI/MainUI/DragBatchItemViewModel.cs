using System.Collections.Generic;
using DH.Data;
using DH.Game.UI;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class DragBatchItemViewModel : ViewModelBase
    {
        [AutoNotify] private float alpha;//透明度
        [AutoNotify] private bool isNewAdd;//是否为新增地块
        [AutoNotify] private Vector2Int batchSize;
        [AutoNotify] private RectTransform batchRect;
        [AutoNotify] private RectTransform gridListRect;
        [AutoNotify] private RectTransform weaponListRect;
        [AutoNotify] private ObservableList<GridItemViewModel> gridList = new();
        [AutoNotify] private ObservableList<WeaponItemViewModel> weaponList = new();
        [AutoNotify] private List<Vector2Int> occupyList;
        [Preserve]
        public DragBatchItemViewModel(bool isNewAdd,List<GridItemViewModel> gridList, List<WeaponItemViewModel> weaponList)
        {
            UpdateGridList(isNewAdd,gridList,weaponList);
        }
        public void UpdateGridList(bool isNewAdd,List<GridItemViewModel> gridList, List<WeaponItemViewModel> weaponList)
        {
            if (gridList != null && gridList.Count > 0)
            {
                for (var i = 0; i < gridList.Count; i++)
                {
                    GridList.Add(gridList[i]);
                }
            }
            if (weaponList != null && weaponList.Count > 0)
            {
                for (var i = 0; i < weaponList.Count; i++)
                {
                    WeaponList.Add(weaponList[i]);
                }
            }
            IsNewAdd = isNewAdd;
            GameDataManager.Instance.GetAreaBorder();
            var minRow = GameDataManager.Instance.GridBorderRect.xMin;
            var maxRow =GameDataManager.Instance.GridBorderRect.xMax;
            var minColumn = GameDataManager.Instance.GridBorderRect.yMin;
            var maxColumn = GameDataManager.Instance.GridBorderRect.yMax;
            var rowSize = (int)(maxRow - minRow + 1);
            var columnSize = (int)(maxColumn - minColumn + 1);
            OccupyList = new List<Vector2Int>(35);
            if (rowSize > 0 && columnSize > 0)
            {
                BatchSize = new Vector2Int(rowSize,columnSize);
                for (var i = 0; i < rowSize; i++)
                {
                    var curRow = (int)minRow + i-1;
                    for (var j = 0; j < columnSize; j++)
                    {
                        var curColumn = (int)minColumn + j-1;
                        if (GameDataManager.Instance.GridData[curRow][curColumn] > 0)
                        {
                            var newPos = new Vector2Int{x = i,y = j};
                            OccupyList.Add(newPos);
                        }
                    }
                }
            }
            Alpha = 1;
        }
        /// <summary>
        /// 这里检测是否点击到批量拖拽
        /// </summary>
        /// <param name="screenPos"></param>
        /// <returns></returns>
        public bool IsClickInArea(Vector2 screenPos)
        {
            if (GridListRect == null) return false;
            if (!RectTransformUtility.RectangleContainsScreenPoint(GridListRect, 
                    screenPos, AppGlobal.Instance.UICamera)) return false;
            var localPoint = Vector2.zero;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(GridListRect,
                    screenPos, AppGlobal.Instance.UICamera, out localPoint)) return false;
            var cellY = Mathf.FloorToInt((localPoint.x + GridListRect.rect.width * GridListRect.anchorMax.x) / GameManager.Instance.CellSize);
            var cellX = Mathf.FloorToInt((GridListRect.rect.height - localPoint.y + GridListRect.rect.yMin) / GameManager.Instance.CellSize);
            var isExist = false;
            for (var i = 0; i < OccupyList.Count; i++)
            {
                var occuY = OccupyList[i].x;
                var occuX = OccupyList[i].y;
                if (cellX == occuX && cellY == occuY)
                {
                    isExist = true;
                    break;
                }
            }
            return isExist;
        }
    }
}