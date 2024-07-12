using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SceneProfiler.Editor
{
    public class CollectLightData
    {
        private SceneProfiler _sceneProfiler;

        public CollectLightData(SceneProfiler sceneProfiler)
        {
            _sceneProfiler = sceneProfiler;
        }
    
        public void CheckLights()
        {
            Light[] lights = _sceneProfiler.FindObjects<Light>();

            foreach (Light light in lights)
            {
                LightDetails lightDetails = new LightDetails();
                lightDetails.light = light;
                lightDetails.isEnabled = light.enabled;
                lightDetails.shadowType = light.shadows;
                lightDetails.isActive = light.gameObject.activeInHierarchy; 

                _sceneProfiler.ActiveLights.Add(lightDetails);
            }
        }
    
        public void CheckLightReferences(FieldInfo field, MonoBehaviour script)
        {
            if (field.FieldType == typeof(Light))
            {
                Light tLight = field.GetValue(script) as Light;
                if (tLight != null)
                {
                    LightDetails tLightDetails = new LightDetails
                    {
                        light = tLight,
                        isEnabled = tLight.enabled,
                        shadowType = tLight.shadows,
                        isActive = tLight.gameObject.activeInHierarchy
                    };

                    if (!_sceneProfiler.ActiveLights.Any(l => l.light == tLight))
                    {
                        _sceneProfiler.ActiveLights.Add(tLightDetails);
                    }
                }
            }
        }
    }
}