using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Trivias.Shared.Presentation;

namespace Trivias.Shared.Tests
{
    public sealed class UiKeyedTextTests
    {
        private static (string Text, List<(int First, int Count)> Tokens) Markup(string source)
        {
            var tokens = new List<(int First, int Count)>();
            return (UiKeyedText.Markup(source, new StringBuilder(), tokens, "F5EFE1"), tokens);
        }

        [Test]
        public void EachBracketedKey_BecomesAKey_WithRoomAround()
        {
            var (text, tokens) = Markup("[E] coge uno. [Shift] corre");
            Assert.That(text, Does.Not.Contain("[").And.Not.Contain("]"));
            Assert.That(text, Does.Contain("<space=").And.Contain(">E<").And.Contain(">Shift<"));
            // Caracteres visibles: «E», « coge uno. », «Shift»…
            Assert.That(tokens, Is.EqualTo(new[] { (0, 1), (12, 5) }));
        }

        [Test]
        public void FormatTags_DoNotCountAsCharacters()
        {
            var (_, tokens) = Markup("Pulsa <b>VERIFICAR</b> con [E].");
            Assert.That(tokens, Is.EqualTo(new[] { (20, 1) }), "«Pulsa VERIFICAR con » son 20 caracteres.");
        }

        [Test]
        public void NewLines_Count_AndPlainTextStaysAsItIs()
        {
            var (_, tokens) = Markup("Uno\n[Esc] menú");
            Assert.That(tokens, Is.EqualTo(new[] { (4, 3) }));
            Assert.That(Markup("Sin teclas [ni esto").Text, Is.EqualTo("Sin teclas [ni esto"), "Un corchete suelto no es una tecla.");
        }
    }
}
