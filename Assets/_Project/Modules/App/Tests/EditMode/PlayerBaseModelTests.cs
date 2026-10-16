using System.Linq;
using NUnit.Framework;
using Trivias.Player.Editor;
using Trivias.Player.Network;
using Trivias.Shop.Data;
using Trivias.Shop.Editor;
using UnityEditor;
using UnityEngine;

namespace Trivias.App.Tests
{
    /// <summary>
    /// El concursante es el cuerpo base oficial (sin ropa, pelo ni zapatos) tal cual: sin perder polígonos ni altura,
    /// riggeado, Humanoid para las animaciones de Starter Assets y con los cosméticos a la escala de su cabeza.
    /// </summary>
    public sealed class PlayerBaseModelTests
    {
        private static GameObject Prefab() => AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabBuilder.PREFAB_PATH);

        private static Transform Model() => Prefab().transform.Find("Model");

        [Test]
        public void UsesTheRiggedBaseBody()
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(Model().gameObject);
            Assert.That(AssetDatabase.GetAssetPath(source), Is.EqualTo(PlayerBaseModelBuilder.MODEL_PATH));
        }

        [Test]
        public void TheBaseBody_IsTheOnlyPlayerModelLeft()
        {
            var folders = AssetDatabase.GetSubFolders("Assets/_Project/Modules/Player/Models");
            Assert.That(folders, Is.EquivalentTo(new[] { "Assets/_Project/Modules/Player/Models/PlayerBase" }),
                "Los modelos antiguos (con ropa) ya no se usan: la ropa irá por piezas sobre este cuerpo.");
        }

        [Test]
        public void KeepsEveryTriangleOfTheOriginal()
        {
            var triangles = Model().GetComponentsInChildren<SkinnedMeshRenderer>(true).Sum(r => r.sharedMesh.triangles.Length / 3);
            Assert.That(triangles, Is.EqualTo(1_000_000), "Tal cual: sin quitarle ni un triángulo.");
        }

        [Test]
        public void IsAValidHumanoid_WithEveryBoneMapped()
        {
            var animator = Model().GetComponent<Animator>();
            Assert.That(animator.avatar.isHuman && animator.avatar.isValid, Is.True, "Humanoid: le sirven las animaciones de Starter Assets.");
            var mapped = animator.avatar.humanDescription.human.Select(h => h.humanName).ToList();
            foreach (var bone in new[] { "Hips", "Spine", "Chest", "Neck", "Head", "LeftUpperArm", "RightHand", "LeftUpperLeg", "RightFoot" })
                Assert.That(mapped, Does.Contain(bone));
            Assert.That(animator.runtimeAnimatorController.name, Is.EqualTo("StarterAssetsThirdPerson"));
        }

        [Test]
        public void KeepsItsOriginalHeight_OnTheGround_FacingForward()
        {
            // Vértices en su pose de reposo (la caja de una malla con esqueleto es solo aproximada).
            var bounds = new Bounds(Model().position, Vector3.zero);
            foreach (var renderer in Model().GetComponentsInChildren<SkinnedMeshRenderer>(true))
                foreach (var vertex in renderer.sharedMesh.vertices)
                    bounds.Encapsulate(renderer.transform.TransformPoint(vertex));
            Assert.That(bounds.min.y, Is.EqualTo(0f).Within(0.02f), "Con los pies en el suelo.");
            Assert.That(bounds.max.y, Is.EqualTo(1.9f).Within(0.02f), "La altura del original, sin escalar.");
            Assert.That(Model().localScale, Is.EqualTo(Vector3.one));
            Assert.That(Prefab().GetComponent<CharacterController>().height, Is.EqualTo(1.9f).Within(0.01f), "La cápsula mide lo que él.");
            var hips = Model().GetComponentsInChildren<Transform>().First(t => t.name == "Hips");
            var leftHand = Model().GetComponentsInChildren<Transform>().First(t => t.name == "LeftHand");
            Assert.That(leftHand.position.x, Is.LessThan(hips.position.x - 0.3f), "La mano izquierda a su izquierda: mira hacia +Z.");
        }

        [Test]
        public void LooksLikeTheOriginal_PaintedLightAsEmission()
        {
            var body = Model().Find(PlayerBaseModelBuilder.BODY_MESH).GetComponent<SkinnedMeshRenderer>().sharedMaterial;
            Assert.That(body.name, Is.EqualTo(PlayerBaseModelBuilder.BODY_MATERIAL));
            Assert.That(body.GetColor("_BaseColor"), Is.EqualTo(Color.black), "Como lo importa Unity: color base negro…");
            Assert.That(body.IsKeywordEnabled("_EMISSION"), Is.True);
            Assert.That(body.GetColor("_EmissionColor"), Is.EqualTo(Color.white), "…y su textura con la luz pintada como emisión.");
            Assert.That(AssetDatabase.GetAssetPath(body.GetTexture("_EmissionMap")), Is.EqualTo(PlayerBaseModelBuilder.FOLDER + "/shaded.png"));
        }

        [Test]
        public void SuitColors_NeverTintTheSkin()
        {
            Assert.That(Prefab().GetComponent<PlayerNetwork>().BodyRenderers, Is.Empty,
                "El cuerpo base no lleva ropa: los trajes de colores no tiñen nada (nunca la piel).");
        }

        [Test]
        public void Cosmetics_SitOnThisBigHead()
        {
            var anchor = Prefab().GetComponent<PlayerNetwork>().HeadAnchor;
            Assert.That(anchor.name, Is.EqualTo(PlayerPrefabBuilder.COSMETICS_ANCHOR));
            Assert.That(anchor.parent.name, Is.EqualTo("Head"), "Sigue a la cabeza en las animaciones.");
            Assert.That(anchor.lossyScale.x, Is.EqualTo(PlayerPrefabBuilder.COSMETICS_SCALE).Within(0.01f));
            Assert.That(Vector3.Angle(anchor.forward, Prefab().transform.forward), Is.LessThan(1f), "Mira como el concursante.");

            var catalog = AssetDatabase.LoadAssetAtPath<CosmeticCatalog>(ShopBuilder.CATALOG_PATH);
            Vector3 At(string id) => anchor.TransformPoint(catalog.Find(id).Offset);
            Assert.That(At("gafas").y, Is.InRange(1.2f, 1.45f), "Las gafas, a la altura de los ojos.");
            Assert.That(At("gafas").z, Is.GreaterThan(0.4f), "Delante de la cara.");
            Assert.That(At("pajarita").y, Is.InRange(0.85f, 1.1f), "La pajarita, bajo la barbilla.");
            foreach (var hat in new[] { "gorra", "corona", "chistera" })
                Assert.That(At(hat).y, Is.InRange(1.7f, 1.85f), $"{hat}: sobre la coronilla.");
            foreach (var hat in new[] { "gorra", "corona", "chistera" })
                Assert.That(Mathf.Abs(At(hat).z), Is.LessThan(0.1f), $"{hat}: centrado sobre la cabeza, no en la frente.");
        }
    }
}
