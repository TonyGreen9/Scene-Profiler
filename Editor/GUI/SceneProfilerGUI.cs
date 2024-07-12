using UnityEditor;
using UnityEngine;

namespace SceneProfiler.Editor.GUI
{
    public class SceneProfilerGUI
    {
        private SceneProfiler _profiler;
        private Color _defColor;
        private Vector2 _scrollPosition;
        private Vector2 _particleSystemScrollPosition;
        private Vector2 _warningsScrollPosition;
        private GUIStyle _evenRowStyle;
        private GUIStyle _oddRowStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _buttonStyle;
        private readonly float buttonHeight = 50f;
        private bool _showWarnings = false;
        private float _rowHeight = 20f;

        private ParticleSystemsProfilerGUI _particleSystemsProfilerGUI;
        private LightsProfilerGUI _lightsProfilerGUI;
        private TexturesProfilerGUI _texturesProfilerGUI;
        private PhysicsProfilerGUI _physicsProfilerGUI;
        private MeshesProfilerGUI _meshesProfilerGUI;
        private MaterialsProfilerGUI _materialsProfilerGUI;
        private MissingProfilerGUI _missingProfilerGUI;
        private AudioClipsProfilerGUI _audioClipsProfilerGUI;
        private WarningsGUI _warningsGUI;
    

        public SceneProfilerGUI(SceneProfiler profiler)
        {
            this._profiler = profiler;
            _particleSystemsProfilerGUI = new ParticleSystemsProfilerGUI(profiler, _defColor);
            _lightsProfilerGUI = new LightsProfilerGUI(profiler, _defColor);
            _texturesProfilerGUI = new TexturesProfilerGUI(profiler, _defColor, () => _rowHeight);
            _physicsProfilerGUI = new PhysicsProfilerGUI(profiler, _defColor);
            _meshesProfilerGUI = new MeshesProfilerGUI(profiler, _defColor);
            _materialsProfilerGUI = new MaterialsProfilerGUI(profiler, _defColor, () => _rowHeight);
            _missingProfilerGUI = new MissingProfilerGUI(profiler, _defColor);
            _audioClipsProfilerGUI = new AudioClipsProfilerGUI(profiler, _defColor, () => _rowHeight);
            _warningsGUI = new WarningsGUI(profiler);
        }
    
        public SceneProfiler Profiler => _profiler;

        private GUIStyle ButtonStyle
        {
            get
            {
                if (_buttonStyle == null)
                {
                    _buttonStyle = new GUIStyle(EditorStyles.toolbarButton)
                    {
                        fixedHeight = buttonHeight
                    };
                }
                return _buttonStyle;
            }
        }

        public void DrawGUI()
        {
            _defColor = UnityEngine.GUI.color;

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            UnityEngine.GUI.color = new Color(1.4f, 1.4f, 1.4f);

            if (GUILayout.Button(new GUIContent("Collect Data", "Collects data from all objects in the current scene"), EditorStyles.toolbarButton))
            {
                _profiler.CollectData();
            }

            UnityEngine.GUI.color = _defColor;

            if (GUILayout.Button(new GUIContent("Clear", "Clears the collected data and refreshes the interface"), EditorStyles.toolbarButton))
            {
                _profiler.ClearAndRepaint();
            }

            GUILayout.FlexibleSpace();
        
            if (_profiler.ActiveInspectType == SceneProfiler.InspectType.Textures && _texturesProfilerGUI != null && _profiler.ActiveTextures.Count > 0 ||
                _profiler.ActiveInspectType == SceneProfiler.InspectType.Materials && _materialsProfilerGUI != null && _profiler.ActiveMaterials.Count > 0 ||
                _profiler.ActiveInspectType == SceneProfiler.InspectType.AudioClips && _audioClipsProfilerGUI != null && _profiler.ActiveClipDetails.Count > 0)
            {
                _rowHeight = GUILayout.HorizontalSlider(_rowHeight, 20, 200, GUILayout.Width(100));
            }

            Rect settingsButtonRect = GUILayoutUtility.GetRect(new GUIContent("Settings", "Opens the settings menu for the profiler"), EditorStyles.toolbarButton);
            if (UnityEngine.GUI.Button(settingsButtonRect, new GUIContent("Settings", "Opens the settings menu for the profiler"), EditorStyles.toolbarButton))
            {
                ShowSettingsMenu(settingsButtonRect);
            }

            DrawWarningButton();

            GUILayout.EndHorizontal();

            UnityEngine.GUI.color = new Color(1.4f, 1.4f, 1.4f);

            GUILayout.BeginHorizontal();
            float windowWidth = EditorGUIUtility.currentViewWidth;
            int buttonCount = System.Enum.GetValues(typeof(SceneProfiler.InspectType)).Length;
            float buttonWidth = windowWidth / buttonCount;
            GUILayoutOption buttonHeightOption = GUILayout.Height(buttonHeight + 1);
            GUILayoutOption buttonWidthOption = GUILayout.Width(buttonWidth);
        
            string[] toolbarLabels = {
                $"Textures ({_profiler.ActiveTextures.Count})\n{_profiler.FormatSizeString(_profiler.TotalTextureMemory)}",
                $"Materials ({_profiler.ActiveMaterials.Count})\n{_profiler.UniqueShadersCount} shaders",
                $"Meshes ({_profiler.ActiveMeshDetails.Count})\n{_profiler.TotalMeshVertices} vertices",
                $"Audio ({_profiler.ActiveClipDetails.Count})",
                $"Missing ({_profiler.MissingObjects.Count})",
                $"Particles ({_profiler.ActiveParticleSystems.Count})",
                $"Lights ({_profiler.ActiveLights.Count})",
                $"Physics ({_profiler.ActivePhysicsObjects.Count})"
            };

            for (int i = 0; i < toolbarLabels.Length; i++)
            {
                if (GUILayout.Toggle(_profiler.ActiveInspectType == (SceneProfiler.InspectType)i, toolbarLabels[i], ButtonStyle, buttonWidthOption, buttonHeightOption))
                {
                    _profiler.ActiveInspectType = (SceneProfiler.InspectType)i;
                }
            }

            UnityEngine.GUI.color = _defColor;

            GUILayout.EndHorizontal();

            _profiler.ctrlPressed = Event.current.control || Event.current.command;

            if (_showWarnings)
            {
                _warningsGUI.DrawWarnings(ref _warningsScrollPosition);
            }

            switch (_profiler.ActiveInspectType)
            {
                case SceneProfiler.InspectType.Textures:
                    _texturesProfilerGUI.ListTextures();
                    break;
                case SceneProfiler.InspectType.Materials:
                    _materialsProfilerGUI.ListMaterials();
                    break;
                case SceneProfiler.InspectType.Meshes:
                    _meshesProfilerGUI.ListMeshes();
                    break;
                case SceneProfiler.InspectType.Missing:
                    _missingProfilerGUI.ListMissing();
                    break;
                case SceneProfiler.InspectType.AudioClips:
                    _audioClipsProfilerGUI.ListAudioClips();
                    break;
                case SceneProfiler.InspectType.Particles:
                    _particleSystemsProfilerGUI.ListParticleSystems();
                    break;
                case SceneProfiler.InspectType.Lights:
                    _lightsProfilerGUI.ListLights();
                    break;
                case SceneProfiler.InspectType.Physics:
                    _physicsProfilerGUI.ListPhysicsObjects();
                    break;
            }
        }
    
        private void ShowSettingsMenu(Rect buttonRect)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Include disabled objects"), _profiler.includeDisabledObjects, () => _profiler.includeDisabledObjects = !_profiler.includeDisabledObjects);
            menu.AddItem(new GUIContent("Look in sprite animations (Textures)"), _profiler.IncludeSpriteAnimations, () => _profiler.IncludeSpriteAnimations = !_profiler.IncludeSpriteAnimations);
            menu.AddItem(new GUIContent("Look in behavior fields (Textures, mats, meshes)"), _profiler.IncludeScriptReferences, () => _profiler.IncludeScriptReferences = !_profiler.IncludeScriptReferences);
            menu.AddItem(new GUIContent("Look in GUI elements (Textures, mats)"), _profiler.IncludeGuiElements, () => _profiler.IncludeGuiElements = !_profiler.IncludeGuiElements);
            menu.AddItem(new GUIContent("Look in Lightmap textures"), _profiler.IncludeLightmapTextures, () => _profiler.IncludeLightmapTextures = !_profiler.IncludeLightmapTextures);
            menu.AddItem(new GUIContent("Look in Selected Folders (Textures, Audio)"), _profiler.IncludeSelectedFolder, () => _profiler.IncludeSelectedFolder = !_profiler.IncludeSelectedFolder);

            menu.DropDown(new Rect(buttonRect.x, buttonRect.yMax, 0, 0));
        }

        private void DrawWarningButton()
        {
            GUIContent warningButtonContent = new GUIContent
            {
                image = _warningsGUI.GetWarningButtonIcon(),
                tooltip = "Toggle the display of warning messages"
            };

            if (GUILayout.Button(warningButtonContent, EditorStyles.toolbarButton))
            {
                _showWarnings = !_showWarnings;
            }
        }
    }
}