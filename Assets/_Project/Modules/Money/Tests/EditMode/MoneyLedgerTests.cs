using System.Collections.Generic;
using NUnit.Framework;
using Trivias.Money.Logic;

namespace Trivias.Money.Tests
{
    public sealed class MoneyLedgerTests
    {
        private const ulong ALICE = 0;
        private const ulong BOB = 1;
        private const int TRAY_A = 0;
        private const int TRAY_B = 1;
        private const int CAPACITY = 5; // fajos
        private const int BAG_LIMIT = 2;

        private MoneyLedger _ledger;
        private readonly List<int> _changed = new();

        [SetUp]
        public void CreateLedgerWithStartingCapital()
        {
            _ledger = NewLedger();
            for (var i = 0; i < 20; i++)
                _ledger.AddToTable(Denomination.Bundle);
            _changed.Clear();
        }

        [Test]
        public void StartingCapital_IsAllOnTable()
        {
            Assert.That(_ledger.TotalAt(MoneyPlace.Table, 0), Is.EqualTo(100_000));
            Assert.That(_ledger.TotalInPlay(), Is.EqualTo(100_000));
        }

        [Test]
        public void PickOne_CarriesOneItem()
        {
            Assert.That(_ledger.PickFromTable(ALICE, 1, _changed), Is.EqualTo(1));
            Assert.That(_ledger.CountCarriedBy(ALICE), Is.EqualTo(1));
            Assert.That(_ledger.TotalAt(MoneyPlace.Table, 0), Is.EqualTo(95_000));
            Assert.That(_ledger.TotalInPlay(), Is.EqualTo(100_000), "Coger dinero no cambia el total.");
        }

        [Test]
        public void PickOne_RepeatedlyStacksInHands()
        {
            _ledger.PickFromTable(ALICE, 1, _changed);
            _ledger.PickFromTable(ALICE, 1, _changed);
            Assert.That(_ledger.CountCarriedBy(ALICE), Is.EqualTo(2));
            Assert.That(_ledger.CarriedValue(ALICE), Is.EqualTo(10_000));
        }

        [Test]
        public void PickBatch_FillsHandsUpToCapacity()
        {
            Assert.That(_ledger.PickFromTable(ALICE, int.MaxValue, _changed), Is.EqualTo(CAPACITY));
            Assert.That(_changed.Count, Is.EqualTo(CAPACITY));
            Assert.That(_ledger.PickFromTable(ALICE, 1, _changed), Is.Zero, "Con las manos llenas no se coge más.");
        }

        [Test]
        public void PickBatch_TopsUpPartiallyFilledHands()
        {
            _ledger.PickFromTable(ALICE, 2, _changed);
            Assert.That(_ledger.PickFromTable(ALICE, int.MaxValue, _changed), Is.EqualTo(CAPACITY - 2));
        }

        [Test]
        public void TwoPlayers_PickDifferentItems()
        {
            _ledger.PickFromTable(ALICE, CAPACITY, _changed);
            _ledger.PickFromTable(BOB, CAPACITY, _changed);
            Assert.That(new HashSet<int>(_changed).Count, Is.EqualTo(CAPACITY * 2));
            Assert.That(_ledger.TotalAt(MoneyPlace.Table, 0), Is.EqualTo(50_000));
        }

        [Test]
        public void DepositOne_LeavesTopOfStack()
        {
            _ledger.PickFromTable(ALICE, 3, _changed);
            var top = _ledger.FindCarriedBy(ALICE);
            _changed.Clear();

            Assert.That(_ledger.DepositToTray(ALICE, TRAY_A, 1, _changed), Is.EqualTo(1));
            Assert.That(_changed[0], Is.EqualTo(top), "Se deja primero el último cogido.");
            Assert.That(_ledger.CountCarriedBy(ALICE), Is.EqualTo(2));
            Assert.That(_ledger.TotalAt(MoneyPlace.Tray, TRAY_A), Is.EqualTo(5_000));
        }

        [Test]
        public void DepositAll_EmptiesHands()
        {
            _ledger.PickFromTable(ALICE, CAPACITY, _changed);
            Assert.That(_ledger.DepositToTray(ALICE, TRAY_B, int.MaxValue, _changed), Is.EqualTo(CAPACITY));
            Assert.That(_ledger.CountCarriedBy(ALICE), Is.Zero);
            Assert.That(_ledger.TotalAt(MoneyPlace.Tray, TRAY_B), Is.EqualTo(25_000));
            Assert.That(_ledger.TotalAt(MoneyPlace.Tray, TRAY_A), Is.Zero);
        }

        [Test]
        public void Deposit_WithoutCarrying_MovesNothing() =>
            Assert.That(_ledger.DepositToTray(ALICE, TRAY_A, 1, _changed), Is.Zero);

        [Test]
        public void PickFromTray_RecoversLastDeposited()
        {
            _ledger.PickFromTable(ALICE, 2, _changed);
            var lastCarried = _ledger.FindCarriedBy(ALICE);
            _ledger.DepositToTray(ALICE, TRAY_A, 1, _changed);
            _ledger.DepositToTray(ALICE, TRAY_A, 1, _changed);
            _changed.Clear();

            Assert.That(_ledger.PickFromTray(BOB, TRAY_A, 1, _changed), Is.EqualTo(1));
            Assert.That(_changed[0], Is.Not.EqualTo(lastCarried), "El primero dejado queda debajo.");
            Assert.That(_ledger.TotalAt(MoneyPlace.Tray, TRAY_A), Is.EqualTo(5_000));
        }

        [Test]
        public void PickFromEmptyTray_MovesNothing() =>
            Assert.That(_ledger.PickFromTray(ALICE, TRAY_A, CAPACITY, _changed), Is.Zero);

        [Test]
        public void ReturnAll_PutsEverythingBackOnTable()
        {
            _ledger.PickFromTable(ALICE, CAPACITY, _changed);
            Assert.That(_ledger.ReturnToTable(ALICE, int.MaxValue, _changed), Is.EqualTo(CAPACITY));
            Assert.That(_ledger.TotalAt(MoneyPlace.Table, 0), Is.EqualTo(100_000));
        }

        [Test]
        public void CarryStack_StaysContiguousAfterDepositAndPick()
        {
            _ledger.PickFromTable(ALICE, 3, _changed);
            _ledger.DepositToTray(ALICE, TRAY_A, 1, _changed);
            _ledger.PickFromTable(ALICE, 1, _changed);

            var slots = new HashSet<int>();
            foreach (var item in _ledger.Items)
            {
                if (item.Place == MoneyPlace.Carried && item.Holder == ALICE)
                    slots.Add(item.Slot);
            }
            Assert.That(slots, Is.EquivalentTo(new[] { 0, 1, 2 }), "La pila en las manos no debe tener huecos.");
        }

        [Test]
        public void Slots_DoNotOverlapInSamePlace()
        {
            _ledger.PickFromTable(ALICE, 3, _changed);
            _ledger.DepositToTray(ALICE, TRAY_A, 3, _changed);

            var seen = new HashSet<int>();
            foreach (var item in _ledger.Items)
            {
                if (item.IsAt(MoneyPlace.Tray, TRAY_A))
                    Assert.That(seen.Add(item.Slot), Is.True, "Dos objetos en el mismo hueco de la bandeja.");
            }
        }

        [Test]
        public void TableSlot_IsReusedAfterReturning()
        {
            var top = _ledger.FindTopAt(MoneyPlace.Table, 0, preferSmallest: true);
            var freedSlot = _ledger.Items[top].Slot;
            _ledger.PickFromTable(ALICE, 1, _changed);
            Assert.That(_changed[0], Is.EqualTo(top));
            _ledger.ReturnToTable(ALICE, 1, _changed);
            Assert.That(_ledger.Items[top].Slot, Is.EqualTo(freedSlot));
        }

        [Test]
        public void PickFromTable_PrefersSmallestDenomination()
        {
            var bar = _ledger.AddToTable(Denomination.GoldBar);
            _ledger.PickFromTable(ALICE, 1, _changed);
            Assert.That(_changed[0], Is.Not.EqualTo(bar));
            Assert.That(_ledger.Items[_changed[0]].Denomination, Is.EqualTo(Denomination.Bundle));
        }

        [Test]
        public void Replica_SetMirrorsAuthorityState()
        {
            _ledger.PickFromTable(ALICE, 2, _changed);
            var replica = NewLedger();
            for (var i = 0; i < _ledger.Count; i++)
                replica.Set(i, _ledger.Items[i]);

            Assert.That(replica.CountCarriedBy(ALICE), Is.EqualTo(2));
            Assert.That(replica.TotalAt(MoneyPlace.Table, 0), Is.EqualTo(90_000));
        }

        // ---- Un solo tipo por viaje ----

        [Test]
        public void CarryingBundles_OnlyPicksMoreBundles()
        {
            _ledger.PickFromTable(ALICE, 1, _changed);
            _ledger.AddToTable(Denomination.Bag);
            _changed.Clear();

            _ledger.PickFromTable(ALICE, int.MaxValue, _changed);
            foreach (var index in _changed)
                Assert.That(_ledger.Items[index].Denomination, Is.EqualTo(Denomination.Bundle));
            Assert.That(_ledger.CountAt(MoneyPlace.Table, 0, Denomination.Bag), Is.EqualTo(1), "La bolsa debe seguir en la mesa.");
        }

        [Test]
        public void CarryingBags_CannotPickBundles()
        {
            var ledger = NewLedger();
            ledger.AddToTable(Denomination.Bag);
            ledger.PickFromTable(ALICE, 1, _changed);
            Assert.That(ledger.Items[_changed[0]].Denomination, Is.EqualTo(Denomination.Bag));

            ledger.AddToTable(Denomination.Bundle);
            _changed.Clear();
            Assert.That(ledger.PickFromTable(ALICE, 1, _changed), Is.Zero, "Llevando bolsas no se cogen fajos.");
        }

        [Test]
        public void Bags_AreLimitedToTwo()
        {
            var ledger = NewLedger();
            for (var i = 0; i < 4; i++)
                ledger.AddToTable(Denomination.Bag);

            Assert.That(ledger.PickFromTable(ALICE, int.MaxValue, _changed), Is.EqualTo(BAG_LIMIT));
            Assert.That(ledger.CountPickable(ALICE, MoneyPlace.Table, 0, preferSmallest: true), Is.Zero);
        }

        [Test]
        public void GoldBars_AreLimitedToFive()
        {
            var ledger = NewLedger();
            for (var i = 0; i < 7; i++)
                ledger.AddToTable(Denomination.GoldBar);

            Assert.That(ledger.PickFromTable(ALICE, int.MaxValue, _changed), Is.EqualTo(5));
        }

        [Test]
        public void TrayRecovery_StopsAtDifferentDenomination()
        {
            var ledger = NewLedger();
            for (var i = 0; i < 3; i++)
                ledger.AddToTable(Denomination.Bundle);
            ledger.AddToTable(Denomination.Bag);

            // Abajo de la bandeja: 3 fajos. Encima: 1 bolsa.
            ledger.PickFromTable(ALICE, int.MaxValue, _changed);
            ledger.DepositToTray(ALICE, TRAY_A, int.MaxValue, _changed);
            ledger.PickFromTable(ALICE, 1, _changed);
            var bag = ledger.FindCarriedBy(ALICE);
            Assert.That(ledger.Items[bag].Denomination, Is.EqualTo(Denomination.Bag));
            ledger.DepositToTray(ALICE, TRAY_A, 1, _changed);
            _changed.Clear();

            Assert.That(ledger.CountPickable(BOB, MoneyPlace.Tray, TRAY_A, preferSmallest: false), Is.EqualTo(1),
                "Solo se recupera la bolsa de arriba; debajo hay fajos.");
            Assert.That(ledger.PickFromTray(BOB, TRAY_A, int.MaxValue, _changed), Is.EqualTo(1));
            Assert.That(_changed[0], Is.EqualTo(bag));
            Assert.That(ledger.CountPickable(BOB, MoneyPlace.Tray, TRAY_A, preferSmallest: false), Is.Zero,
                "Llevando una bolsa no se recuperan fajos.");
        }

        [Test]
        public void CountPickable_MatchesBatchPick()
        {
            var expected = _ledger.CountPickable(ALICE, MoneyPlace.Table, 0, preferSmallest: true);
            Assert.That(_ledger.PickFromTable(ALICE, int.MaxValue, _changed), Is.EqualTo(expected));
        }

        private static MoneyLedger NewLedger() =>
            new(new DenominationValues(5_000, 50_000, 500_000), new CarryLimits(CAPACITY, BAG_LIMIT, 5));

        [TestCase(0, "0")]
        [TestCase(5_000, "5.000")]
        [TestCase(6_400_000, "6.400.000")]
        public void Format_UsesDotsAsThousandsSeparator(long value, string expected) =>
            Assert.That(MoneyFormat.Amount(value), Is.EqualTo(expected));
    }
}
