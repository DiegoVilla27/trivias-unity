using NUnit.Framework;
using Trivias.Match.Data;
using UnityEditor;

namespace Trivias.Match.Tests
{
    /// <summary>
    /// El ritmo del juego es el del GDD aunque las pruebas en juego lo aceleren: la configuración guardada no se toca.
    /// </summary>
    public sealed class MatchConfigAssetTests
    {
        [Test]
        public void TheGame_KeepsItsRealPace()
        {
            var config = AssetDatabase.LoadAssetAtPath<MatchConfig>("Assets/_Project/Modules/Match/Config/MatchConfig.asset");
            Assert.That(config.LockCountdownSeconds, Is.EqualTo(3f), "Cuenta atrás de 3 segundos (GDD).");
            Assert.That(config.RevealSeconds, Is.EqualTo(7f));
        }
    }
}
