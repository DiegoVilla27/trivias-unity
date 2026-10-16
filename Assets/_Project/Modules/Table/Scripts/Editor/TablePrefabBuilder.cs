using Trivias.Shared.Editor;
using Trivias.Shared.Physics;
using Trivias.Shared.Presentation;
using UnityEditor;
using UnityEngine;

namespace Trivias.Table.Editor
{
    /// <summary>Mesa central redonda (modelo del módulo) donde empieza y vuelve el dinero.</summary>
    public static class TablePrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Table";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/Table.prefab";
        public const string MODEL_PATH = MODULE_ROOT + "/Models/Table/Table.obj";
        public const string TOTAL_LABEL = "TotalLabel";

        /// <summary>Altura del tablero del modelo (0,95 m): ahí se apila el dinero.</summary>
        public const float SURFACE_HEIGHT = 0.95f;
        private const float TOP_RADIUS = 1.45f;

        [MenuItem("Tools/Trivias/Build/Table")]
        public static GameObject Build()
        {
            var root = new GameObject("Table") { layer = GameLayers.STAGE };
            var t = root.transform;
            const int layer = GameLayers.STAGE;

            EditorAssets.PlaceModel(MODEL_PATH, "Model", t, Vector3.zero, Quaternion.identity, Vector3.one, layer, true);
            // Colisión: un cilindro bajo del tamaño del tablero (no se puede atravesar la mesa).
            EditorAssets.AddCylinderCollider("Collider", t, new Vector3(0f, SURFACE_HEIGHT * 0.5f, 0f), TOP_RADIUS * 2f,
                SURFACE_HEIGHT, layer);

            var total = EditorAssets.CreateWorldText(TOTAL_LABEL, t, string.Empty, 3.2f, Color.white,
                new Vector3(0f, 2.1f, 0f), Quaternion.identity, new Vector2(2.4f, 0.9f));
            total.outlineWidth = 0.2f;
            total.outlineColor = Color.black;
            total.gameObject.AddComponent<Billboard>();

            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }
    }
}
