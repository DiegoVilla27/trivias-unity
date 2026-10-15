using System.Collections;
using NUnit.Framework;
using Trivias.Money.Components;
using Trivias.Money.Logic;
using Trivias.Money.Network;
using Trivias.Networking.Components;
using Trivias.Player.Components;
using Trivias.Shared.Interaction;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Trivias.App.Tests
{
    /// <summary>Flujo del hito 1: coger un fajo de la mesa y dejarlo en una bandeja, con validación del host.</summary>
    public sealed class MoneyFlowTests
    {
        private const string STAGE_SCENE = "Stage";

        private MoneySessionNetwork _session;
        private NetworkObject _player;

        [UnitySetUp]
        public IEnumerator StartHost()
        {
            yield return SceneManager.LoadSceneAsync(STAGE_SCENE, LoadSceneMode.Single);
            var connection = Object.FindAnyObjectByType<ConnectionService>();
            Assert.That(connection.StartHost(), Is.True);
            yield return new WaitForSeconds(0.5f);
            // El dinero solo se puede tocar con la partida empezada.
            Assert.That(connection.TryStartMatch(), Is.True);
            yield return null;

            _session = Object.FindAnyObjectByType<MoneySessionNetwork>();
            _player = NetworkManager.Singleton.LocalClient.PlayerObject;
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
        public void Session_StartsWithCapitalOnTable()
        {
            Assert.That(_session, Is.Not.Null, "El host no creó la sesión de dinero.");
            Assert.That(_session.Ledger.TotalAt(MoneyPlace.Table, 0), Is.EqualTo(100_000));
            Assert.That(_session.Ledger.Count, Is.EqualTo(20), "El capital inicial debe ser 20 fajos.");
        }

        [UnityTest]
        public IEnumerator PickUpAtTable_ThenDepositInTrayA()
        {
            var table = Object.FindAnyObjectByType<MoneyTableTarget>();
            MoveNextTo(table.Position);
            yield return null;

            table.Interact(default);
            yield return null;
            var carried = _session.Ledger.FindCarriedBy(_player.OwnerClientId);
            Assert.That(carried, Is.GreaterThanOrEqualTo(0), "El host no entregó el fajo.");
            Assert.That(_session.Ledger.TotalAt(MoneyPlace.Table, 0), Is.EqualTo(95_000));

            var carryPoint = _player.GetComponent<PlayerCarrySocket>().CarryPoint;
            Assert.That(carryPoint.childCount, Is.EqualTo(1), "El fajo no se ve en las manos del concursante.");

            var trayA = FindTray('A');
            MoveNextTo(trayA.Position);
            yield return null;

            trayA.Interact(default);
            yield return null;
            Assert.That(_session.Ledger.TotalAt(MoneyPlace.Tray, 0), Is.EqualTo(5_000));
            Assert.That(_session.Ledger.FindCarriedBy(_player.OwnerClientId), Is.EqualTo(-1));
            Assert.That(carryPoint.childCount, Is.Zero, "El fajo sigue en las manos tras depositarlo.");
            Assert.That(trayA.GetComponentInChildren<TMPro.TextMeshPro>().text, Is.EqualTo("5.000"));
        }

        [UnityTest]
        public IEnumerator Host_RejectsInteractionFromTooFar()
        {
            var table = Object.FindAnyObjectByType<MoneyTableTarget>();
            MoveNextTo(table.Position + new Vector3(0f, 0f, -9f));
            yield return null;

            _session.RequestTableInteraction(InteractionAction.Primary);
            yield return null;
            Assert.That(_session.Ledger.FindCarriedBy(_player.OwnerClientId), Is.EqualTo(-1),
                "El host aceptó coger dinero desde demasiado lejos.");
        }

        [UnityTest]
        public IEnumerator BatchPickUp_ThenDepositAll_ThenReturn()
        {
            var client = _player.OwnerClientId;
            var table = Object.FindAnyObjectByType<MoneyTableTarget>();
            MoveNextTo(table.Position);
            yield return null;

            table.Interact(new InteractionContext(client, InteractionAction.Secondary));
            yield return null;
            var capacity = _session.Ledger.CarryLimit(Denomination.Bundle);
            Assert.That(_session.Ledger.CountCarriedBy(client), Is.EqualTo(capacity), "Q debe llenar las manos.");
            Assert.That(_player.GetComponent<PlayerCarrySocket>().CarryPoint.childCount, Is.EqualTo(capacity));

            var trayB = FindTray('B');
            MoveNextTo(trayB.Position);
            yield return null;
            trayB.Interact(new InteractionContext(client, InteractionAction.Secondary));
            yield return null;
            Assert.That(_session.Ledger.CountCarriedBy(client), Is.Zero, "Q en la bandeja debe dejarlo todo.");
            Assert.That(_session.Ledger.TotalAt(MoneyPlace.Tray, 1), Is.EqualTo(capacity * 5_000L));

            trayB.Interact(new InteractionContext(client, InteractionAction.Primary));
            yield return null;
            Assert.That(_session.Ledger.CountCarriedBy(client), Is.EqualTo(1), "E con las manos vacías recupera uno.");

            MoveNextTo(table.Position);
            yield return null;
            table.Interact(new InteractionContext(client, InteractionAction.Tertiary));
            yield return null;
            Assert.That(_session.Ledger.CountCarriedBy(client), Is.Zero, "R debe devolver todo a la mesa.");
            Assert.That(_session.Ledger.TotalInPlay(), Is.EqualTo(100_000));
        }

        private void MoveNextTo(Vector3 target)
        {
            var toCenter = (Vector3.zero - target);
            toCenter.y = 0f;
            var position = target + (toCenter.sqrMagnitude > 0.01f ? toCenter.normalized : Vector3.back) * 1.0f;
            position.y = 0.05f;
            _player.GetComponent<PlayerMotor>().Teleport(position, Quaternion.identity);
        }

        private static MoneyTrayTarget FindTray(char letter)
        {
            // Las bandejas cuelgan de su cabina (Booth_A, Booth_B...).
            foreach (var tray in Object.FindObjectsByType<MoneyTrayTarget>(FindObjectsInactive.Exclude))
            {
                if (tray.transform.parent.name.EndsWith(letter.ToString()))
                    return tray;
            }
            Assert.Fail($"No se encontró la bandeja {letter}.");
            return null;
        }
    }
}
