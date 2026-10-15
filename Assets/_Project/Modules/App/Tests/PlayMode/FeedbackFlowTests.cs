using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Trivias.Feedback.Components;
using Trivias.Feedback.Data;
using Trivias.Feedback.Logic;
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
    /// <summary>Sonido y celebraciones en juego: cada momento suena, la música sigue la dificultad y hay confeti al ganar.</summary>
    public sealed class FeedbackFlowTests
    {
        private const BindingFlags PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic;

        private ConnectionService _connection;
        private GameFeedbackPlayer _feedback;
        private StageConfetti _confetti;
        private NetworkObject _player;
        private readonly List<FeedbackCue> _played = new();

        [UnitySetUp]
        public IEnumerator HostInLobby()
        {
            TemporaryProfileSetup.FreshProfile();
            yield return SceneManager.LoadSceneAsync("Stage", LoadSceneMode.Single);
            _connection = Object.FindAnyObjectByType<ConnectionService>();
            _feedback = Object.FindAnyObjectByType<GameFeedbackPlayer>();
            _confetti = Object.FindAnyObjectByType<StageConfetti>();
            Assert.That(_connection.StartHost(), Is.True);
            yield return new WaitForSeconds(0.5f);
            _player = NetworkManager.Singleton.LocalClient.PlayerObject;
            _played.Clear();
            _feedback.CuePlayed += _played.Add;
        }

        [UnityTearDown]
        public IEnumerator Shutdown()
        {
            _feedback.CuePlayed -= _played.Add;
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
                Object.Destroy(NetworkManager.Singleton.gameObject);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator Lobby_IsQuiet()
        {
            yield return null;
            Assert.That(_feedback.CurrentMusicBlock, Is.EqualTo(FeedbackRules.NO_MUSIC));
            Assert.That(_confetti.IsVictoryRaining, Is.False);
            Assert.That(_confetti.CorrectCelebrations, Is.Zero);
            Assert.That(_played, Is.Empty);
        }

        [UnityTest]
        public IEnumerator StartingTheMatch_ChimesAndPlaysEasyMusic()
        {
            yield return StartMatch();
            Assert.That(_played, Does.Contain(FeedbackCue.QuestionShown));
            Assert.That(_feedback.CurrentMusicBlock, Is.EqualTo(0));
            Assert.That(ActiveMusicClip(), Is.SameAs(Library().Music(0)), "Suena la pista del bloque fácil.");
        }

        [UnityTest]
        public IEnumerator PickingAndDroppingMoney_Sounds()
        {
            yield return StartMatch();
            var table = FindTable();
            MoveNextTo(_player, table.Position);
            yield return null;

            table.Interact(new InteractionContext(_player.OwnerClientId, InteractionAction.Primary));
            yield return null;
            yield return null;
            Assert.That(_played, Does.Contain(FeedbackCue.MoneyPick));

            table.Interact(new InteractionContext(_player.OwnerClientId, InteractionAction.Tertiary));
            yield return null;
            yield return null;
            Assert.That(_played, Does.Contain(FeedbackCue.MoneyDrop));
        }

        [UnityTest]
        public IEnumerator BettingOnABooth_Dings()
        {
            yield return StartMatch();
            var zone = FindBetZone(0);
            MoveTo(_player, zone.Position);
            yield return null;
            zone.Interact(new InteractionContext(_player.OwnerClientId, InteractionAction.Primary));
            yield return null;
            yield return null;
            Assert.That(_played, Does.Contain(FeedbackCue.BetPlaced));
        }

        [UnityTest]
        public IEnumerator VerifyRevealAndDefeat_SoundInOrder_AndMusicStops()
        {
            yield return StartMatch();
            var match = Object.FindAnyObjectByType<MatchSessionNetwork>();
            _played.Clear();

            match.RequestVerify();
            yield return null;
            yield return null;
            Assert.That(_played, Is.EqualTo(new[] { FeedbackCue.Lock, FeedbackCue.CountdownBeep }));

            yield return new WaitForSeconds(3.3f);
            Assert.That(_played, Does.Contain(FeedbackCue.Trapdoor));
            Assert.That(_played, Does.Contain(FeedbackCue.Fail), "Todo el dinero estaba fuera de las bandejas: se pierde.");
            Assert.That(_played.FindAll(c => c == FeedbackCue.CountdownBeep).Count, Is.EqualTo(3), "3, 2, 1.");

            yield return new WaitForSeconds(7.3f);
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.Defeat));
            Assert.That(_played, Does.Contain(FeedbackCue.Defeat));
            Assert.That(_feedback.CurrentMusicBlock, Is.EqualTo(FeedbackRules.NO_MUSIC));
            Assert.That(_confetti.CorrectCelebrations, Is.Zero, "Fallar no lanza confeti.");
            Assert.That(_confetti.IsVictoryRaining, Is.False, "No hay confeti al perder.");
        }

        [UnityTest]
        public IEnumerator CorrectAnswer_ShootsColouredConfettiBesideTheScreen()
        {
            yield return StartMatch();
            var match = Object.FindAnyObjectByType<MatchSessionNetwork>();
            var correct = match.Order.BoothOf(match.CurrentQuestion.CorrectOption);

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

            match.RequestVerify();
            yield return new WaitForSeconds(3.3f);
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.Reveal));
            Assert.That(match.LastRound.Kept, Is.GreaterThan(0), "El dinero estaba en la cabina correcta.");
            Assert.That(_confetti.CorrectCelebrations, Is.EqualTo(1));
            Assert.That(Burst("CorrectLeft").isPlaying && Burst("CorrectRight").isPlaying, Is.True,
                "Salen los dos chorros, a cada lado de la pantalla.");
            Assert.That(_confetti.IsVictoryRaining, Is.False, "La lluvia dorada es solo para la victoria.");
            Assert.That(_played, Does.Contain(FeedbackCue.Success));
        }

        [UnityTest]
        public IEnumerator Victory_RainsGoldenConfetti_AndClearsItBackInTheLobby()
        {
            yield return StartMatch();
            var match = Object.FindAnyObjectByType<MatchSessionNetwork>();
            typeof(MatchSessionNetwork).GetMethod("Finish", PRIVATE).Invoke(match, new object[] { MatchPhase.Victory });
            yield return null;
            yield return null;

            Assert.That(_played, Does.Contain(FeedbackCue.Victory));
            Assert.That(_confetti.IsVictoryRaining, Is.True, "Lluvia dorada en toda la sala.");
            Assert.That(_feedback.CurrentMusicBlock, Is.EqualTo(FeedbackRules.NO_MUSIC));

            match.RequestReturnToLobby();
            yield return null;
            yield return null;
            yield return null;
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.Lobby));
            Assert.That(_confetti.IsVictoryRaining, Is.False);
        }

        private ParticleSystem Burst(string side) => _confetti.transform.Find(side).GetComponent<ParticleSystem>();

        private IEnumerator StartMatch()
        {
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;
        }

        private FeedbackLibrary Library() => (FeedbackLibrary)typeof(GameFeedbackPlayer).GetField("_library", PRIVATE).GetValue(_feedback);

        private AudioClip ActiveMusicClip()
        {
            var sources = (AudioSource[])typeof(GameFeedbackPlayer).GetField("_musicSources", PRIVATE).GetValue(_feedback);
            var active = (int)typeof(GameFeedbackPlayer).GetField("_activeMusic", PRIVATE).GetValue(_feedback);
            return sources[active].clip;
        }
    }
}
