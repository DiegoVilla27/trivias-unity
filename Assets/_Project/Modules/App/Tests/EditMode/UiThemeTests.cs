using System.Linq;
using NUnit.Framework;
using TMPro;
using Trivias.Shared.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.App.Tests
{
    /// <summary>Toda la interfaz con el estilo del plató: tipografías, paneles con marco dorado, botones con tacto.</summary>
    public sealed class UiThemeTests
    {
        private static readonly string[] UiPrefabs =
        {
            "Assets/_Project/Modules/Menu/Prefabs/Menu.prefab",
            "Assets/_Project/Modules/HUD/Prefabs/MatchHud.prefab",
            "Assets/_Project/Modules/Results/Prefabs/ResultsScreen.prefab",
            "Assets/_Project/Modules/Shop/Prefabs/Shop.prefab",
            "Assets/_Project/Modules/Interaction/Prefabs/InteractionHud.prefab",
            "Assets/_Project/Modules/Spectator/Prefabs/Spectator.prefab",
        };

        private static GameObject Load(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, path);
            return prefab;
        }

        [Test]
        public void EveryText_UsesTheGameFonts()
        {
            foreach (var path in UiPrefabs)
            {
                foreach (var text in Load(path).GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    Assert.That(text.font.name, Does.StartWith("Poppins").Or.StartWith("LilitaOne"), $"{path}: {text.name}");
                    Assert.That(text.enableAutoSizing, Is.True, $"{path}: {text.name} encoge si no cabe.");
                }
            }
        }

        [Test]
        public void EveryButton_IsRounded_AndRespondsToTheMouse()
        {
            foreach (var path in UiPrefabs)
            {
                foreach (var button in Load(path).GetComponentsInChildren<Button>(true))
                {
                    var image = (Image)button.targetGraphic;
                    Assert.That(image.sprite, Is.Not.Null, $"{path}: {button.name} redondeado.");
                    Assert.That(image.type, Is.EqualTo(Image.Type.Sliced));
                    Assert.That(button.GetComponent<UiButtonFeel>(), Is.Not.Null, $"{path}: {button.name} con tacto.");
                }
            }
        }

        [Test]
        public void MainPanels_HaveAGoldFrame_AndPopIn()
        {
            var menu = Load(UiPrefabs[0]);
            foreach (var name in new[] { "MainMenu", "JoinMenu", "Lobby" })
            {
                var panel = menu.transform.Find(name);
                Assert.That(panel.Find(Trivias.Shared.Editor.UiTheme.FRAME), Is.Not.Null, $"{name}: marco dorado.");
                Assert.That(panel.GetComponent<UiPanelIntro>(), Is.Not.Null, $"{name}: aparece con rebote.");
            }
            var results = Load(UiPrefabs[2]).transform.Find("Panel");
            Assert.That(results.GetComponent<UiPanelIntro>(), Is.Not.Null);
        }

        [Test]
        public void MainMenu_HasAMarqueeOfBulbs_AndABeatingLogo()
        {
            var main = Load(UiPrefabs[0]).transform.Find("MainMenu");
            var marquee = main.GetComponentInChildren<UiMarquee>(true);
            Assert.That(marquee, Is.Not.Null);
            Assert.That(marquee.Count, Is.GreaterThanOrEqualTo(40), "Bombillas alrededor del panel.");
            var logo = main.Find("Title").GetComponent<TextMeshProUGUI>();
            Assert.That(logo.font.name, Does.StartWith("LilitaOne"));
            Assert.That(logo.enableVertexGradient, Is.True, "Degradado dorado, como el rótulo del plató.");
            Assert.That(logo.GetComponent<UiPulse>(), Is.Not.Null, "El logo late.");
            Assert.That(Load(UiPrefabs[2]).GetComponentInChildren<UiMarquee>(true), Is.Not.Null, "Resultados con bombillas.");
        }

        [Test]
        public void FullScreenDims_StayPlain()
        {
            foreach (var image in Load(UiPrefabs[0]).GetComponentsInChildren<Image>(true)
                         .Where(i => i.rectTransform.anchorMin == Vector2.zero && i.rectTransform.anchorMax == Vector2.one &&
                                     i.transform.parent != null && i.transform.parent.GetComponent<Canvas>() != null))
                Assert.That(image.sprite, Is.Null, $"{image.name}: el fondo oscuro de pantalla completa no se redondea.");
        }
    }
}
