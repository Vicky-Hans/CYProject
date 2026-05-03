using System;

namespace DH.UIFramework
{
    using UnityEngine;
    using UnityEngine.UI;
    using  Manager = ClipperMaterialManager;

    /// <summary>
    /// Mesh和ParticleSystem裁剪脚本:自动设置该对象及其子节点Mesh和ParticleSystem的裁剪区域
    /// </summary>
    public sealed class Clipper3D : MonoBehaviour, IClippable
    {
        private RectMask2D parentMask = null;
        private Renderer[] renderers;
        private static readonly int ClipRect = Shader.PropertyToID("_ClipRect");
        
        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
            UpdateClipParent();
        }

        private void OnDestroy()
        {
            if (!parentMask)
            {
                return;
            }
            
            ReleaseMaterial(parentMask);
        }

        private void ReleaseMaterial(IClipper clipper)
        {
            foreach (var item in renderers)
            {
                Material[] materials = null;
                for(int index = 0;index < item.sharedMaterials.Length;index++)
                {
                    var material = item.sharedMaterials[index];
                    var oldMat = Manager.Instance.ReleaseMaterial(material);
                    if (!oldMat)
                    {
                        break;
                    }

                    if (materials == null)
                    {
                        materials = new Material[item.sharedMaterials.Length];
                    }

                    materials[index] = oldMat;
                }

                item.sharedMaterials = materials;
            }
        }

        private void ReplaceMaterial(IClipper clipper)
        {
            foreach (var item in renderers)
            {
                Material[] materials =  new Material[item.sharedMaterials.Length];
                for (int index = 0; index < item.sharedMaterials.Length; index++)
                {
                    var material = item.sharedMaterials[index];
                    var newMat = Manager.Instance.GetMaterial(material, clipper);
                    materials[index] = newMat;
                }

                item.sharedMaterials = materials;
            }
        }
        
        private void UpdateClipParent()
        {
            var newParent = isActiveAndEnabled ? MaskUtilities.GetRectMaskForClippable(this) : null;
            // if the new parent is different OR is now inactive
            if (parentMask != null && (newParent != parentMask || !newParent.IsActive()))
            {
                parentMask.RemoveClippable(this);
                ReleaseMaterial(parentMask);
            }

            // don't re-add it if the newparent is inactive
            if (newParent != null && newParent.IsActive())
                newParent.AddClippable(this);
            parentMask = newParent;
            ReplaceMaterial(parentMask);
        }

        public void RecalculateClipping()
        {
            UpdateClipParent();
        }

        public void Cull(Rect clipRect, bool validRect)
        {
        }

        public void SetClipRect(Rect value, bool validRect)
        {
            Vector3[] wc = new Vector3[4];
            rectTransform.GetWorldCorners(wc); // 计算world space中的点坐标
            var clipRect = new Vector4(wc[0].x, wc[0].y, wc[2].x, wc[2].y); // 选取左下角和右上角
            foreach (var item in renderers)
            {
                foreach (var mat in item.sharedMaterials)
                {
                    mat.EnableKeyword("UNITY_UI_CLIP_RECT");
                    mat.SetVector(ClipRect, clipRect); // 设置裁剪区域
                }
            }
        }

        public RectTransform rectTransform
        {
            get
            {
                if (parentMask)
                {
                    return parentMask.GetComponent<RectTransform>();
                }

                return null;
            }
        }

        public void SetClipSoftness(Vector2 clipSoftness)
        {
        }
    }
}