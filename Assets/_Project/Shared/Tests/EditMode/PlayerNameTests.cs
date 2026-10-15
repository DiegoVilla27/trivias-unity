using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Trivias.Shared.Profile;
using UnityEngine;

namespace Trivias.Shared.Tests
{
    /// <summary>Nombre del concursante: limpio, corto, único en la sala y guardado en el perfil.</summary>
    public sealed class PlayerNameTests
    {
        private string _folder;

        [SetUp]
        public void NewFolder() => _folder = Path.Combine(Path.GetTempPath(), "trivias-name-tests", Path.GetRandomFileName());

        [TearDown]
        public void DeleteFiles()
        {
            if (Directory.Exists(_folder))
                Directory.Delete(_folder, true);
        }

        [Test]
        public void Sanitize_TrimsJoinsSpacesAndCutsLongNames()
        {
            Assert.That(PlayerNameRules.Sanitize("   Ana    María  "), Is.EqualTo("Ana María"));
            Assert.That(PlayerNameRules.Sanitize("Supercalifragilístico"), Has.Length.EqualTo(PlayerNameRules.MAX_LENGTH));
            Assert.That(PlayerNameRules.Sanitize("Quince letras y más"), Is.EqualTo("Quince letras y"), "Sin espacio final.");
            Assert.That(PlayerNameRules.Sanitize(null), Is.Empty);
            Assert.That(PlayerNameRules.Sanitize("   "), Is.Empty);
        }

        [Test]
        public void Sanitize_KeepsAccents_AndDropsTagsAndSymbolsTheFontLacks()
        {
            Assert.That(PlayerNameRules.Sanitize("José-Ñandú_1."), Is.EqualTo("José-Ñandú_1."));
            Assert.That(PlayerNameRules.Sanitize("<b>Rojo</b>"), Does.Not.Contain("<").And.Not.Contain(">"),
                "Sin etiquetas de texto enriquecido.");
            Assert.That(PlayerNameRules.Sanitize("Ana\U0001F600"), Is.EqualTo("Ana"), "Sin emojis.");
            Assert.That(PlayerNameRules.Sanitize("日本"), Is.Empty, "Solo letras latinas (las que tiene la fuente).");
        }

        [Test]
        public void WithoutAName_TheContestantIsJugadorN()
        {
            Assert.That(PlayerNameRules.DisplayName(string.Empty, 2), Is.EqualTo("Jugador 3"));
            Assert.That(PlayerNameRules.DisplayName("Eva", 2), Is.EqualTo("Eva"));
        }

        [Test]
        public void RepeatedNames_GetANumber_WithoutGettingTooLong()
        {
            Assert.That(PlayerNameRules.Unique("Ana", new List<string> { "Luis" }), Is.EqualTo("Ana"));
            Assert.That(PlayerNameRules.Unique("Ana", new List<string> { "ana" }), Is.EqualTo("Ana 2"), "Sin distinguir mayúsculas.");
            Assert.That(PlayerNameRules.Unique("Ana", new List<string> { "Ana", "Ana 2" }), Is.EqualTo("Ana 3"));

            var longName = new string('x', PlayerNameRules.MAX_LENGTH);
            var unique = PlayerNameRules.Unique(longName, new List<string> { longName });
            Assert.That(unique, Has.Length.LessThanOrEqualTo(PlayerNameRules.MAX_LENGTH));
            Assert.That(unique, Does.EndWith(" 2"));
        }

        [Test]
        public void Profile_SavesTheCleanName_AndLoadsIt()
        {
            var path = Path.Combine(_folder, "profile.json");
            var profile = ScriptableObject.CreateInstance<PlayerProfile>();
            var other = ScriptableObject.CreateInstance<PlayerProfile>();
            try
            {
                profile.UseStoragePath(path);
                Assert.That(profile.PlayerName, Is.Empty, "Sin nombre hasta que lo elige.");
                Assert.That(profile.SetName("  Eva  "), Is.True);
                Assert.That(profile.PlayerName, Is.EqualTo("Eva"));
                Assert.That(profile.SetName("Eva"), Is.False, "Mismo nombre: no guarda otra vez.");

                other.UseStoragePath(path);
                Assert.That(other.PlayerName, Is.EqualTo("Eva"));
            }
            finally
            {
                Object.DestroyImmediate(profile);
                Object.DestroyImmediate(other);
            }
        }

        [Test]
        public void Profile_EditedByHand_IsCleanedOnLoad()
        {
            Directory.CreateDirectory(_folder);
            var path = Path.Combine(_folder, "edited.json");
            File.WriteAllText(path, "{\"name\":\"  <i>Ana</i>   con   espacios largos de más  \",\"tokens\":5}");
            var loaded = ProfileStore.Load(path);
            Assert.That(loaded.name, Has.Length.LessThanOrEqualTo(PlayerNameRules.MAX_LENGTH));
            Assert.That(loaded.name, Does.Not.Contain("<").And.Not.StartWith(" "));
            Assert.That(loaded.tokens, Is.EqualTo(5));
        }
    }
}
