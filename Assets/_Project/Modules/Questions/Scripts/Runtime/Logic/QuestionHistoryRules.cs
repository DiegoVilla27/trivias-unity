using System.Collections.Generic;

namespace Trivias.Questions.Logic
{
    /// <summary>
    /// Historial de preguntas vistas (CONTENIDO.md): cada jugador recuerda en su ordenador qué preguntas ha visto y en
    /// qué orden, y al empezar una partida el host combina los historiales del grupo. C# puro.
    /// </summary>
    public static class QuestionHistoryRules
    {
        /// <summary>Máximo de preguntas recordadas por jugador (más que todo el banco).</summary>
        public const int LIMIT = 1000;

        /// <summary>Apunta una pregunta como la última vista (si ya estaba, pasa al final).</summary>
        public static bool MarkSeen(List<string> seen, string id, int limit = LIMIT)
        {
            if (string.IsNullOrEmpty(id))
                return false;
            if (seen.Count > 0 && seen[seen.Count - 1] == id)
                return false;

            seen.Remove(id);
            seen.Add(id);
            while (seen.Count > limit)
                seen.RemoveAt(0);
            return true;
        }

        /// <summary>
        /// Penalización de cada pregunta del banco para el grupo: 0 si nadie la ha visto. Si alguien la vio, suma su
        /// posición en el historial de cada jugador (1 para la más antigua). Así se eligen antes las que nadie ha visto,
        /// después las que vieron menos jugadores y hace más tiempo. Índices fuera del banco se ignoran.
        /// </summary>
        /// <param name="histories">Historial de cada participante: índices del banco, de la más antigua a la última.</param>
        public static int[] Penalties(int bankCount, IReadOnlyList<IReadOnlyList<ushort>> histories)
        {
            var penalties = new int[bankCount];
            if (histories == null)
                return penalties;

            foreach (var history in histories)
            {
                if (history == null)
                    continue;
                for (var position = 0; position < history.Count; position++)
                {
                    var index = history[position];
                    if (index < bankCount)
                        penalties[index] += position + 1;
                }
            }
            return penalties;
        }
    }
}
