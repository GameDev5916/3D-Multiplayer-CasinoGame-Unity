using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(CSCardValue))]
public class CSCardValueDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Card Value"));

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var rect1 = new Rect(position.x, position.y, position.width * 0.49f, position.height);
        var rect2 = new Rect(position.x + position.width * 0.51f, position.y, position.width * 0.49f, position.height);

        EditorGUI.PropertyField(rect1, property.FindPropertyRelative("suit"), GUIContent.none);
        EditorGUI.PropertyField(rect2, property.FindPropertyRelative("rank"), GUIContent.none);

        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
