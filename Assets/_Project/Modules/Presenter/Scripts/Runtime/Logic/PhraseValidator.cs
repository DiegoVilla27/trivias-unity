using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Trivias.Presenter.Logic
{
    /// <summary>Reglas del catálogo de frases: id único, evento conocido, texto y solo variables admitidas.</summary>
    public static class PhraseValidator
    {
        private static readonly Regex Placeholder = new(@"\{[^}]*\}");

        public static List<string> Validate(IReadOnlyList<(string Id, string Event, string Text)> phrases)
        {
            var errors = new List<string>();
            var ids = new HashSet<string>();
            var covered = new bool[PresenterEvents.COUNT];

            for (var i = 0; i < phrases.Count; i++)
            {
                var (id, eventKey, text) = phrases[i];
                var label = string.IsNullOrWhiteSpace(id) ? $"#{i}" : id;

                if (string.IsNullOrWhiteSpace(id))
                    errors.Add($"{label}: falta el id.");
                else if (!ids.Add(id))
                    errors.Add($"{label}: id repetido.");

                if (!PresenterEvents.TryParse(eventKey, out var presenterEvent))
                    errors.Add($"{label}: evento desconocido '{eventKey}'.");
                else
                    covered[(int)presenterEvent] = true;

                if (string.IsNullOrWhiteSpace(text))
                {
                    errors.Add($"{label}: falta el texto.");
                    continue;
                }

                foreach (System.Text.RegularExpressions.Match match in Placeholder.Matches(text))
                {
                    if (System.Array.IndexOf(PhraseVariables.Allowed, match.Value) < 0)
                        errors.Add($"{label}: variable no admitida {match.Value}.");
                }
            }

            for (var e = 0; e < covered.Length; e++)
            {
                if (!covered[e])
                    errors.Add($"Falta al menos una frase para el evento {(PresenterEvent)e}.");
            }
            return errors;
        }
    }
}
