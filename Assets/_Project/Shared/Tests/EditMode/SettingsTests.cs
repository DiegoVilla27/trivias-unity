using System.IO;
using NUnit.Framework;
using Trivias.Shared.Gameplay;
using Trivias.Shared.Rooms;
using Trivias.Shared.Settings;
using UnityEngine;

namespace Trivias.Shared.Tests
{
    public sealed class SettingsTests
    {
        private string _folder;
        private GameSettings _settings;

        [SetUp]
        public void Create()
        {
            _folder = Path.Combine(Path.GetTempPath(), "trivias-settings-tests", Path.GetRandomFileName());
            _settings = ScriptableObject.CreateInstance<GameSettings>();
            _settings.UseStoragePath(Path.Combine(_folder, "settings.json"));
        }

        [TearDown]
        public void Delete()
        {
            Object.DestroyImmediate(_settings);
            if (Directory.Exists(_folder))
                Directory.Delete(_folder, true);
        }

        [Test]
        public void Defaults_AreSensible()
        {
            Assert.That(_settings.MasterVolume, Is.EqualTo(1f));
            Assert.That(_settings.MusicVolume, Is.LessThan(1f).And.GreaterThan(0f), "La música, algo más baja que el juego.");
            Assert.That(_settings.MouseSensitivity, Is.EqualTo(1f));
            Assert.That(_settings.InvertY, Is.False);
            Assert.That(_settings.Fullscreen, Is.True);
        }

        [Test]
        public void Values_AreClamped()
        {
            _settings.SetMasterVolume(3f);
            _settings.SetMusicVolume(-1f);
            _settings.SetMouseSensitivity(50f);
            Assert.That(_settings.MasterVolume, Is.EqualTo(1f));
            Assert.That(_settings.MusicVolume, Is.EqualTo(0f));
            Assert.That(_settings.MouseSensitivity, Is.EqualTo(SettingsRules.MAX_SENSITIVITY));
            _settings.SetMouseSensitivity(0f);
            Assert.That(_settings.MouseSensitivity, Is.EqualTo(SettingsRules.MIN_SENSITIVITY));
        }

        [Test]
        public void Changes_NotifyListeners()
        {
            var calls = 0;
            _settings.Changed += () => calls++;
            _settings.SetEffectsVolume(0.5f);
            _settings.SetInvertY(true);
            Assert.That(calls, Is.EqualTo(2));
        }

        [Test]
        public void Save_WritesOnlyWhenSomethingChanged_AndRoundTrips()
        {
            var path = Path.Combine(_folder, "settings.json");
            _settings.Save();
            Assert.That(File.Exists(path), Is.False, "Sin cambios no se escribe nada.");

            _settings.SetMusicVolume(0.3f);
            _settings.SetMouseSensitivity(2f);
            _settings.SetFullscreen(false);
            _settings.Save();

            var loaded = SettingsStore.Load(path);
            Assert.That(loaded.musicVolume, Is.EqualTo(0.3f).Within(0.0001f));
            Assert.That(loaded.mouseSensitivity, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(loaded.fullscreen, Is.False);
        }

        [Test]
        public void LastAddress_IsSavedAtOnce_AndSurvivesReset()
        {
            _settings.SetLastAddress("10.147.17.5");
            Assert.That(SettingsStore.Load(Path.Combine(_folder, "settings.json")).lastAddress, Is.EqualTo("10.147.17.5"));

            _settings.SetMouseSensitivity(2.5f);
            _settings.ResetToDefaults();
            Assert.That(_settings.MouseSensitivity, Is.EqualTo(1f));
            Assert.That(_settings.LastAddress, Is.EqualTo("10.147.17.5"), "Restablecer no borra la dirección.");
        }

        [Test]
        public void CorruptFile_FallsBackToDefaults()
        {
            Directory.CreateDirectory(_folder);
            var path = Path.Combine(_folder, "broken.json");
            File.WriteAllText(path, "no es json {");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("No se pudieron leer"));
            Assert.That(SettingsStore.Load(path).masterVolume, Is.EqualTo(1f));
        }

        [Test]
        public void InvertY_FlipsVerticalLook()
        {
            Assert.That(SettingsRules.LookVertical(2f, false), Is.EqualTo(2f));
            Assert.That(SettingsRules.LookVertical(2f, true), Is.EqualTo(-2f));
        }

        // ---- Pausa ----

        [Test]
        public void Pause_StopsMovement_OnlyWhileOpen()
        {
            var gate = ScriptableObject.CreateInstance<PlayerControlGate>();
            var changes = 0;
            gate.Changed += _ => changes++;

            gate.SetCanMove(true);
            Assert.That(gate.CanMove, Is.True);
            gate.SetPaused(true);
            Assert.That(gate.CanMove, Is.False, "En pausa el concursante se queda quieto.");
            gate.SetCanMove(true);
            Assert.That(gate.CanMove, Is.False, "La partida no puede saltarse la pausa.");
            gate.SetPaused(false);
            Assert.That(gate.CanMove, Is.True);
            Assert.That(changes, Is.EqualTo(3), "Solo avisa cuando cambia de verdad.");

            gate.SetCanMove(false);
            gate.SetPaused(false);
            Assert.That(gate.CanMove, Is.False, "Quitar la pausa en la sala no deja moverse.");
            Object.DestroyImmediate(gate);
        }

        // ---- Dirección para unirse ----

        [TestCase("10.147.17.5", "10.147.17.5")]
        [TestCase("  192.168.1.20 ", "192.168.1.20")]
        [TestCase("127.0.0.1", "127.0.0.1")]
        [TestCase("mi-pc.local", "mi-pc.local")]
        public void AddressInput_AcceptsIpsAndHostNames(string input, string expected)
        {
            Assert.That(RoomAddressInput.TryNormalize(input, out var address, out var error), Is.True, error);
            Assert.That(address, Is.EqualTo(expected));
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("10.147.17")]
        [TestCase("10.147.17.256")]
        [TestCase("10..17.5")]
        [TestCase("mi pc")]
        [TestCase("http://10.0.0.1")]
        public void AddressInput_RejectsInvalid_WithAnExplanation(string input)
        {
            Assert.That(RoomAddressInput.TryNormalize(input, out _, out var error), Is.False);
            Assert.That(error, Is.Not.Empty);
        }
    }
}
