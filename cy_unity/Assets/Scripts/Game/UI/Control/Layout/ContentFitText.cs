using TMPro;

namespace Game.UI.Control
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    [RequireComponent(typeof (RectTransform))]
    [ExecuteInEditMode]
    public class ContentFitText : UIBehaviour, ILayoutElement
    {
        public TextMeshProUGUI sourceText;
        public Vector2 padding;

        public float minWidth
        {
            get
            {
                if (sourceText == null)
                {
                    return -1f;
                }
                return LayoutUtility.GetMinWidth(sourceText.rectTransform) + padding.x;
            }
        }

        public float preferredWidth
        {
            get
            {
                if (sourceText == null)
                {
                    return -1f;
                }
                return LayoutUtility.GetPreferredWidth(sourceText.rectTransform) + padding.x;
            }
        }

        public float flexibleWidth
        {
            get
            {
                if (sourceText == null)
                {
                    return -1f;
                }
                return LayoutUtility.GetFlexibleWidth(sourceText.rectTransform);
            }
        }

        public float minHeight
        {
            get
            {
                if (sourceText == null)
                {
                    return -1f;
                }
                return LayoutUtility.GetFlexibleHeight(sourceText.rectTransform) + padding.y;
            }
        }

        public float preferredHeight
        {
            get
            {
                if (sourceText == null)
                {
                    return -1f;
                }
                return LayoutUtility.GetPreferredHeight(sourceText.rectTransform) + padding.y;
            }
        }

        public float flexibleHeight
        {
            get
            {
                if (sourceText == null)
                {
                    return -1f;
                }
                return LayoutUtility.GetFlexibleHeight(sourceText.rectTransform);
            }
        }

        public int layoutPriority
        {
            get { return 0; }
        }

        public void CalculateLayoutInputHorizontal()
        {
            sourceText.CalculateLayoutInputHorizontal();
        }

        public void CalculateLayoutInputVertical()
        {
            sourceText.CalculateLayoutInputVertical();
        }

        protected override void OnEnable()
        {
            SetDirty();

            if (!sourceText)
            {
                return;
            }
            sourceText.RegisterDirtyLayoutCallback(CopySourceOnLayoutDirty);
        }

        private void CopySourceOnLayoutDirty()
        {
            SetDirty();
        }

        protected override void OnTransformParentChanged()
        {
            SetDirty();
        }

        protected override void OnDisable()
        {
            if (sourceText)
            {
                sourceText.UnregisterDirtyLayoutCallback( CopySourceOnLayoutDirty);
            }

            SetDirty();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            SetDirty();
        }

        protected override void OnBeforeTransformParentChanged()
        {
            SetDirty();
        }

        protected void SetDirty()
        {
            if (!IsActive())
            {
                return;
            }

            LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        }
    }
}