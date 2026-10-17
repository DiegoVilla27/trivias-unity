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
        public void EveryItemHasItsModel_AndSuitColorsAreGone()
        {
            foreach (var item in Catalog().Items)
                Assert.That(item.Prefab, Is.Not.Null, $"{item.Id}: le falta el modelo.");
            Assert.That(Catalog().Items.Any(i => i.Id.StartsWith("color-")), Is.False,
                "Los trajes de colores se sustituyeron por personajes.");
        }
    }
}
