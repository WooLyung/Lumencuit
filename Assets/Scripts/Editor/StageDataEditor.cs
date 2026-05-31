#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Lumencuit.Editor
{
    /// <summary>
    /// 스테이지 데이터의 인스펙터 에디터입니다.
    /// </summary>
    [CustomEditor(typeof(StageData))]
    public sealed class StageDataEditor : UnityEditor.Editor
    {
        private const int CellSize = 24;
        private const int CellGap = 2;
        private SerializedProperty entityBlueprintsProperty;

        private void OnEnable()
        {
            entityBlueprintsProperty = serializedObject.FindProperty("Blueprints");
        }

        public override void OnInspectorGUI()
        {
            StageData stageData = (StageData)target;
            EditorGUI.BeginChangeCheck();

            stageData.StageName = EditorGUILayout.TextField("Stage Name", stageData.StageName);
            stageData.Width = EditorGUILayout.IntField("Width", stageData.Width);
            stageData.Height = EditorGUILayout.IntField("Height", stageData.Height);
            EditorGUILayout.PropertyField(entityBlueprintsProperty, true);

            stageData.ResizeTiles();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Tile Layout", EditorStyles.boldLabel);
            DrawTileGrid(stageData);

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(stageData);
        }

        private static void DrawTileGrid(StageData stageData)
        {
            for (int y = stageData.Height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < stageData.Width; x++)
                {
                    bool enabled = stageData.IsEnabledTile(x, y);
                    GUI.backgroundColor = enabled ? new Color(0.35f, 0.8f, 0.45f) : new Color(0.25f, 0.25f, 0.25f);
                    if (GUILayout.Button("", GUILayout.Width(CellSize), GUILayout.Height(CellSize)))
                    {
                        Undo.RecordObject(stageData, "Toggle Stage Tile");
                        stageData.SetEnabledTile(x, y, !enabled);
                        EditorUtility.SetDirty(stageData);
                    }
                    GUILayout.Space(CellGap);
                }
                EditorGUILayout.EndHorizontal();
            }
            GUI.backgroundColor = Color.white;
        }
    }
}
#endif