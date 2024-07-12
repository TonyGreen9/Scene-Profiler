using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SceneProfiler.Editor.GUI
{
    public abstract class ProfilerGUI<T>
    {
        protected SceneProfiler profiler;
        protected Vector2 scrollPosition;
        protected GUIStyle buttonStyle;
        protected GUIStyle labelStyle;
        protected MultiColumnHeader columnHeader;
        protected MultiColumnHeaderState.Column[] columns;
        protected Color defColor;
        protected Vector2 horizontalScrollPosition;

        protected ProfilerGUI(SceneProfiler profiler, Color defColor)
        {
            this.profiler = profiler;
            this.defColor = defColor;
        }
    
        protected abstract void InitializeColumns();
    
        protected abstract List<T> GetProfilerItems();
    
        protected void InitializeStyles()
        {
            buttonStyle = new GUIStyle(UnityEngine.GUI.skin.button)
            {
                padding = new RectOffset(2, 2, 2, 2),
                margin = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter
            };

            labelStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft
            };
        }
    
        protected abstract int CompareItems(T a, T b, int columnIndex);
    
        protected MultiColumnHeaderState.Column CreateColumn(string headerText, float width, bool autoResize)
        {
            return new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent(headerText),
                width = width,
                minWidth = width / 4,
                maxWidth = width * 2,
                headerTextAlignment = TextAlignment.Left,
                sortingArrowAlignment = TextAlignment.Right,
                autoResize = autoResize,
                allowToggleVisibility = true
            };
        }
    
        protected void InitializeColumnHeader()
        {
            columnHeader = new MultiColumnHeader(new MultiColumnHeaderState(columns))
            {
                height = 20,
                canSort = true
            };

            columnHeader.sortingChanged += OnSortingChanged;
            columnHeader.visibleColumnsChanged += OnVisibleColumnsChanged;
        }
    
        protected void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            var sortColumnIndex = multiColumnHeader.sortedColumnIndex;
            var sortAscending = multiColumnHeader.IsSortedAscending(sortColumnIndex);
            var items = GetProfilerItems();
            items.Sort((a, b) => CompareItems(a, b, sortColumnIndex));
            if (!sortAscending)
            {
                items.Reverse();
            }
        }
    
        protected void OnVisibleColumnsChanged(MultiColumnHeader multiColumnHeader)
        {
            SceneView.RepaintAll();
        }
    
    
        public static Color AdjustBrightness(Color color, float adjustment)
        {
            float r = Mathf.Clamp01(color.r + adjustment);
            float g = Mathf.Clamp01(color.g + adjustment);
            float b = Mathf.Clamp01(color.b + adjustment);
            return new Color(r, g, b, color.a);
        }
    }
}