using System.Collections.Generic;

namespace Trivias.SelfBet.Logic
{
    /// <summary>
    /// Comodines de la partida (GDD 5): quién se ha apostado y dónde, usos personales y reserva compartida.
    /// C# puro. Reglas:
    /// - Apostarse reserva un uso (personal o, sin personales, de la reserva); cancelar lo libera.
    /// - Solo una apuesta por jugador y pregunta; se puede cambiar de cabina antes de verificar.
    /// - Al verificar se consumen los usos; al resolver, los apostados en la correcta siguen y los demás quedan eliminados.
    /// - El reparto se calcula con el tamaño inicial y no se recalcula al eliminar a alguien.
    /// </summary>
    public sealed class SelfBetBook
    {
        public const int NO_BOOTH = -1;

        private readonly List<Bettor> _bettors = new();

        public IReadOnlyList<Bettor> Bettors => _bettors;
        public int Reserve { get; private set; }

        public void Start(IReadOnlyList<ulong> clients)
        {
            _bettors.Clear();
            SelfBetDistribution.For(clients.Count, out var personal, out var reserve);
            Reserve = reserve;
            foreach (var client in clients)
                _bettors.Add(new Bettor(client, personal, NO_BOOTH, false, false, false));
        }

        /// <summary>Réplica en clientes: copia el estado recibido por red.</summary>
        public void Set(IReadOnlyList<Bettor> bettors, int reserve)
        {
            _bettors.Clear();
            _bettors.AddRange(bettors);
            Reserve = reserve;
        }

        public int IndexOf(ulong client)
        {
            for (var i = 0; i < _bettors.Count; i++)
            {
                if (_bettors[i].ClientId == client)
                    return i;
            }
            return -1;
        }

        /// <summary>Usos que puede gastar ahora: los personales, o la reserva si ya no le quedan.</summary>
        public int AvailableUses(ulong client)
        {
            var i = IndexOf(client);
            if (i < 0 || !_bettors[i].IsActive)
                return 0;
            return _bettors[i].PersonalUses > 0 ? _bettors[i].PersonalUses : Reserve;
        }

        /// <summary>Se apuesta (o cambia de cabina). Devuelve false si no puede.</summary>
        public bool Prepare(ulong client, int booth)
        {
            var i = IndexOf(client);
            if (i < 0)
                return false;

            var bettor = _bettors[i];
            if (!bettor.IsActive || bettor.IsLocked)
                return false;

            if (bettor.Booth != NO_BOOTH)
            {
                _bettors[i] = bettor.WithBooth(booth); // Cambio de cabina: mismo uso reservado.
                return true;
            }

            if (bettor.PersonalUses > 0)
            {
                _bettors[i] = new Bettor(client, bettor.PersonalUses, booth, false, false, false);
                return true;
            }

            if (Reserve <= 0)
                return false;

            Reserve--;
            _bettors[i] = new Bettor(client, 0, booth, true, false, false);
            return true;
        }

        public bool Cancel(ulong client)
        {
            var i = IndexOf(client);
            if (i < 0 || _bettors[i].Booth == NO_BOOTH || _bettors[i].IsLocked)
                return false;

            if (_bettors[i].UsesReserve)
                Reserve++;
            _bettors[i] = _bettors[i].WithBooth(NO_BOOTH).WithReserve(false);
            return true;
        }

        /// <summary>Al verificar: las apuestas preparadas quedan fijas y consumen su uso.</summary>
        public void LockAll()
        {
            for (var i = 0; i < _bettors.Count; i++)
            {
                var bettor = _bettors[i];
                if (bettor.Booth == NO_BOOTH)
                    continue;

                var personal = bettor.UsesReserve ? bettor.PersonalUses : bettor.PersonalUses - 1;
                _bettors[i] = new Bettor(bettor.ClientId, personal, bettor.Booth, bettor.UsesReserve, true, false);
            }
        }

        public int CountOn(int booth)
        {
            var count = 0;
            foreach (var bettor in _bettors)
            {
                if (bettor.Booth == booth && bettor.IsActive)
                    count++;
            }
            return count;
        }

        public bool HasBet(ulong client)
        {
            var i = IndexOf(client);
            return i >= 0 && _bettors[i].HasBet;
        }

        /// <summary>Todos los concursantes activos están apostados (nadie libre para ir al botón).</summary>
        public bool AllActiveHaveBet
        {
            get
            {
                var any = false;
                foreach (var bettor in _bettors)
                {
                    if (!bettor.IsActive)
                        continue;
                    if (!bettor.HasBet)
                        return false;
                    any = true;
                }
                return any;
            }
        }

        public int ActivePlayers
        {
            get
            {
                var count = 0;
                foreach (var bettor in _bettors)
                {
                    if (bettor.IsActive)
                        count++;
                }
                return count;
            }
        }

        /// <summary>
        /// Un concursante se desconecta durante la partida: su apuesta se anula (si usaba la reserva y aún no se había
        /// verificado, el uso vuelve al equipo), pierde sus usos y deja de contar. El reparto no se recalcula (GDD 10).
        /// </summary>
        public bool RemovePlayer(ulong client)
        {
            var i = IndexOf(client);
            if (i < 0 || _bettors[i].HasLeft)
                return false;

            var bettor = _bettors[i];
            if (bettor.HasBet && bettor.UsesReserve && !bettor.IsLocked)
                Reserve++;
            _bettors[i] = new Bettor(client, 0, NO_BOOTH, false, false, bettor.IsEliminated, hasLeft: true);
            return true;
        }

        /// <summary>
        /// Resuelve la pregunta: multiplicador = 1 + apostados en la cabina correcta. Los apostados en otra cabina
        /// quedan eliminados (pierden sus usos restantes) y se añaden a <paramref name="eliminated"/>.
        /// </summary>
        public int Resolve(int correctBooth, List<ulong> eliminated)
        {
            var multiplier = 1;
            for (var i = 0; i < _bettors.Count; i++)
            {
                var bettor = _bettors[i];
                if (!bettor.IsLocked || bettor.HasLeft)
                    continue;

                if (bettor.Booth == correctBooth)
                {
                    multiplier++;
                    _bettors[i] = new Bettor(bettor.ClientId, bettor.PersonalUses, NO_BOOTH, false, false, false);
                }
                else
                {
                    eliminated.Add(bettor.ClientId);
                    _bettors[i] = new Bettor(bettor.ClientId, 0, NO_BOOTH, false, false, true);
                }
            }
            return multiplier;
        }
    }
}
