using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TMPro;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Match.Network;
using Trivias.Menu.Components;
using Trivias.Networking.Components;
using Trivias.Presenter.Data;
using Trivias.Presenter.Logic;
using Trivias.Presenter.Network;
using Trivias.Questions.Data;
using Trivias.Shared.Interaction;
using Trivias.Shared.Profile;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using static Trivias.App.Tests.PlayModeHelpers;

namespace Trivias.App.Tests
{
    /// <summary>Historial de preguntas vistas, nombre del jugador y comentarios nuevos del presentador.</summary>
    public sealed class QuestionsAndNamesTests
    {
        private const BindingFlags PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo StartField = typeof(MatchConfig).GetField("_testStartQuestion", PRIVATE);

        private PlayerProfile _profile;
        private QuestionHistory _history;
        private ConnectionService _connection;

        [UnitySetUp]
        public IEnumerator LoadStage()
        {
            _profile = TemporaryProfileSetup.FreshProfile();
            _history = TemporaryProfileSetup.FreshHistory();
            TemporaryProfileSetup.FreshRecords();
            yield return SceneManager.LoadSceneAsync("Stage", LoadSceneMode.Single);
            yield return null;
            _connection = Object.FindAnyObjectByType<ConnectionService>();
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

        // ---- Historial de preguntas ----

        [UnityTest]
        public IEnumerator Match_StartsWithAQuestionNobodyHasSeen_AndRemembersIt()
        {
            Assert.That(_connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            var match = Object.FindAnyObjectByType<MatchSessionNetwork>();
            var bank = (QuestionBank)typeof(MatchSessionNetwork).GetField("_bank", PRIVATE).GetValue(match);
            var unseenEasy = bank.Questions.Where(q => q.Difficulty == 1).Take(5).Select(q => q.Id).ToList();
            foreach (var question in bank.Questions)
            {
                if (!unseenEasy.Contains(question.Id))
                    _history.MarkSeen(question.Id);
            }

            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;

            Assert.That(match.QuestionNumber, Is.EqualTo(1));
            Assert.That(unseenEasy, Does.Contain(match.CurrentQuestion.Id),
                "Con todo lo demás visto, la primera fácil debe ser una de las cinco que nadie ha visto.");
            Assert.That(_history.Seen.Last(), Is.EqualTo(match.CurrentQuestion.Id), "La pregunta mostrada queda apuntada.");
        }

        // ---- Nombre ----

        [UnityTest]
        public IEnumerator NameTypedInTheMenu_IsSaved_AndShownInTheRoom()
        {
            var menu = Object.FindAnyObjectByType<MenuView>();
            var field = (TMP_InputField)typeof(MenuView).GetField("_playerName", PRIVATE).GetValue(menu);
            Assert.That(field.characterLimit, Is.EqualTo(PlayerNameRules.MAX_LENGTH));
            field.text = "  Ana   <b>López  ";
            ((Button)typeof(MenuView).GetField("_hostButton", PRIVATE).GetValue(menu)).onClick.Invoke();
            Assert.That(_profile.PlayerName, Is.EqualTo("Ana bLópez"), "Se guarda limpio al crear la sala.");
            Assert.That(field.text, Is.EqualTo("Ana bLópez"), "El campo muestra cómo lo verán los demás.");

            yield return new WaitForSeconds(0.5f);
            var avatar = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Trivias.Shared.Players.IPlayerAvatar>();
            Assert.That(avatar.DisplayName, Is.EqualTo("Ana bLópez"));
            var seats = (LobbyPlayerRow[])typeof(MenuView).GetField("_rows", PRIVATE).GetValue(menu);
            Assert.That(seats[0].NameText, Is.EqualTo("Ana bLópez"), "La sala de espera muestra el nombre elegido.");
        }

        [UnityTest]
        public IEnumerator WithoutAName_TheContestantIsJugador1()
        {
            Assert.That(_profile.PlayerName, Is.Empty);
            Assert.That(_connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            var avatar = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Trivias.Shared.Players.IPlayerAvatar>();
            Assert.That(avatar.DisplayName, Is.EqualTo("Jugador 1"));

            _profile.SetName("Eva");
            yield return null;
            yield return null;
            Assert.That(avatar.DisplayName, Is.EqualTo("Eva"), "Un cambio de nombre en el perfil llega al concursante.");
        }

        // ---- Presentador ----

        [UnityTest]
        public IEnumerator Presenter_AnnouncesTheLastQuestion()
        {
            Assert.That(_connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            var match = Object.FindAnyObjectByType<MatchSessionNetwork>();
            var config = (MatchConfig)typeof(MatchSessionNetwork).GetField("_config", PRIVATE).GetValue(match);
            var original = (int)StartField.GetValue(config);
            try
            {
                StartField.SetValue(config, 14);
                yield return WinCurrentQuestion(match);
                Assert.That(match.Phase, Is.EqualTo(MatchPhase.Betting));
                Assert.That(match.QuestionNumber, Is.EqualTo(15));
                Assert.That(PresenterSaid(PresenterEvent.LastQuestion), Is.True,
                    $"Al empezar la última debe avisar. Dijo: «{Presenter().CurrentLine}»");
            }
            finally
            {
                StartField.SetValue(config, original);
            }
        }

        // ---- Utilidades ----

        private static IEnumerator WinCurrentQuestion(MatchSessionNetwork match)
        {
            var player = NetworkManager.Singleton.LocalClient.PlayerObject;
            Assert.That(Object.FindAnyObjectByType<ConnectionService>().TryStartMatch(), Is.True);
            yield return null;
            var correct = match.Order.BoothOf(match.CurrentQuestion.CorrectOption);
            var table = FindTable();
            MoveNextTo(player, table.Position);
            yield return null;
            table.Interact(new InteractionContext(player.OwnerClientId, InteractionAction.Secondary));
            yield return null;
            var tray = FindTray(correct);
            MoveNextTo(player, tray.Position);
            yield return null;
            tray.Interact(new InteractionContext(player.OwnerClientId, InteractionAction.Secondary));
            yield return null;
            match.RequestVerify();
            yield return PlayModeHelpers.AfterRound();
            yield return null;
        }

        private static PresenterRuntime Presenter() =>
            (PresenterRuntime)typeof(PresenterSessionNetwork).GetField("_runtime", PRIVATE)
                .GetValue(Object.FindAnyObjectByType<PresenterSessionNetwork>());

        /// <summary>La frase actual es una de las del evento (con sus variables ya rellenas).</summary>
        private static bool PresenterSaid(PresenterEvent presenterEvent)
        {
            var presenter = Object.FindAnyObjectByType<PresenterSessionNetwork>();
            var catalog = (PresenterCatalog)typeof(PresenterSessionNetwork).GetField("_catalog", PRIVATE).GetValue(presenter);
            var line = Presenter().CurrentLine;
            for (var i = 0; i < catalog.Count; i++)
            {
                if (catalog[i].Event != presenterEvent)
                    continue;
                var parts = Regex.Split(catalog[i].Text, @"\{[^}]*\}").Select(Regex.Escape);
                if (Regex.IsMatch(line, "^" + string.Join(".+", parts) + "$"))
                    return true;
            }
            return false;
        }
    }
}
