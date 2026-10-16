using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Trivias.Feedback.Components;
using Trivias.Feedback.Logic;
using Trivias.Match.Logic;
using Trivias.Match.Network;
using Trivias.Menu.Components;
using Trivias.Money.Components;
using Trivias.Money.Logic;
using Trivias.Money.Network;
using Trivias.Networking.Components;
using Trivias.Player.Components;
using Trivias.Shared.Interaction;
using Trivias.Shared.Presentation;
using Trivias.Shared.Tubes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using static Trivias.App.Tests.PlayModeHelpers;

namespace Trivias.App.Tests
{
    /// <summary>
    /// El plató vivo en juego: el dinero perdido y el expulsado suben por los tubos, el público, las bombillas y las
    /// cámaras reaccionan, y los paneles de la interfaz aparecen con su animación.
    /// </summary>
    public sealed class StageLifeTests
    {
        private const BindingFlags PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic;

        private MatchSessionNetwork _match;
        private MoneySessionNetwork _money;
        private NetworkObject _player;
        private ulong _client;

        [UnitySetUp]
        public IEnumerator StartHostAndMatch()
        {
            TemporaryProfileSetup.FreshProfile();
            yield return SceneManager.LoadSceneAsync("Stage", LoadSceneMode.Single);
            var connection = Object.FindAnyObjectByType<ConnectionService>();
            Assert.That(connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            Assert.That(connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;
            _match = Object.FindAnyObjectByType<MatchSessionNetwork>();
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
        public IEnumerator LostMoney_IsSuckedUpTheTubeOfItsBooth()
        {
            var correct = _match.Order.BoothOf(_match.CurrentQuestion.CorrectOption);
            var wrong = (correct + 1) % 4;
            yield return BetBundles(wrong);
            var presenter = Object.FindAnyObjectByType<MoneyVisualPresenter>();
            var tubes = (TubeRegistry)typeof(MoneyVisualPresenter).GetField("_tubes", PRIVATE).GetValue(presenter);
            Assert.That(tubes.TryGet(TubeKind.Money, wrong, out var tube), Is.True, "Cada cabina tiene su tubo del dinero.");

            _match.RequestVerify();
            yield return PlayModeHelpers.AfterLock();
            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Reveal));
            Assert.That(presenter.FlightsInProgress, Is.GreaterThan(0), "Los fajos de la bandeja falsa empiezan a subir.");
            Assert.That(tube.IsSucking, Is.True, "El tubo sopla aire y suena al succionar.");

            var highest = 0f;
            for (var t = 0f; t < 1.5f; t += Time.deltaTime)
            {
                foreach (Transform visual in presenter.transform)
                {
                    if (visual.gameObject.activeSelf)
                        highest = Mathf.Max(highest, visual.position.y);
                }
                yield return null;
            }
            Assert.That(highest, Is.GreaterThan(4f), "El dinero sube por el tubo, por encima de la cabina.");

            yield return new WaitForSeconds(2.5f);
            Assert.That(presenter.FlightsInProgress, Is.Zero, "Al llegar al techo, cada fajo vuelve al pool.");
        }

        [UnityTest]
        public IEnumerator ExpelledContestant_IsSuckedIntoTheTubeOverHisPlatform()
        {
            var correct = _match.Order.BoothOf(_match.CurrentQuestion.CorrectOption);
            var wrong = (correct + 1) % 4;
            yield return BetBundles(correct);
            var zone = FindBetZone(wrong);
            MoveTo(_player, zone.Position);
            yield return null;
            zone.Interact(new InteractionContext(_client, InteractionAction.Primary));
            yield return null;

            var expulsion = _player.GetComponent<PlayerExpulsionView>();
            var model = (Transform)typeof(PlayerExpulsionView).GetField("_model", PRIVATE).GetValue(expulsion);
            var tubes = (TubeRegistry)typeof(PlayerExpulsionView).GetField("_tubes", PRIVATE).GetValue(expulsion);
            Assert.That(tubes.TryGet(TubeKind.Player, wrong, out var tube), Is.True);

            _match.RequestVerify();
            yield return PlayModeHelpers.AfterLock();
            Assert.That(tube.IsSucking, Is.True, "Su tubo succiona.");

            var highest = model.position.y;
            for (var t = 0f; t < 1.4f && model.gameObject.activeSelf; t += Time.deltaTime)
            {
                highest = Mathf.Max(highest, model.position.y);
                var flat = model.position - tube.Inlet;
                flat.y = 0f;
                Assert.That(flat.magnitude, Is.LessThan(2.6f), "Va hacia la boca de su tubo, no a otro sitio.");
                yield return null;
            }
            Assert.That(highest, Is.GreaterThan(2.5f), "Sube hacia el tubo.");
            yield return new WaitForSeconds(1.5f);
            Assert.That(model.gameObject.activeSelf, Is.False, "Desaparece por el techo.");
        }

        [UnityTest]
        public IEnumerator Stage_ReactsToEachMoment()
        {
            var bulbs = Object.FindAnyObjectByType<StageBulbs>();
            var crowd = Object.FindAnyObjectByType<AudienceCrowd>();
            var cameras = Object.FindAnyObjectByType<StudioCameraOperators>();
            var button = Object.FindAnyObjectByType<Trivias.VerifyButton.Components.VerifyButtonTarget>();
            Assert.That(bulbs.Mood, Is.Not.EqualTo(StageMood.Countdown));

            var correct = _match.Order.BoothOf(_match.CurrentQuestion.CorrectOption);
            yield return BetBundles(correct);
            _match.RequestVerify();
            yield return null;
            yield return null;
            Assert.That(bulbs.Mood, Is.EqualTo(StageMood.Countdown), "Las bombillas parpadean en la cuenta atrás.");
            Assert.That(Vector3.Distance(cameras.Focus, button.transform.position), Is.LessThan(0.1f), "Las cámaras miran el botón.");

            yield return PlayModeHelpers.AfterLock();
            Assert.That(crowd.Reaction, Is.EqualTo(CrowdReaction.Applause), "El público aplaude el acierto.");
            var figure = ((Transform[])typeof(AudienceCrowd).GetField("_figures", PRIVATE).GetValue(crowd))[0];
            float low = float.MaxValue, high = float.MinValue;
            for (var t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                low = Mathf.Min(low, figure.localPosition.y);
                high = Mathf.Max(high, figure.localPosition.y);
                yield return null;
            }
            Assert.That(high - low, Is.GreaterThan(0.02f), "Y se le ve: botan en sus asientos.");
            Assert.That(bulbs.Mood, Is.EqualTo(StageMood.Correct));
            for (var i = 0; i < bulbs.Count; i++)
            {
                if (bulbs.BoothOf(i) == correct)
                    Assert.That(bulbs.LevelOf(i), Is.GreaterThan(0.75f), "La cabina correcta brilla.");
                else if (bulbs.BoothOf(i) >= 0)
                    Assert.That(bulbs.LevelOf(i), Is.LessThan(0.2f), "Las falsas se apagan.");
            }
            var booths = (Transform[])typeof(StudioCameraOperators).GetField("_booths", PRIVATE).GetValue(cameras);
            Assert.That(Vector3.Distance(cameras.Focus, booths[correct].position), Is.LessThan(0.1f), "Las cámaras miran la cabina correcta.");
        }

        [UnityTest]
        public IEnumerator Panels_PopIn_WhenOpened()
        {
            NetworkManager.Singleton.Shutdown();
            yield return null;
            var pause = Object.FindAnyObjectByType<PauseMenuView>();
            var settings = Object.FindAnyObjectByType<SettingsView>();
            settings.Open();
            yield return null;
            var panel = ((GameObject)typeof(SettingsView).GetField("_panel", PRIVATE).GetValue(settings))
                .GetComponentInChildren<UiPanelIntro>(true);
            Assert.That(panel, Is.Not.Null, "El panel tiene su animación de entrada.");
            Assert.That(panel.IsPlaying, Is.True, "Aparece con un pequeño rebote.");
            yield return new WaitForSecondsRealtime(0.4f);
            Assert.That(panel.IsPlaying, Is.False);
            Assert.That(panel.transform.localScale.x, Is.EqualTo(1f).Within(0.01f), "Y se queda a su tamaño.");
            Assert.That(panel.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1f).Within(0.01f));
            Assert.That(pause, Is.Not.Null);
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
            Assert.That(_money.Ledger.TotalAt(MoneyPlace.Tray, booth), Is.GreaterThan(0));
        }
    }
}
