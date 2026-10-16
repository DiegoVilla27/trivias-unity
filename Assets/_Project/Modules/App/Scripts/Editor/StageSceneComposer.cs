using System.Collections.Generic;
using System.Linq;
using TMPro;
using Trivias.Booth.Editor;
using Trivias.Feedback.Data;
using Trivias.Feedback.Editor;
using Trivias.Match.Editor;
using Trivias.Money.Editor;
using Trivias.Networking.Components;
using Trivias.Networking.Editor;
using Trivias.SelfBet.Editor;
using Trivias.Shared.Editor;
using Trivias.Shared.Spawning;
using Trivias.Shared.Tubes;
using Trivias.Stage.Editor;
using Trivias.Table.Editor;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Trivias.App.Editor
{
    public sealed class StageScenePrefabs
    {
        public GameObject NetworkManager;
        public GameObject Player;
        public GameObject[] SessionPrefabs;
        public GameObject CameraRig;
        public GameObject InteractionHud;
        public GameObject MatchHud;
        public GameObject ResultsScreen;
        public GameObject Menu;
        public GameObject ShopUi;
        public GameObject AvatarCosmetics;
        public GameObject GameFeedback;
        public GameObject StageConfetti;
        public GameObject VictoryPodium;
        public GameObject StageLighting;
        public GameObject Spectator;
        public IReadOnlyList<GameObject> Booths;
        public GameObject Table;
        public GameObject ExchangeStation;
        public GameObject VerifyButton;
        public GameObject Presenter;
        public GameObject Environment;
        public NetworkPrefabsList NetworkPrefabs;
    }

    /// <summary>
    /// Ensambla la escena Stage con instancias de los prefabs, según el plano de <see cref="StageLayout"/> (LEVEL.md):
    /// pantalla al fondo, mesa en el centro, cabinas en dos filas a los lados y presentador al pie de la pantalla.
    /// </summary>
    public static class StageSceneComposer
    {
        public const string SCENE_PATH = "Assets/_Project/Scenes/Stage.unity";

        public static void Compose(StageScenePrefabs prefabs, SpawnPointSet spawnPoints)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateLighting();
            var environment = Place(prefabs.Environment, Vector3.zero, 0f);
            var table = Place(prefabs.Table, StageLayout.TablePosition, 0f);
            var exchange = Place(prefabs.ExchangeStation, StageLayout.ExchangePosition, StageLayout.STATIONS_YAW);
            var verifyButton = Place(prefabs.VerifyButton, StageLayout.VerifyButtonPosition, StageLayout.STATIONS_YAW);
            // En el centro, entre la pantalla y las estaciones, mirando a la zona de inicio. A pie de suelo y con la
            // viñeta al lado de la cabeza: ni él ni lo que dice tapan la pregunta.
            Place(prefabs.Presenter, StageLayout.PresenterSpot, StageLayout.PRESENTER_YAW);
            var booths = PlaceBooths(prefabs.Booths);
            WriteSpawnPoints(spawnPoints);
            ConnectMoney(table, exchange, booths);
            ConnectMatch(environment, booths);
            ConnectSelfBet(booths);

            PlaceNetworkManager(prefabs);
            PlaceCameraRig(prefabs.CameraRig);
            PrefabUtility.InstantiatePrefab(prefabs.InteractionHud);
            PrefabUtility.InstantiatePrefab(prefabs.MatchHud);
            PrefabUtility.InstantiatePrefab(prefabs.ResultsScreen);
            PrefabUtility.InstantiatePrefab(prefabs.Menu);
            PrefabUtility.InstantiatePrefab(prefabs.ShopUi);
            PrefabUtility.InstantiatePrefab(prefabs.AvatarCosmetics);
            PrefabUtility.InstantiatePrefab(prefabs.GameFeedback);
            PrefabUtility.InstantiatePrefab(prefabs.StageConfetti);
            PrefabUtility.InstantiatePrefab(prefabs.VictoryPodium);
            var lighting = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.StageLighting);
            var library = FeedbackBuilder.Library();
            ConnectTubes(booths, library);
            AttachStageShow(environment, booths, table, verifyButton, lighting, library);
            PrefabUtility.InstantiatePrefab(prefabs.Spectator);
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(SCENE_PATH, true) };
        }

        private static GameObject Place(GameObject prefab, Vector3 position, float yaw)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            return instance;
        }

        private static GameObject[] PlaceBooths(IReadOnlyList<GameObject> booths)
        {
            var parent = new GameObject("Booths").transform;
            var instances = new GameObject[booths.Count];
            for (var i = 0; i < booths.Count; i++)
            {
                // A los lados, como la cuadrícula de la pantalla; el frente de la cabina (+Z local) mira al centro.
                instances[i] = Place(booths[i], StageLayout.BoothPosition(i), StageLayout.BoothYaw(i));
                instances[i].transform.SetParent(parent, true);
            }
            return instances;
        }

        /// <summary>
        /// Une mesa y bandejas con el módulo Money: guarda sus posiciones (datos de build) y añade los puntos interactivos.
        /// </summary>
        private static void ConnectMoney(GameObject table, GameObject exchange, GameObject[] booths)
        {
            var tableSurface = table.transform.position + Vector3.up * TablePrefabBuilder.SURFACE_HEIGHT;
            MoneyBuilder.AttachTableTarget(table,
                table.transform.Find(TablePrefabBuilder.TOTAL_LABEL).GetComponent<TextMeshPro>());

            var traySurfaces = new Vector3[booths.Length];
            var trayYaw = new float[booths.Length];
            for (var i = 0; i < booths.Length; i++)
            {
                // «Surface» marca el suelo de la bandeja del mostrador: ahí se apila el dinero.
                traySurfaces[i] = booths[i].transform.Find(BoothPrefabBuilder.TRAY_SURFACE).position;
                trayYaw[i] = booths[i].transform.eulerAngles.y;

                var tray = booths[i].transform.Find(BoothPrefabBuilder.TRAY);
                MoneyBuilder.AttachTrayTarget(tray.gameObject, i,
                    tray.Find(BoothPrefabBuilder.TRAY_TOTAL_LABEL).GetComponent<TextMeshPro>());
            }

            MoneyBuilder.WriteLayout(tableSurface, exchange.transform.position, traySurfaces, trayYaw);
        }

        private static void WriteSpawnPoints(SpawnPointSet spawnPoints)
        {
            var positions = new Vector3[StageLayout.SPAWN_COUNT];
            var yaws = new float[StageLayout.SPAWN_COUNT];
            var startX = -(StageLayout.SPAWN_COUNT - 1) * StageLayout.SPAWN_SPACING * 0.5f;
            for (var i = 0; i < StageLayout.SPAWN_COUNT; i++)
            {
                positions[i] = new Vector3(startX + i * StageLayout.SPAWN_SPACING, 0.05f, StageLayout.SPAWN_Z);
                yaws[i] = 0f;
            }

            spawnPoints.EditorSet(positions, yaws);
            EditorUtility.SetDirty(spawnPoints);
        }

        /// <summary>Une cabinas y pantalla con el módulo Match (textos de respuesta y número de pregunta).</summary>
        private static void ConnectMatch(GameObject environment, GameObject[] booths)
        {
            for (var i = 0; i < booths.Length; i++)
            {
                MatchBuilder.AttachBoothLabel(booths[i], i,
                    booths[i].transform.Find(BoothPrefabBuilder.ANSWER_LABEL).GetComponent<TextMeshPro>());
            }

            var env = environment.transform;
            TextMeshPro FindText(string path) => env.Find(path).GetComponent<TextMeshPro>();

            var optionTexts = new TextMeshPro[booths.Length];
            var optionBoxes = new GameObject[booths.Length];
            for (var i = 0; i < booths.Length; i++)
            {
                optionTexts[i] = FindText(StageEnvironmentPrefabBuilder.ScreenOptionText(i));
                optionBoxes[i] = env.Find(StageEnvironmentPrefabBuilder.ScreenOptionBox(i)).gameObject;
            }

            MatchBuilder.AttachQuestionScreen(env.Find(StageEnvironmentPrefabBuilder.SCREEN).gameObject,
                env.Find(StageEnvironmentPrefabBuilder.SCREEN_CONTENT).gameObject,
                FindText(StageEnvironmentPrefabBuilder.SCREEN_MESSAGE),
                FindText(StageEnvironmentPrefabBuilder.SCREEN_HEADER),
                FindText(StageEnvironmentPrefabBuilder.SCREEN_QUESTION),
                optionTexts, optionBoxes,
                FindText(StageEnvironmentPrefabBuilder.SCREEN_STATUS),
                env.Find(StageEnvironmentPrefabBuilder.SCREEN_CHECK).gameObject,
                FindText(StageEnvironmentPrefabBuilder.SCREEN_CHECK_PAYEE),
                FindText(StageEnvironmentPrefabBuilder.SCREEN_CHECK_AMOUNT));
        }

        /// <summary>Une las zonas de apuesta y los nombres de apostados de cada cabina con el módulo SelfBet.</summary>
        private static void ConnectSelfBet(GameObject[] booths)
        {
            var zoneCenters = new Vector3[booths.Length];
            for (var i = 0; i < booths.Length; i++)
            {
                var zone = booths[i].transform.Find(BoothPrefabBuilder.BET_ZONE);
                zoneCenters[i] = zone.position;
                SelfBetBuilder.AttachToBooth(zone.gameObject, booths[i], i,
                    booths[i].transform.Find(BoothPrefabBuilder.BETTORS_LABEL).GetComponent<TextMeshPro>());
            }
            SelfBetBuilder.WriteLayout(zoneCenters);
        }

        /// <summary>Los tubos suenan al succionar: el sonido lo sintetiza el módulo Feedback.</summary>
        private static void ConnectTubes(GameObject[] booths, FeedbackLibrary library)
        {
            foreach (var booth in booths)
            {
                foreach (var tube in booth.GetComponentsInChildren<TubePath>(true))
                {
                    var audio = tube.GetComponentInChildren<AudioSource>(true);
                    audio.clip = library.Suction;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(audio);
                }
            }
        }

        /// <summary>
        /// Plató vivo (módulo Feedback): bombillas de las cabinas y de la marquesina, público, cámaras de estudio y focos.
        /// App reúne las piezas del plató y de las cabinas; Feedback las anima.
        /// </summary>
        private static void AttachStageShow(GameObject environment, GameObject[] booths, GameObject table, GameObject verifyButton,
            GameObject lighting, FeedbackLibrary library)
        {
            var env = environment.transform;
            var bulbs = new List<Renderer>();
            var bulbBooths = new List<int>();
            var bulbOrders = new List<int>();
            for (var booth = 0; booth < booths.Length; booth++)
            {
                // Bombillas del mueble, ordenadas de abajo arriba: la persecución sube por las dos columnas.
                var boothBulbs = booths[booth].transform.Find(BoothPrefabBuilder.BODY).GetComponentsInChildren<Renderer>()
                    .Where(r => r.sharedMaterial != null && r.sharedMaterial.name.Contains("Bulb"))
                    .OrderBy(r => r.transform.TransformPoint(r.localBounds.center).y).ToList();
                for (var i = 0; i < boothBulbs.Count; i++)
                {
                    bulbs.Add(boothBulbs[i]);
                    bulbBooths.Add(booth);
                    bulbOrders.Add(i / 2);
                }
            }

            var marquee = env.Find(StageEnvironmentPrefabBuilder.MARQUEE).GetComponentsInChildren<Renderer>()
                .Where(r => r.sharedMaterial != null && r.sharedMaterial.name.Contains("Lens"))
                .OrderBy(r => r.bounds.center.x).ToList();
            for (var i = 0; i < marquee.Count; i++)
            {
                bulbs.Add(marquee[i]);
                bulbBooths.Add(Trivias.Feedback.Logic.StageShowRules.MARQUEE);
                bulbOrders.Add(i);
            }

            var audience = env.Find("Audience");
            var figures = audience.GetComponentsInChildren<Transform>()
                .Where(t => t.name == StageEnvironmentPrefabBuilder.AUDIENCE_FIGURE).ToArray();
            var audienceCenter = figures.Aggregate(Vector3.zero, (sum, t) => sum + t.position) / Mathf.Max(1, figures.Length);

            var cameras = env.Find("StudioCameras").Cast<Transform>().Where(t => t.name.StartsWith("Camera") && !t.name.EndsWith("Collider"))
                .Append(env.Find("CameraDais/Camera")).ToArray();
            var fixtures = env.Find("CeilingRig").Cast<Transform>().Where(t => t.name.StartsWith("SmallSpot")).ToArray();
            var spots = lighting.GetComponentsInChildren<Light>().Select(l => l.transform).ToArray();

            FeedbackBuilder.AttachStageShow(MatchBuilder.Runtime(), library, bulbs.ToArray(), bulbBooths.ToArray(), bulbOrders.ToArray(),
                figures, audienceCenter, cameras, table.transform, verifyButton.transform, booths.Select(b => b.transform).ToArray(),
                fixtures, spots);
        }

        private static void PlaceNetworkManager(StageScenePrefabs prefabs)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.NetworkManager);
            var networkManager = instance.GetComponent<NetworkManager>();
            networkManager.NetworkConfig.PlayerPrefab = prefabs.Player;

            var lists = networkManager.NetworkConfig.Prefabs.NetworkPrefabsLists;
            if (prefabs.NetworkPrefabs != null && !lists.Contains(prefabs.NetworkPrefabs))
                lists.Add(prefabs.NetworkPrefabs);

            PrefabUtility.RecordPrefabInstancePropertyModifications(networkManager);
            var connection = instance.GetComponent<ConnectionService>();
            NetworkManagerPrefabBuilder.SetSessionPrefabs(connection, prefabs.SessionPrefabs);
        }

        private static void PlaceCameraRig(GameObject cameraRigPrefab)
        {
            var rig = (GameObject)PrefabUtility.InstantiatePrefab(cameraRigPrefab);
            // Vista general del plató hasta que aparece el jugador local.
            var overview = rig.transform.Find("PlayerFollowCamera");
            overview.SetPositionAndRotation(StageLayout.OverviewPosition,
                Quaternion.LookRotation(StageLayout.OverviewTarget - StageLayout.OverviewPosition));
            var mainCamera = rig.transform.Find("MainCamera");
            mainCamera.SetPositionAndRotation(overview.position, overview.rotation);
        }

        /// <summary>Necesario para los botones de interfaz (pantalla de resultados), con el Input System.</summary>
        private static void CreateEventSystem()
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        private static void CreateLighting()
        {
            var sun = new GameObject("Directional Light");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.color = new Color(1f, 0.96f, 0.9f);
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(55f, -30f, 0f);

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.45f, 0.45f, 0.65f);
            RenderSettings.ambientEquatorColor = new Color(0.30f, 0.25f, 0.40f);
            RenderSettings.ambientGroundColor = new Color(0.12f, 0.10f, 0.18f);
            RenderSettings.sun = light;
        }
    }
}
