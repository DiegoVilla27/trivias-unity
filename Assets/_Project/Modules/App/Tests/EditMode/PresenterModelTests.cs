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
            foreach (var primitive in Prefab().GetComponentsInChildren<MeshFilter>(true))
                Assert.That(primitive.transform.parent.name, Is.EqualTo("Microphone"), "Solo el micrófono provisional es de cajas.");
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
        public void HeReacts_AndTalks_WithHisOwnAnimations_InSeparateLayers()
        {
            var controller = (AnimatorController)Model().GetComponent<Animator>().runtimeAnimatorController;
            Assert.That(controller.layers.Select(l => l.name), Is.EqualTo(new[] { "Base", "TalkMic", "TalkGesture", "TalkHead" }));
            var states = controller.layers[0].stateMachine.states.Select(s => s.state).ToDictionary(s => s.name);
            Assert.That(states.Keys, Is.EquivalentTo(new[] { "Idle", "Happy", "Sad" }));
            Assert.That(controller.parameters.Select(p => p.name), Is.EquivalentTo(new[] { "Happy", "Sad" }));
            foreach (var (state, file, loops) in new[] { ("Happy", "Presenter_Happy", false), ("Sad", "Presenter_Sad", false) })
            {
                var clip = (AnimationClip)states[state].motion;
                Assert.That(AssetDatabase.GetAssetPath(clip), Does.EndWith($"Presenter/Animations/{file}.fbx"));
                Assert.That(clip.humanMotion, Is.True, $"{state}: Humanoid (se pasa por músculos a su modelo).");
                Assert.That(clip.isLooping, Is.EqualTo(loops));
                Assert.That(clip.length / states[state].speed, Is.InRange(1.8f, 3.2f), $"{state}: dura lo justo para verse.");
                Assert.That(clip.frameRate * states[state].speed, Is.EqualTo(30f).Within(0.01f), $"{state}: a 30 fotogramas por segundo, como se diseñó.");
                var back = states[state].transitions.Single();
                Assert.That(back.destinationState.name, Is.EqualTo("Idle"), $"{state}: vuelve al reposo.");
            }

            // Hablar: el brazo del micro y el de los gestos se sustituyen; la cabeza se suma. Empiezan apagadas.
            var parts = new[] { AvatarMaskBodyPart.RightArm, AvatarMaskBodyPart.LeftArm, AvatarMaskBodyPart.Head };
            for (var i = 1; i < 4; i++)
            {
                var layer = controller.layers[i];
                var talk = (AnimationClip)layer.stateMachine.defaultState.motion;
                Assert.That(AssetDatabase.GetAssetPath(talk), Does.EndWith("Presenter/Animations/Presenter_Talk.fbx"));
                Assert.That(talk.isLooping, Is.True, "Hablar se repite sin saltos.");
                Assert.That(layer.defaultWeight, Is.Zero);
                for (var part = AvatarMaskBodyPart.Root; part < AvatarMaskBodyPart.LastBodyPart; part++)
                    Assert.That(layer.avatarMask.GetHumanoidBodyPartActive(part), Is.EqualTo(part == parts[i - 1]), $"{layer.name}: {part}.");
                Assert.That(layer.blendingMode, Is.EqualTo(i == 3 ? AnimatorLayerBlendingMode.Additive : AnimatorLayerBlendingMode.Override));
            }
        }

        [Test]
        public void TheMicrophone_IsInHisRightHand_PointingForwardFromTheFist()
        {
            var mic = Prefab().transform.Find("Model").GetComponentsInChildren<Transform>(true).Single(t => t.name == "Microphone");
            Assert.That(mic.parent.name, Is.EqualTo("RightHand"), "Va con la mano.");
            Assert.That(Vector3.Angle(mic.forward, Prefab().transform.forward), Is.LessThan(1f), "En la pose T apunta al frente (como en Blender).");
            Assert.That(mic.GetComponentsInChildren<Collider>(true), Is.Empty, "Sin colisiones.");
            Assert.That(Prefab().GetComponent<Trivias.Presenter.Components.PresenterAnimator>(), Is.Not.Null);
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
