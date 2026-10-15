using Trivias.Money.Data;
using Trivias.Money.Logic;
using Trivias.Shared.Interaction;
using UnityEngine;

namespace Trivias.Money.Components
{
    /// <summary>
    /// Bandeja de una cabina. Con dinero en las manos: E deja uno, Q deja todo.
    /// Con las manos vacías: E recupera el último depositado, Q recupera los de arriba del mismo tipo.
    /// </summary>
    public sealed class MoneyTrayTarget : MoneyTargetBase
    {
        [SerializeField, Range(0, 3)] private int _trayIndex;

        public override float Radius => _config.TrayReach;

        private char Letter => (char)('A' + _trayIndex);

        protected override void SendRequest(IMoneySession session, InteractionAction action) =>
            session.RequestTrayInteraction(_trayIndex, action);

        protected override void Describe(IMoneySession session, out string prompt, out string totalLabel)
        {
            var ledger = session.Ledger;
            var client = session.LocalClientId;
            totalLabel = MoneyFormat.Amount(ledger.TotalAt(MoneyPlace.Tray, _trayIndex));

            var carried = ledger.CountCarriedBy(client);
            if (carried > 0)
            {
                var top = ledger.FindCarriedBy(client);
                prompt = PromptText.Key("E", $"Dejar {MoneyFormat.Name(ledger.Items[top].Denomination)} en {Letter}");
                if (carried > 1)
                    prompt = PromptText.Join(prompt, PromptText.Key("Q", $"Dejar todo ({MoneyFormat.Amount(ledger.CarriedValue(client))})"));
                return;
            }

            var inTray = ledger.CountAt(MoneyPlace.Tray, _trayIndex);
            if (inTray == 0)
            {
                prompt = string.Empty;
                return;
            }

            var last = ledger.FindTopAt(MoneyPlace.Tray, _trayIndex, preferSmallest: false);
            prompt = PromptText.Key("E", $"Recuperar {MoneyFormat.Name(ledger.Items[last].Denomination)}");
            var batch = ledger.CountPickable(client, MoneyPlace.Tray, _trayIndex, preferSmallest: false);
            if (batch > 1)
                prompt = PromptText.Join(prompt, PromptText.Key("Q", $"Recuperar {batch}"));
        }
    }
}
