using NUnit.Framework;
using Trivias.Presenter.Logic;

namespace Trivias.Presenter.Tests
{
    /// <summary>Viñeta de cómic: cada momento tiene su ánimo y las letras salen a un ritmo que se puede leer.</summary>
    public sealed class SpeechStyleTests
    {
        [Test]
        public void EveryMoment_HasAMood()
        {
            Assert.That(PresenterMoods.For(PresenterEvent.Welcome), Is.EqualTo(PresenterMood.Excited));
            Assert.That(PresenterMoods.For(PresenterEvent.Victory), Is.EqualTo(PresenterMood.Excited));
            Assert.That(PresenterMoods.For(PresenterEvent.Countdown), Is.EqualTo(PresenterMood.Suspense));
            Assert.That(PresenterMoods.For(PresenterEvent.LastQuestion), Is.EqualTo(PresenterMood.Suspense));
            Assert.That(PresenterMoods.For(PresenterEvent.Eliminated), Is.EqualTo(PresenterMood.Sad));
            Assert.That(PresenterMoods.For(PresenterEvent.Defeat), Is.EqualTo(PresenterMood.Sad));
            Assert.That(PresenterMoods.For(PresenterEvent.MathQuestion), Is.EqualTo(PresenterMood.Neutral));
            for (var e = 0; e < PresenterEvents.COUNT; e++)
                Assert.That((int)PresenterMoods.For((PresenterEvent)e), Is.InRange(0, PresenterMoods.COUNT - 1));
        }

        [Test]
        public void Typing_IsSlowerInSuspense_ButAlwaysEndsInTimeToRead()
        {
            Assert.That(SpeechRhythm.CharsPerSecond(PresenterMood.Suspense, 20, 6f),
                Is.LessThan(SpeechRhythm.CharsPerSecond(PresenterMood.Excited, 20, 6f)));
            foreach (PresenterMood mood in System.Enum.GetValues(typeof(PresenterMood)))
            {
                foreach (var length in new[] { 10, 60, 120 })
                {
                    var pauses = 4 * SpeechRhythm.PauseAfter('.', mood);
                    var seconds = length / SpeechRhythm.CharsPerSecond(mood, length, 6f) + pauses;
                    Assert.That(seconds, Is.LessThanOrEqualTo(6f * 0.75f), $"{mood}, {length} letras: queda tiempo para leerla.");
                }
            }
        }

        [Test]
        public void Suspense_HoldsOnTheDots()
        {
            Assert.That(SpeechRhythm.PauseAfter('.', PresenterMood.Suspense), Is.GreaterThan(SpeechRhythm.PauseAfter('.', PresenterMood.Neutral)));
            Assert.That(SpeechRhythm.PauseAfter('a', PresenterMood.Suspense), Is.Zero);
        }

        [Test]
        public void EachLetter_PopsIn_ThenSettles()
        {
            Assert.That(SpeechRhythm.PopScale(0f), Is.EqualTo(SpeechRhythm.POP_START_SCALE).Within(0.001f));
            Assert.That(SpeechRhythm.PopScale(SpeechRhythm.POP_SECONDS * 0.5f), Is.InRange(1.01f, SpeechRhythm.POP_START_SCALE));
            Assert.That(SpeechRhythm.PopScale(1f), Is.EqualTo(1f));
            Assert.That(SpeechRhythm.IntroScale(0f), Is.EqualTo(0f).Within(0.001f));
            Assert.That(SpeechRhythm.IntroScale(0.7f), Is.GreaterThan(1f), "La viñeta rebota al entrar.");
            Assert.That(SpeechRhythm.IntroScale(1f), Is.EqualTo(1f));
        }

        [Test]
        public void Letters_MoveWithTheMood()
        {
            SpeechRhythm.LetterOffset(PresenterMood.Neutral, 3, 1.3f, out var x, out var y);
            Assert.That(x, Is.Zero);
            Assert.That(y, Is.Zero, "Normal: quietas.");
            SpeechRhythm.LetterOffset(PresenterMood.Sad, 3, 1.3f, out _, out y);
            Assert.That(y, Is.LessThanOrEqualTo(0f), "Pena: se hunden.");
            var bounced = false;
            for (var t = 0f; t < 1f; t += 0.05f)
            {
                SpeechRhythm.LetterOffset(PresenterMood.Excited, 0, t, out _, out y);
                Assert.That(y, Is.GreaterThanOrEqualTo(0f));
                bounced |= y > 0.03f;
            }
            Assert.That(bounced, Is.True, "Ánimo: botan.");
            SpeechRhythm.BubbleWobble(PresenterMood.Sad, 0f, out _, out var degrees);
            Assert.That(degrees, Is.LessThan(0f), "Pena: la viñeta se inclina.");
        }
    }
}
