using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CSSymbol), true)]
public class CSSymbolRandomEditor : Editor {
    private SerializedProperty _data;
    private SerializedProperty _percents;
    private Rect _rt;
    private GUILayoutOption[] _options;
    private SerializedProperty _script;

    private void OnEnable()
    {
        _data = serializedObject.FindProperty("data");
        _percents = serializedObject.FindProperty("percents");
        _script = serializedObject.FindProperty("m_Script");

        if (_percents.arraySize != _data.arraySize)
        {
            OnLoad();
        }

        _options = new GUILayoutOption[] {
                GUILayout.Width(EditorGUIUtility.singleLineHeight),
                GUILayout.Height(EditorGUIUtility.singleLineHeight)
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DefaultInspectorGUI();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Random", EditorStyles.boldLabel);

        for (int i = 0; i < _data.arraySize; i++)
        {
            SerializedProperty data = _data.GetArrayElementAtIndex(i);
            if (data.objectReferenceValue == null)
                continue;
            CSSymbolData d = data.objectReferenceValue as CSSymbolData;

            if (Event.current.type != EventType.DragPerform)
            {
                GUILayout.BeginHorizontal();
                _rt = GUILayoutUtility.GetRect(EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight, _options);

                EditorGUILayout.LabelField(d.name, GUILayout.Width(180));

                EditorGUI.DrawPreviewTexture(new Rect(_rt.x, _rt.y, _rt.width - 1, _rt.height - 1), d.sprite.texture);

                EditorGUI.BeginChangeCheck();

                SerializedProperty property = _percents.GetArrayElementAtIndex(i);
                float percent = property.FindPropertyRelative("percent").floatValue;
                EditorGUILayout.PropertyField(property);

                if (EditorGUI.EndChangeCheck())
                {
                    Equalizer(property, percent, i);
                    serializedObject.ApplyModifiedProperties();
                }

                GUILayout.EndHorizontal();
            }
        }

        if (GUILayout.Button("Reset"))
        {
            OnLoad();
        }
    }

    private void DefaultInspectorGUI()
    {
        GUI.enabled = false;
        EditorGUILayout.PropertyField(_script, true, new GUILayoutOption[0]);
        GUI.enabled = true;

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_data, true, new GUILayoutOption[0]);
        if (EditorGUI.EndChangeCheck())
        {
            OnDataChanged();
            serializedObject.ApplyModifiedProperties();
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("type"), true, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cell"), true, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("wildExpand"), true, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("wildExpandStay"), true, new GUILayoutOption[0]);
    }

    private void Equalizer(SerializedProperty property, float oldPercent, int curr)
    {
        float newPercent = property.FindPropertyRelative("percent").floatValue;
        float delta = (newPercent - oldPercent);

        int count = 0;
        for (int i = 0; i < _percents.arraySize; i++)
        {
            if (i == curr) continue;
            SerializedProperty percent = _percents.GetArrayElementAtIndex(i).FindPropertyRelative("percent");

            if (percent.floatValue < 30f || percent.floatValue > 0f)
            {
                count++;
            }
        }
        delta /= count;

        for (int i = 0; i < _percents.arraySize; i++)
        {
            if (i == curr) continue;
            SerializedProperty percent = _percents.GetArrayElementAtIndex(i).FindPropertyRelative("percent");

            if (delta < 0 ? percent.floatValue < 30f : percent.floatValue > 0f)
            {
                percent.floatValue = Mathf.Max(Mathf.Min(percent.floatValue - delta, 30f), 0f);
            }
        }
    }

    private void OnLoad()
    {
        _percents.arraySize = _data.arraySize;
        float delta = 100f / (float)_percents.arraySize;

        for (int i = 0; i < _percents.arraySize; i++)
        {
            SerializedProperty property = _percents.GetArrayElementAtIndex(i);
            SerializedProperty data = _data.GetArrayElementAtIndex(i);
            if (data.objectReferenceValue == null)
                continue;
            CSSymbolData d = data.objectReferenceValue as CSSymbolData;

            property.FindPropertyRelative("percent").floatValue = delta;
            property.FindPropertyRelative("type").enumValueIndex = (int)d.type;
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void OnDataChanged()
    {
        OnLoad();
    }
}
