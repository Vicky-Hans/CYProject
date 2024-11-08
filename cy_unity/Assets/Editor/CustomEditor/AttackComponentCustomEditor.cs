using DH.Game;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AttackComponent))]
public class AttackComponentCustomEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        var attackComponent = serializedObject.targetObject as AttackComponent;
        if (!attackComponent)
        {
            base.OnInspectorGUI();
            return;
        }

        var assetEntity = attackComponent.GetComponent<BaseAssetEntity>();
        if (!assetEntity)
        {
            base.OnInspectorGUI();
        }
        else
        {
            GUI.enabled = false;
            var iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
                do
                {
                    var prop = serializedObject.FindProperty(iterator.name);
                    if (prop.name == "acceptTags") continue;

                    EditorGUILayout.PropertyField(prop);
                } while (iterator.NextVisible(false));

            GUI.enabled = true;
        }
    }
}