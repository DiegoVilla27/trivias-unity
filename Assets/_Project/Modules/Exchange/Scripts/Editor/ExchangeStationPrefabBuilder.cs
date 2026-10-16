using Trivias.Exchange.Components;
using Trivias.Money.Data;
using Trivias.Shared.Editor;
using Trivias.Shared.Interaction;
using TMPro;
using Trivias.Shared.Physics;
using UnityEngine;

namespace Trivias.Exchange.Editor
{
    /// <summary>
    /// Estación de cambio manual de denominaciones, junto a la mesa: la máquina de rodillo (modelo del módulo) sobre un
    /// pedestal dorado y azul, como la mesa.
    /// </summary>
    public static class ExchangeStationPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Exchange";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/ExchangeStation.prefab";
        private const string MODEL_PATH = MODULE_ROOT + "/Models/ExchangeStation/ExchangeStation.obj";
        private const float STAND_HEIGHT = 0.8f;

        public static GameObject Build(MoneyConfig moneyConfig, MoneyRuntime moneyRuntime, InteractableRegistry registry)
        {
            var navy = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Exchange_Base.mat", StagePalette.Navy, 0.4f);
            var gold = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Exchange_Gold.mat", StagePalette.Gold, 0.7f, 0.8f);

            var root = new GameObject("ExchangeStation") { layer = GameLayers.STAGE };
            var t = root.transform;
            const int layer = GameLayers.STAGE;

            // Pedestal sencillo con los colores del plató; la máquina va encima.
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Stand", t, new Vector3(0f, STAND_HEIGHT * 0.5f, 0f),
                new Vector3(1.5f, STAND_HEIGHT, 0.75f), navy, layer, true);
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "StandTrim", t, new Vector3(0f, STAND_HEIGHT - 0.03f, 0f),
                new Vector3(1.6f, 0.06f, 0.85f), gold, layer, true, keepCollider: false);
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "StandFoot", t, new Vector3(0f, 0.04f, 0f),
                new Vector3(1.6f, 0.08f, 0.85f), gold, layer, true, keepCollider: false);
            EditorAssets.PlaceModel(MODEL_PATH, "Model", t, new Vector3(0f, STAND_HEIGHT, 0f), Quaternion.identity,
                Vector3.one, layer, true);

            var label = EditorAssets.CreateWorldText("Label", t, "CAMBIO", 3.4f, StagePalette.Gold,
                new Vector3(0f, STAND_HEIGHT * 0.5f, 0.38f), Quaternion.Euler(0f, 180f, 0f), new Vector2(1.5f, 0.45f), StageFonts.Display());
            label.characterSpacing = 4f;
            label.textWrappingMode = TextWrappingModes.NoWrap;

            var target = root.AddComponent<ExchangeStationTarget>();
            SerializedWiring.Set(target, "_config", moneyConfig);
            SerializedWiring.Set(target, "_runtime", moneyRuntime);
            SerializedWiring.Set(target, "_registry", registry);

            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }
    }
}
