using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Trivias.Match.Logic;
using Trivias.Match.Network;
using Trivias.Money.Components;
using Trivias.Networking.Components;
using Trivias.Player.Components;
using Trivias.Shared.Interaction;
using Trivias.VerifyButton.Components;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static Trivias.App.Tests.PlayModeHelpers;

namespace Trivias.App.Tests
{
    /// <summary>
    /// El plató en juego: el dinero se apila en el mostrador de la cabina, el botón se hunde, el concursante salta y
    /// puede subir a una plataforma de apuesta andando.
    /// </summary>
    public sealed class StageModelsTests
    {
        private const BindingFlags PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic;

        private NetworkObject _player;
        private MatchSessionNetwork _match;

        [UnitySetUp]
        public IEnumerator StartMatch()
        {
            TemporaryProfileSetup.FreshProfile();
            yield return SceneManager.LoadSceneAsync("Stage", LoadSceneMode.Single);
            var connection = Object.FindAnyObjectByType<ConnectionService>();
            Assert.That(connection.StartHost(), Is.True);
            yield return PlayModeHelpers.UntilHostReady();
            Assert.That(connection.TryStartMatch(), Is.True);
            yield return null;
            _player = NetworkManager.Singleton.LocalClient.PlayerObject;
            _match = Object.FindAnyObjectByType<MatchSessionNetwork>();
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

        [UnityTest]
        public IEnumerator DepositedMoney_IsStackedOnTheBoothCounter()
        {
            var table = FindTable();
            MoveNextTo(_player, table.Position);
            yield return null;
            table.Interact(new InteractionContext(_player.OwnerClientId, InteractionAction.Secondary));
            yield return null;

            var tray = FindTray(1);
            MoveNextTo(_player, tray.Position);
            yield return null;
            tray.Interact(new InteractionContext(_player.OwnerClientId, InteractionAction.Secondary));
            yield return null;
            yield return null;

            var surface = tray.transform.Find("Surface").position;
            var onCounter = 0;
            foreach (var visual in Visuals())
            {
                if (!visual.activeInHierarchy)
                    continue;
                var position = visual.transform.position;
                var flat = new Vector2(position.x - surface.x, position.z - surface.z).magnitude;
                if (flat < 0.8f && position.y >= surface.y - 0.01f && position.y < surface.y + 0.5f)
                    onCounter++;
            }
            Assert.That(onCounter, Is.GreaterThan(0), "Los fajos depositados se ven sobre la bandeja del mostrador de la cabina B.");
        }

        [UnityTest]
        public IEnumerator VerifyButtonCap_SinksWhenBetsAreLocked()
        {
            var button = Object.FindAnyObjectByType<VerifyButtonTarget>();
            var cap = (Transform)typeof(VerifyButtonTarget).GetField("_cap", PRIVATE).GetValue(button);
            Assert.That(cap.name, Is.EqualTo("Cap"), "La tapa es la pieza roja del modelo.");
            var rest = cap.localPosition;

            _match.RequestVerify();
            yield return null;
            yield return null;
            Assert.That(_match.Phase, Is.EqualTo(MatchPhase.Locked));
            Assert.That(cap.localPosition.y, Is.LessThan(rest.y - 0.05f), "Pulsado: la tapa se hunde.");
        }

        [UnityTest]
        public IEnumerator Contestant_Jumps_AndLandsAgain()
        {
            yield return new WaitForSeconds(0.3f); // Que se asiente en el suelo.
            var motor = _player.GetComponent<PlayerMotor>();
            var start = _player.transform.position.y;
            Assert.That(motor.TryJump(), Is.True, "Con la partida empezada y en el suelo, salta.");
            Assert.That(motor.TryJump(), Is.False, "No hay doble salto.");

            var peak = start;
            for (var t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                peak = Mathf.Max(peak, _player.transform.position.y);
                yield return null;
            }
            Assert.That(peak - start, Is.GreaterThan(0.5f), "Sube.");
            yield return new WaitForSeconds(0.8f);
            Assert.That(_player.transform.position.y, Is.EqualTo(start).Within(0.1f), "Y vuelve al suelo.");
        }

        [UnityTest]
        public IEnumerator Contestant_WalksUpOntoTheBetPlatform()
        {
            var zone = FindBetZone(2);
            var outside = zone.transform.position + zone.transform.forward * 2.4f;
            MoveTo(_player, new Vector3(outside.x, 0.05f, outside.z));
            yield return new WaitForSeconds(0.3f);

            // Anda hacia el centro de la plataforma con el CharacterController (como al pulsar las teclas), sin
            // teletransportes. El motor se apaga durante el paseo para que su inercia no desvíe al concursante.
            var controller = _player.GetComponent<CharacterController>();
            var motor = _player.GetComponent<PlayerMotor>();
            motor.enabled = false;
            var target = zone.transform.position;
            for (var t = 0f; t < 2f; t += Time.deltaTime)
            {
                var flat = target - _player.transform.position;
                flat.y = 0f;
                if (flat.magnitude < 0.2f)
                    break;
                controller.Move(flat.normalized * (3f * Time.deltaTime) + Vector3.down * (2f * Time.deltaTime));
                yield return null;
            }
            motor.enabled = true;
            var reached = target - _player.transform.position;
            reached.y = 0f;
            Assert.That(reached.magnitude, Is.LessThan(0.5f), "Llega al centro de la plataforma (se sube sin saltar).");
            Assert.That(_player.transform.position.y, Is.GreaterThan(0.15f), "Está encima, no dentro.");
        }

        [UnityTest]
        public IEnumerator Contestant_CannotCrossTheRailing_FromTheSide()
        {
            var zone = FindBetZone(2);
            var outside = zone.transform.position + zone.transform.right * 2.4f;
            MoveTo(_player, new Vector3(outside.x, 0.05f, outside.z));
            yield return new WaitForSeconds(0.3f);

            var controller = _player.GetComponent<CharacterController>();
            var motor = _player.GetComponent<PlayerMotor>();
            motor.enabled = false;
            var target = zone.transform.position;
            for (var t = 0f; t < 2f; t += Time.deltaTime)
            {
                var flat = target - _player.transform.position;
                flat.y = 0f;
                controller.Move(flat.normalized * (3f * Time.deltaTime) + Vector3.down * (2f * Time.deltaTime));
                yield return null;
            }
            motor.enabled = true;
            var left = target - _player.transform.position;
            left.y = 0f;
            Assert.That(left.magnitude, Is.GreaterThan(1f), "La barandilla lo para: solo se entra por delante.");
        }

        [UnityTest]
        public IEnumerator Contestant_RunsWithShift()
        {
            // Las pruebas corren sin foco de ventana: que la entrada simulada llegue igual.
            var settings = InputSystem.settings;
            var background = settings.backgroundBehavior;
            var editorBehavior = settings.editorInputBehaviorInPlayMode;
            settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            var keyboard = InputSystem.AddDevice<Keyboard>();
            try
            {
                var walked = 0f;
                var ran = 0f;
                yield return Walk(keyboard, new KeyboardState(Key.W), d => walked = d);
                yield return Walk(keyboard, new KeyboardState(Key.W, Key.LeftShift), d => ran = d);
                Assert.That(walked, Is.GreaterThan(0.8f), "Con W anda.");
                Assert.That(ran, Is.GreaterThan(walked * 1.35f), "Con Shift corre bastante más.");
            }
            finally
            {
                InputSystem.RemoveDevice(keyboard);
                settings.backgroundBehavior = background;
                settings.editorInputBehaviorInPlayMode = editorBehavior;
            }
        }

        /// <summary>Pulsa las teclas 0,6 s desde el mismo punto y mide lo recorrido.</summary>
        private IEnumerator Walk(Keyboard keyboard, KeyboardState keys, System.Action<float> distance)
        {
            MoveTo(_player, new Vector3(0f, 0f, -4.5f));
            yield return new WaitForSeconds(0.3f);
            var start = _player.transform.position;
            InputSystem.QueueStateEvent(keyboard, keys);
            yield return new WaitForSeconds(0.6f);
            var moved = _player.transform.position - start;
            moved.y = 0f;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return new WaitForSeconds(0.3f);
            distance(moved.magnitude);
        }

        private static IEnumerable<GameObject> Visuals()
        {
            var presenter = Object.FindAnyObjectByType<MoneyVisualPresenter>();
            return (List<GameObject>)typeof(MoneyVisualPresenter).GetField("_visuals", PRIVATE).GetValue(presenter);
        }
    }
}
