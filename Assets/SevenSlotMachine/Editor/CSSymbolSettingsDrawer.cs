using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(CSSymbolPercent))]
public class CSSymbolSettingsDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.Slider(position, property.FindPropertyRelative("percent"), 0f, 30f, "");

        EditorGUI.EndProperty();
    }
}
