using System;
using UnityEngine;

namespace Trivias.Shared.Gameplay
{
    /// <summary>
    /// Indica si los concursantes pueden moverse. La partida lo cierra mientras se espera en la sala
    /// y lo abre al empezar; el menú de pausa (Esc) lo cierra mientras está abierto. El concursante y la cámara
    /// lo consultan sin conocer esos módulos.
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Gameplay/Player Control Gate", fileName = "PlayerControlGate")]
    public sealed class PlayerControlGate : ScriptableObject
    {
        public event Action<bool> Changed;

        private bool _matchAllows;
        private bool _paused;

        public bool CanMove => _matchAllows && !_paused;

        /// <summary>La partida permite moverse (fases de juego) o no (sala, resultados).</summary>
        public bool MatchAllowsMovement => _matchAllows;

        public bool IsPaused => _paused;

        public void SetCanMove(bool canMove) => Set(canMove, _paused);

        /// <summary>Menú de pausa abierto: el concursante se queda quieto y el ratón libre.</summary>
        public void SetPaused(bool paused) => Set(_matchAllows, paused);

        private void Set(bool matchAllows, bool paused)
        {
            var before = CanMove;
            _matchAllows = matchAllows;
            _paused = paused;
            if (CanMove != before)
                Changed?.Invoke(CanMove);
        }

        private void OnEnable()
        {
            _matchAllows = false;
            _paused = false;
        }
    }
}
