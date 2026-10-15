using Trivias.Networking.Data;
using Trivias.Networking.Logic;
using Trivias.Shared.Diagnostics;
using Trivias.Shared.Events;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Trivias.Networking.Components
{
    /// <summary>
    /// Punto único para crear o unirse a una sala. El transporte concreto (local ahora, Steam más adelante)
    /// queda oculto detrás de este servicio. Expone el estado de la conexión para la interfaz.
    /// </summary>
    [RequireComponent(typeof(NetworkManager))]
    public sealed class ConnectionService : MonoBehaviour
    {
        private const int CONNECT_ATTEMPT_MS = 1000;

        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private UnityTransport _transport;
        [SerializeField] private NetworkSessionConfig _config;
        [Tooltip("Objetos de red de la sesión que crea el host al abrir la sala (dinero, partida...). " +
                 "Networking no conoce sus tipos: los asigna el constructor de la escena.")]
        [SerializeField] private NetworkObject[] _sessionPrefabs = new NetworkObject[0];
        [Tooltip("Se lanza en el host al empezar la partida; la escucha el módulo Match.")]
        [SerializeField] private VoidEventChannel _matchStartRequested;
        [Tooltip("Lo lanza la partida al volver a la sala de espera: la sala se reabre a nuevos jugadores.")]
        [SerializeField] private VoidEventChannel _returnedToLobby;
        [Tooltip("Petición local de salir de la sala (pantalla de resultados).")]
        [SerializeField] private VoidEventChannel _leaveRoomRequested;

        private string _targetAddress;

        public NetworkSessionConfig Config => _config;
        public ConnectionState State { get; private set; } = ConnectionState.Disconnected;
        public string StatusMessage { get; private set; } = string.Empty;
        public bool IsHost => _networkManager.IsHost;
        public int ConnectedPlayers => _networkManager.IsServer ? _networkManager.ConnectedClientsIds.Count : 0;

        /// <summary>Solo en el host. Al empezar, la sala se cierra a nuevos jugadores (GDD: sin entradas a mitad de partida).</summary>
        public bool MatchStarted { get; private set; }

        public bool CanStartMatch => _networkManager.IsHost &&
            RoomAdmission.CanStartMatch(ConnectedPlayers, _config.MinPlayersToStart, _config.MaxPlayers, MatchStarted);

        private void Awake()
        {
            _networkManager.ConnectionApprovalCallback = ApproveConnection;
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
            _returnedToLobby.Register(OnReturnedToLobby);
            _leaveRoomRequested.Register(Leave);
        }

        private void OnDestroy()
        {
            if (_networkManager == null)
                return;

            _networkManager.ConnectionApprovalCallback = null;
            _networkManager.OnClientConnectedCallback -= OnClientConnected;
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            _returnedToLobby.Unregister(OnReturnedToLobby);
            _leaveRoomRequested.Unregister(Leave);
        }

        private void OnReturnedToLobby()
        {
            if (!_networkManager.IsHost)
                return;
            MatchStarted = false;
            SetState(ConnectionState.Connected, "De vuelta en la sala. La sala vuelve a admitir jugadores.");
        }

        public bool StartHost()
        {
            MatchStarted = false;
            ConfigureTimeouts();
            _transport.SetConnectionData(_config.DefaultAddress, _config.Port, "0.0.0.0");
            var started = _networkManager.StartHost();
            if (started)
                SpawnSessionObjects();
            SetState(started ? ConnectionState.Connected : ConnectionState.Failed,
                started
                    ? "Sala creada. Los demás jugadores deben pulsar «Unirse»."
                    : $"No se pudo crear la sala. ¿Hay otra sala abierta en el puerto {_config.Port}?");
            return started;
        }

        public bool StartClient(string address)
        {
            _targetAddress = address;
            ConfigureTimeouts();
            _transport.SetConnectionData(address, _config.Port);

            var started = _networkManager.StartClient();
            SetState(started ? ConnectionState.Connecting : ConnectionState.Failed,
                started ? $"Buscando la sala en {address}..." : "No se pudo iniciar la conexión.");
            return started;
        }

        public void Leave()
        {
            _networkManager.Shutdown();
            MatchStarted = false;
            SetState(ConnectionState.Disconnected, string.Empty);
        }

        /// <summary>Host: empieza la partida si se cumple el mínimo de jugadores y cierra la sala a nuevas entradas.</summary>
        public bool TryStartMatch()
        {
            if (!CanStartMatch)
                return false;

            MatchStarted = true;
            _matchStartRequested.Raise();
            SetState(ConnectionState.Connected, $"Partida empezada con {ConnectedPlayers} jugador(es). La sala está cerrada.");
            return true;
        }

        // Cierre ordenado: avisa a los demás al salir del juego en vez de esperar al tiempo de desconexión.
        private void OnApplicationQuit()
        {
            if (_networkManager != null && _networkManager.IsListening)
                _networkManager.Shutdown();
        }

        // Se ejecuta una vez al abrir la sala (tiempo de carga), nunca durante la partida.
        private void SpawnSessionObjects()
        {
            foreach (var prefab in _sessionPrefabs)
                Instantiate(prefab).Spawn(destroyWithScene: true);
        }

        private void ConfigureTimeouts()
        {
            _transport.ConnectTimeoutMS = CONNECT_ATTEMPT_MS;
            _transport.MaxConnectAttempts = _config.ConnectTimeoutSeconds;
            _transport.DisconnectTimeoutMS = _config.DisconnectTimeoutSeconds * 1000;
        }

        private void OnClientConnected(ulong clientId)
        {
            if (clientId == _networkManager.LocalClientId && !_networkManager.IsHost)
                SetState(ConnectionState.Connected, "Conectado a la sala.");
        }

        private void OnClientDisconnected(ulong clientId)
        {
            // En el host este evento también llega cuando se va otro jugador: solo nos importa la conexión propia.
            if (_networkManager.IsServer && clientId != NetworkManager.ServerClientId)
                return;

            var reason = _networkManager.DisconnectReason;
            if (State == ConnectionState.Connecting)
            {
                SetState(ConnectionState.Failed, string.IsNullOrEmpty(reason)
                    ? $"No hay ninguna sala en {_targetAddress}. Primero alguien debe pulsar «Crear sala»."
                    : reason);
            }
            else if (State == ConnectionState.Connected)
            {
                // El motivo que da NGO al cerrarse la sala es técnico; al jugador le basta con saber qué pasó.
                SetState(ConnectionState.Disconnected, _networkManager.IsServer
                    ? string.Empty
                    : "Se cerró la sala o se perdió la conexión con el host.");
            }
        }

        private void SetState(ConnectionState state, string message)
        {
            State = state;
            StatusMessage = message;
            GameLog.Info($"[Red] {state}: {message}", this);
        }

        private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            var rejection = RoomAdmission.RejectionReason(_networkManager.ConnectedClientsIds.Count,
                _config.MaxPlayers, MatchStarted);
            response.Approved = rejection == null;
            response.CreatePlayerObject = rejection == null;
            response.Reason = rejection ?? string.Empty;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _networkManager = GetComponent<NetworkManager>();
            _transport = GetComponent<UnityTransport>();
        }
#endif
    }
}
