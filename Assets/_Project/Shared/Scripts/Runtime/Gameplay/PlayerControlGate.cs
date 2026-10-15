using System;
using UnityEngine;

namespace Trivias.Shared.Gameplay
{
    /// <summary>
    /// Indica si los concursantes pueden moverse. La partida lo cierra mientras se espera en la sala
    /// y lo abre al empezar; el concursante lo consulta sin conocer el módulo de partida.
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Gameplay/Player Control Gate", fileName = "PlayerControlGate")]
    public sealed class PlayerControlGate : ScriptableObject
    {
        public event Action<bool> Changed;

        public bool CanMove { get; private set; }

        public void SetCanMove(bool canMove)
        {
            if (CanMove == canMove)
                return;
            CanMove = canMove;
            Changed?.Invoke(canMove);
        }

        private void OnEnable() => CanMove = false;
    }
}
