using System;
using System.Collections.Generic;
using System.Text;

namespace Trivias.Shared.Profile
{
    /// <summary>
    /// Nombre del concursante (GDD 11): limpio, corto y único en la sala. C# puro, lo usan el menú (al escribirlo), el
    /// perfil (al guardarlo) y el host (al aceptarlo; lo que manda un cliente nunca se da por bueno).
    /// </summary>
    public static class PlayerNameRules
    {
        public const int MAX_LENGTH = 16;

        /// <summary>Último carácter admitido: letras latinas con tildes, que tiene la fuente del juego.</summary>
        private const char LAST_LATIN = 'ɏ';

        /// <summary>
        /// Deja solo letras latinas, números, espacios y «- _ .»; junta espacios y recorta a <see cref="MAX_LENGTH"/>.
        /// Quita también «&lt; &gt;» para que nadie meta etiquetas de texto enriquecido.
        /// </summary>
        public static string Sanitize(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return string.Empty;

            var builder = new StringBuilder(MAX_LENGTH);
            var pendingSpace = false;
            foreach (var c in raw)
            {
                if (char.IsWhiteSpace(c))
                {
                    pendingSpace = builder.Length > 0;
                    continue;
                }
                if (!IsAllowed(c))
                    continue;
                if (pendingSpace)
                {
                    if (builder.Length + 1 >= MAX_LENGTH)
                        break;
                    builder.Append(' ');
                    pendingSpace = false;
                }
                if (builder.Length >= MAX_LENGTH)
                    break;
                builder.Append(c);
            }
            return builder.ToString();
        }

        /// <summary>Nombre que se muestra: el elegido o «Jugador N» si no hay ninguno.</summary>
        public static string DisplayName(string sanitized, ulong clientId) =>
            string.IsNullOrEmpty(sanitized) ? DefaultName(clientId) : sanitized;

        public static string DefaultName(ulong clientId) => $"Jugador {clientId + 1}";

        /// <summary>Si otro concursante ya se llama igual (sin distinguir mayúsculas), añade « 2», « 3»…</summary>
        public static string Unique(string name, IReadOnlyCollection<string> taken)
        {
            if (!Contains(taken, name))
                return name;

            for (var number = 2; ; number++)
            {
                var suffix = " " + number;
                var stem = name.Length + suffix.Length > MAX_LENGTH ? name.Substring(0, MAX_LENGTH - suffix.Length).TrimEnd() : name;
                var candidate = stem + suffix;
                if (!Contains(taken, candidate))
                    return candidate;
            }
        }

        private static bool IsAllowed(char c) =>
            (c <= LAST_LATIN && char.IsLetterOrDigit(c)) || c == '-' || c == '_' || c == '.';

        private static bool Contains(IReadOnlyCollection<string> taken, string name)
        {
            if (taken == null)
                return false;
            foreach (var other in taken)
            {
                if (string.Equals(other, name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
