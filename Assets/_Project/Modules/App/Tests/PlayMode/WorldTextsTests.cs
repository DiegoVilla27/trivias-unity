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
using Trivias.Presenter.Data;
using Trivias.Presenter.Logic;
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
            yield return PlayModeHelpers.UntilHostReady();
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
        public void Booths_ShowOnlyTheirMoney_NotTheAnswer()
        {
            var question = _match.CurrentQuestion;
            for (var booth = 0; booth < 4; booth++)
            {
                var tray = FindTray(booth);
                var root = tray.transform.parent;
                foreach (var text in root.GetComponentsInChildren<TextMeshPro>())
                {
                    for (var option = 0; option < 4; option++)
                        Assert.That(text.text, Does.Not.Contain(question.Option(option)), $"{root.name}/{text.name}: la respuesta solo está en la pantalla.");
                }
                var total = Field<TextMeshPro>(tray, "_totalLabel");
                Assert.That(total.text, Is.EqualTo("0"), "La bandeja vacía muestra 0.");
                var window = total.transform.position.y - root.position.y;
                Assert.That(window, Is.InRange(1.6f, 2.4f), "La cantidad se ve en el hueco alto de la cabina, en grande.");
                Assert.That(total.fontSize, Is.GreaterThanOrEqualTo(2.4f));
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
        public void PresenterSpeaks_InATvCaption_WithHisFace()
        {
            var views = Object.FindObjectsByType<PresenterCaptionView>(FindObjectsInactive.Include);
            Assert.That(views.Length, Is.EqualTo(1), "El presentador habla en un solo rótulo.");
            var view = views[0];
            var canvas = view.GetComponent<Canvas>();
            Assert.That(canvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay), "En pantalla: se lee mires donde mires.");
            Assert.That(canvas.sortingOrder, Is.GreaterThan(Object.FindAnyObjectByType<MatchHudView>().GetComponent<Canvas>().sortingOrder));

            Assert.That(view.IsShowing, Is.True, "Al empezar, el presentador da la bienvenida.");
            Assert.That(view.Text, Is.Not.Empty);
            var portrait = view.transform.Find("BottomLeft/Card/Portrait").GetComponent<UnityEngine.UI.Image>();
            Assert.That(portrait.sprite.name, Is.EqualTo("Portrait_Presenter"), "Con su cara.");
            foreach (var text in GameObject.Find("Presenter").GetComponentsInChildren<TMP_Text>(true))
                Assert.Fail($"El presentador del plató no lleva textos ({text.name}): habla en el rótulo.");
        }

        [UnityTest]
        public IEnumerator PresenterCaption_TypesCalmly_AndOnlyItsColourChangesWithTheMood()
        {
            var view = Object.FindAnyObjectByType<PresenterCaptionView>();
            var runtime = Field<PresenterRuntime>(view, "_runtime");
            var colors = (Color[])typeof(PresenterCaptionView).GetField("_moodColors", PRIVATE).GetValue(view);
            var card = (RectTransform)Field<GameObject>(view, "_card").transform;
            var size = card.sizeDelta;

            runtime.Say("¡Bienvenidos al concurso más loco de la televisión!", PresenterMood.Excited);
            yield return null;
            Assert.That(view.IsShowing, Is.True);
            Assert.That(view.VisibleCharacters, Is.InRange(1, view.TotalCharacters - 1), "Las letras salen poco a poco.");
            Assert.That(view.AccentColor, Is.EqualTo(colors[(int)PresenterMood.Excited]), "El ánimo, en el color del nombre.");
            yield return new WaitForSeconds(3f);
            Assert.That(view.VisibleCharacters, Is.EqualTo(view.TotalCharacters), "La frase se termina a tiempo de leerla.");
            Assert.That(card.localRotation, Is.EqualTo(Quaternion.identity), "Sin temblores ni giros.");
            Assert.That(card.sizeDelta, Is.EqualTo(size), "Siempre del mismo tamaño.");

            runtime.Say("Tres... dos... uno...", PresenterMood.Suspense);
            yield return null;
            Assert.That(view.AccentColor, Is.EqualTo(colors[(int)PresenterMood.Suspense]));
            Assert.That(colors[(int)PresenterMood.Suspense], Is.Not.EqualTo(colors[(int)PresenterMood.Excited]));
            Assert.That(view.VisibleCharacters, Is.LessThan(view.TotalCharacters), "Vuelve a escribir desde el principio.");

            runtime.Say(string.Empty);
            yield return new WaitForSeconds(0.3f);
            Assert.That(view.IsShowing, Is.False, "Al callar, el rótulo se va.");
            Assert.That(Field<GameObject>(view, "_card").activeSelf, Is.False);
        }

        [Test]
        public void Hud_DoesNotShowTheQuestionOrOptions()
        {
            var hud = Object.FindAnyObjectByType<MatchHudView>();
            var question = _match.CurrentQuestion;
            foreach (var label in hud.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                // Lo que se lee, sin etiquetas de formato (un «<size=60%>» no es la opción «60»).
                var text = (name: label.name, text: System.Text.RegularExpressions.Regex.Replace(label.text, "<[^>]+>", ""));
                Assert.That(text.text, Does.Not.Contain(question.Text), $"«{text.name}» muestra la pregunta: debe verse solo en la pantalla.");
                // Una opción corta («60», «100») puede salir dentro de otra cifra (el dinero): esas, solo como línea entera.
                var lines = text.text.Split('\n');
                for (var option = 0; option < 4; option++)
                {
                    var answer = question.Option(option);
                    if (answer.Length >= 6)
                        Assert.That(text.text, Does.Not.Contain(answer), $"«{text.name}» muestra una opción.");
                    else
                        Assert.That(lines, Has.None.EqualTo(answer), $"«{text.name}» muestra una opción.");
                }
            }
        }

        [Test]
        public void Hud_ShowsTheBettingClock()
        {
            var hud = Object.FindAnyObjectByType<MatchHudView>();
            Assert.That(Field<TMP_Text>(hud, "_clock").text, Does.Match(@"^\d\d:\d\d$"), "Reloj «01:12».");
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
