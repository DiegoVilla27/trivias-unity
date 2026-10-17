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
        public void Cosmetics_HangFromTheirMeasuredPoints_InMetres()
        {
            var view = Prefab().GetComponent<PlayerCharacterView>();
            var catalog = AssetDatabase.LoadAssetAtPath<Trivias.Shared.Characters.CharacterCatalog>(
                "Assets/_Project/Shared/Config/Characters/CharacterCatalog.asset");
            var character = catalog.Find(catalog.DefaultId);
            for (var slot = Trivias.Shared.Profile.CosmeticSlot.Head; slot <= Trivias.Shared.Profile.CosmeticSlot.Back; slot++)
            {
                var anchor = view.AnchorFor(slot);
                Assert.That(anchor, Is.Not.Null, $"{slot}");
                var bone = slot is Trivias.Shared.Profile.CosmeticSlot.Neck or Trivias.Shared.Profile.CosmeticSlot.Back ? "Chest" : "Head";
                Assert.That(anchor.parent.name, Is.EqualTo(bone), $"{slot}: sigue a su hueso en las animaciones.");
                Assert.That(anchor.lossyScale.x, Is.EqualTo(1f).Within(0.01f), $"{slot}: en metros.");
                Assert.That(Vector3.Distance(Prefab().transform.InverseTransformPoint(anchor.position), character.Anchor(slot)), Is.LessThan(0.001f),
                    $"{slot}: en el punto medido de su modelo.");
            }
        }

        [Test]
        public void EveryCharacter_HasItsAnchorsMeasured_WhereEachAccessoryGoes()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<Trivias.Shared.Characters.CharacterCatalog>(
                "Assets/_Project/Shared/Config/Characters/CharacterCatalog.asset");
            foreach (var character in catalog.Characters)
            {
                var head = character.Anchor(Trivias.Shared.Profile.CosmeticSlot.Head);
                var face = character.Anchor(Trivias.Shared.Profile.CosmeticSlot.Face);
                var mouth = character.Anchor(Trivias.Shared.Profile.CosmeticSlot.Mouth);
                var neck = character.Anchor(Trivias.Shared.Profile.CosmeticSlot.Neck);
                var back = character.Anchor(Trivias.Shared.Profile.CosmeticSlot.Back);
                Assert.That(head.y, Is.InRange(1.85f, 1.95f), $"{character.Id}: encima del pelo.");
                Assert.That(face.z, Is.InRange(0.3f, 0.6f), $"{character.Id}: delante de los ojos.");
                Assert.That(mouth.z, Is.InRange(0.35f, 0.6f), $"{character.Id}: delante de la boca.");
                Assert.That(mouth.y, Is.LessThan(face.y), $"{character.Id}: la boca, bajo los ojos.");
                Assert.That(neck.z, Is.InRange(0.1f, 0.5f), $"{character.Id}: bajo la barbilla, por delante.");
                Assert.That(back.z, Is.LessThan(neck.z - 0.15f).And.LessThan(0.05f), $"{character.Id}: la espalda, detrás del pecho.");
            }
        }
    }
}
