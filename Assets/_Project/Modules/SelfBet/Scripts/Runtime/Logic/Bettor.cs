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

        /// <summary>Se desconectó durante la partida: no cuenta como activo, ni multiplica, ni es expulsado.</summary>
        public readonly bool HasLeft;

        public Bettor(ulong clientId, int personalUses, int booth, bool usesReserve, bool isLocked, bool isEliminated,
            bool hasLeft = false)
        {
            ClientId = clientId;
            PersonalUses = personalUses;
            Booth = booth;
            UsesReserve = usesReserve;
            IsLocked = isLocked;
            IsEliminated = isEliminated;
            HasLeft = hasLeft;
        }

        public bool HasBet => Booth >= 0;

        /// <summary>Sigue jugando: ni expulsado ni desconectado.</summary>
        public bool IsActive => !IsEliminated && !HasLeft;

        public Bettor WithBooth(int booth) => new(ClientId, PersonalUses, booth, UsesReserve, IsLocked, IsEliminated, HasLeft);

        public Bettor WithReserve(bool usesReserve) => new(ClientId, PersonalUses, Booth, usesReserve, IsLocked, IsEliminated, HasLeft);

        public bool Equals(Bettor other) =>
            ClientId == other.ClientId && PersonalUses == other.PersonalUses && Booth == other.Booth &&
            UsesReserve == other.UsesReserve && IsLocked == other.IsLocked && IsEliminated == other.IsEliminated &&
            HasLeft == other.HasLeft;

        public override bool Equals(object obj) => obj is Bettor other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(ClientId, PersonalUses, Booth, UsesReserve, IsLocked, IsEliminated, HasLeft);
    }
}
