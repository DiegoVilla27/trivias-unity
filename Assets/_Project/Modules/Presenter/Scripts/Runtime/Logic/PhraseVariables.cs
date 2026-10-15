using System.Text;
using Trivias.Money.Logic;

namespace Trivias.Presenter.Logic
{
    /// <summary>Datos públicos del concurso con los que se rellenan las frases (GDD 9.1).</summary>
    public readonly struct PhraseVariables
    {
        public static readonly string[] Allowed = { "{cantidad}", "{premio}", "{supervivientes}", "{ronda}", "{multiplicador}" };

        public readonly long Amount;
        public readonly long Prize;
        public readonly int Survivors;
        public readonly int Round;
        public readonly int Multiplier;

        public PhraseVariables(long amount, long prize, int survivors, int round, int multiplier)
        {
            Amount = amount;
            Prize = prize;
            Survivors = survivors;
            Round = round;
            Multiplier = multiplier;
        }

        /// <summary>Sustituye las variables del texto. Se usa solo cuando habla el presentador, nunca por frame.</summary>
        public string Format(string text) =>
            new StringBuilder(text)
                .Replace("{cantidad}", MoneyFormat.Amount(Amount))
                .Replace("{premio}", MoneyFormat.Amount(Prize))
                .Replace("{supervivientes}", Survivors.ToString())
                .Replace("{ronda}", Round.ToString())
                .Replace("{multiplicador}", Multiplier.ToString())
                .ToString();
    }
}
