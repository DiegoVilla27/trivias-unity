using System.Collections.Generic;
using NUnit.Framework;
using Trivias.Spectator.Logic;

namespace Trivias.Spectator.Tests
{
    /// <summary>A quién mira el espectador y qué dice su rótulo.</summary>
    public sealed class SpectatorLogicTests
    {
        private static readonly List<ulong> Three = new() { 1, 3, 5 };

        [Test]
        public void WithNoOneToWatch_ThereIsNoTarget()
        {
            Assert.That(SpectatorTargets.Step(new List<ulong>(), false, 0, 0), Is.EqualTo(SpectatorTargets.NONE));
            Assert.That(SpectatorTargets.Step(new List<ulong>(), true, 3, +1), Is.EqualTo(SpectatorTargets.NONE));
        }

        [Test]
        public void Starting_WatchesTheFirst_OrTheLastWhenGoingBack()
        {
            Assert.That(SpectatorTargets.Step(Three, false, 0, 0), Is.EqualTo(0));
            Assert.That(SpectatorTargets.Step(Three, false, 0, +1), Is.EqualTo(0));
            Assert.That(SpectatorTargets.Step(Three, false, 0, -1), Is.EqualTo(2));
        }

        [Test]
        public void NextAndPrevious_WrapAround()
        {
            Assert.That(SpectatorTargets.Step(Three, true, 1, +1), Is.EqualTo(1));
            Assert.That(SpectatorTargets.Step(Three, true, 5, +1), Is.EqualTo(0), "Tras el último, el primero.");
            Assert.That(SpectatorTargets.Step(Three, true, 1, -1), Is.EqualTo(2), "Antes del primero, el último.");
            Assert.That(SpectatorTargets.Step(Three, true, 3, 0), Is.EqualTo(1), "Paso 0: sigue mirando al mismo.");
        }

        [Test]
        public void WhenTheWatchedPlayerFalls_TheNextOneIsWatched()
        {
            Assert.That(SpectatorTargets.Step(Three, true, 2, +1), Is.EqualTo(1), "Cayó el 2: pasa al 3.");
            Assert.That(SpectatorTargets.Step(Three, true, 2, -1), Is.EqualTo(0), "Hacia atrás: el 1.");
            Assert.That(SpectatorTargets.Step(Three, true, 9, +1), Is.EqualTo(0), "Cayó el último: vuelve al primero.");
            Assert.That(SpectatorTargets.Step(Three, true, 0, -1), Is.EqualTo(2), "Cayó el primero hacia atrás: el último.");
        }

        [Test]
        public void Banner_SaysWhoIsWatched_AndHowToChange()
        {
            var following = SpectatorText.Banner("Jugador 2", true);
            Assert.That(following, Does.Contain("ELIMINADO").And.Contain("espectador"));
            Assert.That(following, Does.Contain("Viendo a").And.Contain("Jugador 2"));
            Assert.That(following, Does.Contain("A/D Cambiar jugador").And.Contain("Espacio Vista general").And.Contain("Esc Menú"));
            Assert.That(SpectatorText.Watching("Ana"), Does.Contain("Viendo a").And.Contain("<b>Ana</b>"), "El nombre resaltado.");

            Assert.That(SpectatorText.Banner(string.Empty, true), Does.Contain("Vista general").And.Contain("Seguir a un concursante"));
            Assert.That(SpectatorText.Banner(string.Empty, true), Does.Not.Contain("Espacio"), "Ya está en la vista general.");
            Assert.That(SpectatorText.Banner(string.Empty, false), Does.Contain("Vista general").And.Not.Contain("eguir"),
                "Sin nadie activo no se ofrece seguir a nadie.");
        }
    }
}
