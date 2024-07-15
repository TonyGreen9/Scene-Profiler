using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SceneProfiler.Editor.GUI
{
    public class TexturesProfilerGUI : ProfilerGUI<TextureDetails>
    {
        private Func<float> _getRowHeight;
        private Dictionary<Texture, Texture2D> texturePreviewCache = new Dictionary<Texture, Texture2D>();

        public TexturesProfilerGUI(SceneProfiler profiler, Color defColor, Func<float> getRowHeight)
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
                ("Texture", 150, false),
                ("Resolution", 80, false),
                ("Size", 60, false),
                ("Mipmap", 50, false),
                ("Format", 100, false),
                ("Materials", 60, false),
                ("GameObjects", 100, false),
                ("Path", 500, true)
            };

            columns = columnDefinitions.Select(def => CreateColumn(def.Item1, def.Item2, def.Item3)).ToArray();
        }

        protected override List<TextureDetails> GetProfilerItems()
        {
            return profiler.ActiveTextures;
        }

        protected override int CompareItems(TextureDetails a, TextureDetails b, int columnIndex)
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;

            switch (columnIndex)
            {
                case 0: return 0;
                case 1: return string.Compare(a.texture?.name, b.texture?.name);
                case 2: return (a.texture?.width * a.texture?.height ?? 0).CompareTo(b.texture?.width * b.texture?.height ?? 0); // Resolution
                case 3: return a.memSizeKB.CompareTo(b.memSizeKB); // Size
                case 4: return a.mipMapCount.CompareTo(b.mipMapCount); // Mipmap
                case 5: return string.Compare(a.format.ToString(), b.format.ToString()); // Format
                case 6: return a.FoundInMaterials.Count.CompareTo(b.FoundInMaterials.Count);
                case 7:
                    int aCount = (a.FoundInRenderers?.Count ?? 0) + (a.FoundInAnimators?.Count ?? 0) +
                                 (a.FoundInGraphics?.Count ?? 0) + (a.FoundInButtons?.Count ?? 0) +
                                 (a.FoundInScripts?.Count ?? 0);
                    int bCount = (b.FoundInRenderers?.Count ?? 0) + (b.FoundInAnimators?.Count ?? 0) +
                                 (b.FoundInGraphics?.Count ?? 0) + (b.FoundInButtons?.Count ?? 0) +
                                 (b.FoundInScripts?.Count ?? 0);
                    return aCount.CompareTo(bCount);
                case 8: return string.Compare(AssetDatabase.GetAssetPath(a.texture), AssetDatabase.GetAssetPath(b.texture));
                default: return 0;
            }
        }

        public void ListTextures()
        {
            if (buttonStyle == null || labelStyle == null)
            {
                InitializeStyles();
            }

            var headerRect = GUILayoutUtility.GetRect(0, columnHeader.height, GUILayout.ExpandWidth(true));
            columnHeader.OnGUI(headerRect, scrollPosition.x);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            int displayedTextures = 0;
            foreach (TextureDetails tDetails in profiler.ActiveTextures)
            {
                if (displayedTextures >= profiler.currentObjectsInColumnCount) break;
                if (tDetails.texture == null) continue;

                EditorGUILayout.BeginHorizontal();

                for (int visibleColumnIndex = 0; visibleColumnIndex < columnHeader.state.visibleColumns.Length; visibleColumnIndex++)
                {
                    int columnIndex = columnHeader.state.visibleColumns[visibleColumnIndex];
                    Rect cellRect = EditorGUILayout.GetControlRect(GUILayout.Width(columns[columnIndex].width - 4), GUILayout.Height(_getRowHeight()));
                    cellRect.x += headerRect.x;

                    switch (columnIndex)
                    {
                        case 0:
                            DrawThumbnail(tDetails, cellRect);
                            break;
                        case 1:
                            DrawTextureName(tDetails, cellRect);
                            break;
                        case 2:
                            DrawResolution(tDetails, cellRect);
                            break;
                        case 3:
                            DrawTextureSize(tDetails, cellRect);
                            break;
                        case 4:
                            DrawMipmap(tDetails, cellRect);
                            break;
                        case 5:
                            DrawTextureFormat(tDetails, cellRect);
                            break;
                        case 6:
                            DrawMaterialsButton(tDetails, cellRect);
                            break;
                        case 7:
                            DrawGameObjectsButton(tDetails, cellRect);
                            break;
                        case 8:
                            DrawTexturePath(tDetails, cellRect);
                            break;
                    }
                }

                EditorGUILayout.EndHorizontal();
                displayedTextures++;
            }

            if (profiler.currentObjectsInColumnCount < profiler.ActiveTextures.Count)
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

        private void DrawThumbnail(TextureDetails tDetails, Rect cellRect)
        {
            if (!texturePreviewCache.TryGetValue(tDetails.texture, out var previewTexture) || previewTexture == null)
            {
                if (tDetails.texture.GetType() == typeof(Texture2DArray) || tDetails.texture.GetType() == typeof(Cubemap))
                {
                    previewTexture = AssetPreview.GetMiniThumbnail(tDetails.texture);
                }
                else
                {
                    previewTexture = AssetPreview.GetAssetPreview(tDetails.texture);
                }
                
                if (previewTexture == null)
                {
                    previewTexture = AssetPreview.GetMiniThumbnail(tDetails.texture);
                }

                texturePreviewCache[tDetails.texture] = previewTexture;
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

        private void DrawTextureName(TextureDetails tDetails, Rect cellRect)
        {
            if (UnityEngine.GUI.Button(cellRect, tDetails.texture.name, buttonStyle))
            {
                profiler.SelectObject(tDetails.texture, profiler.ctrlPressed);
            }
        }

        private void DrawResolution(TextureDetails tDetails, Rect cellRect)
        {
            string resolution = $"{tDetails.texture.width}x{tDetails.texture.height}";
            if (tDetails.isCubeMap) resolution += "x6";
            if (tDetails.texture.GetType() == typeof(Texture2DArray))
                resolution += "[]\n" + ((Texture2DArray)tDetails.texture).depth + " depths";
            EditorGUI.LabelField(cellRect, resolution, labelStyle);
        }

        private void DrawTextureSize(TextureDetails tDetails, Rect cellRect)
        {
            double roundedSizeKB = Math.Round(tDetails.memSizeKB, 1);
            string size = profiler.FormatSizeString((float)roundedSizeKB);
            EditorGUI.LabelField(cellRect, size, labelStyle);
        }

        private void DrawMipmap(TextureDetails tDetails, Rect cellRect)
        {
            string mipmap = tDetails.mipMapCount.ToString();
            EditorGUI.LabelField(cellRect, mipmap, labelStyle);
        }

        private void DrawTextureFormat(TextureDetails tDetails, Rect cellRect)
        {
            string format = tDetails.format.ToString();
            EditorGUI.LabelField(cellRect, format, labelStyle);
        }

        private void DrawMaterialsButton(TextureDetails tDetails, Rect cellRect)
        {
            if (UnityEngine.GUI.Button(cellRect, tDetails.FoundInMaterials.Count + " Mat", buttonStyle))
            {
                profiler.SelectObjects(tDetails.FoundInMaterials, profiler.ctrlPressed);
            }
        }

        private void DrawGameObjectsButton(TextureDetails tDetails, Rect cellRect)
        {
            HashSet<Object> FoundObjects = new HashSet<Object>();
            foreach (Renderer renderer in tDetails.FoundInRenderers)
            {
                if (renderer != null) FoundObjects.Add(renderer.gameObject);
            }
            foreach (Animator animator in tDetails.FoundInAnimators)
            {
                if (animator != null) FoundObjects.Add(animator.gameObject);
            }
            foreach (Graphic graphic in tDetails.FoundInGraphics)
            {
                if (graphic != null) FoundObjects.Add(graphic.gameObject);
            }
            foreach (Button button in tDetails.FoundInButtons)
            {
                if (button != null) FoundObjects.Add(button.gameObject);
            }
            foreach (MonoBehaviour script in tDetails.FoundInScripts)
            {
                if (script != null) FoundObjects.Add(script.gameObject);
            }

            if (UnityEngine.GUI.Button(cellRect, FoundObjects.Count + " GO", buttonStyle))
            {
                profiler.SelectObjects(new List<Object>(FoundObjects), profiler.ctrlPressed);
            }
        }

        private void DrawTexturePath(TextureDetails tDetails, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, AssetDatabase.GetAssetPath(tDetails.texture), labelStyle);
        }
    }
}
