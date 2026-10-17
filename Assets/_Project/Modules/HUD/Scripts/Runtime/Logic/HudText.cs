using Trivias.Match.Logic;
using Trivias.Money.Logic;

namespace Trivias.HUD.Logic
{
    /// <summary>
    /// Textos del HUD de la partida (C# puro, cubierto por tests). Las cifras que cambian cada segundo o cada fotograma
    /// de una animación (reloj, dinero que sube) se escriben en un búfer de caracteres para no reservar memoria.
    /// </summary>
    public static class HudText
    {
        public const string OPEN = "Apuestas abiertas";
        public const string LOCKED = "Apuestas cerradas";
        public const string REVEALED = "Respuesta revelada";
        public const string GOLD = "#FFD15C";
        /// <summary>Caracteres que necesita la cifra más larga (signo, 19 dígitos y sus puntos).</summary>
        public const int AMOUNT_BUFFER = 28;

        public static string Question(int number, int total) => $"PREGUNTA <color={GOLD}>{number}/{total}</color>";

        public static string State(MatchPhase phase) => phase switch
        {
            MatchPhase.Locked => LOCKED,
            MatchPhase.Reveal => REVEALED,
            _ => OPEN,
        };

        public static string AtRisk(long amount) => $"Fuera de bandejas: <b>{MoneyFormat.Amount(amount)}</b> · Se pierde al verificar";

        public static string SelfBets(int personal, int reserve) => $"Comodines <b>{personal}</b> · reserva <b>{reserve}</b>";

        public static string SelfBetTitle(int booth) => $"COMODÍN PREPARADO · {(char)('A' + booth)}";

        /// <summary>
        /// Lo que daría la cabina si es la correcta: una previsión, no dinero ganado. Sin flechas: las tipografías del
        /// juego no tienen «→».
        /// </summary>
        public static string Projection(long trayAmount, int multiplier) =>
            $"Si aciertas: {MoneyFormat.Amount(trayAmount)} × {multiplier} = <color={GOLD}>{MoneyFormat.Amount(trayAmount * multiplier)}</color>";

        public static string Name(Denomination denomination, int count) =>
            count == 1 ? MoneyFormat.Name(denomination) : MoneyFormat.Name(denomination) + "s";

        public static string Carry(Denomination denomination, int count, long amount) =>
            $"{count} {Name(denomination, count)} · {MoneyFormat.Amount(amount)}";

        /// <summary>Capacidad del tipo que se lleva (cada denominación tiene su límite).</summary>
        public static string CarryLimit(Denomination denomination, int count, int limit) =>
            count >= limit ? $"No puedes llevar más {Name(denomination, 2)}" : $"Capacidad {count}/{limit}";

        public static string Bonus(int multiplier) => $"Bono x{multiplier}";

        /// <summary>Reloj «01:12». Devuelve cuántos caracteres escribió.</summary>
        public static int WriteClock(int seconds, char[] buffer)
        {
            if (seconds < 0)
                seconds = 0;
            var minutes = seconds / 60;
            seconds %= 60;
            buffer[0] = (char)('0' + minutes / 10 % 10);
            buffer[1] = (char)('0' + minutes % 10);
            buffer[2] = ':';
            buffer[3] = (char)('0' + seconds / 10);
            buffer[4] = (char)('0' + seconds % 10);
            return 5;
        }

        /// <summary>Cantidad con puntos de miles («100.000»), con un signo delante si se pide. Devuelve la longitud.</summary>
        public static int WriteAmount(long value, char[] buffer, char sign = '\0')
        {
            var length = 0;
            if (sign != '\0')
                buffer[length++] = sign;
            if (value < 0)
                value = -value;

            // Se escriben los dígitos al revés al final del búfer y luego se copian en orden.
            var end = buffer.Length;
            var position = end;
            var digits = 0;
            do
            {
                if (digits > 0 && digits % 3 == 0)
                    buffer[--position] = '.';
                buffer[--position] = (char)('0' + value % 10);
                value /= 10;
                digits++;
            }
            while (value > 0);

            for (var i = position; i < end; i++)
                buffer[length++] = buffer[i];
            return length;
        }
    }
}
