using TMPro;
using Trivias.Money.Components;
using Trivias.Money.Data;
using Trivias.Money.Network;
using Trivias.Shared.Editor;
using Trivias.Shared.Physics;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Trivias.Money.Editor
{
    /// <summary>
    /// Construye en tiempo de edición todo lo del dinero: configuración, objetos visibles (fajo, bolsa, lingote)
    /// con los modelos del módulo, el prefab de red de la sesión y los componentes de mesa y bandejas.
    /// </summary>
    public static class MoneyBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Money";
        private const string CONFIG = MODULE_ROOT + "/Config";
        private const string PREFABS = MODULE_ROOT + "/Prefabs";
        public const string SESSION_PREFAB_PATH = PREFABS + "/MoneySession.prefab";


        public static MoneyConfig Config() => EditorAssets.GetOrCreateAsset<MoneyConfig>($"{CONFIG}/MoneyConfig.asset");
        public static MoneyLayout Layout() => EditorAssets.GetOrCreateAsset<MoneyLayout>($"{CONFIG}/MoneyLayout.asset");
        public static MoneyRuntime Runtime() => EditorAssets.GetOrCreateAsset<MoneyRuntime>($"{CONFIG}/MoneyRuntime.asset");

        [MenuItem("Tools/Trivias/Build/Money")]
        public static GameObject Build()
        {
            // Guarda de nuevo los assets de configuración para que incluyan los campos añadidos con sus valores.
            EditorUtility.SetDirty(Config());
            EditorUtility.SetDirty(Layout());

            var bundle = BuildBundle();
            var bag = BuildBag();
            var goldBar = BuildGoldBar();

            var root = new GameObject("MoneySession");
            root.AddComponent<NetworkObject>();

            var presenter = root.AddComponent<MoneyVisualPresenter>();
            SerializedWiring.Set(presenter, "_bundlePrefab", bundle);
            SerializedWiring.Set(presenter, "_bagPrefab", bag);
            SerializedWiring.Set(presenter, "_goldBarPrefab", goldBar);
            SerializedWiring.Set(presenter, "_avatars", SharedAssets.PlayerAvatars());
            SerializedWiring.Set(presenter, "_tubes", SharedAssets.TubeRegistry());

            var session = root.AddComponent<MoneySessionNetwork>();
            SerializedWiring.Set(session, "_config", Config());
            SerializedWiring.Set(session, "_layout", Layout());
            SerializedWiring.Set(session, "_runtime", Runtime());
            SerializedWiring.Set(session, "_presenter", presenter);
            SerializedWiring.Set(session, "_avatars", SharedAssets.PlayerAvatars());

            return EditorAssets.SaveAsPrefab(root, SESSION_PREFAB_PATH);
        }

        /// <summary>Guarda dónde están la superficie de la mesa y la de cada bandeja (escrito por el constructor de la escena).</summary>
        public static void WriteLayout(Vector3 tableSurface, Vector3 exchangeStation, Vector3[] traySurfaces, float[] trayYaw)
        {
            var layout = Layout();
            layout.EditorSet(tableSurface, exchangeStation, traySurfaces, trayYaw);
            EditorUtility.SetDirty(layout);
        }

        public static void AttachTableTarget(GameObject table, TextMeshPro totalLabel)
        {
            var target = table.AddComponent<MoneyTableTarget>();
            WireTarget(target, totalLabel);
        }

        public static void AttachTrayTarget(GameObject tray, int trayIndex, TextMeshPro totalLabel)
        {
            var target = tray.AddComponent<MoneyTrayTarget>();
            WireTarget(target, totalLabel);
            SerializedWiring.Set(target, "_trayIndex", trayIndex);
        }

        private static void WireTarget(MoneyTargetBase target, TextMeshPro totalLabel)
        {
            SerializedWiring.Set(target, "_config", Config());
            SerializedWiring.Set(target, "_runtime", Runtime());
            SerializedWiring.Set(target, "_registry", SharedAssets.InteractableRegistry());
            SerializedWiring.Set(target, "_totalLabel", totalLabel);
        }

        // ---- Objetos visibles (modelos del módulo): pivote en la base para apilarlos ----

        private const string MODELS = MODULE_ROOT + "/Models";

        /// <summary>Fajo: el modelo mide 10 cm de alto; a 0,8 cabe en cada capa de la pila (9 cm).</summary>
        private static GameObject BuildBundle() => BuildVisual("Bundle", 0.8f);

        private static GameObject BuildBag() => BuildVisual("Bag", 0.9f);

        private static GameObject BuildGoldBar() => BuildVisual("GoldBar", 0.8f);

        private static GameObject BuildVisual(string name, float scale)
        {
            var root = NewVisual(name);
            // No son estáticos (se mueven) ni proyectan sombra: hay muchos y son pequeños.
            EditorAssets.PlaceModel($"{MODELS}/{name}/{name}.obj", "Model", root.transform, Vector3.zero, Quaternion.identity,
                Vector3.one * scale, GameLayers.MONEY, false, castShadows: false);
            return EditorAssets.SaveAsPrefab(root, $"{PREFABS}/{name}.prefab");
        }

        private static GameObject NewVisual(string name) => new(name) { layer = GameLayers.MONEY };
    }
}
