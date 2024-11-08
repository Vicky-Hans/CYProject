using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
public class UIParticleSystem : UIBehaviour, IClippable
{
    [Serializable]
    public sealed class UIParticleItem
    {
        public Renderer Renderer;
        public Material Material;
    }
    
    private static readonly string ClipKeyword = "UNITY_UI_CLIP_RECT";
    private static readonly int UIMaskClipRectId = Shader.PropertyToID("_ClipRect");
    private static readonly int UIMaskSoftnessXId = Shader.PropertyToID("_UIMaskSoftnessX");
    private static readonly int UIMaskSoftnessYId = Shader.PropertyToID("_UIMaskSoftnessY");
    
    public List<UIParticleItem> TargetRenders;

    public RectTransform rectTransform { get; }

    private RectMask2D parentMask;
    private Vector3[] maskCorners = new Vector3[4];
    
    private MaterialPropertyBlock propertyBlock;
    private MaterialPropertyBlock PropertyBlock
    {
        get
        {
            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }
            return propertyBlock;
        }
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
        
        UpdateClipParent();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
    }

    private void UpdateClipParent()
    {
        var newParent = (IsActive()) ? MaskUtilities.GetRectMaskForClippable(this) : null;

        // if the new parent is different OR is now inactive
        if (parentMask != null && (newParent != parentMask || !newParent.IsActive()))
        {
            parentMask.RemoveClippable(this);
        }

        // don't re-add it if the newparent is inactive
        if (newParent != null && newParent.IsActive())
            newParent.AddClippable(this);

        parentMask = newParent;
    }

    #region IClippable
    
    public void RecalculateClipping()
    {
        UpdateClipParent();
    }

    public void Cull(Rect clipRect, bool validRect)
    {
        
    }

    public void SetClipRect(Rect value, bool validRect)
    {
        if (TargetRenders == null || TargetRenders.Count < 1)
        {
            return;
        }
        if (parentMask != null && parentMask.rectTransform != null)
            parentMask.rectTransform.GetWorldCorners(maskCorners);

        for (int i = 0; i < TargetRenders.Count; i++)
        {
            var renderItem = TargetRenders[i];
            if (validRect)
            {
                if (renderItem.Material != null)
                {
                    renderItem.Material.EnableKeyword(ClipKeyword);
                }
            }
            else
            {
                if (renderItem.Material != null)
                {
                    renderItem.Material.DisableKeyword(ClipKeyword);
                }
            }

            if (renderItem.Renderer != null)
            {
                renderItem.Renderer.material = renderItem.Material;

                PropertyBlock.Clear();
                renderItem.Renderer.GetPropertyBlock(PropertyBlock);
                PropertyBlock.SetVector(UIMaskClipRectId,
                    new Vector4(maskCorners[0].x, maskCorners[0].y, maskCorners[2].x, maskCorners[2].y));
                renderItem.Renderer.SetPropertyBlock(PropertyBlock);
            }
        }
    }

    public void SetClipSoftness(Vector2 clipSoftness)
    {
        for (int i = 0; i < TargetRenders.Count; i++)
        {
            var renderItem = TargetRenders[i];
            if (renderItem.Renderer == null)
            {
                continue;
            }
            
            PropertyBlock.Clear();
            renderItem.Renderer.GetPropertyBlock(PropertyBlock);
            PropertyBlock.SetFloat(UIMaskSoftnessXId, clipSoftness.x);
            PropertyBlock.SetFloat(UIMaskSoftnessYId, clipSoftness.y);
            renderItem.Renderer.SetPropertyBlock(PropertyBlock);
        }
    }
    
    #endregion

#if UNITY_EDITOR

    [ContextMenu("Find Child Particle System")]
    private void FindChildParticleSystem()
    {

        TargetRenders = new List<UIParticleItem>();

        var psArray = gameObject.GetComponentsInChildren<ParticleSystem>();
        foreach (var item in psArray)
        {
            var itemRender = item.GetComponent<Renderer>();
            if (!itemRender.enabled)
            {
                continue;
            }
            
            TargetRenders.Add(new UIParticleItem()
            {
                Renderer = itemRender,
            });

        }

    }

#endif


}
