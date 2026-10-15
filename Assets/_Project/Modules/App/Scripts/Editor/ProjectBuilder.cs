using System;
using Trivias.Booth.Editor;
using Trivias.Cameras.Editor;
using Trivias.Exchange.Editor;
using Trivias.Networking.Editor;
using Trivias.Player.Editor;
using Trivias.Presenter.Editor;
using Trivias.Shared.Editor;
using Trivias.Stage.Editor;
using Trivias.Table.Editor;
using Trivias.VerifyButton.Editor;
using UnityEditor;
using UnityEngine;

namespace Trivias.App.Editor
{
    /// <summary>
    /// Raíz de composición en tiempo de edición: construye los prefabs de cada módulo y ensambla la escena del plató.
    /// Se puede ejecutar desde el menú o en modo consola (-executeMethod Trivias.App.Editor.ProjectBuilder.BuildAllBatch).
    /// </summary>
    public static class ProjectBuilder
    {
        [MenuItem("Tools/Trivias/Build All", priority = 0)]
        public static void BuildAll()
        {
            var cameraTarget = SharedAssets.LocalPlayerCameraTarget();
            var spawnPoints = SharedAssets.StageSpawnPoints();

            var networkManager = NetworkManagerPrefabBuilder.Build();
            var player = PlayerPrefabBuilder.Build(spawnPoints, cameraTarget);
            NetworkManagerPrefabBuilder.EnsureGlobalObjectIdHash(player);

            var prefabs = new StageScenePrefabs
            {
                NetworkManager = networkManager,
                Player = player,
                CameraRig = CameraRigPrefabBuilder.Build(cameraTarget),
                Booths = BoothPrefabBuilder.Build(),
                Table = TablePrefabBuilder.Build(),
                ExchangeStation = ExchangeStationPrefabBuilder.Build(),
                VerifyButton = VerifyButtonPrefabBuilder.Build(),
                Presenter = PresenterPrefabBuilder.Build(),
                Environment = StageEnvironmentPrefabBuilder.Build(),
                NetworkPrefabs = NetworkManagerPrefabBuilder.GetOrCreateNetworkPrefabsList(player),
            };

            AssetDatabase.SaveAssets();
            StageSceneComposer.Compose(prefabs, spawnPoints);
            AssetDatabase.SaveAssets();
            Debug.Log("[Trivias] Build All completado.");
        }

        public static void BuildAllBatch()
        {
            try
            {
                BuildAll();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }
    }
}
