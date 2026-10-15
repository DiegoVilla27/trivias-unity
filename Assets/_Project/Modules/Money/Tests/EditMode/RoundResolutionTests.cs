using System.Collections.Generic;
using NUnit.Framework;
using Trivias.Money.Logic;

namespace Trivias.Money.Tests
{
    public sealed class RoundResolutionTests
    {
        private const ulong ALICE = 0;
        private const int CORRECT = 1;
        private const int WRONG = 2;

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

        private void Bet(int tray, int bundles)
        {
            _ledger.PickFromTable(ALICE, bundles, _changed);
            _ledger.DepositToTray(ALICE, tray, bundles, _changed);
        }

        [Test]
        public void WrongTray_IsLost_CorrectTray_ReturnsToTable()
        {
            Bet(CORRECT, 5);
            Bet(WRONG, 5);
            var outcome = _ledger.ResolveRound(CORRECT, 1, loseUnplaced: false, _changed);

            Assert.That(outcome.Kept, Is.EqualTo(25_000));
            Assert.That(outcome.Lost, Is.EqualTo(25_000));
            Assert.That(outcome.Total, Is.EqualTo(75_000));
            Assert.That(_ledger.TotalAt(MoneyPlace.Table, 0), Is.EqualTo(75_000));
            Assert.That(_ledger.TotalAt(MoneyPlace.Tray, CORRECT), Is.Zero, "Las bandejas quedan vacías.");
        }

        [Test]
        public void ProvisionalRule_LosesMoneyOutsideTrays()
        {
            Bet(CORRECT, 5);
            var outcome = _ledger.ResolveRound(CORRECT, 1, loseUnplaced: true, _changed);
            Assert.That(outcome.Lost, Is.EqualTo(75_000), "Los 15 fajos de la mesa se pierden.");
            Assert.That(outcome.Total, Is.EqualTo(25_000));
        }

        [Test]
        public void ProvisionalRule_LosesMoneyInHands()
        {
            _ledger.PickFromTable(ALICE, 2, _changed);
            var outcome = _ledger.ResolveRound(CORRECT, 1, loseUnplaced: true, _changed);
            Assert.That(outcome.Total, Is.Zero);
            Assert.That(_ledger.CountCarriedBy(ALICE), Is.Zero);
        }

        [Test]
        public void WithoutProvisionalRule_MoneyInHandsReturnsToTable()
        {
            _ledger.PickFromTable(ALICE, 2, _changed);
            var outcome = _ledger.ResolveRound(CORRECT, 1, loseUnplaced: false, _changed);
            Assert.That(outcome.Total, Is.EqualTo(100_000));
            Assert.That(_ledger.CountCarriedBy(ALICE), Is.Zero);
        }

        [Test]
        public void NothingBet_WithProvisionalRule_LosesEverything()
        {
            var outcome = _ledger.ResolveRound(CORRECT, 1, loseUnplaced: true, _changed);
            Assert.That(outcome.Total, Is.Zero);
        }

        [Test]
        public void Multiplier_PaysBonusInLargestDenominations()
        {
            // 20 fajos en la correcta = 100.000; x7 => bono 600.000 = 1 lingote + 2 bolsas.
            Bet(CORRECT, 5); Bet(CORRECT, 5); Bet(CORRECT, 5); Bet(CORRECT, 5);
            _changed.Clear();
            var outcome = _ledger.ResolveRound(CORRECT, 7, loseUnplaced: true, _changed);

            Assert.That(outcome.Bonus, Is.EqualTo(600_000));
            Assert.That(outcome.Total, Is.EqualTo(700_000));
            Assert.That(_ledger.CountAt(MoneyPlace.Table, 0, Denomination.GoldBar), Is.EqualTo(1));
            Assert.That(_ledger.CountAt(MoneyPlace.Table, 0, Denomination.Bag), Is.EqualTo(2));
            Assert.That(_ledger.CountAt(MoneyPlace.Table, 0, Denomination.Bundle), Is.EqualTo(20));
        }

        [Test]
        public void Bonus_ReusesLostItemSlotsInsteadOfGrowing()
        {
            Bet(CORRECT, 5);
            Bet(WRONG, 5);
            var countBefore = _ledger.Count;
            _ledger.ResolveRound(CORRECT, 2, loseUnplaced: true, _changed); // pierde 15, bono 25.000
            Assert.That(_ledger.Count, Is.EqualTo(countBefore), "El bono reutiliza índices de objetos perdidos.");
        }

        [Test]
        public void Resolution_KeepsTableSlotsCompact()
        {
            Bet(CORRECT, 5);
            _ledger.ResolveRound(CORRECT, 1, loseUnplaced: true, _changed);
            foreach (var item in _ledger.Items)
            {
                if (item.Place == MoneyPlace.Table)
                    Assert.That(item.Slot, Is.LessThan(5), "Lo que vuelve ocupa los primeros huecos de la mesa.");
            }
        }

        [Test]
        public void EmitToTable_UsesFewestItems()
        {
            var ledger = new MoneyLedger(new DenominationValues(5_000, 50_000, 500_000), new CarryLimits(5, 2, 5));
            var emitted = ledger.EmitToTable(6_400_000, _changed);
            Assert.That(emitted, Is.EqualTo(6_400_000));
            Assert.That(ledger.CountAt(MoneyPlace.Table, 0, Denomination.GoldBar), Is.EqualTo(12));
            Assert.That(ledger.CountAt(MoneyPlace.Table, 0, Denomination.Bag), Is.EqualTo(8));
            Assert.That(ledger.CountAt(MoneyPlace.Table, 0, Denomination.Bundle), Is.Zero);
        }
    }
}
