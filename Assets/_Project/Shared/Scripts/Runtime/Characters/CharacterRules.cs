using System;
using System.Collections.Generic;

namespace Trivias.Shared.Characters
{
    /// <summary>
    /// Reglas de los personajes (C# puro, con pruebas): cuáles tiene un jugador, cuál lleva si el guardado ya no vale,
    /// cómo se pasa de uno a otro en el menú y cómo el host evita que dos jugadores lleven el mismo.
    /// </summary>
    public static class CharacterRules
    {
        /// <summary>Lo tiene si es gratuito o lo compró.</summary>
        public static bool IsUnlocked(string id, IReadOnlyList<string> freeIds, ICollection<string> owned) =>
            !string.IsNullOrEmpty(id) && (Contains(freeIds, id) || owned.Contains(id));

        /// <summary>El guardado si existe y lo tiene; si no, el primero que tenga (o vacío si no tiene ninguno).</summary>
        public static string Current(string saved, IReadOnlyList<string> all, IReadOnlyList<string> freeIds, ICollection<string> owned)
        {
            if (Contains(all, saved) && IsUnlocked(saved, freeIds, owned))
                return saved;
            foreach (var id in all)
            {
                if (IsUnlocked(id, freeIds, owned))
                    return id;
            }
            return string.Empty;
        }

        /// <summary>El siguiente (+1) o anterior (−1) de los que tiene, dando la vuelta.</summary>
        public static string Step(string current, int direction, IReadOnlyList<string> all, IReadOnlyList<string> freeIds,
            ICollection<string> owned)
        {
            var count = all.Count;
            if (count == 0)
                return string.Empty;
            var start = IndexOf(all, current);
            if (start < 0)
                start = direction > 0 ? -1 : 0;
            var step = Math.Sign(direction == 0 ? 1 : direction);
            for (var i = 1; i <= count; i++)
            {
                var candidate = all[((start + step * i) % count + count) % count];
                if (IsUnlocked(candidate, freeIds, owned))
                    return candidate;
            }
            return current;
        }

        /// <summary>
        /// En el host: el pedido si existe y nadie más lo lleva; si no, el primer gratuito libre (todos los tienen) y,
        /// si no queda ninguno, el primero libre del catálogo.
        /// </summary>
        public static string Assign(string requested, IReadOnlyList<string> all, IReadOnlyList<string> freeIds,
            ICollection<string> taken)
        {
            if (Contains(all, requested) && !taken.Contains(requested))
                return requested;
            foreach (var id in freeIds)
            {
                if (Contains(all, id) && !taken.Contains(id))
                    return id;
            }
            foreach (var id in all)
            {
                if (!taken.Contains(id))
                    return id;
            }
            return all.Count > 0 ? all[0] : string.Empty;
        }

        private static bool Contains(IReadOnlyList<string> list, string id) => !string.IsNullOrEmpty(id) && IndexOf(list, id) >= 0;

        private static int IndexOf(IReadOnlyList<string> list, string id)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] == id)
                    return i;
            }
            return -1;
        }
    }
}
