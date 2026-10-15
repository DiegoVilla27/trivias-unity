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
    /// con primitivas, el prefab de red de la sesión y los componentes de mesa y bandejas.
    /// </summary>
    public static class MoneyBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Money";
        private const string CONFIG = MODULE_ROOT + "/Config";
        private const string PREFABS = MODULE_ROOT + "/Prefabs";
        private const string MATERIALS = MODULE_ROOT + "/Materials";
        public const string SESSION_PREFAB_PATH = PREFABS + "/MoneySession.prefab";

        private static readonly Color BanknoteGreen = new(0.42f, 0.66f, 0.42f);
        private static readonly Color PaperBand = new(0.93f, 0.86f, 0.66f);
        private static readonly Color Burlap = new(0.78f, 0.64f, 0.45f);
        private static readonly Color Rope = new(0.45f, 0.30f, 0.18f);

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
            SerializedWiring.Set(presenter, "_carrySockets", SharedAssets.CarrySocketRegistry());

            var session = root.AddComponent<MoneySessionNetwork>();
            SerializedWiring.Set(session, "_config", Config());
            SerializedWiring.Set(session, "_layout", Layout());
            SerializedWiring.Set(session, "_runtime", Runtime());
            SerializedWiring.Set(session, "_presenter", presenter);

            return EditorAssets.SaveAsPrefab(root, SESSION_PREFAB_PATH);
        }

        /// <summary>Guarda dónde están la superficie de la mesa y la de cada bandeja (escrito por el constructor de la escena).</summary>
        public static void WriteLayout(Vector3 tableSurface, Vector3[] traySurfaces, float[] trayYaw)
        {
            var layout = Layout();
            layout.EditorSet(tableSurface, traySurfaces, trayYaw);
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

        // ---- Objetos visibles: pivote en la base para apilarlos ----

        private static GameObject BuildBundle()
        {
            var notes = Material("Money_Banknotes", BanknoteGreen, 0.2f);
            var band = Material("Money_Band", PaperBand, 0.2f);
            var root = NewVisual("Bundle");
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Notes", root.transform, new Vector3(0f, 0.035f, 0f), new Vector3(0.30f, 0.07f, 0.15f), notes, GameLayers.MONEY, false, keepCollider: false);
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Band", root.transform, new Vector3(0f, 0.035f, 0f), new Vector3(0.07f, 0.074f, 0.154f), band, GameLayers.MONEY, false, keepCollider: false);
            return EditorAssets.SaveAsPrefab(root, $"{PREFABS}/Bundle.prefab");
        }

        private static GameObject BuildBag()
        {
            var cloth = Material("Money_Burlap", Burlap, 0.1f);
            var rope = Material("Money_Rope", Rope, 0.1f);
            var root = NewVisual("Bag");
            EditorAssets.CreatePrimitive(PrimitiveType.Sphere, "Body", root.transform, new Vector3(0f, 0.13f, 0f), new Vector3(0.28f, 0.26f, 0.28f), cloth, GameLayers.MONEY, false, keepCollider: false);
            EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, "Neck", root.transform, new Vector3(0f, 0.28f, 0f), new Vector3(0.09f, 0.035f, 0.09f), rope, GameLayers.MONEY, false, keepCollider: false);
            EditorAssets.CreatePrimitive(PrimitiveType.Sphere, "Top", root.transform, new Vector3(0f, 0.33f, 0f), new Vector3(0.12f, 0.07f, 0.12f), cloth, GameLayers.MONEY, false, keepCollider: false);
            return EditorAssets.SaveAsPrefab(root, $"{PREFABS}/Bag.prefab");
        }

        private static GameObject BuildGoldBar()
        {
            var gold = Material("Money_Gold", StagePalette.Gold, 0.85f, 1f);
            var root = NewVisual("GoldBar");
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Bar", root.transform, new Vector3(0f, 0.04f, 0f), new Vector3(0.28f, 0.08f, 0.12f), gold, GameLayers.MONEY, false, keepCollider: false);
            return EditorAssets.SaveAsPrefab(root, $"{PREFABS}/GoldBar.prefab");
        }

        private static GameObject NewVisual(string name) => new(name) { layer = GameLayers.MONEY };

        private static Material Material(string name, Color color, float smoothness, float metallic = 0f) =>
            EditorAssets.GetOrCreateMaterial($"{MATERIALS}/{name}.mat", color, smoothness, metallic);
    }
}
