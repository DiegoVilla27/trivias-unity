using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Trivias.Shared.Editor
{
    /// <summary>
    /// Utilidades para los constructores de tiempo de edición: crean o actualizan assets de forma idempotente
    /// (ejecutar un constructor dos veces deja el mismo resultado, sin duplicados).
    /// </summary>
    public static class EditorAssets
    {
        private const string URP_LIT_SHADER = "Universal Render Pipeline/Lit";
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");
        private static readonly int MetallicId = Shader.PropertyToID("_Metallic");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        public const StaticEditorFlags STATIC_GEOMETRY =
            StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI |
            StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic |
            StaticEditorFlags.ReflectionProbeStatic;

        public static void EnsureFolder(string path)
        {
            path = path.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        public static T GetOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            EnsureFolder(Path.GetDirectoryName(path));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        public static Material GetOrCreateMaterial(string path, Color color, float smoothness = 0.35f,
            float metallic = 0f, Color? emission = null)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                EnsureFolder(Path.GetDirectoryName(path));
                material = new Material(Shader.Find(URP_LIT_SHADER));
                AssetDatabase.CreateAsset(material, path);
            }

            material.SetColor(BaseColorId, color);
            material.SetFloat(SmoothnessId, smoothness);
            material.SetFloat(MetallicId, metallic);
            if (emission.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                material.SetColor(EmissionColorId, emission.Value);
            }
            else
            {
                material.DisableKeyword("_EMISSION");
            }

            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        public static GameObject CreatePrimitive(PrimitiveType type, string name, Transform parent,
            Vector3 localPosition, Vector3 localScale, Material material, int layer, bool isStatic,
            bool keepCollider = true)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            go.layer = layer;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;

            if (!keepCollider)
            {
                Object.DestroyImmediate(go.GetComponent<Collider>());
            }
            else if (type == PrimitiveType.Cylinder)
            {
                // El CapsuleCollider del cilindro se vuelve una esfera si el cilindro es ancho y bajo.
                Object.DestroyImmediate(go.GetComponent<Collider>());
                go.AddComponent<MeshCollider>().convex = true;
            }
            if (isStatic)
                GameObjectUtility.SetStaticEditorFlags(go, STATIC_GEOMETRY);
            return go;
        }

        public static GameObject CreateEmpty(string name, Transform parent, Vector3 localPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            return go;
        }

        public static TextMeshPro CreateWorldText(string name, Transform parent, string text, float fontSize,
            Color color, Vector3 localPosition, Quaternion localRotation, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = localRotation;

            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.rectTransform.sizeDelta = size;
            return tmp;
        }

        /// <summary>Guarda la jerarquía como prefab y destruye el objeto temporal de la escena.</summary>
        public static GameObject SaveAsPrefab(GameObject root, string path)
        {
            EnsureFolder(Path.GetDirectoryName(path));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        /// <summary>Crea o actualiza una variante de prefab y permite modificarla antes de guardar.</summary>
        public static GameObject SaveVariant(GameObject basePrefab, string path, System.Action<GameObject> configure)
        {
            EnsureFolder(Path.GetDirectoryName(path));
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            configure(instance);
            var variant = PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            return variant;
        }
    }
}
