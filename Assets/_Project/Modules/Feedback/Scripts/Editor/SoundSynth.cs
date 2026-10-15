using System;

namespace Trivias.Feedback.Editor
{
    public enum Wave : byte
    {
        Sine,
        Square,
        Triangle,
        Saw,
        Noise,
    }

    /// <summary>Una nota o efecto dentro de un clip. Tiempos en segundos y frecuencias en Hz.</summary>
    public struct Tone
    {
        public float Start;
        public float Duration;
        public float Frequency;

        /// <summary>Frecuencia final para barridos; 0 = igual que <see cref="Frequency"/>.</summary>
        public float FrequencyEnd;

        public Wave Wave;
        public float Volume;
        public float Attack;
        public float Release;

        /// <summary>Caída exponencial (sonidos percusivos); 0 = sostenido.</summary>
        public float Decay;

        /// <summary>Filtro paso bajo en Hz; 0 = sin filtro.</summary>
        public float LowPass;

        public float VibratoRate;
        public float VibratoDepth;

        public static Tone Note(float start, float duration, float frequency, Wave wave, float volume, float decay = 0f) => new()
        {
            Start = start,
            Duration = duration,
            Frequency = frequency,
            Wave = wave,
            Volume = volume,
            Attack = 0.005f,
            Release = Math.Min(0.04f, duration * 0.3f),
            Decay = decay,
        };
    }

    /// <summary>
    /// Sintetizador mínimo para generar sonidos en tiempo de edición (sin audio externo en el MVP).
    /// Determinista: la misma receta y semilla dan siempre las mismas muestras (los .wav no cambian entre builds).
    /// </summary>
    public sealed class SoundSynth
    {
        public const int SFX_RATE = 44100;
        public const int MUSIC_RATE = 22050;

        private readonly float[] _samples;
        private readonly int _rate;
        private readonly Random _random;

        public SoundSynth(float seconds, int sampleRate, int seed)
        {
            _rate = sampleRate;
            _samples = new float[Math.Max(1, (int)Math.Ceiling(seconds * sampleRate))];
            _random = new Random(seed);
        }

        public int SampleRate => _rate;
        public float[] Samples => _samples;

        public SoundSynth Add(Tone tone)
        {
            var first = (int)(tone.Start * _rate);
            var count = (int)(tone.Duration * _rate);
            var endFrequency = tone.FrequencyEnd > 0f ? tone.FrequencyEnd : tone.Frequency;
            var filter = tone.LowPass > 0f ? 1f - (float)Math.Exp(-2.0 * Math.PI * tone.LowPass / _rate) : 1f;
            double phase = 0;
            float filtered = 0f;

            for (var i = 0; i < count && first + i < _samples.Length; i++)
            {
                var t = (float)i / _rate;
                var progress = count > 1 ? (float)i / (count - 1) : 0f;
                var frequency = tone.Frequency + (endFrequency - tone.Frequency) * progress;
                if (tone.VibratoDepth > 0f)
                    frequency *= 1f + tone.VibratoDepth * (float)Math.Sin(2.0 * Math.PI * tone.VibratoRate * t);
                phase += 2.0 * Math.PI * frequency / _rate;

                var raw = Oscillator(tone.Wave, phase);
                filtered += filter * (raw - filtered);
                _samples[first + i] += filtered * tone.Volume * Envelope(tone, t);
            }
            return this;
        }

        /// <summary>Ajusta el volumen para que el pico sea <paramref name="peak"/> (nunca satura).</summary>
        public float[] Normalized(float peak = 0.85f)
        {
            var max = 0f;
            foreach (var sample in _samples)
                max = Math.Max(max, Math.Abs(sample));
            if (max > 0f)
            {
                var gain = peak / max;
                for (var i = 0; i < _samples.Length; i++)
                    _samples[i] *= gain;
            }
            return _samples;
        }

        private float Oscillator(Wave wave, double phase)
        {
            switch (wave)
            {
                case Wave.Square: return Math.Sin(phase) >= 0 ? 0.6f : -0.6f;
                case Wave.Triangle: return (float)(2.0 / Math.PI * Math.Asin(Math.Sin(phase)));
                case Wave.Saw:
                    var cycle = phase / (2.0 * Math.PI);
                    return (float)((cycle - Math.Floor(cycle)) * 2.0 - 1.0) * 0.6f;
                case Wave.Noise: return (float)(_random.NextDouble() * 2.0 - 1.0);
                default: return (float)Math.Sin(phase);
            }
        }

        private static float Envelope(in Tone tone, float t)
        {
            var amplitude = 1f;
            if (tone.Attack > 0f && t < tone.Attack)
                amplitude = t / tone.Attack;
            var untilEnd = tone.Duration - t;
            if (tone.Release > 0f && untilEnd < tone.Release)
                amplitude *= Math.Max(0f, untilEnd / tone.Release);
            if (tone.Decay > 0f)
                amplitude *= (float)Math.Exp(-tone.Decay * t);
            return amplitude;
        }
    }
}
