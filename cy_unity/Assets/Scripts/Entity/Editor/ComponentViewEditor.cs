using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DH
{
    [CustomEditor(typeof(ComponentView))]

    public class ComponentViewEditor : Editor
    {
        private SerializedProperty typeName;
        private SerializedProperty entity;
    
        private void OnEnable()
        {
            typeName = serializedObject.FindProperty("fullTypeName");
            entity = serializedObject.FindProperty("component");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(typeName);
            if (string.IsNullOrEmpty(typeName.stringValue))
            {
                //target.name = "Component View";
            }
            else
            {
                //target.name = typeName.stringValue;
            }
            ComponentViewHelper.Draw(entity,typeName.stringValue);
            serializedObject.ApplyModifiedProperties();
        }
    }

    public static class ComponentViewHelper
    {
        private static readonly List<ITypeDrawer> typeDrawers = new List<ITypeDrawer>();

        static ComponentViewHelper()
        {
            Assembly assembly = typeof(ComponentViewHelper).Assembly;
            foreach (Type type in assembly.GetTypes())
            {
                if (!type.IsDefined(typeof(TypeDrawerAttribute)))
                {
                    continue;
                }

                ITypeDrawer iTypeDrawer = (ITypeDrawer)Activator.CreateInstance(type);
                typeDrawers.Add(iTypeDrawer);
            }
        }
        
        public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty serializedProperty)
        {
            SerializedProperty currentProperty = serializedProperty.Copy();
            SerializedProperty nextSiblingProperty = serializedProperty.Copy();
            {
                nextSiblingProperty.Next(false);
            }
 
            if (currentProperty.Next(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;
 
                    yield return currentProperty;
                }
                while (currentProperty.Next(false));
            }
        }

        private static ComponentView GetMatched(ComponentView[] container, long id)
        {
            foreach (var component in container)
            {
                if (UnityEngine.Serialization.ManagedReferenceUtility.GetManagedReference(component,id) != null)
                {
                    return component;
                }
            }

            return null;
        }
        
        public static void Draw(SerializedProperty entity,string typeName)
        {
            try
            {
                var entities = (entity.serializedObject.targetObject as MonoBehaviour)
                    .GetComponentsInChildren<ComponentView>();
                var type = Type.GetType(typeName);
                var oldType = entity.managedReferenceValue?.GetType();
                if (entity.managedReferenceValue == null)
                {
                    if (type == null)
                    {
                        return;
                    }

                    entity.managedReferenceValue = Activator.CreateInstance(type);
                }
                else if(oldType != type)
                {
                    if (type == null)
                    {
                        entity.managedReferenceValue = null;
                        return;
                    }

                    entity.managedReferenceValue = Activator.CreateInstance(type);
                }
                
                foreach (var property in GetChildren(entity))
                {
                    if (property.propertyType == SerializedPropertyType.ManagedReference)
                    {
                        var oldComponent = GetMatched( entities,property.managedReferenceId);
                        var newComponent = EditorGUILayout.ObjectField(property.name,oldComponent,typeof(ComponentView),true) as ComponentView;
                        if (newComponent == oldComponent)
                        {
                            continue;
                        }
                        
                        var targetValue = newComponent != null ? newComponent.component : null;
                        property.managedReferenceId =
                            UnityEngine.Serialization.ManagedReferenceUtility.GetManagedReferenceIdForObject(newComponent, targetValue);
                        continue;
                    }

                    EditorGUILayout.PropertyField(property, true);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"component view error: {entity.GetType().FullName} {e}");
            }
        }
    }


}