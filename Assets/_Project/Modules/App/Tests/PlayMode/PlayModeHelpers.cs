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
