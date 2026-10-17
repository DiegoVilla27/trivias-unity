using System.Collections;
using NUnit.Framework;
using Trivias.Shared.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Trivias.App.Tests
{
    /// <summary>El tacto de los botones es suave: crece apenas, sin saltos, y al pulsar se hunde un poco.</summary>
    public sealed class ButtonFeelTests
    {
        private GameObject _canvas;
        private UiButtonFeel _feel;

        [SetUp]
        public void SetUp()
        {
            _canvas = new GameObject("Canvas", typeof(Canvas));
            var button = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(UiButtonFeel));
            button.transform.SetParent(_canvas.transform, false);
            ((RectTransform)button.transform).sizeDelta = new Vector2(300f, 80f);
            _feel = button.GetComponent<UiButtonFeel>();
        }

        [TearDown]
        public void TearDown() => Object.Destroy(_canvas);

        [UnityTest]
        public IEnumerator Hover_GrowsALittle_Smoothly_AndPressSinksGently()
        {
            yield return null;
            Assert.That(_feel.HoverScale, Is.InRange(1.005f, 1.03f), "Crece poco.");
            Assert.That(_feel.PressedScale, Is.InRange(0.97f, 0.995f), "Se hunde poco.");

            _feel.OnPointerEnter(null);
            yield return null;
            Assert.That(_feel.Current, Is.GreaterThan(1f).And.LessThan(_feel.HoverScale - 0.002f), "Sin salto: va subiendo.");
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.That(_feel.Current, Is.EqualTo(_feel.HoverScale).Within(0.001f), "Y llega a su sitio.");

            _feel.OnPointerDown(null);
            yield return new WaitForSecondsRealtime(0.3f);
            Assert.That(_feel.Current, Is.EqualTo(_feel.PressedScale).Within(0.001f));

            _feel.OnPointerUp(null);
            _feel.OnPointerExit(null);
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.That(_feel.Current, Is.EqualTo(1f).Within(0.001f), "Vuelve a su tamaño.");
        }
    }
}
