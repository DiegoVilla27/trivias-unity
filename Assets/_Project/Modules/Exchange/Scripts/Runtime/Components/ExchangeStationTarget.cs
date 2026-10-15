using Trivias.Money.Data;
using Trivias.Money.Logic;
using Trivias.Shared.Interaction;
using UnityEngine;

namespace Trivias.Exchange.Components
{
    /// <summary>
    /// Estación de cambio manual (GDD 4.2). Con las manos vacías agrupa el dinero de la mesa (E: 10 fajos → 1 bolsa,
    /// Q: 10 bolsas → 1 lingote). Llevando bolsas o lingotes los divide (E: uno, Q: todos) y el resultado va a la mesa.
    /// El aviso se recalcula solo cuando cambia el dinero.
    /// </summary>
    public sealed class ExchangeStationTarget : MonoBehaviour, IInteractable
    {
        [SerializeField] private MoneyConfig _config;
        [SerializeField] private MoneyRuntime _runtime;
        [SerializeField] private InteractableRegistry _registry;

        private string _prompt = string.Empty;

        public Vector3 Position => transform.position;
        public float Radius => _config.ExchangeReach;

        private void OnEnable()
        {
            _registry.Register(this);
            _runtime.Changed += OnMoneyChanged;
            Recalculate();
        }

        private void OnDisable()
        {
            _registry.Unregister(this);
            _runtime.Changed -= OnMoneyChanged;
        }

        public bool TryGetPrompt(in InteractionContext context, out string prompt)
        {
            prompt = _prompt;
            return prompt.Length > 0;
        }

        public void Interact(in InteractionContext context) => _runtime.Session?.RequestExchange(context.Action);

        private void OnMoneyChanged(int _) => Recalculate();

        private void Recalculate()
        {
            var session = _runtime.Session;
            if (session == null || session.IsLocked)
            {
                _prompt = string.Empty;
                return;
            }

            var ledger = session.Ledger;
            var client = session.LocalClientId;
            var carried = ledger.FindCarriedBy(client);

            if (carried >= 0)
            {
                _prompt = DescribeSplit(ledger, client, ledger.Items[carried].Denomination);
                return;
            }

            _prompt = DescribeGroup(ledger);
        }

        private static string DescribeSplit(MoneyLedger ledger, ulong client, Denomination carried)
        {
            if (carried == Denomination.Bundle)
                return "Los fajos no se dividen: déjalos en la mesa para agruparlos";

            var into = carried - 1;
            var ratio = ledger.ExchangeRatio(carried);
            var prompt = PromptText.Key("E", $"Dividir {MoneyFormat.Name(carried)} → {ratio} {Plural(into)}");
            var count = ledger.CountCarriedBy(client);
            if (count > 1)
                prompt = PromptText.Join(prompt, PromptText.Key("Q", $"Dividir las {count}"));
            return prompt;
        }

        private static string DescribeGroup(MoneyLedger ledger)
        {
            var bagRatio = ledger.ExchangeRatio(Denomination.Bag);
            var barRatio = ledger.ExchangeRatio(Denomination.GoldBar);
            var canGroupBundles = ledger.CountAt(MoneyPlace.Table, 0, Denomination.Bundle) >= bagRatio;
            var canGroupBags = ledger.CountAt(MoneyPlace.Table, 0, Denomination.Bag) >= barRatio;

            if (!canGroupBundles && !canGroupBags)
                return $"Agrupar: hacen falta {bagRatio} fajos o {barRatio} bolsas en la mesa";

            var prompt = canGroupBundles ? PromptText.Key("E", $"Agrupar {bagRatio} fajos → bolsa") : string.Empty;
            if (canGroupBags)
                prompt = PromptText.Join(prompt, PromptText.Key("Q", $"Agrupar {barRatio} bolsas → lingote"));
            return prompt;
        }

        private static string Plural(Denomination denomination) => denomination switch
        {
            Denomination.Bundle => "fajos",
            Denomination.Bag => "bolsas",
            _ => "lingotes",
        };
    }
}
