using Trivias.Shared.Editor;
using Trivias.Shared.Physics;
using UnityEditor;
using UnityEngine;

namespace Trivias.Presenter.Editor
{
    /// <summary>Presentador provisional con primitivas: cuerpo granate y cabeza grande. Solo se comunica por texto.</summary>
    public static class PresenterPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Presenter";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/Presenter.prefab";

        [MenuItem("Tools/Trivias/Build/Presenter")]
        public static GameObject Build()
        {
            var suit = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Presenter_Suit.mat", StagePalette.Burgundy, 0.5f);
            var skin = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Presenter_Skin.mat", StagePalette.Skin, 0.3f);
            var hair = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Presenter_Hair.mat", new Color(0.2f, 0.2f, 0.22f), 0.4f);

            var root = new GameObject("Presenter") { layer = GameLayers.STAGE };
            var t = root.transform;
            const int layer = GameLayers.STAGE;

            EditorAssets.CreatePrimitive(PrimitiveType.Capsule, "Body", t, new Vector3(0f, 0.75f, 0f), new Vector3(0.8f, 0.75f, 0.6f), suit, layer, false);
            EditorAssets.CreatePrimitive(PrimitiveType.Sphere, "Head", t, new Vector3(0f, 1.85f, 0f), Vector3.one * 0.9f, skin, layer, false, keepCollider: false);
            EditorAssets.CreatePrimitive(PrimitiveType.Sphere, "Hair", t, new Vector3(0f, 2.1f, -0.08f), new Vector3(0.95f, 0.5f, 0.85f), hair, layer, false, keepCollider: false);
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Moustache", t, new Vector3(0f, 1.72f, 0.43f), new Vector3(0.4f, 0.08f, 0.06f), hair, layer, false, keepCollider: false);

            EditorAssets.CreateWorldText("Label", t, "PRESENTADOR", 2.5f, Color.white,
                new Vector3(0f, 2.75f, 0f), Quaternion.Euler(0f, 180f, 0f), new Vector2(2.5f, 0.5f));

            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }
    }
}
