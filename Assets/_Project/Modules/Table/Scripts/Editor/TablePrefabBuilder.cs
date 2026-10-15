using Trivias.Shared.Editor;
using Trivias.Shared.Physics;
using Trivias.Shared.Presentation;
using UnityEditor;
using UnityEngine;

namespace Trivias.Table.Editor
{
    /// <summary>Mesa central redonda donde empieza y vuelve el dinero.</summary>
    public static class TablePrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Table";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/Table.prefab";
        public const string TOTAL_LABEL = "TotalLabel";
        public const float SURFACE_HEIGHT = 0.95f;

        [MenuItem("Tools/Trivias/Build/Table")]
        public static GameObject Build()
        {
            var navy = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Table_Base.mat", StagePalette.Navy, 0.4f);
            var top = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Table_Top.mat", StagePalette.StageBlue, 0.6f);
            var gold = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Table_Gold.mat", StagePalette.Gold, 0.7f, 0.8f);

            var root = new GameObject("Table") { layer = GameLayers.STAGE };
            var t = root.transform;
            const int layer = GameLayers.STAGE;

            EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, "Base", t, new Vector3(0f, 0.45f, 0f), new Vector3(2.4f, 0.45f, 2.4f), navy, layer, true);
            EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, "Rim", t, new Vector3(0f, 0.9f, 0f), new Vector3(2.9f, 0.03f, 2.9f), gold, layer, true, keepCollider: false);
            EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, "Top", t, new Vector3(0f, 0.93f, 0f), new Vector3(2.75f, 0.02f, 2.75f), top, layer, true);

            var total = EditorAssets.CreateWorldText(TOTAL_LABEL, t, string.Empty, 3.2f, Color.white,
                new Vector3(0f, 2.1f, 0f), Quaternion.identity, new Vector2(2.4f, 0.9f));
            total.outlineWidth = 0.2f;
            total.outlineColor = Color.black;
            total.gameObject.AddComponent<Billboard>();

            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }
    }
}
