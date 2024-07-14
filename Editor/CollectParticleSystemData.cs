using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SceneProfiler.Editor
{
    public class CollectParticleSystemData
    {
        private SceneProfiler _sceneProfiler;

        public CollectParticleSystemData(SceneProfiler sceneProfiler)
        {
            _sceneProfiler = sceneProfiler;
        }
    
        public void CheckParticleSystems()
        {
            ParticleSystem[] particleSystems = _sceneProfiler.FindObjects<ParticleSystem>();

            foreach (ParticleSystem ps in particleSystems)
            {
                if (ps == null) continue;
                
                if (!_sceneProfiler.ActiveParticleSystems.Any(p => p.particleSystem == ps))
                {
                    ParticleSystemDetails psDetails = new ParticleSystemDetails
                    {
                        particleSystem = ps
                    };

                    psDetails.FoundInGameObjects.Add(ps.gameObject);

                    var renderer = ps.GetComponent<ParticleSystemRenderer>();
                    if (renderer != null)
                    {
                        psDetails.material = renderer.sharedMaterial;
                    }

                    var main = ps.main;
                    psDetails.maxParticles = main.maxParticles;
                    psDetails.activeParticles = ps.particleCount;

                    _sceneProfiler.ActiveParticleSystems.Add(psDetails);
                }
            }

            _sceneProfiler.ActiveParticleSystems.Sort((a, b) => b.activeParticles.CompareTo(a.activeParticles));
        }
    
        public void CheckParticleSystemReferences(FieldInfo field, MonoBehaviour script)
        {
            if (field.FieldType == typeof(ParticleSystem))
            {
                ParticleSystem tParticleSystem = field.GetValue(script) as ParticleSystem;
                if (tParticleSystem != null)
                {
                    if (!_sceneProfiler.ActiveParticleSystems.Any(p => p.particleSystem == tParticleSystem))
                    {
                        ParticleSystemDetails tParticleSystemDetails = new ParticleSystemDetails
                        {
                            particleSystem = tParticleSystem
                        };

                        _sceneProfiler.ActiveParticleSystems.Add(tParticleSystemDetails);
                    }
                }
            }
        }
    }
}
