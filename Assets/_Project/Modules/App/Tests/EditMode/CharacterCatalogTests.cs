using System.Linq;
using NUnit.Framework;
using Trivias.Shared.Characters;
using Trivias.Shop.Components;
using UnityEditor;
using UnityEngine;

namespace Trivias.App.Tests
{
    /// <summary>
    /// Catálogo de personajes: los diez de los bocetos, seis gratuitos en un array fácil de cambiar, el
    /// resto en la tienda al precio de un atuendo especial.
    /// </summary>
    public sealed class CharacterCatalogTests
    {
        private static CharacterCatalog Catalog() =>
            AssetDatabase.LoadAssetAtPath<CharacterCatalog>("Assets/_Project/Shared/Config/Characters/CharacterCatalog.asset");

        [Test]
        public void HasTenCharacters_WithUniqueIdsNamesAndColors()
        {
            var characters = Catalog().Characters;
            Assert.That(characters, Has.Count.EqualTo(10));
            Assert.That(characters.Select(c => c.Id), Is.Unique);
            Assert.That(characters.Select(c => c.DisplayName), Is.Unique.And.All.Not.Empty);
            Assert.That(characters.Select(c => (Color32)c.Tint), Is.Unique, "Cada uno con su color.");
        }

        [Test]
        public void SixAreFree_AndTheyAllExist()
        {
            var catalog = Catalog();
            Assert.That(catalog.FreeIds.Count, Is.EqualTo(6), "Seis gratuitos.");
            Assert.That(catalog.FreeIds, Is.Unique);
            foreach (var id in catalog.FreeIds)
                Assert.That(catalog.Find(id), Is.Not.Null, $"«{id}» no está en el catálogo.");
            Assert.That(catalog.Price, Is.EqualTo(1200), "Precio de un atuendo especial (GDD 11).");
        }

        [Test]
        public void EveryCharacter_HasItsRiggedModel_AsAValidHumanoid()
        {
            foreach (var character in Catalog().Characters)
            {
                Assert.That(character.Model, Is.Not.Null, $"{character.Id}: modelo.");
                Assert.That(character.Avatar != null && character.Avatar.isHuman && character.Avatar.isValid, Is.True,
                    $"{character.Id}: avatar Humanoid (le sirven las animaciones de Starter Assets).");
                var body = character.Model.GetComponentInChildren<SkinnedMeshRenderer>();
                Assert.That(body.sharedMesh.triangles.Length / 3, Is.EqualTo(1_000_000), $"{character.Id}: tal cual, sin quitarle triángulos.");
                Assert.That(body.sharedMesh.bounds.size.y, Is.EqualTo(1.9f).Within(0.03f), $"{character.Id}: su altura original.");
                var material = body.sharedMaterial;
                Assert.That(material.GetColor("_BaseColor"), Is.EqualTo(Color.black), $"{character.Id}: como el original en Unity…");
                Assert.That(material.GetTexture("_EmissionMap"), Is.Not.Null, $"{character.Id}: …su textura como emisión.");
            }
        }

        [Test]
        public void TheFreeOnes_AreTheMockupsFirstSix()
        {
            Assert.That(Catalog().FreeIds, Is.EqualTo(new[] { "tracksuit", "casual", "gardener", "elegant", "explorer", "pajamas" }));
            Assert.That(Catalog().DefaultId, Is.EqualTo("tracksuit"), "Quien no elige lleva Blue Tracksuit.");
        }

        [Test]
        public void TheShop_HasACardForEveryCharacter()
        {
            var shop = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Modules/Shop/Prefabs/Shop.prefab");
            var cards = shop.transform.Find("Panel/Screen/Grid/Characters").GetComponentsInChildren<ShopCard>(true);
            Assert.That(cards, Has.Length.EqualTo(Catalog().Characters.Count));
        }
    }
}
