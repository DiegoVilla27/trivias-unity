using NUnit.Framework;
using Trivias.HUD.Logic;
using Trivias.Match.Logic;
using Trivias.Money.Logic;

namespace Trivias.HUD.Tests
{
    public sealed class HudTextTests
    {
        private static string Clock(int seconds)
        {
            var buffer = new char[8];
            return new string(buffer, 0, HudText.WriteClock(seconds, buffer));
        }

        private static string Amount(long value, char sign = '\0')
        {
            var buffer = new char[HudText.AMOUNT_BUFFER];
            return new string(buffer, 0, HudText.WriteAmount(value, buffer, sign));
        }

        [TestCase(72, "01:12")]
        [TestCase(5, "00:05")]
        [TestCase(0, "00:00")]
        [TestCase(-3, "00:00")]
        [TestCase(600, "10:00")]
        public void TheClock_ReadsMinutesAndSeconds(int seconds, string expected) => Assert.That(Clock(seconds), Is.EqualTo(expected));

        [TestCase(0L, "0")]
        [TestCase(950L, "950")]
        [TestCase(5000L, "5.000")]
        [TestCase(100000L, "100.000")]
        [TestCase(1234567L, "1.234.567")]
        public void Amounts_MatchTheGamesFormat(long value, string expected)
        {
            Assert.That(Amount(value), Is.EqualTo(expected));
            Assert.That(Amount(value), Is.EqualTo(MoneyFormat.Amount(value)), "Igual que el resto del juego.");
        }

        [Test]
        public void Amounts_CanCarryASign()
        {
            Assert.That(Amount(120000, '+'), Is.EqualTo("+120.000"));
            Assert.That(Amount(-40000, '–'), Is.EqualTo("–40.000"));
            Assert.That(Amount(long.MaxValue), Is.EqualTo(MoneyFormat.Amount(long.MaxValue)), "Cabe la cifra más larga.");
        }

        [Test]
        public void TheStateOfTheRound_IsWrittenOut()
        {
            Assert.That(HudText.State(MatchPhase.Betting), Is.EqualTo("Apuestas abiertas"));
            Assert.That(HudText.State(MatchPhase.Locked), Is.EqualTo("Apuestas cerradas"));
            Assert.That(HudText.State(MatchPhase.Reveal), Is.EqualTo("Respuesta revelada"));
            Assert.That(HudText.Question(5, 15), Does.StartWith("PREGUNTA").And.Contain("5/15"));
        }

        [Test]
        public void WhatYouCarry_SaysTypeAmountAndThatTypesCapacity()
        {
            Assert.That(HudText.Carry(Denomination.Bundle, 2, 10000), Is.EqualTo("2 fajos · 10.000"));
            Assert.That(HudText.Carry(Denomination.Bag, 1, 50000), Is.EqualTo("1 bolsa · 50.000"));
            Assert.That(HudText.Carry(Denomination.GoldBar, 3, 1500000), Is.EqualTo("3 lingotes · 1.500.000"));
            Assert.That(HudText.CarryLimit(Denomination.Bundle, 2, 5), Is.EqualTo("Capacidad 2/5"));
            Assert.That(HudText.CarryLimit(Denomination.Bundle, 5, 5), Is.EqualTo("No puedes llevar más fajos"));
            Assert.That(HudText.CarryLimit(Denomination.Bag, 3, 3), Is.EqualTo("No puedes llevar más bolsas"));
        }

        [Test]
        public void ThePreparedSelfBet_IsAProjection_NotMoneyWon()
        {
            Assert.That(HudText.SelfBetTitle(1), Is.EqualTo("COMODÍN PREPARADO · B"));
            var projection = HudText.Projection(60000, 3);
            Assert.That(projection, Does.Contain("Si aciertas").And.Contain("60.000 × 3").And.Contain("180.000"));
            Assert.That(HudText.SelfBets(1, 2), Does.Contain("Comodines").And.Contain("reserva"));
        }

        [Test]
        public void EveryText_UsesOnlyCharactersTheGameFontsHave()
        {
            // Las tipografías del juego no tienen «→» ni «✓» (saldrían como cuadrados).
            foreach (var text in new[]
                     {
                         HudText.Projection(60000, 3), HudText.AtRisk(30000), HudText.SelfBets(1, 2), HudText.SelfBetTitle(2),
                         HudText.Carry(Denomination.Bag, 2, 100000), HudText.CarryLimit(Denomination.GoldBar, 2, 2), HudText.Bonus(3),
                     })
                Assert.That(text, Does.Not.Contain("→").And.Not.Contain("✓").And.Not.Contain("−"), text);
        }

        [Test]
        public void MoneyOutsideTheTrays_WarnsItIsLostOnVerify() =>
            Assert.That(HudText.AtRisk(30000), Does.Contain("30.000").And.Contain("Se pierde al verificar"));
    }
}
