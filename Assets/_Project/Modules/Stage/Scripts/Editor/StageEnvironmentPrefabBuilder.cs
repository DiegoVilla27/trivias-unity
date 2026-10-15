using TMPro;
using Trivias.Shared.Editor;
using Trivias.Shared.Physics;
using UnityEditor;
using UnityEngine;

namespace Trivias.Stage.Editor
{
    /// <summary>
    /// Volumen del plató con primitivas: suelo, escenario circular, paredes y pantalla de preguntas.
    /// Toda la geometría es estática (batching y oclusión en tiempo de build).
    /// </summary>
    public static class StageEnvironmentPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Stage";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/StageEnvironment.prefab";

        public const float HALF_SIZE = 22f;
        private const float WALL_HEIGHT = 8f;
        private const float WALL_THICKNESS = 0.5f;
        private const float STAGE_RADIUS = 12f;

        [MenuItem("Tools/Trivias/Build/Stage Environment")]
        public static GameObject Build()
        {
            var floor = Mat("Stage_Floor", StagePalette.Navy * 0.8f, 0.5f);
            var stage = Mat("Stage_Platform", StagePalette.StageBlue, 0.65f);
            var gold = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Stage_Gold.mat", StagePalette.Gold, 0.7f, 0.8f);
            var wall = Mat("Stage_Wall", StagePalette.Purple, 0.3f);
            var frame = Mat("Stage_ScreenFrame", StagePalette.Navy, 0.5f);
            var screen = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Stage_Screen.mat", StagePalette.ScreenGlow, 0.9f,
                emission: StagePalette.ScreenGlow * 0.8f);

            var root = new GameObject("StageEnvironment") { layer = GameLayers.STAGE };
            var t = root.transform;
            const int layer = GameLayers.STAGE;
            const float size = HALF_SIZE * 2f;

            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Floor", t, new Vector3(0f, -0.1f, 0f), new Vector3(size, 0.2f, size), floor, layer, true);
            EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, "StageRing", t, new Vector3(0f, 0.004f, 0f), new Vector3(STAGE_RADIUS * 2f + 0.6f, 0.004f, STAGE_RADIUS * 2f + 0.6f), gold, layer, true, keepCollider: false);
            EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, "StagePlatform", t, new Vector3(0f, 0.008f, 0f), new Vector3(STAGE_RADIUS * 2f, 0.004f, STAGE_RADIUS * 2f), stage, layer, true, keepCollider: false);
            EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, "CenterRing", t, new Vector3(0f, 0.012f, 0f), new Vector3(7.2f, 0.004f, 7.2f), gold, layer, true, keepCollider: false);
            EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, "CenterDisc", t, new Vector3(0f, 0.016f, 0f), new Vector3(6.9f, 0.004f, 6.9f), floor, layer, true, keepCollider: false);

            var halfHeight = WALL_HEIGHT * 0.5f;
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "WallNorth", t, new Vector3(0f, halfHeight, HALF_SIZE), new Vector3(size, WALL_HEIGHT, WALL_THICKNESS), wall, layer, true);
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "WallSouth", t, new Vector3(0f, halfHeight, -HALF_SIZE), new Vector3(size, WALL_HEIGHT, WALL_THICKNESS), wall, layer, true);
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "WallEast", t, new Vector3(HALF_SIZE, halfHeight, 0f), new Vector3(WALL_THICKNESS, WALL_HEIGHT, size), wall, layer, true);
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "WallWest", t, new Vector3(-HALF_SIZE, halfHeight, 0f), new Vector3(WALL_THICKNESS, WALL_HEIGHT, size), wall, layer, true);

            var screenRoot = EditorAssets.CreateEmpty("QuestionScreen", t, new Vector3(0f, 5.2f, 15f)).transform;
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Frame", screenRoot, Vector3.zero, new Vector3(9f, 5f, 0.3f), frame, layer, true);
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Panel", screenRoot, new Vector3(0f, 0f, -0.16f), new Vector3(8.4f, 4.4f, 0.04f), screen, layer, true, keepCollider: false);
            var title = EditorAssets.CreateWorldText("Title", screenRoot, "PREGUNTA 1 / 15", 12f, Color.white,
                new Vector3(0f, 0f, -0.2f), Quaternion.identity, new Vector2(8f, 2f));
            title.fontStyle = FontStyles.Bold;

            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }

        private static Material Mat(string name, Color color, float smoothness) =>
            EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/{name}.mat", color, smoothness);
    }
}
