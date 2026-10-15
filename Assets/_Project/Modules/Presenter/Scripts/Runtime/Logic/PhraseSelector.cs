using System;
using System.Collections.Generic;

namespace Trivias.Presenter.Logic
{
    /// <summary>
    /// Elige la frase de un evento evitando repetir las últimas usadas de ese evento (GDD 9.1). C# puro.
    /// </summary>
    public sealed class PhraseSelector
    {
        private const int AVOID_LAST = 2;

        private readonly IReadOnlyList<int>[] _phrasesByEvent;
        private readonly List<int>[] _recent;
        private readonly Random _random;
        private readonly List<int> _candidates = new();

        /// <param name="phrasesByEvent">Índices de frase del catálogo para cada evento.</param>
        public PhraseSelector(IReadOnlyList<int>[] phrasesByEvent, Random random)
        {
            _phrasesByEvent = phrasesByEvent;
            _random = random;
            _recent = new List<int>[phrasesByEvent.Length];
            for (var i = 0; i < _recent.Length; i++)
                _recent[i] = new List<int>(AVOID_LAST);
        }

        /// <summary>Índice de frase para el evento, o -1 si no hay ninguna.</summary>
        public int Next(PresenterEvent presenterEvent)
        {
            var phrases = _phrasesByEvent[(int)presenterEvent];
            if (phrases == null || phrases.Count == 0)
                return -1;

            var recent = _recent[(int)presenterEvent];
            var avoid = Math.Min(AVOID_LAST, phrases.Count - 1);

            _candidates.Clear();
            foreach (var phrase in phrases)
            {
                if (!IsRecent(recent, phrase, avoid))
                    _candidates.Add(phrase);
            }

            var chosen = _candidates[_random.Next(_candidates.Count)];
            recent.Insert(0, chosen);
            if (recent.Count > AVOID_LAST)
                recent.RemoveAt(recent.Count - 1);
            return chosen;
        }

        private static bool IsRecent(List<int> recent, int phrase, int window)
        {
            for (var i = 0; i < window && i < recent.Count; i++)
            {
                if (recent[i] == phrase)
                    return true;
            }
            return false;
        }
    }
}
