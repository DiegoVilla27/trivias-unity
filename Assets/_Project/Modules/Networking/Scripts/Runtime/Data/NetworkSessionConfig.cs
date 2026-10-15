using UnityEngine;

namespace Trivias.Networking.Data
{
    /// <summary>Reglas de la sala (GDD: de 2 a 6 jugadores) y parámetros de conexión.</summary>
    [CreateAssetMenu(menuName = "Trivias/Networking/Network Session Config", fileName = "NetworkSessionConfig")]
    public sealed class NetworkSessionConfig : ScriptableObject
    {
        [Header("Sala (GDD)")]
        [SerializeField, Range(2, 6)] private int _minPlayers = 2;
        [SerializeField, Range(2, 6)] private int _maxPlayers = 6;

        [Header("Solo editor y builds de depuración")]
        [Tooltip("Permite empezar la partida con un único jugador para probar. Nunca se aplica en builds de release.")]
        [SerializeField] private bool _allowSoloForTesting = true;

        [Header("Conexión")]
        [SerializeField] private string _defaultAddress = "127.0.0.1";
        [SerializeField] private ushort _port = 7777;
        [Tooltip("Segundos buscando la sala antes de avisar de que no existe.")]
        [SerializeField, Range(1, 30)] private int _connectTimeoutSeconds = 5;
        [Tooltip("Segundos sin noticias de un jugador antes de darlo por desconectado (p. ej. si se cierra el juego de golpe).")]
        [SerializeField, Range(2, 30)] private int _disconnectTimeoutSeconds = 5;

        public int MaxPlayers => _maxPlayers;
        public string DefaultAddress => _defaultAddress;
        public ushort Port => _port;
        public int ConnectTimeoutSeconds => _connectTimeoutSeconds;
        public int DisconnectTimeoutSeconds => _disconnectTimeoutSeconds;

        /// <summary>Mínimo efectivo para empezar: 1 solo en editor/development si está permitido; si no, el del GDD.</summary>
        public int MinPlayersToStart
        {
            get
            {
#if UNITY_EDITOR || DEBUG
                if (_allowSoloForTesting)
                    return 1;
#endif
                return _minPlayers;
            }
        }

        private void OnValidate()
        {
            if (_minPlayers > _maxPlayers)
                _minPlayers = _maxPlayers;
        }
    }
}
