using System.Collections.Generic;
using UnityEngine;

namespace DH.UIFramework
{
    public partial class UICircularScrollView
    {
        public TextAnchor childAlignment = TextAnchor.UpperLeft;

        public void CalAllCellOriPos()
        {
            for (int i = 0; i < m_CellInfos.Count; ++i)
            {
                var cellInfo = m_CellInfos[i];
                if (cellInfo == null)
                {
                    continue;
                }
                
                var newPos = GetCellOriPos(i, m_CellInfos);
                cellInfo.SetOriPos(newPos);
            }
        }
        
        /// <summary>
        /// 计算第i个元素的初始位置
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Vector2 GetCellOriPos(int i, List<CellInfo> cellInfos = null)
        {
            float pos = 0;  //坐标( isVertical ? 记录Y : 记录X )
            float rowPos = 0; //计算每排里面的cell 坐标
            Vector2 newCellPos = new Vector2(m_PaddingLeft, -m_PaddingTop);
            
            if (m_Row > 1)
            {
                m_CellSizeType = CellItemSizeType.CellOriginSize;
            }

            var maxRowCount = m_CellInfos.Count >= m_Row ? m_Row : m_CellInfos.Count;
            var requiredSpace = m_Direction == e_Direction.Vertical
                ? maxRowCount * m_CellObjectWidth + (maxRowCount - 1) * m_SpacingX
                : maxRowCount * m_CellObjectHeight + (maxRowCount - 1) * m_SpacingY;
            var startOffset = GetStartOffset(requiredSpace);

            if (Application.isPlaying && cellInfos != null && (m_CellSizeType != CellItemSizeType.CellOriginSize || m_Row == 1))
            {
                Vector2 lastCellPos = startOffset;
                float offset = 0;
                float itemSpacing = m_Direction == e_Direction.Vertical ? m_SpacingY : m_SpacingX;
                
                if (i - 1 >= 0 && i - 1 < cellInfos.Count)
                {
                    lastCellPos = cellInfos[i - 1].Pos;
                    offset = cellInfos[i - 1].ItemSize + itemSpacing;
                }
                
                // * -> 计算每个Cell坐标
                newCellPos = m_Direction == e_Direction.Vertical ? 
                    new Vector3(startOffset.x, lastCellPos.y - offset) : 
                    new Vector3(lastCellPos.x + offset, startOffset.y);
            }
            else
            {
                int curRow = i / m_Row;
            
                // * -> 计算每个Cell坐标
                if(m_Direction == e_Direction.Vertical)
                {
                    pos = m_CellObjectHeight * curRow + m_SpacingY * curRow;
                    rowPos = m_CellObjectWidth * (i % m_Row) + m_SpacingX * (i % m_Row);
                    newCellPos = new Vector2(rowPos, -pos);
                }
                else
                {
                    pos = m_CellObjectWidth * curRow + m_SpacingX * curRow;
                    rowPos = m_CellObjectHeight * (i % m_Row) + m_SpacingY * (i % m_Row);
                    newCellPos = new Vector2(pos, -rowPos);
                }

                newCellPos += startOffset;
            }

            return newCellPos;
        }

        protected float GetCellDirectionContentExtra()
        {
            var len = m_Direction == e_Direction.Horizontal ? m_scrollViewRectTrans.rect.width : m_scrollViewRectTrans.rect.height;

            return cellOffset > 0 ? len : 0;
        }

        protected Vector2 GetCellShowOffset()
        {
            if (cellOffset > 0)
            {
                var offsetTarget = m_Direction == e_Direction.Vertical
                    ? new Vector2(0, m_scrollViewRectTrans.rect.height * cellOffset) -
                      new Vector2(0, m_CellObjectHeight * .5f)
                    : new Vector2(m_scrollViewRectTrans.rect.width * .5f, 0) -
                      new Vector2(m_CellObjectWidth * .5f, 0);

                if (m_Direction == e_Direction.Vertical)
                {
                    offsetTarget.y *= -1;
                }

                return offsetTarget;
            }

            return Vector2.zero;
        }

        /// <summary>
        /// 第一次获取成功prefab后重排所有的cell 位置
        /// </summary>
        protected virtual void InitAllCellPos()
        {
            if (!m_isInited)
            {
                return;
            }
            
            //先初始化每个cell的size
            int cellInfoCount = m_CellInfos.Count;
            for (int i = 0; i < cellInfoCount; ++i)
            {
                CellInfo cellInfo = m_CellInfos[i];
                if (cellInfo == null)
                {
                    continue;
                }
                
                cellInfo.SetItemSize(m_Direction == e_Direction.Vertical
                    ? m_CellObjectHeight
                    : m_CellObjectWidth);
            }
            
            CalAllCellOriPos();
            
            for (int i = 0; i < cellInfoCount; ++i)
            {
                CellInfo cellInfo = m_CellInfos[i];
                if (cellInfo == null)
                {
                    continue;
                }
                
                cellInfo.SetAnchorPos(cellInfo.Pos + GetCellShowOffset());
            }

            UpdateContentSize(checkDirectionOffset:true);
            m_Dirty = true;
        }

        /// <summary>
        /// 从index开始重新计算cell的pos 到最后
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pageIdx"></param>
        /// <returns></returns>
        private float ReCalCellPos(int index, int pageIdx = -1)
        {
            CalAllCellOriPos();
            
            int maxPage = 0;
            
            for (int i = 0; i < m_CellInfos.Count; ++i)
            {
                var cellInfo = m_CellInfos[i];
                if (i < index && (pageIdx == -1 || cellInfo.PageIdx <= pageIdx))
                {
                    continue;
                }
                
                Vector2 offset = Vector2.zero;
                
                if(m_Direction == e_Direction.Vertical)
                {
                    offset.y = -(m_ContentHeight + m_SpacingY) * cellInfo.PageIdx;
                }
                else
                {
                    offset.x = (m_ContentWidth + m_SpacingX) * cellInfo.PageIdx;
                }
                
                cellInfo.SetAnchorPos(cellInfo.Pos + offset + GetCellShowOffset());

                if (cellInfo.PageIdx > maxPage)
                {
                    maxPage = cellInfo.PageIdx;
                }
            }
            
            return UpdateContentSize(maxPage, true);
        }
        
        /// <summary>
        /// 根据对齐方式获取偏移量
        /// </summary>
        /// <param name="requiredSpaceWithoutPadding"></param>
        /// <returns></returns>
        protected Vector2 GetStartOffset(float requiredSpaceWithoutPadding)
        {
            var oriOffset = new Vector2(m_PaddingLeft, -m_PaddingTop);
            
            var padding = m_Direction == e_Direction.Horizontal ?  m_PaddingTop + m_PaddingBottom : m_PaddingLeft + m_PaddingRight;
            var requiredSpace = requiredSpaceWithoutPadding + padding;
            
            var axis = m_Direction == e_Direction.Horizontal ? 1 : 0;
            var availableSpace = m_ContentRectTrans.rect.size[axis];
            var surplusSpace = availableSpace - requiredSpace;
            var alignmentOnAxis = GetAlignmentOnAxis();
            
            var offset = padding + surplusSpace * alignmentOnAxis;
            var tmpVec = m_Direction == e_Direction.Horizontal
                ? new Vector2(0, -offset)
                : new Vector2(offset, 0);
            
            return tmpVec + oriOffset;
        }
        
        /// <summary>
        /// 更新滑动方向的对齐方式
        /// </summary>
        protected void UpdateDirectionOffset(float surplusSpace)
        {
            var offset = GetDirectionOffset(surplusSpace);
            
            for (int i = 0; i < m_CellInfos.Count; ++i)
            {
                var cellInfo = m_CellInfos[i];
                if (cellInfo == null)
                {
                    continue;
                }
                
                cellInfo.SetOriPos(cellInfo.Pos + offset);
            }
        }
        
        /// <summary>
        /// 根据对齐方式获取移动方向的偏移量
        /// </summary>
        /// <param name="surplusSpace">没有内容的空白大小</param>
        /// <returns></returns>
        protected Vector2 GetDirectionOffset(float surplusSpace)
        {
            var alignmentOnAxis = GetAlignmentOnAxis(false);
            
            var offset = surplusSpace * alignmentOnAxis;
            var tmpVec = m_Direction == e_Direction.Horizontal
                ? new Vector2(offset, 0)
                : new Vector2(0, -offset);
            
            return tmpVec;
        }
        
        protected float GetAlignmentOnAxis(bool otherDirection = true)
        {
            var horizontal = (int)childAlignment % 3 * 0.5f;
            var vertical = (int)childAlignment / 3 * 0.5f;
            
            if (m_Direction == e_Direction.Vertical)
                return otherDirection ? horizontal : vertical;
            else
                return otherDirection ? vertical : horizontal;
        }
    }
}
