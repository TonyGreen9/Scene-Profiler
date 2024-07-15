using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SceneProfiler.Editor.GUI
{
    public class MaterialsProfilerGUI : ProfilerGUI<MaterialDetails>
    {
        private Func<float> _getRowHeight;
        private Dictionary<Material, Texture2D> materialPreviewCache = new Dictionary<Material, Texture2D>();

        public MaterialsProfilerGUI(SceneProfiler profiler, Color defColor, Func<float> getRowHeight)
            : base(profiler, defColor)
        {
            _getRowHeight = getRowHeight;
            InitializeColumns();
            InitializeColumnHeader();
        }

        protected override void InitializeColumns()
        {
            var columnDefinitions = new (string, float, bool)[]
            {
                ("Thumbnail", 70, false),
                ("Material", 150, false),
                ("Shader", 250, false),
                ("GameObjects", 100, false),
                ("Render Queue", 100, false),
                ("Asset Path", 350, true)
            };

            columns = columnDefinitions.Select(def => CreateColumn(def.Item1, def.Item2, def.Item3)).ToArray();
        }

        protected override List<MaterialDetails> GetProfilerItems()
        {
            return profiler.ActiveMaterials;
        }

        protected override int CompareItems(MaterialDetails a, MaterialDetails b, int columnIndex)
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;

            switch (columnIndex)
            {
                case 0: return 0;
                case 1: return string.Compare(a.material?.name, b.material?.name);
                case 2: return string.Compare(a.material?.shader?.name, b.material?.shader?.name);
                case 3:
                    int aCount = (a.FoundInRenderers?.Count ?? 0) + (a.FoundInGraphics?.Count ?? 0);
                    int bCount = (b.FoundInRenderers?.Count ?? 0) + (b.FoundInGraphics?.Count ?? 0);
                    return aCount.CompareTo(bCount);
                case 4: return a.material?.renderQueue.CompareTo(b.material?.renderQueue) ?? 0;
                case 5: return string.Compare(AssetDatabase.GetAssetPath(a.material), AssetDatabase.GetAssetPath(b.material));
                default: return 0;
            }
        }

        public void ListMaterials()
        {
            if (buttonStyle == null || labelStyle == null)
            {
                InitializeStyles();
            }

            var headerRect = GUILayoutUtility.GetRect(0, columnHeader.height, GUILayout.ExpandWidth(true));
            columnHeader.OnGUI(headerRect, scrollPosition.x);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            int displayedMaterials = 0;
            foreach (MaterialDetails tDetails in profiler.ActiveMaterials)
            {
                if (displayedMaterials >= profiler.currentObjectsInColumnCount) break;
                if (tDetails.material == null) continue;

                EditorGUILayout.BeginHorizontal();

                for (int visibleColumnIndex = 0; visibleColumnIndex < columnHeader.state.visibleColumns.Length; visibleColumnIndex++)
                {
                    int columnIndex = columnHeader.state.visibleColumns[visibleColumnIndex];
                    Rect cellRect = EditorGUILayout.GetControlRect(GUILayout.Width(columns[columnIndex].width - 3), GUILayout.Height(_getRowHeight()));
                    cellRect.x += headerRect.x;

                    Color originalColor = UnityEngine.GUI.color;

                    switch (columnIndex)
                    {
                        case 0:
                            DrawThumbnail(tDetails, cellRect);
                            break;
                        case 1:
                            DrawMaterialName(tDetails, cellRect);
                            break;
                        case 2:
                            DrawShader(tDetails, cellRect);
                            break;
                        case 3:
                            DrawGameObjectsButton(tDetails, cellRect);
                            break;
                        case 4:
                            DrawRenderQueue(tDetails, cellRect);
                            break;
                        case 5:
                            DrawAssetPath(tDetails, cellRect);
                            break;
                    }

                    UnityEngine.GUI.color = originalColor;
                }

                EditorGUILayout.EndHorizontal();
                displayedMaterials++;
            }

            if (profiler.currentObjectsInColumnCount < profiler.ActiveMaterials.Count)
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

        private void DrawThumbnail(MaterialDetails tDetails, Rect cellRect)
        {
            if (!materialPreviewCache.TryGetValue(tDetails.material, out var previewTexture) || previewTexture == null)
            {
                previewTexture = AssetPreview.GetAssetPreview(tDetails.material);
                
                if (previewTexture == null)
                {
                    previewTexture = AssetPreview.GetMiniThumbnail(tDetails.material);
                }
                
                materialPreviewCache[tDetails.material] = previewTexture;
            }

            if (previewTexture != null)
            {
                UnityEngine.GUI.DrawTexture(cellRect, previewTexture, ScaleMode.ScaleToFit);
            }
            else
            {
                EditorGUI.LabelField(cellRect, "No Preview", labelStyle);
            }
        }

        private void DrawMaterialName(MaterialDetails tDetails, Rect cellRect)
        {
            if (tDetails.instance)
                UnityEngine.GUI.color = new Color(0.8f, 0.8f, defColor.b, 1.0f);
            if (tDetails.isgui)
                UnityEngine.GUI.color = new Color(defColor.r, 0.95f, 0.8f, 1.0f);
            if (tDetails.isSky)
                UnityEngine.GUI.color = new Color(0.9f, defColor.g, defColor.b, 1.0f);

            if (UnityEngine.GUI.Button(cellRect, tDetails.material.name, buttonStyle))
            {
                profiler.SelectObject(tDetails.material, profiler.ctrlPressed);
            }

            UnityEngine.GUI.color = defColor;
        }

        private void DrawShader(MaterialDetails tDetails, Rect cellRect)
        {
            string shaderLabel = tDetails.material.shader != null ? tDetails.material.shader.name : "no shader";
            EditorGUI.LabelField(cellRect, shaderLabel, labelStyle);
        }

        private void DrawGameObjectsButton(MaterialDetails tDetails, Rect cellRect)
        {
            if (UnityEngine.GUI.Button(cellRect, (tDetails.FoundInRenderers.Count + tDetails.FoundInGraphics.Count) + " GO", buttonStyle))
            {
                List<Object> foundObjects = new List<Object>();
                foreach (Renderer renderer in tDetails.FoundInRenderers)
                    foundObjects.Add(renderer.gameObject);
                foreach (Graphic graphic in tDetails.FoundInGraphics)
                    foundObjects.Add(graphic.gameObject);
                profiler.SelectObjects(foundObjects, profiler.ctrlPressed);
            }
        }

        private void DrawRenderQueue(MaterialDetails tDetails, Rect cellRect)
        {
            GUIStyle customStyle = new GUIStyle(EditorStyles.numberField)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                normal = { textColor = Color.gray },
                focused = { textColor = Color.white }
            };

            EditorGUI.BeginChangeCheck();
            int renderQueue = EditorGUI.DelayedIntField(cellRect, tDetails.material.renderQueue, customStyle);
            if (EditorGUI.EndChangeCheck())
            {
                tDetails.material.renderQueue = renderQueue;
                profiler.ActiveMaterials.Sort((a, b) => CompareItems(a, b, 4));
            }
        }

        private void DrawAssetPath(MaterialDetails tDetails, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, AssetDatabase.GetAssetPath(tDetails.material), labelStyle);
        }
    }
}
