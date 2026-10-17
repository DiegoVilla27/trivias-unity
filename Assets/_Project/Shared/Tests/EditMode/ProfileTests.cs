using System.IO;
using NUnit.Framework;
using Trivias.Shared.Profile;

namespace Trivias.Shared.Tests
{
    public sealed class ProfileTests
    {
        private ProfileData _profile;
        private string _folder;

        [SetUp]
        public void NewProfile()
        {
            _profile = new ProfileData();
            _profile.Normalize();
            _folder = Path.Combine(Path.GetTempPath(), "trivias-profile-tests", Path.GetRandomFileName());
        }

        [TearDown]
        public void DeleteFiles()
        {
            if (Directory.Exists(_folder))
                Directory.Delete(_folder, true);
        }

        // ---- Fichas ----

        [Test]
        public void Award_AddsTokensOncePerMatch()
        {
            Assert.That(ProfileRules.AwardMatch(_profile, "m1", 150), Is.True);
            Assert.That(ProfileRules.AwardMatch(_profile, "m1", 150), Is.False, "La misma partida no entrega dos veces.");
            Assert.That(ProfileRules.AwardMatch(_profile, "m2", 50), Is.True);
            Assert.That(_profile.tokens, Is.EqualTo(200));
        }

        [Test]
        public void Award_WithZeroTokens_IsRecordedWithoutAdding()
        {
            Assert.That(ProfileRules.AwardMatch(_profile, "m1", 0), Is.True);
            Assert.That(_profile.tokens, Is.Zero);
        }

        [Test]
        public void Award_HistoryIsBounded()
        {
            for (var i = 0; i < ProfileRules.AWARD_HISTORY + 20; i++)
                ProfileRules.AwardMatch(_profile, $"m{i}", 1);
            Assert.That(_profile.awardedMatches.Count, Is.EqualTo(ProfileRules.AWARD_HISTORY));
        }

        // ---- Compras ----

        [Test]
        public void Buy_SpendsTokensAndOwnsItem()
        {
            _profile.tokens = 700;
            Assert.That(ProfileRules.TryBuy(_profile, "gorra", 300), Is.True);
            Assert.That(_profile.tokens, Is.EqualTo(400));
            Assert.That(ProfileRules.Owns(_profile, "gorra"), Is.True);
        }

        [Test]
        public void Buy_FailsWithoutEnoughTokens()
        {
            _profile.tokens = 299;
            Assert.That(ProfileRules.TryBuy(_profile, "gorra", 300), Is.False);
            Assert.That(_profile.tokens, Is.EqualTo(299));
        }

        [Test]
        public void Buy_FailsIfAlreadyOwned()
        {
            _profile.tokens = 1000;
            ProfileRules.TryBuy(_profile, "gorra", 300);
            Assert.That(ProfileRules.TryBuy(_profile, "gorra", 300), Is.False, "Las compras son permanentes: no se compra dos veces.");
            Assert.That(_profile.tokens, Is.EqualTo(700));
        }

        // ---- Equipar ----

        [Test]
        public void Equip_RequiresOwnership()
        {
            Assert.That(ProfileRules.TryEquip(_profile, CosmeticSlot.Head, "corona"), Is.False);
            Assert.That(_profile.Loadout.Head, Is.Empty);
        }

        [Test]
        public void Equip_ReplacesItemInSameSlot_AndUnequipClears()
        {
            _profile.tokens = 5000;
            ProfileRules.TryBuy(_profile, "gorra", 300);
            ProfileRules.TryBuy(_profile, "corona", 1200);
            ProfileRules.TryBuy(_profile, "gafas", 300);
            ProfileRules.TryEquip(_profile, CosmeticSlot.Head, "gorra");
            ProfileRules.TryEquip(_profile, CosmeticSlot.Head, "corona");
            ProfileRules.TryEquip(_profile, CosmeticSlot.Face, "gafas");

            Assert.That(_profile.Loadout.Head, Is.EqualTo("corona"), "Un solo objeto por hueco.");
            Assert.That(_profile.Loadout.Face, Is.EqualTo("gafas"));

            ProfileRules.Unequip(_profile, CosmeticSlot.Head);
            Assert.That(_profile.Loadout.Head, Is.Empty);
            Assert.That(ProfileRules.Owns(_profile, "corona"), Is.True, "Quitar no vende el objeto.");
        }

        // ---- Guardado ----

        [Test]
        public void Store_RoundTripsProfile()
        {
            var path = Path.Combine(_folder, "profile.json");
            _profile.tokens = 450;
            ProfileRules.AwardMatch(_profile, "m9", 0);
            _profile.owned.Add("gafas");
            ProfileRules.TryEquip(_profile, CosmeticSlot.Face, "gafas");
            ProfileStore.Save(path, _profile);

            var loaded = ProfileStore.Load(path);
            Assert.That(loaded.tokens, Is.EqualTo(450));
            Assert.That(loaded.owned, Does.Contain("gafas"));
            Assert.That(loaded.Loadout.Face, Is.EqualTo("gafas"));
            Assert.That(loaded.awardedMatches, Does.Contain("m9"));
        }

        [Test]
        public void Store_MissingFile_GivesEmptyProfile()
        {
            var loaded = ProfileStore.Load(Path.Combine(_folder, "nope.json"));
            Assert.That(loaded.tokens, Is.Zero);
            Assert.That(loaded.equipped.Length, Is.EqualTo(CosmeticSlots.COUNT));
        }

        [Test]
        public void Store_CorruptFile_GivesEmptyProfileWithoutCrashing()
        {
            Directory.CreateDirectory(_folder);
            var path = Path.Combine(_folder, "broken.json");
            File.WriteAllText(path, "{ esto no es json");
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Warning, new System.Text.RegularExpressions.Regex("No se pudo leer"));
            var loaded = ProfileStore.Load(path);
            Assert.That(loaded.tokens, Is.Zero);
        }

        [Test]
        public void FiveSlots_MouthAndBackToo_AndAnOldThreeSlotFileStillLoads()
        {
            _profile.owned.AddRange(new[] { "bigote", "alas-angel", "corona" });
            Assert.That(ProfileRules.TryEquip(_profile, CosmeticSlot.Mouth, "bigote"), Is.True);
            Assert.That(ProfileRules.TryEquip(_profile, CosmeticSlot.Back, "alas-angel"), Is.True);
            Assert.That(ProfileRules.TryEquip(_profile, CosmeticSlot.Head, "corona"), Is.True);
            var loadout = _profile.Loadout;
            Assert.That(loadout[CosmeticSlot.Mouth], Is.EqualTo("bigote"));
            Assert.That(loadout[CosmeticSlot.Back], Is.EqualTo("alas-angel"));
            Assert.That(loadout[CosmeticSlot.Head], Is.EqualTo("corona"));
            Assert.That(loadout, Is.Not.EqualTo(new CosmeticLoadout("corona", "", "", "bigote", "")), "Cuenta también la espalda.");

            // Un perfil guardado antes, con tres huecos: conserva lo puesto y gana los dos nuevos vacíos.
            var old = new ProfileData { equipped = new[] { "corona", "gafas-sol", "pajarita" } };
            old.Normalize();
            Assert.That(old.equipped, Is.EqualTo(new[] { "corona", "gafas-sol", "pajarita", null, null }));
            Assert.That(old.Loadout.Mouth, Is.Empty);
            Assert.That(old.Loadout.Back, Is.Empty);
        }
    }
}
