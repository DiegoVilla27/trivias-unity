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
                    // Los botones de flecha son un icono redondo; el resto, formas que se estiran sin deformarse.
                    var isIcon = AssetDatabase.GetAssetPath(image.sprite).Contains("/UI/Icons/");
                    Assert.That(isIcon || image.type == Image.Type.Sliced, Is.True, $"{path}: {button.name}");
                    Assert.That(button.GetComponent<UiButtonFeel>(), Is.Not.Null, $"{path}: {button.name} con tacto.");
                }
            }
        }

        [Test]
        public void MainPanels_HaveAGoldFrame_AndPopIn()
        {
            var menu = Load(UiPrefabs[0]);
            foreach (var name in new[] { "MainScreen/MainMenu", "MainScreen/CharacterPanel", "JoinMenu/Panel", "Lobby/Players", "Lobby/Actions" })
            {
                var panel = menu.transform.Find(name);
                Assert.That(panel.GetComponent<Image>().sprite, Is.EqualTo(Trivias.Shared.Editor.UiKit.PanelSprite()), $"{name}: marco dorado.");
                Assert.That(panel.GetComponent<UiPanelIntro>(), Is.Not.Null, $"{name}: aparece con rebote.");
            }
            var results = Load(UiPrefabs[2]).transform.Find("Panel");
            Assert.That(results.GetComponent<UiPanelIntro>(), Is.Not.Null);
        }

        [Test]
        public void MainMenu_HasTheGoldenLogo_AndResultsKeepTheirBulbs()
        {
            // Como el boceto: el rótulo dorado de TRIVIAS (imagen) sobre el panel; las bombillas, en el plató de fondo.
            var logo = Load(UiPrefabs[0]).transform.Find("MainScreen/Title").GetComponent<Image>();
            Assert.That(AssetDatabase.GetAssetPath(logo.sprite), Does.EndWith("Title_Trivias.png"));
            Assert.That(logo.preserveAspect, Is.True);
            Assert.That(Load(UiPrefabs[2]).GetComponentInChildren<UiMarquee>(true), Is.Not.Null, "Resultados con bombillas.");
        }

        [Test]
        public void MainScreen_MenuOnTheLeft_CharacterOnTheRight_WithoutOverlapping()
        {
            var screen = Load(UiPrefabs[0]).transform.Find("MainScreen");
            var menu = (RectTransform)screen.Find("MainMenu");
            var character = (RectTransform)screen.Find("CharacterPanel");
            // Medidas desde la esquina superior izquierda de un lienzo de 1920 × 1080 (como los bocetos).
            var menuRight = menu.anchoredPosition.x + menu.sizeDelta.x;
            Assert.That(menuRight, Is.LessThan(960f), "El menú, a la izquierda.");
            Assert.That(character.anchoredPosition.x, Is.GreaterThan(menuRight + 40f), "El personaje, a la derecha y sin taparse.");
            Assert.That(character.anchoredPosition.x + character.sizeDelta.x, Is.LessThanOrEqualTo(1920f - 64f), "Con margen de seguridad.");
            Assert.That(character.GetComponentInChildren<RawImage>(true), Is.Not.Null, "Con el personaje en 3D.");
        }

        [Test]
        public void RoundedPieces_NeverHaveCornersBiggerThanHalfTheirSize()
        {
            // Pistas de deslizador, pastillas e interruptores: extremos de píldora, sin esquinas cortadas.
            foreach (var path in new[] { UiPrefabs[0], UiPrefabs[3] })
            {
                foreach (var image in Load(path).GetComponentsInChildren<Image>(true))
                {
                    if (image.type != Image.Type.Sliced || image.sprite == null || image.rectTransform.anchorMin != image.rectTransform.anchorMax)
                        continue;
                    var border = Mathf.Max(image.sprite.border.x, image.sprite.border.y);
                    var smallest = Mathf.Min(image.rectTransform.sizeDelta.x, image.rectTransform.sizeDelta.y);
                    Assert.That(border / image.pixelsPerUnitMultiplier, Is.LessThanOrEqualTo(smallest * 0.5f + 0.01f),
                        $"{path}: {image.name} tiene las esquinas más grandes que la pieza.");
                }
            }
        }

        [Test]
        public void SliderTracks_ArePills()
        {
            foreach (var slider in Load(UiPrefabs[0]).GetComponentsInChildren<Slider>(true))
            {
                var track = slider.transform.Find("Track").GetComponent<Image>();
                var radius = track.sprite.border.x / track.pixelsPerUnitMultiplier;
                Assert.That(radius, Is.EqualTo(track.rectTransform.sizeDelta.y * 0.5f).Within(1f), $"{slider.name}: extremos redondos.");
            }
        }

        [Test]
        public void WhatOpensFromThePause_GoesOnTopOfIt()
        {
            var menu = Load(UiPrefabs[0]).transform;
            var pause = menu.Find("Pause").GetSiblingIndex();
            foreach (var overlay in new[] { "HowToPlay", "Settings", "Records", "ConfirmLeave" })
                Assert.That(menu.Find(overlay).GetSiblingIndex(), Is.GreaterThan(pause), $"{overlay} se ve encima de la pausa.");
        }

        [Test]
        public void ImagesThatKeepTheirShape_AreCentredInTheirSpace()
        {
            foreach (var image in Load(UiPrefabs[0]).GetComponentsInChildren<Image>(true))
            {
                if (image.preserveAspect)
                    Assert.That(image.rectTransform.pivot, Is.EqualTo(new Vector2(0.5f, 0.5f)), $"{image.name}: centrada.");
            }
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
