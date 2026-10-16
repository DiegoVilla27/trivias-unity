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
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

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
        public IEnumerator BuyingAHat_EquipsItOnThePlayersHead()
        {
            _profile.AwardMatch("regalo", 1000);
            _shop.Open();
            yield return null;

            Row("gorra").Button.onClick.Invoke();
            yield return null;
            yield return null;

            Assert.That(_profile.Tokens, Is.EqualTo(700), "La gorra cuesta 300.");
            Assert.That(_profile.Loadout.Head, Is.EqualTo("gorra"), "Al comprar se equipa.");
            var avatar = _player.GetComponent<IPlayerAvatar>();
            Assert.That(avatar.Loadout.Head, Is.EqualTo("gorra"), "Lo que lleva se publica en red.");
            Assert.That(avatar.HeadAnchor.GetComponentsInChildren<Transform>().Any(t => t.name.StartsWith("Gorra")), Is.True,
                "La gorra debe verse en la cabeza.");

            Row("gorra").Button.onClick.Invoke(); // Ya comprada: ahora quita.
            yield return null;
            yield return null;
            Assert.That(avatar.Loadout.Head, Is.Empty);
            Assert.That(avatar.HeadAnchor.GetComponentsInChildren<Transform>().Any(t => t.name.StartsWith("Gorra")), Is.False);
            Assert.That(_profile.Tokens, Is.EqualTo(700), "Quitar no devuelve ni cobra fichas.");
        }

        [UnityTest]
        public IEnumerator BuyingAColor_TintsTheSuit()
        {
            _profile.AwardMatch("regalo", 1000);
            _shop.Open();
            yield return null;
            Row("color-rojo").Button.onClick.Invoke();
            yield return null;
            yield return null;

            var avatar = _player.GetComponent<IPlayerAvatar>();
            var expected = Catalog().Find("color-rojo").Tint;
            var block = new MaterialPropertyBlock();
            avatar.BodyRenderers[0].GetPropertyBlock(block);
            var actual = block.GetColor(BaseColorId);
            Assert.That(Vector4.Distance(actual, expected), Is.LessThan(0.01f),
                $"El traje debe teñirse con el color comprado (esperado {expected}, aplicado {actual}).");
        }

        [UnityTest]
        public IEnumerator WithoutTokens_ItemsCannotBeBought()
        {
            _shop.Open();
            yield return null;
            var row = Row("corona");
            Assert.That(row.Button.interactable, Is.False, "Sin fichas el botón de comprar está desactivado.");
            row.Button.onClick.Invoke();
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

        private ShopItemRow Row(string itemId)
        {
            var catalog = Catalog();
            var rows = (ShopItemRow[])typeof(ShopView).GetField("_rows", PRIVATE).GetValue(_shop);
            return rows.First(r => catalog.Items[r.ItemIndex].Id == itemId);
        }

        private CosmeticCatalog Catalog() => (CosmeticCatalog)typeof(ShopView).GetField("_catalog", PRIVATE).GetValue(_shop);
    }
}
