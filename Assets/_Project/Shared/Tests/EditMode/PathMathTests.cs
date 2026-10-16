using NUnit.Framework;
using Trivias.Shared.Tubes;
using UnityEngine;

namespace Trivias.Shared.Tests
{
    /// <summary>Recorridos por los tubos: muestreo por distancia y movimiento de succión.</summary>
    public sealed class PathMathTests
    {
        private static readonly Vector3[] LShape = { Vector3.zero, new(0f, 2f, 0f), new(0f, 2f, -3f) };

        [Test]
        public void Lengths_AddUpEachSegment()
        {
            var lengths = PathMath.CumulativeLengths(LShape);
            Assert.That(lengths, Is.EqualTo(new[] { 0f, 2f, 5f }));
        }

        [Test]
        public void Sample_WalksTheSegments_AndClampsAtTheEnds()
        {
            var lengths = PathMath.CumulativeLengths(LShape);
            PathMath.Sample(LShape, lengths, 1f, out var p, out var d);
            Assert.That(p, Is.EqualTo(new Vector3(0f, 1f, 0f)));
            Assert.That(d, Is.EqualTo(Vector3.up));

            PathMath.Sample(LShape, lengths, 3.5f, out p, out d);
            Assert.That(Vector3.Distance(p, new Vector3(0f, 2f, -1.5f)), Is.LessThan(0.0001f));
            Assert.That(d, Is.EqualTo(Vector3.back));

            PathMath.Sample(LShape, lengths, -1f, out p, out _);
            Assert.That(p, Is.EqualTo(Vector3.zero), "Antes del inicio, el inicio.");
            PathMath.Sample(LShape, lengths, 99f, out p, out _);
            Assert.That(p, Is.EqualTo(LShape[2]), "Después del final, el final.");
        }

        [Test]
        public void Suction_StartsSlow_AndAccelerates()
        {
            Assert.That(PathMath.SuctionDistance(0f, 3f, 20f), Is.Zero);
            var first = PathMath.SuctionDistance(0.2f, 3f, 20f);
            var second = PathMath.SuctionDistance(0.4f, 3f, 20f) - first;
            Assert.That(second, Is.GreaterThan(first), "Cada vez tira más fuerte.");
            var time = PathMath.SuctionTime(10f, 3f, 20f);
            Assert.That(PathMath.SuctionDistance(time, 3f, 20f), Is.EqualTo(10f).Within(0.001f), "Tiempo y distancia son inversos.");
        }

        [Test]
        public void Bezier_GoesFromStartToEnd_PassingNearTheControl()
        {
            var start = Vector3.zero;
            var control = new Vector3(0f, 2f, 0f);
            var end = new Vector3(0f, 2f, -2f);
            Assert.That(PathMath.Bezier(start, control, end, 0f), Is.EqualTo(start));
            Assert.That(PathMath.Bezier(start, control, end, 1f), Is.EqualTo(end));
            Assert.That(PathMath.Bezier(start, control, end, 0.5f).y, Is.GreaterThan(1f), "Sube antes de entrar.");
            Assert.That(PathMath.EaseIn(0.5f), Is.LessThan(0.5f), "Arranca suave.");
        }
    }
}
