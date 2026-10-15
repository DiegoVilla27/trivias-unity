using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Trivias.HUD.Components;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Match.Network;
using Trivias.Menu.Components;
using Trivias.Networking.Components;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Trivias.App.Tests
{
    /// <summary>Menú (principal → sala → oculto en partida), HUD solo en las preguntas y tiempo para apostar.</summary>
    public sealed class MenuAndTimerTests
    {
        private const BindingFlags PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic;

        private ConnectionService _connection;
        private MenuView _menu;
        private int _originalBettingSeconds = -1;
        private MatchConfig _config;

        [UnitySetUp]
        public IEnumerator LoadStage()
        {
            yield return SceneManager.LoadSceneAsync("Stage", LoadSceneMode.Single);
            yield return null;
            _connection = Object.FindAnyObjectByType<ConnectionService>();
            _menu = Object.FindAnyObjectByType<MenuView>();
        }

        [UnityTearDown]
        public IEnumerator Shutdown()
        {
            if (_config != null && _originalBettingSeconds >= 0)
                typeof(MatchConfig).GetField("_bettingSeconds", PRIVATE).SetValue(_config, _originalBettingSeconds);

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
                Object.Destroy(NetworkManager.Singleton.gameObject);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator Menu_GoesFromMainToLobby_AndHidesDuringTheMatch()
        {
            Assert.That(Panel("_mainPanel").activeSelf, Is.True, "Al arrancar se ve el menú principal.");
            Assert.That(Panel("_lobbyPanel").activeSelf, Is.False);

            Assert.That(_connection.StartHost(), Is.True);
            yield return new WaitForSeconds(0.5f);
            Assert.That(Panel("_mainPanel").activeSelf, Is.False);
            Assert.That(Panel("_lobbyPanel").activeSelf, Is.True, "Con la sala creada se ve la sala de espera.");
            Assert.That(HudPanel().activeSelf, Is.False, "En la sala no se muestra el HUD de la partida.");

            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;
            Assert.That(Panel("_lobbyPanel").activeSelf, Is.False, "Al empezar la partida el menú desaparece.");
            Assert.That(HudPanel().activeSelf, Is.True, "Durante las preguntas se ve el HUD.");
        }

        [UnityTest]
        public IEnumerator BettingTimer_VerifiesAutomaticallyWhenItRunsOut()
        {
            Assert.That(_connection.StartHost(), Is.True);
            yield return new WaitForSeconds(0.5f);

            var match = Object.FindAnyObjectByType<MatchSessionNetwork>();
            _config = (MatchConfig)typeof(MatchSessionNetwork).GetField("_config", PRIVATE).GetValue(match);
            _originalBettingSeconds = _config.BettingSeconds;
            typeof(MatchConfig).GetField("_bettingSeconds", PRIVATE).SetValue(_config, 2);

            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.Betting));
            Assert.That(match.IsBettingTimed, Is.True);
            Assert.That(match.PhaseSecondsLeft, Is.GreaterThan(0f).And.LessThanOrEqualTo(2f));

            yield return new WaitForSeconds(2.5f);
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.Locked), "Al acabarse el tiempo se verifica sola.");
        }

        private GameObject Panel(string field) => (GameObject)typeof(MenuView).GetField(field, PRIVATE).GetValue(_menu);

        private static GameObject HudPanel() =>
            (GameObject)typeof(MatchHudView).GetField("_statusPanel", PRIVATE)
                .GetValue(Object.FindAnyObjectByType<MatchHudView>());
    }
}
