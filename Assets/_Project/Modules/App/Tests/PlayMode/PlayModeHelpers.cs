using Trivias.Exchange.Components;
using Trivias.Money.Components;
using Trivias.Player.Components;
using Trivias.SelfBet.Components;
using Unity.Netcode;
using UnityEngine;

namespace Trivias.App.Tests
{
    /// <summary>Utilidades compartidas por las pruebas en juego (solo tests: pueden buscar en la escena).</summary>
    internal static class PlayModeHelpers
    {
        /// <summary>Ritmo de la partida en las pruebas (<see cref="FastMatchTimings"/>): cuenta atrás y revelación cortas.</summary>
        public const float LOCK_SECONDS = 0.5f;
        public const float REVEAL_SECONDS = 1f;
        private const float MARGIN = 0.3f;

        /// <summary>
        /// Espera a que la sala recién abierta esté lista (jugador local y sesión de partida en red), en lugar de medio
        /// segundo fijo: suele bastar con unos fotogramas. Máximo 3 s; si no llega, la prueba falla en su comprobación.
        /// </summary>
        public static System.Collections.IEnumerator UntilHostReady()
        {
            var deadline = Time.realtimeSinceStartup + 3f;
            while (Time.realtimeSinceStartup < deadline && !IsHostReady())
                yield return null;
            yield return null;
            yield return null;
        }

        private static bool IsHostReady()
        {
            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening || manager.LocalClient?.PlayerObject == null)
                return false;
            var match = Object.FindAnyObjectByType<Trivias.Match.Network.MatchSessionNetwork>();
            return match != null && match.IsSpawned;
        }

        /// <summary>Cambia el ritmo de la partida para una prueba que necesita otro (y se restaura al acabarla).</summary>
        public static void SetPace(float lockSeconds, float revealSeconds)
        {
            const System.Reflection.BindingFlags privateField = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            foreach (var config in Resources.FindObjectsOfTypeAll<Trivias.Match.Data.MatchConfig>())
            {
                typeof(Trivias.Match.Data.MatchConfig).GetField("_lockCountdownSeconds", privateField).SetValue(config, lockSeconds);
                typeof(Trivias.Match.Data.MatchConfig).GetField("_revealSeconds", privateField).SetValue(config, revealSeconds);
            }
        }

        /// <summary>Tras la cuenta atrás de verificar: ya se ve la respuesta.</summary>
        public static WaitForSeconds AfterLock() => new(LOCK_SECONDS + MARGIN);

        /// <summary>Tras la revelación: siguiente pregunta o resultados.</summary>
        public static WaitForSeconds AfterReveal() => new(REVEAL_SECONDS + MARGIN);

        /// <summary>Una ronda entera desde que se pulsa verificar.</summary>
        public static WaitForSeconds AfterRound() => new(LOCK_SECONDS + REVEAL_SECONDS + MARGIN);

        public static void MoveNextTo(NetworkObject player, Vector3 target, float distance = 1.0f)
        {
            var toCenter = -target;
            toCenter.y = 0f;
            var position = target + (toCenter.sqrMagnitude > 0.01f ? toCenter.normalized : Vector3.back) * distance;
            position.y = 0.05f;
            player.GetComponent<PlayerMotor>().Teleport(position, Quaternion.identity);
        }

        public static void MoveTo(NetworkObject player, Vector3 position)
        {
            position.y = 0.05f;
            player.GetComponent<PlayerMotor>().Teleport(position, Quaternion.identity);
        }

        public static MoneyTableTarget FindTable() => Object.FindAnyObjectByType<MoneyTableTarget>();

        public static ExchangeStationTarget FindExchangeStation() => Object.FindAnyObjectByType<ExchangeStationTarget>();

        public static MoneyTrayTarget FindTray(int booth) => FindInBooth<MoneyTrayTarget>(booth);

        public static BetZoneTarget FindBetZone(int booth) => FindInBooth<BetZoneTarget>(booth);

        /// <summary>Busca un componente que cuelga de la cabina A/B/C/D (Booth_A...).</summary>
        private static T FindInBooth<T>(int booth) where T : Component
        {
            var letter = ((char)('A' + booth)).ToString();
            foreach (var candidate in Object.FindObjectsByType<T>(FindObjectsInactive.Exclude))
            {
                if (candidate.transform.parent != null && candidate.transform.parent.name.EndsWith(letter))
                    return candidate;
            }
            return null;
        }
    }
}
