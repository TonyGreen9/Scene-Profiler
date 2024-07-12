using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SceneProfiler.Editor
{
    public class CollectAudioClipData
    {
        private SceneProfiler _sceneProfiler;
    
        public CollectAudioClipData(SceneProfiler sceneProfiler)
        {
            _sceneProfiler = sceneProfiler;
        }
    
        public void CheckSelectedFolder()
        {
            if (!_sceneProfiler.IncludeSelectedFolder || Selection.objects.Length == 0) return;

            var folders = new List<string>();
            foreach (var obj in Selection.objects)
            {
                if (obj.GetType() != typeof(DefaultAsset)) continue;
                var path = AssetDatabase.GetAssetPath(obj);
                folders.Add(path);
            }

            if (folders.Count != 0)
            {
                var guids = AssetDatabase.FindAssets("t:AudioClip", folders.ToArray());
                if (guids.Length != 0)
                {
                    foreach (var guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        var item = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                        var tClipDetails = FindClipDetails(item);
                        if (tClipDetails == null)
                        {
                            tClipDetails = new AudioClipDetails();
                            tClipDetails.clip = item;
                            _sceneProfiler.ActiveClipDetails.Add(tClipDetails);
                        }
                    }
                }
            }
        }
    
        AudioClipDetails FindClipDetails(AudioClip tClip)
        {
            foreach (AudioClipDetails tClipDetails in _sceneProfiler.ActiveClipDetails)
            {
                if (tClipDetails.clip == tClip) return tClipDetails;
            }
            return null;
        }
    
        public void CheckAudioSources()
        {
            AudioSource[] AudioSources = _sceneProfiler.FindObjects<AudioSource>();

            foreach (AudioSource tAudioSource in AudioSources)
            {
                AudioClip tClip = tAudioSource.clip;
                if (tClip != null)
                {
                    AudioClipDetails tClipDetails = FindClipDetails(tClip);
                    if (tClipDetails == null)
                    {
                        tClipDetails = new AudioClipDetails();
                        tClipDetails.clip = tClip;
                        _sceneProfiler.ActiveClipDetails.Add(tClipDetails);
                    }
                    tClipDetails.FoundInAudioSources.Add(tAudioSource);

                }
                else if (tClip == null)
                {
                    Missing tMissing = new Missing();
                    tMissing.Object = tAudioSource.transform;
                    tMissing.type = "audio clip";
                    tMissing.name = tAudioSource.transform.name;
                    _sceneProfiler.MissingObjects.Add(tMissing);
                    _sceneProfiler.thingsMissing = true;
                }
            }
        }
    
        public void CheckAudioClipReferences(FieldInfo field, MonoBehaviour script)
        {
            if (field.FieldType == typeof(AudioClip))
            {
                AudioClip tClip = field.GetValue(script) as AudioClip;
                if (tClip != null)
                {
                    AudioClipDetails tClipDetails = FindClipDetails(tClip);
                    if (tClipDetails == null)
                    {
                        tClipDetails = new AudioClipDetails();
                        tClipDetails.clip = tClip;
                        _sceneProfiler.ActiveClipDetails.Add(tClipDetails);
                    }
                }
            }
        }
    }
}