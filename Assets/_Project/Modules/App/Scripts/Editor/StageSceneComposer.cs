using System.Collections.Generic;
using TMPro;
using Trivias.Booth.Editor;
using Trivias.Match.Editor;
using Trivias.Money.Editor;
using Trivias.Networking.Components;
using Trivias.Networking.Editor;
using Trivias.SelfBet.Editor;
using Trivias.Shared.Editor;
using Trivias.Shared.Spawning;
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
        public IReadOnlyList<GameObject> Booths;
        public GameObject Table;
        public GameObject ExchangeStation;
        public GameObject VerifyButton;
        public GameObject Presenter;
        public GameObject Environment;
        public NetworkPrefabsList NetworkPrefabs;
    }

    /// <summary>
    /// Ensambla la escena Stage con instancias de los prefabs. Distribución según LEVEL.md:
    /// mesa en el centro, cabinas en semicírculo delante, presentador y pantalla detrás.
    /// </summary>
    public static class StageSceneComposer
    {
        public const string SCENE_PATH = "Assets/_Project/Scenes/Stage.unity";

        private const float BOOTH_RADIUS = 8.5f;
        private static readonly float[] BoothAngles = { -51f, -17f, 17f, 51f };

        private const int SPAWN_COUNT = 6;
        private const float SPAWN_SPACING = 1.5f;
        private const float SPAWN_Z = -4.5f;

        public static void Compose(StageScenePrefabs prefabs, SpawnPointSet spawnPoints)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateLighting();
            var environment = Place(prefabs.Environment, Vector3.zero, 0f);
            var table = Place(prefabs.Table, Vector3.zero, 0f);
            var exchange = Place(prefabs.ExchangeStation, new Vector3(-3.4f, 0f, -0.5f), 180f);
            Place(prefabs.VerifyButton, new Vector3(3.4f, 0f, -0.5f), 180f);
            // A un lado de la pantalla, mirando al centro: ni él ni su viñeta tapan la pregunta.
            Place(prefabs.Presenter, new Vector3(12.5f, 0f, 12f), 225f);
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
                var angle = BoothAngles[i];
                var radians = angle * Mathf.Deg2Rad;
                var position = new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians)) * BOOTH_RADIUS;
                // El frente de la cabina (+Z local) mira al centro del plató.
                instances[i] = Place(booths[i], position, angle + 180f);
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
                var surface = booths[i].transform.Find(BoothPrefabBuilder.TRAY_SURFACE);
                traySurfaces[i] = surface.position + Vector3.up * (surface.lossyScale.y * 0.5f);
                trayYaw[i] = booths[i].transform.eulerAngles.y;

                var tray = booths[i].transform.Find(BoothPrefabBuilder.TRAY);
                MoneyBuilder.AttachTrayTarget(tray.gameObject, i,
                    tray.Find(BoothPrefabBuilder.TRAY_TOTAL_LABEL).GetComponent<TextMeshPro>());
            }

            MoneyBuilder.WriteLayout(tableSurface, exchange.transform.position, traySurfaces, trayYaw);
        }

        private static void WriteSpawnPoints(SpawnPointSet spawnPoints)
        {
            var positions = new Vector3[SPAWN_COUNT];
            var yaws = new float[SPAWN_COUNT];
            var startX = -(SPAWN_COUNT - 1) * SPAWN_SPACING * 0.5f;
            for (var i = 0; i < SPAWN_COUNT; i++)
            {
                positions[i] = new Vector3(startX + i * SPAWN_SPACING, 0.05f, SPAWN_Z);
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
                FindText(StageEnvironmentPrefabBuilder.SCREEN_STATUS));
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
            overview.SetPositionAndRotation(new Vector3(0f, 9f, -16f),
                Quaternion.LookRotation(new Vector3(0f, 4f, 8f) - new Vector3(0f, 9f, -16f)));
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
