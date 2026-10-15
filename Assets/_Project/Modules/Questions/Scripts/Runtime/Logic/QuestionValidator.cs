using System.Collections.Generic;

namespace Trivias.Questions.Logic
{
    /// <summary>
    /// Reglas de contenido (CONTENIDO.md): id único, texto, 4 opciones distintas, una correcta existente,
    /// dificultad 1-3 y explicación. Se ejecuta en el editor al importar, nunca durante la partida.
    /// </summary>
    public static class QuestionValidator
    {
        public readonly struct Entry
        {
            public readonly string Id;
            public readonly string Text;
            public readonly int Difficulty;
            public readonly IReadOnlyList<string> OptionIds;
            public readonly IReadOnlyList<string> OptionTexts;
            public readonly string CorrectOptionId;
            public readonly string Explanation;

            public Entry(string id, string text, int difficulty, IReadOnlyList<string> optionIds,
                IReadOnlyList<string> optionTexts, string correctOptionId, string explanation)
            {
                Id = id;
                Text = text;
                Difficulty = difficulty;
                OptionIds = optionIds;
                OptionTexts = optionTexts;
                CorrectOptionId = correctOptionId;
                Explanation = explanation;
            }
        }

        /// <summary>Devuelve la lista de errores (vacía si todo es correcto).</summary>
        public static List<string> Validate(IReadOnlyList<Entry> entries)
        {
            var errors = new List<string>();
            var ids = new HashSet<string>();

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var label = string.IsNullOrWhiteSpace(entry.Id) ? $"#{i}" : entry.Id;

                if (string.IsNullOrWhiteSpace(entry.Id))
                    errors.Add($"{label}: falta el id.");
                else if (!ids.Add(entry.Id))
                    errors.Add($"{label}: id repetido.");

                if (string.IsNullOrWhiteSpace(entry.Text))
                    errors.Add($"{label}: falta el texto de la pregunta.");
                if (entry.Difficulty < 1 || entry.Difficulty > QuestionPlanner.DIFFICULTY_LEVELS)
                    errors.Add($"{label}: la dificultad debe estar entre 1 y {QuestionPlanner.DIFFICULTY_LEVELS}.");
                if (string.IsNullOrWhiteSpace(entry.Explanation))
                    errors.Add($"{label}: falta la explicación.");

                if (entry.OptionIds == null || entry.OptionIds.Count != OptionOrder.COUNT)
                {
                    errors.Add($"{label}: debe tener exactamente {OptionOrder.COUNT} opciones.");
                    continue;
                }

                var optionIds = new HashSet<string>();
                var optionTexts = new HashSet<string>();
                for (var o = 0; o < entry.OptionIds.Count; o++)
                {
                    if (!optionIds.Add(entry.OptionIds[o] ?? string.Empty))
                        errors.Add($"{label}: id de opción repetido '{entry.OptionIds[o]}'.");
                    var text = entry.OptionTexts[o];
                    if (string.IsNullOrWhiteSpace(text))
                        errors.Add($"{label}: la opción '{entry.OptionIds[o]}' no tiene texto.");
                    else if (!optionTexts.Add(text.Trim().ToLowerInvariant()))
                        errors.Add($"{label}: opción repetida '{text}'.");
                }

                if (!optionIds.Contains(entry.CorrectOptionId ?? string.Empty))
                    errors.Add($"{label}: la respuesta correcta '{entry.CorrectOptionId}' no es ninguna de las opciones.");
            }

            return errors;
        }
    }
}
