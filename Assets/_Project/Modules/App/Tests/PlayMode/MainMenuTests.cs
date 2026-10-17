using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using Trivias.Menu.Components;
using Trivias.Networking.Components;
using Trivias.Shared.Gameplay;
using Trivias.Shared.Rooms;
using Trivias.Shared.Settings;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Trivias.App.Tests
{
    /// <summary>Menú principal, unirse, invitar, tutorial, configuración y pausa (Esc).</summary>
    public sealed class MainMenuTests
    {
        private const BindingFlags PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic;

        private MenuView _menu;
        private HowToPlayView _howToPlay;
        private SettingsView _settingsView;
        private PauseMenuView _pause;
        private ConnectionService _connection;
        private GameSettings _settings;

        [UnitySetUp]
        public IEnumerator LoadStage()
        {
            TemporaryProfileSetup.FreshProfile();
            _settings = TemporaryProfileSetup.FreshSettings();
            yield return SceneManager.LoadSceneAsync("Stage", LoadSceneMode.Single);
            yield return null;
            _menu = Object.FindAnyObjectByType<MenuView>();
            _howToPlay = Object.FindAnyObjectByType<HowToPlayView>();
            _settingsView = Object.FindAnyObjectByType<SettingsView>();
            _pause = Object.FindAnyObjectByType<PauseMenuView>();
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

        [Test]
        public void MainMenu_HasItsOptions()
        {
            Assert.That(_menu.IsMainOpen, Is.True);
            Assert.That(Caption(MenuButton("_hostButton")), Is.EqualTo("Crear sala"));
            Assert.That(Caption(MenuButton("_joinMenuButton")), Is.EqualTo("Unirme a una sala"));
            Assert.That(Caption(MenuButton("_howToPlayButton")), Is.EqualTo("Cómo se juega"));
            Assert.That(Caption(MenuButton("_recordsButton")), Is.EqualTo("Récords"));
            Assert.That(Caption(MenuButton("_settingsButton")), Is.EqualTo("Configuración"));
            Assert.That(Caption(MenuButton("_quitButton")), Is.EqualTo("Salir"));
        }

        [UnityTest]
        public IEnumerator MenuAndLobby_BlurTheStageBehind_ButNotTheMatch()
        {
            Assert.That(_menu.IsBackdropShown, Is.True, "Plató desenfocado detrás del menú principal.");
            var view = Object.FindAnyObjectByType<MenuBackdropView>();
            Assert.That(view.IsShown, Is.True);
            var blur = (UnityEngine.Rendering.Volume)typeof(MenuBackdropView).GetField("_blur", PRIVATE).GetValue(view);
            Assert.That(blur.weight, Is.EqualTo(1f), "Desenfocado.");
            var backdrop = ((GameObject)typeof(MenuBackdropView).GetField("_dim", PRIVATE).GetValue(view)).GetComponent<Image>();
            Assert.That(backdrop.color.a, Is.InRange(0.15f, 0.5f), "Un velo suave: el plató se sigue viendo detrás.");
            var rect = backdrop.rectTransform;
            Assert.That(rect.anchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(rect.anchorMax, Is.EqualTo(Vector2.one), "A pantalla completa.");
            Assert.That(rect.GetSiblingIndex(), Is.LessThan(_menu.transform.Find("MainScreen").GetSiblingIndex()), "Detrás del menú.");

            MenuButton("_joinMenuButton").onClick.Invoke();
            yield return null;
            Assert.That(_menu.IsBackdropShown, Is.True, "También al unirse a una sala.");

            Assert.That(_connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            Assert.That(_menu.IsLobbyOpen, Is.True);
            Assert.That(_menu.IsBackdropShown, Is.True, "La sala de espera, también con el plató desenfocado (boceto 03).");

            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;
            Assert.That(_menu.IsBackdropShown, Is.False, "En la partida se ve el plató tal cual.");
            Assert.That(blur.weight, Is.Zero);
        }

        [UnityTest]
        public IEnumerator Join_AsksForTheAddress_RemembersIt_AndGoesBack()
        {
            _settings.SetLastAddress("10.147.17.5");
            MenuButton("_joinMenuButton").onClick.Invoke();
            yield return null;
            Assert.That(_menu.IsJoinOpen, Is.True);
            Assert.That(_menu.IsMainOpen, Is.False);
            Assert.That(Address().text, Is.EqualTo("10.147.17.5"), "Recuerda la última dirección.");

            MenuButton("_joinBackButton").onClick.Invoke();
            yield return null;
            Assert.That(_menu.IsMainOpen, Is.True);
        }

        [UnityTest]
        public IEnumerator Join_WithAnInvalidAddress_ExplainsAndDoesNotConnect()
        {
            MenuButton("_joinMenuButton").onClick.Invoke();
            yield return null;
            Address().text = "10.147.17";
            MenuButton("_connectButton").onClick.Invoke();
            yield return null;
            Assert.That(_connection.State, Is.EqualTo(RoomConnectionState.Disconnected), "No intenta conectar.");
            Assert.That(MenuLabel("_joinStatus").text, Does.Contain("no es válida"));
        }

        [UnityTest]
        public IEnumerator Join_WithAValidAddress_TriesToConnect_AndSavesIt()
        {
            MenuButton("_joinMenuButton").onClick.Invoke();
            yield return null;
            Address().text = " 127.0.0.1 ";
            MenuButton("_connectButton").onClick.Invoke();
            yield return null;
            Assert.That(_connection.State, Is.Not.EqualTo(RoomConnectionState.Disconnected), "Intenta conectar.");
            Assert.That(_settings.LastAddress, Is.EqualTo("127.0.0.1"));
        }

        [UnityTest]
        public IEnumerator CreatingARoom_ShowsTheAddressToShare()
        {
            MenuButton("_hostButton").onClick.Invoke();
            yield return PlayModeHelpers.UntilHostReady();
            Assert.That(_menu.IsLobbyOpen, Is.True);
            var invite = (GameObject)typeof(MenuView).GetField("_invitePanel", PRIVATE).GetValue(_menu);
            Assert.That(invite.activeSelf, Is.True, "El host ve «Invita a tus amigos».");
            var shareable = _connection.ShareableAddresses;
            Assert.That(shareable.Count, Is.GreaterThan(0));
            Assert.That(MenuLabel("_inviteAddress").text, Is.EqualTo(shareable[0].Address));

            MenuButton("_copyButton").onClick.Invoke();
            Assert.That(MenuLabel("_copyLabel").text, Is.EqualTo("Copiado"));
        }

        [UnityTest]
        public IEnumerator HowToPlay_PagesThroughAndCloses()
        {
            MenuButton("_howToPlayButton").onClick.Invoke();
            yield return null;
            Assert.That(_howToPlay.IsOpen, Is.True);
            Assert.That(_howToPlay.Page, Is.Zero);

            var next = (Button)typeof(HowToPlayView).GetField("_nextButton", PRIVATE).GetValue(_howToPlay);
            for (var i = 1; i < _howToPlay.PageCount; i++)
                next.onClick.Invoke();
            Assert.That(_howToPlay.Page, Is.EqualTo(_howToPlay.PageCount - 1));
            next.onClick.Invoke(); // «Entendido»
            Assert.That(_howToPlay.IsOpen, Is.False);
        }

        [UnityTest]
        public IEnumerator HowToPlay_DrawsEachKeyLikeTheGamesKeys()
        {
            MenuButton("_howToPlayButton").onClick.Invoke();
            yield return null;
            _howToPlay.Show(_howToPlay.PageCount - 1); // Controles: W A S D, Shift, Ratón, Espacio, E, Q, R y Esc.
            yield return null;

            var keyed = (Trivias.Shared.Presentation.UiKeyedText)typeof(HowToPlayView).GetField("_bodyKeys", PRIVATE).GetValue(_howToPlay);
            Assert.That(keyed.ShownKeys, Is.EqualTo(11), "Una tecla dibujada por cada tecla del texto.");
            Assert.That(keyed.Label.text, Does.Not.Contain("["), "Sin corchetes: se ven teclas.");

            // Cada tecla, bajo su letra: la «W» empieza la primera línea, a la izquierda del texto.
            var keys = (UnityEngine.UI.Image[])typeof(Trivias.Shared.Presentation.UiKeyedText).GetField("_keys", PRIVATE).GetValue(keyed);
            var info = keyed.Label.textInfo;
            var w = info.characterInfo[0];
            Assert.That(w.character, Is.EqualTo('W'));
            var center = keys[0].rectTransform.anchoredPosition;
            Assert.That(center.x, Is.EqualTo((w.bottomLeft.x + w.topRight.x) * 0.5f).Within(w.pointSize * 0.2f));
            Assert.That(center.y, Is.EqualTo((w.bottomLeft.y + w.topRight.y) * 0.5f).Within(w.pointSize * 0.5f));
            Assert.That(keys[0].rectTransform.sizeDelta.x, Is.GreaterThan(w.topRight.x - w.bottomLeft.x), "La tecla abraza su letra.");
            Assert.That(keys[0].transform.parent.GetSiblingIndex(), Is.LessThan(keyed.transform.GetSiblingIndex()), "Detrás de las letras.");

            _howToPlay.Show(0); // El objetivo no habla de teclas.
            yield return null;
            Assert.That(keyed.ShownKeys, Is.Zero);
        }

        [UnityTest]
        public IEnumerator Settings_ApplyAtOnce_AndSaveOnClose()
        {
            MenuButton("_settingsButton").onClick.Invoke();
            yield return null;
            Assert.That(_settingsView.IsOpen, Is.True);

            SettingsSlider("_masterVolume").value = 0.4f;
            SettingsSlider("_mouseSensitivity").value = 2f;
            ((Toggle)typeof(SettingsView).GetField("_invertY", PRIVATE).GetValue(_settingsView)).isOn = true;
            Assert.That(AudioListener.volume, Is.EqualTo(0.4f).Within(0.001f), "El volumen general se aplica al momento.");
            Assert.That(_settings.MouseSensitivity, Is.EqualTo(2f).Within(0.001f));
            Assert.That(_settings.InvertY, Is.True);

            ((Button)typeof(SettingsView).GetField("_closeButton", PRIVATE).GetValue(_settingsView)).onClick.Invoke();
            Assert.That(_settingsView.IsOpen, Is.False);
            var path = (string)typeof(GameSettings).GetField("_path", PRIVATE).GetValue(_settings);
            Assert.That(SettingsStore.Load(path).mouseSensitivity, Is.EqualTo(2f).Within(0.001f), "Se guarda al cerrar.");

            _settings.SetMasterVolume(1f);
        }

        [UnityTest]
        public IEnumerator Escape_InTheLobby_DoesNotPause()
        {
            Assert.That(_connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            _pause.HandleEscape();
            Assert.That(_pause.IsOpen, Is.False, "En la sala de espera no hay pausa: ya está el menú.");
        }

        [UnityTest]
        public IEnumerator Escape_DuringTheMatch_PausesAndResumes()
        {
            var gate = Gate();
            Assert.That(_connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;
            Assert.That(gate.CanMove, Is.True);

            _pause.HandleEscape();
            Assert.That(_pause.IsOpen, Is.True);
            Assert.That(gate.CanMove, Is.False, "En pausa tu concursante se queda quieto.");

            _pause.HandleEscape();
            Assert.That(_pause.IsOpen, Is.False);
            Assert.That(gate.CanMove, Is.True);
        }

        [UnityTest]
        public IEnumerator Escape_ClosesTheTutorialBeforeThePause()
        {
            Assert.That(_connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            _pause.HandleEscape();
            ((Button)typeof(PauseMenuView).GetField("_howToPlayButton", PRIVATE).GetValue(_pause)).onClick.Invoke();
            Assert.That(_howToPlay.IsOpen, Is.True);

            _pause.HandleEscape();
            Assert.That(_howToPlay.IsOpen, Is.False);
            Assert.That(_pause.IsOpen, Is.True, "Primero se cierra el tutorial; la pausa sigue abierta.");
        }

        [UnityTest]
        public IEnumerator LeavingFromThePause_AsksFirst_ThenReturnsToTheMainMenu()
        {
            Assert.That(_connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            _pause.HandleEscape();

            PauseButton("_leaveButton").onClick.Invoke();
            var confirm = (GameObject)typeof(PauseMenuView).GetField("_confirmPanel", PRIVATE).GetValue(_pause);
            Assert.That(confirm.activeSelf, Is.True, "Pide confirmación antes de salir.");
            Assert.That(NetworkManager.Singleton.IsListening, Is.True);

            PauseButton("_confirmLeaveButton").onClick.Invoke();
            yield return new WaitForSeconds(0.3f);
            Assert.That(NetworkManager.Singleton.IsListening, Is.False);
            Assert.That(_pause.IsOpen, Is.False);
            Assert.That(Gate().IsPaused, Is.False, "Al salir no se queda la pausa puesta.");
            Assert.That(_menu.IsMainOpen, Is.True);
        }

        private Button MenuButton(string field) => (Button)typeof(MenuView).GetField(field, PRIVATE).GetValue(_menu);
        private Button PauseButton(string field) => (Button)typeof(PauseMenuView).GetField(field, PRIVATE).GetValue(_pause);
        private TextMeshProUGUI MenuLabel(string field) => (TextMeshProUGUI)typeof(MenuView).GetField(field, PRIVATE).GetValue(_menu);
        private TMP_InputField Address() => (TMP_InputField)typeof(MenuView).GetField("_address", PRIVATE).GetValue(_menu);
        private Slider SettingsSlider(string field) => (Slider)typeof(SettingsView).GetField(field, PRIVATE).GetValue(_settingsView);
        private PlayerControlGate Gate() => (PlayerControlGate)typeof(PauseMenuView).GetField("_controlGate", PRIVATE).GetValue(_pause);
        private static string Caption(Button button) => button.GetComponentInChildren<TextMeshProUGUI>().text;
    }
}
