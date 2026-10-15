using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Trivias.Feedback.Editor;
using Trivias.Feedback.Logic;

namespace Trivias.Feedback.Tests
{
    public sealed class SoundSynthTests
    {
        private static float Peak(float[] samples) => samples.Max(Math.Abs);

        [Test]
        public void Synth_HasExpectedLength()
        {
            var synth = new SoundSynth(0.5f, 1000, 1);
            Assert.That(synth.Samples.Length, Is.EqualTo(500));
        }

        [Test]
        public void Normalized_NeverClips()
        {
            var synth = new SoundSynth(0.2f, SoundSynth.SFX_RATE, 1)
                .Add(Tone.Note(0f, 0.2f, 440f, Wave.Square, 5f))
                .Add(Tone.Note(0f, 0.2f, 660f, Wave.Saw, 5f));
            Assert.That(Peak(synth.Normalized(0.85f)), Is.EqualTo(0.85f).Within(0.001f));
        }

        [Test]
        public void SameRecipe_GivesSameSamples()
        {
            var a = SoundRecipes.Cue(FeedbackCue.Lock).Normalized();
            var b = SoundRecipes.Cue(FeedbackCue.Lock).Normalized();
            Assert.That(a, Is.EqualTo(b), "Determinista: los .wav no cambian entre builds.");
        }

        [Test]
        public void EveryCue_IsAudible_Short_AndEndsInSilence()
        {
            for (var i = 0; i < FeedbackCues.COUNT; i++)
            {
                var cue = (FeedbackCue)i;
                var synth = SoundRecipes.Cue(cue);
                var samples = synth.Normalized();
                Assert.That(Peak(samples), Is.GreaterThan(0.5f), $"{cue}: no suena.");
                Assert.That(samples.Length / (float)synth.SampleRate, Is.LessThanOrEqualTo(3f), $"{cue}: demasiado largo.");
                Assert.That(Math.Abs(samples[^1]), Is.LessThan(0.02f), $"{cue}: termina con un chasquido.");
            }
        }

        [Test]
        public void MusicLoops_AreLongEnough_AndLoopWithoutClicks()
        {
            for (var block = 0; block < FeedbackRules.MUSIC_BLOCKS; block++)
            {
                var synth = SoundRecipes.Music(block);
                var samples = synth.Normalized(0.8f);
                Assert.That(samples.Length / (float)synth.SampleRate, Is.GreaterThan(5f), $"Bloque {block}: bucle demasiado corto.");
                Assert.That(Math.Abs(samples[0]), Is.LessThan(0.02f), $"Bloque {block}: empieza con chasquido.");
                Assert.That(Math.Abs(samples[^1]), Is.LessThan(0.02f), $"Bloque {block}: el bucle salta al repetirse.");
            }
        }

        [Test]
        public void MusicBlocks_GetFasterWithDifficulty() =>
            Assert.That(SoundRecipes.Music(2).Samples.Length, Is.LessThan(SoundRecipes.Music(0).Samples.Length),
                "Mismos compases a más tempo: la pista difícil es más corta.");

        [Test]
        public void Wav_HasValidPcmHeader()
        {
            var bytes = WavEncoder.Encode(new[] { 0f, 1f, -1f, 0.5f }, 22050);
            Assert.That(Encoding.ASCII.GetString(bytes, 0, 4), Is.EqualTo("RIFF"));
            Assert.That(Encoding.ASCII.GetString(bytes, 8, 4), Is.EqualTo("WAVE"));
            Assert.That(BitConverter.ToInt32(bytes, 24), Is.EqualTo(22050));
            Assert.That(BitConverter.ToInt16(bytes, 34), Is.EqualTo(16));
            Assert.That(BitConverter.ToInt32(bytes, 40), Is.EqualTo(8), "4 muestras de 2 bytes.");
            Assert.That(BitConverter.ToInt16(bytes, 46), Is.EqualTo(short.MaxValue));
            Assert.That(bytes.Length, Is.EqualTo(44 + 8));
        }
    }
}
