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
        // Pantalla de preguntas: rutas de sus partes, para conectarlas con el módulo Match.
        public const string SCREEN = "QuestionScreen";
        public const string SCREEN_CONTENT = SCREEN + "/Content";
        public const string SCREEN_MESSAGE = SCREEN + "/Message";
        public const string SCREEN_HEADER = SCREEN_CONTENT + "/Header";
        public const string SCREEN_QUESTION = SCREEN_CONTENT + "/Question";
        public const string SCREEN_STATUS = SCREEN_CONTENT + "/Status";
        public static string ScreenOptionBox(int booth) => $"{SCREEN_CONTENT}/Option{(char)('A' + booth)}/Box";
        public static string ScreenOptionText(int booth) => $"{SCREEN_CONTENT}/Option{(char)('A' + booth)}/Text";

        /// <summary>Centro de la pantalla: alta para que cabinas y tubos no tapen las opciones desde la zona de inicio.</summary>
        public static readonly Vector3 ScreenCenter = new(0f, 7.6f, 15f);
        public static readonly Vector2 ScreenPanelSize = new(14.2f, 7.4f);

        public const float HALF_SIZE = 22f;
        private const float WALL_HEIGHT = 12f;
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
            // Fondo oscuro con un poco de brillo: el texto blanco y dorado se lee mejor que sobre azul intenso.
            var screen = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Stage_Screen.mat",
                new Color(0.05f, 0.07f, 0.2f), 0.9f, emission: new Color(0.03f, 0.05f, 0.16f));

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

            BuildQuestionScreen(t, frame, screen, gold, layer);

            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }

        /// <summary>
        /// Pantalla grande (GDD: la pregunta se ve en el plató): cabecera, pregunta, las 4 opciones en recuadros
        /// del color de su cabina y una línea de estado. El texto mira a -Z (hacia el centro del plató).
        /// </summary>
        private static void BuildQuestionScreen(Transform parent, Material frame, Material panel, Material gold, int layer)
        {
            var root = EditorAssets.CreateEmpty(SCREEN, parent, ScreenCenter).transform;
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Frame", root, Vector3.zero,
                new Vector3(ScreenPanelSize.x + 0.8f, ScreenPanelSize.y + 0.8f, 0.4f), frame, layer, true);
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Trim", root, new Vector3(0f, 0f, -0.21f),
                new Vector3(ScreenPanelSize.x + 0.3f, ScreenPanelSize.y + 0.3f, 0.02f), gold, layer, true, keepCollider: false);
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Panel", root, new Vector3(0f, 0f, -0.23f),
                new Vector3(ScreenPanelSize.x, ScreenPanelSize.y, 0.02f), panel, layer, true, keepCollider: false);

            const float textZ = -0.3f;
            var message = EditorAssets.CreateWorldText("Message", root, "TRIVIAS", 16f, StagePalette.Gold,
                new Vector3(0f, 0f, textZ), Quaternion.identity, new Vector2(13.6f, 6f));
            message.fontStyle = FontStyles.Bold;

            var content = EditorAssets.CreateEmpty("Content", root, Vector3.zero).transform;
            var header = EditorAssets.CreateWorldText("Header", content, "PREGUNTA 1/15", 5f, StagePalette.Gold,
                new Vector3(0f, 3.1f, textZ), Quaternion.identity, new Vector2(13.6f, 0.8f));
            header.fontStyle = FontStyles.Bold;

            var question = EditorAssets.CreateWorldText("Question", content, "¿Pregunta?", 7f, Color.white,
                new Vector3(0f, 1.75f, textZ), Quaternion.identity, new Vector2(13.6f, 2f));
            question.fontStyle = FontStyles.Bold;

            var optionColors = new[] { StagePalette.Red, StagePalette.Blue, StagePalette.Yellow, StagePalette.Green };
            for (var booth = 0; booth < optionColors.Length; booth++)
            {
                var column = booth % 2;
                var row = booth / 2;
                var letter = (char)('A' + booth);
                var option = EditorAssets.CreateEmpty($"Option{letter}", content,
                    new Vector3(column == 0 ? -3.5f : 3.5f, row == 0 ? 0.05f : -1.55f, 0f)).transform;

                var boxMaterial = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Stage_Option{letter}.mat",
                    optionColors[booth] * 0.8f, 0.5f, emission: optionColors[booth] * 0.25f);
                // Los recuadros cambian al revelar (solo queda el correcto): no son estáticos.
                EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Box", option, new Vector3(0f, 0f, -0.26f),
                    new Vector3(6.7f, 1.35f, 0.02f), boxMaterial, layer, false, keepCollider: false);

                var text = EditorAssets.CreateWorldText("Text", option, $"{letter}  Opción", 5.5f, Color.white,
                    new Vector3(0f, 0f, textZ), Quaternion.identity, new Vector2(6.3f, 1.3f));
                text.fontStyle = FontStyles.Bold;
                text.outlineWidth = 0.15f;
                text.outlineColor = Color.black;
            }

            var status = EditorAssets.CreateWorldText("Status", content, string.Empty, 6.5f, Color.white,
                new Vector3(0f, -2.95f, textZ), Quaternion.identity, new Vector2(13.6f, 1.3f));
            status.fontStyle = FontStyles.Bold;
        }

        private static Material Mat(string name, Color color, float smoothness) =>
            EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/{name}.mat", color, smoothness);
    }
}
