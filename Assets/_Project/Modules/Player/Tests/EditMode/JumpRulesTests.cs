using NUnit.Framework;
using Trivias.Player.Logic;

namespace Trivias.Player.Tests
{
    /// <summary>Salto: solo desde el suelo, con el control abierto y tras la espera; despega a la altura pedida.</summary>
    public sealed class JumpRulesTests
    {
        [Test]
        public void Jumps_OnlyFromTheGround_WhenAllowed_AndAfterTheCooldown()
        {
            Assert.That(JumpRules.CanJump(grounded: true, canMove: true, cooldownLeft: 0f), Is.True);
            Assert.That(JumpRules.CanJump(grounded: false, canMove: true, cooldownLeft: 0f), Is.False, "En el aire no.");
            Assert.That(JumpRules.CanJump(grounded: true, canMove: false, cooldownLeft: 0f), Is.False, "Sala de espera o pausa: no.");
            Assert.That(JumpRules.CanJump(grounded: true, canMove: true, cooldownLeft: 0.1f), Is.False, "Recién aterrizado: espera.");
        }

        [Test]
        public void TakeOffSpeed_ReachesTheRequestedHeight()
        {
            const float gravity = -15f;
            var speed = JumpRules.TakeOffSpeed(0.9f, gravity);
            var apex = speed * speed / (-2f * gravity);
            Assert.That(apex, Is.EqualTo(0.9f).Within(0.001f));
            Assert.That(JumpRules.TakeOffSpeed(0f, gravity), Is.Zero);
            Assert.That(JumpRules.TakeOffSpeed(1f, 0f), Is.Zero, "Sin gravedad no hay salto que calcular.");
        }
    }
}
