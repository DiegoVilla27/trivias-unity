using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using Trivias.Cameras.Components;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Match.Network;
using Trivias.Menu.Components;
using Trivias.Networking.Components;
using Trivias.Ranking.Data;
using Trivias.Ranking.Logic;
using Trivias.Results.Components;
using Trivias.Shared.Interaction;
using Trivias.Shared.Players;
using Trivias.Shared.Profile;
using Trivias.Spectator.Components;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using static Trivias.App.Tests.PlayModeHelpers;

namespace Trivias.App.Tests
{
    /// <summary>Récords locales (menú y resultados) y cámara de espectador del eliminado.</summary>
    public sealed class RecordsAndSpectatorTests
    {
        private const BindingFlags PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo StartField = typeof(MatchConfig).GetField("_testStartQuestion", PRIVATE);

        private LocalRecords _records;
        private ConnectionService _connection;
        private FakeAvatar _fake;

        [UnitySetUp]
        public IEnumerator LoadStage()
        {
            TemporaryProfileSetup.FreshProfile();
            _records = TemporaryProfileSetup.FreshRecords();
            yield return SceneManager.LoadSceneAsync("Stage", LoadSceneMode.Single);
            yield return null;
            _connection = Object.FindAnyObjectByType<ConnectionService>();
        }

        [UnityTearDown]
        public IEnumerator Shutdown()
        {
            if (_fake != null)
            {
                Avatars().Unregister(_fake.ClientId);
                Object.Destroy(_fake.Root);
                _fake = null;
            }
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
                Object.Destroy(NetworkManager.Singleton.gameObject);
            }
            yield return null;
        }

        // ---- Récords ----

        [UnityTest]
        public IEnumerator RecordsMenu_ShowsEachGroupSize_AndEscCloses()
        {
            _records.Submit(RankingRules.Entry("a", 3, 6_400_000, 754f, "Ana, Luis", "Eva", new System.DateTime(2026, 10, 15)),
                RecordEligibility.Eligible);
            _records.Submit(RankingRules.Entry("b", 3, 500_000, 600f, "Ana", "Luis, Eva", new System.DateTime(2026, 10, 14)),
                RecordEligibility.Eligible);

            var menu = Object.FindAnyObjectByType<MenuView>();
            var view = Object.FindAnyObjectByType<RecordsView>();
            Assert.That(view.IsOpen, Is.False);
            ((Button)typeof(MenuView).GetField("_recordsButton", PRIVATE).GetValue(menu)).onClick.Invoke();
            yield return null;

            Assert.That(view.IsOpen, Is.True);
            Assert.That(view.GroupSize, Is.EqualTo(3), "Abre en la primera categoría con victorias.");
            Assert.That(view.ListText, Does.Contain("12:34").And.Contain("Ana, Luis, Eva").And.Contain("2/3"),
                "Tiempo, participantes y supervivientes de cada victoria.");
            Assert.That(view.ListText.IndexOf("6.400.000", System.StringComparison.Ordinal),
                Is.LessThan(view.ListText.IndexOf("500.000", System.StringComparison.Ordinal)), "Primero el mayor premio.");

            var tabs = (Button[])typeof(RecordsView).GetField("_tabs", PRIVATE).GetValue(view);
            Assert.That(tabs, Has.Length.EqualTo(5), "Una pestaña por tamaño de grupo, de 2 a 6.");
            Assert.That(tabs[0].GetComponentInChildren<TextMeshProUGUI>().text, Is.EqualTo("2 jugadores"));
            tabs[3].onClick.Invoke();
            Assert.That(view.GroupSize, Is.EqualTo(5));
            Assert.That(view.ListText, Is.EqualTo(Trivias.Menu.Logic.RecordsTable.EMPTY), "Categoría vacía: lo dice claro.");

            Object.FindAnyObjectByType<PauseMenuView>().HandleEscape();
            Assert.That(view.IsOpen, Is.False, "Esc cierra los récords.");
        }

        [UnityTest]
        public IEnumerator TestRunVictory_DoesNotEnterTheRecords_AndSaysWhy()
        {
            Assert.That(_connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            var match = Object.FindAnyObjectByType<MatchSessionNetwork>();
            var config = (MatchConfig)typeof(MatchSessionNetwork).GetField("_config", PRIVATE).GetValue(match);
            var original = (int)StartField.GetValue(config);
            try
            {
                StartField.SetValue(config, 15);
                yield return WinCurrentQuestion(match);
                Assert.That(match.Phase, Is.EqualTo(MatchPhase.Victory));
                Assert.That(match.Result.StartingPlayers, Is.EqualTo(1));
                Assert.That(match.Result.StartedAtFirstQuestion, Is.False);

                var results = Object.FindAnyObjectByType<ResultsView>();
                Assert.That(results.LastRecord.Kind, Is.EqualTo(RecordResultKind.NotEligible));
                Assert.That(results.LastRecord.Eligibility, Is.EqualTo(RecordEligibility.TestRun));
                var details = (TextMeshProUGUI)typeof(ResultsView).GetField("_details", PRIVATE).GetValue(results);
                Assert.That(details.text, Does.Contain("no cuenta para los récords"));
                Assert.That(_records.Book.entries, Is.Empty, "Nada se guarda.");
            }
            finally
            {
                StartField.SetValue(config, original);
            }
        }

        [UnityTest]
        public IEnumerator Defeat_DoesNotTalkAboutRecords()
        {
            Assert.That(_connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            var match = Object.FindAnyObjectByType<MatchSessionNetwork>();
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            match.RequestVerify();
            yield return PlayModeHelpers.AfterRound();
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.Defeat));
            Assert.That(match.Result.StartedAtFirstQuestion, Is.True, "Partida normal, desde la pregunta 1.");
            Assert.That(match.Result.StartingPlayers, Is.EqualTo(1));

            var results = Object.FindAnyObjectByType<ResultsView>();
            Assert.That(results.LastRecord.Eligibility, Is.EqualTo(RecordEligibility.Defeat));
            var details = (TextMeshProUGUI)typeof(ResultsView).GetField("_details", PRIVATE).GetValue(results);
            Assert.That(details.text, Does.Not.Contain("récord"), "Una derrota no habla de récords.");
            Assert.That(_records.Book.entries, Is.Empty);
        }

        // ---- Espectador ----

        [UnityTest]
        public IEnumerator Eliminated_WatchesActivePlayers_AndGetsBackOnRespawn()
        {
            Assert.That(_connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            var player = NetworkManager.Singleton.LocalClient.PlayerObject;
            var local = player.GetComponent<IPlayerAvatar>();
            var spectator = Object.FindAnyObjectByType<SpectatorController>();
            var camera = FollowCamera();
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;

            _fake = new FakeAvatar(99, "Jugador Falso");
            Avatars().Register(_fake);
            yield return null;
            Assert.That(spectator.IsSpectating, Is.False, "Un activo no es espectador.");
            Assert.That(spectator.IsBannerShown, Is.False);

            local.Eliminate();
            yield return null;
            yield return null;
            Assert.That(spectator.IsWaitingToStart, Is.True, "Primero se ve la expulsión.");
            Assert.That(camera.Target.TrackingTarget, Is.Null, "Durante la expulsión, vista general.");

            yield return new WaitForSeconds(2.8f);
            Assert.That(spectator.IsSpectating, Is.True);
            Assert.That(spectator.IsFollowing(_fake.ClientId), Is.True, "Mira al único activo.");
            Assert.That(camera.Target.TrackingTarget, Is.EqualTo(_fake.ViewTarget), "La cámara sigue al activo.");
            Assert.That(spectator.IsBannerShown, Is.True);
            Assert.That(spectator.BannerText, Does.Contain("ELIMINADO").And.Contain("Viendo a <b>Jugador Falso</b>"));

            spectator.ToggleOverview();
            Assert.That(spectator.IsOverview, Is.True);
            Assert.That(camera.Target.TrackingTarget, Is.Null, "Espacio: vista general.");
            spectator.ToggleOverview();
            Assert.That(spectator.IsFollowing(_fake.ClientId), Is.True, "Espacio otra vez: vuelve a seguir.");
            spectator.Next();
            Assert.That(spectator.IsFollowing(_fake.ClientId), Is.True, "Con un solo activo, siguiente es el mismo.");

            _fake.IsEliminated = true;
            Avatars().NotifyChanged(_fake.ClientId);
            Assert.That(spectator.IsOverview, Is.True, "Si cae el que miraba y no queda nadie: vista general.");
            Assert.That(spectator.BannerText, Does.Contain("Vista general").And.Not.Contain("seguir"));

            local.Respawn();
            yield return null;
            yield return null;
            Assert.That(spectator.IsSpectating, Is.False, "De vuelta en juego.");
            Assert.That(spectator.IsBannerShown, Is.False);
            Assert.That(camera.Target.TrackingTarget, Is.EqualTo(local.ViewTarget), "La cámara vuelve a su concursante.");
        }

        [UnityTest]
        public IEnumerator SpectatorBanner_HidesWhenTheMatchIsOver()
        {
            Assert.That(_connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            var local = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<IPlayerAvatar>();
            var spectator = Object.FindAnyObjectByType<SpectatorController>();
            var match = Object.FindAnyObjectByType<MatchSessionNetwork>();
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            _fake = new FakeAvatar(98, "Jugador Falso");
            Avatars().Register(_fake);

            // Todo el dinero fuera de las bandejas: al verificar se pierde y la partida acaba en derrota.
            // (Se verifica antes de eliminarlo: un eliminado no puede pulsar el botón.)
            // La expulsión dura 2,8 s: la revelación de esta prueba dura lo justo para verla terminar.
            PlayModeHelpers.SetPace(PlayModeHelpers.LOCK_SECONDS, 3f);
            try
            {
                match.RequestVerify();
                yield return null;
                local.Eliminate();
                yield return new WaitForSeconds(2.8f);
                Assert.That(spectator.IsBannerShown, Is.True, "Durante la cuenta atrás y la revelación se ve el rótulo.");
                yield return new WaitForSeconds(PlayModeHelpers.LOCK_SECONDS + 3f - 2.8f + 0.3f);
            }
            finally
            {
                PlayModeHelpers.SetPace(PlayModeHelpers.LOCK_SECONDS, PlayModeHelpers.REVEAL_SECONDS);
            }
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.Defeat));
            Assert.That(spectator.IsBannerShown, Is.False, "En los resultados el rótulo sobra.");
            Assert.That(Object.FindAnyObjectByType<ResultsView>().IsShown, Is.True);
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

        private static PlayerAvatarRegistry Avatars()
        {
            var spectator = Object.FindAnyObjectByType<SpectatorController>();
            return (PlayerAvatarRegistry)typeof(SpectatorController).GetField("_avatars", PRIVATE).GetValue(spectator);
        }

        private static CinemachineCamera FollowCamera()
        {
            var binder = Object.FindAnyObjectByType<LocalPlayerCameraBinder>();
            return (CinemachineCamera)typeof(LocalPlayerCameraBinder).GetField("_followCamera", PRIVATE).GetValue(binder);
        }

        /// <summary>Otro concursante activo (en las pruebas solo existe el host).</summary>
        private sealed class FakeAvatar : IPlayerAvatar
        {
            public readonly GameObject Root;

            public FakeAvatar(ulong clientId, string name)
            {
                ClientId = clientId;
                DisplayName = name;
                Root = new GameObject(name);
                Root.transform.position = new Vector3(2f, 0f, -3f);
                var view = new GameObject("ViewTarget");
                view.transform.SetParent(Root.transform, false);
                view.transform.localPosition = new Vector3(0f, 1.5f, 0f);
                ViewTarget = view.transform;
            }

            public ulong ClientId { get; }
            public string DisplayName { get; }
            public Vector3 Position => Root.transform.position;
            public Transform CarryPoint => Root.transform;
            public Transform HeadAnchor => null;
            public string CharacterId => string.Empty;
            public CosmeticLoadout Loadout => default;
            public bool IsLocal => false;
            public Transform ViewTarget { get; }
            public bool IsEliminated { get; set; }

            public void Eliminate() => IsEliminated = true;

            public void Respawn() => IsEliminated = false;
        }
    }
}
