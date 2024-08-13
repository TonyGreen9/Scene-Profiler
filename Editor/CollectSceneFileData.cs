using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Reflection;

namespace SceneProfiler.Editor
{
    public class CollectSceneFileData
    {
        private SceneProfiler _sceneProfiler;
        private const int MaxGameObjects = 20000;

        public CollectSceneFileData(SceneProfiler sceneProfiler)
        {
            _sceneProfiler = sceneProfiler;
        }

        public void CollectData()
        {
            string scenePath = SceneManager.GetActiveScene().path;
            if (string.IsNullOrEmpty(scenePath)) return;

            string[] lines = File.ReadAllLines(scenePath);
            _sceneProfiler.totalLineCount = lines.Length;
            
            Dictionary<string, (GameObject go, string guid)> sceneObjects = new Dictionary<string, (GameObject go, string guid)>();
            
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (IsInCurrentScene(go))
                {
                    string fileId = GetLocalIdentifierInFile(go);
                    string guid = AssetDatabase.AssetPathToGUID(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go));
                    if (!string.IsNullOrEmpty(fileId) && !sceneObjects.ContainsKey(fileId))
                    {
                        sceneObjects.Add(fileId, (go, guid));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(fileId))
                        {
                            Debug.LogWarning($"GameObject: {go.name} has an empty File ID.");
                        }
                    }
                }
            }

            Dictionary<string, int> prefabInstanceCounter = new Dictionary<string, int>();

            int processedGameObjects = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("--- !u!1 &") || lines[i].StartsWith("--- !u!1001 &")) // Game objects and Prefab Instances
                {
                    ProcessObject(lines, ref i, ref processedGameObjects, sceneObjects, prefabInstanceCounter);
                }
            }
        }

        private bool IsInCurrentScene(GameObject go)
        {
            return go.scene == SceneManager.GetActiveScene();
        }

        private void ProcessObject(string[] lines, ref int i, ref int processedGameObjects, Dictionary<string, (GameObject go, string guid)> sceneObjects, Dictionary<string, int> prefabInstanceCounter)
        {
            if (processedGameObjects >= MaxGameObjects)
            {
                return;
            }

            string identifier = ExtractIdentifier(lines[i]);
            string fileId = GetFileIdFromIdentifier(identifier, lines, ref i);
            string objectType = lines[i + 1].Trim();
            string objectName = FindObjectName(lines, i);
            int totalLines = CalculateTotalLines(lines, i);
            string guid = ExtractGUIDFromPrefabInstance(lines, ref i);

            bool isStripped = identifier.Contains("stripped");

            GameObject foundObject = null;

            if (!isStripped)
            {
                if (objectType.Contains("PrefabInstance"))
                {
                    string prefabGuid = ExtractGUIDFromPrefabInstance(lines, ref i);

                    if (!prefabInstanceCounter.ContainsKey(objectName))
                    {
                        prefabInstanceCounter[objectName] = 0;
                    }

                    int prefabIndex = prefabInstanceCounter[objectName];

                    var matchingObjects = sceneObjects.Values
                        .Where(obj => obj.guid == prefabGuid && obj.go.name == objectName)
                        .Select(obj => obj.go)
                        .ToList();

                    if (prefabIndex < matchingObjects.Count)
                    {
                        foundObject = matchingObjects[prefabIndex];
                    }

                    prefabInstanceCounter[objectName]++;
                }
                else if (sceneObjects.TryGetValue(fileId, out (GameObject go, string guidValue) obj))
                {
                    foundObject = obj.go;
                    guid = obj.guidValue;
                }
            }

            float lineSizeInKB = (float)new FileInfo(SceneManager.GetActiveScene().path).Length / 1024f / _sceneProfiler.totalLineCount;
            float sizeInKB = totalLines * lineSizeInKB;
            float sizePercentage = (sizeInKB / (new FileInfo(SceneManager.GetActiveScene().path).Length / 1024f)) * 100f;

            if (!_sceneProfiler.SceneFileDetails.Any(detail => detail.identifier == identifier))
            {
                var sceneFileDetails = new SceneFileDetails(objectName, foundObject, totalLines, objectType, identifier, guid, sizeInKB, sizePercentage);

                // Collect component details
                CollectComponentDetails(lines, ref i, ref sceneFileDetails, lineSizeInKB);

                _sceneProfiler.SceneFileDetails.Add(sceneFileDetails);
            }
            processedGameObjects++;
        }

        private void CollectComponentDetails(string[] lines, ref int startLine, ref SceneFileDetails sceneFileDetails, float lineSizeInKB)
        {
            

            for (int i = startLine + 1; i < lines.Length; i++)
            {
                // Break if a new GameObject starts
                if (lines[i].StartsWith("--- !u!1 &") || lines[i].StartsWith("--- !u!1001 &"))
                {
                    startLine = i - 1; // Update startLine to reflect the last processed line
                    break;
                }

                // Check for a component entry by looking for lines that have a valid component ID pattern (e.g., "--- !u!20 &")
                if (lines[i].StartsWith("--- !u!") && lines[i].Contains("&"))
                {
                    string componentIdentifier = ExtractIdentifier(lines[i]);
                    string componentType = lines[i + 1].Trim();
                    int componentLineCount = CalculateTotalLines(lines, i);
                    float componentSizeInKB = componentLineCount * lineSizeInKB;
                    float componentSizePercentage = (componentSizeInKB / (new FileInfo(SceneManager.GetActiveScene().path).Length / 1024f)) * 100f;

                    // Adding the component details to the list
                    var componentDetails = new ComponentDetails(
                        null, // Reference to the actual component (can be linked later if needed)
                        componentType,
                        componentLineCount,
                        componentIdentifier,
                        string.Empty, // GUID is optional and might not be necessary for all components
                        componentSizeInKB,
                        componentSizePercentage
                    );

                    sceneFileDetails.components.Add(componentDetails);
                }
            }
        }


        private string ExtractIdentifier(string line)
        {
            return line.Trim();
        }

        private string GetFileIdFromIdentifier(string identifier, string[] lines, ref int startLine)
        {
            if (identifier.StartsWith("--- !u!1001 &"))
            {
                for (int i = startLine + 1; i < lines.Length; i++)
                {
                    if (lines[i].Contains("propertyPath: m_Name"))
                    {
                        string previousLine = lines[i - 1];
                        string fileId = ExtractFileIdFromPrefabInstance(previousLine);
                        return fileId;
                    }

                    if (lines[i].StartsWith("--- !u!"))
                    {
                        break;
                    }
                }
            }

            string[] parts = identifier.Split('&');
            if (parts.Length > 1)
            {
                string fileId = parts[1].Trim();
                return fileId.Contains("stripped") ? fileId.Split(' ')[0] : fileId;
            }

            return null;
        }

        private string ExtractFileIdFromPrefabInstance(string line)
        {
            string[] parts = line.Split(new[] { "fileID:" }, StringSplitOptions.None);
            if (parts.Length > 1)
            {
                string fileIdPart = parts[1].Split(',')[0].Trim();
                return fileIdPart;
            }

            return null;
        }

        private string ExtractGUIDFromPrefabInstance(string[] lines, ref int startLine)
        {
            for (int i = startLine + 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.Contains("m_SourcePrefab"))
                {
                    int guidIndex = line.IndexOf("guid: ") + 6;
                    string guid = line.Substring(guidIndex, 32);
                    return guid;
                }
                
                if (line.StartsWith("--- !u!") || line.Contains("component: {fileID: "))
                {
                    break;
                }
            }

            return null;
        }

        private string FindObjectName(string[] lines, int startLine)
        {
            for (int i = startLine + 1; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("--- !u!"))
                {
                    break;
                }

                // Find and return the object name
                if (lines[i].Contains("m_Name:"))
                {
                    return lines[i].Split(':')[1].Trim();
                }
                if (lines[i].Contains("propertyPath: m_Name"))
                {
                    return lines[i + 1].Split(':')[1].Trim();
                }
            }
            return "__Unnamed__";
        }

        private int CalculateTotalLines(string[] lines, int startLine)
        {
            int endLine = -1;

            for (int i = startLine + 1; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("--- !u!1 &") || lines[i].StartsWith("--- !u!1001480554 &") || lines[i].StartsWith("--- !u!1001 &"))
                {
                    endLine = i;
                    break;
                }
            }

            if (endLine == -1)
                endLine = lines.Length;

            return endLine - startLine;
        }

        private string GetLocalIdentifierInFile(GameObject obj)
        {
            #if UNITY_EDITOR
            PropertyInfo inspectorModeInfo = typeof(UnityEditor.SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
            UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(obj);
            inspectorModeInfo.SetValue(serializedObject, UnityEditor.InspectorMode.Debug, null);

            UnityEditor.SerializedProperty localIdProp = serializedObject.FindProperty("m_LocalIdentfierInFile");
            long localId = localIdProp.longValue;

            return localId.ToString();
            #else
            return null;
            #endif
        }
    }
}
