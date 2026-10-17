using NUnit.Framework;
using Trivias.Shared.Presentation;
using UnityEngine;

namespace Trivias.Shared.Tests
{
    /// <summary>Fondo de los menús: lo piden las pantallas; las ventanas de encima, además, tapan el menú de detrás.</summary>
    public sealed class UiBackdropTests
    {
        [Test]
        public void ShowsWhileAnyScreenAsks_AndCoversOnlyWithAWindowOnTop()
        {
            var backdrop = ScriptableObject.CreateInstance<UiBackdrop>();
            var menu = ScriptableObject.CreateInstance<UiBackdrop>();
            var shop = ScriptableObject.CreateInstance<UiBackdrop>();
            var changes = 0;
            backdrop.Changed += () => changes++;
            try
            {
                Assert.That(backdrop.IsShown, Is.False);
                backdrop.Show(menu, true);
                Assert.That(backdrop.IsShown, Is.True);
                Assert.That(backdrop.IsCovered, Is.False, "El menú se ve.");

                backdrop.Cover(shop, true);
                Assert.That(backdrop.IsCovered, Is.True, "La tienda tapa la sala de espera.");
                backdrop.Show(menu, false);
                Assert.That(backdrop.IsShown, Is.True, "Con la tienda abierta sigue el fondo.");
                backdrop.Cover(shop, false);
                Assert.That(backdrop.IsShown, Is.False);

                backdrop.Cover(shop, false);
                Assert.That(changes, Is.EqualTo(4), "Solo avisa cuando cambia algo.");
            }
            finally
            {
                Object.DestroyImmediate(backdrop);
                Object.DestroyImmediate(menu);
                Object.DestroyImmediate(shop);
            }
        }
    }
}
