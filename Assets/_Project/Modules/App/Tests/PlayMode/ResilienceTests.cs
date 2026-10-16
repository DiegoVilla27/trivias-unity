using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Trivias.Match.Logic;
using Trivias.Match.Network;
using Trivias.Money.Network;
using Trivias.Networking.Components;
using Trivias.Presenter.Data;
using Trivias.Presenter.Logic;
using Trivias.Presenter.Network;
using Trivias.SelfBet.Data;
using Trivias.SelfBet.Network;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Trivias.App.Tests
{
    /// <summary>La partida aguanta a la gente real: jugadores que se van, salas que se cierran y se vuelven a crear.</summary>
    public sealed class ResilienceTests
    {
        private const BindingFlags PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic;
        private const ulong GHOST_A = 98;
        private const ulong GHOST_B = 99;

        private ConnectionService _connection;
        private MatchSessionNetwork _match;
        private SelfBetSessionNetwork _bets;

        [UnitySetUp]
        public IEnumerator HostAndMatch()
        {
            TemporaryProfileSetup.FreshProfile();
            yield return SceneManager.LoadSceneAsync("Stage", LoadSceneMode.Single);
            _connection = Object.FindAnyObjectByType<ConnectionService>();
            Assert.That(_connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;
            _match = Object.FindAnyObjectByType<MatchSessionNetwork>();
            _bets = Object.FindAnyObjectByType<SelfBetSessionNetwork>();
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
        public IEnumerator ClosingAndReopeningTheRoom_StartsACleanMatch()
        {
            _connection.Leave();
            yield return new WaitForSeconds(0.5f);
            Assert.That(NetworkManager.Singleton.IsListening, Is.False);

            Assert.That(_connection.StartHost(), Is.True, "Se debe poder crear otra sala tras cerrar la anterior.");
            yield return PlayModeHelpers.UntilHostReady();
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;

            var match = Object.FindAnyObjectByType<MatchSessionNetwork>();
            var money = Object.FindAnyObjectByType<MoneySessionNetwork>();
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.Betting));
            Assert.That(match.QuestionNumber, Is.EqualTo(1));
            Assert.That(money.Ledger.TotalInPlay(), Is.EqualTo(100_000));
            Assert.That(money.IsLocked, Is.False);
        }

        [UnityTest]
        public IEnumerator PlayerLeavingMidMatch_StopsCounting_AndThePresenterNotices()
        {
            // Simula un segundo jugador que entra en el reparto y luego se desconecta.
            Authority().StartMatch(new[] { NetworkManager.ServerClientId, GHOST_B });
            yield return null;
            var activeBefore = _bets.ActivePlayers;

            Leave(GHOST_B);
            yield return null;
            yield return null;

            Assert.That(_bets.ActivePlayers, Is.EqualTo(activeBefore - 1));
            Assert.That(_bets.Book.Bettors[_bets.Book.IndexOf(GHOST_B)].HasLeft, Is.True);
            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Betting), "Con el host activo, la partida sigue.");
            Assert.That(PresenterSaid(PresenterEvent.PlayerLeft), Is.True, "El presentador comenta que alguien se fue.");
        }

        [UnityTest]
        public IEnumerator LastActivePlayersLeaving_EndsInDefeatAtOnce()
        {
            Authority().StartMatch(new[] { GHOST_A, GHOST_B }); // El host no participa en este reparto simulado.
            yield return null;

            Leave(GHOST_A);
            yield return null;
            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Betting));
            Leave(GHOST_B);
            yield return null;
            yield return null;

            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Defeat), "Sin activos, derrota inmediata (sin esperar al reloj).");
            Assert.That(_match.Result.Survivors, Is.Empty);
            Assert.That(_match.Result.Eliminated, Is.Empty, "Quien se fue no figura como expulsado.");
        }

        private ISelfBetAuthority Authority() => _bets;

        private void Leave(ulong client) =>
            typeof(MatchSessionNetwork).GetMethod("OnClientLeft", PRIVATE).Invoke(_match, new object[] { client });

        private static bool PresenterSaid(PresenterEvent presenterEvent)
        {
            var presenter = Object.FindAnyObjectByType<PresenterSessionNetwork>();
            var runtime = (PresenterRuntime)typeof(PresenterSessionNetwork).GetField("_runtime", PRIVATE).GetValue(presenter);
            var catalog = (PresenterCatalog)typeof(PresenterSessionNetwork).GetField("_catalog", PRIVATE).GetValue(presenter);
            for (var i = 0; i < catalog.Count; i++)
            {
                if (catalog[i].Event == presenterEvent && runtime.CurrentLine == catalog[i].Text)
                    return true;
            }
            return false;
        }
    }
}
