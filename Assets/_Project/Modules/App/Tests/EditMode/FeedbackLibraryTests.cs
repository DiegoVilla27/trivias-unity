using NUnit.Framework;
using Trivias.Feedback.Data;
using Trivias.Feedback.Logic;
using UnityEditor;

namespace Trivias.App.Tests
{
    /// <summary>La biblioteca construida tiene un sonido para cada momento y música para cada bloque.</summary>
    public sealed class FeedbackLibraryTests
    {
        private static FeedbackLibrary Library() =>
            AssetDatabase.LoadAssetAtPath<FeedbackLibrary>("Assets/_Project/Modules/Feedback/Config/FeedbackLibrary.asset");

        [Test]
        public void EveryCue_HasAClipAndVolume()
        {
            var library = Library();
            for (var i = 0; i < FeedbackCues.COUNT; i++)
            {
                var cue = (FeedbackCue)i;
                Assert.That(library.Clip(cue), Is.Not.Null, $"Falta el sonido de {cue}.");
                Assert.That(library.Clip(cue).length, Is.GreaterThan(0.02f).And.LessThanOrEqualTo(3f), cue.ToString());
                Assert.That(library.Volume(cue), Is.GreaterThan(0f).And.LessThanOrEqualTo(1f), cue.ToString());
            }
        }

        [Test]
        public void Stage_HasSuctionAndCrowdSounds()
        {
            var library = AssetDatabase.LoadAssetAtPath<FeedbackLibrary>("Assets/_Project/Modules/Feedback/Config/FeedbackLibrary.asset");
            foreach (var clip in new[] { library.Suction, library.Applause, library.Cheer, library.Aww })
            {
                Assert.That(clip, Is.Not.Null);
                Assert.That(clip.length, Is.GreaterThan(1f), $"{clip.name}: dura lo que la succión o la reacción.");
            }
            Assert.That(library.CrowdVolume, Is.GreaterThan(0f));
        }

        [Test]
        public void EveryDifficultyBlock_HasMusic()
        {
            var library = Library();
            Assert.That(library.MusicCount, Is.EqualTo(FeedbackRules.MUSIC_BLOCKS));
            for (var block = 0; block < FeedbackRules.MUSIC_BLOCKS; block++)
                Assert.That(library.Music(block).length, Is.GreaterThan(5f), $"Bloque {block}.");
            Assert.That(library.MusicVolume, Is.GreaterThan(0f).And.LessThan(0.5f), "La música va por debajo del juego.");
        }
    }
}
