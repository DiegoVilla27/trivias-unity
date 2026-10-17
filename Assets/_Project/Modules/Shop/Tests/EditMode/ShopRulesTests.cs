using NUnit.Framework;
using Trivias.Shop.Logic;

namespace Trivias.Shop.Tests
{
    /// <summary>Tienda: qué dice cada tarjeta y qué hace el botón (bocetos 04 y 05 y DISENO.md).</summary>
    public sealed class ShopRulesTests
    {
        [Test]
        public void Tokens_UseADotForThousands() => Assert.That(ShopRules.Tokens(1200), Is.EqualTo("1.200 fichas"));

        [Test]
        public void ACharacterToBuy_ShowsItsPrice_AndBuyingChoosesIt()
        {
            var offer = ShopRules.Character(free: false, owned: false, chosen: false, price: 1200, tokens: 1500);
            Assert.That(offer.State, Is.EqualTo(OfferState.Locked));
            Assert.That(offer.Badge, Is.EqualTo("1.200 fichas"));
            Assert.That(offer.Action, Is.EqualTo("Comprar y elegir"));
            Assert.That(offer.CanAct, Is.True);
        }

        [Test]
        public void WithoutEnoughTokens_ThePriceStays_AndItSaysHowManyAreMissing()
        {
            var offer = ShopRules.Character(false, false, false, 1200, 900);
            Assert.That(offer.Badge, Is.EqualTo("1.200 fichas"));
            Assert.That(offer.Action, Is.EqualTo("Te faltan 300 fichas"));
            Assert.That(offer.CanAct, Is.False);
        }

        [Test]
        public void FreeOrBoughtCharacters_AreChosen_AndTheChosenOneCannotBeChosenAgain()
        {
            Assert.That(ShopRules.Character(true, false, false, 1200, 0).Action, Is.EqualTo("Elegir"));
            Assert.That(ShopRules.Character(true, false, false, 1200, 0).Badge, Is.EqualTo("Gratis"));
            Assert.That(ShopRules.Character(false, true, false, 1200, 0).Badge, Is.EqualTo("Comprado"));
            var chosen = ShopRules.Character(true, false, true, 1200, 0);
            Assert.That(chosen.Action, Is.EqualTo("Elegido"));
            Assert.That(chosen.CanAct, Is.False);
        }

        [Test]
        public void Accessories_AreBoughtAndWorn_ThenTakenOff()
        {
            Assert.That(ShopRules.Accessory(false, false, 300, 1500).Action, Is.EqualTo("Comprar y equipar"));
            Assert.That(ShopRules.Accessory(false, false, 300, 100).Action, Is.EqualTo("Te faltan 200 fichas"));
            Assert.That(ShopRules.Accessory(true, false, 300, 0).Badge, Is.EqualTo("Adquirido"));
            Assert.That(ShopRules.Accessory(true, false, 300, 0).Action, Is.EqualTo("Equipar"));
            Assert.That(ShopRules.Accessory(true, true, 300, 0).Action, Is.EqualTo("Quitar"));
        }

        [Test]
        public void Arrows_WrapAround()
        {
            Assert.That(ShopRules.Step(4, 1, 5), Is.Zero);
            Assert.That(ShopRules.Step(0, -1, 5), Is.EqualTo(4));
            Assert.That(ShopRules.Step(2, 1, 5), Is.EqualTo(3));
        }
    }
}
