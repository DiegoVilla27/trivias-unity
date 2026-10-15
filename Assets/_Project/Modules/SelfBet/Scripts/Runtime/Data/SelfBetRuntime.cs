using System;
using System.Collections.Generic;
using Trivias.SelfBet.Logic;
using UnityEngine;

namespace Trivias.SelfBet.Data
{
    /// <summary>Anclaje en runtime de los comodines de la sesión. La interfaz y la partida lo consultan aquí.</summary>
    [CreateAssetMenu(menuName = "Trivias/SelfBet/Self Bet Runtime", fileName = "SelfBetRuntime")]
    public sealed class SelfBetRuntime : ScriptableObject
    {
        public event Action Changed;

        public ISelfBetSession Session { get; private set; }

        /// <summary>Solo en el host.</summary>
        public ISelfBetAuthority Authority { get; private set; }

        public void Attach(ISelfBetSession session, ISelfBetAuthority authority)
        {
            Session = session;
            Authority = authority;
            Changed?.Invoke();
        }

        public void Detach(ISelfBetSession session)
        {
            if (Session != session)
                return;
            Session = null;
            Authority = null;
            Changed?.Invoke();
        }

        public void NotifyChanged() => Changed?.Invoke();

        private void OnEnable()
        {
            Session = null;
            Authority = null;
        }
    }

    public interface ISelfBetSession
    {
        SelfBetBook Book { get; }
        ulong LocalClientId { get; }

        /// <summary>Se puede apostar ahora (durante las apuestas de una pregunta).</summary>
        bool IsOpen { get; }

        /// <summary>Apostarse en una cabina, cambiar a ella o cancelar si ya está apostado en ella.</summary>
        void RequestToggle(int booth);
    }

    /// <summary>Control de los comodines que solo tiene el host (lo usa el módulo Match).</summary>
    public interface ISelfBetAuthority
    {
        int ActivePlayers { get; }

        /// <summary>El concursante tiene una apuesta preparada o fija.</summary>
        bool HasBet(ulong client);

        bool AllActiveHaveBet { get; }
        void StartMatch(IReadOnlyList<ulong> clients);
        void SetOpen(bool open);
        void LockBets();

        /// <summary>Resuelve: devuelve el multiplicador y elimina a los apostados en cabinas incorrectas.</summary>
        int Resolve(int correctBooth, List<ulong> eliminated);

        /// <summary>Sala de espera: sin apuestas ni reparto hasta la próxima partida.</summary>
        void ResetForNewMatch();
    }
}
