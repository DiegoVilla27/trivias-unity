using TMPro;
using Trivias.Shared.Editor;
using Trivias.Shared.Physics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Trivias.Stage.Editor
{
    /// <summary>
    /// El plató de concepto-01 con los modelos del módulo, sobre el plano de <see cref="StageLayout"/> (sala compacta,
    /// sin espacio muerto): suelo de baldosas con aro dorado alrededor de la mesa; pantalla al fondo con bombillas
    /// debajo, telón rojo, cortinas recogidas, columnas y altavoces; paredes con paneles, columnas, rejillas y
    /// altavoces; techo con paneles (cara de luz hacia abajo), dos líneas de vigas con focos y abrazaderas; cámaras de
    /// estudio (una en una plataforma con escalera); público en gradas detrás de una barandilla; la marca del presentador; y el volumen de
    /// posprocesado (brillo de bombillas y focos). Todo es decorado estático; lo alto y lejano no proyecta sombras.
    /// </summary>
    public static class StageEnvironmentPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Stage";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/StageEnvironment.prefab";
        private const string MODELS = MODULE_ROOT + "/Models";
        private const string VOLUME_PROFILE_PATH = MODULE_ROOT + "/Config/StageVolumeProfile.asset";

        /// <summary>Foco de estudio (modelo): App lo cuelga en las luces del ambiente para que la luz salga de un foco.</summary>
        public const string SPOTLIGHT_MODEL = MODELS + "/StudioSpotlight/StudioSpotlight.obj";

        // Pantalla de preguntas: rutas de sus partes, para conectarlas con el módulo Match.
        public const string SCREEN = "QuestionScreen";
        public const string SCREEN_FRAME = SCREEN + "/Frame";
        public const string SCREEN_CONTENT = SCREEN + "/Content";
        public const string SCREEN_MESSAGE = SCREEN + "/Message";
        public const string SCREEN_HEADER = SCREEN_CONTENT + "/Header";
        public const string SCREEN_QUESTION = SCREEN_CONTENT + "/Question";
        public const string SCREEN_STATUS = SCREEN_CONTENT + "/Status";
        public const string SCREEN_CHECK = SCREEN + "/Check";
        public const string SCREEN_CHECK_PAYEE = SCREEN_CHECK + "/Payee";
        public const string SCREEN_CHECK_AMOUNT = SCREEN_CHECK + "/Amount";
        public static string ScreenOptionBox(int booth) => $"{SCREEN_CONTENT}/Option{(char)('A' + booth)}/Box";
        public static string ScreenOptionText(int booth) => $"{SCREEN_CONTENT}/Option{(char)('A' + booth)}/Text";

        /// <summary>Fondo oscuro detrás del texto: del tamaño del cristal de la pantalla (modelo a escala 1,12).</summary>
        public static readonly Vector2 ScreenPanelSize = new(14.9f, 7.05f);

        /// <summary>Plataforma de cámara: borde curvo en círculo (escala 1: 1,62 m de radio y 0,62 m de alto).</summary>
        private const float DAIS_HEIGHT = 0.62f;
        private const float DAIS_RADIUS = 1.62f;

        // Medidas de los modelos (sin escalar).
        private const float TILE_SIZE = 2f;
        private const float TILE_HEIGHT = 0.15f;
        private const float PANEL_WIDTH = 2.41f;
        private const float CEILING_PANEL_HEIGHT = 0.37f;
        private const float CLAMP_HEIGHT = 0.78f;
        private const float TRUSS_LENGTH = 2.46f;
        private const float TRUSS_THICKNESS = 0.46f;

        private const int LAYER = GameLayers.STAGE;

        [MenuItem("Tools/Trivias/Build/Stage Environment")]
        public static GameObject Build()
        {
            var gold = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Stage_Gold.mat", StagePalette.Gold, 0.7f, 0.8f);
            var wall = Mat("Stage_Wall", StagePalette.Purple * 0.55f, 0.3f);
            var ceiling = Mat("Stage_Ceiling", new Color(0.04f, 0.04f, 0.08f), 0.2f);

            var root = new GameObject("StageEnvironment") { layer = LAYER };
            var t = root.transform;

            BuildFloor(t);
            BuildRoom(t, wall, ceiling);
            BuildQuestionScreen(t, gold);
            BuildBackdrop(t);
            BuildWalls(t);
            BuildCeiling(t);
            BuildStudioGear(t);
            BuildAudience(t);
            BuildPresenterMark(t);
            BuildCameraDais(t);
            BuildPostProcessing(t);

            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }

        // ---- Suelo: baldosas a ras (su cara superior en y = 0), colisión plana y aro dorado alrededor de la mesa ----

        private static void BuildFloor(Transform t)
        {
            var under = Mat("Stage_Floor", StagePalette.Navy * 0.6f, 0.4f);
            var dark = Mat("Stage_CenterDisc", StagePalette.Navy * 0.75f, 0.85f);
            var width = StageLayout.InnerHalfWidth * 2f;
            var depth = StageLayout.Depth - StageLayout.WALL_THICKNESS;
            var center = StageLayout.RoomCenter;

            // Base oscura bajo las baldosas (se ve en las juntas) y colisión plana a nivel del suelo.
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Floor", t, new Vector3(center.x, -TILE_HEIGHT - 0.1f, center.z),
                new Vector3(width, 0.2f, depth), under, LAYER, true, keepCollider: false);
            EditorAssets.AddBoxCollider("FloorCollider", t, new Vector3(center.x, -0.1f, center.z), new Vector3(width, 0.2f, depth), LAYER);

            // Baldosas de unos 4 m (el modelo mide 2 m), ajustadas para cubrir la sala de pared a pared.
            var tiles = EditorAssets.CreateEmpty("FloorTiles", t, Vector3.zero).transform;
            var columns = Mathf.CeilToInt(width / (TILE_SIZE * 2f));
            var rows = Mathf.CeilToInt(depth / (TILE_SIZE * 2f));
            var tileX = width / columns;
            var tileZ = depth / rows;
            for (var c = 0; c < columns; c++)
            {
                for (var r = 0; r < rows; r++)
                {
                    var position = new Vector3(-width * 0.5f + (c + 0.5f) * tileX, -TILE_HEIGHT,
                        center.z - depth * 0.5f + (r + 0.5f) * tileZ);
                    Model("StudioFloorTile", $"Tile{c + 1}_{r + 1}", tiles, position, 0f,
                        new Vector3(tileX / TILE_SIZE, 1f, tileZ / TILE_SIZE));
                }
            }

            // Disco oscuro bajo la mesa y aro dorado de cuatro cuartos de incrustación curva.
            var table = StageLayout.TablePosition;
            EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, "CenterDisc", t, table + Vector3.up * 0.004f,
                new Vector3(6.9f, 0.002f, 6.9f), dark, LAYER, true, keepCollider: false);
            var ring = EditorAssets.CreateEmpty("CenterInlay", t, table).transform;
            for (var i = 0; i < 4; i++)
                Model("CurvedFloorInlay", $"Quarter{i + 1}", ring, Vector3.zero, i * 90f, new Vector3(3.55f, 1f, 3.55f));

            // Línea dorada que marca el borde del plató delante del público.
            var edge = EditorAssets.CreateEmpty("StageEdgeInlay", t, Vector3.zero).transform;
            const int pieces = 8;
            var pieceLength = width / pieces;
            for (var i = 0; i < pieces; i++)
                Model("StraightFloorInlay", $"Edge{i + 1}", edge,
                    new Vector3(-width * 0.5f + (i + 0.5f) * pieceLength, 0f, StageLayout.AUDIENCE_BARRIER_Z + 0.7f), 0f,
                    new Vector3(pieceLength / 1.51f, 1f, 1.5f));
        }

        // ---- Sala: paredes y techo lisos detrás de los decorados ----

        private static void BuildRoom(Transform t, Material wall, Material ceiling)
        {
            var height = StageLayout.CEILING_HEIGHT;
            var halfWidth = StageLayout.HALF_WIDTH;
            var center = StageLayout.RoomCenter;
            var depth = StageLayout.Depth;
            const float thickness = StageLayout.WALL_THICKNESS;
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "WallNorth", t, new Vector3(0f, height * 0.5f, StageLayout.BACK_Z),
                new Vector3(halfWidth * 2f, height, thickness), wall, LAYER, true);
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "WallSouth", t, new Vector3(0f, height * 0.5f, StageLayout.FRONT_Z),
                new Vector3(halfWidth * 2f, height, thickness), wall, LAYER, true);
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "WallEast", t, new Vector3(halfWidth, height * 0.5f, center.z),
                new Vector3(thickness, height, depth), wall, LAYER, true);
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "WallWest", t, new Vector3(-halfWidth, height * 0.5f, center.z),
                new Vector3(thickness, height, depth), wall, LAYER, true);
            var roof = EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Ceiling", t, new Vector3(0f, height + 0.1f, center.z),
                new Vector3(halfWidth * 2f, 0.2f, depth), ceiling, LAYER, true, keepCollider: false);
            // El techo no tapa la luz del sol: el plató sigue iluminado.
            roof.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        // ---- Pantalla de preguntas ----

        /// <summary>
        /// Pantalla grande (GDD: la pregunta se ve en el plató): el modelo con su marco dorado, un fondo oscuro
        /// translúcido para leer bien (deja ver el resplandor de la pantalla), cabecera, pregunta, las 4 opciones en
        /// recuadros del color de su cabina y una línea de estado. El texto mira a -Z (hacia el centro del plató).
        /// </summary>
        private static void BuildQuestionScreen(Transform parent, Material gold)
        {
            var panel = EditorAssets.GetOrCreateTransparentMaterial($"{MODULE_ROOT}/Materials/Stage_Screen.mat",
                new Color(0.03f, 0.05f, 0.16f, 0.82f), 0.9f);
            // Antes que el texto (URP calcula la cola con _QueueOffset): el fondo nunca tapa la pregunta.
            panel.SetFloat("_QueueOffset", -10f);
            panel.renderQueue = (int)RenderQueue.Transparent - 10;

            var root = EditorAssets.CreateEmpty(SCREEN, parent, StageLayout.ScreenCenter).transform;
            // El modelo mira a +Z: se gira para que mire al plató y su cristal quede justo detrás del texto.
            const float s = StageLayout.SCREEN_MODEL_SCALE;
            const float displayFront = 0.66f;
            const float displayCenterY = 3.845f;
            Model("QuestionScreen", "Frame", root, new Vector3(0f, -displayCenterY * s, -0.24f + displayFront * s), 180f,
                Vector3.one * s, castShadows: false);
            Sign(EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Panel", root, new Vector3(0f, 0f, -0.25f),
                new Vector3(ScreenPanelSize.x, ScreenPanelSize.y, 0.01f), panel, LAYER, true, keepCollider: false));

            const float textZ = -0.3f;
            var display = StageFonts.Display();
            var message = EditorAssets.CreateWorldText("Message", root, "TRIVIAS", 18f, StagePalette.Gold,
                new Vector3(0f, 0f, textZ), Quaternion.identity, new Vector2(13.6f, 6f), display);
            message.characterSpacing = 4f;
            message.outlineWidth = 0.2f;
            message.outlineColor = new Color(0.25f, 0.12f, 0f);

            var content = EditorAssets.CreateEmpty("Content", root, Vector3.zero).transform;
            var header = EditorAssets.CreateWorldText("Header", content, "PREGUNTA 1/15", 5.4f, StagePalette.Gold,
                new Vector3(0f, 3.1f, textZ), Quaternion.identity, new Vector2(13.6f, 0.8f), display);
            header.characterSpacing = 3f;

            var question = EditorAssets.CreateWorldText("Question", content, "¿Pregunta?", 7f, Color.white,
                new Vector3(0f, 1.75f, textZ), Quaternion.identity, new Vector2(13.6f, 2f));
            question.fontStyle = FontStyles.Bold;
            question.enableAutoSizing = true; // Las preguntas largas encogen en vez de salirse.
            question.fontSizeMin = 4.5f;
            question.fontSizeMax = 7f;

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
                Sign(EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Box", option, new Vector3(0f, 0f, -0.26f),
                    new Vector3(6.7f, 1.35f, 0.02f), boxMaterial, LAYER, false, keepCollider: false));

                var text = EditorAssets.CreateWorldText("Text", option, $"{letter}  Opción", 5.5f, Color.white,
                    new Vector3(0f, 0f, textZ), Quaternion.identity, new Vector2(6.3f, 1.3f));
                text.fontStyle = FontStyles.Bold;
                text.enableAutoSizing = true;
                text.fontSizeMin = 3.4f;
                text.fontSizeMax = 5.5f;
                text.outlineWidth = 0.15f;
                text.outlineColor = Color.black;
            }

            var status = EditorAssets.CreateWorldText("Status", content, string.Empty, 6.5f, Color.white,
                new Vector3(0f, -2.95f, textZ), Quaternion.identity, new Vector2(13.6f, 1.3f));
            status.fontStyle = FontStyles.Bold;

            BuildCheck(root, gold);

            // Bombillas bajo la pantalla, como una marquesina.
            var marquee = EditorAssets.CreateEmpty("Marquee", root, new Vector3(0f, -displayCenterY * s - 0.45f, -0.25f)).transform;
            for (var i = 0; i < 4; i++)
                Model("StudioLightStrip", $"Strip{i + 1}", marquee, new Vector3((i - 1.5f) * 2.3f, 0f, 0f), 180f, Vector3.one * 1.45f,
                    castShadows: false);
        }

        /// <summary>Los carteles y recuadros de la pantalla no hacen sombra (se ven siempre limpios).</summary>
        private static void Sign(GameObject go) => go.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;

        /// <summary>Cheque gigante de la victoria (GDD 3.3): se muestra en la pantalla, que es lo que todos miran.</summary>
        private static void BuildCheck(Transform screen, Material gold)
        {
            var check = EditorAssets.CreateEmpty("Check", screen, Vector3.zero).transform;
            var paper = Mat("Stage_CheckPaper", new Color(0.96f, 0.93f, 0.80f), 0.3f);
            var ink = new Color(0.12f, 0.12f, 0.28f);
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Border", check, new Vector3(0f, 0f, -0.27f),
                new Vector3(12.8f, 6.1f, 0.02f), gold, LAYER, false, keepCollider: false);
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Paper", check, new Vector3(0f, 0f, -0.29f),
                new Vector3(12.4f, 5.7f, 0.02f), paper, LAYER, false, keepCollider: false);

            const float z = -0.34f;
            var bank = EditorAssets.CreateWorldText("Bank", check, "BANCO TRIVIAS", 5.4f, ink, new Vector3(-2.6f, 2.15f, z),
                Quaternion.identity, new Vector2(6.8f, 0.9f), StageFonts.Display());
            bank.alignment = TextAlignmentOptions.Left;
            var number = EditorAssets.CreateWorldText("Number", check, "Nº 000015 · CHEQUE DE CAMPEONES", 2.6f, ink,
                new Vector3(3.2f, 2.15f, z), Quaternion.identity, new Vector2(5.4f, 0.6f));
            number.alignment = TextAlignmentOptions.Right;

            var payeeLabel = EditorAssets.CreateWorldText("PayeeLabel", check, "PÁGUESE A:", 3f, ink, new Vector3(-4.6f, 0.95f, z),
                Quaternion.identity, new Vector2(2.6f, 0.6f));
            payeeLabel.alignment = TextAlignmentOptions.Left;
            var payee = EditorAssets.CreateWorldText("Payee", check, "Los supervivientes", 4.2f, ink, new Vector3(1.4f, 0.95f, z),
                Quaternion.identity, new Vector2(9.2f, 1f));
            payee.fontStyle = FontStyles.Bold | FontStyles.Italic;
            payee.alignment = TextAlignmentOptions.Left;

            var amountLabel = EditorAssets.CreateWorldText("AmountLabel", check, "LA CANTIDAD DE:", 3f, ink, new Vector3(-4.6f, -0.45f, z),
                Quaternion.identity, new Vector2(2.6f, 0.6f));
            amountLabel.alignment = TextAlignmentOptions.Left;
            var amount = EditorAssets.CreateWorldText("Amount", check, "0", 9.5f, new Color(0.10f, 0.45f, 0.20f), new Vector3(1.4f, -0.55f, z),
                Quaternion.identity, new Vector2(9.2f, 1.6f), StageFonts.Display());
            amount.alignment = TextAlignmentOptions.Left;

            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "SignatureLine", check, new Vector3(3.4f, -2.05f, -0.32f),
                new Vector3(4.6f, 0.04f, 0.01f), Mat("Stage_CheckInk", ink, 0.2f), LAYER, false, keepCollider: false);
            var signature = EditorAssets.CreateWorldText("Signature", check, "El Presentador", 4f, ink, new Vector3(3.4f, -1.65f, z),
                Quaternion.identity, new Vector2(4.6f, 0.8f));
            signature.fontStyle = FontStyles.Italic;
            check.gameObject.SetActive(false);
        }

        // ---- Fondo: telón rojo, bambalinas, cortinas recogidas, columnas y altavoces junto a la pantalla ----

        private static void BuildBackdrop(Transform t)
        {
            var backdrop = EditorAssets.CreateEmpty("Backdrop", t, Vector3.zero).transform;
            var curtainZ = StageLayout.BACK_Z - 0.5f;
            var height = StageLayout.CEILING_HEIGHT;
            // Telón (estirado en alto hasta el techo: las telas lo permiten) y bambalinas arriba.
            for (var i = 0; i < 4; i++)
                Model("StraightCurtain", $"Curtain{i + 1}", backdrop, new Vector3((i - 1.5f) * 6.3f, 0f, curtainZ), 0f,
                    new Vector3(2.6f, height / 2.28f, 2f), castShadows: false);
            for (var i = 0; i < 4; i++)
                Model("UpperCurtain", $"Valance{i + 1}", backdrop, new Vector3((i - 1.5f) * 7.2f, height - 2.25f, curtainZ - 0.45f), 0f,
                    Vector3.one * 2.4f, castShadows: false);

            // Cortinas recogidas a los lados de la pantalla (la derecha, en espejo).
            var sideX = StageLayout.SCREEN_FRAME_HALF_WIDTH + 2.4f;
            Model("SideCurtain", "SideCurtainLeft", backdrop, new Vector3(-sideX, 0f, curtainZ - 0.5f), 0f,
                new Vector3(3f, height / 2.27f, 3f), castShadows: false);
            Model("SideCurtain", "SideCurtainRight", backdrop, new Vector3(sideX, 0f, curtainZ - 0.5f), 0f,
                new Vector3(-3f, height / 2.27f, 3f), castShadows: false);

            // Columnas que enmarcan la pantalla (justo por fuera del marco) y altavoces apilados delante del telón.
            var screenZ = StageLayout.ScreenCenter.z;
            foreach (var side in new[] { -1f, 1f })
            {
                var name = side < 0 ? "Left" : "Right";
                var columnX = side * (StageLayout.SCREEN_FRAME_HALF_WIDTH + 0.7f);
                Model("StudioColumn", $"ScreenColumn{name}", backdrop, new Vector3(columnX, 0f, screenZ + 1f), 0f,
                    new Vector3(1.3f, (height - 0.4f) / 2.25f, 1.3f));
                EditorAssets.AddCylinderCollider($"ScreenColumn{name}Collider", backdrop, new Vector3(columnX, 2f, screenZ + 1f), 1.2f, 4f, LAYER);

                var speakerX = side * (StageLayout.SCREEN_FRAME_HALF_WIDTH + 2.5f);
                var speakerYaw = 180f + side * 20f;
                for (var level = 0; level < 2; level++)
                    Model("StudioSpeaker", $"Speaker{name}{level + 1}", backdrop, new Vector3(speakerX, level * 1.6f, screenZ + 0.8f),
                        speakerYaw, Vector3.one * 2.2f);
                EditorAssets.AddBoxCollider($"Speaker{name}Collider", backdrop, new Vector3(speakerX, 1.6f, screenZ + 0.8f),
                    new Vector3(1.2f, 3.2f, 1.2f), LAYER, Quaternion.Euler(0f, speakerYaw, 0f));
            }
        }

        // ---- Paredes laterales y del fondo: paneles, columnas, rejillas y altavoces ----

        private static void BuildWalls(Transform t)
        {
            var walls = EditorAssets.CreateEmpty("WallDecor", t, Vector3.zero).transform;
            const float panelScale = 1.6f;
            const float panelWidth = PANEL_WIDTH * panelScale;
            var inner = StageLayout.InnerHalfWidth;
            var center = StageLayout.RoomCenter;
            var depth = StageLayout.Depth - StageLayout.WALL_THICKNESS;
            var count = Mathf.FloorToInt(depth / panelWidth);
            foreach (var side in new[] { -1f, 1f })
            {
                var name = side < 0 ? "West" : "East";
                var yaw = side < 0 ? 90f : -90f; // El frente del panel (+Z) mira al interior de la sala.
                var x = side * (inner - 0.2f);
                for (var i = 0; i < count; i++)
                    Model("StudioWallPanel", $"{name}Panel{i + 1}", walls,
                        new Vector3(x, 0f, center.z + (i - (count - 1) * 0.5f) * panelWidth), yaw, Vector3.one * panelScale,
                        castShadows: false);

                // Columnas entre las filas de cabinas y junto al público (no junto a los tubos del dinero).
                var columns = side < 0 ? new[] { -6.5f, 4.75f, 12.4f } : new[] { -6.5f, 4.75f };
                foreach (var z in columns)
                    Model("StudioColumn", $"{name}Column{z:+0;-0;0}", walls, new Vector3(side * (inner - 0.7f), 0f, z), 0f,
                        new Vector3(1.4f, StageLayout.CEILING_HEIGHT / 2.25f, 1.4f), castShadows: false);

                foreach (var z in new[] { -3f, 10f })
                    Model("VentGrille", $"{name}Vent{z:+0;-0}", walls, new Vector3(x, 6.6f, z), yaw, Vector3.one * 1.8f, castShadows: false);
                foreach (var z in new[] { -10f, -1f })
                    Model("StudioSpeaker", $"{name}WallSpeaker{z:+0;-0}", walls, new Vector3(side * (inner - 0.4f), 4.6f, z), yaw,
                        Vector3.one * 1.8f, castShadows: false);
            }

            // Pared del fondo, detrás del público: paneles a lo ancho.
            var southCount = Mathf.FloorToInt(inner * 2f / panelWidth);
            for (var i = 0; i < southCount; i++)
                Model("StudioWallPanel", $"SouthPanel{i + 1}", walls,
                    new Vector3((i - (southCount - 1) * 0.5f) * panelWidth, 0f, StageLayout.FRONT_Z + StageLayout.WALL_THICKNESS * 0.5f + 0.2f),
                    0f, Vector3.one * panelScale, castShadows: false);
        }

        // ---- Techo: paneles con la cara de luz hacia abajo, vigas, abrazaderas y focos ----

        private static void BuildCeiling(Transform t)
        {
            var rig = EditorAssets.CreateEmpty("CeilingRig", t, Vector3.zero).transform;
            var height = StageLayout.CEILING_HEIGHT;
            const float beamScale = 2f;
            const float beamLength = TRUSS_LENGTH * beamScale;
            var trussTop = StageLayout.TRUSS_Y + TRUSS_THICKNESS * beamScale;
            var beams = Mathf.RoundToInt(StageLayout.TRUSS_HALF_LENGTH * 2f / beamLength);
            var cable = Mat("Stage_Cable", new Color(0.08f, 0.08f, 0.1f), 0.3f);

            foreach (var z in StageLayout.TrussZ)
            {
                for (var i = 0; i < beams; i++)
                    Model("TrussBeam", $"Truss{z:+0;-0}_{i + 1}", rig, new Vector3((i - (beams - 1) * 0.5f) * beamLength, StageLayout.TRUSS_Y, z),
                        0f, Vector3.one * beamScale, castShadows: false);
                foreach (var x in new[] { -StageLayout.TRUSS_HALF_LENGTH + 0.6f, StageLayout.TRUSS_HALF_LENGTH - 0.6f })
                {
                    var hanger = EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, $"Hanger{z:+0;-0}{x:+0;-0}", rig,
                        new Vector3(x, (trussTop + height) * 0.5f, z), new Vector3(0.06f, (height - trussTop) * 0.5f, 0.06f),
                        cable, LAYER, true, keepCollider: false);
                    hanger.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                }

                // Focos pequeños de adorno a lo largo de la viga, apuntando al plató.
                foreach (var x in new[] { -6.6f, -1.3f, 1.3f, 6.6f })
                {
                    var position = new Vector3(x, StageLayout.TRUSS_Y - 0.45f, z);
                    var look = Quaternion.LookRotation(new Vector3(x * 0.4f, 0f, StageLayout.TablePosition.z) - position);
                    EditorAssets.PlaceModel($"{MODELS}/SmallSpotlight/SmallSpotlight.obj", $"SmallSpot{z:+0;-0}{x:+0;-0}", rig,
                        position, look, Vector3.one * 1.4f, LAYER, true, castShadows: false);
                }
            }

            // Abrazaderas que sujetan los focos de verdad a la viga (la luz la pone el módulo Feedback).
            for (var i = 0; i < StageLayout.SpotPositions.Length; i++)
            {
                var spot = StageLayout.SpotPositions[i];
                const float clampScale = 1.2f;
                Model("SpotlightClamp", $"SpotClamp{i + 1}", rig, new Vector3(spot.x, StageLayout.TRUSS_Y - CLAMP_HEIGHT * clampScale, spot.z), 0f,
                    Vector3.one * clampScale, castShadows: false);
            }

            // Paneles de techo: los soportes contra el techo y la cara lisa con su marco dorado hacia abajo.
            // Repartidos entre los tubos (que atraviesan el techo sobre cabinas y plataformas).
            const float panelScale = 2f;
            foreach (var x in new[] { -7.2f, -2.4f, 2.4f, 7.2f })
            {
                foreach (var z in new[] { -4f, StageLayout.TablePosition.z, 11f })
                    Model("CeilingPanel", $"CeilingPanel{x:+0;-0}{z:+0;-0}", rig,
                        new Vector3(x, height - CEILING_PANEL_HEIGHT * panelScale, z), 0f, Vector3.one * panelScale, castShadows: false);
            }
        }

        // ---- Cámaras de estudio en el suelo, mirando a la mesa ----

        private static void BuildStudioGear(Transform t)
        {
            var gear = EditorAssets.CreateEmpty("StudioCameras", t, Vector3.zero).transform;
            var inner = StageLayout.InnerHalfWidth;
            var positions = new[]
            {
                new Vector3(-inner + 2.2f, 0f, StageLayout.AUDIENCE_BARRIER_Z + 2.2f),
                new Vector3(inner - 2.2f, 0f, StageLayout.AUDIENCE_BARRIER_Z + 2.2f),
                new Vector3(-inner + 1.8f, 0f, StageLayout.ScreenCenter.z - 2.6f),
            };
            for (var i = 0; i < positions.Length; i++)
            {
                var yaw = StageLayout.Yaw(positions[i], StageLayout.TablePosition);
                Model("StudioCamera", $"Camera{i + 1}", gear, positions[i], yaw, Vector3.one * 1.5f);
                EditorAssets.AddBoxCollider($"Camera{i + 1}Collider", gear, positions[i] + Vector3.up * 0.95f,
                    new Vector3(1.1f, 1.9f, 1.3f), LAYER, Quaternion.Euler(0f, yaw, 0f));
            }
        }

        // ---- Público en gradas, detrás de una barandilla ----

        private static void BuildAudience(Transform t)
        {
            var audience = EditorAssets.CreateEmpty("Audience", t, Vector3.zero).transform;
            var riser = Mat("Stage_Riser", StagePalette.Navy * 0.6f, 0.4f);
            var seat = Mat("Stage_Seat", StagePalette.Burgundy, 0.3f);
            var shirts = new[]
            {
                Mat("Audience_Shirt1", new Color(0.55f, 0.16f, 0.2f), 0.2f), Mat("Audience_Shirt2", new Color(0.12f, 0.4f, 0.45f), 0.2f),
                Mat("Audience_Shirt3", new Color(0.7f, 0.55f, 0.18f), 0.2f), Mat("Audience_Shirt4", new Color(0.32f, 0.2f, 0.45f), 0.2f),
                Mat("Audience_Shirt5", new Color(0.25f, 0.27f, 0.32f), 0.2f),
            };
            var skins = new[]
            {
                Mat("Audience_Skin1", StagePalette.Skin, 0.3f), Mat("Audience_Skin2", new Color(0.72f, 0.5f, 0.35f), 0.3f),
                Mat("Audience_Skin3", new Color(0.42f, 0.28f, 0.2f), 0.3f),
            };
            var hair = Mat("Audience_Hair", new Color(0.15f, 0.1f, 0.07f), 0.3f);

            // Tres gradas desde la barandilla hasta la pared del fondo.
            const int rows = 3;
            var inner = StageLayout.InnerHalfWidth;
            var rowDepth = (StageLayout.AUDIENCE_BARRIER_Z - 0.6f - (StageLayout.FRONT_Z + StageLayout.WALL_THICKNESS * 0.5f + 0.45f)) / rows;
            var perRow = Mathf.FloorToInt((inner * 2f - 2.4f) / 1.15f);
            var random = new System.Random(7);
            for (var row = 0; row < rows; row++)
            {
                var height = 0.45f * (row + 1);
                var z = StageLayout.AUDIENCE_BARRIER_Z - 0.6f - (row + 0.5f) * rowDepth;
                EditorAssets.CreatePrimitive(PrimitiveType.Cube, $"Riser{row + 1}", audience, new Vector3(0f, height * 0.5f, z),
                    new Vector3(inner * 2f, height, rowDepth), riser, LAYER, true, keepCollider: false);
                for (var i = 0; i < perRow; i++)
                {
                    var x = -(perRow - 1) * 0.575f + i * 1.15f + (row % 2) * 0.3f;
                    var person = EditorAssets.CreateEmpty($"Person{row + 1}_{i + 1}", audience, new Vector3(x, height, z)).transform;
                    Part(person, PrimitiveType.Cube, "Seat", new Vector3(0f, 0.22f, 0f), new Vector3(0.7f, 0.44f, 0.6f), seat);
                    if (random.NextDouble() < 0.1)
                        continue; // Algún asiento vacío.
                    var tall = 0.9f + (float)random.NextDouble() * 0.2f;
                    Part(person, PrimitiveType.Capsule, "Body", new Vector3(0f, 0.44f + tall * 0.45f, 0f), new Vector3(0.5f, tall * 0.45f, 0.36f),
                        shirts[random.Next(shirts.Length)]);
                    var headY = 0.44f + tall * 0.9f + 0.12f;
                    Part(person, PrimitiveType.Sphere, "Head", new Vector3(0f, headY, 0f), Vector3.one * 0.32f, skins[random.Next(skins.Length)]);
                    Part(person, PrimitiveType.Sphere, "Hair", new Vector3(0f, headY + 0.07f, -0.04f), new Vector3(0.34f, 0.24f, 0.32f), hair);
                }
            }

            // Barandilla dorada que separa al público del plató, con una colisión continua de pared a pared.
            var railings = EditorAssets.CreateEmpty("AudienceRailing", t, Vector3.zero).transform;
            const float railScale = 1.5f;
            const float railWidth = 1.39f * railScale;
            var railCount = Mathf.FloorToInt(inner * 2f / railWidth);
            for (var i = 0; i < railCount; i++)
                Model("StageRailing", $"Railing{i + 1}", railings,
                    new Vector3((i - (railCount - 1) * 0.5f) * railWidth, 0f, StageLayout.AUDIENCE_BARRIER_Z), 180f,
                    Vector3.one * railScale, castShadows: false);
            EditorAssets.AddBoxCollider("RailingCollider", railings, new Vector3(0f, 0.7f, StageLayout.AUDIENCE_BARRIER_Z),
                new Vector3(inner * 2f, 1.4f, 0.4f), LAYER);
        }

        private static void Part(Transform parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material)
        {
            var part = EditorAssets.CreatePrimitive(type, name, parent, position, scale, material, LAYER, true, keepCollider: false);
            part.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        // ---- Marca del presentador y plataforma de cámara ----

        /// <summary>Aro dorado en el suelo donde se coloca el presentador, entre la pantalla y las estaciones.</summary>
        private static void BuildPresenterMark(Transform t)
        {
            var mark = EditorAssets.CreateEmpty("PresenterMark", t, StageLayout.PresenterSpot).transform;
            for (var i = 0; i < 4; i++)
                Model("CurvedFloorInlay", $"Quarter{i + 1}", mark, Vector3.zero, i * 90f, new Vector3(1.25f, 1f, 1.25f));
        }

        /// <summary>
        /// Plataforma redonda (borde curvo de escenario en círculo) con escalera y una cámara de estudio encima, en la
        /// esquina junto a la pantalla, mirando a la mesa.
        /// </summary>
        private static void BuildCameraDais(Transform t)
        {
            var deck = Mat("Stage_DaisDeck", StagePalette.Purple * 0.8f, 0.6f);
            var spot = StageLayout.CameraDaisSpot;
            var dais = EditorAssets.CreateEmpty("CameraDais", t, spot).transform;
            for (var i = 0; i < 6; i++)
                Model("StageEdge", $"Edge{i + 1}", dais, Vector3.zero, i * 60f, Vector3.one);
            // El suelo llena el aro por dentro, un poco por debajo de sus baldosas (sin parpadeos).
            const float inner = 1.28f * 2f;
            EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, "Deck", dais, new Vector3(0f, (DAIS_HEIGHT - 0.02f) * 0.5f, 0f),
                new Vector3(inner, (DAIS_HEIGHT - 0.02f) * 0.5f, inner), deck, LAYER, true, keepCollider: false);
            EditorAssets.AddCylinderCollider("DaisCollider", dais, new Vector3(0f, DAIS_HEIGHT * 0.5f, 0f), DAIS_RADIUS * 2f,
                DAIS_HEIGHT, LAYER);

            // Escalera hacia el centro del plató (el peldaño bajo, fuera) y la cámara encima, mirando a la mesa.
            var yaw = StageLayout.Yaw(spot, StageLayout.TablePosition);
            var facing = Quaternion.Euler(0f, yaw, 0f);
            var stairs = facing * Vector3.forward * (DAIS_RADIUS + 0.5f);
            Model("StageStairs", "Stairs", dais, stairs, yaw, new Vector3(1f, 0.9f, 1f));
            EditorAssets.AddBoxCollider("StairsCollider", dais, stairs + Vector3.up * 0.15f, new Vector3(1.1f, 0.3f, 1.2f), LAYER, facing);
            Model("StudioCamera", "Camera", dais, Vector3.up * DAIS_HEIGHT, yaw, Vector3.one * 1.5f);
        }

        // ---- Posprocesado: brillo de bombillas y focos, viñeta suave ----

        private static void BuildPostProcessing(Transform t)
        {
            var profile = EditorAssets.GetOrCreateAsset<VolumeProfile>(VOLUME_PROFILE_PATH);
            var bloom = Effect<Bloom>(profile);
            bloom.threshold.Override(1f);
            bloom.intensity.Override(0.8f);
            bloom.scatter.Override(0.7f);
            var vignette = Effect<Vignette>(profile);
            vignette.intensity.Override(0.28f);
            vignette.smoothness.Override(0.45f);
            var color = Effect<ColorAdjustments>(profile);
            color.saturation.Override(12f);
            color.contrast.Override(8f);
            EditorUtility.SetDirty(profile);

            var go = EditorAssets.CreateEmpty("PostProcessing", t, Vector3.zero);
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
        }

        /// <summary>Añade el efecto al perfil una sola vez (los constructores se pueden repetir).</summary>
        private static T Effect<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet<T>(out var effect))
                return effect;
            effect = profile.Add<T>(true);
            effect.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(effect, profile);
            return effect;
        }

        // ---- Utilidades ----

        private static GameObject Model(string model, string name, Transform parent, Vector3 position, float yaw, Vector3 scale,
            bool castShadows = true) =>
            EditorAssets.PlaceModel($"{MODELS}/{model}/{model}.obj", name, parent, position, Quaternion.Euler(0f, yaw, 0f), scale,
                LAYER, true, castShadows);

        private static Material Mat(string name, Color color, float smoothness) =>
            EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/{name}.mat", color, smoothness);
    }
}
