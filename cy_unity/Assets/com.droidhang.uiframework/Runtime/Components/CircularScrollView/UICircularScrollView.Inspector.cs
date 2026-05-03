using DHFramework;
using UnityEngine;
using UnityEngine.UI;

namespace DH.UIFramework
{
    public partial class UICircularScrollView
    {
#if UNITY_EDITOR
        private string parentName = "";
        protected void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            var baseView = GetComponentInParent<BaseView>();
            if (baseView)
            {
                parentName = baseView.ToString();
            }
            else
            {
                return;
            }
            
            CheckContentAnchor();
            CheckValidSettings();
        }

        private void CheckValidSettings()
        {
            if (m_CellSizeType != CellItemSizeType.CellOriginSize && m_Row > 1)
            {
                DHLog.Warning($"{parentName} 循环列表在一行显示多个时不支持自适应模式,m_AdaptingSize 设置为false", gameObject);
                m_CellSizeType = CellItemSizeType.CellOriginSize;
            }

            if (m_CanAutoScroll)
            {
                inertia = false;
                DHLog.Warning("循环列表在自动滚动时,inertia 设置为false", gameObject);
            }
        }
        
        private void CheckContentAnchor()
        {
            if (m_ScrollRect == null || m_ContentRectTrans == null)
            {
                m_ScrollRect = this.GetComponent<ScrollRect>();
                m_Content = m_ScrollRect.content.gameObject;
            
                //记录 Content 信息
                m_ContentRectTrans = m_Content.GetComponent<RectTransform>();
            }
            
            if (m_ScrollRect == null || m_ContentRectTrans == null)
            {
                Debug.LogError($"{parentName} ScrollRect 为空，请检查");
                return;
            }

            var pivot = m_Direction == e_Direction.Vertical ? new Vector2(0.5f, 1f) : new Vector2(0f, 0.5f);
            var anchorMin = m_Direction == e_Direction.Vertical ? Vector2.up : Vector2.zero;
            var anchorMax = m_Direction == e_Direction.Vertical ? Vector2.one : Vector2.up;

            if (!Mathf.Approximately(m_ContentRectTrans.pivot.sqrMagnitude, pivot.sqrMagnitude) ||
                !Mathf.Approximately(m_ContentRectTrans.anchorMin.sqrMagnitude, anchorMin.sqrMagnitude) ||
                !Mathf.Approximately(m_ContentRectTrans.anchorMax.sqrMagnitude, anchorMax.sqrMagnitude))
            {
                if(m_Direction == e_Direction.Vertical)
                {
                    Debug.LogError($"{parentName} 中ScrollRect 的ContentRectTrans 设置不正确：\n Vertial模式下使用Top Stretch，Pivot：（0.5，1）；", m_ContentRectTrans.gameObject);
                }
                else
                {
                    Debug.LogError($"{parentName} 中ScrollRect 的ContentRectTrans 设置不正确：\n Horizontal模式下使用Stretch Left ，Pivot：（0, 0.5）", m_ContentRectTrans.gameObject);
                }
            }
        }
#endif
    }
}
