using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using Trivias.HUD.Components;
using Trivias.Match.Components;
using Trivias.Match.Network;
using Trivias.Money.Components;
using Trivias.Networking.Components;
using Trivias.Presenter.Components;
using Trivias.SelfBet.Components;
using Trivias.Shared.Interaction;
using Trivias.VerifyButton.Components;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static Trivias.App.Tests.PlayModeHelpers;

namespace Trivias.App.Tests
{
    /// <summary>Textos del plató y avisos: lo que el jugador lee debe corresponder siempre al estado real.</summary>
    public sealed class WorldTextsTests
    {
        private const BindingFlags PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic;

        private MatchSessionNetwork _match;
        private NetworkObject _player;
        private ulong _client;

        [UnitySetUp]
        public IEnumerator StartHostAndMatch()
        {
            yield return SceneManager.LoadSceneAsync("Stage", LoadSceneMode.Single);
            var connection = Object.FindAnyObjectByType<ConnectionService>();
            Assert.That(connection.StartHost(), Is.True);
            yield return new WaitForSeconds(0.5f);
            Assert.That(connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;

            _match = Object.FindAnyObjectByType<MatchSessionNetwork>();
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
        public void BoothLabels_ShowTheirAnswer()
        {
            var question = _match.CurrentQuestion;
            foreach (var label in Object.FindObjectsByType<BoothAnswerLabel>(FindObjectsInactive.Exclude))
            {
                var booth = (int)typeof(BoothAnswerLabel).GetField("_booth", PRIVATE).GetValue(label);
                var text = Field<TextMeshPro>(label, "_label").text;
                Assert.That(text, Is.EqualTo($"{(char)('A' + booth)} · {question.Option(_match.Order.OriginalAt(booth))}"));
            }
        }

        [Test]
        public void TableLabel_ShowsTheTeamMoney() =>
            Assert.That(Field<TextMeshPro>(FindTable(), "_totalLabel").text, Is.EqualTo("MESA\n100.000"));

        [Test]
        public void TablePrompt_OffersTakingOneOrSeveral()
        {
            Assert.That(FindTable().TryGetPrompt(default, out var prompt), Is.True);
            Assert.That(prompt, Does.Contain("Coger fajo (5.000)"));
            Assert.That(prompt, Does.Contain("Coger 5"));
        }

        [Test]
        public void VerifyPrompt_WarnsAboutMoneyOutsideTrays()
        {
            var button = Object.FindAnyObjectByType<VerifyButtonTarget>();
            Assert.That(button.TryGetPrompt(default, out var prompt), Is.True);
            Assert.That(prompt, Does.Contain("VERIFICAR"));
            Assert.That(prompt, Does.Contain("100.000"), "Todo está en la mesa: el aviso debe decir cuánto se perdería.");
        }

        [UnityTest]
        public IEnumerator TrayLabel_ShowsDepositedAmount()
        {
            MoveNextTo(_player, FindTable().Position);
            yield return null;
            FindTable().Interact(new InteractionContext(_client, InteractionAction.Secondary));
            yield return null;
            MoveNextTo(_player, FindTray(2).Position);
            yield return null;
            FindTray(2).Interact(new InteractionContext(_client, InteractionAction.Secondary));
            yield return null;
            yield return null;

            Assert.That(Field<TextMeshPro>(FindTray(2), "_totalLabel").text, Is.EqualTo("25.000"));
            Assert.That(Field<TextMeshPro>(FindTable(), "_totalLabel").text, Is.EqualTo("MESA\n75.000"));
        }

        [UnityTest]
        public IEnumerator BettorsLabel_ShowsWhoIsBetOnTheBooth()
        {
            var zone = FindBetZone(3);
            MoveTo(_player, zone.Position);
            yield return null;
            zone.Interact(new InteractionContext(_client, InteractionAction.Primary));
            yield return null;
            yield return null;

            foreach (var label in Object.FindObjectsByType<BoothBettorsLabel>(FindObjectsInactive.Exclude))
            {
                var booth = (int)typeof(BoothBettorsLabel).GetField("_booth", PRIVATE).GetValue(label);
                var text = Field<TextMeshPro>(label, "_label").text;
                Assert.That(text, booth == 3 ? Is.EqualTo("APOSTADOS\nJugador 1") : Is.Empty, $"Cabina {(char)('A' + booth)}.");
            }
        }

        [Test]
        public void PresenterSpeaks_OnlyInTheBubbleBesideHisHead()
        {
            var views = Object.FindObjectsByType<PresenterLineView>(FindObjectsInactive.Include);
            Assert.That(views.Length, Is.EqualTo(1), "El presentador habla solo en su viñeta (ni subtítulos ni HUD).");
            var view = views[0];
            Assert.That(view.GetComponentInParent<Canvas>(), Is.Null, "La viñeta está en el plató, no en la interfaz.");
            Assert.That(view.name, Is.EqualTo("Presenter"));

            var bubble = Field<GameObject>(view, "_container");
            Assert.That(bubble.activeSelf, Is.True, "Al empezar, el presentador da la bienvenida.");
            // Al pie de la pantalla, la viñeta sale al lado de la cabeza (encima taparía la pregunta).
            var head = view.transform.Find("Head").position;
            var background = bubble.transform.Find("Background").position;
            Assert.That(Mathf.Abs(background.y - head.y), Is.LessThan(0.8f), "A la altura de su cabeza.");
            var side = background - head;
            side.y = 0f;
            Assert.That(side.magnitude, Is.GreaterThan(1.5f), "Al lado, no encima.");
            Assert.That(Field<TMP_Text>(view, "_label").text, Is.Not.Empty);
        }

        [Test]
        public void Hud_DoesNotShowTheQuestionOrOptions()
        {
            var hud = Object.FindAnyObjectByType<MatchHudView>();
            var question = _match.CurrentQuestion;
            foreach (var text in hud.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                Assert.That(text.text, Does.Not.Contain(question.Text), $"«{text.name}» muestra la pregunta: debe verse solo en la pantalla.");
                for (var option = 0; option < 4; option++)
                    Assert.That(text.text, Does.Not.Contain(question.Option(option)), $"«{text.name}» muestra una opción.");
            }
        }

        [Test]
        public void Hud_ShowsTheBettingClock()
        {
            var hud = Object.FindAnyObjectByType<MatchHudView>();
            Assert.That(Field<TextMeshProUGUI>(hud, "_status").text, Does.Match(@"\d:\d\d"));
        }

        private static T Field<T>(object target, string name) where T : class
        {
            for (var type = target.GetType(); type != null; type = type.BaseType)
            {
                var field = type.GetField(name, PRIVATE);
                if (field != null)
                    return (T)field.GetValue(target);
            }
            Assert.Fail($"No existe el campo {name} en {target.GetType().Name}.");
            return null;
        }
    }
}
