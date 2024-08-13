using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SceneProfiler.Editor.GUI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Application = UnityEngine.Application;
using Object = UnityEngine.Object;


namespace SceneProfiler.Editor
{
    public class SceneProfiler : EditorWindow
    {
        public enum InspectType
        {
            Textures, Materials, Meshes, AudioClips, Missing, Particles, Lights, Physics, Expensive, SceneFile
        };

        public bool includeDisabledObjects = true;
        public bool IncludeSpriteAnimations = true;
        public bool IncludeScriptReferences = true;
        public bool IncludeGuiElements = true;
        public bool IncludeLightmapTextures = true;
        public bool IncludeSelectedFolder = false;
        public bool thingsMissing = false;
        
        public bool EnableExpensiveObjectCollection = true;
        public bool EnableSceneFileCollection = true;
        public bool EnableParticleSystemCollection = true;
        public bool EnableLightCollection = true;
        public bool EnablePhysicsObjectCollection = true;
        public bool EnableMaterialCollection = true;
        public bool EnableTextureCollection = true;
        public bool EnableMeshCollection = true;
        public bool EnableAudioClipCollection = true;
        public bool EnableScriptReferencesCheck = true;
        public bool EnableMissingScriptCheck = true;

        public InspectType ActiveInspectType = InspectType.Textures;

        public List<TextureDetails> ActiveTextures = new List<TextureDetails>();
        public List<MaterialDetails> ActiveMaterials = new List<MaterialDetails>();
        public List<MeshDetails> ActiveMeshDetails = new List<MeshDetails>();
        public List<AudioClipDetails> ActiveClipDetails = new List<AudioClipDetails>();
        public List<ParticleSystemDetails> ActiveParticleSystems = new List<ParticleSystemDetails>();
        public List<Missing> MissingObjects = new List<Missing>();
        public List<LightDetails> ActiveLights = new List<LightDetails>();
        public List<PhysicsObjectDetails> ActivePhysicsObjects = new List<PhysicsObjectDetails>();
        public List<SceneWarningDetails> Warnings = new List<SceneWarningDetails>();
        public List<ExpensiveObjectDetails> ActiveExpensiveObjects = new List<ExpensiveObjectDetails>();
        public List<SceneFileDetails> SceneFileDetails = new List<SceneFileDetails>();
        public List<SceneFileDetails> SceneFileComponentsDetails = new List<SceneFileDetails>();

        public float TotalTextureMemory = 0;
        public int TotalMeshVertices = 0;
        public int totalLineCount; 

        public bool ctrlPressed = false;

        private static int _minWidth = 800;
        public int currentObjectsInColumnCount = 100;
        
        private GameObject[] _cachedRootGameObjects;
        private List<GameObject> _cachedDontDestroyOnLoadRoots;
        private List<GameObject> _cachedAllGameObjects;

        private SceneProfilerGUI _sceneProfilerGUI;
        private CollectTextureData _collectTextureData;
        private CollectMaterialsData _collectMaterialsData;
        private CollectAudioClipData _collectAudioClipData;
        private CollectMeshData _collectMeshData;
        private CollectParticleSystemData _сollectParticleSystemData;
        private CollectLightData _collectLightData;
        private CollectPhysicsData _collectPhysicsData;
        private CollectWarningsData _collectWarningsData;
        private CollectExpensiveObject _collectExpensiveObject;
        private CollectSceneFileData _сollectSceneFileData;

        [MenuItem("Window/Analysis/Scene Profiler")]
        public static void Init()
        {
            SceneProfiler window = (SceneProfiler)EditorWindow.GetWindow(typeof(SceneProfiler));
            GUIContent titleContent = EditorGUIUtility.IconContent("UnityEditor.SceneView");
            titleContent.text = "Scene Profiler";
            window.titleContent = titleContent; 
            window.minSize = new Vector2(_minWidth, 400);
        }

        private void OnEnable()
        {
            _sceneProfilerGUI = new SceneProfilerGUI(this);
            _collectTextureData = new CollectTextureData(this);
            _collectMaterialsData = new CollectMaterialsData(this);
            _collectAudioClipData = new CollectAudioClipData(this);
            _collectMeshData = new CollectMeshData(this);
            _сollectParticleSystemData = new CollectParticleSystemData(this);
            _collectLightData = new CollectLightData(this);
            _collectPhysicsData = new CollectPhysicsData(this);
            _collectWarningsData = new CollectWarningsData(this);
            _collectExpensiveObject = new CollectExpensiveObject(this);
            _сollectSceneFileData  = new CollectSceneFileData(this);
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            ClearAndRepaint();
        }

        private void OnDisable()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            ClearAndRepaint();
        }
        
        private void OnSceneUnloaded(Scene scene)
        {
            ClearAndRepaint();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ClearAndRepaint();
        }


        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.ExitingPlayMode)
            {
                ClearAndRepaint();
            }
        }

        public void ClearAndRepaint()
        {
            ActiveTextures.Clear();
            ActiveMaterials.Clear();
            ActiveMeshDetails.Clear();
            ActiveClipDetails.Clear();
            MissingObjects.Clear();
            ActiveParticleSystems.Clear();
            ActiveLights.Clear();
            ActivePhysicsObjects.Clear();
            ActiveExpensiveObjects.Clear();
            Warnings.Clear();
            SceneFileDetails.Clear();

            TotalTextureMemory = 0;
            TotalMeshVertices = 0;
            totalLineCount = 0;

            thingsMissing = false;

            Repaint();
        }

        void OnGUI()
        {
            _sceneProfilerGUI.DrawGUI();
        }

        public int UniqueShadersCount
        {
            get
            {
                return ActiveMaterials
                    .Where(mat => mat.material != null && mat.material.shader != null)
                    .Select(mat => mat.material.shader)
                    .Distinct()
                    .Count();
            }
        }

        public void SelectObject(Object selectedObject, bool append)
        {
            if (append)
            {
                List<Object> currentSelection = new List<Object>(Selection.objects);
                if (currentSelection.Contains(selectedObject)) currentSelection.Remove(selectedObject);
                else currentSelection.Add(selectedObject);

                Selection.objects = currentSelection.ToArray();
            }
            else Selection.activeObject = selectedObject;
        }

        public void SelectObjects(List<Object> selectedObjects, bool append)
        {
            if (append)
            {
                List<Object> currentSelection = new List<Object>(Selection.objects);
                currentSelection.AddRange(selectedObjects);
                Selection.objects = currentSelection.ToArray();
            }
            else Selection.objects = selectedObjects.ToArray();
        }

        private static int MaterialSorter(MaterialDetails first, MaterialDetails second)
        {
            var firstIsNull = first.material == null;
            var secondIsNull = second.material == null;

            if (firstIsNull && secondIsNull) return 0;
            if (firstIsNull) return int.MaxValue;
            if (secondIsNull) return int.MinValue;

            return first.material.renderQueue - second.material.renderQueue;
        }

        public string FormatSizeString(float memSizeKB)
        {
            if (memSizeKB < 1024) return "" + memSizeKB + "k";
            else
            {
                float memSizeMB = ((float)memSizeKB) / 1024.0f;
                return memSizeMB.ToString("0.00") + "Mb";
            }
        }
        // Main method
        public void CollectData()
        {
            _cachedRootGameObjects = null;
            _cachedDontDestroyOnLoadRoots = null;
            _cachedAllGameObjects = null;
            ActiveTextures.Clear();
            ActiveMaterials.Clear();
            ActiveMeshDetails.Clear();
            MissingObjects.Clear();
            ActiveClipDetails.Clear();
            ActiveParticleSystems.Clear();
            ActiveLights.Clear();
            ActivePhysicsObjects.Clear();
            ActiveExpensiveObjects.Clear();
            thingsMissing = false;

            // Start collecting data according to the flags, in the original order
            
            if (EnableExpensiveObjectCollection)
                _collectExpensiveObject.CollectData();

            if (EnableSceneFileCollection)
                _сollectSceneFileData.CollectData();

            if (EnableParticleSystemCollection)
                _сollectParticleSystemData.CheckParticleSystems();

            if (EnableLightCollection)
                _collectLightData.CheckLights();

            if (EnablePhysicsObjectCollection)
                _collectPhysicsData.CheckPhysicsObjects();

            if (EnableMaterialCollection)
            {
                _collectMaterialsData.CheckRenderers();
                _collectMaterialsData.CheckGUIElements();
            }

            if (EnableTextureCollection)
            {
                _collectTextureData.CheckRenderers();
                _collectTextureData.CheckLightmaps();
                _collectTextureData.CheckGUIElements();
                _collectTextureData.CheckSelectedFolder();
                _collectTextureData.CheckSpriteAnimations();
                _collectTextureData.CheckMaterials(); // Collect all materials before
            }

            if (EnableMeshCollection)
            {
                _collectMeshData.CheckMeshFilters();
                _collectMeshData.CheckSkinnedMeshRenderers();
                _collectMeshData.CheckLODGroups();
            }

            if (EnableAudioClipCollection)
            {
                _collectAudioClipData.CheckSelectedFolder();
                _collectAudioClipData.CheckAudioSources();
            }

            if (EnableScriptReferencesCheck)
                CheckScriptReferences();

            if (EnableMissingScriptCheck)
                CheckForMissingScriptsInAllGameObjects();

            // Finalize by calculating totals and sorting materials
            CalculateTotals();

            ActiveMaterials.Sort(MaterialSorter);
            CheckWarnings();
        }

        
        public void UpdateFlagsBasedOnModuleStates(Dictionary<InspectType, bool> moduleStates)
        {
            EnableTextureCollection = moduleStates[InspectType.Textures];
            EnableMaterialCollection = moduleStates[InspectType.Materials];
            EnableMeshCollection = moduleStates[InspectType.Meshes];
            EnableAudioClipCollection = moduleStates[InspectType.AudioClips];
            EnableParticleSystemCollection = moduleStates[InspectType.Particles];
            EnableLightCollection = moduleStates[InspectType.Lights];
            EnablePhysicsObjectCollection = moduleStates[InspectType.Physics];
            EnableMissingScriptCheck = moduleStates[InspectType.Missing];
            EnableExpensiveObjectCollection = moduleStates[InspectType.Expensive];
            EnableSceneFileCollection = moduleStates[InspectType.SceneFile];
        }

        public void AddMissingSprite(SpriteRenderer tSpriteRenderer)
        {
            Missing tMissing = new Missing
            {
                Object = tSpriteRenderer.transform,
                type = "sprite",
                name = tSpriteRenderer.transform.name
            };
            MissingObjects.Add(tMissing);
            thingsMissing = true;
        }
    
        private void CheckScriptReferences()
        {
            if (!IncludeScriptReferences) return;

            MonoBehaviour[] scripts = FindObjects<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
                FieldInfo[] fields = script.GetType().GetFields(flags);

                foreach (FieldInfo field in fields)
                {
                    _collectTextureData.CheckSpriteReferences(field, script);
                    _collectMeshData.CheckMeshReferences(field, script);
                    CheckMaterialReferences(field, script);
                    _collectAudioClipData.CheckAudioClipReferences(field, script);
                    _collectLightData.CheckLightReferences(field, script);
                    _сollectParticleSystemData.CheckParticleSystemReferences(field, script);
                    _collectPhysicsData.CheckRigidbodyReferences(field, script);
                }
            }
        }
    
        private void CheckMaterialReferences(FieldInfo field, MonoBehaviour script)
        {
            if (field.FieldType == typeof(Material))
            {
                Material tMaterial = field.GetValue(script) as Material;
                if (tMaterial != null)
                {
                    _collectMaterialsData.AddMaterialDetails(tMaterial);
                    _collectTextureData.CheckMaterialTextures(tMaterial);
                    _collectTextureData.CheckMaterialDependencies(tMaterial);
                }
            }
        }
    
        private void CheckForMissingScriptsInAllGameObjects()
        {
            GameObject[] allGameObjects = GetAllRootGameObjects();
            foreach (GameObject go in allGameObjects)
            {
                CheckForMissingScripts(go);
            }
        }

        void CheckForMissingScripts(GameObject go)
        {
            Component[] components = go.GetComponents<Component>();

            foreach (Component component in components)
            {
                if (component == null)
                {
                    Missing tMissing = new Missing();
                    tMissing.Object = go.transform;
                    tMissing.type = "missing script";
                    tMissing.name = go.name;
                    MissingObjects.Add(tMissing);
                    thingsMissing = true;
                }
            }

            foreach (Transform child in go.transform)
            {
                CheckForMissingScripts(child.gameObject);
            }
        }
    
        private void CalculateTotals()
        {
            TotalMeshVertices = 0;
            foreach (MeshDetails tMeshDetails in ActiveMeshDetails) TotalMeshVertices += tMeshDetails.mesh.vertexCount;

            ActiveTextures.Sort(delegate (TextureDetails details1, TextureDetails details2) { return (int)(details2.memSizeKB - details1.memSizeKB); });
            ActiveTextures = ActiveTextures.Distinct().ToList();
            TotalTextureMemory = 0;
            foreach (TextureDetails tTextureDetails in ActiveTextures) TotalTextureMemory += tTextureDetails.memSizeKB;
            ActiveMeshDetails.Sort(delegate (MeshDetails details1, MeshDetails details2) { return details2.mesh.vertexCount - details1.mesh.vertexCount; });
        }

        private void CheckWarnings()
        {
            Warnings.Clear();

            if (thingsMissing)
            {
                Warnings.Add(new SceneWarningDetails("Some GameObjects are missing elements.", MessageType.Error));
            }
        
            float sceneFileSizeMB = _collectWarningsData.GetSceneFileSize();
            Warnings.Add(new SceneWarningDetails($"Scene file size: {sceneFileSizeMB:F2} MB.", MessageType.Info));
        
            int objectCount = _collectWarningsData.CountObjectsInScene();
            Warnings.Add(new SceneWarningDetails($"There are {objectCount} objects in the scene.", MessageType.Info));
        
            int canvasCount = _collectWarningsData.CountCanvasComponentsInScene();
            if (canvasCount > 0)
            {
                Warnings.Add(new SceneWarningDetails($"There are {canvasCount} Canvas components in the scene.", MessageType.Info));
            }

            var platformsWithoutStaticBatching = _collectWarningsData.GetPlatformsWithoutStaticBatching();
            if (platformsWithoutStaticBatching.Count > 0)
            {
                string message = "Static batching is not enabled: " + string.Join(", ", platformsWithoutStaticBatching);
                Warnings.Add(new SceneWarningDetails(message, MessageType.Warning));
            }
        }

        public GameObject[] GetAllRootGameObjects()
        {
            if (_cachedRootGameObjects != null)
            {
                return _cachedRootGameObjects;
            }

            List<GameObject> allGo = new List<GameObject>();
        
            for (int sceneIdx = 0; sceneIdx < SceneManager.sceneCount; ++sceneIdx)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIdx);
                if (scene.isLoaded)
                {
                    GameObject[] rootGameObjects = scene.GetRootGameObjects();
                    allGo.AddRange(rootGameObjects);
                }
            }
            _cachedRootGameObjects = allGo.ToArray();
            return _cachedRootGameObjects;
        }


        private List<GameObject> GetDontDestroyOnLoadRoots()
        {
            if (_cachedDontDestroyOnLoadRoots != null)
            {
                return _cachedDontDestroyOnLoadRoots;
            }

            List<GameObject> objs = new List<GameObject>();
            if (Application.isPlaying)
            {
                GameObject temp = null;
                try
                {
                    temp = new GameObject();
                    DontDestroyOnLoad(temp);
                    Scene dontDestryScene = temp.scene;
                    DestroyImmediate(temp);
                    temp = null;

                    if (dontDestryScene.IsValid())
                    {
                        objs = dontDestryScene.GetRootGameObjects().ToList();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                    return null;
                }
                finally
                {
                    if (temp != null)
                        DestroyImmediate(temp);
                }
            }
            _cachedDontDestroyOnLoadRoots = objs;
            return _cachedDontDestroyOnLoadRoots;
        }

        public T[] FindObjects<T>() where T : Object
        {
            if (includeDisabledObjects) {
                List<T> meshfilters = new List<T> ();
                GameObject[] allGo = GetAllRootGameObjects();
                foreach (GameObject go in allGo) {
                    Transform[] tgo = go.GetComponentsInChildren<Transform> (true).ToArray ();
                    foreach (Transform tr in tgo) {
                        if (tr.GetComponent<T> ())
                            meshfilters.Add (tr.GetComponent<T> ());
                    }
                }
                return (T[])meshfilters.ToArray ();
            }
            else
                return (T[])FindObjectsOfType(typeof(T));
        }
        
        public List<GameObject> FindAllGameObjects()
        {
            if (_cachedAllGameObjects != null)
            {
                return _cachedAllGameObjects;
            }

            List<GameObject> allObjects = new List<GameObject>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    allObjects.AddRange(scene.GetRootGameObjects());
                }
            }

            List<GameObject> dontDestroyOnLoadObjects = GetDontDestroyOnLoadRoots();
            allObjects.AddRange(dontDestroyOnLoadObjects);

            if (includeDisabledObjects)
            {
                List<GameObject> allObjectsWithChildren = new List<GameObject>();
                foreach (GameObject go in allObjects)
                {
                    allObjectsWithChildren.AddRange(go.GetComponentsInChildren<Transform>(true)
                        .Select(t => t.gameObject));
                }
                _cachedAllGameObjects = allObjectsWithChildren;
                return allObjectsWithChildren;
            }
            else
            {
                List<GameObject> allActiveObjectsWithChildren = new List<GameObject>();
                foreach (GameObject go in allObjects)
                {
                    if (go.activeInHierarchy)
                    {
                        allActiveObjectsWithChildren.AddRange(go.GetComponentsInChildren<Transform>(false)
                            .Where(t => t.gameObject.activeInHierarchy)
                            .Select(t => t.gameObject));
                    }
                }
                _cachedAllGameObjects = allActiveObjectsWithChildren;
                return allActiveObjectsWithChildren;
            }
        }
    }
}