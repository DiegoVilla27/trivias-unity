using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Trivias.Shared.Characters;
using Trivias.Shared.Profile;
using UnityEngine;

namespace Trivias.Shared.Tests
{
    /// <summary>Personajes: cuáles tiene el jugador, cómo se eligen en el menú y cómo el host evita repetirlos.</summary>
    public sealed class CharacterRulesTests
    {
        private static readonly string[] All = { "a", "b", "c", "d", "e" };
        private static readonly string[] Free = { "a", "c" };

        // ---- Tenerlos ----

        [Test]
        public void FreeOnes_AreForEveryone_PaidOnes_OnlyIfBought()
        {
            var owned = new List<string> { "d" };
            Assert.That(CharacterRules.IsUnlocked("a", Free, owned), Is.True);
            Assert.That(CharacterRules.IsUnlocked("d", Free, owned), Is.True, "Comprado.");
            Assert.That(CharacterRules.IsUnlocked("b", Free, owned), Is.False, "De pago y sin comprar.");
            Assert.That(CharacterRules.IsUnlocked("", Free, owned), Is.False);
        }

        [Test]
        public void Current_IsTheSavedOne_OrTheFirstOneHeHas()
        {
            var owned = new List<string>();
            Assert.That(CharacterRules.Current("c", All, Free, owned), Is.EqualTo("c"));
            Assert.That(CharacterRules.Current("", All, Free, owned), Is.EqualTo("a"), "Sin elegir: el primero que tiene.");
            Assert.That(CharacterRules.Current("b", All, Free, owned), Is.EqualTo("a"), "Guardado pero no lo tiene.");
            Assert.That(CharacterRules.Current("borrado", All, Free, owned), Is.EqualTo("a"), "Ya no existe en el catálogo.");
        }

        // ---- Elegir en el menú ----

        [Test]
        public void Arrows_GoThroughTheOnesHeHas_AndWrapAround()
        {
            var owned = new List<string> { "e" };
            Assert.That(CharacterRules.Step("a", 1, All, Free, owned), Is.EqualTo("c"), "Salta el de pago sin comprar.");
            Assert.That(CharacterRules.Step("c", 1, All, Free, owned), Is.EqualTo("e"), "Incluye el comprado.");
            Assert.That(CharacterRules.Step("e", 1, All, Free, owned), Is.EqualTo("a"), "Da la vuelta.");
            Assert.That(CharacterRules.Step("a", -1, All, Free, owned), Is.EqualTo("e"), "Hacia atrás también.");
        }

        [Test]
        public void Arrows_WithASingleCharacter_StayOnIt()
        {
            Assert.That(CharacterRules.Step("a", 1, All, new[] { "a" }, new List<string>()), Is.EqualTo("a"));
        }

        // ---- En la sala (host) ----

        [Test]
        public void Host_GivesTheRequestedOne_IfNobodyHasIt()
        {
            Assert.That(CharacterRules.Assign("d", All, Free, new List<string> { "a" }), Is.EqualTo("d"));
        }

        [Test]
        public void Host_NeverRepeatsACharacter_GivesTheFirstFreeOneLeft()
        {
            Assert.That(CharacterRules.Assign("a", All, Free, new List<string> { "a" }), Is.EqualTo("c"));
            Assert.That(CharacterRules.Assign("a", All, Free, new List<string> { "a", "c" }), Is.EqualTo("b"),
                "Sin gratuitos libres: el primero libre del catálogo.");
            Assert.That(CharacterRules.Assign("inventado", All, Free, new List<string>()), Is.EqualTo("a"));
        }

        [Test]
        public void SixPlayers_WithSixFreeCharacters_AllGetADifferentOne()
        {
            var all = new[] { "r", "g", "b", "y", "o", "p", "x", "z" };
            var free = new[] { "r", "g", "b", "y", "o", "p" };
            var taken = new List<string>();
            for (var player = 0; player < 6; player++)
                taken.Add(CharacterRules.Assign("r", all, free, taken)); // Todos quieren el mismo.
            Assert.That(taken, Is.Unique);
            Assert.That(taken, Is.EquivalentTo(free));
        }

        // ---- Perfil ----

        [Test]
        public void Profile_ChoosesOnlyCharactersHeHas_AndSavesIt()
        {
            var folder = Path.Combine(Path.GetTempPath(), "trivias-character-tests", Path.GetRandomFileName());
            var catalog = ScriptableObject.CreateInstance<CharacterCatalog>();
            var profile = ScriptableObject.CreateInstance<PlayerProfile>();
            try
            {
                catalog.EditorSet(new List<CharacterDefinition>
                {
                    new("a", "A", Color.red), new("b", "B", Color.blue), new("c", "C", Color.green),
                }, new[] { "a", "c" }, 1200);
                profile.UseStoragePath(Path.Combine(folder, "profile.json"));

                Assert.That(profile.TryChooseCharacter(catalog, "b"), Is.False, "De pago y sin comprar.");
                Assert.That(profile.TryChooseCharacter(catalog, "c"), Is.True);
                Assert.That(profile.TryChooseCharacter(catalog, "nadie"), Is.False);
                profile.AwardMatch("m", 1200);
                profile.TryBuy("b", catalog.Price);
                Assert.That(profile.TryChooseCharacter(catalog, "b"), Is.True, "Comprado.");

                var saved = ProfileStore.Load(Path.Combine(folder, "profile.json"));
                Assert.That(saved.character, Is.EqualTo("b"), "Se recuerda para la próxima vez.");
            }
            finally
            {
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(profile);
                if (Directory.Exists(folder))
                    Directory.Delete(folder, true);
            }
        }
    }
}
