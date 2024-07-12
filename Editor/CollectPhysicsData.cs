using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SceneProfiler.Editor
{
    public class CollectPhysicsData
    {
        private SceneProfiler _sceneProfiler;

        public CollectPhysicsData(SceneProfiler sceneProfiler)
        {
            _sceneProfiler = sceneProfiler;
        }
    
        public void CheckPhysicsObjects()
        {
            Rigidbody[] rigidbodies = _sceneProfiler.FindObjects<Rigidbody>();

            foreach (var rb in rigidbodies)
            {
                if (rb != null)
                {
                    _sceneProfiler.ActivePhysicsObjects.Add(new PhysicsObjectDetails(rb));
                }
            }
        }
    
        public void CheckRigidbodyReferences(FieldInfo field, MonoBehaviour script)
        {
            if (field.FieldType == typeof(Rigidbody))
            {
                Rigidbody tRigidbody = field.GetValue(script) as Rigidbody;
                if (tRigidbody != null)
                {
                    PhysicsObjectDetails tPhysicsObjectDetails = new PhysicsObjectDetails(tRigidbody);

                    if (!_sceneProfiler.ActivePhysicsObjects.Any(r => r.rigidbody == tRigidbody))
                    {
                        _sceneProfiler.ActivePhysicsObjects.Add(tPhysicsObjectDetails);
                    }
                }
            }
        }
    }
}