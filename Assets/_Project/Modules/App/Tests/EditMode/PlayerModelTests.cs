using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Trivias.App.Tests
{
    /// <summary>
    /// Mientras se modelan los personajes completos, el concursante es el robot de Starter Assets (Humanoid, con sus
    /// animaciones) y no queda ningún modelo de prueba del concursante en el proyecto.
    /// </summary>
    public sealed class PlayerModelTests
    {
        private const string PREFAB_PATH = "Assets/_Project/Modules/Player/Prefabs/Player.prefab";
        private const string ROBOT_PATH = "Assets/ThirdParty/StarterAssets/Runtime/ThirdPersonController/Character/Models/Armature.fbx";

        private static Transform Model() => AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH).transform.Find("Model");

        [Test]
        public void TheContestant_IsTheStarterAssetsRobot_ForNow()
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(Model().gameObject);
            Assert.That(AssetDatabase.GetAssetPath(source), Is.EqualTo(ROBOT_PATH));
            var animator = Model().GetComponent<Animator>();
            Assert.That(animator.avatar.isHuman && animator.avatar.isValid, Is.True);
            Assert.That(animator.runtimeAnimatorController.name, Is.EqualTo("StarterAssetsThirdPerson"));
        }

        [Test]
        public void NoTrialContestantModels_AreLeftInTheProject()
        {
            Assert.That(AssetDatabase.IsValidFolder("Assets/_Project/Modules/Player/Models"), Is.False,
                "El cuerpo base y las piezas sueltas se descartaron: llegarán personajes completos.");
        }
    }
}
