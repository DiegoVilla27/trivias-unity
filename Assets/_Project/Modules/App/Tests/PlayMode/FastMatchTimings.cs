#if UNITY_EDITOR
using System.Reflection;
using NUnit.Framework;
using Trivias.Match.Data;
using UnityEditor;

namespace Trivias.App.Tests
{
    /// <summary>
    /// Durante las pruebas en juego la partida va deprisa: la cuenta atrás de verificar y la revelación duran lo mínimo
    /// (<see cref="PlayModeHelpers.LOCK_SECONDS"/>, <see cref="PlayModeHelpers.REVEAL_SECONDS"/>) en vez de 3 y 7 s.
    /// Las reglas no cambian, solo el ritmo; al acabar se restauran los valores del juego.
    /// </summary>
    [SetUpFixture]
    public sealed class FastMatchTimings
    {
        private const string CONFIG_PATH = "Assets/_Project/Modules/Match/Config/MatchConfig.asset";
        private const BindingFlags PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo LockField = typeof(MatchConfig).GetField("_lockCountdownSeconds", PRIVATE);
        private static readonly FieldInfo RevealField = typeof(MatchConfig).GetField("_revealSeconds", PRIVATE);

        private MatchConfig _config;
        private float _lock;
        private float _reveal;

        [OneTimeSetUp]
        public void SpeedUp()
        {
            _config = AssetDatabase.LoadAssetAtPath<MatchConfig>(CONFIG_PATH);
            _lock = _config.LockCountdownSeconds;
            _reveal = _config.RevealSeconds;
            LockField.SetValue(_config, PlayModeHelpers.LOCK_SECONDS);
            RevealField.SetValue(_config, PlayModeHelpers.REVEAL_SECONDS);
        }

        [OneTimeTearDown]
        public void Restore()
        {
            LockField.SetValue(_config, _lock);
            RevealField.SetValue(_config, _reveal);
        }
    }
}
#endif
