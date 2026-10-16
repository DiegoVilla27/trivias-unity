using System.Collections;
using NUnit.Framework;
using Trivias.Money.Logic;
using Trivias.Money.Network;
using Trivias.Networking.Components;
using Trivias.Shared.Interaction;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static Trivias.App.Tests.PlayModeHelpers;

namespace Trivias.App.Tests
{
    /// <summary>Hito 3: estación de cambio (agrupar en la mesa y dividir lo que se lleva), conservando el valor.</summary>
    public sealed class ExchangeFlowTests
    {
        private MoneySessionNetwork _money;
        private NetworkObject _player;
        private ulong _client;

        [UnitySetUp]
        public IEnumerator StartHostAndMatch()
        {
            yield return SceneManager.LoadSceneAsync("Stage", LoadSceneMode.Single);
            var connection = Object.FindAnyObjectByType<ConnectionService>();
            Assert.That(connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            Assert.That(connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;

            _money = Object.FindAnyObjectByType<MoneySessionNetwork>();
            _player = NetworkManager.Singleton.LocalClient.PlayerObject;
            _client = _player.OwnerClientId;
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
        public IEnumerator Group_ThenCarryBag_ThenSplitIt()
        {
            var ledger = _money.Ledger;

            MoveNextTo(_player, FindExchangeStation().Position);
            yield return null;
            FindExchangeStation().Interact(new InteractionContext(_client, InteractionAction.Primary));
            yield return null;
            Assert.That(ledger.CountAt(MoneyPlace.Table, 0, Denomination.Bag), Is.EqualTo(1), "E agrupa 10 fajos en una bolsa.");
            Assert.That(ledger.CountAt(MoneyPlace.Table, 0, Denomination.Bundle), Is.EqualTo(10));
            Assert.That(ledger.TotalInPlay(), Is.EqualTo(100_000));

            // En la mesa, R elige el tipo (con las manos vacías) y E coge uno de ese tipo.
            var table = FindTable();
            MoveNextTo(_player, table.Position);
            yield return null;
            table.Interact(new InteractionContext(_client, InteractionAction.Tertiary));
            yield return null;
            Assert.That(table.TryGetPrompt(default, out var prompt), Is.True);
            Assert.That(prompt, Does.Contain("Coger bolsa"));
            table.Interact(new InteractionContext(_client, InteractionAction.Primary));
            yield return null;
            Assert.That(ledger.Items[ledger.FindCarriedBy(_client)].Denomination, Is.EqualTo(Denomination.Bag));

            MoveNextTo(_player, FindExchangeStation().Position);
            yield return null;
            FindExchangeStation().Interact(new InteractionContext(_client, InteractionAction.Primary));
            yield return null;
            Assert.That(ledger.CountCarriedBy(_client), Is.Zero, "La bolsa se divide.");
            Assert.That(ledger.CountAt(MoneyPlace.Table, 0, Denomination.Bundle), Is.EqualTo(20));
            Assert.That(ledger.TotalInPlay(), Is.EqualTo(100_000), "Cambiar nunca crea ni destruye dinero.");
        }
    }
}
