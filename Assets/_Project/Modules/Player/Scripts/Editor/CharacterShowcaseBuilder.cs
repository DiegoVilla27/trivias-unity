using Trivias.Player.Components;
using Trivias.Shared.Editor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Trivias.Player.Editor
{
    /// <summary>
    /// Construye el expositor del personaje para el menú: el mismo modelo que el concursante sobre una peana, con su
    /// cámara (que pinta en la imagen del menú, con fondo transparente; la imagen se crea al cargar la escena) y dos luces suaves. Va muy por debajo del plató
    /// para que ni se vea desde el juego ni le afecten sus luces de punto.
    /// </summary>
    public static class CharacterShowcaseBuilder
    {
        public const string PREFAB_PATH = PlayerPrefabBuilder.MODULE_ROOT + "/Prefabs/CharacterShowcase.prefab";
        private const string PEDESTAL_MATERIAL = PlayerPrefabBuilder.MODULE_ROOT + "/Materials/ShowcasePedestal.mat";
        private const string PEDESTAL_RIM_MATERIAL = PlayerPrefabBuilder.MODULE_ROOT + "/Materials/ShowcasePedestalRim.mat";

        /// <summary>Lejos del plató (que está en y = 0).</summary>
        public static readonly Vector3 Position = new(0f, -300f, 0f);
        private static readonly Vector3 CameraPosition = new(0f, 1.15f, 4.9f);
        private static readonly Vector3 LookAt = new(0f, 0.9f, 0f);
        private const float FIELD_OF_VIEW = 26f;

        public static GameObject Build()
        {
            var root = new GameObject("CharacterShowcase");
            root.transform.position = Position;
            var stage = new GameObject("Stage");
            stage.transform.SetParent(root.transform, false);

            var turntable = new GameObject("Turntable").transform;
            turntable.SetParent(stage.transform, false);
            var catalog = CharacterModelBuilder.Build();
            var animator = PlayerPrefabBuilder.CreateModel(turntable, CharacterModelBuilder.Default(catalog));

            // Peana de concurso: canto dorado y tapa azul noche, como en los bocetos del menú.
            var top = EditorAssets.GetOrCreateMaterial(PEDESTAL_MATERIAL, new Color(0.12f, 0.16f, 0.32f), 0.75f);
            var rim = EditorAssets.GetOrCreateMaterial(PEDESTAL_RIM_MATERIAL, new Color(0.95f, 0.68f, 0.25f), 0.85f, metallic: 0.8f);
            var rimDisc = EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, "PedestalRim", stage.transform, new Vector3(0f, -0.09f, 0f),
                new Vector3(1.3f, 0.09f, 1.3f), rim, 0, false, keepCollider: false);
            var topDisc = EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, "Pedestal", stage.transform, new Vector3(0f, -0.02f, 0f),
                new Vector3(1.18f, 0.03f, 1.18f), top, 0, false, keepCollider: false);
            rimDisc.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            topDisc.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;

            var camera = new GameObject("Camera").AddComponent<Camera>();
            camera.transform.SetParent(stage.transform, false);
            camera.transform.localPosition = CameraPosition;
            camera.transform.LookAt(root.transform.TransformPoint(LookAt));
            camera.fieldOfView = FIELD_OF_VIEW;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 12f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f); // Transparente: se ve el panel del menú detrás.
            camera.allowHDR = false;

            AddLight(stage.transform, "KeyLight", new Vector3(1.6f, 2.6f, 3f), new Color(1f, 0.95f, 0.88f), 4f);
            AddLight(stage.transform, "RimLight", new Vector3(-1.8f, 2.4f, -2.2f), new Color(1f, 0.8f, 0.45f), 3f);

            var view = PlayerPrefabBuilder.AddCharacterView(root, animator, catalog);

            var showcase = root.AddComponent<CharacterShowcase>();
            SerializedWiring.Set(showcase, "_preview", SharedAssets.CharacterPreview());
            SerializedWiring.Set(showcase, "_width", SharedAssets.CHARACTER_PREVIEW_WIDTH);
            SerializedWiring.Set(showcase, "_height", SharedAssets.CHARACTER_PREVIEW_HEIGHT);
            SerializedWiring.Set(showcase, "_view", view);
            SerializedWiring.Set(showcase, "_catalog", SharedAssets.CharacterCatalog());
            SerializedWiring.Set(showcase, "_animator", animator);
            SerializedWiring.Set(showcase, "_turntable", turntable);
            SerializedWiring.Set(showcase, "_camera", camera);
            // Siempre animado: es un solo modelo y su cámara pinta en una imagen (Unity no siempre lo cuenta como visto).
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }

        private static void AddLight(Transform parent, string name, Vector3 position, Color color, float intensity)
        {
            var light = new GameObject(name).AddComponent<Light>();
            light.transform.SetParent(parent, false);
            light.transform.localPosition = position;
            light.type = LightType.Point;
            light.range = 8f;
            light.color = color;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
        }
    }
}
