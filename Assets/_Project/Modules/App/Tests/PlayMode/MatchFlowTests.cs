using System.Collections;
using NUnit.Framework;
using Trivias.Match.Logic;
using Trivias.Match.Network;
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
    /// <summary>Hito 2: una pregunta completa con apuestas, verificación, cuenta atrás y resolución.</summary>
    public sealed class MatchFlowTests
    {
        private ConnectionService _connection;
        private MatchSessionNetwork _match;
        private MoneySessionNetwork _money;
        private NetworkObject _player;

        [UnitySetUp]
        public IEnumerator StartHostAndMatch()
        {
            yield return SceneManager.LoadSceneAsync("Stage", LoadSceneMode.Single);
            _connection = Object.FindAnyObjectByType<ConnectionService>();
            Assert.That(_connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();

            _match = Object.FindAnyObjectByType<MatchSessionNetwork>();
            _money = Object.FindAnyObjectByType<MoneySessionNetwork>();
            _player = NetworkManager.Singleton.LocalClient.PlayerObject;
            Assert.That(_match, Is.Not.Null, "El host no creó la sesión de partida.");
            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Lobby));

            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
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
        public void StartMatch_ShowsFirstEasyQuestion()
        {
            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Betting));
            Assert.That(_match.QuestionNumber, Is.EqualTo(1));
            Assert.That(_match.CurrentQuestion.Difficulty, Is.EqualTo(1));
            Assert.That(_match.CorrectBooth, Is.EqualTo(-1), "La respuesta no se conoce antes de verificar.");
            Assert.That(_match.Order.IsValidPermutation(), Is.True);
        }

        [UnityTest]
        public IEnumerator Verify_LocksBets_ThenRevealsAndResolves()
        {
            var correct = _match.Order.BoothOf(_match.CurrentQuestion.CorrectOption);
            var wrong = (correct + 1) % 4;
            yield return BetBundles(correct);
            yield return BetBundles(wrong);
            Assert.That(_match.MoneyAtRisk, Is.EqualTo(50_000), "Quedan 10 fajos en la mesa.");

            _match.RequestVerify();
            yield return null;
            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Locked));

            // Bloqueo inmediato: ya no se puede mover dinero.
            var table = Object.FindAnyObjectByType<MoneyTableTarget>();
            MoveNextTo(table.Position);
            yield return null;
            table.Interact(new InteractionContext(_player.OwnerClientId, InteractionAction.Secondary));
            yield return null;
            Assert.That(_money.Ledger.CountCarriedBy(_player.OwnerClientId), Is.Zero, "Se cogió dinero con las apuestas cerradas.");

            yield return PlayModeHelpers.AfterLock();
            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Reveal));
            Assert.That(_match.CorrectBooth, Is.EqualTo(correct));
            Assert.That(_match.LastRound.Kept, Is.EqualTo(25_000));
            Assert.That(_match.LastRound.Lost, Is.EqualTo(75_000), "Bandeja incorrecta (25.000) + mesa (50.000).");
            Assert.That(_match.TeamMoney, Is.EqualTo(25_000));

            yield return PlayModeHelpers.AfterReveal();
            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Betting), "Debe pasar a la pregunta 2.");
            Assert.That(_match.QuestionNumber, Is.EqualTo(2));
            Assert.That(_money.IsLocked, Is.False, "El dinero se desbloquea con la nueva pregunta.");
        }

        [UnityTest]
        public IEnumerator VerifyWithoutBetting_EndsInDefeat()
        {
            _match.RequestVerify();
            yield return PlayModeHelpers.AfterLock();
            Assert.That(_match.TeamMoney, Is.Zero);
            yield return PlayModeHelpers.AfterReveal();
            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Defeat));
        }

        private IEnumerator BetBundles(int booth)
        {
            var table = Object.FindAnyObjectByType<MoneyTableTarget>();
            MoveNextTo(table.Position);
            yield return null;
            table.Interact(new InteractionContext(_player.OwnerClientId, InteractionAction.Secondary));
            yield return null;

            var tray = FindTray(booth);
            MoveNextTo(tray.Position);
            yield return null;
            tray.Interact(new InteractionContext(_player.OwnerClientId, InteractionAction.Secondary));
            yield return null;
        }

        private void MoveNextTo(Vector3 target)
        {
            var toCenter = -target;
            toCenter.y = 0f;
            var position = target + (toCenter.sqrMagnitude > 0.01f ? toCenter.normalized : Vector3.back) * 1.0f;
            position.y = 0.05f;
            _player.GetComponent<PlayerMotor>().Teleport(position, Quaternion.identity);
        }

        private static MoneyTrayTarget FindTray(int booth)
        {
            var letter = ((char)('A' + booth)).ToString();
            foreach (var tray in Object.FindObjectsByType<MoneyTrayTarget>(FindObjectsInactive.Exclude))
            {
                if (tray.transform.parent.name.EndsWith(letter))
                    return tray;
            }
            Assert.Fail($"No se encontró la bandeja {letter}.");
            return null;
        }
    }
}
