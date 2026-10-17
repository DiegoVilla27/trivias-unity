using System.Linq;
using NUnit.Framework;
using Trivias.Shared.Profile;
using Trivias.Shop.Data;
using UnityEditor;

namespace Trivias.App.Tests
{
    /// <summary>Catálogo de la tienda (GDD 11): precios por tipo, ids únicos y cada cosmético con lo que necesita.</summary>
    public sealed class ShopCatalogTests
    {
        private static CosmeticCatalog Catalog() =>
            AssetDatabase.LoadAssetAtPath<CosmeticCatalog>("Assets/_Project/Modules/Shop/Config/CosmeticCatalog.asset");

        [Test]
        public void Catalog_HasItemsForEverySlot()
        {
            var items = Catalog().Items;
            for (var slot = 0; slot < CosmeticSlots.COUNT; slot++)
                Assert.That(items.Any(i => (int)i.Slot == slot), Is.True, $"No hay cosméticos para {(CosmeticSlot)slot}.");
        }

        [Test]
        public void Catalog_IdsAreUnique() =>
            Assert.That(Catalog().Items.Select(i => i.Id).Distinct().Count(), Is.EqualTo(Catalog().Items.Count));

        [Test]
        public void Catalog_PricesFollowGddCategories()
        {
            foreach (var item in Catalog().Items)
                Assert.That(item.Price, Is.EqualTo(300).Or.EqualTo(1200), $"{item.Id}: precio fuera de las categorías del GDD.");
            Assert.That(Catalog().Items.Any(i => i.Price == 300), Is.True, "Faltan accesorios de 300.");
            Assert.That(Catalog().Items.Any(i => i.Price == 1200), Is.True, "Faltan atuendos especiales de 1.200.");
        }

        [Test]
        public void TheTenAccessories_AreTheUsersModels_InTheirSlots_AndPrices()
        {
            var expected = new (string Id, string Folder, CosmeticSlot Slot, int Price, float Width)[]
            {
                ("corona", "Crown", CosmeticSlot.Head, 1200, 0.46f), ("pajarita", "BowTie", CosmeticSlot.Neck, 300, 0.26f),
                ("sombrero-copa", "TopHat", CosmeticSlot.Head, 1200, 0.60f), ("gafas-estrella", "StarGlasses", CosmeticSlot.Face, 300, 0.62f),
                ("bigote", "Mustache", CosmeticSlot.Mouth, 300, 0.30f), ("gafas-sol", "SunGlasses", CosmeticSlot.Face, 300, 0.58f),
                ("antifaz", "Mask", CosmeticSlot.Face, 300, 0.68f), ("aureola", "Halo", CosmeticSlot.Head, 1200, 0.50f),
                ("alas-angel", "AngelWings", CosmeticSlot.Back, 1200, 1.10f), ("alas-dragon", "DragonWings", CosmeticSlot.Back, 1200, 1.15f),
            };
            var items = Catalog().Items;
            Assert.That(items.Select(i => i.Id), Is.EqualTo(expected.Select(e => e.Id)), "Los diez, en el orden de la tienda.");
            for (var i = 0; i < expected.Length; i++)
            {
                var item = items[i];
                Assert.That(item.Slot, Is.EqualTo(expected[i].Slot), item.Id);
                Assert.That(item.Price, Is.EqualTo(expected[i].Price), item.Id);
                var model = item.Prefab.transform.Find("Model");
                Assert.That(AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(model.gameObject)),
                    Does.EndWith($"Shop/Models/Accessories/{expected[i].Folder}/{expected[i].Folder}.fbx"), item.Id);
                var mesh = model.GetComponentInChildren<UnityEngine.MeshFilter>().sharedMesh;
                Assert.That(mesh.triangles.Length / 3, Is.EqualTo(500_000), $"{item.Id}: tal cual, sin quitarle triángulos.");
                Assert.That(mesh.bounds.size.x, Is.EqualTo(expected[i].Width).Within(0.01f), $"{item.Id}: a su tamaño real.");
                var material = model.GetComponentInChildren<UnityEngine.MeshRenderer>().sharedMaterial;
                Assert.That(material.GetTexture("_EmissionMap"), Is.Not.Null, $"{item.Id}: su textura, como en el original.");
                Assert.That(item.Prefab.GetComponentsInChildren<UnityEngine.Collider>(true), Is.Empty, $"{item.Id}: sin colisiones.");
            }
        }

        [Test]
        public void EveryItemHasItsModel_AndSuitColorsAreGone()
        {
            foreach (var item in Catalog().Items)
                Assert.That(item.Prefab, Is.Not.Null, $"{item.Id}: le falta el modelo.");
            Assert.That(Catalog().Items.Any(i => i.Id.StartsWith("color-")), Is.False,
                "Los trajes de colores se sustituyeron por personajes.");
        }
    }
}
