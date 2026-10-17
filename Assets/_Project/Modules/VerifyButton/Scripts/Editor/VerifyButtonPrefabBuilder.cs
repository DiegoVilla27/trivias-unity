using Trivias.Match.Data;
using Trivias.Shared.Editor;
using Trivias.Shared.Interaction;
using Trivias.VerifyButton.Components;
using Trivias.Shared.Physics;
using UnityEditor;
using UnityEngine;

namespace Trivias.VerifyButton.Editor
{
    /// <summary>Botón grande de verificar (modelo del módulo, con su placa «VERIFICAR»), separado de la mesa y del dinero.</summary>
    public static class VerifyButtonPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/VerifyButton";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/VerifyButton.prefab";
        private const string MODEL_PATH = MODULE_ROOT + "/Models/VerifyButton/VerifyButton.obj";
        private const float MODEL_SCALE = 1.15f;
        private const string PLAQUE = "VerifyPlaque";

        public static GameObject Build(MatchRuntime matchRuntime, InteractableRegistry registry)
        {
            var root = new GameObject("VerifyButton") { layer = GameLayers.STAGE };
            var t = root.transform;
            const int layer = GameLayers.STAGE;

            var model = EditorAssets.PlaceModel(MODEL_PATH, "Model", t, Vector3.zero, Quaternion.identity,
                Vector3.one * MODEL_SCALE, layer, true);
            // Sin la placa del modelo (decisión del usuario): se quita la pieza y lo demás queda tal cual.
            PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            var plaque = model.transform.Find(PLAQUE);
            if (plaque != null)
                Object.DestroyImmediate(plaque.gameObject);
            // La tapa no es estática: se hunde cuando las apuestas están cerradas.
            var cap = model.transform.Find("Cap");
            GameObjectUtility.SetStaticEditorFlags(cap.gameObject, 0);
            EditorAssets.AddCylinderCollider("Collider", t, new Vector3(0f, 0.5f * MODEL_SCALE, 0f), 0.85f * MODEL_SCALE,
                MODEL_SCALE, layer);

            var target = root.AddComponent<VerifyButtonTarget>();
            SerializedWiring.Set(target, "_runtime", matchRuntime);
            SerializedWiring.Set(target, "_registry", registry);
            SerializedWiring.Set(target, "_cap", cap);
            SerializedWiring.Set(target, "_pressedDepth", 0.1f);

            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }
    }
}
