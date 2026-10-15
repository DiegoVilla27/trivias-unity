using Trivias.Money.Data;
using Trivias.Money.Logic;
using Trivias.Shared.Interaction;

namespace Trivias.Money.Components
{
    /// <summary>
    /// Mesa central. Con las manos vacías: R elige el tipo a coger (fajos, bolsas o lingotes de los que haya),
    /// E coge uno y Q coge hasta el límite de ese tipo. Llevando dinero: E/Q cogen más del mismo tipo y R lo devuelve todo.
    /// El tipo elegido es estado local de cada jugador (no viaja por red hasta que coge).
    /// </summary>
    public sealed class MoneyTableTarget : MoneyTargetBase
    {
        private Denomination _selected = Denomination.Bundle;

        public override float Radius => _config.TableReach;

        protected override void SendRequest(IMoneySession session, InteractionAction action)
        {
            var carrying = session.Ledger.FindCarriedBy(session.LocalClientId) >= 0;
            if (action == InteractionAction.Tertiary && !carrying)
            {
                SelectNextAvailable(session.Ledger);
                Recalculate();
                return;
            }

            session.RequestTableInteraction(action, _selected);
        }

        protected override void Describe(IMoneySession session, out string prompt, out string totalLabel)
        {
            var ledger = session.Ledger;
            var client = session.LocalClientId;
            totalLabel = $"MESA\n{MoneyFormat.Amount(ledger.TotalAt(MoneyPlace.Table, 0))}";

            var carriedTop = ledger.FindCarriedBy(client);
            if (carriedTop < 0)
                EnsureSelectionAvailable(ledger);

            var type = carriedTop >= 0 ? ledger.Items[carriedTop].Denomination : _selected;
            var pickable = ledger.CountPickableFromTable(client, type);

            prompt = string.Empty;
            if (pickable > 0)
            {
                prompt = $"E: coger {MoneyFormat.Name(type)} ({MoneyFormat.Amount(ledger.ValueOfDenomination(type))})";
                if (pickable > 1)
                    prompt += $"{SEPARATOR}Q: coger {pickable}";
            }

            if (carriedTop >= 0)
            {
                var giveBack = $"R: devolver {ledger.CountCarriedBy(client)} ({MoneyFormat.Amount(ledger.CarriedValue(client))})";
                prompt = prompt.Length > 0 ? $"{prompt}{SEPARATOR}{giveBack}" : giveBack;
                return;
            }

            var next = NextAvailable(ledger, _selected);
            if (next != _selected)
                prompt += $"{(prompt.Length > 0 ? SEPARATOR : string.Empty)}R: cambiar a {Plural(next)}";
        }

        private void EnsureSelectionAvailable(MoneyLedger ledger)
        {
            if (ledger.CountAt(MoneyPlace.Table, 0, _selected) == 0)
                _selected = NextAvailable(ledger, _selected);
        }

        private void SelectNextAvailable(MoneyLedger ledger) => _selected = NextAvailable(ledger, _selected);

        /// <summary>Siguiente denominación con objetos en la mesa (en orden fajo → bolsa → lingote), o la actual.</summary>
        private static Denomination NextAvailable(MoneyLedger ledger, Denomination current)
        {
            for (var step = 1; step <= 3; step++)
            {
                var candidate = (Denomination)(((int)current + step) % 3);
                if (ledger.CountAt(MoneyPlace.Table, 0, candidate) > 0)
                    return candidate;
            }
            return current;
        }

        private static string Plural(Denomination denomination) => denomination switch
        {
            Denomination.Bundle => "fajos",
            Denomination.Bag => "bolsas",
            _ => "lingotes",
        };
    }
}
