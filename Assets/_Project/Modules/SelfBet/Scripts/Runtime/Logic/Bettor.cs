using System;

namespace Trivias.SelfBet.Logic
{
    /// <summary>Estado de comodín de un concursante. Inmutable.</summary>
    public readonly struct Bettor : IEquatable<Bettor>
    {
        public readonly ulong ClientId;

        /// <summary>Usos personales que le quedan (sin contar el que tiene reservado si se apostó con uno personal).</summary>
        public readonly int PersonalUses;

        /// <summary>Cabina en la que se ha apostado (0-3) o -1.</summary>
        public readonly int Booth;

        /// <summary>La apuesta actual usa un comodín de la reserva compartida.</summary>
        public readonly bool UsesReserve;

        /// <summary>Apuesta fija tras verificar (ya no se puede cambiar).</summary>
        public readonly bool IsLocked;

        public readonly bool IsEliminated;

        public Bettor(ulong clientId, int personalUses, int booth, bool usesReserve, bool isLocked, bool isEliminated)
        {
            ClientId = clientId;
            PersonalUses = personalUses;
            Booth = booth;
            UsesReserve = usesReserve;
            IsLocked = isLocked;
            IsEliminated = isEliminated;
        }

        public bool HasBet => Booth >= 0;

        public Bettor WithBooth(int booth) => new(ClientId, PersonalUses, booth, UsesReserve, IsLocked, IsEliminated);

        public Bettor WithReserve(bool usesReserve) => new(ClientId, PersonalUses, Booth, usesReserve, IsLocked, IsEliminated);

        public bool Equals(Bettor other) =>
            ClientId == other.ClientId && PersonalUses == other.PersonalUses && Booth == other.Booth &&
            UsesReserve == other.UsesReserve && IsLocked == other.IsLocked && IsEliminated == other.IsEliminated;

        public override bool Equals(object obj) => obj is Bettor other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(ClientId, PersonalUses, Booth, UsesReserve, IsLocked, IsEliminated);
    }
}
