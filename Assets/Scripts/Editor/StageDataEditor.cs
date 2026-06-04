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
        private static readonly string[] SignalNames = {
            "Black",
            "Red",
            "Green",
            "Blue",
            "Yellow",
            "Cyan",
            "Magenta",
            "White"
        };

        private static readonly Signal[] SignalValues = {
            Signal.Black,
            Signal.Red,
            Signal.Green,
            Signal.Blue,
            Signal.Yellow,
            Signal.Cyan,
            Signal.Magenta,
            Signal.White
        };

        private const int CellSize = 24;
        private const int CellGap = 2;
        private SerializedProperty entityBlueprintsProperty;
        private SerializedProperty goalsProperty;

        private void OnEnable()
        {
            entityBlueprintsProperty = serializedObject.FindProperty("Blueprints");
            goalsProperty = serializedObject.FindProperty("Goals");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            StageData stageData = (StageData)target;
            EditorGUI.BeginChangeCheck();

            stageData.StageName = EditorGUILayout.TextField("Stage Name", stageData.StageName);
            stageData.Width = EditorGUILayout.IntField("Width", stageData.Width);
            stageData.Height = EditorGUILayout.IntField("Height", stageData.Height);
            EditorGUILayout.LabelField("Tile Layout", EditorStyles.boldLabel);
            stageData.ResizeTiles();
            DrawTileGrid(stageData);
            EditorGUILayout.Space(20);
            DrawBlueprintStacks(entityBlueprintsProperty);
            EditorGUILayout.Space(20);
            DrawGoals(goalsProperty);
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(stageData);
        }

        private static Signal DrawSignalPopup(string label, Signal current)
        {
            int index = 0;

            for (int i = 0; i < SignalValues.Length; i++)
            {
                if (SignalValues[i] == current)
                {
                    index = i;
                    break;
                }
            }

            int newIndex = EditorGUILayout.Popup(label, index, SignalNames);
            return SignalValues[newIndex];
        }

        private static void DrawGoals(SerializedProperty goalsProperty)
        {
            EditorGUILayout.LabelField("Goals", EditorStyles.boldLabel);

            for (int i = 0; i < goalsProperty.arraySize; i++)
            {
                SerializedProperty goalProperty = goalsProperty.GetArrayElementAtIndex(i);
                SerializedProperty signalProperty = goalProperty.FindPropertyRelative("signal");
                SerializedProperty countProperty = goalProperty.FindPropertyRelative("count");

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField($"Goal {i}", EditorStyles.boldLabel);

                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    goalsProperty.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }

                EditorGUILayout.EndHorizontal();

                Signal oldSignal = signalProperty != null
                    ? (Signal)signalProperty.boxedValue
                    : Signal.Black;

                Signal newSignal = DrawSignalPopup("Color", oldSignal);

                if (signalProperty != null && newSignal != oldSignal)
                    signalProperty.boxedValue = newSignal;

                EditorGUILayout.PropertyField(countProperty);

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Goal"))
            {
                int index = goalsProperty.arraySize;
                goalsProperty.InsertArrayElementAtIndex(index);

                SerializedProperty goalProperty = goalsProperty.GetArrayElementAtIndex(index);
                SerializedProperty signalProperty = goalProperty.FindPropertyRelative("signal");
                SerializedProperty countProperty = goalProperty.FindPropertyRelative("count");

                if (signalProperty != null)
                    signalProperty.boxedValue = Signal.Black;

                if (countProperty != null)
                    countProperty.intValue = 1;
            }
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

        private static void DrawBlueprintStacks(SerializedProperty blueprintsProperty)
        {
            EditorGUILayout.LabelField("Blueprints", EditorStyles.boldLabel);

            for (int i = 0; i < blueprintsProperty.arraySize; i++)
            {
                SerializedProperty stackProperty = blueprintsProperty.GetArrayElementAtIndex(i);
                SerializedProperty blueprintProperty = stackProperty.FindPropertyRelative("blueprint");
                SerializedProperty countProperty = stackProperty.FindPropertyRelative("count");

                if (blueprintProperty.managedReferenceValue == null)
                    blueprintProperty.managedReferenceValue = new EntityBlueprint();

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField($"Blueprint {i}", EditorStyles.boldLabel);

                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    blueprintsProperty.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }

                EditorGUILayout.EndHorizontal();

                DrawBlueprintField(blueprintProperty);
                EditorGUILayout.PropertyField(countProperty);

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Blueprint Stack"))
            {
                int index = blueprintsProperty.arraySize;
                blueprintsProperty.InsertArrayElementAtIndex(index);

                SerializedProperty stackProperty = blueprintsProperty.GetArrayElementAtIndex(index);
                SerializedProperty blueprintProperty = stackProperty.FindPropertyRelative("blueprint");
                SerializedProperty countProperty = stackProperty.FindPropertyRelative("count");

                blueprintProperty.managedReferenceValue = new EntityBlueprint();
                countProperty.intValue = 1;
            }
        }

        private static void DrawBlueprintField(SerializedProperty blueprintProperty)
        {
            EntityBlueprint blueprint = blueprintProperty.managedReferenceValue as EntityBlueprint;

            if (blueprint == null)
            {
                blueprintProperty.managedReferenceValue = new EntityBlueprint();
                return;
            }

            CircuitElement.CircuitElementType oldType = blueprint.Type;
            CircuitElement.CircuitElementType newType =
                (CircuitElement.CircuitElementType)EditorGUILayout.EnumPopup("Type", oldType);

            bool needColor = ColoredBlueprint.HasColor(newType);

            if (needColor)
            {
                Signal oldSignal = blueprint is ColoredBlueprint colored
                    ? colored.Signal
                    : Signal.Black;

                if (newType != oldType || blueprint is not ColoredBlueprint)
                {
                    blueprintProperty.managedReferenceValue = new ColoredBlueprint(newType, oldSignal);
                    return;
                }

                Signal newSignal = DrawSignalPopup("Color", oldSignal);

                if (newSignal != oldSignal)
                    blueprintProperty.managedReferenceValue = new ColoredBlueprint(newType, newSignal);
            }
            else
            {
                if (newType != oldType || blueprint is ColoredBlueprint)
                    blueprintProperty.managedReferenceValue = new EntityBlueprint(newType);
            }
        }
    }
}
#endif