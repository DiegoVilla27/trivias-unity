using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Trivias.Feedback.Components;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Match.Network;
using Trivias.Networking.Components;
using Trivias.Shared.Interaction;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static Trivias.App.Tests.PlayModeHelpers;

namespace Trivias.App.Tests
{
    /// <summary>Empezar en una pregunta concreta para probar (MatchConfig → «Empezar en la pregunta»).</summary>
    public sealed class TestStartQuestionTests
    {
        private const BindingFlags PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo StartField = typeof(MatchConfig).GetField("_testStartQuestion", PRIVATE);

        private ConnectionService _connection;
        private MatchSessionNetwork _match;
        private MatchConfig _config;
        private int _original;
        private NetworkObject _player;

        [UnitySetUp]
        public IEnumerator HostInLobby()
        {
            TemporaryProfileSetup.FreshProfile();
            yield return SceneManager.LoadSceneAsync("Stage", LoadSceneMode.Single);
            _connection = Object.FindAnyObjectByType<ConnectionService>();
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
        public IEnumerator ByDefault_TheMatchStartsAtQuestionOne()
        {
            StartField.SetValue(_config, 1);
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            Assert.That(_match.QuestionNumber, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator StartingAtAMiddleQuestion_ShowsThatQuestion()
        {
            StartField.SetValue(_config, 8);
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            Assert.That(_match.QuestionNumber, Is.EqualTo(8));
            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Betting));
        }

        [UnityTest]
        public IEnumerator StartingAtTheLastQuestion_AndAnsweringRight_WinsWithGoldenRain()
        {
            StartField.SetValue(_config, 15);
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            Assert.That(_match.QuestionNumber, Is.EqualTo(15));
            Assert.That(_match.CurrentQuestion.Difficulty, Is.EqualTo(3), "La última sigue siendo difícil.");

            // Todo el dinero que se pueda llevar, a la cabina correcta.
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
            Assert.That(_match.Result.Victory, Is.True);
            Assert.That(Object.FindAnyObjectByType<StageConfetti>().IsVictoryRaining, Is.True, "Lluvia dorada al ganar.");
        }

        [UnityTest]
        public IEnumerator AnOutOfRangeValue_StopsAtTheLastQuestion()
        {
            StartField.SetValue(_config, 30);
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            Assert.That(_match.QuestionNumber, Is.EqualTo(_config.QuestionsPerMatch));
        }
    }
}
