using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Shared.Events;
using Trivias.Shared.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Trivias.Menu.Components
{
    /// <summary>
    /// Esc: cierra el panel abierto (tutorial, configuración) o, durante la partida, abre el menú de pausa
    /// (continuar, cómo se juega, configuración, salir de la sala). El juego no se detiene para los demás:
    /// solo tu concursante se queda quieto y el ratón queda libre.
    /// </summary>
    public sealed class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private MatchRuntime _match;
        [SerializeField] private PlayerControlGate _controlGate;
        [SerializeField] private VoidEventChannel _leaveRoomRequested;
        [SerializeField] private HowToPlayView _howToPlay;
        [SerializeField] private SettingsView _settingsView;
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _howToPlayButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _leaveButton;
        [SerializeField] private GameObject _confirmPanel;
        [SerializeField] private Button _confirmLeaveButton;
        [SerializeField] private Button _cancelLeaveButton;

        private InputAction _escape;

        public bool IsOpen => _panel.activeSelf;

        private bool InMatch
        {
            get
            {
                var session = _match.Session;
                return session != null && session.Phase != MatchPhase.Lobby && !MatchRules.IsFinished(session.Phase);
            }
        }

        private void Awake() => _escape = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");

        private void OnEnable()
        {
            _escape.performed += OnEscape;
            _escape.Enable();
            _match.Changed += OnMatchChanged;
            _resumeButton.onClick.AddListener(Close);
            _howToPlayButton.onClick.AddListener(_howToPlay.Open);
            _settingsButton.onClick.AddListener(_settingsView.Open);
            _leaveButton.onClick.AddListener(AskToLeave);
            _confirmLeaveButton.onClick.AddListener(Leave);
            _cancelLeaveButton.onClick.AddListener(CancelLeave);
        }

        private void OnDisable()
        {
            _escape.performed -= OnEscape;
            _escape.Disable();
            _match.Changed -= OnMatchChanged;
            _resumeButton.onClick.RemoveListener(Close);
            _howToPlayButton.onClick.RemoveListener(_howToPlay.Open);
            _settingsButton.onClick.RemoveListener(_settingsView.Open);
            _leaveButton.onClick.RemoveListener(AskToLeave);
            _confirmLeaveButton.onClick.RemoveListener(Leave);
            _cancelLeaveButton.onClick.RemoveListener(CancelLeave);
            Close();
        }

        private void OnDestroy() => _escape.Dispose();

        /// <summary>Lo que hace Esc (público para las pruebas).</summary>
        public void HandleEscape()
        {
            if (_howToPlay.IsOpen)
                _howToPlay.Close();
            else if (_settingsView.IsOpen)
                _settingsView.Close();
            else if (_confirmPanel.activeSelf)
                CancelLeave();
            else if (IsOpen)
                Close();
            else if (InMatch)
                Open();
        }

        public void Open()
        {
            _panel.SetActive(true);
            _confirmPanel.SetActive(false);
            _controlGate.SetPaused(true);
        }

        public void Close()
        {
            _panel.SetActive(false);
            _confirmPanel.SetActive(false);
            _controlGate.SetPaused(false);
        }

        private void OnEscape(InputAction.CallbackContext _) => HandleEscape();

        // Al terminar la partida o volver a la sala, la pausa deja de tener sentido.
        private void OnMatchChanged()
        {
            if (IsOpen && !InMatch)
                Close();
        }

        private void AskToLeave() => _confirmPanel.SetActive(true);

        private void CancelLeave() => _confirmPanel.SetActive(false);

        private void Leave()
        {
            Close();
            _leaveRoomRequested.Raise();
        }
    }
}
