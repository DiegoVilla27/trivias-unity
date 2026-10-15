using System.Collections.Generic;
using NUnit.Framework;
using Trivias.Money.Logic;

namespace Trivias.Money.Tests
{
    public sealed class ExchangeTests
    {
        private const ulong ALICE = 0;
        private MoneyLedger _ledger;
        private readonly List<int> _changed = new();

        [SetUp]
        public void TwentyBundles()
        {
            _ledger = new MoneyLedger(new DenominationValues(5_000, 50_000, 500_000), new CarryLimits(5, 2, 5));
            for (var i = 0; i < 20; i++)
                _ledger.AddToTable(Denomination.Bundle);
            _changed.Clear();
        }

        [Test]
        public void GroupBundles_MakesOneBag_KeepingValue()
        {
            Assert.That(_ledger.GroupOnTable(Denomination.Bundle, _changed), Is.True);
            Assert.That(_ledger.CountAt(MoneyPlace.Table, 0, Denomination.Bundle), Is.EqualTo(10));
            Assert.That(_ledger.CountAt(MoneyPlace.Table, 0, Denomination.Bag), Is.EqualTo(1));
            Assert.That(_ledger.TotalInPlay(), Is.EqualTo(100_000));
        }

        [Test]
        public void GroupBundles_FailsWithFewerThanTen()
        {
            _ledger.PickFromTable(ALICE, 5, _changed);
            _ledger.DepositToTray(ALICE, 0, 5, _changed);
            _ledger.PickFromTable(ALICE, 5, _changed);
            _ledger.DepositToTray(ALICE, 0, 5, _changed);
            _ledger.PickFromTable(ALICE, 1, _changed);
            _changed.Clear();
            Assert.That(_ledger.GroupOnTable(Denomination.Bundle, _changed), Is.False);
            Assert.That(_changed, Is.Empty, "Si no se puede, no se toca nada.");
        }

        [Test]
        public void GroupBags_MakesGoldBar()
        {
            var ledger = new MoneyLedger(new DenominationValues(5_000, 50_000, 500_000), new CarryLimits(5, 2, 5));
            for (var i = 0; i < 10; i++)
                ledger.AddToTable(Denomination.Bag);
            Assert.That(ledger.GroupOnTable(Denomination.Bag, _changed), Is.True);
            Assert.That(ledger.CountAt(MoneyPlace.Table, 0, Denomination.GoldBar), Is.EqualTo(1));
            Assert.That(ledger.TotalInPlay(), Is.EqualTo(500_000));
        }

        [Test]
        public void SplitCarriedBag_PutsTenBundlesOnTable()
        {
            var bagLedger = new MoneyLedger(new DenominationValues(5_000, 50_000, 500_000), new CarryLimits(5, 2, 5));
            bagLedger.AddToTable(Denomination.Bag);
            bagLedger.PickFromTable(ALICE, 1, _changed);

            Assert.That(bagLedger.SplitCarried(ALICE, 1, _changed), Is.EqualTo(1));
            Assert.That(bagLedger.CountCarriedBy(ALICE), Is.Zero);
            Assert.That(bagLedger.CountAt(MoneyPlace.Table, 0, Denomination.Bundle), Is.EqualTo(10));
            Assert.That(bagLedger.TotalInPlay(), Is.EqualTo(50_000));
        }

        [Test]
        public void SplitCarriedGoldBars_AllAtOnce()
        {
            var ledger = new MoneyLedger(new DenominationValues(5_000, 50_000, 500_000), new CarryLimits(5, 2, 5));
            ledger.AddToTable(Denomination.GoldBar);
            ledger.AddToTable(Denomination.GoldBar);
            ledger.PickFromTable(ALICE, 2, _changed);

            Assert.That(ledger.SplitCarried(ALICE, int.MaxValue, _changed), Is.EqualTo(2));
            Assert.That(ledger.CountAt(MoneyPlace.Table, 0, Denomination.Bag), Is.EqualTo(20));
            Assert.That(ledger.TotalInPlay(), Is.EqualTo(1_000_000));
        }

        [Test]
        public void Bundles_CannotBeSplit()
        {
            _ledger.PickFromTable(ALICE, 2, _changed);
            Assert.That(_ledger.SplitCarried(ALICE, 1, _changed), Is.Zero);
            Assert.That(_ledger.CountCarriedBy(ALICE), Is.EqualTo(2));
        }

        [Test]
        public void Grouping_ReusesRetiredSlots()
        {
            var count = _ledger.Count;
            _ledger.GroupOnTable(Denomination.Bundle, _changed);
            Assert.That(_ledger.Count, Is.EqualTo(count), "La bolsa ocupa el índice de un fajo retirado.");
        }

        [Test]
        public void Values_MustBeExactMultiples() =>
            Assert.Throws<System.ArgumentException>(() => new DenominationValues(5_000, 52_000, 520_000));
    }
}
