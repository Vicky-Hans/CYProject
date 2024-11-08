using DHFramework;

namespace DH.UIFramework
{
    using System;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class Clipper2D : MonoBehaviour, IClippable
    {
        public MaskableGraphic[] maskableGraphics;
        private RectMask2D m_ParentMask = null;

        private void Awake()
        {
            maskableGraphics = GetComponentsInChildren<MaskableGraphic>(true);
            UpdateClipParent();
        }

        private void UpdateClipParent()
        {
            m_ParentMask = GetComponentInParent<RectMask2D>(true);
            if (m_ParentMask)
            {
                m_ParentMask.AddClippable(this);
            }
            else
            {
                DHLog.Warning("父节点没有RectMask2D,无需挂载此脚本");
            }
        }

        public void RecalculateClipping()
        {
            UpdateClipParent();
        }

        public void Cull(Rect clipRect, bool validRect)
        {
            foreach (var maskableGraphic in maskableGraphics)
            {
                maskableGraphic.Cull(clipRect, validRect);
            }
        }

        public void SetClipRect(Rect clipRect, bool validRect)
        {
            foreach (var maskableGraphic in maskableGraphics)
            {
                maskableGraphic.SetClipRect(clipRect, validRect);
                maskableGraphic.Cull(clipRect, validRect);
            }
        }

        public void SetClipSoftness(Vector2 clipSoftness)
        {
            foreach (var maskableGraphic in maskableGraphics)
            {
                maskableGraphic.SetClipSoftness(clipSoftness);
            }
        }

        public RectTransform rectTransform
        {
            get { return m_ParentMask ? m_ParentMask.GetComponent<RectTransform>() : null; }
        }

        private void OnDestroy()
        {
            if (m_ParentMask)
            {
                m_ParentMask.RemoveClippable(this);
            }
        }
    }
}