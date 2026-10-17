using System.Collections.Generic;

namespace Trivias.Shared.Interaction
{
    /// <summary>
    /// Formato común de los avisos de interacción: una o varias acciones con su tecla («[E] Coger fajo   [Q] Coger 5»).
    /// Viajan como texto con marcas invisibles que la vista separa para dibujar cada tecla como una tecla de verdad.
    /// </summary>
    public static class PromptText
    {
        private const char KEY_START = '\u0002';
        private const char KEY_END = '\u0003';
        private const char SEPARATOR = '\u001F';

        public static string Key(string key, string action) => $"{KEY_START}{key}{KEY_END}{action}";

        public static string Join(string first, string second) =>
            string.IsNullOrEmpty(first) ? second : string.IsNullOrEmpty(second) ? first : first + SEPARATOR + second;

        /// <summary>Separa el aviso en sus acciones (tecla, o vacía si no lleva, y texto). Se llama al cambiar el aviso.</summary>
        public static void Split(string prompt, List<(string Key, string Action)> into)
        {
            into.Clear();
            if (string.IsNullOrEmpty(prompt))
                return;
            foreach (var part in prompt.Split(SEPARATOR))
            {
                var end = part.IndexOf(KEY_END);
                if (part.Length > 0 && part[0] == KEY_START && end > 0)
                    into.Add((part.Substring(1, end - 1), part.Substring(end + 1)));
                else
                    into.Add((string.Empty, part));
            }
        }

        /// <summary>El aviso tal como se lee («E Coger fajo · Q Coger 5»), para registros y pruebas.</summary>
        public static string Plain(string prompt)
        {
            var parts = new List<(string Key, string Action)>();
            Split(prompt, parts);
            var plain = new System.Text.StringBuilder();
            foreach (var (key, action) in parts)
            {
                if (plain.Length > 0)
                    plain.Append(" · ");
                if (key.Length > 0)
                    plain.Append(key).Append(' ');
                plain.Append(action);
            }
            return plain.ToString();
        }
    }
}
