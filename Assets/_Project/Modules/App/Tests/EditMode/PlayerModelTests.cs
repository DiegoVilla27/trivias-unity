using NUnit.Framework;
using Trivias.Player.Components;
using UnityEditor;
using UnityEngine;

namespace Trivias.App.Tests
{
    /// <summary>
    /// El concursante se construye con el personaje por defecto (Blue Tracksuit, tal cual y riggeado), Humanoid con las
    /// animaciones de Starter Assets, a la medida de su 1,9 m y con el anclaje de los complementos en la cabeza.
    /// </summary>
    public sealed class PlayerModelTests
    {
        private const string PREFAB_PATH = "Assets/_Project/Modules/Player/Prefabs/Player.prefab";

        private static GameObject Prefab() => AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);

        private static Transform Model() => Prefab().transform.Find("Model");

        [Test]
        public void TheContestant_IsBuiltWithTheDefaultCharacter()
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(Model().gameObject);
            Assert.That(AssetDatabase.GetAssetPath(source), Does.EndWith("Characters/Tracksuit/Tracksuit_Rigged.fbx"));
            var animator = Model().GetComponent<Animator>();
            Assert.That(animator.avatar.isHuman && animator.avatar.isValid, Is.True);
            Assert.That(animator.runtimeAnimatorController.name, Is.EqualTo("StarterAssetsThirdPerson"));
        }

        [Test]
        public void TheCapsule_FitsItsOneNinetyMetres()
        {
            var controller = Prefab().GetComponent<CharacterController>();
            Assert.That(controller.height, Is.EqualTo(1.9f).Within(0.01f));
            Assert.That(controller.center.y, Is.EqualTo(0.97f).Within(0.02f), "Con los pies en el suelo.");
        }

        [Test]
        public void Cosmetics_HangFromTheHead_AtItsScale()
        {
            var view = Prefab().GetComponent<PlayerCharacterView>();
            Assert.That(view.HeadAnchor, Is.Not.Null);
            Assert.That(view.HeadAnchor.parent.name, Is.EqualTo("Head"), "Sigue a la cabeza en las animaciones.");
            Assert.That(view.HeadAnchor.lossyScale.x, Is.EqualTo(3f).Within(0.01f), "A la escala de esta cabeza grande.");
            Assert.That(view.HeadAnchor.position.y, Is.InRange(1.2f, 1.4f), "A la altura de los ojos.");
        }
    }
}
