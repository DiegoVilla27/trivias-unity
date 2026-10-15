using Trivias.Networking.Logic;
using UnityEngine;

namespace Trivias.Networking.Components
{
    /// <summary>
    /// Panel provisional para crear o unirse a una sala en local. Solo existe en editor y development builds;
    /// se sustituirá por el vestíbulo real. En builds de release el componente se elimina al arrancar.
    /// </summary>
    public sealed class DebugConnectionPanel : MonoBehaviour
    {
        [SerializeField] private ConnectionService _connection;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const float WIDTH = 320f;

        private string _address;
        private GUIStyle _wrapStyle;
        private GUIStyle _richStyle;

        private void Awake() => _address = _connection.Config.DefaultAddress;

        private void OnGUI()
        {
            _wrapStyle ??= new GUIStyle(GUI.skin.label) { wordWrap = true };
            _richStyle ??= new GUIStyle(_wrapStyle) { richText = true };

            GUILayout.BeginArea(new Rect(16f, 16f, WIDTH, 360f), GUI.skin.box);
            GUILayout.Label("<b>Trivias — red local</b>", _richStyle);

            switch (_connection.State)
            {
                case ConnectionState.Connected:
                    DrawConnected();
                    break;
                case ConnectionState.Connecting:
                    GUILayout.Label(_connection.StatusMessage, _wrapStyle);
                    if (GUILayout.Button("Cancelar"))
                        _connection.Leave();
                    break;
                default:
                    DrawMenu();
                    break;
            }

            GUILayout.EndArea();
        }

        private void DrawMenu()
        {
            if (!string.IsNullOrEmpty(_connection.StatusMessage))
                GUILayout.Label($"<color=#ff8080>{_connection.StatusMessage}</color>", _richStyle);

            GUILayout.Label("1. Un jugador crea la sala.\n2. Los demás se unen con la dirección del host (127.0.0.1 = este ordenador).", _wrapStyle);

            if (GUILayout.Button("Crear sala (host)", GUILayout.Height(32f)))
                _connection.StartHost();

            GUILayout.Space(8f);
            GUILayout.Label("Dirección del host");
            _address = GUILayout.TextField(_address);
            if (GUILayout.Button("Unirse", GUILayout.Height(32f)))
                _connection.StartClient(_address);
        }

        private void DrawConnected()
        {
            GUILayout.Label(_connection.IsHost
                ? $"Eres el host · jugadores: {_connection.ConnectedPlayers}/{_connection.Config.MaxPlayers}"
                : "Conectado como jugador", _wrapStyle);

            if (_connection.IsHost)
                DrawHostMatchControls();

            GUILayout.Label("Clic: capturar ratón · WASD: moverse · Ratón: mirar · Esc: soltar ratón", _wrapStyle);
            if (GUILayout.Button("Salir de la sala"))
                _connection.Leave();
        }

        private void DrawHostMatchControls()
        {
            if (_connection.MatchStarted)
            {
                GUILayout.Label("Partida en curso · sala cerrada a nuevos jugadores.", _wrapStyle);
                return;
            }

            var min = _connection.Config.MinPlayersToStart;
            GUILayout.Label(min == 1
                ? "Modo prueba: se puede empezar con 1 jugador (en la versión final, mínimo 2)."
                : $"Se necesitan al menos {min} jugadores para empezar.", _wrapStyle);

            GUI.enabled = _connection.CanStartMatch;
            if (GUILayout.Button("Empezar partida", GUILayout.Height(28f)))
                _connection.TryStartMatch();
            GUI.enabled = true;
        }
#else
        private void Awake() => Destroy(this);
#endif
    }
}
