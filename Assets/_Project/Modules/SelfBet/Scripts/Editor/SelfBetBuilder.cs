using TMPro;
using Trivias.SelfBet.Components;
using Trivias.SelfBet.Data;
using Trivias.SelfBet.Network;
using Trivias.Shared.Editor;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Trivias.SelfBet.Editor
{
    /// <summary>Construye la configuración de comodines, el prefab de red de la sesión y conecta las cabinas.</summary>
    public static class SelfBetBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/SelfBet";
        private const string CONFIG = MODULE_ROOT + "/Config";
        public const string SESSION_PREFAB_PATH = MODULE_ROOT + "/Prefabs/SelfBetSession.prefab";

        public static SelfBetConfig Config() => EditorAssets.GetOrCreateAsset<SelfBetConfig>($"{CONFIG}/SelfBetConfig.asset");
        public static SelfBetLayout Layout() => EditorAssets.GetOrCreateAsset<SelfBetLayout>($"{CONFIG}/SelfBetLayout.asset");
        public static SelfBetRuntime Runtime() => EditorAssets.GetOrCreateAsset<SelfBetRuntime>($"{CONFIG}/SelfBetRuntime.asset");

        public static GameObject Build()
        {
            EditorUtility.SetDirty(Config());

            var root = new GameObject("SelfBetSession");
            root.AddComponent<NetworkObject>();
            var session = root.AddComponent<SelfBetSessionNetwork>();
            SerializedWiring.Set(session, "_config", Config());
            SerializedWiring.Set(session, "_layout", Layout());
            SerializedWiring.Set(session, "_runtime", Runtime());
            SerializedWiring.Set(session, "_avatars", SharedAssets.PlayerAvatars());
            SerializedWiring.Set(session, "_movementLimit", SharedAssets.PlayerMovementLimit());

            return EditorAssets.SaveAsPrefab(root, SESSION_PREFAB_PATH);
        }

        public static void WriteLayout(Vector3[] zoneCenters)
        {
            var layout = Layout();
            layout.EditorSet(zoneCenters);
            EditorUtility.SetDirty(layout);
        }

        public static void AttachToBooth(GameObject betZone, GameObject booth, int boothIndex, TextMeshPro bettorsLabel)
        {
            var zone = betZone.AddComponent<BetZoneTarget>();
            SerializedWiring.Set(zone, "_config", Config());
            SerializedWiring.Set(zone, "_runtime", Runtime());
            SerializedWiring.Set(zone, "_registry", SharedAssets.InteractableRegistry());
            SerializedWiring.Set(zone, "_booth", boothIndex);
            SerializedWiring.Set(zone, "_verifyFromZoneRequested", SharedAssets.VerifyFromZoneRequested());

            var label = booth.AddComponent<BoothBettorsLabel>();
            SerializedWiring.Set(label, "_runtime", Runtime());
            SerializedWiring.Set(label, "_avatars", SharedAssets.PlayerAvatars());
            SerializedWiring.Set(label, "_label", bettorsLabel);
            SerializedWiring.Set(label, "_booth", boothIndex);
        }
    }
}
