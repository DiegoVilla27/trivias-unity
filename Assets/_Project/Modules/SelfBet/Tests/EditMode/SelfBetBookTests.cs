using System.Collections.Generic;
using NUnit.Framework;
using Trivias.SelfBet.Logic;

namespace Trivias.SelfBet.Tests
{
    public sealed class SelfBetBookTests
    {
        private const int A = 0, B = 1, C = 2;
        private readonly List<ulong> _eliminated = new();

        // NUnit reutiliza la misma instancia en todos los tests: se vacía antes de cada uno.
        [SetUp]
        public void ClearEliminated() => _eliminated.Clear();

        private static SelfBetBook BookWith(int players)
        {
            var clients = new List<ulong>();
            for (var i = 0; i < players; i++)
                clients.Add((ulong)i);
            var book = new SelfBetBook();
            book.Start(clients);
            return book;
        }

        [TestCase(2, 3, 0)]
        [TestCase(3, 2, 0)]
        [TestCase(4, 1, 2)]
        [TestCase(5, 1, 1)]
        [TestCase(6, 1, 0)]
        [TestCase(1, 6, 0)]
        public void Distribution_FollowsGddTable(int players, int personal, int reserve)
        {
            SelfBetDistribution.For(players, out var p, out var r);
            Assert.That(p, Is.EqualTo(personal));
            Assert.That(r, Is.EqualTo(reserve));
            Assert.That(p * players + r, Is.EqualTo(players == 1 ? 6 : SelfBetDistribution.TEAM_USES));
        }

        [Test]
        public void Prepare_ThenCancel_DoesNotConsumeUse()
        {
            var book = BookWith(2);
            Assert.That(book.Prepare(0, A), Is.True);
            Assert.That(book.Cancel(0), Is.True);
            book.LockAll();
            Assert.That(book.Bettors[0].PersonalUses, Is.EqualTo(3));
        }

        [Test]
        public void ChangingBooth_KeepsSingleBet()
        {
            var book = BookWith(2);
            book.Prepare(0, A);
            book.Prepare(0, B);
            Assert.That(book.CountOn(A), Is.Zero);
            Assert.That(book.CountOn(B), Is.EqualTo(1));
            book.LockAll();
            Assert.That(book.Bettors[0].PersonalUses, Is.EqualTo(2), "Un solo uso por pregunta.");
        }

        [Test]
        public void Lock_ConsumesUse_AndForbidsChanges()
        {
            var book = BookWith(2);
            book.Prepare(0, A);
            book.LockAll();
            Assert.That(book.Bettors[0].PersonalUses, Is.EqualTo(2));
            Assert.That(book.Cancel(0), Is.False, "No se cancela tras verificar.");
            Assert.That(book.Prepare(0, B), Is.False, "No se cambia tras verificar.");
        }

        [Test]
        public void Resolve_MultiplierCountsBettorsOnCorrectBooth()
        {
            var book = BookWith(3);
            book.Prepare(0, B);
            book.Prepare(1, B);
            book.Prepare(2, C);
            book.LockAll();

            var multiplier = book.Resolve(B, _eliminated);
            Assert.That(multiplier, Is.EqualTo(3), "1 + 2 apostados en la correcta.");
            Assert.That(_eliminated, Is.EqualTo(new ulong[] { 2 }));
            Assert.That(book.ActivePlayers, Is.EqualTo(2));
        }

        [Test]
        public void Resolve_WithoutBets_IsTimesOne()
        {
            var book = BookWith(2);
            book.LockAll();
            Assert.That(book.Resolve(A, _eliminated), Is.EqualTo(1));
            Assert.That(_eliminated, Is.Empty);
        }

        [Test]
        public void FailedBet_EliminatesEvenWithUsesLeft_AndLosesThem()
        {
            var book = BookWith(2); // 3 usos cada uno
            book.Prepare(0, A);
            book.LockAll();
            book.Resolve(B, _eliminated);
            Assert.That(book.Bettors[0].IsEliminated, Is.True);
            Assert.That(book.Bettors[0].PersonalUses, Is.Zero);
            Assert.That(book.Prepare(0, A), Is.False, "Un eliminado no puede apostarse.");
        }

        [Test]
        public void CorrectBet_StaysActiveAndCanBetAgain()
        {
            var book = BookWith(2);
            book.Prepare(0, A);
            book.LockAll();
            book.Resolve(A, _eliminated);
            Assert.That(book.Bettors[0].IsEliminated, Is.False);
            Assert.That(book.Bettors[0].HasBet, Is.False, "La apuesta se limpia para la siguiente pregunta.");
            Assert.That(book.Prepare(0, B), Is.True);
        }

        [Test]
        public void Reserve_IsUsedOnlyWithoutPersonalUses()
        {
            var book = BookWith(4); // 1 personal cada uno + 2 de reserva
            book.Prepare(0, A);
            book.LockAll();
            book.Resolve(A, _eliminated);
            Assert.That(book.Bettors[0].PersonalUses, Is.Zero);

            Assert.That(book.Prepare(0, A), Is.True, "Sin personales, usa la reserva.");
            Assert.That(book.Reserve, Is.EqualTo(1));
            book.Cancel(0);
            Assert.That(book.Reserve, Is.EqualTo(2), "Cancelar devuelve el uso a la reserva.");
        }

        [Test]
        public void LastReserveUse_CannotBeTakenTwice()
        {
            var book = BookWith(5); // 1 personal cada uno + 1 de reserva
            for (ulong client = 0; client < 2; client++)
            {
                book.Prepare(client, A);
            }
            book.LockAll();
            book.Resolve(A, _eliminated);

            Assert.That(book.Prepare(0, B), Is.True);
            Assert.That(book.Prepare(1, B), Is.False, "Solo queda un uso en la reserva.");
        }

        [Test]
        public void AllActiveHaveBet_OnlyWhenNobodyIsFree()
        {
            var book = BookWith(2);
            Assert.That(book.AllActiveHaveBet, Is.False);
            book.Prepare(0, A);
            Assert.That(book.AllActiveHaveBet, Is.False, "Queda uno libre para pulsar el botón.");
            book.Prepare(1, B);
            Assert.That(book.AllActiveHaveBet, Is.True);
        }

        [Test]
        public void AllActiveHaveBet_IgnoresEliminated()
        {
            var book = BookWith(2);
            book.Prepare(0, A);
            book.LockAll();
            book.Resolve(B, _eliminated); // el 0 queda eliminado
            book.Prepare(1, C);
            Assert.That(book.AllActiveHaveBet, Is.True);
        }

        // ---- Desconexiones ----

        [Test]
        public void RemovedPlayer_IsNoLongerActive_NorEliminated()
        {
            var book = BookWith(3);
            Assert.That(book.RemovePlayer(1), Is.True);
            Assert.That(book.ActivePlayers, Is.EqualTo(2));
            Assert.That(book.Bettors[1].HasLeft, Is.True);
            Assert.That(book.Bettors[1].IsEliminated, Is.False, "Irse no es lo mismo que ser expulsado.");
            Assert.That(book.RemovePlayer(1), Is.False, "No se cuenta dos veces.");
        }

        [Test]
        public void RemovedPlayer_DoesNotMultiplyEvenIfLockedOnCorrectBooth()
        {
            var book = BookWith(2);
            book.Prepare(0, A);
            book.Prepare(1, A);
            book.LockAll();
            book.RemovePlayer(1);
            Assert.That(book.Resolve(A, _eliminated), Is.EqualTo(2), "Solo cuenta el que sigue: x2, no x3.");
            Assert.That(_eliminated, Is.Empty);
        }

        [Test]
        public void RemovedPlayer_OnWrongBooth_IsNotExpelled()
        {
            var book = BookWith(2);
            book.Prepare(1, B);
            book.LockAll();
            book.RemovePlayer(1);
            book.Resolve(A, _eliminated);
            Assert.That(_eliminated, Is.Empty);
        }

        [Test]
        public void RemovedPlayer_ReturnsReservedUseToTheTeam()
        {
            var book = BookWith(4); // 1 personal + 2 de reserva
            book.Prepare(0, A);
            book.LockAll();
            book.Resolve(A, _eliminated);       // el 0 gasta su uso personal
            book.Prepare(0, B);                 // ahora usa la reserva (queda 1)
            Assert.That(book.Reserve, Is.EqualTo(1));
            book.RemovePlayer(0);
            Assert.That(book.Reserve, Is.EqualTo(2), "El uso reservado y no gastado vuelve al equipo.");
        }

        [Test]
        public void RemovedPlayer_DoesNotRecalculateOthersUses()
        {
            var book = BookWith(2); // 3 cada uno
            book.RemovePlayer(1);
            Assert.That(book.Bettors[0].PersonalUses, Is.EqualTo(3), "El reparto no se recalcula al irse alguien.");
        }

        [Test]
        public void RemovedPlayer_CannotBet_AndDoesNotBlockVerifyFromZone()
        {
            var book = BookWith(2);
            book.RemovePlayer(1);
            Assert.That(book.Prepare(1, A), Is.False);
            book.Prepare(0, A);
            Assert.That(book.AllActiveHaveBet, Is.True, "Quien se fue no cuenta como libre para pulsar el botón.");
        }

        [Test]
        public void Elimination_DoesNotIncreaseReserve()
        {
            var book = BookWith(5);
            var before = book.Reserve;
            book.Prepare(0, A);
            book.LockAll();
            book.Resolve(B, _eliminated);
            Assert.That(book.Reserve, Is.EqualTo(before));
        }
    }
}
