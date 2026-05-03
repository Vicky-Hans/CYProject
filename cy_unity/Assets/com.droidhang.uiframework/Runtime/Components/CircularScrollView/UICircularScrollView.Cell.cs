using DHFramework;
using UnityEngine;

namespace DH.UIFramework
{
    public partial class UICircularScrollView
    {
        //记录 物体的坐标 和 物体 
        public class CellInfo : IReference
        {
            public bool InitSize = false;

            public int PageIdx
            {
                get => pageIdx;
                set => pageIdx = value;
            }

            public Vector2 Pos => pos; //初始位置
            public float ItemSize => itemSize;
            public GameObject Go => obj;
            
            private Vector2 pos; //初始位置，
            private GameObject obj = null;
            private RectTransform objRt = null;
            private float itemSize = 0; //自适应时的大小
            private int pageIdx = 0;

            public void SetGameObject(GameObject obj)
            {
                this.obj = obj;
                objRt = obj ? obj.transform as RectTransform : null;
            }

            public void SetItemSize(float size)
            {
                itemSize = size;
            }

            /// <summary>
            /// 只设置cell的初始位置
            /// </summary>
            /// <param name="newPos"></param>
            public void SetOriPos(Vector2 newPos)
            {
                pos = newPos;
            }

            /// <summary>
            /// 对于无限滚动的循环列表显示的位置和初始位置会有不同
            /// </summary>
            /// <param name="newPos"></param>
            public void SetAnchorPos(Vector3 newPos)
            {
                if (objRt)
                {
                    objRt.anchoredPosition = newPos;
                }
            }

            public void ApplyObjOffsetPos(Vector3 offset)
            {
                if (obj != null)
                {
                    var curPos = obj.transform.localPosition;
                    obj.transform.localPosition = curPos + offset;
                }
            }
            
            public void Clear()
            {
                obj = null;
                objRt = null;
                pageIdx = 0;
            }
        }
        
        /// <summary>
        /// 获取第i个位置的 cellinfo
        /// </summary>
        /// <param name="i"></param>
        /// <param name="cellInfo"></param>
        /// <param name="bInsert"></param>
        private void InitCell(int i, CellInfo cellInfo, bool bInsert = false)
        {
            if (cellInfo == null)
            {
                cellInfo = ReferencePool.Acquire<CellInfo>();
                
                if (bInsert)
                {
                    m_CellInfos.Insert(i, cellInfo);
                }
                else
                {
                    m_CellInfos.Add(cellInfo);
                }
            }

            cellInfo.SetItemSize(m_Direction == e_Direction.Vertical
                ? m_CellObjectHeight
                : m_CellObjectWidth);
            
            cellInfo.InitSize = false;
        }

        protected void RecycleCellInfo(CellInfo cellInfo)
        {
            if (cellInfo != null && cellInfo.Go)
            {
                SetPoolsObj(cellInfo.Go);
                cellInfo.Clear();
            }
        }

        private void ResetCellSize(float width, float height)
        {
            if (!m_isInited)
            {
                Init();
            }
            
            m_CellObjectWidth = width;
            m_CellObjectHeight = height;

            InitAllCellPos();
        }
    }
}
