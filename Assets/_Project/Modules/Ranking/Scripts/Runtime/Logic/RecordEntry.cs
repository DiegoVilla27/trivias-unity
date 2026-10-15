using System;
using System.Collections.Generic;

namespace Trivias.Ranking.Logic
{
    /// <summary>Una victoria guardada en los récords de este ordenador (GDD 12).</summary>
    [Serializable]
    public sealed class RecordEntry
    {
        /// <summary>Partida de la que sale (evita guardarla dos veces).</summary>
        public string matchId;

        /// <summary>Tamaño inicial del grupo: la categoría (2-6).</summary>
        public int groupSize;

        public long money;

        /// <summary>Tiempo total en milisegundos (desempate, GDD 12.2).</summary>
        public long durationMs;

        public string players;
        public string survivors;

        /// <summary>Fecha local de la victoria (aaaa-mm-dd).</summary>
        public string date;
    }

    /// <summary>Todos los récords guardados, de todas las categorías.</summary>
    [Serializable]
    public sealed class RecordBook
    {
        public List<RecordEntry> entries = new();

        /// <summary>Corrige datos incompletos de un archivo antiguo o editado a mano.</summary>
        public void Normalize()
        {
            entries ??= new List<RecordEntry>();
            entries.RemoveAll(entry => entry == null);
        }
    }
}
