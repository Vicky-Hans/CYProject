using UnityEngine;

namespace DH.UIFramework
{
    public partial class GroupExpandCircularScrollView
    {
        private Vector2 GetGroupOffset(int groupIdx,bool splitLine,ExpandInfo previous)
        {
            var lastIdx = groupIdx - 1;

            if (lastIdx >= 0)
            {
                var lastGroupInfo = m_ExpandInfos[lastIdx];
                if (lastGroupInfo.cellCount == 0 && splitLine)
                {
                    return lastGroupInfo.buttonPos;
                }

                if (splitLine && previous?.cellCount == 0)
                {
                    return lastGroupInfo.buttonPos;
                }

                if (lastGroupInfo.isExpand && lastGroupInfo.cellInfos.Count > 0)
                {
                    Vector2 cellOffset = lastGroupInfo.cellInfos[lastGroupInfo.cellInfos.Count - 1].Pos;
                        
                    if (m_Direction == e_Direction.Vertical)
                    {
                        cellOffset.x = 0;
                        cellOffset.y -= (m_CellObjectHeight + m_SpacingY);
                    }
                    else
                    {
                        cellOffset.x += (m_CellObjectWidth + m_SpacingX);
                        cellOffset.y = 0;
                    }
                
                    return cellOffset;
                }
                
                Vector2 buttonPosOffset = lastGroupInfo.buttonPos;
                        
                if (m_Direction == e_Direction.Vertical)
                {
                    buttonPosOffset.y -= (m_ExpandButtonHeight + m_SpacingY);
                }
                else
                {
                    buttonPosOffset.x += (m_ExpandButtonWidth + m_SpacingX);
                }
                
                return buttonPosOffset;
            }
            
            return m_Direction == e_Direction.Vertical ? new Vector2(0, -m_PaddingTop) : new Vector2(m_PaddingLeft, 0);
        }
    }
}

