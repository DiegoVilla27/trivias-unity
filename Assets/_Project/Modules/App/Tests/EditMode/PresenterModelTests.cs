using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Trivias.App.Tests
{
    /// <summary>
    /// El presentador es el modelo del usuario tal cual (un millón de triángulos, 1,9 m, su textura), Humanoid, en
    /// reposo con la animación de Starter Assets y con una cápsula para que nadie lo atraviese.
    /// </summary>
    public sealed class PresenterModelTests
    {
        private const string PREFAB_PATH = "Assets/_Project/Modules/Presenter/Prefabs/Presenter.prefab";

        private static GameObject Prefab() => AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);

        private static Transform Model() => Prefab().transform.Find("Model");

        [Test]
        public void ThePresenter_IsTheUsersModel_AsIs()
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(Model().gameObject);
            Assert.That(AssetDatabase.GetAssetPath(source), Does.EndWith("Presenter/Models/Presenter/Presenter_Rigged.fbx"));
            var body = Model().GetComponentInChildren<SkinnedMeshRenderer>();
            Assert.That(body.sharedMesh.triangles.Length / 3, Is.EqualTo(1_000_000), "Sin quitarle triángulos.");
            Assert.That(body.sharedMesh.bounds.size.y, Is.EqualTo(1.9f).Within(0.03f), "Su altura original.");
            Assert.That(body.sharedMaterial.GetColor("_BaseColor"), Is.EqualTo(Color.black), "Como el original en Unity…");
            Assert.That(body.sharedMaterial.GetTexture("_EmissionMap").name, Is.EqualTo("shaded"), "…con su textura como emisión.");
            Assert.That(Prefab().GetComponentsInChildren<MeshFilter>(true), Is.Empty, "Ya no quedan primitivas.");
            Assert.That(Prefab().GetComponentsInChildren<TMPro.TextMeshPro>(true), Is.Empty, "Sin el cartel «PRESENTADOR».");
        }

        [Test]
        public void ThePresenter_IdlesWithTheStarterAssetsAnimation()
        {
            var animator = Model().GetComponent<Animator>();
            Assert.That(animator.avatar.isHuman && animator.avatar.isValid, Is.True);
            Assert.That(animator.applyRootMotion, Is.False, "No se mueve de su marca.");
            var controller = (AnimatorController)animator.runtimeAnimatorController;
            var idle = controller.layers[0].stateMachine.defaultState;
            Assert.That(idle.name, Is.EqualTo("Idle"));
            Assert.That(AssetDatabase.GetAssetPath(idle.motion), Does.EndWith("Stand--Idle.anim.fbx"));
            Assert.That(((AnimationClip)idle.motion).isLooping, Is.True);
        }

        [Test]
        public void NobodyWalksThroughHim()
        {
            var body = Prefab().GetComponent<CapsuleCollider>();
            Assert.That(body.height, Is.EqualTo(1.9f).Within(0.01f));
            Assert.That(body.center.y, Is.EqualTo(0.95f).Within(0.01f));
            Assert.That(Prefab().transform.Find("Bubble"), Is.Null, "Habla en su rótulo de la pantalla, no en una viñeta del plató.");
        }
    }
}
