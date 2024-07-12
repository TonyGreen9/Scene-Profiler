using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SceneProfiler.Editor.GUI
{
    public class MissingProfilerGUI : ProfilerGUI<Missing>
    {
        public MissingProfilerGUI(SceneProfiler profiler, Color defColor)
            : base(profiler, defColor)
        {
            InitializeColumns();
            InitializeColumnHeader();
        }

        protected override void InitializeColumns()
        {
            var columnDefinitions = new (string, float, bool)[]
            {
                ("Name", 150, false),
                ("Type", 100, false),
            };

            columns = columnDefinitions.Select(def => CreateColumn(def.Item1, def.Item2, def.Item3)).ToArray();
        }

        protected override List<Missing> GetProfilerItems()
        {
            return profiler.MissingObjects;
        }

        protected override int CompareItems(Missing a, Missing b, int columnIndex)
        {
            switch (columnIndex)
            {
                case 0: return string.Compare(a.name, b.name);
                case 1: return string.Compare(a.type, b.type);
                default: return 0;
            }
        }

        public void ListMissing()
        {
            if (buttonStyle == null || labelStyle == null)
            {
                InitializeStyles();
            }

            var headerRect = GUILayoutUtility.GetRect(0, columnHeader.height, GUILayout.ExpandWidth(true));
            columnHeader.OnGUI(headerRect, scrollPosition.x);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (Missing dMissing in profiler.MissingObjects)
            {
                if (dMissing == null || dMissing.Object == null) continue;
            
                EditorGUILayout.BeginHorizontal();

                for (int visibleColumnIndex = 0; visibleColumnIndex < columnHeader.state.visibleColumns.Length; visibleColumnIndex++)
                {
                    int columnIndex = columnHeader.state.visibleColumns[visibleColumnIndex];
                    Rect cellRect = EditorGUILayout.GetControlRect(GUILayout.Width(columns[columnIndex].width - 3));
                    cellRect.x += headerRect.x;

                    Color originalColor = UnityEngine.GUI.color;

                    switch (columnIndex)
                    {
                        case 0:
                            DrawMissingName(dMissing, cellRect);
                            break;
                        case 1:
                            DrawMissingType(dMissing, cellRect);
                            break;
                    }

                    UnityEngine.GUI.color = originalColor;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawMissingName(Missing dMissing, Rect cellRect)
        {
            if (UnityEngine.GUI.Button(cellRect, dMissing.name, buttonStyle))
            {
                profiler.SelectObject(dMissing.Object, profiler.ctrlPressed);
            }
        }

        private void DrawMissingType(Missing dMissing, Rect cellRect)
        {
            Color originalColor = UnityEngine.GUI.color;

            switch (dMissing.type)
            {
                case "lod":
                    UnityEngine.GUI.color = UnityEngine.GUI.color;
                    break;
                case "mesh":
                    UnityEngine.GUI.color = UnityEngine.GUI.color;
                    break;
                case "sprite":
                    UnityEngine.GUI.color = UnityEngine.GUI.color;
                    break;
                case "material":
                    UnityEngine.GUI.color = UnityEngine.GUI.color;
                    break;
                case "missing script":
                    UnityEngine.GUI.color = Color.red;
                    break;
            }

            EditorGUI.LabelField(cellRect, dMissing.type, labelStyle);
            UnityEngine.GUI.color = originalColor;
        }
    }
}
