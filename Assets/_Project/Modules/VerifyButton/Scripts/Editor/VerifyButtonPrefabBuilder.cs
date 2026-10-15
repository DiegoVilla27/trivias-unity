using TMPro;
using Trivias.Match.Data;
using Trivias.Shared.Editor;
using Trivias.Shared.Interaction;
using Trivias.VerifyButton.Components;
using Trivias.Shared.Physics;
using UnityEditor;
using UnityEngine;

namespace Trivias.VerifyButton.Editor
{
    /// <summary>Botón grande de verificar, separado de la mesa y del dinero.</summary>
    public static class VerifyButtonPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/VerifyButton";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/VerifyButton.prefab";

        public static GameObject Build(MatchRuntime matchRuntime, InteractableRegistry registry)
        {
            var navy = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/VerifyButton_Base.mat", StagePalette.Navy, 0.4f);
            var gold = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/VerifyButton_Gold.mat", StagePalette.Gold, 0.7f, 0.8f);
            var red = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/VerifyButton_Cap.mat", StagePalette.Red, 0.8f,
                emission: StagePalette.Red * 0.6f);

            var root = new GameObject("VerifyButton") { layer = GameLayers.STAGE };
            var t = root.transform;
            const int layer = GameLayers.STAGE;

            EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, "Pedestal", t, new Vector3(0f, 0.5f, 0f), new Vector3(0.8f, 0.5f, 0.8f), navy, layer, true);
            EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, "Ring", t, new Vector3(0f, 1.0f, 0f), new Vector3(0.85f, 0.03f, 0.85f), gold, layer, true, keepCollider: false);
            // La tapa no es estática: se hunde cuando las apuestas están cerradas.
            var cap = EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, "Cap", t, new Vector3(0f, 1.06f, 0f), new Vector3(0.6f, 0.06f, 0.6f), red, layer, false, keepCollider: false);

            var label = EditorAssets.CreateWorldText("Label", t, "VERIFICAR", 2.6f, StagePalette.Gold,
                new Vector3(0f, 0.65f, 0.41f), Quaternion.Euler(0f, 180f, 0f), new Vector2(1.2f, 0.4f));
            label.fontStyle = FontStyles.Bold;

            var target = root.AddComponent<VerifyButtonTarget>();
            SerializedWiring.Set(target, "_runtime", matchRuntime);
            SerializedWiring.Set(target, "_registry", registry);
            SerializedWiring.Set(target, "_cap", cap.transform);

            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }
    }
}
