using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SceneProfiler.Editor.GUI
{
    public class PhysicsProfilerGUI : ProfilerGUI<PhysicsObjectDetails>
    {
        public PhysicsProfilerGUI(SceneProfiler profiler, Color defColor)
            : base(profiler, defColor)
        {
            InitializeColumns();
            InitializeColumnHeader();
        }

        protected override void InitializeColumns()
        {
            var columnDefinitions = new (string, float, bool)[]
            {
                ("Rigidbody", 150, false),
                ("IsActive", 80, false),
                ("Mass", 60, false),
                ("Drag", 60, false),
                ("Angular Drag", 100, false),
                ("Kinematic", 100, false),
                ("Interpolation", 100, false),
                ("Collision Detection", 150, false),
                ("Collider Type", 100, false)
            };

            columns = columnDefinitions.Select(def => CreateColumn(def.Item1, def.Item2, def.Item3)).ToArray();
        }

        protected override List<PhysicsObjectDetails> GetProfilerItems()
        {
            return profiler.ActivePhysicsObjects;
        }

        protected override int CompareItems(PhysicsObjectDetails a, PhysicsObjectDetails b, int columnIndex)
        {
            switch (columnIndex)
            {
                case 0: return string.Compare(a.rigidbody.name, b.rigidbody.name);
                case 1: return CompareState(a, b);
                case 2: return a.mass.CompareTo(b.mass);
                case 3: return a.drag.CompareTo(b.drag);
                case 4: return a.angularDrag.CompareTo(b.angularDrag);
                case 5: return a.isKinematic.CompareTo(b.isKinematic);
                case 6: return a.interpolation.CompareTo(b.interpolation);
                case 7: return a.collisionDetectionMode.CompareTo(b.collisionDetectionMode);
                case 8: return string.Compare(a.colliderType, b.colliderType);
                default: return 0;
            }
        }

        private int CompareState(PhysicsObjectDetails a, PhysicsObjectDetails b)
        {
            int aState = (a.isActive ? 1 : 0) + (!a.isKinematic ? 1 : 0);
            int bState = (b.isActive ? 1 : 0) + (!b.isKinematic ? 1 : 0);
            return aState.CompareTo(bState);
        }

        public void ListPhysicsObjects()
        {
            if (buttonStyle == null || labelStyle == null)
            {
                InitializeStyles();
            }

            var headerRect = GUILayoutUtility.GetRect(0, columnHeader.height, GUILayout.ExpandWidth(true));
            columnHeader.OnGUI(headerRect, scrollPosition.x);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (PhysicsObjectDetails obj in profiler.ActivePhysicsObjects)
            {
            
                if (obj == null || obj.gameObject == null) continue;
            
                EditorGUILayout.BeginHorizontal();

                for (int visibleColumnIndex = 0; visibleColumnIndex < columnHeader.state.visibleColumns.Length; visibleColumnIndex++)
                {
                    int columnIndex = columnHeader.state.visibleColumns[visibleColumnIndex];
                    Rect cellRect = EditorGUILayout.GetControlRect(GUILayout.Width(columns[columnIndex].width - 3));
                    cellRect.x += headerRect.x;

                    switch (columnIndex)
                    {
                        case 0:
                            DrawRigidbodyName(obj, cellRect);
                            break;
                        case 1:
                            DrawIsActive(obj, cellRect);
                            break;
                        case 2:
                            DrawMass(obj, cellRect);
                            break;
                        case 3:
                            DrawDrag(obj, cellRect);
                            break;
                        case 4:
                            DrawAngularDrag(obj, cellRect);
                            break;
                        case 5:
                            DrawKinematic(obj, cellRect);
                            break;
                        case 6:
                            DrawInterpolation(obj, cellRect);
                            break;
                        case 7:
                            DrawCollisionDetection(obj, cellRect);
                            break;
                        case 8:
                            DrawColliderType(obj, cellRect);
                            break;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawRigidbodyName(PhysicsObjectDetails obj, Rect cellRect)
        {
            if (UnityEngine.GUI.Button(cellRect, obj.rigidbody.name, buttonStyle))
            {
                profiler.SelectObject(obj.gameObject, profiler.ctrlPressed);
            }
        }

        private void DrawIsActive(PhysicsObjectDetails obj, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, obj.isActive ? "Enabled" : "Disabled", labelStyle);
        }

        private void DrawMass(PhysicsObjectDetails obj, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, obj.mass.ToString(), labelStyle);
        }

        private void DrawDrag(PhysicsObjectDetails obj, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, obj.drag.ToString(), labelStyle);
        }

        private void DrawAngularDrag(PhysicsObjectDetails obj, Rect cellRect)
        {
            EditorGUI.LabelField(cellRect, obj.angularDrag.ToString(), labelStyle);
        }

        private void DrawKinematic(PhysicsObjectDetails obj, Rect cellRect)
        {
            var color = obj.isKinematic ? Color.white : new Color(1f, 0.5f, 0f);
            var originalColor = UnityEngine.GUI.color;
            UnityEngine.GUI.color = color;
            EditorGUI.LabelField(cellRect, obj.isKinematic ? "Is Kinematic" : "Not Kinematic", labelStyle);
            UnityEngine.GUI.color = originalColor;
        }


        private void DrawInterpolation(PhysicsObjectDetails obj, Rect cellRect)
        {
            var color = GetInterpolationColor(obj.rigidbody);
            var originalColor = UnityEngine.GUI.color;
            UnityEngine.GUI.color = color;
            EditorGUI.LabelField(cellRect, obj.interpolation.ToString(), labelStyle);
            UnityEngine.GUI.color = originalColor;
        }

        private void DrawCollisionDetection(PhysicsObjectDetails obj, Rect cellRect)
        {
            var color = GetCollisionDetectionColor(obj.rigidbody);
            var originalColor = UnityEngine.GUI.color;
            UnityEngine.GUI.color = color;
            EditorGUI.LabelField(cellRect, obj.collisionDetectionMode.ToString(), labelStyle);
            UnityEngine.GUI.color = originalColor;
        }

        private void DrawColliderType(PhysicsObjectDetails obj, Rect cellRect)
        {
            var color = GetColliderColor(obj.rigidbody);
            var originalColor = UnityEngine.GUI.color;
            UnityEngine.GUI.color = color;
            EditorGUI.LabelField(cellRect, obj.colliderType, labelStyle);
            UnityEngine.GUI.color = originalColor;
        }

        private Color GetInterpolationColor(Rigidbody rb)
        {
            switch (rb.interpolation)
            {
                case RigidbodyInterpolation.None:
                    return Color.white;
                case RigidbodyInterpolation.Interpolate:
                    return new Color(1f, 0.5f, 0f);
                case RigidbodyInterpolation.Extrapolate:
                    return Color.red;
                default:
                    return Color.white;
            }
        }

        private Color GetCollisionDetectionColor(Rigidbody rb)
        {
            switch (rb.collisionDetectionMode)
            {
                case CollisionDetectionMode.Discrete:
                    return Color.white;
                case CollisionDetectionMode.Continuous:
                    return Color.yellow;
                case CollisionDetectionMode.ContinuousSpeculative:
                    return new Color(1f, 0.65f, 0f);
                case CollisionDetectionMode.ContinuousDynamic:
                    return Color.red;
                default:
                    return Color.white;
            }
        }

        private Color GetColliderColor(Rigidbody rb)
        {
            Collider collider = rb.GetComponent<Collider>();
            if (collider is BoxCollider || collider is SphereCollider || collider is CapsuleCollider)
            {
                return Color.white;
            }
            else if (collider is MeshCollider)
            {
                return Color.red;
            }
            else
            {
                return new Color(1f, 0.5f, 0f);
            }
        }
    }
}