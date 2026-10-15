using NUnit.Framework;
using Trivias.Player.Logic;

namespace Trivias.Player.Tests
{
    public sealed class MovementLimitsTests
    {
        [Test]
        public void MoveInsideCircle_IsUnchanged()
        {
            float dx = 0.2f, dz = 0.1f;
            MovementLimits.ClampMove(0f, 0f, ref dx, ref dz, 0f, 0f, 1f);
            Assert.That(dx, Is.EqualTo(0.2f).Within(1e-5f));
            Assert.That(dz, Is.EqualTo(0.1f).Within(1e-5f));
        }

        [Test]
        public void MoveOutsideCircle_StopsAtTheEdge()
        {
            float dx = 2f, dz = 0f;
            MovementLimits.ClampMove(0.5f, 0f, ref dx, ref dz, 0f, 0f, 1f);
            Assert.That(0.5f + dx, Is.EqualTo(1f).Within(1e-5f));
            Assert.That(dz, Is.EqualTo(0f).Within(1e-5f));
        }

        [Test]
        public void MovingAlongTheEdge_SlidesInsteadOfLeaving()
        {
            float dx = 0f, dz = 0.5f;
            MovementLimits.ClampMove(1f, 0f, ref dx, ref dz, 0f, 0f, 1f);
            var x = 1f + dx;
            var z = dz;
            Assert.That(x * x + z * z, Is.EqualTo(1f).Within(1e-4f), "Se queda sobre el borde.");
            Assert.That(z, Is.GreaterThan(0f), "Pero puede desplazarse a lo largo de él.");
        }
    }
}
