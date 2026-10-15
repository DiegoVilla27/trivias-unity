using System;
using System.Collections.Generic;
using System.IO;
using Trivias.Ranking.Logic;
using UnityEngine;

namespace Trivias.Ranking.Data
{
    /// <summary>
    /// Récords de victorias de este ordenador (GDD 12), por tamaño inicial del grupo. Se cargan una vez y se guardan
    /// al entrar una victoria. Los resultados presentan cada partida; el menú los muestra. El ranking global llegará con Steam.
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Ranking/Local Records", fileName = "LocalRecords")]
    public sealed class LocalRecords : ScriptableObject
    {
        private const string FILE_NAME = "trivias-records.json";

        public event Action Changed;

        // Lo que respondió cada partida ya presentada (los resultados se redibujan varias veces).
        private readonly Dictionary<string, RecordOutcome> _submitted = new();
        private RecordBook _book;
        private string _path;

        public RecordBook Book
        {
            get
            {
                EnsureLoaded();
                return _book;
            }
        }

        /// <summary>Cambia dónde se guardan (las pruebas usan una carpeta temporal, nunca los récords reales).</summary>
        public void UseStoragePath(string path)
        {
            _path = path;
            _book = null;
            _submitted.Clear();
            EnsureLoaded();
            Changed?.Invoke();
        }

        /// <summary>Presenta una partida terminada. Repetirla devuelve lo mismo sin volver a guardarla.</summary>
        public RecordOutcome Submit(RecordEntry entry, RecordEligibility eligibility)
        {
            if (!string.IsNullOrEmpty(entry.matchId) && _submitted.TryGetValue(entry.matchId, out var known))
                return known;

            var outcome = RankingRules.Submit(Book, entry, eligibility);
            if (!string.IsNullOrEmpty(entry.matchId))
                _submitted[entry.matchId] = outcome;
            if (eligibility == RecordEligibility.Eligible)
            {
                RecordStore.Save(_path, _book);
                Changed?.Invoke();
            }
            return outcome;
        }

        public void Category(int groupSize, List<RecordEntry> into) => RankingRules.Category(Book, groupSize, into);

        private void EnsureLoaded()
        {
            if (_book != null)
                return;
            _path ??= Path.Combine(Application.persistentDataPath, FILE_NAME);
            _book = RecordStore.Load(_path);
        }

        private void OnEnable()
        {
            _book = null;
            _path = null;
            _submitted.Clear();
        }
    }
}
