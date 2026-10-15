using System;

namespace Trivias.Questions.Logic
{
    /// <summary>
    /// Orden de las 4 opciones en las cabinas A-D, empaquetado en un byte (2 bits por cabina) para la red.
    /// La cabina i muestra la opción original <c>OriginalAt(i)</c>.
    /// </summary>
    public readonly struct OptionOrder : IEquatable<OptionOrder>
    {
        public const int COUNT = 4;
        public static readonly OptionOrder Identity = FromPermutation(0, 1, 2, 3);

        public readonly byte Packed;

        public OptionOrder(byte packed) => Packed = packed;

        public int OriginalAt(int booth) => (Packed >> (booth * 2)) & 0b11;

        /// <summary>Cabina en la que aparece una opción original (p. ej. la correcta).</summary>
        public int BoothOf(int originalOption)
        {
            for (var booth = 0; booth < COUNT; booth++)
            {
                if (OriginalAt(booth) == originalOption)
                    return booth;
            }
            return -1;
        }

        public static OptionOrder FromPermutation(int a, int b, int c, int d) =>
            new((byte)(a | (b << 2) | (c << 4) | (d << 6)));

        public static OptionOrder Shuffle(Random random)
        {
            Span<int> values = stackalloc int[] { 0, 1, 2, 3 };
            for (var i = COUNT - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (values[i], values[j]) = (values[j], values[i]);
            }
            return FromPermutation(values[0], values[1], values[2], values[3]);
        }

        public bool IsValidPermutation()
        {
            var seen = 0;
            for (var booth = 0; booth < COUNT; booth++)
                seen |= 1 << OriginalAt(booth);
            return seen == 0b1111;
        }

        public bool Equals(OptionOrder other) => Packed == other.Packed;
        public override bool Equals(object obj) => obj is OptionOrder other && Equals(other);
        public override int GetHashCode() => Packed;
    }
}
