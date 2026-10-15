using System;
using System.IO;
using System.Text;

namespace Trivias.Feedback.Editor
{
    /// <summary>Codifica muestras en un .wav PCM de 16 bits mono (lo importa Unity como AudioClip).</summary>
    public static class WavEncoder
    {
        public static byte[] Encode(float[] samples, int sampleRate)
        {
            using var stream = new MemoryStream(44 + samples.Length * 2);
            using var writer = new BinaryWriter(stream);
            var dataBytes = samples.Length * 2;

            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataBytes);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);                 // Tamaño del bloque fmt
            writer.Write((short)1);           // PCM
            writer.Write((short)1);           // Mono
            writer.Write(sampleRate);
            writer.Write(sampleRate * 2);     // Bytes por segundo
            writer.Write((short)2);           // Bytes por muestra
            writer.Write((short)16);          // Bits por muestra
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(dataBytes);
            foreach (var sample in samples)
                writer.Write((short)Math.Round(Math.Clamp(sample, -1f, 1f) * short.MaxValue));

            writer.Flush();
            return stream.ToArray();
        }
    }
}
