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

        public void Attach(IMoneySession session)
        {
            Session = session;
            Changed?.Invoke(ALL);
        }

        public void Detach(IMoneySession session)
        {
            if (Session != session)
                return;
            Session = null;
            Changed?.Invoke(ALL);
        }

        public void NotifyChanged(int index) => Changed?.Invoke(index);

        private void OnEnable() => Session = null;
    }

    /// <summary>Lo que mesa y bandejas necesitan del sistema de dinero en red.</summary>
    public interface IMoneySession
    {
        MoneyLedger Ledger { get; }
        ulong LocalClientId { get; }
        void RequestTableInteraction(InteractionAction action);
        void RequestTrayInteraction(int tray, InteractionAction action);
    }
}
