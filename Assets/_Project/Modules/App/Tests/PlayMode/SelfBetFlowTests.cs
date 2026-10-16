using System.Collections;
using NUnit.Framework;
using Trivias.Match.Logic;
using Trivias.Match.Network;
using Trivias.Money.Logic;
using Trivias.Money.Network;
using Trivias.Networking.Components;
using Trivias.Player.Components;
using Trivias.SelfBet.Network;
using Trivias.Shared.Gameplay;
using Trivias.Shared.Interaction;
using Trivias.Shared.Players;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static Trivias.App.Tests.PlayModeHelpers;

namespace Trivias.App.Tests
{
    /// <summary>Hito 3: comodín (apostarse uno mismo) con multiplicador y expulsión.</summary>
    public sealed class SelfBetFlowTests
    {
        private MatchSessionNetwork _match;
        private MoneySessionNetwork _money;
        private SelfBetSessionNetwork _bets;
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

            _match = Object.FindAnyObjectByType<MatchSessionNetwork>();
            _money = Object.FindAnyObjectByType<MoneySessionNetwork>();
            _bets = Object.FindAnyObjectByType<SelfBetSessionNetwork>();
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

        [Test]
        public void SoloTestMode_GetsSixUses()
        {
            Assert.That(_bets.Book.Bettors.Count, Is.EqualTo(1));
            Assert.That(_bets.Book.Bettors[0].PersonalUses, Is.EqualTo(6));
            Assert.That(_bets.IsOpen, Is.True);
        }

        [UnityTest]
        public IEnumerator CorrectSelfBet_DoublesTheTray()
        {
            var correct = _match.Order.BoothOf(_match.CurrentQuestion.CorrectOption);
            yield return BetBundles(correct);
            yield return StandInZoneAndBet(correct);
            Assert.That(_bets.Book.Bettors[0].Booth, Is.EqualTo(correct), "No quedó apostado.");

            _match.RequestVerify();
            yield return PlayModeHelpers.AfterLock();

            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Reveal));
            Assert.That(_match.LastMultiplier, Is.EqualTo(2));
            Assert.That(_match.LastRound.Kept, Is.EqualTo(25_000));
            Assert.That(_match.LastRound.Bonus, Is.EqualTo(25_000));
            Assert.That(_match.TeamMoney, Is.EqualTo(50_000));
            Assert.That(_bets.Book.Bettors[0].PersonalUses, Is.EqualTo(5), "El uso se consume.");
            Assert.That(Avatar().IsEliminated, Is.False);
        }

        [UnityTest]
        public IEnumerator WrongSelfBet_ExpelsAndEndsInDefeat()
        {
            var correct = _match.Order.BoothOf(_match.CurrentQuestion.CorrectOption);
            var wrong = (correct + 1) % 4;
            yield return BetBundles(correct);
            yield return StandInZoneAndBet(wrong);

            _match.RequestVerify();
            yield return PlayModeHelpers.AfterLock();
            Assert.That(Avatar().IsEliminated, Is.True, "Fallar el comodín expulsa.");
            Assert.That(_match.LastEliminated, Does.Contain("Jugador 1"));

            yield return PlayModeHelpers.AfterReveal();
            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Defeat), "Sin concursantes activos es derrota.");
        }

        [UnityTest]
        public IEnumerator Bet_ConfinesPlayerToTheZone_UntilCancelled()
        {
            var limit = MovementLimit();
            yield return StandInZoneAndBet(0);
            Assert.That(limit.HasZone, Is.True, "Apostado, no debe poder salir del círculo.");
            var center = FindBetZone(0).Position;
            Assert.That(new Vector2(limit.Center.x - center.x, limit.Center.z - center.z).magnitude, Is.LessThan(0.01f));

            FindBetZone(0).Interact(new InteractionContext(_client, InteractionAction.Primary));
            yield return null;
            yield return null;
            Assert.That(_bets.Book.Bettors[0].HasBet, Is.False);
            Assert.That(limit.HasZone, Is.False, "Al cancelar vuelve a moverse libre.");
            Assert.That(_bets.Book.Bettors[0].PersonalUses, Is.EqualTo(6), "Cancelar no gasta el uso.");
        }

        [UnityTest]
        public IEnumerator AllActiveBet_CanVerifyFromTheZone()
        {
            yield return StandInZoneAndBet(1);
            Assert.That(_bets.Book.AllActiveHaveBet, Is.True);
            FindBetZone(1).Interact(new InteractionContext(_client, InteractionAction.Secondary));
            yield return null;
            yield return null;
            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Locked), "Q en la zona debe verificar si todos están apostados.");
            Assert.That(MovementLimit().HasZone, Is.True, "Durante la cuenta atrás sigue dentro del círculo.");
        }

        private PlayerMovementLimit MovementLimit() =>
            (PlayerMovementLimit)typeof(PlayerMotor)
                .GetField("_movementLimit", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .GetValue(_player.GetComponent<PlayerMotor>());

        [UnityTest]
        public IEnumerator ToggleInSameZone_Cancels()
        {
            yield return StandInZoneAndBet(2);
            FindBetZone(2).Interact(new InteractionContext(_client, InteractionAction.Primary));
            yield return null;
            yield return null;
            Assert.That(_bets.Book.Bettors[0].HasBet, Is.False);
        }

        private IPlayerAvatar Avatar() => _player.GetComponent<IPlayerAvatar>();

        private IEnumerator StandInZoneAndBet(int booth)
        {
            var zone = FindBetZone(booth);
            MoveTo(_player, zone.Position);
            yield return null;
            zone.Interact(new InteractionContext(_client, InteractionAction.Primary));
            yield return null;
            yield return null;
        }

        private IEnumerator BetBundles(int booth)
        {
            MoveNextTo(_player, FindTable().Position);
            yield return null;
            FindTable().Interact(new InteractionContext(_client, InteractionAction.Secondary));
            yield return null;
            MoveNextTo(_player, FindTray(booth).Position);
            yield return null;
            FindTray(booth).Interact(new InteractionContext(_client, InteractionAction.Secondary));
            yield return null;
            Assert.That(_money.Ledger.TotalAt(MoneyPlace.Tray, booth), Is.EqualTo(25_000));
        }
    }
}
