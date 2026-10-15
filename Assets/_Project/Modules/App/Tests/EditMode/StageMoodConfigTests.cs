using NUnit.Framework;
using Trivias.Feedback.Data;
using Trivias.Feedback.Logic;
using UnityEditor;

namespace Trivias.App.Tests
{
    /// <summary>Los ambientes de luz construidos tienen sentido: el plató se oscurece con la dificultad y brilla al ganar.</summary>
    public sealed class StageMoodConfigTests
    {
        private static StageMoodConfig Config() =>
            AssetDatabase.LoadAssetAtPath<StageMoodConfig>("Assets/_Project/Modules/Feedback/Config/StageMoodConfig.asset");

        [Test]
        public void EveryMood_IsDefined()
        {
            var config = Config();
            Assert.That(config.Count, Is.EqualTo(StageMoods.COUNT));
            for (var i = 0; i < StageMoods.COUNT; i++)
            {
                var mood = config.For((StageMood)i);
                Assert.That(mood.SpotIntensity, Is.GreaterThan(0f), ((StageMood)i).ToString());
                Assert.That(mood.KeyLightScale, Is.GreaterThan(0f), ((StageMood)i).ToString());
            }
        }

        [Test]
        public void Stage_GetsDarkerWithDifficulty()
        {
            var config = Config();
            Assert.That(config.For(StageMood.Hard).KeyLightScale, Is.LessThan(config.For(StageMood.Medium).KeyLightScale));
            Assert.That(config.For(StageMood.Medium).KeyLightScale, Is.LessThan(config.For(StageMood.Easy).KeyLightScale));
        }

        [Test]
        public void Victory_IsTheBrightestMoment_AndDefeatTheDarkest()
        {
            var config = Config();
            for (var i = 0; i < StageMoods.COUNT; i++)
            {
                Assert.That(config.For(StageMood.Victory).SpotIntensity, Is.GreaterThanOrEqualTo(config.For((StageMood)i).SpotIntensity));
                Assert.That(config.For(StageMood.Defeat).KeyLightScale, Is.LessThanOrEqualTo(config.For((StageMood)i).KeyLightScale));
            }
        }

        [Test]
        public void CorrectIsGreen_WrongIsRed()
        {
            var correct = Config().For(StageMood.Correct).SpotColor;
            var wrong = Config().For(StageMood.Wrong).SpotColor;
            Assert.That(correct.g, Is.GreaterThan(correct.r));
            Assert.That(wrong.r, Is.GreaterThan(wrong.g));
        }
    }
}
