using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CSReel), true)]
public class CSReelEditor : Editor {
    private CSReel _reel;
    private CSCell _gridSize;
    private GUILayoutOption[] _options;
    private Rect _rt;
    private SerializedProperty _symbolEnabled;
    private SerializedProperty _script;

    private void OnEnable()
    {
        _reel = target as CSReel;
        _symbolEnabled = serializedObject.FindProperty("symbolEnabled");
        _script = serializedObject.FindProperty("m_Script");

        OnLoad();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUI.enabled = false;
        EditorGUILayout.PropertyField(_script, true, new GUILayoutOption[0]);
        GUI.enabled = true;

        SerializedProperty symbol = serializedObject.FindProperty("symbol");

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(symbol);
        if (EditorGUI.EndChangeCheck())
        {
            OnSymbol(symbol);
            serializedObject.ApplyModifiedProperties();
        }

        if (symbol.objectReferenceValue == null)
            return;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Reel Symbols", EditorStyles.boldLabel);

        for (int r = 0; r < _gridSize.row; r++)
        {
            CSSymbolData data = _reel.symbol.GetComponent<CSSymbol>().data[r];

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(data.name, GUILayout.MaxWidth(100));

            SerializedProperty l0 = _symbolEnabled.FindPropertyRelative("l0").GetArrayElementAtIndex(r);

            for (int c = 0; c < _gridSize.column; c++)
            {
                SerializedProperty column = l0.FindPropertyRelative("l1").GetArrayElementAtIndex(c);

                GUILayout.BeginHorizontal();

                _rt = GUILayoutUtility.GetRect(EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight, _options);
                EditorGUI.DrawPreviewTexture(new Rect(_rt.x, _rt.y, _rt.width - 1, _rt.height - 1), data.sprite.texture);

                EditorGUI.BeginChangeCheck();
                column.boolValue = EditorGUILayout.Toggle(column.boolValue);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Reset"))
        {
            ResetValues(true);
        }
    }

    private void OnLoad()
    {
        SerializedProperty symbol = serializedObject.FindProperty("symbol");
        if (symbol.objectReferenceValue == null)
            return;

        _gridSize = new CSCell(5, (symbol.objectReferenceValue as GameObject).GetComponent<CSSymbol>().data.Length);

        _options = new GUILayoutOption[] {
                GUILayout.Width(EditorGUIUtility.singleLineHeight),
                GUILayout.Height(EditorGUIUtility.singleLineHeight)
        };

        if (_symbolEnabled.FindPropertyRelative("l0").arraySize != _gridSize.row)
        {
            ResetValues();
        }
    }

    private void ResetValues(bool force = false)
    {
        SerializedProperty l0 = _symbolEnabled.FindPropertyRelative("l0");
        l0.arraySize = _gridSize.row;

        for (int r = 0; r < _gridSize.row; r++)
        {
            SerializedProperty l1 = l0.GetArrayElementAtIndex(r).FindPropertyRelative("l1");
            l1.arraySize = _gridSize.column;

            for (int c = 0; c < _gridSize.column; c++)
            {
                l1.GetArrayElementAtIndex(c).boolValue = true;
            }
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void OnSymbol(SerializedProperty symbol)
    {
        if (symbol.objectReferenceValue != null)
        {
            OnLoad();
            ResetValues();
        }
        else
        {
            _symbolEnabled.FindPropertyRelative("l0").arraySize = 0;
        }
    }
}
