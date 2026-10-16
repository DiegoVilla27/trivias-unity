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
    /// El concursante es el modelo oficial base_basic_shaded tal cual (PlayerNew), riggeado: Humanoid para las
    /// animaciones de Starter Assets, con el chándal teñible aparte y los cosméticos a la escala de su cabeza.
    /// </summary>
    public sealed class PlayerNewModelTests
    {
        private static GameObject Prefab() => AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabBuilder.PREFAB_PATH);

        private static Transform Model() => Prefab().transform.Find("Model");

        [Test]
        public void UsesThePlayerNewModel_NotStarterAssets()
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(Model().gameObject);
            Assert.That(AssetDatabase.GetAssetPath(source), Is.EqualTo(PlayerNewModelBuilder.MODEL_PATH));
        }

        [Test]
        public void KeepsEveryTriangleOfTheOfficialModel()
        {
            var triangles = Model().GetComponentsInChildren<SkinnedMeshRenderer>(true).Sum(r => r.sharedMesh.triangles.Length / 3);
            Assert.That(triangles, Is.EqualTo(1_000_000), "Tal cual: sin quitarle ni un triángulo.");
            var file = new System.IO.FileInfo(PlayerNewModelBuilder.MODEL_PATH);
            Assert.That(file.Length, Is.LessThan(95 * 1024 * 1024), "Cabe en GitHub sin LFS (límite 100 MB).");
        }

        [Test]
        public void IsAValidHumanoid_WithEveryBoneMapped()
        {
            var animator = Model().GetComponent<Animator>();
            Assert.That(animator.avatar, Is.Not.Null);
            Assert.That(animator.avatar.isHuman && animator.avatar.isValid, Is.True, "Humanoid: le sirven las animaciones de Starter Assets.");
            var mapped = animator.avatar.humanDescription.human.Select(h => h.humanName).ToList();
            foreach (var bone in new[] { "Hips", "Spine", "Chest", "Neck", "Head", "LeftUpperArm", "RightHand", "LeftUpperLeg", "RightFoot" })
                Assert.That(mapped, Does.Contain(bone));
            Assert.That(animator.runtimeAnimatorController.name, Is.EqualTo("StarterAssetsThirdPerson"));
        }

        [Test]
        public void StandsOnTheGround_AtItsHeight_FacingForward()
        {
            // Vértices en su pose de reposo (la caja de una malla con esqueleto es solo aproximada).
            var bounds = new Bounds(Model().position, Vector3.zero);
            foreach (var renderer in Model().GetComponentsInChildren<SkinnedMeshRenderer>(true))
                foreach (var vertex in renderer.sharedMesh.vertices)
                    bounds.Encapsulate(renderer.transform.TransformPoint(vertex));
            Assert.That(bounds.min.y, Is.EqualTo(0f).Within(0.05f), "Con los pies en el suelo.");
            Assert.That(bounds.max.y, Is.EqualTo(1.65f).Within(0.08f));
            var controller = Prefab().GetComponent<CharacterController>();
            Assert.That(controller.height, Is.EqualTo(1.65f).Within(0.01f), "La cápsula mide lo que el concursante.");
            var head = Model().GetComponentsInChildren<Transform>().First(t => t.name == "Head");
            var hips = Model().GetComponentsInChildren<Transform>().First(t => t.name == "Hips");
            var leftHand = Model().GetComponentsInChildren<Transform>().First(t => t.name == "LeftHand");
            Assert.That(leftHand.position.x, Is.LessThan(hips.position.x - 0.3f), "La mano izquierda queda a su izquierda: mira hacia +Z.");
            Assert.That(head.position.y, Is.GreaterThan(hips.position.y));
        }

        [Test]
        public void SuitColors_TintOnlyTheTracksuit()
        {
            var network = Prefab().GetComponent<PlayerNetwork>();
            Assert.That(network.BodyRenderers.Length, Is.EqualTo(1));
            Assert.That(network.BodyRenderers[0].name, Is.EqualTo(PlayerNewModelBuilder.SUIT_MESH), "Solo el chándal: ni la cara ni el pelo.");
            var suit = network.BodyRenderers[0].sharedMaterial;
            Assert.That(suit.GetTexture("_EmissionMap"), Is.Not.Null);
            var blue = PlayerNewModelBuilder.SuitBlue.linear;
            var emission = suit.GetColor("_EmissionColor");
            Assert.That(Vector3.Distance(new Vector3(emission.r, emission.g, emission.b), new Vector3(blue.r, blue.g, blue.b)), Is.LessThan(0.01f),
                "De fábrica, el azul del original (la emisión se guarda en espacio lineal).");
        }

        [Test]
        public void LooksLikeTheOriginal_PaintedLightAsEmission()
        {
            var body = Model().Find(PlayerNewModelBuilder.BODY_MESH).GetComponent<SkinnedMeshRenderer>().sharedMaterial;
            Assert.That(body.name, Is.EqualTo(PlayerNewModelBuilder.BODY_MATERIAL));
            Assert.That(body.GetColor("_BaseColor"), Is.EqualTo(Color.black), "Como lo importa Unity: color base negro…");
            Assert.That(body.IsKeywordEnabled("_EMISSION"), Is.True);
            Assert.That(body.GetColor("_EmissionColor"), Is.EqualTo(Color.white), "…y la textura con su luz pintada como emisión.");
            Assert.That(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(body.GetTexture("_EmissionMap"))), Is.EqualTo("PlayerNew_Color.png"));
            Assert.That(body.GetFloat("_Smoothness"), Is.Zero);
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
            Assert.That(At("gafas").y, Is.InRange(1.05f, 1.25f), "Las gafas, a la altura de los ojos.");
            Assert.That(At("gafas").z, Is.GreaterThan(0.35f), "Delante de la cara.");
            Assert.That(At("pajarita").y, Is.InRange(0.75f, 0.95f), "La pajarita, bajo la barbilla.");
            foreach (var hat in new[] { "gorra", "corona", "chistera" })
                Assert.That(At(hat).y, Is.InRange(1.45f, 1.7f), $"{hat}: encima del pelo.");
        }
    }
}
