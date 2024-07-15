using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SceneProfiler.Editor.GUI
{
    public class AudioClipsProfilerGUI : ProfilerGUI<AudioClipDetails>
    {
        private Func<float> _getRowHeight;

        public AudioClipsProfilerGUI(SceneProfiler profiler, Color defColor, Func<float> getRowHeight)
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
                ("Name", 150, false),
                ("Channels", 60, false),
                ("Frequency", 70, false),
                ("Length", 60, false),
                ("Load Type", 130, false),
                ("File Size", 80, false),
                ("GameObjects", 90, false),
                ("Asset Path", 300, true)
            };

            columns = columnDefinitions.Select(def => CreateColumn(def.Item1, def.Item2, def.Item3)).ToArray();
        }

        protected override List<AudioClipDetails> GetProfilerItems()
        {
            return profiler.ActiveClipDetails;
        }

        protected override int CompareItems(AudioClipDetails a, AudioClipDetails b, int columnIndex)
        {
            switch (columnIndex)
            {
                case 0: return 0; // Thumbnails are not comparable
                case 1: return string.Compare(a.clip.name, b.clip.name);
                case 2: return a.clip.channels.CompareTo(b.clip.channels);
                case 3: return a.clip.frequency.CompareTo(b.clip.frequency);
                case 4: return a.clip.length.CompareTo(b.clip.length);
                case 5: return a.clip.loadType.CompareTo(b.clip.loadType);
                case 6: return GetFileSize(a).CompareTo(GetFileSize(b));
                case 7: return a.FoundInAudioSources.Count.CompareTo(b.FoundInAudioSources.Count);
                case 8: return string.Compare(AssetDatabase.GetAssetPath(a.clip), AssetDatabase.GetAssetPath(b.clip));
                default: return 0;
            }
        }

        public void ListAudioClips()
        {
            if (buttonStyle == null || labelStyle == null)
            {
                InitializeStyles();
            }

            var headerRect = GUILayoutUtility.GetRect(0, columnHeader.height, GUILayout.ExpandWidth(true));
            columnHeader.OnGUI(headerRect, scrollPosition.x);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (AudioClipDetails aDetails in profiler.ActiveClipDetails)
            {
                if (aDetails.clip == null) continue;

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
                            DrawThumbnail(aDetails, cellRect);
                            break;
                        case 1:
                            DrawName(aDetails, cellRect);
                            break;
                        case 2:
                            DrawChannels(aDetails, cellRect);
                            break;
                        case 3:
                            DrawFrequency(aDetails, cellRect);
                            break;
                        case 4:
                            DrawLength(aDetails, cellRect);
                            break;
                        case 5:
                            DrawLoadType(aDetails, cellRect);
                            break;
                        case 6:
                            DrawFileSize(aDetails, cellRect);
                            break;
                        case 7:
                            DrawGameObjectsButton(aDetails, cellRect);
                            break;
                        case 8:
                            DrawAssetPath(aDetails, cellRect);
                            break;
                    }

                    UnityEngine.GUI.color = originalColor;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawThumbnail(AudioClipDetails aDetails, Rect cellRect)
        {
            var previewTexture = AssetPreview.GetAssetPreview(aDetails.clip);
    
            if (previewTexture == null)
            {
                previewTexture = AssetPreview.GetMiniThumbnail(aDetails.clip);
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

        private void DrawName(AudioClipDetails aDetails, Rect cellRect)
        {
            if (UnityEngine.GUI.Button(cellRect, aDetails.clip.name, buttonStyle))
            {
                profiler.SelectObject(aDetails.clip, profiler.ctrlPressed);
            }
        }

        private void DrawChannels(AudioClipDetails aDetails, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, aDetails.clip.channels.ToString(), labelStyle);
        }

        private void DrawFrequency(AudioClipDetails aDetails, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, aDetails.clip.frequency.ToString(), labelStyle);
        }

        private void DrawLength(AudioClipDetails aDetails, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, aDetails.clip.length.ToString("F2") + " s", labelStyle);
        }

        private void DrawLoadType(AudioClipDetails aDetails, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, aDetails.clip.loadType.ToString(), labelStyle);
        }

        private void DrawFileSize(AudioClipDetails aDetails, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, GetFileSize(aDetails).ToString("F2") + " MB", labelStyle);
        }

        private void DrawGameObjectsButton(AudioClipDetails aDetails, Rect cellRect)
        {
            if (UnityEngine.GUI.Button(cellRect, aDetails.FoundInAudioSources.Count + " GO", buttonStyle))
            {
                HashSet<Object> foundObjects = new HashSet<Object>();
                foreach (AudioSource source in aDetails.FoundInAudioSources)
                    foundObjects.Add(source.gameObject);
                profiler.SelectObjects(new List<Object>(foundObjects), profiler.ctrlPressed);
            }
        }

        private void DrawAssetPath(AudioClipDetails aDetails, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, AssetDatabase.GetAssetPath(aDetails.clip), labelStyle);
        }

        private float GetFileSize(AudioClipDetails aDetails)
        {
            var path = AssetDatabase.GetAssetPath(aDetails.clip);
            var fileInfo = new System.IO.FileInfo(path);
            return fileInfo.Length / (1024f * 1024f);
        }
    }
}
