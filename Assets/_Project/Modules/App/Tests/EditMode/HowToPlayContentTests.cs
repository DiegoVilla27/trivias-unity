using System.Text.RegularExpressions;
using NUnit.Framework;
using Trivias.Menu.Data;
using Trivias.Menu.Editor;
using UnityEditor;

namespace Trivias.App.Tests
{
    /// <summary>El tutorial «Cómo se juega» está completo y explica lo que de verdad hace el juego.</summary>
    public sealed class HowToPlayContentTests
    {
        private static HowToPlayContent Content() => AssetDatabase.LoadAssetAtPath<HowToPlayContent>(MenuPrefabBuilder.HOW_TO_PLAY_ASSET);

        private static string AllText()
        {
            var content = Content();
            var all = string.Empty;
            for (var i = 0; i < content.Count; i++)
                all += content[i].Title + "\n" + content[i].Body + "\n";
            return all;
        }

        [Test]
        public void HasSixPages_WithTitleAndText()
        {
            var content = Content();
            Assert.That(content.Count, Is.GreaterThanOrEqualTo(6));
            for (var i = 0; i < content.Count; i++)
            {
                Assert.That(content[i].Title, Is.Not.Empty, $"Página {i + 1} sin título.");
                Assert.That(content[i].Body.Length, Is.GreaterThan(120), $"Página {i + 1} casi vacía.");
            }
        }

        [Test]
        public void ExplainsEveryControl()
        {
            var text = AllText();
            // Las teclas van entre corchetes: la pantalla las dibuja como las teclas del juego.
            foreach (var key in new[] { "[E]", "[Q]", "[R]", "[Esc]", "[W] [A] [S] [D]", "[Ratón]", "[Shift]", "[Espacio]" })
                Assert.That(text, Does.Contain(key), $"El tutorial no explica {key}.");
        }

        [Test]
        public void ExplainsTheCoreRules()
        {
            var text = AllText().ToLowerInvariant();
            foreach (var idea in new[] { "15 preguntas", "verificar", "cabina correcta", "se pierde", "multiplica", "expulsado", "fichas", "tienda" })
                Assert.That(text, Does.Contain(idea), $"El tutorial no explica «{idea}».");
        }

        [Test]
        public void DoesNotMentionThingsThatDoNotExist()
        {
            var text = AllText().ToLowerInvariant();
            foreach (var missing in new[] { "trampilla", "mayús" })
                Assert.That(text, Does.Not.Contain(missing), $"El juego no tiene «{missing}».");
        }

        [Test]
        public void ExplainsHowToRun()
        {
            var text = AllText().ToLowerInvariant();
            Assert.That(text, Does.Contain("shift").And.Contain("correr"), "Correr (Shift) se explica en los controles.");
        }

        [Test]
        public void EveryPage_ReadsAtAGlance()
        {
            var content = Content();
            for (var i = 0; i < content.Count; i++)
            {
                var visible = Regex.Replace(content[i].Body, "<[^>]+>", string.Empty);
                Assert.That(visible.Length, Is.LessThanOrEqualTo(220), $"Página {i + 1}: demasiado texto, nadie lo leerá.");
                Assert.That(visible.Split('\n').Length, Is.LessThanOrEqualTo(7), $"Página {i + 1}: demasiadas líneas.");
            }
        }

        [Test]
        public void ExplainsHowToJump()
        {
            var text = AllText().ToLowerInvariant();
            Assert.That(text, Does.Contain("espacio").And.Contain("saltar"), "El salto (Espacio) se explica en los controles.");
        }

        [Test]
        public void RichTextTags_AreBalanced()
        {
            var content = Content();
            for (var i = 0; i < content.Count; i++)
            {
                var body = content[i].Body;
                foreach (var tag in new[] { "b", "i", "color" })
                {
                    var open = Regex.Matches(body, $"<{tag}[=>]").Count;
                    var close = Regex.Matches(body, $"</{tag}>").Count;
                    Assert.That(close, Is.EqualTo(open), $"Página {i + 1}: etiqueta <{tag}> sin cerrar.");
                }
            }
        }
    }
}
