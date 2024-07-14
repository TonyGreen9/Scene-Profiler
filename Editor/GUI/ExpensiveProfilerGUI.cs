using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SceneProfiler.Editor.GUI
{
    public class ExpensiveProfilerGUI : ProfilerGUI<ExpensiveObjectDetails>
    {
        public ExpensiveProfilerGUI(SceneProfiler profiler, Color defColor)
            : base(profiler, defColor)
        {
            InitializeColumns();
            InitializeColumnHeader();
        }

        protected override void InitializeColumns()
        {
            var columnDefinitions = new (string, float, bool)[]
            {
                ("GameObject", 200, false),
                ("Scale", 200, false),
                ("Scale Type", 80, false),
                ("Hierarchy Depth", 110, false),
                ("Component Count", 110, false)
            };

            columns = columnDefinitions.Select(def => CreateColumn(def.Item1, def.Item2, def.Item3)).ToArray();
        }

        protected override List<ExpensiveObjectDetails> GetProfilerItems()
        {
            return profiler.ActiveExpensiveObjects;
        }

        protected override int CompareItems(ExpensiveObjectDetails a, ExpensiveObjectDetails b, int columnIndex)
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;

            switch (columnIndex)
            {
                case 0: return string.Compare(a.gameObject.name, b.gameObject.name);
                case 1: return a.scale.sqrMagnitude.CompareTo(b.scale.sqrMagnitude);
                case 2: return string.Compare(a.scaleType, b.scaleType);
                case 3: return a.hierarchyDepth.CompareTo(b.hierarchyDepth);
                case 4: return a.componentCount.CompareTo(b.componentCount);
                default: return 0;
            }
        }

        public void ListExpensiveObjects()
        {
            if (buttonStyle == null || labelStyle == null)
            {
                InitializeStyles();
            }

            var headerRect = GUILayoutUtility.GetRect(0, columnHeader.height, GUILayout.ExpandWidth(true));
            columnHeader.OnGUI(headerRect, scrollPosition.x);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            int displayedObjects = 0;
            foreach (var details in profiler.ActiveExpensiveObjects)
            {
                if (displayedObjects >= profiler.currentObjectsInColumnCount) break;
                if (details.gameObject == null) continue;

                EditorGUILayout.BeginHorizontal();

                for (int visibleColumnIndex = 0; visibleColumnIndex < columnHeader.state.visibleColumns.Length; visibleColumnIndex++)
                {
                    int columnIndex = columnHeader.state.visibleColumns[visibleColumnIndex];
                    Rect cellRect = EditorGUILayout.GetControlRect(GUILayout.Width(columns[columnIndex].width - 4), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    cellRect.x += headerRect.x;

                    switch (columnIndex)
                    {
                        case 0:
                            DrawObjectButton(details, cellRect);
                            break;
                        case 1:
                            DrawScale(details, cellRect);
                            break;
                        case 2:
                            DrawScaleType(details, cellRect);
                            break;
                        case 3:
                            DrawHierarchyDepth(details, cellRect);
                            break;
                        case 4:
                            DrawComponentCount(details, cellRect);
                            break;
                    }
                }

                EditorGUILayout.EndHorizontal();
                displayedObjects++;
            }

            if (profiler.currentObjectsInColumnCount < profiler.ActiveExpensiveObjects.Count)
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

        private void DrawObjectButton(ExpensiveObjectDetails details, Rect cellRect)
        {
            if (UnityEngine.GUI.Button(cellRect, details.gameObject.name, buttonStyle))
            {
                Selection.activeObject = details.gameObject;
            }
        }

        private void DrawScale(ExpensiveObjectDetails details, Rect cellRect)
        {
            float componentWidth = cellRect.width / 3;

            Rect xLabelRect = new Rect(cellRect.x, cellRect.y, 15, EditorGUIUtility.singleLineHeight);
            Rect xRect = new Rect(cellRect.x + 15, cellRect.y, componentWidth - 20, EditorGUIUtility.singleLineHeight);

            Rect yLabelRect = new Rect(cellRect.x + componentWidth, cellRect.y, 15, EditorGUIUtility.singleLineHeight);
            Rect yRect = new Rect(cellRect.x + componentWidth + 15, cellRect.y, componentWidth - 20, EditorGUIUtility.singleLineHeight);

            Rect zLabelRect = new Rect(cellRect.x + 2 * componentWidth, cellRect.y, 15, EditorGUIUtility.singleLineHeight);
            Rect zRect = new Rect(cellRect.x + 2 * componentWidth + 15, cellRect.y, componentWidth - 20, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(xLabelRect, "X");
            details.scale.x = EditorGUI.FloatField(xRect, details.scale.x);

            EditorGUI.LabelField(yLabelRect, "Y");
            details.scale.y = EditorGUI.FloatField(yRect, details.scale.y);

            EditorGUI.LabelField(zLabelRect, "Z");
            details.scale.z = EditorGUI.FloatField(zRect, details.scale.z);
        }


        private void DrawScaleType(ExpensiveObjectDetails details, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, details.scaleType, labelStyle);
        }

        private void DrawHierarchyDepth(ExpensiveObjectDetails details, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, details.hierarchyDepth.ToString(), labelStyle);
        }

        private void DrawComponentCount(ExpensiveObjectDetails details, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, details.componentCount.ToString(), labelStyle);
        }
    }
}
