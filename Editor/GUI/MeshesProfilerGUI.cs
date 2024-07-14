using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SceneProfiler.Editor.GUI
{
    public class MeshesProfilerGUI : ProfilerGUI<MeshDetails>
    {
        public MeshesProfilerGUI(SceneProfiler profiler, Color defColor)
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
                ("Vertex Count", 100, false),
                ("GameObjects", 120, false),
                ("Skinned Meshes", 120, false),
                ("Static Batching", 140, false),
                ("Asset Path", 300, true)
            };

            columns = columnDefinitions.Select(def => CreateColumn(def.Item1, def.Item2, def.Item3)).ToArray();
        }

        protected override List<MeshDetails> GetProfilerItems()
        {
            return profiler.ActiveMeshDetails;
        }

        protected override int CompareItems(MeshDetails a, MeshDetails b, int columnIndex)
        {
            switch (columnIndex)
            {
                case 0: return string.Compare(a.mesh.name, b.mesh.name);
                case 1: return a.mesh.vertexCount.CompareTo(b.mesh.vertexCount);
                case 2: return a.FoundInMeshFilters.Count.CompareTo(b.FoundInMeshFilters.Count);
                case 3: return a.FoundInSkinnedMeshRenderer.Count.CompareTo(b.FoundInSkinnedMeshRenderer.Count);
                case 4: return a.StaticBatchingEnabled.Count.CompareTo(b.StaticBatchingEnabled.Count);
                case 5: return string.Compare(AssetDatabase.GetAssetPath(a.mesh), AssetDatabase.GetAssetPath(b.mesh));
                default: return 0;
            }
        }

        public void ListMeshes()
        {
            if (buttonStyle == null || labelStyle == null)
            {
                InitializeStyles();
            }

            var headerRect = GUILayoutUtility.GetRect(0, columnHeader.height, GUILayout.ExpandWidth(true));
            columnHeader.OnGUI(headerRect, scrollPosition.x);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            int displayedMeshes = 0;
            foreach (MeshDetails tDetails in profiler.ActiveMeshDetails)
            {
                if (displayedMeshes >= profiler.currentObjectsInColumnCount) break;
                if (tDetails.mesh == null) continue;

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
                            DrawMeshName(tDetails, cellRect);
                            break;
                        case 1:
                            DrawVertexCount(tDetails, cellRect);
                            break;
                        case 2:
                            DrawGameObjectsButton(tDetails, cellRect);
                            break;
                        case 3:
                            DrawSkinnedMeshButton(tDetails, cellRect);
                            break;
                        case 4:
                            DrawStaticBatchingButton(tDetails, cellRect);
                            break;
                        case 5:
                            DrawAssetPath(tDetails, cellRect);
                            break;
                    }

                    UnityEngine.GUI.color = originalColor;
                }

                EditorGUILayout.EndHorizontal();
                displayedMeshes++;
            }

            if (profiler.currentObjectsInColumnCount < profiler.ActiveMeshDetails.Count)
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

        private void DrawMeshName(MeshDetails tDetails, Rect cellRect)
        {
            if (string.IsNullOrEmpty(tDetails.mesh.name))
                tDetails.mesh.name = tDetails.FoundInMeshFilters[0].gameObject.name;

            if (UnityEngine.GUI.Button(cellRect, tDetails.mesh.name, buttonStyle))
            {
                profiler.SelectObject(tDetails.mesh, profiler.ctrlPressed);
            }
        }

        private void DrawVertexCount(MeshDetails tDetails, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, tDetails.mesh.vertexCount.ToString(), labelStyle);
        }

        private void DrawGameObjectsButton(MeshDetails tDetails, Rect cellRect)
        {
            if (UnityEngine.GUI.Button(cellRect, tDetails.FoundInMeshFilters.Count + " GO", buttonStyle))
            {
                List<Object> foundObjects = new List<Object>();
                foreach (MeshFilter meshFilter in tDetails.FoundInMeshFilters)
                    foundObjects.Add(meshFilter.gameObject);
                profiler.SelectObjects(foundObjects, profiler.ctrlPressed);
            }
        }

        private void DrawSkinnedMeshButton(MeshDetails tDetails, Rect cellRect)
        {
            if (tDetails.FoundInSkinnedMeshRenderer.Count > 0)
            {
                if (UnityEngine.GUI.Button(cellRect, tDetails.FoundInSkinnedMeshRenderer.Count + " skinned mesh GO", buttonStyle))
                {
                    List<Object> foundObjects = new List<Object>();
                    foreach (SkinnedMeshRenderer skinnedMeshRenderer in tDetails.FoundInSkinnedMeshRenderer)
                        foundObjects.Add(skinnedMeshRenderer.gameObject);
                    profiler.SelectObjects(foundObjects, profiler.ctrlPressed);
                }
            }
            else
            {
                UnityEngine.GUI.color = new Color(defColor.r, defColor.g, defColor.b, 0.5f);
                EditorGUI.LabelField(cellRect, "0 skinned mesh", labelStyle);
                UnityEngine.GUI.color = defColor;
            }
        }

        private void DrawStaticBatchingButton(MeshDetails tDetails, Rect cellRect)
        {
            if (tDetails.StaticBatchingEnabled.Count > 0)
            {
                if (UnityEngine.GUI.Button(cellRect, tDetails.StaticBatchingEnabled.Count + " Static Batching", buttonStyle))
                {
                    List<Object> foundObjects = new List<Object>();
                    foreach (var obj in tDetails.StaticBatchingEnabled)
                        foundObjects.Add(obj);
                    profiler.SelectObjects(foundObjects, profiler.ctrlPressed);
                }
            }
            else
            {
                UnityEngine.GUI.color = new Color(defColor.r, defColor.g, defColor.b, 0.8f);
                EditorGUI.LabelField(cellRect, "0 static batching", labelStyle);
                UnityEngine.GUI.color = defColor;
            }
        }

        private void DrawAssetPath(MeshDetails tDetails, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, AssetDatabase.GetAssetPath(tDetails.mesh), labelStyle);
        }
    }
}
