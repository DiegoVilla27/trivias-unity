namespace Trivias.Shared.Rooms
{
    /// <summary>Comprueba la dirección que escribe quien se une (C# puro, probado en EditMode).</summary>
    public static class RoomAddressInput
    {
        public const int MAX_LENGTH = 253;

        /// <summary>
        /// Acepta una IPv4 (p. ej. 10.147.17.5) o un nombre de equipo (p. ej. mi-pc.local). Quita espacios alrededor.
        /// </summary>
        public static bool TryNormalize(string input, out string address, out string error)
        {
            address = (input ?? string.Empty).Trim();
            error = null;

            if (address.Length == 0)
            {
                error = "Escribe la dirección que te pasó el host.";
                return false;
            }
            if (address.Length > MAX_LENGTH)
            {
                error = "La dirección es demasiado larga.";
                return false;
            }

            if (LooksNumeric(address))
            {
                if (IsIpv4(address))
                    return true;
                error = "La dirección no es válida: deben ser 4 números del 0 al 255 separados por puntos (p. ej. 10.147.17.5).";
                return false;
            }

            if (IsHostName(address))
                return true;
            error = "La dirección no es válida: copia exactamente la que te pasó el host (p. ej. 10.147.17.5).";
            return false;
        }

        private static bool LooksNumeric(string text)
        {
            foreach (var c in text)
            {
                if (c != '.' && (c < '0' || c > '9'))
                    return false;
            }
            return true;
        }

        private static bool IsIpv4(string text)
        {
            var parts = text.Split('.');
            if (parts.Length != 4)
                return false;
            foreach (var part in parts)
            {
                if (part.Length == 0 || part.Length > 3 || !int.TryParse(part, out var value) || value > 255)
                    return false;
            }
            return true;
        }

        private static bool IsHostName(string text)
        {
            foreach (var label in text.Split('.'))
            {
                if (label.Length == 0 || label.Length > 63 || label[0] == '-' || label[label.Length - 1] == '-')
                    return false;
                foreach (var c in label)
                {
                    var ok = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '-';
                    if (!ok)
                        return false;
                }
            }
            return true;
        }
    }
}
