using System.Collections.Generic;
using TMPro;
using Trivias.Shared.Editor;
using Trivias.Shared.Physics;
using UnityEditor;
using UnityEngine;

namespace Trivias.Booth.Editor
{
    /// <summary>
    /// Cabina de respuesta con los modelos del módulo (concepto-01). El frente mira a +Z local (al centro del plató).
    /// <list type="bullet">
    /// <item>Mueble de su color con bombillas; la respuesta se ve en su hueco como una pantallita; el dinero, en la
    /// bandeja del mostrador.</item>
    /// <item>Tubo del dinero: sale del techo del mueble y sube hasta el techo del plató.</item>
    /// <item>Plataforma de apuesta delante, con barandilla abierta hacia el centro y el color de la respuesta.</item>
    /// <item>Tubo del concursante: cuelga del techo justo encima de la plataforma; el expulsado sale disparado hacia
    /// arriba y entra por su boca.</item>
    /// </list>
    /// Un prefab base con lo común y una variante por respuesta (A/B/C/D) con su mueble y el color de su plataforma.
    /// </summary>
    public static class BoothPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Booth";
        private const string MATERIALS = MODULE_ROOT + "/Materials";
        private const string PREFABS = MODULE_ROOT + "/Prefabs";
        private const string MODELS = MODULE_ROOT + "/Models";
        public const string BASE_PREFAB_PATH = PREFABS + "/Booth.prefab";

        public const string BODY = "Body";
        public const string BET_ZONE = "BetZone";
        public const string PLATFORM = "Platform";
        public const string BETTORS_LABEL = "BettorsLabel";
        public const string TRAY = "Tray";
        public const string TRAY_SURFACE = "Tray/Surface";
        public const string TRAY_TOTAL_LABEL = "TotalLabel";
        public const string ANSWER_LABEL = "AnswerLabel";
        public const string MONEY_TUBE = "MoneyTube";
        public const string PLAYER_TUBE = "PlayerTube";

        /// <summary>Los modelos miden 2,15 m de alto; a 1,3 la cabina queda algo más alta que el concursante, como en el boceto.</summary>
        public const float MODEL_SCALE = 1.3f;

        // Medidas del modelo de la cabina (sin escalar, en coordenadas de Unity).
        private const float CABINET_HEIGHT = 2.15f;
        private const float TRAY_FLOOR_HEIGHT = 0.94f;
        private const float TRAY_CENTER_Z = 0.44f;

        /// <summary>Plataforma de apuesta: 2,6 m de ancho (caben seis) y 22 cm de alto, que se sube andando (y saltando).</summary>
        public const float BET_ZONE_Z = 2.7f;
        private static readonly Vector3 PlatformScale = new(1.8f, 0.9f, 1.8f);
        public const float PLATFORM_TOP = 0.21f;
        private const float PLATFORM_RADIUS = 0.72f * 1.8f;

        /// <summary>Boca del tubo del concursante: por encima de la cabeza incluso saltando.</summary>
        public const float PLAYER_TUBE_MOUTH = 3.4f;
        private const float PLAYER_TUBE_SCALE = 1.6f;

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
            var zone = EditorAssets.GetOrCreateMaterial($"{MATERIALS}/Booth_ZoneNeutral.mat", Color.gray * 0.6f);

            var root = new GameObject("Booth") { layer = GameLayers.STAGE };
            var t = root.transform;
            const int layer = GameLayers.STAGE;
            const float s = MODEL_SCALE;

            // Colisiones sencillas del mueble y del mostrador (el modelo no lleva colisiones).
            EditorAssets.AddBoxCollider("ShellCollider", t, new Vector3(0f, CABINET_HEIGHT * s * 0.5f, -0.03f * s),
                new Vector3(1.82f * s, CABINET_HEIGHT * s, 1.16f * s), layer);
            EditorAssets.AddBoxCollider("CounterCollider", t, new Vector3(0f, 0.52f * s, 0.45f * s),
                new Vector3(1.46f * s, 1.04f * s, 1.02f * s), layer);

            // La respuesta, dentro del hueco oscuro de la cabina, como una pantallita (la rellena el módulo Match).
            // Texto mirando a +Z (hacia el centro del plató): TMP se lee mirando a su +Z, por eso gira 180°.
            var answer = EditorAssets.CreateWorldText(ANSWER_LABEL, t, "?", 3f, Color.white,
                new Vector3(0f, 2.0f, 0.1f), Quaternion.Euler(0f, 180f, 0f), new Vector2(1.75f, 1.05f));
            answer.fontStyle = FontStyles.Bold;
            answer.outlineWidth = 0.18f;
            answer.outlineColor = Color.black;
            answer.textWrappingMode = TextWrappingModes.Normal;
            answer.enableAutoSizing = true;
            answer.fontSizeMin = 1.2f;
            answer.fontSizeMax = 3f;

            // Nombres de los apostados, sobre la cabina; los rellena el módulo SelfBet.
            var bettors = EditorAssets.CreateWorldText(BETTORS_LABEL, t, string.Empty, 3f, StagePalette.Yellow,
                new Vector3(0f, CABINET_HEIGHT * s + 0.55f, 1.1f), Quaternion.Euler(0f, 180f, 0f), new Vector2(4.2f, 0.9f));
            bettors.fontStyle = FontStyles.Bold;
            bettors.outlineWidth = 0.22f;
            bettors.outlineColor = Color.black;
            bettors.textWrappingMode = TextWrappingModes.Normal;

            // La bandeja es la del mostrador del modelo: su suelo marca dónde se apila el dinero.
            var tray = EditorAssets.CreateEmpty(TRAY, t, new Vector3(0f, 0f, TRAY_CENTER_Z * s)).transform;
            EditorAssets.CreateEmpty("Surface", tray, new Vector3(0f, TRAY_FLOOR_HEIGHT * s, 0f));
            var trayTotal = EditorAssets.CreateWorldText(TRAY_TOTAL_LABEL, tray, string.Empty, 3.2f, StagePalette.Gold,
                new Vector3(0f, TRAY_FLOOR_HEIGHT * s + 0.75f, 0.35f), Quaternion.Euler(0f, 180f, 0f), new Vector2(1.6f, 0.5f));
            trayTotal.fontStyle = FontStyles.Bold;
            trayTotal.outlineWidth = 0.2f;
            trayTotal.outlineColor = Color.black;

            // Plataforma de apuesta: barandilla abierta hacia el centro (+Z del modelo), suelo que se pisa y su colisión.
            EditorAssets.PlaceModel($"{MODELS}/PlayerPlatform/PlayerPlatform.obj", PLATFORM, t, new Vector3(0f, 0f, BET_ZONE_Z),
                Quaternion.identity, PlatformScale, layer, true);
            EditorAssets.AddCylinderCollider("PlatformCollider", t, new Vector3(0f, PLATFORM_TOP * 0.5f, BET_ZONE_Z),
                PLATFORM_RADIUS * 2f, PLATFORM_TOP, layer);
            // El color de la respuesta sobre el suelo de la plataforma (es la zona de apuesta).
            EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, BET_ZONE, t, new Vector3(0f, PLATFORM_TOP + 0.012f, BET_ZONE_Z),
                new Vector3(1.75f, 0.002f, 1.75f), zone, layer, true, keepCollider: false);

            BuildMoneyTube(t, layer);
            BuildPlayerTube(t, layer);

            return EditorAssets.SaveAsPrefab(root, BASE_PREFAB_PATH);
        }

        private static GameObject BuildVariant(GameObject basePrefab, string letter, Color color)
        {
            var zone = EditorAssets.GetOrCreateMaterial($"{MATERIALS}/Booth_{letter}_Zone.mat", color * 0.75f, 0.5f,
                emission: color * 0.35f);

            return EditorAssets.SaveVariant(basePrefab, $"{PREFABS}/Booth_{letter}.prefab", instance =>
            {
                instance.name = $"Booth_{letter}";
                var body = EditorAssets.PlaceModel($"{MODELS}/BoothCabinet_{letter}/BoothCabinet_{letter}.obj", BODY,
                    instance.transform, Vector3.zero, Quaternion.identity, Vector3.one * MODEL_SCALE, GameLayers.STAGE, true);
                body.transform.SetAsFirstSibling();
                instance.transform.Find(BET_ZONE).GetComponent<MeshRenderer>().sharedMaterial = zone;
            });
        }

        /// <summary>
        /// Tubo del dinero: sale del techo del mueble, sube, se retira hacia atrás (hacia la pared) y sube hasta el techo.
        /// Las cabinas están a los lados del plató, así que no tapa la pantalla.
        /// </summary>
        private static void BuildMoneyTube(Transform parent, int layer)
        {
            var tube = EditorAssets.CreateEmpty(MONEY_TUBE, parent, Vector3.zero).transform;
            var run = new TubeRun(tube, new Vector3(0f, CABINET_HEIGHT * MODEL_SCALE - 0.02f, 0f), Vector3.up, MODEL_SCALE, layer);
            run.Ring();
            run.Straight(1f);
            run.Elbow(Vector3.back);
            run.Elbow(Vector3.up);
            run.Straight(StageLayout.CEILING_HEIGHT - run.Point.y);
        }

        /// <summary>Tubo del concursante: baja del techo hasta encima de la plataforma y acaba en una boca dorada.</summary>
        private static void BuildPlayerTube(Transform parent, int layer)
        {
            var tube = EditorAssets.CreateEmpty(PLAYER_TUBE, parent, Vector3.zero).transform;
            var run = new TubeRun(tube, new Vector3(0f, StageLayout.CEILING_HEIGHT, BET_ZONE_Z), Vector3.down, PLAYER_TUBE_SCALE, layer);
            run.Straight(StageLayout.CEILING_HEIGHT - PLAYER_TUBE_MOUTH);
            run.Ring();
        }

        /// <summary>
        /// Encadena piezas de tubo: cada una empieza donde acaba la anterior y sigue su dirección. Las juntas están en el
        /// centro de los aros dorados de cada pieza; donde dos aros coincidirían, el de la pieza nueva se oculta (un
        /// solo aro por junta).
        /// </summary>
        private sealed class TubeRun
        {
            private const string TUBES = MODELS + "/ExpulsionTube";

            // Geometría de las piezas sin escalar (coordenadas de Unity), medida de sus mallas.
            // Codo: boca lateral en x = 0 (se entra hacia -X) y boca superior en (-0,7; 1,08) (se sale hacia +Y).
            private static readonly Vector3 ElbowInlet = new(0f, 0.38f, 0f);
            private static readonly Vector3 ElbowOutlet = new(-0.7f, 1.08f, 0f);
            private const string ELBOW_INLET_COLLAR = "HorizontalGoldCollar";
            private const string ELBOW_OUTLET_COLLAR = "VerticalGoldCollar";
            // Recto: a lo largo de +X, con un aro centrado en cada extremo (x = ±0,625).
            private static readonly Vector3 StraightInlet = new(-0.625f, 0.38f, 0f);
            private const float STRAIGHT_LENGTH = 1.25f;
            private const string STRAIGHT_INLET_COLLAR = "GoldEndCollar0.625";
            private const string STRAIGHT_OUTLET_COLLAR = "GoldEndCollar-0.625";
            private const float RING_HALF_HEIGHT = 0.065f;

            private readonly Transform _parent;
            private readonly float _scale;
            private readonly int _layer;
            private int _count;
            private bool _collarAtPoint;
            private GameObject _lastOutletCollar;

            public TubeRun(Transform parent, Vector3 start, Vector3 direction, float scale, int layer)
            {
                _parent = parent;
                Point = start;
                Direction = direction;
                _scale = scale;
                _layer = layer;
            }

            public Vector3 Point { get; private set; }
            public Vector3 Direction { get; private set; }

            /// <summary>Anillo dorado en la junta actual (no avanza); sustituye al aro de la pieza anterior.</summary>
            public void Ring()
            {
                if (_lastOutletCollar != null)
                    _lastOutletCollar.SetActive(false);
                var rotation = Quaternion.FromToRotation(Vector3.up, Direction);
                Place("Connector", Point - rotation * (Vector3.up * (RING_HALF_HEIGHT * _scale)), rotation, Vector3.one * _scale);
                _collarAtPoint = true;
                _lastOutletCollar = null;
            }

            /// <summary>Tramo recto de la longitud pedida en metros (estira la pieza solo a lo largo).</summary>
            public void Straight(float length)
            {
                var up = Mathf.Abs(Direction.y) > 0.9f ? Vector3.forward : Vector3.up;
                var rotation = Quaternion.LookRotation(Vector3.Cross(Direction, up), up);
                var scale = new Vector3(length / STRAIGHT_LENGTH, _scale, _scale);
                var piece = Place("Straight", Point - rotation * Vector3.Scale(scale, StraightInlet), rotation, scale);
                Joint(piece, STRAIGHT_INLET_COLLAR, STRAIGHT_OUTLET_COLLAR);
                Point += Direction * length;
            }

            /// <summary>Codo de 90°: entra con la dirección actual y sale hacia <paramref name="newDirection"/>.</summary>
            public void Elbow(Vector3 newDirection)
            {
                // La pieza entra hacia -X y sale hacia +Y: se gira para que -X sea la dirección actual y +Y la nueva.
                var rotation = Quaternion.LookRotation(Vector3.Cross(-Direction, newDirection), newDirection);
                var position = Point - rotation * (ElbowInlet * _scale);
                var piece = Place("Elbow", position, rotation, Vector3.one * _scale);
                Joint(piece, ELBOW_INLET_COLLAR, ELBOW_OUTLET_COLLAR);
                Point = position + rotation * (ElbowOutlet * _scale);
                Direction = newDirection;
            }

            /// <summary>Un solo aro por junta: si ya hay uno donde empieza la pieza, se oculta el suyo.</summary>
            private void Joint(GameObject piece, string inletCollar, string outletCollar)
            {
                if (_collarAtPoint)
                    piece.transform.Find(inletCollar).gameObject.SetActive(false);
                _collarAtPoint = true;
                _lastOutletCollar = piece.transform.Find(outletCollar).gameObject;
            }

            // Los tubos son decorado alto: sin sombras (no oscurecen el plató) y estáticos.
            private GameObject Place(string piece, Vector3 position, Quaternion rotation, Vector3 scale) =>
                EditorAssets.PlaceModel($"{TUBES}{piece}/ExpulsionTube{piece}.obj", $"{piece}{++_count}", _parent,
                    position, rotation, scale, _layer, true, castShadows: false);
        }
    }
}
