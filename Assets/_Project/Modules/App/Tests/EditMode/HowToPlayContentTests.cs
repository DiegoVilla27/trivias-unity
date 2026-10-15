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
            foreach (var key in new[] { "<b>E</b>", "<b>Q</b>", "<b>R</b>", "<b>Esc</b>", "WASD", "Ratón" })
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
        public void DoesNotMentionControlsThatDoNotExist()
        {
            var text = AllText().ToLowerInvariant();
            foreach (var missing in new[] { "saltar", "correr", "espacio", "mayús" })
                Assert.That(text, Does.Not.Contain(missing), $"El juego no tiene «{missing}».");
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
