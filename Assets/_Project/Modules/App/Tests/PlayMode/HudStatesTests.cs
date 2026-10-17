using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using Trivias.HUD.Components;
using Trivias.Interaction.Components;
using Trivias.Match.Components;
using Trivias.Match.Logic;
using Trivias.Match.Network;
using Trivias.Networking.Components;
using Trivias.SelfBet.Components;
using Trivias.Shared.Interaction;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static Trivias.App.Tests.PlayModeHelpers;

namespace Trivias.App.Tests
{
    /// <summary>
    /// HUD de la partida según los bocetos (versión sin saturar): tres tarjetas fijas arriba y el resto solo cuando
    /// importa (lo que llevas, el comodín preparado, el cierre y el resultado), avisos con teclas dibujadas y el ✓ sobre
    /// la cabina correcta.
    /// </summary>
    public sealed class HudStatesTests
    {
        private const BindingFlags PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic;

        private MatchSessionNetwork _match;
        private NetworkObject _player;
        private ulong _client;
        private MatchHudView _hud;

        [UnitySetUp]
        public IEnumerator StartHostAndMatch()
        {
            yield return SceneManager.LoadSceneAsync("Stage", LoadSceneMode.Single);
            var connection = Object.FindAnyObjectByType<ConnectionService>();
            Assert.That(connection.StartHost(), Is.True);
            yield return UntilHostReady();
            Assert.That(connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;

            _match = Object.FindAnyObjectByType<MatchSessionNetwork>();
            _player = NetworkManager.Singleton.LocalClient.PlayerObject;
            _client = _player.OwnerClientId;
            _hud = Object.FindAnyObjectByType<MatchHudView>();
        }

        [UnityTearDown]
        public IEnumerator Shutdown()
        {
            SetPace(LOCK_SECONDS, REVEAL_SECONDS);
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
                Object.Destroy(NetworkManager.Singleton.gameObject);
            }
            yield return null;
        }

        [Test]
        public void Betting_ShowsThreeSmallCardsOnTop_AndNothingElseUntilItMatters()
        {
            Assert.That(Shown("_questionCard") && Shown("_clockCard") && Shown("_moneyCard"), Is.True, "Las tres tarjetas fijas.");
            Assert.That(Text("_question").text, Does.Contain($"{_match.QuestionNumber}/{_match.TotalQuestions}"));
            Assert.That(Text("_state").text, Is.EqualTo("Apuestas abiertas"));
            Assert.That(Text("_clock").text, Does.Match(@"^\d\d:\d\d$"));
            Assert.That(Text("_teamMoney").text, Is.EqualTo("100.000"));
            Assert.That(Text("_selfBets").text, Does.Contain("Comodines").And.Contain("reserva"), "Los comodines, en la tarjeta del dinero.");

            Assert.That(Shown("_risk"), Is.True, "Todo el dinero está en la mesa: se perdería al verificar.");
            Assert.That(Text("_riskText").text, Does.Contain("100.000").And.Contain("Se pierde al verificar"));
            Assert.That(Shown("_escHint"), Is.True, "En la primera pregunta se recuerda el menú.");
            foreach (var card in new[] { "_carryCard", "_selfBetCard", "_lockedCard", "_revealCard" })
                Assert.That(Shown(card), Is.False, $"{card}: solo cuando importa.");
        }

        [UnityTest]
        public IEnumerator Carrying_ShowsWhatYouCarry_WithThatTypesCapacity_AndKeysAreDrawn()
        {
            var table = FindTable();
            MoveNextTo(_player, table.Position);
            yield return null;
            table.Interact(new InteractionContext(_client, InteractionAction.Primary));
            yield return null;
            yield return null;

            Assert.That(Shown("_carryCard"), Is.True, "Mientras llevas algo.");
            Assert.That(Text("_carry").text, Is.EqualTo("1 fajo · 5.000"));
            Assert.That(Text("_carryLimit").text, Does.StartWith("Capacidad 1/"));

            // El aviso de la mesa: cada acción con su tecla dibujada (el interactor lo renueva en cuanto vuelve a mirar).
            var prompt = Object.FindAnyObjectByType<InteractionPromptView>();
            for (var wait = 0f; wait < 1f && prompt.Shown.Length == 0; wait += Time.deltaTime)
                yield return null;
            Assert.That(prompt.Shown, Does.StartWith("E "), prompt.Shown);
            var keys = (TMP_Text[])typeof(InteractionPromptView).GetField("_keyLabels", PRIVATE).GetValue(prompt);
            var keyCaps = (GameObject[])typeof(InteractionPromptView).GetField("_keys", PRIVATE).GetValue(prompt);
            Assert.That(keys[0].text, Is.EqualTo("E"));
            Assert.That(keyCaps[0].activeInHierarchy, Is.True);

            table.Interact(new InteractionContext(_client, InteractionAction.Tertiary));
            yield return null;
            yield return null;
            Assert.That(Shown("_carryCard"), Is.False, "Con las manos vacías, la tarjeta se va.");
        }

        [UnityTest]
        public IEnumerator APreparedSelfBet_ShowsWhatTheBoothWouldGive_IfRight()
        {
            var zone = FindBetZone(1);
            MoveTo(_player, zone.Position);
            yield return null;
            zone.Interact(new InteractionContext(_client, InteractionAction.Primary));
            yield return null;
            yield return null;

            Assert.That(Shown("_selfBetCard"), Is.True);
            Assert.That(Text("_selfBetTitle").text, Is.EqualTo("COMODÍN PREPARADO · B"));
            Assert.That(Text("_selfBetProjection").text, Does.Contain("Si aciertas").And.Contain("× 2"), "Uno apostado: x2.");
        }

        [UnityTest]
        public IEnumerator Locking_ShowsTheCountdownInsteadOfTheClock_AndHidesActions()
        {
            var table = FindTable();
            MoveNextTo(_player, table.Position);
            yield return null;
            table.Interact(new InteractionContext(_client, InteractionAction.Primary));
            var zone = FindBetZone(3);
            MoveTo(_player, zone.Position);
            yield return null;
            zone.Interact(new InteractionContext(_client, InteractionAction.Primary));
            yield return null;

            _match.RequestVerify();
            yield return null;
            yield return null;

            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Locked));
            Assert.That(Shown("_lockedCard"), Is.True);
            Assert.That(Text("_countdown").text, Does.Match(@"^\d$"), "Un solo número grande (3-2-1).");
            Assert.That(Text("_state").text, Is.EqualTo("Apuestas cerradas"));
            foreach (var card in new[] { "_clockCard", "_risk", "_carryCard", "_selfBetCard", "_escHint" })
                Assert.That(Shown(card), Is.False, $"{card}: nada invita a algo que ya no se puede hacer.");

            var label = Text<TextMeshPro>(FindBoothLabel(3), "_label", typeof(BoothBettorsLabel));
            Assert.That(label.text, Does.StartWith("APOSTADOS · BLOQUEADO"), "Los apostados, bloqueados.");
        }

        [UnityTest]
        public IEnumerator TheReveal_CountsUpTheRoundsResult_AndMarksTheRightBooth()
        {
            SetPace(LOCK_SECONDS, 3f);
            _match.RequestVerify();
            yield return AfterLock();

            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Reveal));
            Assert.That(Shown("_revealCard"), Is.True);
            Assert.That(Shown("_lockedCard"), Is.False);
            Assert.That(Text("_state").text, Is.EqualTo("Respuesta revelada"));
            Assert.That(Text("_bonusLabel").text, Is.EqualTo("Bono x1"));

            yield return new WaitForSeconds(0.8f);
            var round = _match.LastRound;
            Assert.That(round.Lost, Is.EqualTo(100000), "Todo estaba fuera de las bandejas.");
            Assert.That(Text("_lost").text, Is.EqualTo("–100.000"), "La cifra ya ha llegado a su valor.");
            Assert.That(Text("_kept").text, Is.EqualTo("0"));

            var presenter = Object.FindAnyObjectByType<Trivias.Presenter.Components.PresenterAnimator>();
            Assert.That(presenter.Animator.GetCurrentAnimatorStateInfo(0).IsName("Sad") || presenter.Animator.GetNextAnimatorStateInfo(0).IsName("Sad"),
                Is.True, "Se ha perdido todo: el presentador se decepciona.");

            foreach (var mark in Object.FindObjectsByType<BoothRevealMark>(FindObjectsInactive.Exclude))
            {
                var booth = (int)typeof(BoothRevealMark).GetField("_booth", PRIVATE).GetValue(mark);
                Assert.That(mark.IsShown, Is.EqualTo(booth == _match.CorrectBooth), $"✓ solo sobre la correcta (cabina {(char)('A' + booth)}).");
            }
        }

        private static BoothBettorsLabel FindBoothLabel(int booth)
        {
            foreach (var label in Object.FindObjectsByType<BoothBettorsLabel>(FindObjectsInactive.Exclude))
            {
                if ((int)typeof(BoothBettorsLabel).GetField("_booth", PRIVATE).GetValue(label) == booth)
                    return label;
            }
            Assert.Fail($"No hay rótulo de apostados en la cabina {booth}.");
            return null;
        }

        private bool Shown(string field) => ((GameObject)typeof(MatchHudView).GetField(field, PRIVATE).GetValue(_hud)).activeSelf;

        private TMP_Text Text(string field) => Text<TMP_Text>(_hud, field, typeof(MatchHudView));

        private static T Text<T>(object target, string field, System.Type type) where T : class =>
            (T)type.GetField(field, PRIVATE).GetValue(target);
    }
}
