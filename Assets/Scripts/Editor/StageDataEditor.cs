#if UNITY_EDITOR
using System.Drawing.Printing;
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
        private static readonly Color[] SignalColors =
        {
            Color.black,   // 0b000 = 0
            Color.red,     // 0b001 = 1
            Color.green,   // 0b010 = 2
            Color.yellow,  // 0b011 = 3
            Color.blue,    // 0b100 = 4
            Color.magenta, // 0b101 = 5
            Color.cyan,    // 0b110 = 6
            Color.white    // 0b111 = 7
        };

        private int selectedTab = 0;
        private SerializedProperty entityBlueprintsProperty;
        private SerializedProperty goalsProperty;

        private static int sectionGap = 15;
        private static GUIStyle titleStyle;

        private static GUIStyle TitleStyle
        {
            get
            {
                if (titleStyle == null)
                {
                    titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, fontStyle = FontStyle.Bold};
                    titleStyle.normal.textColor = Color.white;
                }
                return titleStyle;
            }
        }

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
            selectedTab = GUILayout.Toolbar(selectedTab, new string[] { "Grid", "Blueprint", "Goal" });

            switch (selectedTab)
            {
                case 0:
                    DrawGridTab(stageData);
                    break;
                case 1:
                    DrawBlueprintTab(entityBlueprintsProperty);
                    break;
                case 2:
                    DrawGoalTab(goalsProperty);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(stageData);
        }

        /// <summary>
        /// 스테이지의 그리드와 고정 엔티티를 설정합니다.
        /// </summary>
        private static void DrawGridTab(StageData stageData)
        {
            // 스테이지 프로퍼티
            EditorGUILayout.LabelField("Stage Properties", TitleStyle);
            stageData.StageName = EditorGUILayout.TextField("Stage Name", stageData.StageName);
            stageData.Width = Mathf.Max(1, EditorGUILayout.IntField("Width", stageData.Width));
            stageData.Height = Mathf.Max(1, EditorGUILayout.IntField("Height", stageData.Height));
            EditorGUILayout.Space(sectionGap);

            // 그리드 설정
            EditorGUILayout.LabelField("Tile Layout", TitleStyle);
            stageData.ResizeTiles();
            for (int y = stageData.Height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < stageData.Width; x++)
                {
                    bool enabled = stageData.IsEnabledTile(x, y);
                    GUI.backgroundColor = enabled ? new Color(0.35f, 0.8f, 0.45f) : new Color(0.25f, 0.25f, 0.25f);

                    if (GUILayout.Button("", GUILayout.Width(24), GUILayout.Height(24)))
                    {
                        Undo.RecordObject(stageData, "Toggle Stage Tile");
                        stageData.SetEnabledTile(x, y, !enabled);
                        EditorUtility.SetDirty(stageData);
                    }
                    GUILayout.Space(2);
                }
                EditorGUILayout.EndHorizontal();
            }

            // 고정 엔티티
        }

        private static QuantumSignal DrawQuantumSignalField(string label, QuantumSignal current)
        {
            EditorGUILayout.LabelField(label);
            EditorGUILayout.BeginVertical("box");
            Rect[] rects = new Rect[SignalColors.Length];

            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < SignalColors.Length; i++)
                rects[i] = DrawSignalColorCell(i);
            EditorGUILayout.EndHorizontal();

            byte mask = current.Mask;
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < SignalColors.Length; i++)
            {
                Rect rect = GUILayoutUtility.GetRect(22, 22, GUILayout.Width(22), GUILayout.Height(22));
                
                bool enabled = (mask & (1 << i)) != 0;
                bool newEnabled = EditorGUI.Toggle(rect, enabled);
                if (newEnabled)
                    mask |= (byte)(1 << i);
                else
                    mask &= (byte)~(1 << i);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            return new QuantumSignal(mask);
        }

        private static Rect DrawSignalColorCell(int index)
        {
            Rect rect = GUILayoutUtility.GetRect(22, 22, GUILayout.Width(22), GUILayout.Height(22));
            EditorGUI.DrawRect(rect, SignalColors[index]);

            Handles.color = Color.gray;
            Handles.DrawAAPolyLine(
                1f,
                new Vector3(rect.xMin, rect.yMin),
                new Vector3(rect.xMax, rect.yMin),
                new Vector3(rect.xMax, rect.yMax),
                new Vector3(rect.xMin, rect.yMax),
                new Vector3(rect.xMin, rect.yMin)
            );

            return rect;
        }

        private static void DrawGoalTab(SerializedProperty goalsProperty)
        {
            EditorGUILayout.LabelField("Goals", TitleStyle);
            for (int i = 0; i < goalsProperty.arraySize; i++)
            {
                SerializedProperty goalProperty = goalsProperty.GetArrayElementAtIndex(i);
                SerializedProperty signalProperty = goalProperty.FindPropertyRelative("signal");
                SerializedProperty countProperty = goalProperty.FindPropertyRelative("count");

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();

                Rect lineRect = GUILayoutUtility.GetRect(1, 18, GUILayout.ExpandWidth(true));
                float y = lineRect.y + lineRect.height * 0.5f;
                Handles.color = Color.white;
                Handles.DrawLine(new Vector3(lineRect.xMin, y), new Vector3(lineRect.xMax, y));

                if (GUILayout.Button("✕", GUILayout.Width(20)))
                {
                    goalsProperty.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }

                EditorGUILayout.EndHorizontal();

                QuantumSignal oldSignal = signalProperty != null ? (QuantumSignal)signalProperty.boxedValue : QuantumSignal.Null;
                QuantumSignal newSignal = DrawQuantumSignalField("Signal", oldSignal);

                if (signalProperty != null && newSignal != oldSignal)
                    signalProperty.boxedValue = newSignal;

                if (countProperty != null)
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
                    signalProperty.boxedValue = QuantumSignal.Null;

                if (countProperty != null)
                    countProperty.intValue = 1;
            }
        }

        private static void DrawBlueprintTab(SerializedProperty blueprintsProperty)
        {
            EditorGUILayout.LabelField("Blueprints", TitleStyle);
            for (int i = 0; i < blueprintsProperty.arraySize; i++)
            {
                SerializedProperty stackProperty = blueprintsProperty.GetArrayElementAtIndex(i);
                SerializedProperty blueprintProperty = stackProperty.FindPropertyRelative("blueprint");
                SerializedProperty countProperty = stackProperty.FindPropertyRelative("count");

                if (blueprintProperty.managedReferenceValue == null)
                    blueprintProperty.managedReferenceValue = new EntityBlueprint();

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();

                Rect lineRect = GUILayoutUtility.GetRect(1, 18, GUILayout.ExpandWidth(true));
                float y = lineRect.y + lineRect.height * 0.5f;
                Handles.color = Color.white;
                Handles.DrawLine(new Vector3(lineRect.xMin, y), new Vector3(lineRect.xMax, y));

                if (GUILayout.Button("✕", GUILayout.Width(20)))
                {
                    blueprintsProperty.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }

                EditorGUILayout.EndHorizontal();

                DrawBlueprintField(blueprintProperty);
                if (countProperty != null)
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

                if (countProperty != null)
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
            CircuitElement.CircuitElementType newType = (CircuitElement.CircuitElementType)EditorGUILayout.EnumPopup("Type", oldType);

            bool needColor = ColoredBlueprint.HasColor(newType);
            if (needColor)
            {
                QuantumSignal oldSignal = blueprint is ColoredBlueprint colored ? colored.Signal : QuantumSignal.Null;
                if (newType != oldType || blueprint is not ColoredBlueprint)
                {
                    blueprintProperty.managedReferenceValue = new ColoredBlueprint(newType, oldSignal);
                    return;
                }

                QuantumSignal newSignal = DrawQuantumSignalField("Signal", oldSignal);
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