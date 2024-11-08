using DH.UIFramework;
using UnityEngine;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.Data;
namespace DH.Game.ViewModels
{
    public partial class GridItemViewModel: ViewModelBase
    {
        [AutoNotify] private RectTransform gridRect;
        [AutoNotify] private EBlockOccupyState occupyState;//格子状态
        [AutoNotify] private Vector3 localPos;//地块的位置
        [AutoNotify] private int gridStateType;//格子状态类型
        [AutoNotify] private Vector2Int indexPos;//格子位置信息
        [Preserve]
        public GridItemViewModel(int row,int column)
        {
            IndexPos = new Vector2Int{ x = row,y = column};
        }
        public bool IsClickInArea(Vector2 screenPos)
        {
            if (GridRect == null) return false;
            if (!RectTransformUtility.RectangleContainsScreenPoint(GridRect, screenPos,
                    AppGlobal.Instance.UICamera)) return false;
            return true;
        }
        /// <summary>
        /// 设置格子的状态
        /// </summary>
        /// <param name="state"></param>
        public void SetBlockSolidState(int state)
        {
            if (state == 0)
            {
                OccupyState = EBlockOccupyState.Virtual;
                GridStateType = 3;
            }
            else
            {
                OccupyState = EBlockOccupyState.Solid;
                GridStateType = 0;
            }
        }
        /// <summary>
        /// 设置虚线格子的可用状态
        /// </summary>
        /// <param name="isEqual"></param>
        /// <param name="isAllMatch"></param>
        public void SetGridDashValidState(bool isEqual,bool isAllMatch)
        {
            if (OccupyState != EBlockOccupyState.Virtual) return;
            if (!isEqual)
            {
                GridStateType = 3;
            }
            else
            {
                if (isAllMatch) 
                {
                    GridStateType = 4;
                }
                else
                {
                    GridStateType = 5;
                }
            }
        }
        /// <summary>
        /// 设置固体格子的可用状态
        /// </summary>
        /// <param name="isEqual"></param>
        /// <param name="isAllMatch"></param>
        public void SetGridSolidValidState(bool isEqual,bool isAllMatch)
        {
            if (OccupyState != EBlockOccupyState.Solid) return;
            if (!isEqual)
            {
                GridStateType = 0;
            }
            else
            {
                if (isAllMatch) 
                {
                    GridStateType = 2;
                }
                else 
                {
                    GridStateType = 1;
                }
            }
        }
    }
}