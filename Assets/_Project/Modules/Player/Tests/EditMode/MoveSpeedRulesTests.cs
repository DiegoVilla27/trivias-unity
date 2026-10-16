using NUnit.Framework;
using Trivias.Player.Data;
using Trivias.Player.Logic;
using UnityEditor;
using UnityEngine.InputSystem;

namespace Trivias.Player.Tests
{
    /// <summary>Correr con Shift: más rápido que andar, solo si hay dirección, y con la tecla en los controles.</summary>
    public sealed class MoveSpeedRulesTests
    {
        [Test]
        public void Sprinting_IsFasterThanWalking_OnlyWhenMoving()
        {
            Assert.That(MoveSpeedRules.Target(true, false, 3f, 5f), Is.EqualTo(3f));
            Assert.That(MoveSpeedRules.Target(true, true, 3f, 5f), Is.EqualTo(5f));
            Assert.That(MoveSpeedRules.Target(false, true, 3f, 5f), Is.Zero, "Shift sin dirección no mueve.");
        }

        [Test]
        public void Config_RunsClearlyFasterThanItWalks()
        {
            var config = AssetDatabase.LoadAssetAtPath<PlayerConfig>("Assets/_Project/Modules/Player/Config/PlayerConfig.asset");
            Assert.That(config.SprintSpeed, Is.GreaterThan(config.MoveSpeed * 1.4f));
        }

        [Test]
        public void Controls_SprintWithShift_OrTheLeftStick()
        {
            var controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/_Project/Modules/Player/Config/PlayerControls.inputactions");
            var sprint = controls.FindAction("Player/Sprint", true);
            var paths = new System.Collections.Generic.List<string>();
            foreach (var binding in sprint.bindings)
                paths.Add(binding.path);
            Assert.That(paths, Does.Contain("<Keyboard>/leftShift"));
            Assert.That(paths, Does.Contain("<Gamepad>/leftStickPress"));
        }
    }
}
