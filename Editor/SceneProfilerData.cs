using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SceneProfiler.Editor
{
    public class TextureDetails : IEquatable<TextureDetails>
    {
        public bool isCubeMap;
        public float memSizeKB;
        public Texture texture;
        public TextureFormat format;
        public int mipMapCount;
        public List<Object> FoundInMaterials = new List<Object>();
        public List<Object> FoundInRenderers = new List<Object>();
        public List<Object> FoundInAnimators = new List<Object>();
        public List<Object> FoundInScripts = new List<Object>();
        public List<Object> FoundInGraphics = new List<Object>();
        public List<Object> FoundInButtons = new List<Object>();
        public bool isSky;
        public bool instance;
        public bool isgui;
        public TextureDetails() { }

        public bool Equals(TextureDetails other)
        {
            if (other == null || texture == null || other.texture == null)
                return false;

            return texture.GetNativeTexturePtr() == other.texture.GetNativeTexturePtr();
        }

        public override int GetHashCode()
        {
            return (int)texture.GetNativeTexturePtr();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TextureDetails);
        }
    }

    public class MaterialDetails
    {
        public Material material;
        public List<Renderer> FoundInRenderers = new List<Renderer>();
        public List<Graphic> FoundInGraphics = new List<Graphic>();
        public bool instance;
        public bool isgui;
        public bool isSky;

        public MaterialDetails()
        {
            instance = false;
            isgui = false;
            isSky = false;
        }
    }

    public class MeshDetails
    {
        public Mesh mesh;
        public List<MeshFilter> FoundInMeshFilters = new List<MeshFilter>();
        public List<SkinnedMeshRenderer> FoundInSkinnedMeshRenderer = new List<SkinnedMeshRenderer>();
        public List<GameObject> StaticBatchingEnabled = new List<GameObject>();
        public bool instance;

        public MeshDetails()
        {
            instance = false;
        }
    }

    public class AudioClipDetails
    {
        public AudioClip clip;
        public List<AudioSource> FoundInAudioSources = new List<AudioSource>();

        public AudioClipDetails() { }
    }

    public class Missing
    {
        public Transform Object;
        public string type;
        public string name;
    }

    public class LightDetails
    {
        public Light light;
        public bool isEnabled;
        public LightShadows shadowType;
        public bool isActive;
    }

    public class ParticleSystemDetails
    {
        public ParticleSystem particleSystem;
        public List<GameObject> FoundInGameObjects = new List<GameObject>();
        public Material material;
        public int maxParticles;
        public int activeParticles;
    }

    public class PhysicsObjectDetails
    {
        public Rigidbody rigidbody;
        public GameObject gameObject;
        public bool isActive;
        public bool isKinematic;
        public float mass;
        public float drag;
        public float angularDrag;
        public RigidbodyInterpolation interpolation;
        public CollisionDetectionMode collisionDetectionMode;
        public string colliderType;

        public PhysicsObjectDetails(Rigidbody rb)
        {
            rigidbody = rb;
            gameObject = rb.gameObject;
            isActive = rb.gameObject.activeInHierarchy;
            isKinematic = rb.isKinematic;
            mass = rb.mass;
            drag = rb.drag;
            angularDrag = rb.angularDrag;
            interpolation = rb.interpolation;
            collisionDetectionMode = rb.collisionDetectionMode;
            colliderType = GetColliderType(rb);
        }

        private string GetColliderType(Rigidbody rb)
        {
            Collider collider = rb.GetComponent<Collider>();
            if (collider is BoxCollider) return "Box";
            if (collider is SphereCollider) return "Sphere";
            if (collider is CapsuleCollider) return "Capsule";
            if (collider is MeshCollider) return "Mesh";
            return "Other";
        }

    }

    public class SceneWarningDetails
    {
        public string Message { get; private set; }
        public MessageType Type { get; private set; }

        public SceneWarningDetails(string message, MessageType type)
        {
            Message = message;
            Type = type;
        }
    }
}