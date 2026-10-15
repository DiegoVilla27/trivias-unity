using Trivias.Exchange.Components;
using Trivias.Money.Data;
using Trivias.Shared.Editor;
using Trivias.Shared.Interaction;
using Trivias.Shared.Physics;
using UnityEditor;
using UnityEngine;

namespace Trivias.Exchange.Editor
{
    /// <summary>Estación de cambio manual de denominaciones, junto a la mesa.</summary>
    public static class ExchangeStationPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Exchange";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/ExchangeStation.prefab";

        public static GameObject Build(MoneyConfig moneyConfig, MoneyRuntime moneyRuntime, InteractableRegistry registry)
        {
            var navy = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Exchange_Base.mat", StagePalette.Navy, 0.4f);
            var gold = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Exchange_Gold.mat", StagePalette.Gold, 0.7f, 0.8f);
            var red = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Exchange_Knob.mat", StagePalette.Red, 0.6f);

            var root = new GameObject("ExchangeStation") { layer = GameLayers.STAGE };
            var t = root.transform;
            const int layer = GameLayers.STAGE;

            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Base", t, new Vector3(0f, 0.45f, 0f), new Vector3(1.3f, 0.9f, 0.7f), navy, layer, true);
            var roller = EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, "Roller", t, new Vector3(-0.1f, 1.08f, 0f), new Vector3(0.32f, 0.45f, 0.32f), gold, layer, true, keepCollider: false);
            roller.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, "Lever", t, new Vector3(0.5f, 1.15f, 0f), new Vector3(0.08f, 0.25f, 0.08f), gold, layer, true, keepCollider: false);
            EditorAssets.CreatePrimitive(PrimitiveType.Sphere, "Knob", t, new Vector3(0.5f, 1.42f, 0f), Vector3.one * 0.16f, red, layer, true, keepCollider: false);

            EditorAssets.CreateWorldText("Label", t, "CAMBIO", 3f, StagePalette.Gold,
                new Vector3(0f, 0.55f, 0.36f), Quaternion.Euler(0f, 180f, 0f), new Vector2(1.3f, 0.4f));

            var target = root.AddComponent<ExchangeStationTarget>();
            SerializedWiring.Set(target, "_config", moneyConfig);
            SerializedWiring.Set(target, "_runtime", moneyRuntime);
            SerializedWiring.Set(target, "_registry", registry);

            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }
    }
}
