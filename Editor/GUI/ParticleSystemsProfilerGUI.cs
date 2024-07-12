using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SceneProfiler.Editor.GUI
{
    public class ParticleSystemsProfilerGUI : ProfilerGUI<ParticleSystemDetails>
    {
        public ParticleSystemsProfilerGUI(SceneProfiler profiler, Color defColor)
            : base(profiler, defColor)
        {
            InitializeColumns();
            InitializeColumnHeader();
        }
    
        protected override void InitializeColumns()
        {
            var columnDefinitions = new (string, float, bool)[]
            {
                ("Particle System", 120, false),
                ("Material", 120, false),
                ("Max Particles", 100, false),
                ("Active Particles", 100, false)
            };

            columns = columnDefinitions.Select(def => CreateColumn(def.Item1, def.Item2, def.Item3)).ToArray();
        }
    
        protected override List<ParticleSystemDetails> GetProfilerItems()
        {
            return profiler.ActiveParticleSystems;
        }

        public void ListParticleSystems()
        {
            if (buttonStyle == null || labelStyle == null)
            {
                InitializeStyles();
            }

            var headerRect = GUILayoutUtility.GetRect(0, columnHeader.height, GUILayout.ExpandWidth(true));
            columnHeader.OnGUI(headerRect, scrollPosition.x);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (ParticleSystemDetails psDetails in profiler.ActiveParticleSystems)
            {
                if (psDetails.particleSystem == null) continue;

                EditorGUILayout.BeginHorizontal();

                for (int i = 0; i < columns.Length; i++)
                {
                    Rect cellRect = EditorGUILayout.GetControlRect(GUILayout.Width(columns[i].width - 2));

                    switch (i)
                    {
                        case 0:
                            if (UnityEngine.GUI.Button(cellRect, psDetails.particleSystem.name, buttonStyle))
                            {
                                profiler.SelectObject(psDetails.particleSystem, profiler.ctrlPressed);
                            }
                            break;
                        case 1:
                            if (psDetails.material != null)
                            {
                                if (UnityEngine.GUI.Button(cellRect, psDetails.material.name, buttonStyle))
                                {
                                    profiler.SelectObject(psDetails.material, profiler.ctrlPressed);
                                }
                            }
                            else
                            {
                                EditorGUI.LabelField(cellRect, "Material: None");
                            }
                            break;
                        case 2:
                            EditorGUI.LabelField(cellRect, psDetails.maxParticles.ToString(), labelStyle);
                            break;
                        case 3:
                            EditorGUI.LabelField(cellRect, psDetails.activeParticles.ToString(), labelStyle);
                            break;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        protected override int CompareItems(ParticleSystemDetails a, ParticleSystemDetails b, int columnIndex)
        {
            switch (columnIndex)
            {
                case 0: return string.Compare(a.particleSystem.name, b.particleSystem.name);
                case 1: return string.Compare(a.material?.name ?? "", b.material?.name ?? "");
                case 2: return a.maxParticles.CompareTo(b.maxParticles);
                case 3: return a.activeParticles.CompareTo(b.activeParticles);
                default: return 0;
            }
        }
    }
}