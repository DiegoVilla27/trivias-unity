using System.Text;
using TMPro;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Menu.Logic;
using Trivias.Shared.Events;
using Trivias.Shared.Players;
using Trivias.Shared.Profile;
using Trivias.Shared.Rooms;
using Trivias.Shared.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Menu.Components
{
    /// <summary>
    /// Menú del juego:
    /// - Principal: tu nombre (se guarda en el perfil), crear sala, unirme a una sala, cómo se juega, récords,
    ///   configuración, salir.
    /// - Unirme: dirección del host (recuerda la última) y conectar.
    /// - Sala de espera: jugadores, «Invita a tus amigos» con la dirección para compartir (host), empezar, tienda, salir.
    /// Desaparece al empezar la partida y vuelve al regresar a la sala. Se redibuja solo por eventos.
    /// </summary>
    public sealed class MenuView : MonoBehaviour
    {
        private const string ERROR_COLOR = "#FF8080";

        [SerializeField] private RoomConnectionRuntime _room;
        [SerializeField] private MatchRuntime _match;
        [SerializeField] private PlayerAvatarRegistry _avatars;
        [SerializeField] private PlayerProfile _profile;
        [SerializeField] private GameSettings _settings;
        [SerializeField] private VoidEventChannel _openShopRequested;
        [SerializeField] private HowToPlayView _howToPlay;
        [SerializeField] private SettingsView _settingsView;
        [SerializeField] private RecordsView _recordsView;

        [Header("Pantalla principal")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private TMP_InputField _playerName;
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _joinMenuButton;
        [SerializeField] private Button _howToPlayButton;
        [SerializeField] private Button _recordsButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private TextMeshProUGUI _mainStatus;

        [Header("Unirme a una sala")]
        [SerializeField] private GameObject _joinPanel;
        [SerializeField] private TMP_InputField _address;
        [SerializeField] private Button _connectButton;
        [SerializeField] private Button _joinBackButton;
        [SerializeField] private TextMeshProUGUI _joinStatus;

        [Header("Sala de espera")]
        [SerializeField] private GameObject _lobbyPanel;
        [SerializeField] private TextMeshProUGUI _players;
        [SerializeField] private TextMeshProUGUI _lobbyInfo;
        [SerializeField] private GameObject _invitePanel;
        [SerializeField] private TextMeshProUGUI _inviteText;
        [SerializeField] private Button _copyButton;
        [SerializeField] private TextMeshProUGUI _copyLabel;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _lobbyHowToPlayButton;
        [SerializeField] private Button _leaveButton;

        private readonly StringBuilder _builder = new();
        private bool _joinOpen;
        private string _joinError;

        public bool IsMainOpen => _mainPanel.activeSelf;
        public bool IsJoinOpen => _joinPanel.activeSelf;
        public bool IsLobbyOpen => _lobbyPanel.activeSelf;

        private void OnEnable()
        {
            _room.Changed += Refresh;
            _match.Changed += Refresh;
            _avatars.Changed += OnAvatarChanged;
            _profile.Changed += Refresh;
            _playerName.onEndEdit.AddListener(OnNameEdited);
            _playerName.SetTextWithoutNotify(_profile.PlayerName);
            _hostButton.onClick.AddListener(OnHost);
            _joinMenuButton.onClick.AddListener(OpenJoin);
            _howToPlayButton.onClick.AddListener(_howToPlay.Open);
            _recordsButton.onClick.AddListener(_recordsView.Open);
            _settingsButton.onClick.AddListener(_settingsView.Open);
            _quitButton.onClick.AddListener(GameQuit.Quit);
            _connectButton.onClick.AddListener(OnConnect);
            _joinBackButton.onClick.AddListener(CloseJoin);
            _address.onSubmit.AddListener(OnAddressSubmitted);
            _copyButton.onClick.AddListener(OnCopy);
            _startButton.onClick.AddListener(OnStart);
            _shopButton.onClick.AddListener(OnShop);
            _lobbyHowToPlayButton.onClick.AddListener(_howToPlay.Open);
            _leaveButton.onClick.AddListener(OnLeave);
            Refresh();
        }

        private void OnDisable()
        {
            _room.Changed -= Refresh;
            _match.Changed -= Refresh;
            _avatars.Changed -= OnAvatarChanged;
            _profile.Changed -= Refresh;
            _playerName.onEndEdit.RemoveListener(OnNameEdited);
            _hostButton.onClick.RemoveListener(OnHost);
            _joinMenuButton.onClick.RemoveListener(OpenJoin);
            _howToPlayButton.onClick.RemoveListener(_howToPlay.Open);
            _recordsButton.onClick.RemoveListener(_recordsView.Open);
            _settingsButton.onClick.RemoveListener(_settingsView.Open);
            _quitButton.onClick.RemoveListener(GameQuit.Quit);
            _connectButton.onClick.RemoveListener(OnConnect);
            _joinBackButton.onClick.RemoveListener(CloseJoin);
            _address.onSubmit.RemoveListener(OnAddressSubmitted);
            _copyButton.onClick.RemoveListener(OnCopy);
            _startButton.onClick.RemoveListener(OnStart);
            _shopButton.onClick.RemoveListener(OnShop);
            _lobbyHowToPlayButton.onClick.RemoveListener(_howToPlay.Open);
            _leaveButton.onClick.RemoveListener(OnLeave);
        }

        private void OnAvatarChanged(ulong _) => Refresh();

        private void Refresh()
        {
            var connection = _room.Connection;
            if (connection == null)
            {
                _mainPanel.SetActive(false);
                _joinPanel.SetActive(false);
                _lobbyPanel.SetActive(false);
                return;
            }

            if (connection.State == RoomConnectionState.Connected)
            {
                _joinOpen = false;
                _mainPanel.SetActive(false);
                _joinPanel.SetActive(false);
                var inMatch = _match.Session != null && _match.Session.Phase != MatchPhase.Lobby;
                _lobbyPanel.SetActive(!inMatch);
                if (!inMatch)
                    RefreshLobby(connection);
                return;
            }

            _lobbyPanel.SetActive(false);
            _mainPanel.SetActive(!_joinOpen);
            _joinPanel.SetActive(_joinOpen);
            if (_joinOpen)
                RefreshJoin(connection);
            else
                RefreshMain(connection);
        }

        private void RefreshMain(IRoomConnection connection)
        {
            var connecting = connection.State == RoomConnectionState.Connecting;
            _hostButton.interactable = !connecting;
            _joinMenuButton.interactable = !connecting;
            _mainStatus.text = string.IsNullOrEmpty(connection.StatusMessage)
                ? string.Empty
                : $"<color={ERROR_COLOR}>{connection.StatusMessage}</color>";
        }

        private void RefreshJoin(IRoomConnection connection)
        {
            var connecting = connection.State == RoomConnectionState.Connecting;
            _connectButton.interactable = !connecting;
            _address.interactable = !connecting;
            if (!string.IsNullOrEmpty(_joinError))
                _joinStatus.text = $"<color={ERROR_COLOR}>{_joinError}</color>";
            else if (connecting)
                _joinStatus.text = connection.StatusMessage;
            else
                _joinStatus.text = string.IsNullOrEmpty(connection.StatusMessage) || connection.State == RoomConnectionState.Disconnected
                    ? "Pega la dirección que te pasó el host."
                    : $"<color={ERROR_COLOR}>{connection.StatusMessage}</color>";
        }

        private void RefreshLobby(IRoomConnection connection)
        {
            _builder.Clear();
            var count = 0;
            foreach (var avatar in _avatars.All)
            {
                _builder.Append(count > 0 ? "\n" : string.Empty).Append(avatar.DisplayName);
                count++;
            }
            _players.text = $"<size=75%>JUGADORES {count}/{connection.MaxPlayers}</size>\n{_builder}";

            _invitePanel.SetActive(connection.IsHost);
            if (connection.IsHost)
                _inviteText.text = InviteText.Build(connection.ShareableAddresses);

            _startButton.gameObject.SetActive(connection.IsHost);
            _startButton.interactable = connection.CanStartMatch;
            _lobbyInfo.text = $"Tienes {_profile.Tokens} fichas\n" + (connection.IsHost
                ? connection.MinPlayersToStart > 1
                    ? $"Se necesitan al menos {connection.MinPlayersToStart} jugadores."
                    : "Modo prueba: se puede empezar solo."
                : "Esperando a que el host empiece la partida…");
        }

        public string PlayerNameText => _playerName.text;

        /// <summary>Guarda el nombre limpio y lo muestra tal como lo verán los demás.</summary>
        private void OnNameEdited(string _) => SaveName();

        private void SaveName()
        {
            _profile.SetName(_playerName.text);
            _playerName.SetTextWithoutNotify(_profile.PlayerName);
        }

        private void OnHost()
        {
            SaveName();
            _copyLabel.text = "Copiar";
            _room.Connection?.Host();
        }

        private void OpenJoin()
        {
            SaveName();
            _joinOpen = true;
            _joinError = null;
            _address.text = string.IsNullOrEmpty(_settings.LastAddress) ? string.Empty : _settings.LastAddress;
            Refresh();
        }

        private void CloseJoin()
        {
            _joinOpen = false;
            _joinError = null;
            if (_room.Connection?.State == RoomConnectionState.Connecting)
                _room.Connection.Leave();
            Refresh();
        }

        private void OnAddressSubmitted(string _) => OnConnect();

        private void OnConnect()
        {
            if (!RoomAddressInput.TryNormalize(_address.text, out var address, out var error))
            {
                _joinError = error;
                Refresh();
                return;
            }

            _joinError = null;
            _address.text = address;
            _settings.SetLastAddress(address);
            _room.Connection?.Join(address);
        }

        private void OnCopy()
        {
            var address = InviteText.Primary(_room.Connection?.ShareableAddresses);
            if (string.IsNullOrEmpty(address))
                return;
            GUIUtility.systemCopyBuffer = address;
            _copyLabel.text = "¡Copiada!";
        }

        private void OnStart() => _room.Connection?.StartMatch();

        private void OnLeave() => _room.Connection?.Leave();

        private void OnShop() => _openShopRequested.Raise();
    }
}
