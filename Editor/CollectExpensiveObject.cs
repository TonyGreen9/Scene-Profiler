using System.Collections.Generic;
using UnityEngine;

namespace SceneProfiler.Editor
{
    public class CollectExpensiveObject
    {
        private SceneProfiler _sceneProfiler;
        private const int HierarchyDepthThreshold = 7;
        private const int ComponentCountThreshold = 5;

        public CollectExpensiveObject(SceneProfiler sceneProfiler)
        {
            _sceneProfiler = sceneProfiler;
        }

        public void CollectData()
        {
            _sceneProfiler.ActiveExpensiveObjects.Clear();
            List<GameObject> allObjects = _sceneProfiler.FindAllGameObjects();

            foreach (GameObject obj in allObjects)
            {
                if (obj == null) continue;
                
                if (obj.transform.localScale != Vector3.one)
                {
                    AddExpensiveObjectDetails(obj, "Scale");
                }
            }

            foreach (GameObject obj in allObjects)
            {
                if (obj == null) continue;
                
                if (GetHierarchyDepth(obj.transform) > HierarchyDepthThreshold)
                {
                    AddExpensiveObjectDetails(obj, "Hierarchy");
                }
            }

            foreach (GameObject obj in allObjects)
            {
                if (obj == null) continue;
                
                if (obj.GetComponents<Component>().Length > ComponentCountThreshold)
                {
                    AddExpensiveObjectDetails(obj, "Components");
                }
            }
        }

        private void AddExpensiveObjectDetails(GameObject obj, string type)
        {
            ExpensiveObjectDetails details = new ExpensiveObjectDetails(obj);
            switch (type)
            {
                case "Scale":
                    details.scaleType = IsUniformScale(obj.transform.localScale) ? "Uniform" : "NonUniform";
                    details.scale = obj.transform.localScale;
                    break;
                case "Hierarchy":
                    details.hierarchyDepth = GetHierarchyDepth(obj.transform);
                    break;
                case "Components":
                    details.componentCount = obj.GetComponents<Component>().Length;
                    break;
            }

            if (!_sceneProfiler.ActiveExpensiveObjects.Contains(details))
            {
                _sceneProfiler.ActiveExpensiveObjects.Add(details);
            }
        }

        private bool IsUniformScale(Vector3 scale)
        {
            return Mathf.Approximately(scale.x, scale.y) && Mathf.Approximately(scale.y, scale.z);
        }

        private int GetHierarchyDepth(Transform transform)
        {
            int depth = 0;
            while (transform.parent != null)
            {
                depth++;
                transform = transform.parent;
            }
            return depth;
        }
    }
}