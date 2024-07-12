using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SceneProfiler.Editor
{
    public class CollectMeshData
    {
        private SceneProfiler _sceneProfiler;
    
        public CollectMeshData(SceneProfiler sceneProfiler)
        {
            _sceneProfiler = sceneProfiler;
        }
    
        public void CheckMeshFilters()
        {
            MeshFilter[] meshFilters = _sceneProfiler.FindObjects<MeshFilter>();

            foreach (MeshFilter tMeshFilter in meshFilters)
            {
                Mesh tMesh = tMeshFilter.sharedMesh;
                if (tMesh != null)
                {
                    MeshDetails tMeshDetails = FindMeshDetails(tMesh);
                    if (tMeshDetails == null)
                    {
                        tMeshDetails = new MeshDetails();
                        tMeshDetails.mesh = tMesh;
                        _sceneProfiler.ActiveMeshDetails.Add(tMeshDetails);
                    }
                    tMeshDetails.FoundInMeshFilters.Add(tMeshFilter);

                    if (GameObjectUtility.AreStaticEditorFlagsSet(tMeshFilter.gameObject, StaticEditorFlags.BatchingStatic))
                    {
                        tMeshDetails.StaticBatchingEnabled.Add(tMeshFilter.gameObject);
                    }

                }
                else if (tMesh == null && tMeshFilter.transform.GetComponent("TextContainer") == null)
                {
                    Missing tMissing = new Missing();
                    tMissing.Object = tMeshFilter.transform;
                    tMissing.type = "mesh";
                    tMissing.name = tMeshFilter.transform.name;
                    _sceneProfiler.MissingObjects.Add(tMissing);
                    _sceneProfiler.thingsMissing = true;
                }

                var meshRenderrer = tMeshFilter.transform.GetComponent<MeshRenderer>();

                if (meshRenderrer == null || meshRenderrer.sharedMaterial == null)
                {
                    Missing tMissing = new Missing();
                    tMissing.Object = tMeshFilter.transform;
                    tMissing.type = "material";
                    tMissing.name = tMeshFilter.transform.name;
                    _sceneProfiler.MissingObjects.Add(tMissing);
                    _sceneProfiler.thingsMissing = true;
                }
            }
        }
    
        public void CheckSkinnedMeshRenderers()
        {
            SkinnedMeshRenderer[] skinnedMeshRenderers = _sceneProfiler.FindObjects<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer tSkinnedMeshRenderer in skinnedMeshRenderers)
            {
                Mesh tMesh = tSkinnedMeshRenderer.sharedMesh;
                if (tMesh != null)
                {
                    MeshDetails tMeshDetails = FindMeshDetails(tMesh);
                    if (tMeshDetails == null)
                    {
                        tMeshDetails = new MeshDetails();
                        tMeshDetails.mesh = tMesh;
                        _sceneProfiler.ActiveMeshDetails.Add(tMeshDetails);
                    }
                    tMeshDetails.FoundInSkinnedMeshRenderer.Add(tSkinnedMeshRenderer);
                }
                else if (tMesh == null)
                {
                    Missing tMissing = new Missing();
                    tMissing.Object = tSkinnedMeshRenderer.transform;
                    tMissing.type = "mesh";
                    tMissing.name = tSkinnedMeshRenderer.transform.name;
                    _sceneProfiler.MissingObjects.Add(tMissing);
                    _sceneProfiler.thingsMissing = true;
                }
                if (tSkinnedMeshRenderer.sharedMaterial == null)
                {
                    Missing tMissing = new Missing();
                    tMissing.Object = tSkinnedMeshRenderer.transform;
                    tMissing.type = "material";
                    tMissing.name = tSkinnedMeshRenderer.transform.name;
                    _sceneProfiler.MissingObjects.Add(tMissing);
                    _sceneProfiler.thingsMissing = true;
                }
            }
        }
    
        public void CheckLODGroups()
        {
            LODGroup[] lodGroups = _sceneProfiler.FindObjects<LODGroup>();
            if (lodGroups != null)
            {
                foreach (var group in lodGroups)
                {
                    var lods = group.GetLODs();
                    for (int i = 0, l = lods.Length; i < l; i++)
                    {
                        if (lods[i].renderers.Length == 0)
                        {
                            Missing tMissing = new Missing();
                            tMissing.Object = group.transform;
                            tMissing.type = "lod";
                            tMissing.name = group.transform.name;
                            _sceneProfiler.MissingObjects.Add(tMissing);
                            _sceneProfiler.thingsMissing = true;
                        }
                    }
                }
            }
        }
    
        public void CheckMeshReferences(FieldInfo field, MonoBehaviour script)
        {
            if (field.FieldType == typeof(Mesh))
            {
                Mesh tMesh = field.GetValue(script) as Mesh;
                if (tMesh != null)
                {
                    MeshDetails tMeshDetails = FindMeshDetails(tMesh);
                    if (tMeshDetails == null)
                    {
                        tMeshDetails = new MeshDetails();
                        tMeshDetails.mesh = tMesh;
                        tMeshDetails.instance = true;
                        _sceneProfiler.ActiveMeshDetails.Add(tMeshDetails);
                    }
                }
            }
        }
    
        MeshDetails FindMeshDetails(Mesh tMesh)
        {
            foreach (MeshDetails tMeshDetails in _sceneProfiler.ActiveMeshDetails)
            {
                if (tMeshDetails.mesh == tMesh) return tMeshDetails;
            }
            return null;
        }
    
    }
}
