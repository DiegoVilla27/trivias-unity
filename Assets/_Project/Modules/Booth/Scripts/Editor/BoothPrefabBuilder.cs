using System.Collections.Generic;
using TMPro;
using Trivias.Shared.Editor;
using Trivias.Shared.Physics;
using Trivias.Shared.Tubes;
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
        public const string MONEY_TUBE = "MoneyTube";
        public const string PLAYER_TUBE = "PlayerTube";

        /// <summary>Los modelos miden 2,15 m de alto; a 1,3 la cabina queda algo más alta que el concursante, como en el boceto.</summary>
        public const float MODEL_SCALE = 1.3f;

        // Medidas del modelo de la cabina (sin escalar, en coordenadas de Unity).
        private const float CABINET_HEIGHT = 2.15f;
        private const float TRAY_FLOOR_HEIGHT = 0.94f;
        private const float TRAY_CENTER_Z = 0.44f;
        /// <summary>Hueco oscuro de la parte alta del mueble (ya escalado): ahí se ve el dinero de la bandeja.</summary>
        private const float WINDOW_CENTER_HEIGHT = 2.0f;
        private const float WINDOW_FRONT_Z = 0.1f;

        /// <summary>Plataforma de apuesta: 2,6 m de ancho (caben seis) y 22 cm de alto, que se sube andando (y saltando).</summary>
        /// <remarks>
        /// Al lado derecho de la cabina mirándola desde el centro del plató (−X del modelo), a la altura del mostrador:
        /// así no tapa el mueble. La entrada de la barandilla sigue mirando al centro.
        /// </remarks>
        public static readonly Vector3 BetZoneOffset = new(-2.63f, 0f, 0.6f);
        private static readonly Vector3 PlatformScale = new(1.8f, 0.9f, 1.8f);
        public const float PLATFORM_TOP = 0.21f;
        private const float PLATFORM_RADIUS = 0.72f * 1.8f;

        /// <summary>
        /// Barandilla del modelo: arco de radio 0,64 (×1,8) abierto 90° hacia el centro (entre los postes delanteros).
        /// Su colisión sube a 1,4 m para que tampoco se salte por encima (el salto llega a 0,9 m).
        /// </summary>
        private const float RAIL_RADIUS = 0.64f * 1.8f;
        private const float RAIL_OPENING_DEGREES = 45f;
        private const int RAIL_SEGMENTS = 9;
        private const float RAIL_COLLIDER_HEIGHT = 1.4f;
        private const float RAIL_COLLIDER_DEPTH = 0.14f;
        public const string RAIL_COLLIDERS = "RailColliders";

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
            for (var i = 0; i < Answers.Length; i++)
                variants.Add(BuildVariant(basePrefab, i, Answers[i].Letter, Answers[i].Color));
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
            // El dinero de la bandeja, en grande dentro del hueco oscuro de la cabina, como una pantallita. La letra ya
            // está en el mueble y la respuesta en la pantalla grande: aquí solo la cantidad (la rellena Money).
            // Texto mirando a +Z (hacia el centro del plató): TMP se lee mirando a su +Z, por eso gira 180°.
            var trayTotal = EditorAssets.CreateWorldText(TRAY_TOTAL_LABEL, tray, string.Empty, 5f, StagePalette.Gold,
                new Vector3(0f, WINDOW_CENTER_HEIGHT, WINDOW_FRONT_Z - TRAY_CENTER_Z * s), Quaternion.Euler(0f, 180f, 0f),
                new Vector2(1.75f, 0.9f), StageFonts.Display());
            trayTotal.outlineWidth = 0.16f;
            trayTotal.outlineColor = new Color(0.12f, 0.07f, 0f);
            trayTotal.textWrappingMode = TextWrappingModes.NoWrap;
            trayTotal.enableAutoSizing = true;
            trayTotal.fontSizeMin = 2.4f;
            trayTotal.fontSizeMax = 5f;

            // Plataforma de apuesta: barandilla abierta hacia el centro (+Z del modelo), suelo que se pisa y su colisión.
            EditorAssets.PlaceModel($"{MODELS}/PlayerPlatform/PlayerPlatform.obj", PLATFORM, t, BetZoneOffset,
                Quaternion.identity, PlatformScale, layer, true);
            EditorAssets.AddCylinderCollider("PlatformCollider", t, BetZoneOffset + Vector3.up * (PLATFORM_TOP * 0.5f),
                PLATFORM_RADIUS * 2f, PLATFORM_TOP, layer);
            AddRailColliders(t, layer);
            // El color de la respuesta sobre el suelo de la plataforma (es la zona de apuesta).
            EditorAssets.CreatePrimitive(PrimitiveType.Cylinder, BET_ZONE, t, BetZoneOffset + Vector3.up * (PLATFORM_TOP + 0.012f),
                new Vector3(1.75f, 0.002f, 1.75f), zone, layer, true, keepCollider: false);

            BuildMoneyTube(t, layer);
            BuildPlayerTube(t, layer);

            return EditorAssets.SaveAsPrefab(root, BASE_PREFAB_PATH);
        }

        /// <summary>Paredes invisibles a lo largo de la barandilla: solo se entra (y se sale) por la abertura delantera.</summary>
        private static void AddRailColliders(Transform booth, int layer)
        {
            var holder = EditorAssets.CreateEmpty(RAIL_COLLIDERS, booth, BetZoneOffset).transform;
            var span = (360f - RAIL_OPENING_DEGREES * 2f) / RAIL_SEGMENTS;
            var width = 2f * RAIL_RADIUS * Mathf.Sin(span * 0.5f * Mathf.Deg2Rad) + 0.08f; // Se solapan un poco: sin rendijas.
            for (var i = 0; i < RAIL_SEGMENTS; i++)
            {
                var angle = RAIL_OPENING_DEGREES + span * (i + 0.5f);
                var direction = Quaternion.Euler(0f, angle, 0f);
                EditorAssets.AddBoxCollider($"Rail{i + 1}", holder, direction * new Vector3(0f, RAIL_COLLIDER_HEIGHT * 0.5f, RAIL_RADIUS),
                    new Vector3(width, RAIL_COLLIDER_HEIGHT, RAIL_COLLIDER_DEPTH), layer, direction);
            }
        }

        private static GameObject BuildVariant(GameObject basePrefab, int booth, string letter, Color color)
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
                // Cada tubo sabe de qué cabina es: el dinero de su bandeja y su concursante suben por él.
                SerializedWiring.Set(instance.transform.Find(MONEY_TUBE).GetComponent<TubePath>(), "_booth", booth);
                SerializedWiring.Set(instance.transform.Find(PLAYER_TUBE).GetComponent<TubePath>(), "_booth", booth);
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

            // El dinero perdido sale de la bandeja hacia el techo del mueble: el aire sopla de la bandeja a la entrada.
            var air = BuildAir(tube, "AirIntake", new Vector3(0f, TRAY_FLOOR_HEIGHT * MODEL_SCALE + 0.15f, TRAY_CENTER_Z * MODEL_SCALE),
                Quaternion.identity, new Vector3(1.3f, 0.1f, 0.8f), ParticleSystemShapeType.Box, new Vector3(0f, 3.2f, -1.4f),
                ParticleSystemSimulationSpace.Local, 0f, 0.55f);
            AddTubePath(tube, TubeKind.Money, run.Points, air, layer);
        }

        /// <summary>Tubo del concursante: baja del techo hasta encima de la plataforma y acaba en una boca dorada.</summary>
        private static void BuildPlayerTube(Transform parent, int layer)
        {
            var tube = EditorAssets.CreateEmpty(PLAYER_TUBE, parent, Vector3.zero).transform;
            var run = new TubeRun(tube, BetZoneOffset + Vector3.up * StageLayout.CEILING_HEIGHT, Vector3.down, PLAYER_TUBE_SCALE, layer);
            run.Straight(StageLayout.CEILING_HEIGHT - PLAYER_TUBE_MOUTH);
            run.Ring();

            // El recorrido va al revés que la construcción: de la boca (abajo) al techo. El aire sube desde la plataforma.
            var points = new List<Vector3>(run.Points);
            points.Reverse();
            var air = BuildAir(tube, "AirIntake", BetZoneOffset + Vector3.up * (PLATFORM_TOP + 0.15f), Quaternion.Euler(-90f, 0f, 0f),
                Vector3.one, ParticleSystemShapeType.Circle, new Vector3(0f, 6.5f, 0f), ParticleSystemSimulationSpace.World, -1.3f, 1.1f);
            AddTubePath(tube, TubeKind.Player, points, air, layer);
        }

        private static void AddTubePath(Transform tube, TubeKind kind, List<Vector3> points, ParticleSystem air, int layer)
        {
            var sound = EditorAssets.CreateEmpty("Suction", tube, points[0]);
            var audio = sound.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.loop = false;
            audio.spatialBlend = 0.8f; // Se oye más cerca de su cabina, pero todo el plató la oye.
            audio.minDistance = 4f;
            audio.maxDistance = 35f;
            audio.rolloffMode = AudioRolloffMode.Linear;
            var path = tube.gameObject.AddComponent<TubePath>();
            path.EditorSet(SharedAssets.TubeRegistry(), kind, 0, points.ToArray(), air, audio, SharedAssets.GameSettings());
        }

        /// <summary>
        /// Aire que entra en el tubo al succionar: estelas blancas finas que nacen alrededor y corren hacia la entrada.
        /// Sistema de partículas sin bucle: suena y sopla una vez por succión.
        /// </summary>
        private static ParticleSystem BuildAir(Transform parent, string name, Vector3 position, Quaternion rotation, Vector3 boxSize,
            ParticleSystemShapeType shapeType, Vector3 velocity, ParticleSystemSimulationSpace velocitySpace, float radial, float radius)
        {
            var go = EditorAssets.CreateEmpty(name, parent, position);
            go.transform.localRotation = rotation;
            var particles = go.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 1.6f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.75f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.11f);
            main.startColor = new Color(0.85f, 0.95f, 1f, 0.45f);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 200;

            var emission = particles.emission;
            emission.rateOverTime = 110f;

            var shape = particles.shape;
            shape.shapeType = shapeType;
            shape.radius = radius;
            shape.scale = boxSize;

            var speed = particles.velocityOverLifetime;
            speed.enabled = true;
            speed.space = velocitySpace; // En la cabina (hacia su mueble) o en el mundo (siempre hacia arriba).
            speed.x = new ParticleSystem.MinMaxCurve(velocity.x);
            speed.y = new ParticleSystem.MinMaxCurve(velocity.y);
            speed.z = new ParticleSystem.MinMaxCurve(velocity.z);
            speed.radial = new ParticleSystem.MinMaxCurve(radial);

            var fade = particles.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;

            var noise = particles.noise;
            noise.enabled = true;
            noise.strength = 0.5f;
            noise.frequency = 1.2f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch; // Estelas: se estiran en la dirección del aire.
            renderer.velocityScale = 0.12f;
            renderer.lengthScale = 2f;
            renderer.sharedMaterial = AirMaterial();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return particles;
        }

        /// <summary>Partícula aditiva suave para el aire (sombreador de partículas de URP).</summary>
        private static Material AirMaterial()
        {
            const string path = MATERIALS + "/Booth_Air.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")) { name = "Booth_Air" };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 2f); // Aditivo: el aire brilla un poco sobre el plató.
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.SetTexture("_BaseMap", AssetDatabase.GetBuiltinExtraResource<Texture2D>("Default-Particle.psd"));
            material.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(material);
            return material;
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
            private const float ELBOW_BEND_RADIUS = 0.7f; // De la boca lateral (0; 0,38) a la superior (-0,7; 1,08).
            private const int ELBOW_SAMPLES = 12;

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
                Points.Add(start);
            }

            public Vector3 Point { get; private set; }
            public Vector3 Direction { get; private set; }

            /// <summary>Recorrido interior (eje del tubo) desde el inicio: lo siguen el dinero y el concursante.</summary>
            public List<Vector3> Points { get; } = new();

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
                Points.Add(Point);
            }

            /// <summary>Codo de 90°: entra con la dirección actual y sale hacia <paramref name="newDirection"/>.</summary>
            public void Elbow(Vector3 newDirection)
            {
                // La pieza entra hacia -X y sale hacia +Y: se gira para que -X sea la dirección actual y +Y la nueva.
                var rotation = Quaternion.LookRotation(Vector3.Cross(-Direction, newDirection), newDirection);
                var position = Point - rotation * (ElbowInlet * _scale);
                var piece = Place("Elbow", position, rotation, Vector3.one * _scale);
                Joint(piece, ELBOW_INLET_COLLAR, ELBOW_OUTLET_COLLAR);

                // El eje del codo es un cuarto de circunferencia: se muestrea para que lo recorrido fluya sin esquinas.
                var radius = ELBOW_BEND_RADIUS * _scale;
                var center = Point + newDirection * radius;
                for (var step = 1; step <= ELBOW_SAMPLES; step++)
                {
                    var angle = step * Mathf.PI * 0.5f / ELBOW_SAMPLES;
                    Points.Add(center - newDirection * (radius * Mathf.Cos(angle)) + Direction * (radius * Mathf.Sin(angle)));
                }

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
