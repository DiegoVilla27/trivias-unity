using System.Collections.Generic;
using TMPro;
using Trivias.Shared.Editor;
using Trivias.Shared.Physics;
using UnityEditor;
using UnityEngine;

namespace Trivias.Booth.Editor
{
    /// <summary>
    /// Cabina de respuesta con primitivas: cuerpo, letra, bandeja y zona de apuesta delante.
    /// Un prefab base y una variante por respuesta (A/B/C/D) que solo cambia color y letra.
    /// El frente de la cabina mira a +Z local.
    /// </summary>
    public static class BoothPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Booth";
        private const string MATERIALS = MODULE_ROOT + "/Materials";
        private const string PREFABS = MODULE_ROOT + "/Prefabs";
        public const string BASE_PREFAB_PATH = PREFABS + "/Booth.prefab";

        private const string BODY = "Body";
        private const string LETTER = "Letter";
        private const string BET_ZONE = "BetZone";
        public const string TRAY = "Tray";
        public const string TRAY_SURFACE = "Tray/Surface";
        public const string TRAY_TOTAL_LABEL = "TotalLabel";
        public const string ANSWER_LABEL = "AnswerLabel";

        private static readonly (string Letter, Color Color)[] Answers =
        {
            ("A", StagePalette.Red), ("B", StagePalette.Blue), ("C", StagePalette.Yellow), ("D", StagePalette.Green),
        };

        [MenuItem("Tools/Trivias/Build/Booths")]
        public static IReadOnlyList<GameObject> Build()
        {
            var basePrefab = BuildBase();
            var variants = new List<GameObject>(Answers.Length);
            foreach (var (letter, color) in Answers)
                variants.Add(BuildVariant(basePrefab, letter, color));
            return variants;
        }

        private static GameObject BuildBase()
        {
            var neutral = EditorAssets.GetOrCreateMaterial($"{MATERIALS}/Booth_Neutral.mat", Color.gray);
            var gold = EditorAssets.GetOrCreateMaterial($"{MATERIALS}/Booth_Gold.mat", StagePalette.Gold, 0.7f, 0.8f);
            var navy = EditorAssets.GetOrCreateMaterial($"{MATERIALS}/Booth_Navy.mat", StagePalette.Navy, 0.4f);
            var zone = EditorAssets.GetOrCreateMaterial($"{MATERIALS}/Booth_ZoneNeutral.mat", Color.gray * 0.6f);

            var root = new GameObject("Booth") { layer = GameLayers.STAGE };
            var t = root.transform;
            const int layer = GameLayers.STAGE;

            EditorAssets.CreatePrimitive(PrimitiveType.Cube, BODY, t, new Vector3(0f, 1.3f, 0f), new Vector3(1.8f, 2.6f, 1.4f), neutral, layer, true);
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Crown", t, new Vector3(0f, 2.68f, 0f), new Vector3(2.0f, 0.16f, 1.55f), gold, layer, true);
            EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, "Tube", t, new Vector3(0f, 3.9f, 0f), new Vector3(0.7f, 1.1f, 0.7f), gold, layer, true, keepCollider: false);

            // Texto mirando a +Z (hacia el centro del plató): TMP se lee mirando a su +Z, por eso gira 180°.
            var letter = EditorAssets.CreateWorldText(LETTER, t, "?", 14f, Color.white,
                new Vector3(0f, 1.6f, 0.71f), Quaternion.Euler(0f, 180f, 0f), new Vector2(1.6f, 1.6f));
            letter.fontStyle = FontStyles.Bold;

            // Texto de la respuesta sobre la cabina; lo rellena el módulo Match.
            var answer = EditorAssets.CreateWorldText(ANSWER_LABEL, t, "?", 4f, Color.white,
                new Vector3(0f, 3.05f, 0.8f), Quaternion.Euler(0f, 180f, 0f), new Vector2(4.2f, 0.9f));
            answer.fontStyle = FontStyles.Bold;
            answer.outlineWidth = 0.22f;
            answer.outlineColor = Color.black;
            answer.textWrappingMode = TextWrappingModes.Normal;

            var tray = EditorAssets.CreateEmpty(TRAY, t, new Vector3(-1.45f, 0f, 0.9f)).transform;
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Pedestal", tray, new Vector3(0f, 0.4f, 0f), new Vector3(1.0f, 0.8f, 0.6f), navy, layer, true);
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Surface", tray, new Vector3(0f, 0.83f, 0f), new Vector3(1.1f, 0.06f, 0.7f), gold, layer, true);
            var trayTotal = EditorAssets.CreateWorldText(TRAY_TOTAL_LABEL, tray, string.Empty, 3.2f, StagePalette.Gold,
                new Vector3(0f, 1.45f, 0f), Quaternion.Euler(0f, 180f, 0f), new Vector2(1.6f, 0.5f));
            trayTotal.fontStyle = FontStyles.Bold;
            trayTotal.outlineWidth = 0.2f;
            trayTotal.outlineColor = Color.black;

            EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, BET_ZONE, t, new Vector3(0.6f, 0.01f, 2.1f), new Vector3(2.6f, 0.01f, 2.6f), zone, layer, true, keepCollider: false);

            return EditorAssets.SaveAsPrefab(root, BASE_PREFAB_PATH);
        }

        private static GameObject BuildVariant(GameObject basePrefab, string letter, Color color)
        {
            var body = EditorAssets.GetOrCreateMaterial($"{MATERIALS}/Booth_{letter}.mat", color, 0.45f);
            var zone = EditorAssets.GetOrCreateMaterial($"{MATERIALS}/Booth_{letter}_Zone.mat", color * 0.55f, 0.2f);

            return EditorAssets.SaveVariant(basePrefab, $"{PREFABS}/Booth_{letter}.prefab", instance =>
            {
                instance.name = $"Booth_{letter}";
                instance.transform.Find(BODY).GetComponent<MeshRenderer>().sharedMaterial = body;
                instance.transform.Find(BET_ZONE).GetComponent<MeshRenderer>().sharedMaterial = zone;
                instance.transform.Find(LETTER).GetComponent<TextMeshPro>().text = letter;
            });
        }
    }
}
