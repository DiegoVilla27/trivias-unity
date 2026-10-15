using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using Trivias.Match.Components;
using Trivias.Match.Logic;
using Trivias.Match.Network;
using Trivias.Networking.Components;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Trivias.App.Tests
{
    /// <summary>La pantalla grande muestra la pregunta, las opciones y el estado en cada fase.</summary>
    public sealed class QuestionScreenTests
    {
        private const BindingFlags PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic;

        private ConnectionService _connection;
        private QuestionScreenView _screen;
        private MatchSessionNetwork _match;

        [UnitySetUp]
        public IEnumerator LoadStage()
        {
            yield return SceneManager.LoadSceneAsync("Stage", LoadSceneMode.Single);
            yield return null;
            _connection = Object.FindAnyObjectByType<ConnectionService>();
            _screen = Object.FindAnyObjectByType<QuestionScreenView>();
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
        public IEnumerator Lobby_ShowsGetReadyMessage()
        {
            Assert.That(_connection.StartHost(), Is.True);
            yield return new WaitForSeconds(0.5f);
            Assert.That(Content().activeSelf, Is.False);
            Assert.That(Text("_message").gameObject.activeSelf, Is.True);
            Assert.That(Text("_message").text, Is.EqualTo("¿PREPARADOS?"));
        }

        [UnityTest]
        public IEnumerator Betting_ShowsQuestionOptionsAndClock()
        {
            yield return StartMatch();
            var question = _match.CurrentQuestion;

            Assert.That(Content().activeSelf, Is.True);
            Assert.That(Text("_message").gameObject.activeSelf, Is.False);
            Assert.That(Text("_question").text, Is.EqualTo(question.Text));
            Assert.That(Text("_header").text, Does.Contain("PREGUNTA 1/15"));

            var order = _match.Order;
            for (var booth = 0; booth < 4; booth++)
            {
                Assert.That(OptionText(booth).text, Does.Contain(question.Option(order.OriginalAt(booth))),
                    $"La opción {(char)('A' + booth)} no coincide con su cabina.");
                Assert.That(OptionBox(booth).activeSelf, Is.True, "Antes de revelar todos los recuadros están encendidos.");
            }

            Assert.That(Text("_status").text, Does.Match(@"\d:\d\d"), "Debe verse el reloj de las apuestas.");
        }

        [UnityTest]
        public IEnumerator Verify_ShowsLocked_ThenOnlyTheCorrectBoxStaysLit()
        {
            yield return StartMatch();
            _match.RequestVerify();
            yield return null;
            yield return null;
            Assert.That(Text("_status").text, Does.StartWith("CERRADO"));

            yield return new WaitForSeconds(3.3f);
            var correct = _match.CorrectBooth;
            Assert.That(Text("_status").text, Does.StartWith($"CORRECTA: {(char)('A' + correct)}"));
            Assert.That(Text("_status").text, Does.Contain(_match.CurrentQuestion.Explanation), "La explicación se ve en la pantalla.");
            for (var booth = 0; booth < 4; booth++)
                Assert.That(OptionBox(booth).activeSelf, Is.EqualTo(booth == correct), $"Recuadro {(char)('A' + booth)}.");
        }

        [UnityTest]
        public IEnumerator Defeat_ShowsDefeatMessage()
        {
            yield return StartMatch();
            _match.RequestVerify(); // Sin apostar nada: con la regla provisional se pierde todo.
            yield return new WaitForSeconds(3.3f + 7.3f);
            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Defeat));
            Assert.That(Content().activeSelf, Is.False);
            Assert.That(Text("_message").text, Is.EqualTo("DERROTA"));
        }

        private IEnumerator StartMatch()
        {
            Assert.That(_connection.StartHost(), Is.True);
            yield return new WaitForSeconds(0.5f);
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;
            _match = Object.FindAnyObjectByType<MatchSessionNetwork>();
        }

        private GameObject Content() => (GameObject)typeof(QuestionScreenView).GetField("_content", PRIVATE).GetValue(_screen);

        private TextMeshPro Text(string field) => (TextMeshPro)typeof(QuestionScreenView).GetField(field, PRIVATE).GetValue(_screen);

        private TextMeshPro OptionText(int booth) =>
            ((TextMeshPro[])typeof(QuestionScreenView).GetField("_optionTexts", PRIVATE).GetValue(_screen))[booth];

        private GameObject OptionBox(int booth) =>
            ((GameObject[])typeof(QuestionScreenView).GetField("_optionBoxes", PRIVATE).GetValue(_screen))[booth];
    }
}
