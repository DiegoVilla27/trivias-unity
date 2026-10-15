using Trivias.Money.Data;
using Trivias.Money.Logic;
using Trivias.Shared.Interaction;

namespace Trivias.Money.Components
{
    /// <summary>
    /// Mesa central: E coge uno, Q coge hasta el límite de ese tipo, R devuelve todo lo que se lleva.
    /// Si ya se lleva dinero, solo se puede coger más de la misma denominación.
    /// </summary>
    public sealed class MoneyTableTarget : MoneyTargetBase
    {
        public override float Radius => _config.TableReach;

        protected override void SendRequest(IMoneySession session, InteractionAction action) =>
            session.RequestTableInteraction(action);

        protected override void Describe(IMoneySession session, out string prompt, out string totalLabel)
        {
            var ledger = session.Ledger;
            var client = session.LocalClientId;
            totalLabel = $"MESA\n{MoneyFormat.Amount(ledger.TotalAt(MoneyPlace.Table, 0))}";

            prompt = string.Empty;
            var pickable = ledger.CountPickable(client, MoneyPlace.Table, 0, preferSmallest: true);
            if (pickable > 0)
            {
                var next = NextToPick(ledger, client);
                prompt = $"E: coger {MoneyFormat.Name(ledger.Items[next].Denomination)} ({MoneyFormat.Amount(ledger.ValueOf(next))})";
                if (pickable > 1)
                    prompt += $"{SEPARATOR}Q: coger {pickable}";
            }

            var carried = ledger.CountCarriedBy(client);
            if (carried > 0)
            {
                var giveBack = $"R: devolver {carried} ({MoneyFormat.Amount(ledger.CarriedValue(client))})";
                prompt = prompt.Length > 0 ? $"{prompt}{SEPARATOR}{giveBack}" : giveBack;
            }
        }

        /// <summary>Lo que cogería E: la denominación que ya se lleva o, con las manos vacías, la más pequeña.</summary>
        private static int NextToPick(MoneyLedger ledger, ulong client)
        {
            var carried = ledger.FindCarriedBy(client);
            if (carried < 0)
                return ledger.FindTopAt(MoneyPlace.Table, 0, preferSmallest: true);

            var denomination = ledger.Items[carried].Denomination;
            for (var i = 0; i < ledger.Count; i++)
            {
                if (ledger.Items[i].IsAt(MoneyPlace.Table, 0) && ledger.Items[i].Denomination == denomination)
                    return i;
            }
            return -1;
        }
    }
}
