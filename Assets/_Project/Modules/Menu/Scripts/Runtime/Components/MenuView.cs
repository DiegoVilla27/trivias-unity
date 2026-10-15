using System.Text;
using TMPro;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Shared.Players;
using Trivias.Shared.Rooms;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Menu.Components
{
    /// <summary>
    /// Menú del juego: pantalla principal (crear sala o unirse) y sala de espera (jugadores y empezar).
    /// Desaparece al empezar la partida y vuelve al regresar a la sala. Se redibuja solo por eventos.
    /// </summary>
    public sealed class MenuView : MonoBehaviour
    {
        private const string ERROR_COLOR = "#FF8080";

        [SerializeField] private RoomConnectionRuntime _room;
        [SerializeField] private MatchRuntime _match;
        [SerializeField] private PlayerAvatarRegistry _avatars;

        [Header("Pantalla principal")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private TMP_InputField _address;
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _joinButton;
        [SerializeField] private TextMeshProUGUI _mainStatus;

        [Header("Sala de espera")]
        [SerializeField] private GameObject _lobbyPanel;
        [SerializeField] private TextMeshProUGUI _players;
        [SerializeField] private TextMeshProUGUI _lobbyInfo;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _leaveButton;

        private readonly StringBuilder _builder = new();

        private void OnEnable()
        {
            _room.Changed += Refresh;
            _match.Changed += Refresh;
            _avatars.Changed += OnAvatarChanged;
            _hostButton.onClick.AddListener(OnHost);
            _joinButton.onClick.AddListener(OnJoin);
            _startButton.onClick.AddListener(OnStart);
            _leaveButton.onClick.AddListener(OnLeave);
            Refresh();
        }

        private void OnDisable()
        {
            _room.Changed -= Refresh;
            _match.Changed -= Refresh;
            _avatars.Changed -= OnAvatarChanged;
            _hostButton.onClick.RemoveListener(OnHost);
            _joinButton.onClick.RemoveListener(OnJoin);
            _startButton.onClick.RemoveListener(OnStart);
            _leaveButton.onClick.RemoveListener(OnLeave);
        }

        private void OnAvatarChanged(ulong _) => Refresh();

        private void Refresh()
        {
            var connection = _room.Connection;
            if (connection == null)
            {
                _mainPanel.SetActive(false);
                _lobbyPanel.SetActive(false);
                return;
            }

            if (connection.State == RoomConnectionState.Connected)
            {
                _mainPanel.SetActive(false);
                var inMatch = _match.Session != null && _match.Session.Phase != MatchPhase.Lobby;
                _lobbyPanel.SetActive(!inMatch);
                if (!inMatch)
                    RefreshLobby(connection);
                return;
            }

            _lobbyPanel.SetActive(false);
            _mainPanel.SetActive(true);
            RefreshMain(connection);
        }

        private void RefreshMain(IRoomConnection connection)
        {
            if (string.IsNullOrEmpty(_address.text))
                _address.text = connection.DefaultAddress;

            var connecting = connection.State == RoomConnectionState.Connecting;
            _hostButton.interactable = !connecting;
            _joinButton.interactable = !connecting;
            _mainStatus.text = connecting
                ? connection.StatusMessage
                : string.IsNullOrEmpty(connection.StatusMessage)
                    ? string.Empty
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

            _startButton.gameObject.SetActive(connection.IsHost);
            _startButton.interactable = connection.CanStartMatch;
            _lobbyInfo.text = connection.IsHost
                ? connection.MinPlayersToStart > 1
                    ? $"Se necesitan al menos {connection.MinPlayersToStart} jugadores."
                    : "Modo prueba: se puede empezar solo."
                : "Esperando a que el host empiece la partida…";
        }

        private void OnHost() => _room.Connection?.Host();

        private void OnJoin() => _room.Connection?.Join(_address.text);

        private void OnStart() => _room.Connection?.StartMatch();

        private void OnLeave() => _room.Connection?.Leave();
    }
}
