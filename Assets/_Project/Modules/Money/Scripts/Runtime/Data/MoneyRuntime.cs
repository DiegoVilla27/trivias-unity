using System;
using Trivias.Money.Logic;
using Trivias.Shared.Interaction;
using UnityEngine;

namespace Trivias.Money.Data
{
    /// <summary>
    /// Anclaje en runtime del sistema de dinero de la sesión actual. El sistema se registra al aparecer en red;
    /// mesa y bandejas lo consultan a través de este asset, sin buscarse en la escena.
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Money/Money Runtime", fileName = "MoneyRuntime")]
    public sealed class MoneyRuntime : ScriptableObject
    {
        /// <summary>Índice del objeto que cambió, o <see cref="ALL"/> si cambió todo (aparición, desaparición).</summary>
        public event Action<int> Changed;

        public const int ALL = -1;

        public IMoneySession Session { get; private set; }

        /// <summary>Solo en el host: control del dinero para el flujo de la partida (bloquear y resolver).</summary>
        public IMoneyAuthority Authority { get; private set; }

        public void Attach(IMoneySession session, IMoneyAuthority authority)
        {
            Session = session;
            Authority = authority;
            Changed?.Invoke(ALL);
        }

        public void Detach(IMoneySession session)
        {
            if (Session != session)
                return;
            Session = null;
            Authority = null;
            Changed?.Invoke(ALL);
        }

        public void NotifyChanged(int index) => Changed?.Invoke(index);

        private void OnEnable()
        {
            Session = null;
            Authority = null;
        }
    }

    /// <summary>Lo que mesa y bandejas necesitan del sistema de dinero en red.</summary>
    public interface IMoneySession
    {
        MoneyLedger Ledger { get; }
        ulong LocalClientId { get; }

        /// <summary>Apuestas cerradas: no se puede coger ni dejar dinero.</summary>
        bool IsLocked { get; }

        void RequestTableInteraction(InteractionAction action);
        void RequestTrayInteraction(int tray, InteractionAction action);
    }

    /// <summary>Control del dinero que solo tiene el host (lo usa el módulo Match).</summary>
    public interface IMoneyAuthority
    {
        long TotalInPlay { get; }

        /// <summary>Dinero que se perdería al verificar ahora por no estar en una bandeja (regla provisional).</summary>
        long UnplacedTotal { get; }

        void SetLocked(bool locked);

        RoundOutcome ResolveRound(int correctTray, int multiplier, bool loseUnplaced);
    }
}
