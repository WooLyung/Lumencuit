#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Lumencuit.Editor
{
    /// <summary>
    /// 챕터 데이터의 인스펙터 에디터입니다.
    /// </summary>
    [CustomEditor(typeof(ChapterData))]
    public sealed class ChapterDataEditor : UnityEditor.Editor
    {
        private SerializedProperty stageInfosProperty;

        private static readonly int sectionGap = 15;

        private const float TileWidth = 32f;
        private const float TileHeight = 32f;
        private const float ArrowWidth = 10f;
        private const float ArrowHeight = 8f;
        private const float CenterSize = 18f;
        private const float TileGap = 2f;

        private static GUIStyle titleStyle;
        private static GUIStyle invisibleButtonStyle;
        private static GUIStyle centerButtonStyle;
        private static GUIStyle arrowLabelStyle;

        private static readonly Color DisabledColor = new(0.25f, 0.25f, 0.25f);
        private static readonly Color EnabledColor = new(0.35f, 0.8f, 0.45f);
        private static readonly Color StageColor = new(0.95f, 0.8f, 0.25f);

        private static readonly Color DirectionOnColor = Color.white;

        private static GUIStyle TitleStyle
        {
            get
            {
                if (titleStyle == null)
                {
                    titleStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 16,
                        fontStyle = FontStyle.Bold
                    };

                    titleStyle.normal.textColor = Color.white;
                }

                return titleStyle;
            }
        }

        private static GUIStyle InvisibleButtonStyle
        {
            get
            {
                if (invisibleButtonStyle == null)
                {
                    invisibleButtonStyle = new GUIStyle(GUIStyle.none)
                    {
                        padding = new RectOffset(0, 0, 0, 0),
                        margin = new RectOffset(0, 0, 0, 0)
                    };
                }

                return invisibleButtonStyle;
            }
        }

        private static GUIStyle CenterButtonStyle
        {
            get
            {
                if (centerButtonStyle == null)
                {
                    centerButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        fontSize = 10,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter,
                        padding = new RectOffset(0, 0, 0, 0),
                        margin = new RectOffset(0, 0, 0, 0)
                    };
                }

                return centerButtonStyle;
            }
        }

        private static GUIStyle ArrowLabelStyle
        {
            get
            {
                if (arrowLabelStyle == null)
                {
                    arrowLabelStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 6,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter,
                        padding = new RectOffset(0, 0, 0, 0),
                        margin = new RectOffset(0, 0, 0, 0)
                    };
                    arrowLabelStyle.normal.textColor = DirectionOnColor;
                }

                return arrowLabelStyle;
            }
        }

        private void OnEnable()
        {
            stageInfosProperty = serializedObject.FindProperty("StageInfos");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ChapterData chapterData = (ChapterData)target;

            EditorGUI.BeginChangeCheck();

            DrawChapterProperties(chapterData);

            EditorGUILayout.Space(sectionGap);
            EditorGUILayout.LabelField("Chapter Grid", TitleStyle);
            DrawGrid(chapterData);

            EditorGUILayout.Space(sectionGap);
            EditorGUILayout.LabelField("Stage Infos", TitleStyle);
            DrawStageInfos(stageInfosProperty);

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(chapterData);
        }

        private static void DrawChapterProperties(ChapterData chapterData)
        {
            EditorGUILayout.LabelField("Chapter Properties", TitleStyle);

            chapterData.ChapterId = EditorGUILayout.TextField("Chapter Id", chapterData.ChapterId);

            EditorGUI.BeginChangeCheck();

            int newWidth = Mathf.Max(1, EditorGUILayout.IntField("Width", chapterData.Width));
            int newHeight = Mathf.Max(1, EditorGUILayout.IntField("Height", chapterData.Height));

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(chapterData, "Resize Chapter Grid");

                chapterData.Width = newWidth;
                chapterData.Height = newHeight;
                chapterData.ResizeTiles();

                EditorUtility.SetDirty(chapterData);
            }
        }

        private static void DrawGrid(ChapterData chapterData)
        {
            chapterData.ResizeTiles();

            for (int y = chapterData.Height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();

                for (int x = 0; x < chapterData.Width; x++)
                {
                    ChapterData.ChapterTileData tile = chapterData.GetTile(x, y);

                    DrawTile(chapterData, tile);

                    GUILayout.Space(TileGap);
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(TileGap);
            }
        }

        private static void DrawTile(
            ChapterData chapterData,
            ChapterData.ChapterTileData tile)
        {
            if (tile == null)
                return;

            Rect rect = GUILayoutUtility.GetRect(
                TileWidth,
                TileHeight,
                GUILayout.Width(TileWidth),
                GUILayout.Height(TileHeight)
            );

            EditorGUI.DrawRect(rect, GetTileColor(tile));

            Handles.color = Color.gray;
            Handles.DrawAAPolyLine(
                1f,
                new Vector3(rect.xMin, rect.yMin),
                new Vector3(rect.xMax, rect.yMin),
                new Vector3(rect.xMax, rect.yMax),
                new Vector3(rect.xMin, rect.yMax),
                new Vector3(rect.xMin, rect.yMin)
            );

            Rect centerRect = new Rect(
                rect.center.x - CenterSize * 0.5f,
                rect.center.y - CenterSize * 0.5f,
                CenterSize,
                CenterSize
            );

            Rect upRect = new Rect(
                rect.center.x - ArrowWidth * 0.5f,
                rect.yMin + 1f,
                ArrowWidth,
                ArrowHeight
            );

            Rect downRect = new Rect(
                rect.center.x - ArrowWidth * 0.5f,
                rect.yMax - ArrowHeight - 1f,
                ArrowWidth,
                ArrowHeight
            );

            Rect leftRect = new Rect(
                rect.xMin + 1f,
                rect.center.y - ArrowHeight * 0.5f,
                ArrowWidth,
                ArrowHeight
            );

            Rect rightRect = new Rect(
                rect.xMax - ArrowWidth - 1f,
                rect.center.y - ArrowHeight * 0.5f,
                ArrowWidth,
                ArrowHeight
            );

            DrawDirectionButton(chapterData, tile, upRect, ChapterData.DirectionFlags.Up, "▼");
            DrawDirectionButton(chapterData, tile, downRect, ChapterData.DirectionFlags.Down, "▲");
            DrawDirectionButton(chapterData, tile, leftRect, ChapterData.DirectionFlags.Left, "▶");
            DrawDirectionButton(chapterData, tile, rightRect, ChapterData.DirectionFlags.Right, "◀");

            DrawTileStateButton(chapterData, tile, centerRect);
            HandleStageNumberScroll(chapterData, tile, centerRect);
        }

        private static Color GetTileColor(ChapterData.ChapterTileData tile)
        {
            if (!tile.Enabled)
                return DisabledColor;

            if (tile.StageNumber > 0)
                return StageColor;

            return EnabledColor;
        }

        private static void DrawTileStateButton(
            ChapterData chapterData,
            ChapterData.ChapterTileData tile,
            Rect rect)
        {
            Color prevBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = GetTileColor(tile);

            string label = tile.Enabled && tile.StageNumber > 0
                ? tile.StageNumber.ToString()
                : "";

            if (GUI.Button(rect, label, CenterButtonStyle))
            {
                Undo.RecordObject(chapterData, "Change Chapter Tile State");

                if (!tile.Enabled)
                {
                    tile.Enabled = true;
                    tile.StageNumber = 0;
                }
                else if (tile.StageNumber == 0)
                {
                    tile.StageNumber = GetNextStageNumber(chapterData);
                }
                else
                {
                    tile.Enabled = false;
                    tile.StageNumber = 0;
                    tile.InputDirections = ChapterData.DirectionFlags.None;
                }

                EditorUtility.SetDirty(chapterData);
            }

            GUI.backgroundColor = prevBackgroundColor;
        }

        private static void DrawDirectionButton(
            ChapterData chapterData,
            ChapterData.ChapterTileData tile,
            Rect rect,
            ChapterData.DirectionFlags direction,
            string label)
        {
            bool enabled = (tile.InputDirections & direction) != 0;

            if (GUI.Button(rect, "", InvisibleButtonStyle))
            {
                Undo.RecordObject(chapterData, "Toggle Chapter Tile Output");

                tile.InputDirections ^= direction;

                EditorUtility.SetDirty(chapterData);
            }

            if (!enabled)
                return;

            EditorGUI.DrawRect(rect, new Color(0.0f, 0.45f, 0.65f, 0.95f));
            GUI.Label(rect, label, ArrowLabelStyle);
        }

        private static void HandleStageNumberScroll(
            ChapterData chapterData,
            ChapterData.ChapterTileData tile,
            Rect rect)
        {
            if (!tile.Enabled || tile.StageNumber <= 0)
                return;

            Event currentEvent = Event.current;

            if (currentEvent.type != EventType.ScrollWheel)
                return;

            if (!rect.Contains(currentEvent.mousePosition))
                return;

            Undo.RecordObject(chapterData, "Change Stage Number");

            int delta = currentEvent.delta.y > 0 ? -1 : 1;

            tile.StageNumber = Mathf.Max(1, tile.StageNumber + delta);

            EditorUtility.SetDirty(chapterData);

            currentEvent.Use();
        }

        private static int GetNextStageNumber(ChapterData chapterData)
        {
            int max = 0;

            for (int y = 0; y < chapterData.Height; y++)
            {
                for (int x = 0; x < chapterData.Width; x++)
                {
                    ChapterData.ChapterTileData tile = chapterData.GetTile(x, y);

                    if (tile == null)
                        continue;

                    max = Mathf.Max(max, tile.StageNumber);
                }
            }

            return max + 1;
        }

        private static void DrawStageInfos(SerializedProperty stageInfosProperty)
        {
            for (int i = 0; i < stageInfosProperty.arraySize; i++)
            {
                SerializedProperty itemProperty = stageInfosProperty.GetArrayElementAtIndex(i);
                SerializedProperty stageNumberProperty = itemProperty.FindPropertyRelative("StageNumber");
                SerializedProperty stageDataProperty = itemProperty.FindPropertyRelative("StageData");
                SerializedProperty isHardProperty = itemProperty.FindPropertyRelative("IsHard");

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();

                Rect lineRect = GUILayoutUtility.GetRect(1, 18, GUILayout.ExpandWidth(true));
                float y = lineRect.y + lineRect.height * 0.5f;

                Handles.color = Color.white;
                Handles.DrawLine(
                    new Vector3(lineRect.xMin, y),
                    new Vector3(lineRect.xMax, y)
                );

                if (GUILayout.Button("✕", GUILayout.Width(20)))
                {
                    stageInfosProperty.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(stageNumberProperty, new GUIContent("Stage Number"));
                EditorGUILayout.PropertyField(stageDataProperty, new GUIContent("Stage Data"));
                EditorGUILayout.PropertyField(isHardProperty, new GUIContent("Hard"));

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Stage Info"))
            {
                int index = stageInfosProperty.arraySize;
                stageInfosProperty.InsertArrayElementAtIndex(index);

                SerializedProperty itemProperty = stageInfosProperty.GetArrayElementAtIndex(index);
                SerializedProperty stageNumberProperty = itemProperty.FindPropertyRelative("StageNumber");
                SerializedProperty stageDataProperty = itemProperty.FindPropertyRelative("StageData");
                SerializedProperty isHardProperty = itemProperty.FindPropertyRelative("IsHard");

                stageNumberProperty.intValue = index + 1;
                stageDataProperty.objectReferenceValue = null;
                isHardProperty.boolValue = false;
            }
        }
    }
}
#endif