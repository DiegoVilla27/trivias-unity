using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Match.Network;
using Trivias.Menu.Components;
using Trivias.Networking.Components;
using Trivias.Shared.Players;
using Trivias.Shared.Profile;
using Trivias.Shop.Components;
using Trivias.Shop.Data;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Trivias.App.Tests
{
    /// <summary>Tienda y fichas (GDD 11): comprar, equipar (lo ven todos), cierre al empezar y entrega única de fichas.</summary>
    public sealed class ShopFlowTests
    {
        private const BindingFlags PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic;

        private ConnectionService _connection;
        private ShopView _shop;
        private PlayerProfile _profile;
        private NetworkObject _player;
        private MatchConfig _config;
        private int[] _originalTiers;

        [UnitySetUp]
        public IEnumerator HostInLobby()
        {
            _profile = TemporaryProfileSetup.FreshProfile();
            yield return SceneManager.LoadSceneAsync("Stage", LoadSceneMode.Single);
            _connection = Object.FindAnyObjectByType<ConnectionService>();
            _shop = Object.FindAnyObjectByType<ShopView>();
            Assert.That(_connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            _player = NetworkManager.Singleton.LocalClient.PlayerObject;
        }

        [UnityTearDown]
        public IEnumerator Shutdown()
        {
            if (_config != null && _originalTiers != null)
                typeof(MatchConfig).GetField("_tokenTierMinQuestions", PRIVATE).SetValue(_config, _originalTiers);

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
                Object.Destroy(NetworkManager.Singleton.gameObject);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator LobbyShopButton_OpensTheShop()
        {
            Assert.That(_shop.IsOpen, Is.False);
            var menu = Object.FindAnyObjectByType<MenuView>();
            ((Button)typeof(MenuView).GetField("_shopButton", PRIVATE).GetValue(menu)).onClick.Invoke();
            yield return null;
            Assert.That(_shop.IsOpen, Is.True);
        }

        [UnityTest]
        public IEnumerator BuyingABowTie_EquipsItUnderTheChin()
        {
            _profile.AwardMatch("regalo", 1000);
            _shop.Open();
            yield return null;

            _shop.Select("pajarita");
            Assert.That(_shop.IsAccessoriesTab, Is.True);
            Assert.That(_shop.ActionText, Is.EqualTo("Comprar y equipar"));
            _shop.PressAction();
            yield return null;
            yield return null;

            Assert.That(_profile.Tokens, Is.EqualTo(700), "La pajarita cuesta 300.");
            Assert.That(_shop.ActionText, Is.EqualTo("Quitar"));
            Assert.That(_profile.Loadout.Neck, Is.EqualTo("pajarita"), "Al comprar se equipa.");
            var avatar = _player.GetComponent<IPlayerAvatar>();
            Assert.That(avatar.Loadout.Neck, Is.EqualTo("pajarita"), "Lo que lleva se publica en red.");
            Assert.That(Worn(avatar, CosmeticSlot.Neck, "BowTie"), Is.True, "La pajarita debe verse bajo la barbilla.");

            _shop.PressAction(); // Ya comprada y puesta: ahora quita.
            yield return null;
            yield return null;
            Assert.That(avatar.Loadout.Neck, Is.Empty);
            Assert.That(Worn(avatar, CosmeticSlot.Neck, "BowTie"), Is.False);
            Assert.That(_profile.Tokens, Is.EqualTo(700), "Quitar no devuelve ni cobra fichas.");
        }

        [UnityTest]
        public IEnumerator FiveSlots_CanBeWornAtOnce_EachInItsPlace()
        {
            _profile.AwardMatch("regalo", 10000);
            _shop.Open();
            yield return null;
            foreach (var id in new[] { "corona", "gafas-sol", "pajarita", "bigote", "alas-dragon" })
            {
                _shop.Select(id);
                _shop.PressAction();
                yield return null;
            }
            yield return null;
            yield return null;

            var avatar = _player.GetComponent<IPlayerAvatar>();
            var loadout = avatar.Loadout;
            Assert.That(new[] { loadout.Head, loadout.Face, loadout.Neck, loadout.Mouth, loadout.Back },
                Is.EqualTo(new[] { "corona", "gafas-sol", "pajarita", "bigote", "alas-dragon" }), "Los cinco, publicados en red.");
            Assert.That(Worn(avatar, CosmeticSlot.Head, "Crown") && Worn(avatar, CosmeticSlot.Face, "SunGlasses") &&
                        Worn(avatar, CosmeticSlot.Neck, "BowTie") && Worn(avatar, CosmeticSlot.Mouth, "Mustache") &&
                        Worn(avatar, CosmeticSlot.Back, "DragonWings"), Is.True, "Cada uno en su sitio a la vez.");

            // Las alas, en la espalda (detrás del concursante); las gafas, delante de la cara; la corona, encima del pelo.
            var root = _player.transform;
            Assert.That(root.InverseTransformPoint(avatar.AnchorFor(CosmeticSlot.Back).position).z, Is.LessThan(0f), "Alas detrás.");
            Assert.That(root.InverseTransformPoint(avatar.AnchorFor(CosmeticSlot.Face).position).z, Is.GreaterThan(0.3f), "Gafas delante de la cara.");
            Assert.That(root.InverseTransformPoint(avatar.AnchorFor(CosmeticSlot.Head).position).y, Is.GreaterThan(1.8f), "Corona encima del pelo.");
        }

        private static bool Worn(IPlayerAvatar avatar, CosmeticSlot slot, string model) =>
            avatar.AnchorFor(slot).GetComponentsInChildren<Transform>().Any(t => t.name.StartsWith(model));

        [UnityTest]
        public IEnumerator BuyingACharacter_ChoosesIt_AndTheContestantWearsIt()
        {
            _profile.AwardMatch("regalo", 1200);
            _shop.Open();
            yield return null;
            _shop.Select("raincoat");
            Assert.That(_shop.ActionText, Is.EqualTo("Comprar y elegir"));
            _shop.PressAction();
            yield return null;
            yield return null;

            Assert.That(ProfileRules.Owns(_profile.Data, "raincoat"), Is.True);
            Assert.That(_profile.Tokens, Is.Zero);
            Assert.That(_profile.CharacterId, Is.EqualTo("raincoat"), "Al comprarlo se elige.");
            Assert.That(_shop.ActionText, Is.EqualTo("Elegido"));
            Assert.That(_shop.CanAct, Is.False, "El elegido no se vuelve a elegir.");

            var avatar = _player.GetComponent<IPlayerAvatar>();
            Assert.That(avatar.CharacterId, Is.EqualTo("raincoat"), "El concursante lo lleva puesto.");
            var view = _player.GetComponent<Trivias.Player.Components.PlayerCharacterView>();
            Assert.That(view.ShownId, Is.EqualTo("raincoat"));
            Assert.That(view.Animator.avatar.name, Does.StartWith("Raincoat"), "Con el modelo del personaje comprado.");
        }

        [UnityTest]
        public IEnumerator TheShop_ShowsEveryCharacter_FreeOnesAsFree_PaidOnesWithPriceAndLock()
        {
            _shop.Open();
            yield return null;
            var cards = (ShopCard[])typeof(ShopView).GetField("_characterCards", PRIVATE).GetValue(_shop);
            Assert.That(cards.Length, Is.EqualTo(10));
            Assert.That(cards.Count(c => c.BadgeText == "Gratis" || c.BadgeText == "Elegido"), Is.EqualTo(6), "Seis gratuitos.");
            Assert.That(cards.Count(c => c.BadgeText == "1.200 fichas" && c.IsLocked), Is.EqualTo(4), "Cuatro de pago, con candado.");
            Assert.That(_shop.SelectedId, Is.EqualTo(_profile.CharacterId.Length > 0 ? _profile.CharacterId : "tracksuit"),
                "Se abre con tu personaje seleccionado.");
        }

        [UnityTest]
        public IEnumerator WithoutEnoughTokens_ThePriceStays_AndTheButtonSaysHowManyAreMissing()
        {
            _profile.AwardMatch("poco", 900);
            _shop.Open();
            yield return null;
            _shop.Select("winter");
            Assert.That(_shop.ActionText, Is.EqualTo("Te faltan 300 fichas"));
            Assert.That(_shop.CanAct, Is.False);
        }

        [UnityTest]
        public IEnumerator TheShop_CoversTheLobby_AndClosingBringsItBack()
        {
            var menu = Object.FindAnyObjectByType<MenuView>();
            Assert.That(menu.IsLobbyOpen, Is.True);
            _shop.Open();
            yield return null;
            Assert.That(menu.IsLobbyOpen, Is.False, "Como en el boceto: la tienda ocupa la pantalla.");
            _shop.Close();
            yield return null;
            Assert.That(menu.IsLobbyOpen, Is.True);
        }

        [UnityTest]
        public IEnumerator WithoutTokens_ItemsCannotBeBought()
        {
            _shop.Open();
            yield return null;
            _shop.Select("corona");
            Assert.That(_shop.CanAct, Is.False, "Sin fichas el botón de comprar está desactivado.");
            _shop.PressAction();
            yield return null;
            Assert.That(ProfileRules.Owns(_profile.Data, "corona"), Is.False);
        }

        [UnityTest]
        public IEnumerator Shop_ClosesWhenTheMatchStarts()
        {
            _shop.Open();
            yield return null;
            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;
            Assert.That(_shop.IsOpen, Is.False);
        }

        [UnityTest]
        public IEnumerator FinishedMatch_AwardsTokensOnlyOnce()
        {
            // Para no jugar 3 preguntas: el primer tramo se da con 0 preguntas superadas.
            var match = Object.FindAnyObjectByType<MatchSessionNetwork>();
            _config = (MatchConfig)typeof(MatchSessionNetwork).GetField("_config", PRIVATE).GetValue(match);
            _originalTiers = _config.TokenTierMinQuestions;
            typeof(MatchConfig).GetField("_tokenTierMinQuestions", PRIVATE).SetValue(_config, new[] { 0, 6, 11, 15 });

            Assert.That(_connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;
            match.RequestVerify();
            yield return PlayModeHelpers.AfterRound();
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.Defeat));
            Assert.That(match.Result.TokensEach, Is.EqualTo(50));
            Assert.That(_profile.Tokens, Is.EqualTo(50), "Las fichas se guardan al ver los resultados.");

            // Redibujar los resultados (cualquier cambio de la partida) no vuelve a entregarlas.
            var runtime = (MatchRuntime)typeof(MatchSessionNetwork).GetField("_runtime", PRIVATE).GetValue(match);
            runtime.NotifyChanged();
            runtime.NotifyChanged();
            yield return null;
            Assert.That(_profile.Tokens, Is.EqualTo(50), "Una sola entrega por partida.");
        }

    }
}
