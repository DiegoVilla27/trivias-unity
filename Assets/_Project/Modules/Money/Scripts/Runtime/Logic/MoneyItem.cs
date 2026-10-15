using System;

namespace Trivias.Money.Logic
{
    /// <summary>
    /// Estado de un objeto de dinero. Inmutable: cada cambio crea un valor nuevo.
    /// <see cref="PlaceIndex"/> es la bandeja (0-3) cuando está en una; <see cref="Slot"/> su hueco para apilarse;
    /// <see cref="Holder"/> el jugador que lo lleva cuando está transportado.
    /// </summary>
    public readonly struct MoneyItem : IEquatable<MoneyItem>
    {
        public readonly Denomination Denomination;
        public readonly MoneyPlace Place;
        public readonly byte PlaceIndex;
        public readonly ushort Slot;
        public readonly ulong Holder;

        public MoneyItem(Denomination denomination, MoneyPlace place, byte placeIndex, ushort slot, ulong holder)
        {
            Denomination = denomination;
            Place = place;
            PlaceIndex = placeIndex;
            Slot = slot;
            Holder = holder;
        }

        public bool IsAt(MoneyPlace place, int placeIndex) =>
            Place == place && (place != MoneyPlace.Tray || PlaceIndex == placeIndex);

        public MoneyItem With(MoneyPlace place, int placeIndex, int slot, ulong holder) =>
            new(Denomination, place, (byte)placeIndex, (ushort)slot, holder);

        public bool Equals(MoneyItem other) =>
            Denomination == other.Denomination && Place == other.Place && PlaceIndex == other.PlaceIndex &&
            Slot == other.Slot && Holder == other.Holder;

        public override bool Equals(object obj) => obj is MoneyItem other && Equals(other);

        public override int GetHashCode() => HashCode.Combine((int)Denomination, (int)Place, PlaceIndex, Slot, Holder);
    }
}
