using System.Collections;
using System.Linq;
using NUnit.Framework;
using Trivias.Menu.Components;
using Trivias.Networking.Components;
using Trivias.Player.Components;
using Trivias.Shared.Characters;
using Trivias.Shared.Players;
using Trivias.Shared.Profile;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Trivias.App.Tests
{
    /// <summary>
    /// Elegir personaje en el menú principal (solo los que tiene) y llevarlo en la sala, sin que dos jugadores lleven
    /// el mismo.
    /// </summary>
    public sealed class CharacterSelectionTests
    {
        private PlayerProfile _profile;
        private CharacterCatalog _catalog;
        private PlayerAvatarRegistry _avatars;
        private OtherPlayer _other;

        [UnitySetUp]
        public IEnumerator LoadStage()
        {
            _profile = TemporaryProfileSetup.FreshProfile();
            yield return SceneManager.LoadSceneAsync("Stage", LoadSceneMode.Single);
            var picker = Object.FindAnyObjectByType<CharacterPickerView>();
            _catalog = (CharacterCatalog)typeof(CharacterPickerView)
                .GetField("_catalog", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .GetValue(picker);
            _avatars = Resources.FindObjectsOfTypeAll<PlayerAvatarRegistry>().First();
        }

        [UnityTearDown]
        public IEnumerator Shutdown()
        {
            if (_other != null)
                _avatars.Unregister(_other.ClientId);
            _other = null;
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
                Object.Destroy(NetworkManager.Singleton.gameObject);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator MainMenu_StartsOnTheDefaultCharacter_ShownIn3D()
        {
            yield return null;
            var picker = Object.FindAnyObjectByType<CharacterPickerView>();
            Assert.That(picker.ShownId, Is.EqualTo(_catalog.DefaultId), "Quien no elige lleva el de por defecto.");
            Assert.That(_catalog.DefaultId, Is.EqualTo(_catalog.FreeIds[0]));

            var showcase = Object.FindAnyObjectByType<CharacterShowcase>();
            Assert.That(showcase.IsShowing, Is.True, "El personaje se ve en 3D en el menú.");
            Assert.That(showcase.ShownId, Is.EqualTo(picker.ShownId));
            var camera = showcase.GetComponentInChildren<Camera>();
            var preview = picker.GetComponentInChildren<UnityEngine.UI.RawImage>();
            Assert.That(camera.targetTexture, Is.Not.Null.And.SameAs(preview.texture), "Lo que filma el expositor es lo que muestra el menú.");
            Assert.That(showcase.transform.position.y, Is.LessThan(-100f), "Lejos del plató: no se ve desde el juego.");
        }

        [UnityTest]
        public IEnumerator Showcase_FollowsTheArrows_AndTurnsOffWhenTheMenuCloses()
        {
            yield return null;
            var picker = Object.FindAnyObjectByType<CharacterPickerView>();
            var showcase = Object.FindAnyObjectByType<CharacterShowcase>();
            var turntable = showcase.GetComponentInChildren<Animator>().transform.parent;
            var angle = turntable.eulerAngles.y;
            picker.Next();
            yield return new WaitForSeconds(0.2f);
            Assert.That(showcase.ShownId, Is.EqualTo(picker.ShownId), "Cambia con las flechas.");
            Assert.That(turntable.eulerAngles.y, Is.Not.EqualTo(angle), "Gira despacio.");

            var connection = Object.FindAnyObjectByType<ConnectionService>();
            Assert.That(connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            Assert.That(showcase.IsShowing, Is.True, "En la sala de espera se ve tu personaje.");
            Assert.That(showcase.ShownId, Is.EqualTo(picker.ShownId));

            Assert.That(connection.TryStartMatch(), Is.True);
            yield return null;
            yield return null;
            Assert.That(showcase.IsShowing, Is.False, "En la partida ya no hace falta: se apaga.");
        }

        [UnityTest]
        public IEnumerator MainMenu_ArrowsGoThroughTheFreeOnes_AndRememberTheChoice()
        {
            var picker = Object.FindAnyObjectByType<CharacterPickerView>();
            var seen = new System.Collections.Generic.List<string> { picker.ShownId };
            for (var i = 0; i < 5; i++)
            {
                picker.Next();
                yield return null;
                seen.Add(picker.ShownId);
            }
            Assert.That(seen, Is.EquivalentTo(_catalog.FreeIds), "Pasa por los seis gratuitos y ninguno de pago.");
            Assert.That(_profile.CharacterId, Is.EqualTo(seen[5]), "Se guarda al elegirlo.");

            picker.Next();
            yield return null;
            Assert.That(picker.ShownId, Is.EqualTo(_catalog.FreeIds[0]), "Da la vuelta.");
            picker.Previous();
            yield return null;
            Assert.That(picker.ShownId, Is.EqualTo(seen[5]));
        }

        [UnityTest]
        public IEnumerator MainMenu_ShowsABoughtCharacterToo()
        {
            var paid = _catalog.Ids.First(id => !_catalog.IsFree(id));
            _profile.AwardMatch("regalo", _catalog.Price);
            Assert.That(_profile.TryBuy(paid, _catalog.Price), Is.True);
            var picker = Object.FindAnyObjectByType<CharacterPickerView>();
            for (var i = 0; i < _catalog.Ids.Count && picker.ShownId != paid; i++)
            {
                picker.Next();
                yield return null;
            }
            Assert.That(picker.ShownId, Is.EqualTo(paid), "Lo comprado también se puede elegir.");
        }

        [UnityTest]
        public IEnumerator InTheRoom_TheContestantWearsTheChosenCharacter()
        {
            Assert.That(_profile.TryChooseCharacter(_catalog, "gardener"), Is.True);
            Assert.That(Object.FindAnyObjectByType<ConnectionService>().StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();

            var local = NetworkManager.Singleton.LocalClient.PlayerObject;
            Assert.That(local.GetComponent<IPlayerAvatar>().CharacterId, Is.EqualTo("gardener"));
            Assert.That(local.GetComponent<PlayerCharacterView>().ShownId, Is.EqualTo("gardener"), "Con su modelo.");

            // Cambiarlo en la sala (p. ej. desde la tienda) se ve al momento.
            Assert.That(_profile.TryChooseCharacter(_catalog, "elegant"), Is.True);
            yield return null;
            yield return null;
            Assert.That(local.GetComponent<IPlayerAvatar>().CharacterId, Is.EqualTo("elegant"));

            // Al cambiar, el concursante lleva el modelo nuevo: un solo esqueleto, el avatar suyo y la cabeza nueva.
            var view = local.GetComponent<PlayerCharacterView>();
            var animator = view.Animator;
            Assert.That(animator.avatar.name, Does.StartWith("Elegant"));
            Assert.That(animator.GetComponentsInChildren<Transform>(true).Count(t => t.name == "Hips"), Is.EqualTo(1),
                "El esqueleto anterior ya no cuelga del Animator.");
            Assert.That(animator.GetBoneTransform(HumanBodyBones.Head), Is.SameAs(view.Head), "El avatar mueve la cabeza nueva.");
            Assert.That(view.HeadAnchor.parent, Is.SameAs(view.Head), "Los complementos van en la cabeza nueva.");
            Assert.That(local.GetComponent<IPlayerAvatar>().HeadAnchor, Is.SameAs(view.HeadAnchor));
        }

        [UnityTest]
        public IEnumerator InTheRoom_TwoPlayersNeverWearTheSameCharacter()
        {
            // Otro jugador ya lleva el rojo y este también lo quiere.
            _other = new OtherPlayer(77, "tracksuit");
            _avatars.Register(_other);
            Assert.That(_profile.TryChooseCharacter(_catalog, "tracksuit"), Is.True);

            Assert.That(Object.FindAnyObjectByType<ConnectionService>().StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();

            var local = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<IPlayerAvatar>();
            Assert.That(local.CharacterId, Is.Not.EqualTo("tracksuit"), "Tracksuit ya estaba cogido.");
            Assert.That(local.CharacterId, Is.EqualTo("casual"), "Le toca el primer gratuito libre.");
            Assert.That(_profile.CharacterId, Is.EqualTo("tracksuit"), "Su preferencia no se pierde para otra sala.");
        }

        /// <summary>Otro concursante de la sala (solo lo que importa aquí: qué personaje lleva).</summary>
        private sealed class OtherPlayer : IPlayerAvatar
        {
            public OtherPlayer(ulong clientId, string characterId)
            {
                ClientId = clientId;
                CharacterId = characterId;
            }

            public ulong ClientId { get; }
            public string CharacterId { get; }
            public string DisplayName => "Otro";
            public Vector3 Position => Vector3.zero;
            public Transform CarryPoint => null;
            public Transform HeadAnchor => null;
            public CosmeticLoadout Loadout => default;
            public bool IsLocal => false;
            public Transform ViewTarget => null;
            public bool IsEliminated => false;

            public void Eliminate()
            {
            }

            public void Respawn()
            {
            }
        }
    }
}
