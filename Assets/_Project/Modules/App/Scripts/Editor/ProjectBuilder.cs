using System;
using Trivias.Booth.Editor;
using Trivias.Cameras.Editor;
using Trivias.Exchange.Editor;
using Trivias.HUD.Editor;
using Trivias.Interaction.Editor;
using Trivias.Match.Editor;
using Trivias.Money.Editor;
using Trivias.Networking.Editor;
using Trivias.Player.Editor;
using Trivias.Presenter.Editor;
using Trivias.Questions.Editor;
using Trivias.SelfBet.Editor;
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
            var interactables = SharedAssets.InteractableRegistry();

            var networkManager = NetworkManagerPrefabBuilder.Build();

            // Player no conoce Interaction: la composición le añade el interactor como extensión.
            var player = PlayerPrefabBuilder.Build(spawnPoints, cameraTarget,
                root => InteractionBuilder.AttachInteractor(root, interactables,
                    PlayerPrefabBuilder.GetActionReference("Interact"),
                    PlayerPrefabBuilder.GetActionReference("InteractSecondary"),
                    PlayerPrefabBuilder.GetActionReference("InteractTertiary")));
            NetworkManagerPrefabBuilder.EnsureGlobalObjectIdHash(player);

            var moneySession = MoneyBuilder.Build();
            NetworkManagerPrefabBuilder.EnsureGlobalObjectIdHash(moneySession);

            // El contenido se importa y valida aquí: si una pregunta está mal, el build se detiene.
            var questionBank = QuestionBankBuilder.Build();
            var matchRuntime = MatchBuilder.Runtime();
            var selfBetSession = SelfBetBuilder.Build();
            NetworkManagerPrefabBuilder.EnsureGlobalObjectIdHash(selfBetSession);

            var matchSession = MatchBuilder.Build(questionBank, MoneyBuilder.Runtime(), SelfBetBuilder.Runtime(),
                SharedAssets.MatchStartRequested());
            NetworkManagerPrefabBuilder.EnsureGlobalObjectIdHash(matchSession);

            var prefabs = new StageScenePrefabs
            {
                NetworkManager = networkManager,
                Player = player,
                SessionPrefabs = new[] { moneySession, selfBetSession, matchSession },
                CameraRig = CameraRigPrefabBuilder.Build(cameraTarget),
                InteractionHud = InteractionBuilder.BuildHud(),
                MatchHud = HudPrefabBuilder.Build(matchRuntime, SelfBetBuilder.Runtime()),
                Booths = BoothPrefabBuilder.Build(),
                Table = TablePrefabBuilder.Build(),
                ExchangeStation = ExchangeStationPrefabBuilder.Build(MoneyBuilder.Config(), MoneyBuilder.Runtime(), interactables),
                VerifyButton = VerifyButtonPrefabBuilder.Build(matchRuntime, interactables),
                Presenter = PresenterPrefabBuilder.Build(),
                Environment = StageEnvironmentPrefabBuilder.Build(),
                NetworkPrefabs = NetworkManagerPrefabBuilder.GetOrCreateNetworkPrefabsList(
                    player, moneySession, selfBetSession, matchSession),
            };

            AssetDatabase.SaveAssets();
            StageSceneComposer.Compose(prefabs, spawnPoints);
            AssetDatabase.SaveAssets();
            AutoBuild.WriteStamp();
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
