using System.Collections.Generic;
using NUnit.Framework;
using Trivias.Shared.Interaction;

namespace Trivias.Shared.Tests
{
    public sealed class PromptTextTests
    {
        [Test]
        public void APrompt_SplitsIntoItsKeysAndActions()
        {
            var prompt = PromptText.Join(PromptText.Join(PromptText.Key("E", "Coger fajo (5.000)"), PromptText.Key("Q", "Coger 5")),
                PromptText.Key("R", "Devolver 2"));
            var parts = new List<(string Key, string Action)>();
            PromptText.Split(prompt, parts);
            Assert.That(parts, Is.EqualTo(new[] { ("E", "Coger fajo (5.000)"), ("Q", "Coger 5"), ("R", "Devolver 2") }));
            Assert.That(prompt, Does.Contain("Coger fajo (5.000)"), "Se puede buscar una acción en el texto.");
        }

        [Test]
        public void AnActionWithoutKey_KeepsItsText()
        {
            var parts = new List<(string Key, string Action)>();
            PromptText.Split(PromptText.Join("Apuestas cerradas", PromptText.Key("Esc", "Menú")), parts);
            Assert.That(parts, Is.EqualTo(new[] { ("", "Apuestas cerradas"), ("Esc", "Menú") }));
        }

        [Test]
        public void AnEmptyPrompt_HasNoActions()
        {
            var parts = new List<(string Key, string Action)> { ("X", "vieja") };
            PromptText.Split(string.Empty, parts);
            Assert.That(parts, Is.Empty);
            Assert.That(PromptText.Join(string.Empty, PromptText.Key("E", "Uno")), Is.EqualTo(PromptText.Key("E", "Uno")));
        }

        [Test]
        public void ThePlainReading_ListsKeyAndAction() =>
            Assert.That(PromptText.Plain(PromptText.Join(PromptText.Key("E", "Dejar uno"), PromptText.Key("Q", "Dejar varios"))),
                Is.EqualTo("E Dejar uno · Q Dejar varios"));
    }
}
