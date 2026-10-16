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

        /// <summary>
        /// Coloca un modelo importado (OBJ con sus materiales) como hijo. Toda su jerarquía recibe la capa, las marcas
        /// de geometría estática y, si se pide, deja de proyectar sombras (decorado lejano: más barato de dibujar).
        /// Los modelos no traen colisiones: quien los usa añade colisiones sencillas (<see cref="AddBoxCollider"/>).
        /// </summary>
        public static GameObject PlaceModel(string modelPath, string name, Transform parent, Vector3 localPosition,
            Quaternion localRotation, Vector3 localScale, int layer, bool isStatic, bool castShadows = true)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null)
                throw new System.InvalidOperationException($"No se encuentra el modelo {modelPath}.");

            var go = (GameObject)PrefabUtility.InstantiatePrefab(model, parent);
            go.name = name;
            go.transform.SetLocalPositionAndRotation(localPosition, localRotation);
            go.transform.localScale = localScale;
            foreach (var child in go.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = layer;
                if (isStatic)
                    GameObjectUtility.SetStaticEditorFlags(child.gameObject, STATIC_GEOMETRY);
            }
            if (!castShadows)
            {
                foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            return go;
        }

        /// <summary>Colisión invisible en forma de caja (los modelos detallados no llevan MeshCollider).</summary>
        public static BoxCollider AddBoxCollider(string name, Transform parent, Vector3 localCenter, Vector3 size, int layer,
            Quaternion? localRotation = null)
        {
            var go = new GameObject(name) { layer = layer };
            go.transform.SetParent(parent, false);
            go.transform.SetLocalPositionAndRotation(localCenter, localRotation ?? Quaternion.identity);
            var box = go.AddComponent<BoxCollider>();
            box.size = size;
            GameObjectUtility.SetStaticEditorFlags(go, STATIC_GEOMETRY);
            return box;
        }

        /// <summary>Colisión invisible cilíndrica (convexa), centrada en <paramref name="localCenter"/>.</summary>
        public static GameObject AddCylinderCollider(string name, Transform parent, Vector3 localCenter, float diameter,
            float height, int layer)
        {
            var go = CreatePrimitive(PrimitiveType.Cylinder, name, parent, localCenter,
                new Vector3(diameter, height * 0.5f, diameter), null, layer, true);
            Object.DestroyImmediate(go.GetComponent<MeshRenderer>());
            Object.DestroyImmediate(go.GetComponent<MeshFilter>());
            return go;
        }

        /// <summary>Material transparente URP (p. ej. el fondo oscuro de la pantalla, que deja ver el decorado detrás).</summary>
        public static Material GetOrCreateTransparentMaterial(string path, Color color, float smoothness = 0.5f)
        {
            var material = GetOrCreateMaterial(path, color, smoothness);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            return material;
        }

        public static GameObject CreateEmpty(string name, Transform parent, Vector3 localPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            return go;
        }

        /// <summary>Texto 3D del plató. Por defecto en Poppins (<see cref="StageFonts.Body"/>); titulares, con Lilita One.</summary>
        public static TextMeshPro CreateWorldText(string name, Transform parent, string text, float fontSize,
            Color color, Vector3 localPosition, Quaternion localRotation, Vector2 size, TMP_FontAsset font = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = localRotation;

            var tmp = go.AddComponent<TextMeshPro>();
            tmp.font = font != null ? font : StageFonts.Body();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.rectTransform.sizeDelta = size;
            return tmp;
        }

        /// <summary>
        /// Guarda la jerarquía como prefab y destruye el objeto temporal de la escena. Si es interfaz (tiene Canvas),
        /// antes le aplica el tema del juego (<see cref="UiTheme"/>).
        /// </summary>
        public static GameObject SaveAsPrefab(GameObject root, string path)
        {
            if (root.GetComponentInChildren<Canvas>(true) != null)
                UiTheme.Apply(root);
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
