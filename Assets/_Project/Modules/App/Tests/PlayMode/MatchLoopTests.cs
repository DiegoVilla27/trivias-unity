using System.Collections;
using NUnit.Framework;
using Trivias.Match.Logic;
using Trivias.Match.Network;
using Trivias.Money.Network;
using Trivias.Networking.Components;
using Trivias.Presenter.Data;
using Trivias.Presenter.Network;
using Trivias.SelfBet.Network;
using Trivias.Shared.Interaction;
using Trivias.Shared.Players;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static Trivias.App.Tests.PlayModeHelpers;

namespace Trivias.App.Tests
{
    /// <summary>Hito 4: presentador, resultados y volver a jugar.</summary>
    public sealed class MatchLoopTests
    {
        private ConnectionService _connection;
        private MatchSessionNetwork _match;
        private MoneySessionNetwork _money;
        private SelfBetSessionNetwork _bets;
        private NetworkObject _player;

        [UnitySetUp]
        public IEnumerator StartHostAndMatch()
        {
            yield return SceneManager.LoadSceneAsync("Stage", LoadSceneMode.Single);
            _connection = Object.FindAnyObjectByType<ConnectionService>();
            Assert.That(_connection.StartHost(), Is.True);
            yield return new WaitForSeconds(0.5f);
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;

            _match = Object.FindAnyObjectByType<MatchSessionNetwork>();
            _money = Object.FindAnyObjectByType<MoneySessionNetwork>();
            _bets = Object.FindAnyObjectByType<SelfBetSessionNetwork>();
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

        [UnityTest]
        public IEnumerator Presenter_WelcomesAndAnnouncesCountdown()
        {
            var presenter = FindPresenterRuntime();
            yield return null;
            Assert.That(presenter.CurrentLine, Is.Not.Empty, "El presentador debe dar la bienvenida al empezar.");
            var welcome = presenter.CurrentLine;

            _match.RequestVerify();
            yield return null;
            yield return null;
            Assert.That(presenter.CurrentLine, Is.Not.Empty);
            Assert.That(presenter.CurrentLine, Is.Not.EqualTo(welcome), "Al verificar debe comentar la cuenta atrás.");
        }

        [UnityTest]
        public IEnumerator Defeat_ShowsResults_ThenPlayAgainResetsEverything()
        {
            // Derrota: comodín en una cabina incorrecta (en solitario, sin activos = derrota).
            var correct = _match.Order.BoothOf(_match.CurrentQuestion.CorrectOption);
            var zone = FindBetZone((correct + 1) % 4);
            MoveTo(_player, zone.Position);
            yield return null;
            zone.Interact(new InteractionContext(_player.OwnerClientId, InteractionAction.Primary));
            yield return null;
            yield return null;
            _match.RequestVerify();
            yield return new WaitForSeconds(3.3f + 7.3f);

            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Defeat));
            var result = _match.Result;
            Assert.That(result.Victory, Is.False);
            Assert.That(result.QuestionsPassed, Is.Zero);
            Assert.That(result.TokensEach, Is.Zero);
            Assert.That(result.Eliminated, Does.Contain("Jugador 1"));
            Assert.That(result.DurationSeconds, Is.GreaterThan(9f), "El tiempo incluye cuenta atrás y revelación.");
            Assert.That(ResultsPanel().activeSelf, Is.True, "Debe verse la pantalla de resultados.");

            // Volver a jugar.
            _match.RequestReturnToLobby();
            yield return null;
            yield return null;
            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Lobby));
            Assert.That(ResultsPanel().activeSelf, Is.False);
            Assert.That(_money.Ledger.TotalInPlay(), Is.EqualTo(100_000), "El dinero vuelve al capital inicial.");
            Assert.That(_money.IsLocked, Is.True, "En la sala el dinero está bloqueado.");
            Assert.That(_player.GetComponent<IPlayerAvatar>().IsEliminated, Is.False, "El expulsado vuelve a jugar.");
            Assert.That(_bets.Book.Bettors.Count, Is.Zero);
            Assert.That(_connection.MatchStarted, Is.False, "La sala se reabre.");

            // Y se puede empezar otra partida.
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;
            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Betting));
            Assert.That(_match.QuestionNumber, Is.EqualTo(1));
            Assert.That(_bets.Book.Bettors[0].PersonalUses, Is.EqualTo(6), "Los comodines se reparten de nuevo.");
        }

        private static PresenterRuntime FindPresenterRuntime() =>
            (PresenterRuntime)typeof(PresenterSessionNetwork)
                .GetField("_runtime", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .GetValue(Object.FindAnyObjectByType<PresenterSessionNetwork>());

        private static GameObject ResultsPanel()
        {
            var view = Object.FindAnyObjectByType<Trivias.Results.Components.ResultsView>(FindObjectsInactive.Include);
            return view.transform.Find("Panel").gameObject;
        }
    }
}
