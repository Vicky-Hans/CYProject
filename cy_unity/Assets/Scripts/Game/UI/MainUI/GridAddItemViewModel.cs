using DH.Game.UI;
using DH.Data;
using DH.UIFramework;
using UnityEngine;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class GridAddItemViewModel : ViewModelBase
    {
        [AutoNotify] private int adState;//广告状态 0~不为广告 1~是广告但未解锁 2~是广告已解锁
        [AutoNotify] private float alpha;//透明度
        [AutoNotify] private Vector2 gridSize;
        [AutoNotify] private Vector2 iconSize;
        [AutoNotify] private Vector2 adIconSize;
        [AutoNotify] private string iconPathStr;
        [AutoNotify] private string adIconPathStr;//广告资源路径
        [AutoNotify] private Vector2Int posIndex;//格子位置信息
        [AutoNotify] private RectTransform gridNodeRect;
        private GridAddData gridData;
        public GridAddData GridAddData => gridData;
        public GameManager Manager => GameManager.Instance;
        [Preserve]
        public GridAddItemViewModel(GridAddData data)
        {
            UpdateGridAddData(data);
        }
        /// <summary>
        /// 刷新格子数据
        /// </summary>
        /// <param name="data"></param>
        public void UpdateGridAddData(GridAddData data)
        {
            Alpha = 1;
            gridData = data;
            GridSize = Vector2.zero;
            IconSize = Vector2.zero;
            GridSize = new Vector2(GridAddData.Width*Manager.CellSize,GridAddData.Height*Manager.CellSize);
            IconSize = new Vector2(GridAddData.Width*Manager.CellSize-10,GridAddData.Height*Manager.CellSize-10);
            IconPathStr = "";
            IconPathStr = GridAddData.IconPath;
        }
    }
}