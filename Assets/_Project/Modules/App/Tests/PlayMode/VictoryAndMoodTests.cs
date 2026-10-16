using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using Trivias.Feedback.Components;
using Trivias.Feedback.Logic;
using Trivias.Match.Components;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Match.Network;
using Trivias.Networking.Components;
using Trivias.Results.Components;
using Trivias.Shared.Interaction;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static Trivias.App.Tests.PlayModeHelpers;

namespace Trivias.App.Tests
{
    /// <summary>Victoria vistosa (cheque, podio, resultados después de celebrar) y ambiente de luz por momento.</summary>
    public sealed class VictoryAndMoodTests
    {
        private const BindingFlags PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo StartField = typeof(MatchConfig).GetField("_testStartQuestion", PRIVATE);

        private ConnectionService _connection;
        private MatchSessionNetwork _match;
        private MatchConfig _config;
        private int _original;
        private NetworkObject _player;
        private StageLighting _lighting;

        [UnitySetUp]
        public IEnumerator HostInLobby()
        {
            TemporaryProfileSetup.FreshProfile();
            yield return SceneManager.LoadSceneAsync("Stage", LoadSceneMode.Single);
            _connection = Object.FindAnyObjectByType<ConnectionService>();
            _lighting = Object.FindAnyObjectByType<StageLighting>();
            Assert.That(_connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            _player = NetworkManager.Singleton.LocalClient.PlayerObject;
            _match = Object.FindAnyObjectByType<MatchSessionNetwork>();
            _config = (MatchConfig)typeof(MatchSessionNetwork).GetField("_config", PRIVATE).GetValue(_match);
            _original = (int)StartField.GetValue(_config);
        }

        [UnityTearDown]
        public IEnumerator Shutdown()
        {
            StartField.SetValue(_config, _original);
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
                Object.Destroy(NetworkManager.Singleton.gameObject);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator Winning_ShowsTheCheck_ThePodium_AndThenTheResults()
        {
            yield return WinFromTheLastQuestion();

            var screen = Object.FindAnyObjectByType<QuestionScreenView>();
            Assert.That(screen.IsShowingCheck, Is.True, "Cheque gigante en la pantalla.");
            var payee = (TextMeshPro)typeof(QuestionScreenView).GetField("_checkPayee", PRIVATE).GetValue(screen);
            var amount = (TextMeshPro)typeof(QuestionScreenView).GetField("_checkAmount", PRIVATE).GetValue(screen);
            Assert.That(payee.text, Does.Contain("Jugador"), "A nombre de los supervivientes.");
            Assert.That(amount.text, Is.EqualTo(Trivias.Money.Logic.MoneyFormat.Amount(_match.Result.FinalMoney)));

            var podium = Object.FindAnyObjectByType<VictoryPodium>();
            Assert.That(podium.IsShown, Is.True, "El podio sube al ganar.");
            var names = (TextMeshPro[])typeof(VictoryPodium).GetField("_plaqueNames", PRIVATE).GetValue(podium);
            var plaques = (GameObject[])typeof(VictoryPodium).GetField("_plaques", PRIVATE).GetValue(podium);
            Assert.That(plaques[0].activeSelf, Is.True);
            Assert.That(names[0].text, Does.Contain("Jugador"));
            Assert.That(plaques[1].activeSelf, Is.False, "Una placa por concursante.");

            var results = Object.FindAnyObjectByType<ResultsView>();
            Assert.That(results.IsShown, Is.False, "Primero se celebra; los resultados esperan.");
            yield return new WaitForSeconds(5.3f);
            Assert.That(results.IsShown, Is.True, "Después aparecen los resultados.");
            Assert.That(podium.IsRising, Is.False, "El podio ya terminó de subir.");
            Assert.That(_lighting.Mood, Is.EqualTo(StageMood.Victory));

            _match.RequestReturnToLobby();
            yield return null;
            yield return null;
            Assert.That(podium.IsShown, Is.False, "En la sala de espera el podio desaparece.");
            Assert.That(screen.IsShowingCheck, Is.False);
            Assert.That(_lighting.Mood, Is.EqualTo(StageMood.Lobby));
        }

        [UnityTest]
        public IEnumerator Defeat_ShowsResultsAtOnce_WithoutCheckOrPodium()
        {
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            _match.RequestVerify();
            yield return PlayModeHelpers.AfterRound();
            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Defeat));
            Assert.That(Object.FindAnyObjectByType<ResultsView>().IsShown, Is.True);
            Assert.That(Object.FindAnyObjectByType<QuestionScreenView>().IsShowingCheck, Is.False);
            Assert.That(Object.FindAnyObjectByType<VictoryPodium>().IsShown, Is.False);
            Assert.That(_lighting.Mood, Is.EqualTo(StageMood.Defeat));
        }

        [UnityTest]
        public IEnumerator Lighting_FollowsTheMatch()
        {
            yield return null;
            Assert.That(_lighting.Mood, Is.EqualTo(StageMood.Lobby));

            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;
            Assert.That(_lighting.Mood, Is.EqualTo(StageMood.Easy), "Preguntas fáciles: luz cálida.");
            var before = _lighting.SpotColor;
            yield return new WaitForSeconds(1.5f);
            Assert.That(_lighting.SpotColor, Is.Not.EqualTo(before), "El color cambia con un fundido, no de golpe.");

            _match.RequestVerify();
            yield return null;
            yield return null;
            Assert.That(_lighting.Mood, Is.EqualTo(StageMood.Countdown));
            yield return PlayModeHelpers.AfterLock();
            Assert.That(_lighting.Mood, Is.EqualTo(StageMood.Wrong), "Todo el dinero estaba fuera: fallo en rojo.");
        }

        [UnityTest]
        public IEnumerator HardQuestions_DarkenTheStage()
        {
            StartField.SetValue(_config, 11);
            var keyLight = RenderSettings.sun;
            var lobbyIntensity = keyLight.intensity;
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;
            Assert.That(_lighting.Mood, Is.EqualTo(StageMood.Hard));
            yield return new WaitForSeconds(2f);
            Assert.That(keyLight.intensity, Is.LessThan(lobbyIntensity * 0.8f), "En las difíciles el plató se oscurece.");
        }

        private IEnumerator WinFromTheLastQuestion()
        {
            StartField.SetValue(_config, 15);
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            var correct = _match.Order.BoothOf(_match.CurrentQuestion.CorrectOption);
            var table = FindTable();
            MoveNextTo(_player, table.Position);
            yield return null;
            table.Interact(new InteractionContext(_player.OwnerClientId, InteractionAction.Secondary));
            yield return null;
            var tray = FindTray(correct);
            MoveNextTo(_player, tray.Position);
            yield return null;
            tray.Interact(new InteractionContext(_player.OwnerClientId, InteractionAction.Secondary));
            yield return null;
            _match.RequestVerify();
            yield return PlayModeHelpers.AfterRound();
            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Victory));
            yield return null;
        }
    }
}
