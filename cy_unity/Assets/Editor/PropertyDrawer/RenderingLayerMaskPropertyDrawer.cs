using DH.Base;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[CustomPropertyDrawer(typeof(RenderingLayersMaskPropertyAttribute))]
public class RenderingLayerMaskPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var defaultRenderPipeline = GraphicsSettings.defaultRenderPipeline;
        if (defaultRenderPipeline == null) return;

        if (defaultRenderPipeline.renderingLayerMaskNames == null) return;

        var renderingLayerMaskNames = defaultRenderPipeline.renderingLayerMaskNames;

        var mask = property.uintValue;
        EditorGUI.BeginProperty(position, label, property);
        var fieldRect = EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));
        var newMask = EditorGUI.MaskField(fieldRect, (int)mask, renderingLayerMaskNames);
        if (newMask != mask) property.uintValue = newMask == -1 ? uint.MaxValue : (uint)newMask;

        EditorGUI.EndProperty();
    }
}