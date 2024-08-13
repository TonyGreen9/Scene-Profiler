using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SceneProfiler.Editor.GUI
{
    public class SceneFileProfilerGUI : ProfilerGUI<SceneFileDetails>
    {
        public SceneFileProfilerGUI(SceneProfiler profiler, Color defColor)
            : base(profiler, defColor)
        {
            InitializeColumns();
            InitializeColumnHeader();
        }

        protected override void InitializeColumns()
        {
            var columnDefinitions = new (string, float, bool)[]
            {
                ("GameObject", 150, false),
                ("Name in Scene File", 150, false),
                ("Lines", 80, false),
                ("Identifier", 120, false),
                ("Object Type", 120, false),
                ("GUID", 100, false),              // New column for GUID
                ("Size(KB)", 80, false),         // New column for Size in KB
                ("Size(%)", 80, false), // New column for Size Percentage
                ("Components", 80, false) 
            };

            columns = columnDefinitions.Select(def => CreateColumn(def.Item1, def.Item2, def.Item3)).ToArray();
        }


        protected override List<SceneFileDetails> GetProfilerItems()
        {
            return profiler.SceneFileDetails;
        }

        protected override int CompareItems(SceneFileDetails a, SceneFileDetails b, int columnIndex)
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;

            switch (columnIndex)
            {
                case 0:
                    return string.Compare(a.gameObject?.name ?? string.Empty, b.gameObject?.name ?? string.Empty);
                case 1:
                    return string.Compare(a.gameObjectName, b.gameObjectName);
                case 2:
                    return a.lineCount.CompareTo(b.lineCount);
                case 3:
                    return string.Compare(a.identifier, b.identifier);
                case 4:
                    return string.Compare(a.objectType, b.objectType);
                case 5:
                    return string.Compare(a.guid, b.guid);
                case 6:
                    return a.sizeInKB.CompareTo(b.sizeInKB);
                case 7:
                    return a.sizePercentage.CompareTo(b.sizePercentage);
                case 8:
                    return a.components.Count.CompareTo(b.components.Count);  // Comparing Component Count
                default:
                    return 0;
            }
        }



        public void ListSceneFileDetails()
        {
            if (buttonStyle == null || labelStyle == null)
            {
                InitializeStyles();
            }

            var headerRect = GUILayoutUtility.GetRect(0, columnHeader.height, GUILayout.ExpandWidth(true));
            columnHeader.OnGUI(headerRect, scrollPosition.x);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            int displayedItems = 0;
            foreach (var details in profiler.SceneFileDetails)
            {
                if (displayedItems >= profiler.currentObjectsInColumnCount) break;

                EditorGUILayout.BeginHorizontal();

                for (int visibleColumnIndex = 0; visibleColumnIndex < columnHeader.state.visibleColumns.Length; visibleColumnIndex++)
                {
                    int columnIndex = columnHeader.state.visibleColumns[visibleColumnIndex];
                    Rect cellRect = EditorGUILayout.GetControlRect(GUILayout.Width(columns[columnIndex].width - 4), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    cellRect.x += headerRect.x;

                    switch (columnIndex)
                    {
                        case 0:
                            DrawGameObject(details, cellRect);
                            break;
                        case 1:
                            DrawObjectName(details, cellRect);
                            break;
                        case 2:
                            DrawLineCount(details, cellRect);
                            break;
                        case 3:
                            DrawIdentifier(details, cellRect);
                            break;
                        case 4:
                            DrawObjectType(details, cellRect);
                            break;
                        case 5:
                            DrawGUID(details, cellRect);
                            break;
                        case 6:
                            DrawSizeInKB(details, cellRect);
                            break;
                        case 7:
                            DrawSizePercentage(details, cellRect);
                            break;
                        case 8:
                            DrawComponentCount(details, cellRect);  // Drawing the Component Count column
                            break;
                    }
                }

                EditorGUILayout.EndHorizontal();
                displayedItems++;
            }

            if (profiler.currentObjectsInColumnCount < profiler.SceneFileDetails.Count)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Load More", GUILayout.Width(150)))
                {
                    profiler.currentObjectsInColumnCount += 100;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }


        private void DrawGameObject(SceneFileDetails details, Rect cellRect)
        {
            if (details.gameObject != null)
            {
                if (UnityEngine.GUI.Button(cellRect, details.gameObject.name, buttonStyle))
                {
                    Selection.activeObject = details.gameObject;
                }
            }
        }

        private void DrawObjectName(SceneFileDetails details, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, details.gameObjectName, labelStyle);
        }

        private void DrawLineCount(SceneFileDetails details, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, details.lineCount.ToString(), labelStyle);
        }

        private void DrawIdentifier(SceneFileDetails details, Rect cellRect)
        {
            // Drawing the identifier as a text field to allow copying
            EditorGUI.TextField(cellRect, details.identifier);
        }

        private void DrawObjectType(SceneFileDetails details, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, details.objectType, labelStyle);
        }
        
        private void DrawGUID(SceneFileDetails details, Rect cellRect)
        {
            EditorGUI.TextField(cellRect, details.guid);
        }

        private void DrawSizeInKB(SceneFileDetails details, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, details.sizeInKB.ToString("F2"), labelStyle);
        }

        private void DrawSizePercentage(SceneFileDetails details, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, details.sizePercentage.ToString("F2") + " %", labelStyle);
        }
        
        private void DrawComponentCount(SceneFileDetails details, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, details.components.Count.ToString(), labelStyle);
        }
    }
}
