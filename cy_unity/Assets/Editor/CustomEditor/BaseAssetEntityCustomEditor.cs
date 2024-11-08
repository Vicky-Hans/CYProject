using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DH.Game;
using DH.UIFramework;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseAssetEntity),true)]
public class BaseAssetEntityCustomEditor : UnityEditor.Editor
{
    private readonly List<SerializedProperty> properties = new List<SerializedProperty>();
    private readonly List<SerializedProperty> debugProperties = new List<SerializedProperty>();
    private SerializedProperty attackComponentProp;
    private readonly Type attackComponentType = typeof(AttackComponent);

    private readonly Dictionary<SerializedProperty, SerializedObject> objectsMap =
        new Dictionary<SerializedProperty, SerializedObject>();
    private GUIStyle style = new GUIStyle();
    
    private void OnEnable()
    {
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;

        var fields = serializedObject.targetObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
        SerializedProperty iterator = serializedObject.GetIterator();
        if (iterator.NextVisible(true))
        {
            do
            {
                var prop = serializedObject.FindProperty(iterator.name);
                var field = fields.FirstOrDefault(x => x.Name == prop.name);
                if (field != null && field.FieldType == attackComponentType)
                {
                    attackComponentProp = prop;
                }
                var attr = field?.GetCustomAttribute<DebugDataAttribute>();
                if (attr == null)
                {
                    properties.Add(prop);
                }
                else
                {
                    debugProperties.Add(prop);
                }
            } while (iterator.NextVisible(false));
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        foreach (var item in properties)
        {
            EditorGUILayout.PropertyField(item);
            if (item.propertyType == SerializedPropertyType.ObjectReference && item.objectReferenceValue != null && item.objectReferenceValue.GetType() == typeof(AttackComponent))
            {
                if (!objectsMap.TryGetValue(item, out var obj))
                {
                    obj = new SerializedObject(item.objectReferenceValue);
                    objectsMap.Add(item,obj);
                }
                obj.Update();
                var property = obj.FindProperty("acceptTags");
                EditorGUILayout.PropertyField(property,true);
                obj.ApplyModifiedProperties();
                item.objectReferenceValue = obj.targetObject;
            }

            if (item == attackComponentProp && item.objectReferenceValue == null)
            {
                var targetEntity = serializedObject.targetObject as BaseAssetEntity;
                item.objectReferenceValue = targetEntity.GetOrAddComponent<AttackComponent>();
            }
        }
        EditorGUILayout.Space();
        var rect = EditorGUILayout.BeginHorizontal();
        Handles.color = Color.green;
        Handles.DrawLine(new Vector2(rect.x - 5, rect.y), new Vector2(rect.width + 5, rect.y));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("调试信息",style);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        foreach (var item in debugProperties)
        {
            EditorGUILayout.PropertyField(item);
        }
        serializedObject.ApplyModifiedProperties();
    }
}