using UnityEditor;
using UnityEngine;

namespace SceneProfiler.Editor.GUI
{
    public class WarningsGUI
    {
        private SceneProfiler _profiler;

        public WarningsGUI(SceneProfiler profiler)
        {
            _profiler = profiler;
        }

        public void DrawWarnings(ref Vector2 scrollPosition)
        {
            if (_profiler.Warnings == null || _profiler.Warnings.Count == 0)
                return;

            float contentHeight = GetWarningsContentHeight();
            float windowHeight = EditorGUIUtility.currentViewWidth;
            float scrollViewHeight = Mathf.Min(contentHeight, windowHeight / 2);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(scrollViewHeight));
            GUILayout.BeginVertical("box");

            foreach (var warning in _profiler.Warnings)
            {
                EditorGUILayout.HelpBox(warning.Message, warning.Type);
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        public Texture GetWarningButtonIcon()
        {
            if (_profiler.Warnings == null || _profiler.Warnings.Count == 0)
            {
                return EditorGUIUtility.IconContent("console.infoicon.sml").image;
            }

            bool hasErrors = false;
            bool hasWarnings = false;
            bool hasInfo = false;

            foreach (var warning in _profiler.Warnings)
            {
                switch (warning.Type)
                {
                    case MessageType.Error:
                        hasErrors = true;
                        break;
                    case MessageType.Warning:
                        hasWarnings = true;
                        break;
                    case MessageType.Info:
                        hasInfo = true;
                        break;
                }
            }

            if (hasErrors)
            {
                return EditorGUIUtility.IconContent("console.erroricon.sml").image;
            }
            if (hasWarnings)
            {
                return EditorGUIUtility.IconContent("console.warnicon.sml").image;
            }
            if (hasInfo)
            {
                return EditorGUIUtility.IconContent("console.infoicon.sml").image;
            }

            return EditorGUIUtility.IconContent("console.infoicon.sml").image;
        }

        private float GetWarningsContentHeight()
        {
            int messageCount = _profiler.Warnings?.Count ?? 0;
            return messageCount * EditorGUIUtility.singleLineHeight * 2.5f;
        }
    }
}
