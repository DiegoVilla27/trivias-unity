using System.Collections;
using NUnit.Framework;
using Trivias.Networking.Components;
using Trivias.Player.Components;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Trivias.App.Tests
{
    /// <summary>Prueba de humo de la escena del plató: crear sala y comprobar el jugador local.</summary>
    public sealed class StageSceneSmokeTests
    {
        private const string STAGE_SCENE = "Stage";
        private const float SPAWN_Z = -4.5f;

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

            var followCamera = Object.FindAnyObjectByType<CinemachineCamera>();
            Assert.That(followCamera.Target.TrackingTarget, Is.EqualTo(player.transform.Find("CameraRoot")),
                "La cámara no sigue al jugador local.");
        }
    }
}
