using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Trivias.Menu.Components;
using Trivias.Networking.Components;
using Trivias.Player.Components;
using Trivias.Shared.Characters;
using Trivias.Shared.Presentation;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Trivias.App.Tests
{
    /// <summary>Pantallas del menú como los bocetos de docs/bocetos/ui/menu: sala de espera, salir, tutorial, configuración y retratos.</summary>
    public sealed class MenuScreensTests
    {
        private const BindingFlags PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic;

        private MenuView _menu;
        private ConnectionService _connection;

        [UnitySetUp]
        public IEnumerator LoadStage()
        {
            var profile = TemporaryProfileSetup.FreshProfile();
            profile.SetName("Diego");
            profile.AwardMatch("regalo", 1500);
            yield return SceneManager.LoadSceneAsync("Stage", LoadSceneMode.Single);
            _menu = Object.FindAnyObjectByType<MenuView>();
            _connection = Object.FindAnyObjectByType<ConnectionService>();
        }

        [UnityTearDown]
        public IEnumerator Shutdown()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
                Object.Destroy(NetworkManager.Singleton.gameObject);
            }
            yield return null;
        }

        private T Field<T>(object target, string name) => (T)target.GetType().GetField(name, PRIVATE).GetValue(target);

        [UnityTest]
        public IEnumerator Lobby_HasSixSeats_TheHostFirst_TheRestWaiting()
        {
            Assert.That(_connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            yield return null;

            var seats = Field<LobbyPlayerRow[]>(_menu, "_rows");
            Assert.That(seats, Has.Length.EqualTo(6));
            Assert.That(seats[0].IsFilled, Is.True);
            Assert.That(seats[0].NameText, Is.EqualTo("Diego"));
            Assert.That(seats[0].CharacterText, Is.EqualTo("Blue Tracksuit"), "Con el nombre de su personaje.");
            Assert.That(seats[0].IsHost, Is.True, "Insignia de anfitrión.");
            Assert.That(seats.Skip(1).All(s => !s.IsFilled), Is.True, "«Esperando jugador…» en las demás plazas.");
            Assert.That(Field<TextMeshProUGUI>(_menu, "_playersTitle").text, Does.Contain("1/6"));
            Assert.That(Field<TextMeshProUGUI>(_menu, "_meName").text, Does.Contain("Diego").And.Contain("Anfitrión"));
            Assert.That(Field<TextMeshProUGUI>(_menu, "_tokens").text, Is.EqualTo("1.500 fichas"));
            Assert.That(Field<RawImage>(_menu, "_mePortrait").texture, Is.Not.Null, "Con el retrato de tu personaje.");
        }

        [UnityTest]
        public IEnumerator LeavingTheLobby_AsksFirst_AndWarnsTheHost()
        {
            Assert.That(_connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            var pause = Object.FindAnyObjectByType<PauseMenuView>();

            Field<Button>(_menu, "_leaveButton").onClick.Invoke();
            yield return null;
            Assert.That(pause.IsConfirmingLeave, Is.True, "Primero pregunta.");
            Assert.That(Field<GameObject>(pause, "_hostWarning").activeSelf, Is.True, "Al anfitrión le avisa de que se cierra para todos.");

            Field<Button>(pause, "_cancelLeaveButton").onClick.Invoke();
            yield return null;
            Assert.That(pause.IsConfirmingLeave, Is.False);
            Assert.That(_menu.IsLobbyOpen, Is.True, "«Quedarme» deja todo como estaba.");
        }

        [UnityTest]
        public IEnumerator HowToPlay_StepsJumpToTheirPage_WithItsIllustration()
        {
            var view = Object.FindAnyObjectByType<HowToPlayView>(FindObjectsInactive.Include);
            view.Open();
            yield return null;
            Assert.That(_menu.IsMainOpen, Is.False, "El tutorial tapa el menú de detrás.");

            Field<Button[]>(view, "_steps")[3].onClick.Invoke();
            Assert.That(view.Page, Is.EqualTo(3), "El paso 4 (Comodín) lleva a su página.");
            var illustrations = Field<Sprite[]>(view, "_illustrations");
            Assert.That(illustrations, Has.Length.EqualTo(view.PageCount).And.All.Not.Null, "Una ilustración por página.");
            var illustration = Field<Image>(view, "_illustration");
            Assert.That(illustration.sprite, Is.SameAs(illustrations[3]));
            Assert.That(illustration.type, Is.EqualTo(Image.Type.Simple), "La ilustración no se estira…");
            Assert.That(illustration.preserveAspect, Is.True, "…guarda su proporción.");
            Assert.That(Field<TextMeshProUGUI>(view, "_title").text, Is.EqualTo("APUÉSTATE A TI MISMO"));

            Field<Button[]>(view, "_steps")[5].onClick.Invoke();
            Assert.That(Field<TextMeshProUGUI>(view, "_nextLabel").text, Is.EqualTo("Entendido"), "En la última, «Entendido».");
            view.Close();
            yield return null;
            Assert.That(_menu.IsMainOpen, Is.True, "Al cerrar vuelve el menú.");
        }

        [UnityTest]
        public IEnumerator Settings_SwitchesSayYesOrNo()
        {
            var view = Object.FindAnyObjectByType<SettingsView>(FindObjectsInactive.Include);
            view.Open();
            yield return null;
            var fullscreen = Field<Toggle>(view, "_fullscreen");
            var state = fullscreen.GetComponentsInChildren<TextMeshProUGUI>(true).First(t => t.name == "State");
            fullscreen.isOn = true;
            Assert.That(state.text, Is.EqualTo("Sí"));
            fullscreen.isOn = false;
            Assert.That(state.text, Is.EqualTo("No"));
            view.Close();
        }

        [UnityTest]
        public IEnumerator BigButtons_BarelyGrowOnHover_SoTheyNeverLeaveTheirPanel()
        {
            yield return null;
            foreach (var feel in Object.FindObjectsByType<UiButtonFeel>(FindObjectsInactive.Include))
            {
                var rect = (RectTransform)feel.transform;
                var grow = (feel.HoverScale - 1f) * Mathf.Max(rect.rect.width, rect.rect.height) * 0.5f;
                Assert.That(grow, Is.LessThanOrEqualTo(5.01f), $"{feel.name}: crece {grow:0.0} px por lado.");
            }
        }

        [UnityTest]
        public IEnumerator EveryCharacter_HasItsPortraits()
        {
            yield return null;
            var showcase = Object.FindAnyObjectByType<CharacterShowcase>();
            var catalog = Field<CharacterCatalog>(showcase, "_catalog");
            var preview = Field<CharacterPreview>(showcase, "_preview");
            Assert.That(showcase.PortraitCount, Is.EqualTo(catalog.Characters.Count));
            foreach (var id in catalog.Ids)
            {
                Assert.That(preview.Face(id), Is.Not.Null, $"{id}: cara (lista de jugadores).");
                Assert.That(preview.Body(id), Is.Not.Null, $"{id}: cuerpo entero (tienda).");
            }
        }

        [UnityTest]
        public IEnumerator TheBackdrop_IsTheRealStage_Blurred()
        {
            yield return null;
            var backdrop = Object.FindAnyObjectByType<MenuBackdropView>();
            Assert.That(backdrop.IsShown, Is.True, "En el menú principal.");
            var blur = Field<UnityEngine.Rendering.Volume>(backdrop, "_blur");
            Assert.That(blur.isGlobal && blur.weight > 0.99f, Is.True);
            Assert.That(blur.sharedProfile.TryGet<UnityEngine.Rendering.Universal.DepthOfField>(out var dof) && dof.active, Is.True,
                "Desenfoque de profundidad de campo.");
        }
    }
}
