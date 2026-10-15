using System.Collections.Generic;

namespace Trivias.Money.Logic
{
    /// <summary>
    /// Libro de cuentas del dinero de la partida: qué objetos existen y dónde está cada uno.
    /// C# puro. El host lo usa como autoridad (operaciones Try*) y los clientes como réplica (<see cref="Set"/>).
    /// Los objetos no se borran: el índice es su identificador estable.
    /// Un concursante puede llevar varios objetos apilados, todos de la misma denominación y hasta el límite
    /// de esa denominación (<see cref="CarryLimits"/>); el hueco (<see cref="MoneyItem.Slot"/>) de un objeto
    /// transportado es su posición en la pila.
    /// </summary>
    public sealed class MoneyLedger
    {
        private const int ANY_DENOMINATION = -1;

        private readonly DenominationValues _values;
        private readonly CarryLimits _limits;
        private readonly List<MoneyItem> _items = new();

        public MoneyLedger(DenominationValues values, CarryLimits limits)
        {
            _values = values;
            _limits = limits;
        }

        public IReadOnlyList<MoneyItem> Items => _items;
        public int Count => _items.Count;

        public int CarryLimit(Denomination denomination) => _limits.For(denomination);

        public long ValueOf(int index) => _values.Of(_items[index].Denomination);

        // ---- Réplica (clientes) ----

        public void Clear() => _items.Clear();

        public void Set(int index, MoneyItem item)
        {
            while (_items.Count <= index)
                _items.Add(default);
            _items[index] = item;
        }

        // ---- Autoridad (host) ----
        // Las operaciones devuelven cuántos objetos movieron y añaden sus índices a "changed" (lista reutilizable).

        /// <summary>Crea un objeto nuevo en la mesa. Devuelve su índice.</summary>
        public int AddToTable(Denomination denomination)
        {
            var slot = FreeSlot(MoneyPlace.Table, 0);
            _items.Add(new MoneyItem(denomination, MoneyPlace.Table, 0, (ushort)slot, 0));
            return _items.Count - 1;
        }

        /// <summary>
        /// Coge de la mesa hasta <paramref name="count"/> objetos. Si ya lleva algo, solo de esa denominación;
        /// si no, de la denominación más pequeña disponible.
        /// </summary>
        public int PickFromTable(ulong client, int count, List<int> changed) =>
            PickUp(client, MoneyPlace.Table, 0, preferSmallest: true, count, changed);

        /// <summary>
        /// Recupera de una bandeja hasta <paramref name="count"/> objetos, empezando por el último depositado
        /// y parando cuando el siguiente es de otra denominación.
        /// </summary>
        public int PickFromTray(ulong client, int tray, int count, List<int> changed) =>
            PickUp(client, MoneyPlace.Tray, tray, preferSmallest: false, count, changed);

        /// <summary>Deja en una bandeja hasta <paramref name="count"/> objetos, empezando por el de arriba de la pila.</summary>
        public int DepositToTray(ulong client, int tray, int count, List<int> changed) =>
            PutDown(client, MoneyPlace.Tray, tray, count, changed);

        public int ReturnToTable(ulong client, int count, List<int> changed) =>
            PutDown(client, MoneyPlace.Table, 0, count, changed);

        private int PickUp(ulong client, MoneyPlace from, int fromIndex, bool preferSmallest, int count, List<int> changed)
        {
            var carried = CountCarriedBy(client);
            var carriedTop = FindCarriedBy(client);
            var denomination = carriedTop >= 0 ? (int)_items[carriedTop].Denomination : ANY_DENOMINATION;
            var moved = 0;

            while (moved < count)
            {
                var index = FindTopAt(from, fromIndex, preferSmallest, denomination);
                if (index < 0)
                    break;

                var itemDenomination = _items[index].Denomination;
                // En la bandeja se para si el de arriba es de otra denominación: no se rebusca debajo.
                if (denomination != ANY_DENOMINATION && (int)itemDenomination != denomination)
                    break;
                if (carried >= _limits.For(itemDenomination))
                    break;

                denomination = (int)itemDenomination;
                _items[index] = _items[index].With(MoneyPlace.Carried, 0, carried, client);
                changed.Add(index);
                carried++;
                moved++;
            }
            return moved;
        }

        private int PutDown(ulong client, MoneyPlace to, int toIndex, int count, List<int> changed)
        {
            var moved = 0;
            while (moved < count)
            {
                var index = FindCarriedBy(client);
                if (index < 0)
                    break;

                _items[index] = _items[index].With(to, toIndex, FreeSlot(to, toIndex), 0);
                changed.Add(index);
                moved++;
            }
            return moved;
        }

        // ---- Consultas ----

        /// <summary>
        /// Cuántos objetos movería coger "todo lo posible" ahora (para el aviso de Q), sin modificar nada.
        /// </summary>
        public int CountPickable(ulong client, MoneyPlace from, int fromIndex, bool preferSmallest)
        {
            var carriedTop = FindCarriedBy(client);
            var carried = CountCarriedBy(client);

            if (from == MoneyPlace.Table)
            {
                var top = carriedTop >= 0
                    ? FindTopAt(from, fromIndex, preferSmallest, (int)_items[carriedTop].Denomination)
                    : FindTopAt(from, fromIndex, preferSmallest);
                if (top < 0)
                    return 0;

                var denomination = _items[top].Denomination;
                var available = CountAt(from, fromIndex, denomination);
                var room = _limits.For(denomination) - carried;
                return available < room ? available : room;
            }

            // Bandeja: racha de objetos de arriba con la misma denominación.
            var first = FindTopAt(from, fromIndex, preferSmallest: false);
            if (first < 0 || (carriedTop >= 0 && _items[carriedTop].Denomination != _items[first].Denomination))
                return 0;

            var runDenomination = _items[first].Denomination;
            var run = 1;
            var slot = _items[first].Slot;
            while (true)
            {
                var below = FindHighestSlotBelow(from, fromIndex, slot);
                if (below < 0 || _items[below].Denomination != runDenomination)
                    break;
                run++;
                slot = _items[below].Slot;
            }

            var free = _limits.For(runDenomination) - carried;
            return run < free ? run : free;
        }

        /// <summary>Objeto de arriba de la pila que lleva un concursante, o -1 si no lleva nada.</summary>
        public int FindCarriedBy(ulong client)
        {
            var top = -1;
            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item.Place == MoneyPlace.Carried && item.Holder == client &&
                    (top < 0 || item.Slot > _items[top].Slot))
                {
                    top = i;
                }
            }
            return top;
        }

        public int CountCarriedBy(ulong client)
        {
            var count = 0;
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].Place == MoneyPlace.Carried && _items[i].Holder == client)
                    count++;
            }
            return count;
        }

        public long CarriedValue(ulong client)
        {
            long total = 0;
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].Place == MoneyPlace.Carried && _items[i].Holder == client)
                    total += _values.Of(_items[i].Denomination);
            }
            return total;
        }

        public int CountAt(MoneyPlace place, int placeIndex)
        {
            var count = 0;
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].IsAt(place, placeIndex))
                    count++;
            }
            return count;
        }

        public int CountAt(MoneyPlace place, int placeIndex, Denomination denomination)
        {
            var count = 0;
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].IsAt(place, placeIndex) && _items[i].Denomination == denomination)
                    count++;
            }
            return count;
        }

        public long TotalAt(MoneyPlace place, int placeIndex)
        {
            long total = 0;
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].IsAt(place, placeIndex))
                    total += _values.Of(_items[i].Denomination);
            }
            return total;
        }

        /// <summary>Todo el dinero que no se ha perdido.</summary>
        public long TotalInPlay()
        {
            long total = 0;
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].Place != MoneyPlace.Lost)
                    total += _values.Of(_items[i].Denomination);
            }
            return total;
        }

        /// <summary>
        /// Objeto "de arriba" en un lugar: el de hueco más alto. En la mesa se prefiere la denominación más pequeña
        /// para no arriesgar lingotes por accidente.
        /// </summary>
        public int FindTopAt(MoneyPlace place, int placeIndex, bool preferSmallest) =>
            FindTopAt(place, placeIndex, preferSmallest, ANY_DENOMINATION);

        private int FindTopAt(MoneyPlace place, int placeIndex, bool preferSmallest, int denomination)
        {
            // En la mesa se puede filtrar por denominación; en la bandeja manda el orden (solo el de arriba).
            var filter = place == MoneyPlace.Table ? denomination : ANY_DENOMINATION;
            var best = -1;
            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (!item.IsAt(place, placeIndex) || (filter != ANY_DENOMINATION && (int)item.Denomination != filter))
                    continue;

                if (best < 0)
                {
                    best = i;
                    continue;
                }

                var current = _items[best];
                var isBetter = preferSmallest
                    ? item.Denomination < current.Denomination ||
                      (item.Denomination == current.Denomination && item.Slot > current.Slot)
                    : item.Slot > current.Slot;

                if (isBetter)
                    best = i;
            }
            return best;
        }

        private int FindHighestSlotBelow(MoneyPlace place, int placeIndex, int slot)
        {
            var best = -1;
            for (var i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item.IsAt(place, placeIndex) && item.Slot < slot && (best < 0 || item.Slot > _items[best].Slot))
                    best = i;
            }
            return best;
        }

        /// <summary>Primer hueco libre en un lugar, para colocar el objeto sin solaparse con otros.</summary>
        private int FreeSlot(MoneyPlace place, int placeIndex)
        {
            for (var slot = 0; ; slot++)
            {
                var taken = false;
                for (var i = 0; i < _items.Count; i++)
                {
                    if (_items[i].IsAt(place, placeIndex) && _items[i].Slot == slot)
                    {
                        taken = true;
                        break;
                    }
                }

                if (!taken)
                    return slot;
            }
        }
    }
}
