using System.Collections;
using NUnit.Framework;
using Trivias.Interaction.Network;
using Trivias.Networking.Components;
using Trivias.Money.Network;
using Trivias.Player.Components;
using Trivias.Shared.Gameplay;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Trivias.App.Tests
{
    /// <summary>Prueba de humo de la escena del plató: crear sala y comprobar el jugador local.</summary>
    public sealed class StageSceneSmokeTests
    {
        private const string STAGE_SCENE = "Stage";
        private const float SPAWN_Z = -2.5f; // StageLayout.SPAWN_Z

        [UnitySetUp]
        public IEnumerator LoadStage()
        {
            yield return SceneManager.LoadSceneAsync(STAGE_SCENE, LoadSceneMode.Single);
        }

        [UnityTearDown]
        public IEnumerator Shutdown()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
                Object.Destroy(NetworkManager.Singleton.gameObject);
            }
            yield return null;
        }

        [Test]
        public void Tests_PlayTheMatchAtAFastPace()
        {
            var configs = Resources.FindObjectsOfTypeAll<Trivias.Match.Data.MatchConfig>();
            Assert.That(configs, Has.Length.EqualTo(1), "Una sola configuración de partida.");
            var config = configs[0];
            Assert.That(config.LockCountdownSeconds, Is.EqualTo(PlayModeHelpers.LOCK_SECONDS), "Las pruebas no esperan los 3 s reales.");
            Assert.That(config.RevealSeconds, Is.EqualTo(PlayModeHelpers.REVEAL_SECONDS), "Ni los 7 s de la revelación.");
        }

        [UnityTest]
        public IEnumerator Host_SpawnsLocalPlayerWithControlAndCamera()
        {
            var connection = Object.FindAnyObjectByType<ConnectionService>();
            Assert.That(connection, Is.Not.Null, "La escena no tiene ConnectionService.");
            Assert.That(connection.StartHost(), Is.True, "No se pudo crear la sala.");

            yield return new WaitForSeconds(1.5f);

            var player = NetworkManager.Singleton.LocalClient.PlayerObject;
            Assert.That(player, Is.Not.Null, "No apareció el jugador local.");
            Assert.That(player.transform.position.z, Is.EqualTo(SPAWN_Z).Within(0.5f), "El jugador no está en su punto de aparición.");
            Assert.That(player.transform.position.y, Is.GreaterThan(-0.2f), "El jugador atravesó el suelo.");
            Assert.That(player.transform.position.y, Is.LessThan(0.3f), "El jugador no está apoyado en el suelo.");

            Assert.That(player.GetComponent<PlayerMotor>().enabled, Is.True, "El movimiento no está activo para el dueño.");
            Assert.That(player.GetComponent<PlayerInputReader>().enabled, Is.True, "La entrada no está activa para el dueño.");
            Assert.That(player.GetComponent<PlayerInteractor>().enabled, Is.True, "La interacción no está activa para el dueño.");

            var followCamera = Object.FindAnyObjectByType<CinemachineCamera>();
            Assert.That(followCamera.Target.TrackingTarget, Is.EqualTo(player.transform.Find("CameraRoot")),
                "La cámara no sigue al jugador local.");
        }

        [UnityTest]
        public IEnumerator Lobby_BlocksMovementAndMoney_UntilMatchStarts()
        {
            var connection = Object.FindAnyObjectByType<ConnectionService>();
            Assert.That(connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();

            var player = NetworkManager.Singleton.LocalClient.PlayerObject;
            var gate = (PlayerControlGate)typeof(PlayerMotor)
                .GetField("_controlGate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .GetValue(player.GetComponent<PlayerMotor>());
            var money = Object.FindAnyObjectByType<MoneySessionNetwork>();

            Assert.That(gate.CanMove, Is.False, "En la sala nadie debe poder moverse.");
            Assert.That(money.IsLocked, Is.True, "En la sala el dinero debe estar bloqueado.");

            Assert.That(connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;
            Assert.That(gate.CanMove, Is.True, "Al empezar la partida se puede mover.");
            Assert.That(money.IsLocked, Is.False, "Al empezar la partida se puede apostar.");
        }

        [UnityTest]
        public IEnumerator Host_CanStartSoloInTestModeAndClosesRoom()
        {
            var connection = Object.FindAnyObjectByType<ConnectionService>();
            Assert.That(connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            Assert.That(transport.DisconnectTimeoutMS, Is.EqualTo(connection.Config.DisconnectTimeoutSeconds * 1000),
                "El tiempo de desconexión no se aplicó al transporte.");

            Assert.That(connection.Config.MinPlayersToStart, Is.EqualTo(1), "En editor con modo prueba el mínimo debe ser 1.");
            Assert.That(connection.CanStartMatch, Is.True, "El host solo debería poder empezar en modo prueba.");
            Assert.That(connection.TryStartMatch(), Is.True);
            Assert.That(connection.MatchStarted, Is.True);
            Assert.That(connection.CanStartMatch, Is.False, "No se debe poder empezar dos veces.");
        }
    }
}
