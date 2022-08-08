using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(CSCell))]
public class CSCellDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        Vector2 c_size = Vector2.zero;
        GUIContent columnContent = Content("Column", ref c_size);

        Vector2 r_size = Vector2.zero;
        GUIContent rowContent = Content("Row", ref r_size);

        var columnLabel = new Rect(position.x, position.y, c_size.x, position.height);
        var columnField = new Rect(position.x + c_size.x, position.y, position.width * 0.31f, position.height);

        var rowLabel = new Rect(position.x + c_size.x + position.width * 0.344f, position.y, r_size.x, position.height);
        var rowField = new Rect(position.x + c_size.x + position.width * 0.344f + r_size.x, position.y, position.width * 0.31f, position.height);


        EditorGUI.LabelField(columnLabel, columnContent);
        EditorGUI.PropertyField(columnField, property.FindPropertyRelative("column"), GUIContent.none);


        EditorGUI.LabelField(rowLabel, rowContent);
        EditorGUI.PropertyField(rowField, property.FindPropertyRelative("row"), GUIContent.none);


        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }

    private GUIContent Content(string str, ref Vector2 size)
    {
        GUIContent content = new GUIContent(str);

        GUIStyle style = GUI.skin.box;
        style.alignment = TextAnchor.MiddleCenter;

        size = style.CalcSize(content);
        return content;
    }
}
