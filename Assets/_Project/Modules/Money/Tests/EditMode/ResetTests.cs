using System.Collections.Generic;
using NUnit.Framework;
using Trivias.Money.Logic;

namespace Trivias.Money.Tests
{
    public sealed class ResetTests
    {
        [Test]
        public void ResetToStart_RestoresCapitalInBundlesOnTable()
        {
            var changed = new List<int>();
            var ledger = new MoneyLedger(new DenominationValues(5_000, 50_000, 500_000), new CarryLimits(5, 2, 5));
            for (var i = 0; i < 20; i++)
                ledger.AddToTable(Denomination.Bundle);
            ledger.PickFromTable(0, 3, changed);
            ledger.EmitToTable(600_000, changed);

            changed.Clear();
            ledger.ResetToStart(20, changed);

            Assert.That(ledger.TotalInPlay(), Is.EqualTo(100_000));
            Assert.That(ledger.TotalAt(MoneyPlace.Table, 0), Is.EqualTo(100_000));
            Assert.That(ledger.CountAt(MoneyPlace.Table, 0, Denomination.Bundle), Is.EqualTo(20));
            Assert.That(ledger.CountCarriedBy(0), Is.Zero);
        }
    }
}
