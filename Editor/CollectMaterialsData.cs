using UnityEngine;
using UnityEngine.UI;

namespace SceneProfiler.Editor
{
    public class CollectMaterialsData
    {
        private SceneProfiler _sceneProfiler;
    
        public CollectMaterialsData(SceneProfiler sceneProfiler)
        {
            _sceneProfiler = sceneProfiler;
        }
    
        MaterialDetails FindMaterialDetails(Material tMaterial)
        {
            foreach (MaterialDetails tMaterialDetails in _sceneProfiler.ActiveMaterials)
            {
                if (tMaterialDetails.material == tMaterial) return tMaterialDetails;
            }
            return null;
        }
    
        public void CheckRenderers()
        {
            Renderer[] renderers = _sceneProfiler.FindObjects<Renderer>();

            AddSkyboxMaterial();

            foreach (Renderer renderer in renderers)
            {
                AddMaterialDetails(renderer);
            }
        }
    
        private void AddSkyboxMaterial()
        {
            MaterialDetails skyMat = new MaterialDetails
            {
                material = RenderSettings.skybox,
                isSky = true
            };
            _sceneProfiler.ActiveMaterials.Add(skyMat);
        }
    
        private void AddMaterialDetails(Renderer renderer)
        {
            foreach (Material material in renderer.sharedMaterials)
            {
                MaterialDetails tMaterialDetails = FindMaterialDetails(material);
                if (tMaterialDetails == null)
                {
                    tMaterialDetails = new MaterialDetails
                    {
                        material = material
                    };
                    _sceneProfiler.ActiveMaterials.Add(tMaterialDetails);
                }
                tMaterialDetails.FoundInRenderers.Add(renderer);
            }
        }
    
        public void CheckGUIElements()
        {
            if (!_sceneProfiler.IncludeGuiElements) return;

            Graphic[] graphics = _sceneProfiler.FindObjects<Graphic>();

            foreach (Graphic graphic in graphics)
            {

                if (graphic.materialForRendering)
                {
                    MaterialDetails tMaterialDetails = FindMaterialDetails(graphic.materialForRendering);
                    if (tMaterialDetails == null)
                    {
                        tMaterialDetails = new MaterialDetails();
                        tMaterialDetails.material = graphic.materialForRendering;
                        tMaterialDetails.isgui = true;
                        _sceneProfiler.ActiveMaterials.Add(tMaterialDetails);
                    }
                    tMaterialDetails.FoundInGraphics.Add(graphic);
                }
            }
        }
    
        public void AddMaterialDetails(Material tMaterial)
        {
            MaterialDetails tMatDetails = FindMaterialDetails(tMaterial);
            if (tMatDetails == null)
            {
                tMatDetails = new MaterialDetails();
                tMatDetails.instance = true;
                tMatDetails.material = tMaterial;
                if (!_sceneProfiler.ActiveMaterials.Contains(tMatDetails))
                {
                    _sceneProfiler.ActiveMaterials.Add(tMatDetails);
                }
            }
        }
    }
}