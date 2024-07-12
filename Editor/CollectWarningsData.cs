using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneProfiler.Editor
{
    public class CollectWarningsData
    {
        private SceneProfiler _sceneProfiler;
    
        public CollectWarningsData(SceneProfiler sceneProfiler)
        {
            _sceneProfiler = sceneProfiler;
        }
        
         public float GetSceneFileSize()
        {
            float totalSize = 0;
            string scenePath = SceneManager.GetActiveScene().path;

            if (!string.IsNullOrEmpty(scenePath))
            {
                totalSize = new FileInfo(scenePath).Length;
            }

            return totalSize / (1024f * 1024f); 
        }
    
        public int CountObjectsInScene()
        {
            GameObject[] allObjects = _sceneProfiler.GetAllRootGameObjects();
            int objectCount = 0;

            foreach (GameObject go in allObjects)
            {
                objectCount += go.GetComponentsInChildren<Transform>(true).Length;
            }

            return objectCount;
        }
    
        public int CountCanvasComponentsInScene()
        {
            Canvas[] canvases = _sceneProfiler.GetAllRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<Canvas>(true))
                .ToArray();

            return canvases.Length;
        }

        public List<string> GetPlatformsWithoutStaticBatching()
        {
            var platformsWithoutStaticBatching = new List<string>();

            var buildTargets = new BuildTarget[]
            {
                BuildTarget.StandaloneWindows,
                BuildTarget.StandaloneWindows64,
                BuildTarget.StandaloneOSX,
                BuildTarget.StandaloneLinux64,
                BuildTarget.Android,
                BuildTarget.iOS,
                BuildTarget.PS4,
                BuildTarget.XboxOne,
                BuildTarget.Switch
            };

            var playerSettingsType = typeof(PlayerSettings);
            var method = playerSettingsType.GetMethod("GetBatchingForPlatform", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                Debug.LogError("GetBatchingForPlatform method not found!");
                return platformsWithoutStaticBatching;
            }

            foreach (var buildTarget in buildTargets)
            {
                try
                {
                    object[] parameters = new object[] { buildTarget, 0, 0 };
                    method.Invoke(null, parameters);
                    int staticBatchingFlags = (int)parameters[1];
                    if ((staticBatchingFlags & 1) == 0)
                    {
                        platformsWithoutStaticBatching.Add(buildTarget.ToString());
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error checking static batching for platform {buildTarget}: {e.Message}");
                }
            }

            return platformsWithoutStaticBatching;
        }
    }
}