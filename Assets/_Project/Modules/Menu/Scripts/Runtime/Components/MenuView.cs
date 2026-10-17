using System;
using System.Collections.Generic;
using TMPro;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Menu.Logic;
using Trivias.Money.Logic;
using Trivias.Shared.Characters;
using Trivias.Shared.Events;
using Trivias.Shared.Players;
using Trivias.Shared.Presentation;
using Trivias.Shared.Profile;
using Trivias.Shared.Rooms;
using Trivias.Shared.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Menu.Components
{
    /// <summary>
    /// Menú del juego, como en los bocetos (<c>docs/bocetos/ui/menu</c>):
    /// - Principal: tu nombre (se guarda en el perfil), crear sala, unirme, cómo se juega, récords, configuración, salir;
    ///   a la derecha, tu personaje (<see cref="CharacterPickerView"/>).
    /// - Unirme: dirección del anfitrión (recuerda la última) y conectar.
    /// - Sala de espera: las seis plazas (retrato, nombre, personaje y anfitrión), «Invita a tus amigos» con la dirección
    ///   (anfitrión), tu personaje en 3D, empezar, tienda, cómo se juega y salir (con confirmación).
    /// Detrás, el plató desenfocado. Desaparece al empezar la partida y vuelve al regresar a la sala. Se redibuja solo
    /// por eventos.
    /// </summary>
    public sealed class MenuView : MonoBehaviour
    {
        private const string ERROR_COLOR = "#FF8A8A";
        private const ulong HOST_CLIENT_ID = 0; // En Netcode el anfitrión es siempre el cliente 0.

        [SerializeField] private RoomConnectionRuntime _room;
        [SerializeField] private MatchRuntime _match;
        [SerializeField] private PlayerAvatarRegistry _avatars;
        [SerializeField] private PlayerProfile _profile;
        [SerializeField] private CharacterCatalog _characters;
        [SerializeField] private CharacterPreview _preview;
        [SerializeField] private UiBackdrop _backdrop;
        [SerializeField] private GameSettings _settings;
        [SerializeField] private VoidEventChannel _openShopRequested;
        [SerializeField] private HowToPlayView _howToPlay;
        [SerializeField] private SettingsView _settingsView;
        [SerializeField] private RecordsView _recordsView;
        [SerializeField] private PauseMenuView _pause;

        [Header("Pantalla principal")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private TMP_InputField _playerName;
        [Tooltip("«Así te verán los demás · 5/16».")]
        [SerializeField] private TextMeshProUGUI _nameHint;
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
        [SerializeField] private TextMeshProUGUI _joinName;

        [Header("Sala de espera")]
        [SerializeField] private GameObject _lobbyPanel;
        [SerializeField] private TextMeshProUGUI _playersTitle;
        [SerializeField] private LobbyPlayerRow[] _rows;
        [SerializeField] private TextMeshProUGUI _meName;
        [SerializeField] private RawImage _mePortrait;
        [SerializeField] private TextMeshProUGUI _tokens;
        [SerializeField] private GameObject _invitePanel;
        [SerializeField] private TextMeshProUGUI _inviteAddress;
        [SerializeField] private Button _copyButton;
        [SerializeField] private TextMeshProUGUI _copyLabel;
        [SerializeField] private TextMeshProUGUI _lobbyHeading;
        [SerializeField] private Button _startButton;
        [SerializeField] private TextMeshProUGUI _waiting;
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _lobbyHowToPlayButton;
        [SerializeField] private Button _leaveButton;

        private readonly List<IPlayerAvatar> _seated = new();
        private readonly Comparison<IPlayerAvatar> _byClient = (a, b) => a.ClientId.CompareTo(b.ClientId);
        private bool _joinOpen;
        private bool _overlayOpen;
        private bool _lobbyPreview;
        private string _joinError;

        public bool IsMainOpen => _mainPanel.activeSelf;
        /// <summary>El menú pide el plató desenfocado detrás (principal, unirse y sala de espera).</summary>
        public bool IsBackdropShown { get; private set; }
        public bool IsJoinOpen => _joinPanel.activeSelf;
        public bool IsLobbyOpen => _lobbyPanel.activeSelf;

        private void OnEnable()
        {
            _room.Changed += Refresh;
            _match.Changed += Refresh;
            _avatars.Changed += OnAvatarChanged;
            _profile.Changed += Refresh;
            _preview.PortraitsChanged += Refresh;
            _backdrop.Changed += OnBackdropChanged;
            _playerName.onEndEdit.AddListener(OnNameEdited);
            _playerName.onValueChanged.AddListener(OnNameTyped);
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
            _preview.PortraitsChanged -= Refresh;
            _backdrop.Changed -= OnBackdropChanged;
            _playerName.onEndEdit.RemoveListener(OnNameEdited);
            _playerName.onValueChanged.RemoveListener(OnNameTyped);
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
            SetBackdrop(false);
            StopLobbyPreview();
        }

        private void OnAvatarChanged(ulong _) => Refresh();

        /// <summary>Con una ventana encima (tienda, tutorial, récords, configuración), el menú de detrás se esconde.</summary>
        private void OnBackdropChanged()
        {
            if (_overlayOpen == _backdrop.IsCovered)
                return;
            _overlayOpen = _backdrop.IsCovered;
            Refresh();
        }

        private void SetBackdrop(bool shown)
        {
            IsBackdropShown = shown;
            _backdrop.Show(this, shown);
        }

        private void Refresh()
        {
            var connection = _room.Connection;
            if (connection == null)
            {
                SetBackdrop(false);
                _mainPanel.SetActive(false);
                _joinPanel.SetActive(false);
                _lobbyPanel.SetActive(false);
                StopLobbyPreview();
                return;
            }

            if (connection.State == RoomConnectionState.Connected)
            {
                _joinOpen = false;
                _mainPanel.SetActive(false);
                _joinPanel.SetActive(false);
                var inMatch = _match.Session != null && _match.Session.Phase != MatchPhase.Lobby;
                SetBackdrop(!inMatch);
                _lobbyPanel.SetActive(!inMatch && !_overlayOpen);
                if (!inMatch && !_overlayOpen)
                    RefreshLobby(connection);
                else
                    StopLobbyPreview();
                return;
            }

            StopLobbyPreview();
            _lobbyPanel.SetActive(false);
            SetBackdrop(true);
            _mainPanel.SetActive(!_joinOpen && !_overlayOpen);
            _joinPanel.SetActive(_joinOpen && !_overlayOpen);
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
            OnNameTyped(_playerName.text);
        }

        private void RefreshJoin(IRoomConnection connection)
        {
            var connecting = connection.State == RoomConnectionState.Connecting;
            _connectButton.interactable = !connecting;
            _address.interactable = !connecting;
            _joinName.text = $"Tu nombre:  <b><color=#F5EFE1>{PlayerNameRules.DisplayName(_profile.PlayerName, 0)}</color></b>";
            if (!string.IsNullOrEmpty(_joinError))
                _joinStatus.text = $"<color={ERROR_COLOR}>{_joinError}</color>";
            else if (connecting)
                _joinStatus.text = "Conectando…";
            else
                _joinStatus.text = string.IsNullOrEmpty(connection.StatusMessage) || connection.State == RoomConnectionState.Disconnected
                    ? "Recordaremos la última dirección"
                    : $"<color={ERROR_COLOR}>{connection.StatusMessage}</color>";
        }

        private void RefreshLobby(IRoomConnection connection)
        {
            _seated.Clear();
            IPlayerAvatar local = null;
            foreach (var avatar in _avatars.All)
            {
                _seated.Add(avatar);
                if (avatar.IsLocal)
                    local = avatar;
            }
            _seated.Sort(_byClient);

            var seats = Mathf.Min(_rows.Length, connection.MaxPlayers);
            for (var i = 0; i < _rows.Length; i++)
            {
                _rows[i].gameObject.SetActive(i < seats);
                if (i >= seats)
                    continue;
                if (i < _seated.Count)
                {
                    var avatar = _seated[i];
                    var character = _characters.Find(avatar.CharacterId);
                    _rows[i].Show(avatar.DisplayName, character != null ? character.DisplayName : string.Empty,
                        _preview.Face(avatar.CharacterId), avatar.ClientId == HOST_CLIENT_ID);
                }
                else
                {
                    _rows[i].ShowEmpty();
                }
            }
            _playersTitle.text = $"JUGADORES  <color=#F2C55C>{_seated.Count}/{connection.MaxPlayers}</color>";

            _meName.text = local == null ? string.Empty
                : connection.IsHost ? $"{local.DisplayName} <color=#C9CFE0>· Anfitrión</color>" : local.DisplayName;
            _mePortrait.texture = local != null ? _preview.Face(local.CharacterId) : null;
            _mePortrait.enabled = _mePortrait.texture != null;
            _tokens.text = $"{MoneyFormat.Amount(_profile.Tokens)} fichas";

            _invitePanel.SetActive(connection.IsHost);
            if (connection.IsHost)
                _inviteAddress.text = InviteText.Primary(connection.ShareableAddresses) ?? string.Empty;

            _startButton.gameObject.SetActive(connection.IsHost);
            _startButton.interactable = connection.CanStartMatch;
            _waiting.gameObject.SetActive(!connection.IsHost);
            _lobbyHeading.text = !connection.IsHost ? "Esperando al anfitrión"
                : connection.CanStartMatch ? "Todo listo para el concurso"
                : $"Faltan jugadores (mínimo {connection.MinPlayersToStart})";

            // Tu personaje en 3D, a la derecha.
            _lobbyPreview = true;
            _preview.Request(local != null ? local.CharacterId : string.Empty);
        }

        private void StopLobbyPreview()
        {
            if (!_lobbyPreview)
                return;
            _lobbyPreview = false;
            _preview.Request(string.Empty);
        }

        public string PlayerNameText => _playerName.text;

        /// <summary>Guarda el nombre limpio y lo muestra tal como lo verán los demás.</summary>
        private void OnNameEdited(string _) => SaveName();

        /// <summary>«Así te verán los demás · 5/16» mientras escribe.</summary>
        private void OnNameTyped(string text) =>
            _nameHint.text = $"Así te verán los demás · {PlayerNameRules.Sanitize(text).Length}/{PlayerNameRules.MAX_LENGTH}";

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
            _copyLabel.text = "Copiado";
        }

        private void OnStart() => _room.Connection?.StartMatch();

        private void OnLeave() => _pause.AskToLeave();

        private void OnShop() => _openShopRequested.Raise();
    }
}
