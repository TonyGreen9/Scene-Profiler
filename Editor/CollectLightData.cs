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
                if (!_sceneProfiler.ActiveLights.Any(l => l.light == light))
                {
                    LightDetails lightDetails = new LightDetails
                    {
                        light = light,
                        isEnabled = light.enabled,
                        shadowType = light.shadows,
                        isActive = light.gameObject.activeInHierarchy,
                        radius = light.type == LightType.Point ? light.range : 0f,  // Radius is relevant for point lights
                        intensity = light.intensity,
                        lightType = light.type,
                        mode = light.lightmapBakeType,
                        renderMode = light.renderMode,
                        cullingMask = light.cullingMask
                    };

                    _sceneProfiler.ActiveLights.Add(lightDetails);
                }
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
                        isActive = tLight.gameObject.activeInHierarchy,
                        radius = tLight.type == LightType.Point ? tLight.range : 0f,  // Radius is relevant for point lights
                        intensity = tLight.intensity,
                        lightType = tLight.type,
                        mode = tLight.lightmapBakeType,
                        renderMode = tLight.renderMode,
                        cullingMask = tLight.cullingMask
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
