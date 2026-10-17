using NUnit.Framework;
using Trivias.Presenter.Logic;

namespace Trivias.Presenter.Tests
{
    /// <summary>Rótulo del presentador: cada momento tiene su ánimo y las letras salen a un ritmo que se puede leer.</summary>
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
    }
}
