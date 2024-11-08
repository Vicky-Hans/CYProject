using DH.Game;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using Provider = AddressableAssetSettingsLocatorExtension;

[CustomPropertyDrawer(typeof(SyncAssetPathAttribute))]
public class SyncAssetPathPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.String)
        {
            EditorGUI.BeginProperty(position, label, property);
            Object target = null;
            Object oldValue = null;
            if (!string.IsNullOrEmpty(property.stringValue))
            {
                Provider.Locate(property.stringValue, typeof(GameObject), out var locations);
                if (locations?.Count > 0)
                {
                    var assetPath = locations[0].InternalId;
                    oldValue = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                }
            }

            target = EditorGUI.ObjectField(position, $"{label} (Ref)", oldValue, typeof(GameObject), false);
            if (oldValue != target &&
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(target, out var guid, out long localId))
            {
                var address = Provider.FindAddress(guid);
                if (!string.IsNullOrEmpty(address)) property.stringValue = address;
            }

            EditorGUI.EndProperty();
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}