using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SceneProfiler.Editor.GUI
{
    public class LightsProfilerGUI : ProfilerGUI<LightDetails>
    {

        public LightsProfilerGUI(SceneProfiler profiler, Color defColor)
            : base(profiler, defColor)
        {
            InitializeColumns();
            InitializeColumnHeader();
        }

        protected override void InitializeColumns()
        {
            var columnDefinitions = new (string, float, bool)[]
            {
                ("Light", 150, false),
                ("Enabled", 100, false),
                ("Active", 100, false),
                ("Shadows", 100, false)
            };

            columns = columnDefinitions.Select(def => CreateColumn(def.Item1, def.Item2, def.Item3)).ToArray();
        }
    
        protected override List<LightDetails> GetProfilerItems()
        {
            return profiler.ActiveLights;
        }

        protected override int CompareItems(LightDetails a, LightDetails b, int columnIndex)
        {
            switch (columnIndex)
            {
                case 0: return string.Compare(a.light.name, b.light.name);
                case 1: return a.isEnabled.CompareTo(b.isEnabled);
                case 2: return a.isActive.CompareTo(b.isActive);
                case 3: return string.Compare(a.shadowType.ToString(), b.shadowType.ToString());
                default: return 0;
            }
        }

        public void ListLights()
        {
            if (buttonStyle == null || labelStyle == null)
            {
                InitializeStyles();
            }

            var headerRect = GUILayoutUtility.GetRect(0, columnHeader.height, GUILayout.ExpandWidth(true));
            columnHeader.OnGUI(headerRect, scrollPosition.x);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (LightDetails lightDetails in profiler.ActiveLights)
            {
                if (lightDetails.light == null) continue;

                EditorGUILayout.BeginHorizontal();

                for (int i = 0; i < columns.Length; i++)
                {
                    Rect cellRect = EditorGUILayout.GetControlRect(GUILayout.Width(columns[i].width - 2));

                    switch (i)
                    {
                        case 0:
                            if (UnityEngine.GUI.Button(cellRect, lightDetails.light.name, buttonStyle))
                            {
                                profiler.SelectObject(lightDetails.light, profiler.ctrlPressed);
                            }
                            break;
                        case 1:
                            EditorGUI.LabelField(cellRect, lightDetails.isEnabled ? "Yes" : "No", labelStyle);
                            break;
                        case 2:
                            EditorGUI.LabelField(cellRect, lightDetails.isActive ? "Yes" : "No", labelStyle);
                            break;
                        case 3:
                            EditorGUI.LabelField(cellRect, lightDetails.shadowType.ToString(), labelStyle);
                            break;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
