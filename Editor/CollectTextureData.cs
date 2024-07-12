using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SceneProfiler.Editor
{
    public class CollectTextureData
    {
        private SceneProfiler _sceneProfiler;
    
        public CollectTextureData(SceneProfiler sceneProfiler)
        {
            _sceneProfiler = sceneProfiler;
        }
    
        float GetBitsPerPixel(TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.Alpha8: return 8;
                case TextureFormat.ARGB4444: return 16;
                case TextureFormat.RGBA4444: return 16;
                case TextureFormat.RGB24: return 24;
                case TextureFormat.RGBA32: return 32;
                case TextureFormat.ARGB32: return 32;
                case TextureFormat.RGB565: return 16;
                case TextureFormat.DXT1: return 4;
                case TextureFormat.DXT1Crunched: return 4;
                case TextureFormat.DXT5: return 8;
                case TextureFormat.DXT5Crunched: return 8;
                case TextureFormat.BC4: return 4;
                case TextureFormat.BC7: return 8;
                case TextureFormat.PVRTC_RGB2: return 2;
                case TextureFormat.PVRTC_RGBA2: return 2;
                case TextureFormat.PVRTC_RGB4: return 4;
                case TextureFormat.PVRTC_RGBA4: return 4;
                case TextureFormat.ETC_RGB4: return 4;
                case TextureFormat.ETC_RGB4Crunched: return 4;
                case TextureFormat.ETC2_RGBA8: return 8;
                case TextureFormat.ETC2_RGB: return 4;
                case TextureFormat.ETC2_RGBA8Crunched: return 4;
                case TextureFormat.EAC_R: return 4;
                case TextureFormat.BGRA32: return 32;
                case TextureFormat.ASTC_4x4: return 8;
                case TextureFormat.ASTC_5x5: return 5.12f;
                case TextureFormat.ASTC_6x6: return 3.56f;
                case TextureFormat.ASTC_8x8: return 2;
                case TextureFormat.ASTC_10x10: return 1.28f;
                case TextureFormat.ASTC_12x12: return 0.89f;
            }
            return 0;
        }

        float CalculateTextureSizeBytes(Texture tTexture)
        {
            int tWidth = tTexture.width;
            int tHeight = tTexture.height;
            float tSize = 0;

            if (tTexture is Texture2D)
            {
                Texture2D tTex2D = tTexture as Texture2D;
                float bitsPerPixel = GetBitsPerPixel(tTex2D.format);
                int mipMapCount = tTex2D.mipmapCount;
                int mipLevel = 1;

                while (mipLevel <= mipMapCount)
                {
                    tSize += tWidth * tHeight * bitsPerPixel / 8;
                    tWidth = Mathf.Max(1, tWidth / 2);
                    tHeight = Mathf.Max(1, tHeight / 2);
                    mipLevel++;
                }
            }
            else if (tTexture is Texture2DArray)
            {
                Texture2DArray tTex2DArray = tTexture as Texture2DArray;
                float bitsPerPixel = GetBitsPerPixel(tTex2DArray.format);
                int mipMapCount = 10; // Assuming a fixed mip map count for Texture2DArray
                int mipLevel = 1;

                while (mipLevel <= mipMapCount)
                {
                    tSize += tWidth * tHeight * bitsPerPixel / 8;
                    tWidth = Mathf.Max(1, tWidth / 2);
                    tHeight = Mathf.Max(1, tHeight / 2);
                    mipLevel++;
                }
                tSize *= tTex2DArray.depth;
            }
            else if (tTexture is Cubemap)
            {
                Cubemap tCubemap = tTexture as Cubemap;
                float bitsPerPixel = GetBitsPerPixel(tCubemap.format);
                tSize = tWidth * tHeight * 6 * bitsPerPixel / 8;
            }

            return tSize;
        }
    
        TextureDetails FindTextureDetails(Texture tTexture)
        {
            foreach (TextureDetails tTextureDetails in _sceneProfiler.ActiveTextures)
            {
                if (tTextureDetails.texture == tTexture) return tTextureDetails;
            }
            return null;
        }
    
        public void CheckRenderers()
        {
            Renderer[] renderers = _sceneProfiler.FindObjects<Renderer>();
        

            foreach (Renderer renderer in renderers)
            {
                CheckSpriteRenderer(renderer);
            }
        }
    
        private void CheckSpriteRenderer(Renderer renderer)
        {
            if (renderer is SpriteRenderer tSpriteRenderer)
            {
                if (tSpriteRenderer.sprite != null)
                {
                    AddSpriteTextureDetails(tSpriteRenderer, renderer);
                }
                else
                {
                    _sceneProfiler.AddMissingSprite(tSpriteRenderer);
                }
            }
        }
    
        private void AddSpriteTextureDetails(SpriteRenderer tSpriteRenderer, Renderer renderer)
        {
            var tSpriteTextureDetail = GetTextureDetail(tSpriteRenderer.sprite.texture, renderer);
            if (!_sceneProfiler.ActiveTextures.Contains(tSpriteTextureDetail))
            {
                _sceneProfiler.ActiveTextures.Add(tSpriteTextureDetail);
            }

            var secondarySpriteTextureResult = new SecondarySpriteTexture[tSpriteRenderer.sprite.GetSecondaryTextureCount()];
            tSpriteRenderer.sprite.GetSecondaryTextures(secondarySpriteTextureResult);
            foreach (var sst in secondarySpriteTextureResult)
            {
                var tSpriteSecondaryTextureDetail = GetTextureDetail(sst.texture, renderer);
                if (!_sceneProfiler.ActiveTextures.Contains(tSpriteSecondaryTextureDetail))
                {
                    _sceneProfiler.ActiveTextures.Add(tSpriteSecondaryTextureDetail);
                }
            }

            if (!_sceneProfiler.ActiveTextures.Contains(tSpriteTextureDetail))
            {
                _sceneProfiler.ActiveTextures.Add(tSpriteTextureDetail);
            }
        }
    
        public void CheckLightmaps()
        {
            if (!_sceneProfiler.IncludeLightmapTextures) return;

            LightmapData[] lightmapTextures = LightmapSettings.lightmaps;

            foreach (LightmapData lightmapData in lightmapTextures)
            {
                if (lightmapData.lightmapColor != null)
                {
                    var textureDetail = GetTextureDetail(lightmapData.lightmapColor);

                    if (!_sceneProfiler.ActiveTextures.Contains(textureDetail))
                        _sceneProfiler.ActiveTextures.Add(textureDetail);
                }

                if (lightmapData.lightmapDir != null)
                {
                    var textureDetail = GetTextureDetail(lightmapData.lightmapDir);

                    if (!_sceneProfiler.ActiveTextures.Contains(textureDetail))
                        _sceneProfiler.ActiveTextures.Add(textureDetail);
                }

                if (lightmapData.shadowMask != null)
                {
                    var textureDetail = GetTextureDetail(lightmapData.shadowMask);

                    if (!_sceneProfiler.ActiveTextures.Contains(textureDetail))
                        _sceneProfiler.ActiveTextures.Add(textureDetail);
                }
            }
        }
    
        public void CheckGUIElements()
        {
            if (!_sceneProfiler.IncludeGuiElements) return;

            Graphic[] graphics = _sceneProfiler.FindObjects<Graphic>();

            foreach (Graphic graphic in graphics)
            {
                if (graphic.mainTexture)
                {
                    var tSpriteTextureDetail = GetTextureDetail(graphic.mainTexture, graphic);
                    if (!_sceneProfiler.ActiveTextures.Contains(tSpriteTextureDetail))
                    {
                        _sceneProfiler.ActiveTextures.Add(tSpriteTextureDetail);
                    }
                }
            }

            Button[] buttons = _sceneProfiler.FindObjects<Button>();
            foreach (Button button in buttons)
            {
                CheckButtonSpriteState(button, button.spriteState.disabledSprite);
                CheckButtonSpriteState(button, button.spriteState.highlightedSprite);
                CheckButtonSpriteState(button, button.spriteState.pressedSprite);
            }
        }
    
        public void CheckMaterials()
        {
            foreach (MaterialDetails tMaterialDetails in _sceneProfiler.ActiveMaterials)
            {
                Material tMaterial = tMaterialDetails.material;
                if (tMaterial != null)
                {
                    var dependencies = EditorUtility.CollectDependencies(new UnityEngine.Object[] { tMaterial });
                    foreach (Object obj in dependencies)
                    {
                        if (obj is Texture)
                        {
                            Texture tTexture = obj as Texture;
                            var tTextureDetail = GetTextureDetail(tTexture, tMaterial, tMaterialDetails);
                            tTextureDetail.isSky = tMaterialDetails.isSky;
                            tTextureDetail.instance = tMaterialDetails.instance;
                            tTextureDetail.isgui = tMaterialDetails.isgui;
                            _sceneProfiler.ActiveTextures.Add(tTextureDetail);
                        }
                    }

                    if (tMaterial.HasProperty("_MainTex"))
                    {
                        if (tMaterial.mainTexture != null && !dependencies.Contains(tMaterial.mainTexture))
                        {
                            var tTextureDetail = GetTextureDetail(tMaterial.mainTexture, tMaterial, tMaterialDetails);
                            _sceneProfiler.ActiveTextures.Add(tTextureDetail);
                        }
                    }
                }
            }
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
                var guids = AssetDatabase.FindAssets("t:Texture", folders.ToArray());
                if (guids.Length != 0)
                {
                    foreach (var guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        var item = AssetDatabase.LoadAssetAtPath<Texture>(path);
                        var textureDetail = GetTextureDetail(item);
                        if (!_sceneProfiler.ActiveTextures.Contains(textureDetail))
                            _sceneProfiler.ActiveTextures.Add(textureDetail);
                    }
                }
            }
        }
    
        public void CheckSpriteAnimations()
        {
            if (!_sceneProfiler.IncludeSpriteAnimations) return;

            Animator[] animators = _sceneProfiler.FindObjects<Animator>();
            foreach (Animator anim in animators)
            {
                if (!anim.gameObject.activeInHierarchy || !anim.enabled || anim.runtimeAnimatorController == null)
                    continue;

                UnityEditor.Animations.AnimatorController ac = anim.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;

                if (ac == null || ac.layers == null || ac.layers.Length == 0)
                    continue;

                for (int x = 0; x < anim.layerCount; x++)
                {
                    UnityEditor.Animations.AnimatorStateMachine sm = ac.layers[x].stateMachine;
                    if (sm == null)
                        continue;

                    int cnt = sm.states.Length;

                    for (int i = 0; i < cnt; i++)
                    {
                        UnityEditor.Animations.AnimatorState state = sm.states[i].state;
                        if (state == null)
                            continue;

                        Motion m = state.motion;
                        if (m != null)
                        {
                            AnimationClip clip = m as AnimationClip;

                            if (clip != null)
                            {
                                EditorCurveBinding[] ecbs = AnimationUtility.GetObjectReferenceCurveBindings(clip);

                                foreach (EditorCurveBinding ecb in ecbs)
                                {
                                    if (ecb.propertyName == "m_Sprite")
                                    {
                                        foreach (ObjectReferenceKeyframe keyframe in AnimationUtility.GetObjectReferenceCurve(clip, ecb))
                                        {
                                            Sprite tSprite = keyframe.value as Sprite;

                                            if (tSprite != null)
                                            {
                                                var tTextureDetail = GetTextureDetail(tSprite.texture, anim);
                                                if (!_sceneProfiler.ActiveTextures.Contains(tTextureDetail))
                                                {
                                                    _sceneProfiler.ActiveTextures.Add(tTextureDetail);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    
        public void CheckMaterialTextures(Material tMaterial)
        {
            if (tMaterial.mainTexture)
            {
                var tSpriteTextureDetail = GetTextureDetail(tMaterial.mainTexture);
                if (!_sceneProfiler.ActiveTextures.Contains(tSpriteTextureDetail))
                {
                    _sceneProfiler.ActiveTextures.Add(tSpriteTextureDetail);
                }
            }
        }

        public void CheckMaterialDependencies(Material tMaterial)
        {
            var dependencies = EditorUtility.CollectDependencies(new UnityEngine.Object[] { tMaterial });
            foreach (Object obj in dependencies)
            {
                if (obj is Texture)
                {
                    Texture tTexture = obj as Texture;
                    var tTextureDetail = GetTextureDetail(tTexture, tMaterial, FindMaterialDetails(tMaterial));
                    if (!_sceneProfiler.ActiveTextures.Contains(tTextureDetail))
                    {
                        _sceneProfiler.ActiveTextures.Add(tTextureDetail);
                    }
                }
            }
        }
    
        public void CheckSpriteReferences(FieldInfo field, MonoBehaviour script)
        {
            if (field.FieldType == typeof(Sprite))
            {
                Sprite tSprite = field.GetValue(script) as Sprite;
                if (tSprite != null)
                {
                    var tSpriteTextureDetail = GetTextureDetail(tSprite.texture, script);
                    if (!_sceneProfiler.ActiveTextures.Contains(tSpriteTextureDetail))
                    {
                        _sceneProfiler.ActiveTextures.Add(tSpriteTextureDetail);
                    }
                }
            }
        }
    
        MaterialDetails FindMaterialDetails(Material tMaterial)
        {
            foreach (MaterialDetails tMaterialDetails in _sceneProfiler.ActiveMaterials)
            {
                if (tMaterialDetails.material == tMaterial) return tMaterialDetails;
            }
            return null;
        }
    
        private void CheckButtonSpriteState(Button button, Sprite sprite)
        {
            if (sprite == null) return;

            var texture = sprite.texture;
            var tButtonTextureDetail = GetTextureDetail(texture, button);
            if (!_sceneProfiler.ActiveTextures.Contains(tButtonTextureDetail))
            {
                _sceneProfiler.ActiveTextures.Add(tButtonTextureDetail);
            }
        }
    
        private TextureDetails GetTextureDetail(Texture tTexture, Material tMaterial, MaterialDetails tMaterialDetails)
        {
            TextureDetails tTextureDetails = GetTextureDetail(tTexture);

            tTextureDetails.FoundInMaterials.Add(tMaterial);
            foreach (Renderer renderer in tMaterialDetails.FoundInRenderers)
            {
                if (!tTextureDetails.FoundInRenderers.Contains(renderer)) tTextureDetails.FoundInRenderers.Add(renderer);
            }
            foreach (Graphic graphic in tMaterialDetails.FoundInGraphics)
            {
                if (!tTextureDetails.FoundInGraphics.Contains(graphic)) tTextureDetails.FoundInGraphics.Add(graphic);
            }
            return tTextureDetails;
        }

        private TextureDetails GetTextureDetail(Texture tTexture, Renderer renderer)
        {
            TextureDetails tTextureDetails = GetTextureDetail(tTexture);

            tTextureDetails.FoundInRenderers.Add(renderer);
            return tTextureDetails;
        }

        private TextureDetails GetTextureDetail(Texture tTexture, Animator animator)
        {
            TextureDetails tTextureDetails = GetTextureDetail(tTexture);

            tTextureDetails.FoundInAnimators.Add(animator);
            return tTextureDetails;
        }

        private TextureDetails GetTextureDetail(Texture tTexture, Graphic graphic)
        {
            TextureDetails tTextureDetails = GetTextureDetail(tTexture);

            tTextureDetails.FoundInGraphics.Add(graphic);
            return tTextureDetails;
        }

        private TextureDetails GetTextureDetail(Texture tTexture, MonoBehaviour script)
        {
            TextureDetails tTextureDetails = GetTextureDetail(tTexture);

            tTextureDetails.FoundInScripts.Add(script);
            return tTextureDetails;
        }

        private TextureDetails GetTextureDetail(Texture tTexture, Button button) 
        {
            TextureDetails tTextureDetails = GetTextureDetail(tTexture);

            if (!tTextureDetails.FoundInButtons.Contains(button))
            {
                tTextureDetails.FoundInButtons.Add(button);
            }
            return tTextureDetails;
        }

        private TextureDetails GetTextureDetail(Texture tTexture)
        {
            TextureDetails tTextureDetails = FindTextureDetails(tTexture);
            if (tTextureDetails == null)
            {
                tTextureDetails = new TextureDetails();
                tTextureDetails.texture = tTexture;
                tTextureDetails.isCubeMap = tTexture is Cubemap;

                float memSize = CalculateTextureSizeBytes(tTexture);

                TextureFormat tFormat = TextureFormat.RGBA32;
                int tMipMapCount = 1;
                if (tTexture is Texture2D)
                {
                    tFormat = (tTexture as Texture2D).format;
                    tMipMapCount = (tTexture as Texture2D).mipmapCount;
                }
                if (tTexture is Cubemap)
                {
                    tFormat = (tTexture as Cubemap).format;
                    memSize = 8 * tTexture.height * tTexture.width;
                }
                if(tTexture is Texture2DArray){
                    tFormat = (tTexture as Texture2DArray).format;
                    tMipMapCount = 10;
                }

                tTextureDetails.memSizeKB = memSize / 1024;
                tTextureDetails.format = tFormat;
                tTextureDetails.mipMapCount = tMipMapCount;

            }

            return tTextureDetails;
        }
    }
}