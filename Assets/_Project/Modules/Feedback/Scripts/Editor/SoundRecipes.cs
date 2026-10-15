using System;
using Trivias.Feedback.Logic;

namespace Trivias.Feedback.Editor
{
    /// <summary>Recetas de cada sonido y de las tres pistas de música (fácil, media, difícil).</summary>
    public static class SoundRecipes
    {
        // Notas (Hz).
        private const float G3 = 196.00f, FS3 = 185.00f, F3 = 174.61f, E3 = 164.81f;
        private const float BB3 = 233.08f, A3 = 220.00f, AB3 = 207.65f;
        private const float G4 = 392.00f, C5 = 523.25f, E5 = 659.25f, G5 = 783.99f, C6 = 1046.50f;
        private const float E6 = 1318.51f, G6 = 1567.98f, B6 = 1975.53f, C7 = 2093.00f, E7 = 2637.02f;

        public static SoundSynth Cue(FeedbackCue cue)
        {
            var seed = 1000 + (int)cue;
            switch (cue)
            {
                case FeedbackCue.MoneyPick:
                    return new SoundSynth(0.16f, SoundSynth.SFX_RATE, seed)
                        .Add(new Tone { Duration = 0.05f, Wave = Wave.Noise, Volume = 0.35f, LowPass = 3000f, Release = 0.03f })
                        .Add(new Tone { Start = 0.02f, Duration = 0.12f, Frequency = 600f, FrequencyEnd = 1100f, Wave = Wave.Sine, Volume = 0.5f, Attack = 0.005f, Decay = 20f, Release = 0.02f });
                case FeedbackCue.MoneyDrop:
                    return new SoundSynth(0.22f, SoundSynth.SFX_RATE, seed)
                        .Add(new Tone { Duration = 0.2f, Frequency = 160f, FrequencyEnd = 70f, Wave = Wave.Sine, Volume = 0.9f, Attack = 0.003f, Decay = 18f, Release = 0.03f })
                        .Add(new Tone { Duration = 0.06f, Wave = Wave.Noise, Volume = 0.3f, LowPass = 1200f, Release = 0.03f });
                case FeedbackCue.BetPlaced:
                    return new SoundSynth(0.9f, SoundSynth.SFX_RATE, seed)
                        .Add(Tone.Note(0f, 0.85f, E6, Wave.Sine, 0.5f, 5f))
                        .Add(Tone.Note(0f, 0.6f, E7, Wave.Sine, 0.2f, 8f))
                        .Add(Tone.Note(0.08f, 0.8f, B6, Wave.Sine, 0.35f, 5f));
                case FeedbackCue.QuestionShown:
                    return new SoundSynth(0.75f, SoundSynth.SFX_RATE, seed)
                        .Add(Tone.Note(0.0f, 0.35f, C5, Wave.Triangle, 0.45f, 6f))
                        .Add(Tone.Note(0.1f, 0.35f, E5, Wave.Triangle, 0.45f, 6f))
                        .Add(Tone.Note(0.2f, 0.35f, G5, Wave.Triangle, 0.45f, 6f))
                        .Add(Tone.Note(0.3f, 0.42f, C6, Wave.Triangle, 0.45f, 6f));
                case FeedbackCue.ClockTick:
                    return new SoundSynth(0.06f, SoundSynth.SFX_RATE, seed)
                        .Add(new Tone { Duration = 0.012f, Wave = Wave.Noise, Volume = 0.5f, LowPass = 6000f, Release = 0.006f })
                        .Add(new Tone { Duration = 0.05f, Frequency = 2000f, Wave = Wave.Sine, Volume = 0.35f, Attack = 0.001f, Decay = 80f, Release = 0.01f });
                case FeedbackCue.Lock:
                    return new SoundSynth(0.45f, SoundSynth.SFX_RATE, seed)
                        .Add(new Tone { Duration = 0.12f, Wave = Wave.Noise, Volume = 0.8f, LowPass = 500f, Release = 0.05f })
                        .Add(new Tone { Duration = 0.35f, Frequency = 90f, FrequencyEnd = 55f, Wave = Wave.Square, Volume = 0.5f, Attack = 0.002f, Decay = 10f, Release = 0.05f })
                        .Add(new Tone { Duration = 0.42f, Frequency = 55f, Wave = Wave.Sine, Volume = 0.6f, Attack = 0.002f, Decay = 8f, Release = 0.05f });
                case FeedbackCue.CountdownBeep:
                    return new SoundSynth(0.18f, SoundSynth.SFX_RATE, seed)
                        .Add(Tone.Note(0f, 0.16f, 880f, Wave.Square, 0.4f));
                case FeedbackCue.Trapdoor:
                    return new SoundSynth(0.9f, SoundSynth.SFX_RATE, seed)
                        .Add(new Tone { Duration = 0.85f, Wave = Wave.Noise, Volume = 0.5f, LowPass = 900f, Attack = 0.1f, Release = 0.4f })
                        .Add(new Tone { Duration = 0.8f, Frequency = 600f, FrequencyEnd = 80f, Wave = Wave.Sine, Volume = 0.35f, Attack = 0.01f, Release = 0.2f });
                case FeedbackCue.Success:
                    return new SoundSynth(0.95f, SoundSynth.SFX_RATE, seed)
                        .Add(Tone.Note(0.00f, 0.2f, C5, Wave.Square, 0.22f)).Add(Tone.Note(0.00f, 0.2f, C5, Wave.Triangle, 0.35f))
                        .Add(Tone.Note(0.09f, 0.2f, E5, Wave.Square, 0.22f)).Add(Tone.Note(0.09f, 0.2f, E5, Wave.Triangle, 0.35f))
                        .Add(Tone.Note(0.18f, 0.2f, G5, Wave.Square, 0.22f)).Add(Tone.Note(0.18f, 0.2f, G5, Wave.Triangle, 0.35f))
                        .Add(Tone.Note(0.27f, 0.6f, C6, Wave.Square, 0.22f, 2f)).Add(Tone.Note(0.27f, 0.6f, C6, Wave.Triangle, 0.35f, 2f));
                case FeedbackCue.Fail:
                    return Trombone(seed, 0.3f, G3, FS3, F3, E3);
                case FeedbackCue.Expelled:
                    return new SoundSynth(0.9f, SoundSynth.SFX_RATE, seed)
                        .Add(new Tone { Duration = 0.85f, Frequency = 500f, FrequencyEnd = 120f, Wave = Wave.Saw, Volume = 0.45f, LowPass = 1500f, VibratoRate = 9f, VibratoDepth = 0.03f, Attack = 0.01f, Release = 0.15f });
                case FeedbackCue.Bonus:
                    var bonus = new SoundSynth(0.6f, SoundSynth.SFX_RATE, seed);
                    var notes = new[] { C6, E6, G6, C7, E7 };
                    for (var i = 0; i < notes.Length; i++)
                        bonus.Add(Tone.Note(i * 0.06f, 0.3f, notes[i], Wave.Sine, 0.35f, 10f));
                    return bonus;
                case FeedbackCue.Victory:
                    return new SoundSynth(2.6f, SoundSynth.SFX_RATE, seed)
                        .Add(Tone.Note(0.00f, 0.15f, G4, Wave.Square, 0.25f)).Add(Tone.Note(0.15f, 0.15f, C5, Wave.Square, 0.25f))
                        .Add(Tone.Note(0.30f, 0.15f, E5, Wave.Square, 0.25f)).Add(Tone.Note(0.45f, 0.35f, G5, Wave.Square, 0.25f))
                        .Add(Tone.Note(0.80f, 0.15f, E5, Wave.Square, 0.25f)).Add(Tone.Note(0.95f, 1.5f, G5, Wave.Square, 0.25f, 1f))
                        .Add(Tone.Note(0.95f, 1.5f, C5, Wave.Triangle, 0.3f, 1f)).Add(Tone.Note(0.95f, 1.5f, E5, Wave.Triangle, 0.3f, 1f));
                case FeedbackCue.Defeat:
                    return Trombone(seed, 0.45f, BB3, A3, AB3, G3);
                default:
                    throw new ArgumentOutOfRangeException(nameof(cue), cue, null);
            }
        }

        /// <summary>«Trombón triste»: tres notas que bajan y una última larga con vibrato.</summary>
        private static SoundSynth Trombone(int seed, float step, float a, float b, float c, float last)
        {
            var synth = new SoundSynth(step * 3f + step * 2.4f + 0.05f, SoundSynth.SFX_RATE, seed);
            var notes = new[] { a, b, c };
            for (var i = 0; i < notes.Length; i++)
                synth.Add(new Tone { Start = i * step, Duration = step, Frequency = notes[i], Wave = Wave.Saw, Volume = 0.45f, LowPass = 1100f, Attack = 0.02f, Release = 0.05f });
            synth.Add(new Tone { Start = step * 3f, Duration = step * 2.4f, Frequency = last, Wave = Wave.Saw, Volume = 0.45f, LowPass = 1100f, Attack = 0.02f, Release = 0.3f, VibratoRate = 5f, VibratoDepth = 0.03f });
            return synth;
        }

        // ---- Música ----

        private readonly struct Chord
        {
            public readonly float Root;
            public readonly bool Minor;
            public Chord(float root, bool minor) { Root = root; Minor = minor; }
        }

        private const int BARS = 4;
        private const int BEATS_PER_BAR = 4;

        /// <summary>Bucle de 4 compases. Cada nota termina dentro del bucle, así que se repite sin chasquidos.</summary>
        public static SoundSynth Music(int block)
        {
            switch (block)
            {
                case 0: // Fácil: alegre y tranquila (I-vi-IV-V en Do).
                    return Loop(100, Wave.Triangle, kick: false, hats: false, 2000 + block,
                        new Chord(130.81f, false), new Chord(110.00f, true), new Chord(87.31f, false), new Chord(98.00f, false));
                case 1: // Media: más ritmo, en La menor.
                    return Loop(116, Wave.Square, kick: true, hats: false, 2000 + block,
                        new Chord(110.00f, true), new Chord(87.31f, false), new Chord(130.81f, false), new Chord(98.00f, false));
                case 2: // Difícil: tensa y rápida, en Re menor.
                    return Loop(132, Wave.Saw, kick: true, hats: true, 2000 + block,
                        new Chord(146.83f, true), new Chord(116.54f, false), new Chord(130.81f, false), new Chord(110.00f, false));
                default:
                    throw new ArgumentOutOfRangeException(nameof(block), block, null);
            }
        }

        public static float MusicSeconds(int bpm) => BARS * BEATS_PER_BAR * 60f / bpm;

        private static SoundSynth Loop(int bpm, Wave lead, bool kick, bool hats, int seed, params Chord[] chords)
        {
            var beat = 60f / bpm;
            var eighth = beat * 0.5f;
            var synth = new SoundSynth(MusicSeconds(bpm), SoundSynth.MUSIC_RATE, seed);
            var arpeggio = new[] { 0, 1, 2, 3, 2, 1, 2, 1 };

            for (var bar = 0; bar < BARS; bar++)
            {
                var chord = chords[bar];
                var barStart = bar * BEATS_PER_BAR * beat;
                var third = chord.Minor ? 1.1892f : 1.2599f;
                var tones = new[] { 1f, third, 1.4983f, 2f };

                // Bajo en los tiempos 1 y 3.
                for (var b = 0; b < BEATS_PER_BAR; b += 2)
                    synth.Add(Tone.Note(barStart + b * beat, beat * 1.8f, chord.Root * 0.5f, Wave.Triangle, 0.5f, 1.5f));

                // Arpegio en corcheas, dos octavas por encima.
                for (var e = 0; e < 8; e++)
                {
                    var note = Tone.Note(barStart + e * eighth, eighth * 0.8f, chord.Root * 2f * tones[arpeggio[e]], lead, 0.16f, 5f);
                    if (lead == Wave.Saw)
                        note.LowPass = 2200f;
                    synth.Add(note);
                }

                for (var b = 0; b < BEATS_PER_BAR; b++)
                {
                    if (kick)
                        synth.Add(new Tone { Start = barStart + b * beat, Duration = 0.15f, Frequency = 120f, FrequencyEnd = 45f, Wave = Wave.Sine, Volume = 0.55f, Attack = 0.002f, Decay = 18f, Release = 0.03f });
                }

                if (hats)
                {
                    for (var e = 0; e < 8; e++)
                        synth.Add(new Tone { Start = barStart + e * eighth + eighth * 0.5f, Duration = 0.03f, Wave = Wave.Noise, Volume = 0.1f, Attack = 0.001f, Release = 0.02f });
                }
            }
            return synth;
        }
    }
}
