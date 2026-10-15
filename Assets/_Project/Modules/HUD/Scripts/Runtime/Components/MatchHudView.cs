using TMPro;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Money.Logic;
using Trivias.SelfBet.Data;
using Trivias.Shared.Players;
using UnityEngine;

namespace Trivias.HUD.Components
{
    /// <summary>
    /// Interfaz durante las preguntas. La pregunta y las opciones NO van aquí: se leen en la pantalla grande del plató.
    /// Arriba, una línea de estado (reloj, cierre o resultado de la ronda); a la derecha, dinero del equipo y comodines.
    /// En la sala y en los resultados no se muestra. Se redibuja por eventos; el reloj solo cuando cambia el segundo.
    /// </summary>
    public sealed class MatchHudView : MonoBehaviour
    {
        private const string GREEN = "#5CFF7A";
        private const string RED = "#FF6B6B";
        private const int URGENT_SECONDS = 10;

        [SerializeField] private MatchRuntime _runtime;
        [SerializeField] private GameObject _statusPanel;
        [SerializeField] private TextMeshProUGUI _status;
        [SerializeField] private TextMeshProUGUI _teamMoney;
        [SerializeField] private SelfBetRuntime _selfBet;
        [SerializeField] private PlayerAvatarRegistry _avatars;
        [SerializeField] private TextMeshProUGUI _selfBetInfo;

        private int _shownSeconds = -1;
        private string _atRisk = string.Empty;

        private void OnEnable()
        {
            _runtime.Changed += Refresh;
            _selfBet.Changed += Refresh;
            _avatars.Changed += OnAvatarChanged;
            Refresh();
        }

        private void OnDisable()
        {
            _runtime.Changed -= Refresh;
            _selfBet.Changed -= Refresh;
            _avatars.Changed -= OnAvatarChanged;
        }

        private void OnAvatarChanged(ulong _) => Refresh();

        private void Update()
        {
            var session = _runtime.Session;
            if (session == null || !HasClock(session))
                return;

            var seconds = Mathf.CeilToInt(session.PhaseSecondsLeft);
            if (seconds == _shownSeconds)
                return;

            _shownSeconds = seconds;
            _status.text = session.Phase == MatchPhase.Locked ? LockedStatus(seconds) : BettingStatus(seconds);
        }

        private void Refresh()
        {
            var session = _runtime.Session;
            var playing = session != null && session.CurrentQuestion != null &&
                          (session.Phase == MatchPhase.Betting || session.Phase == MatchPhase.Locked ||
                           session.Phase == MatchPhase.Reveal);
            _statusPanel.SetActive(playing);
            _teamMoney.transform.parent.gameObject.SetActive(playing);
            _selfBetInfo.transform.parent.gameObject.SetActive(playing);
            if (!playing)
                return;

            _teamMoney.text = $"<size=60%>DINERO</size>\n<b>{MoneyFormat.Amount(session.TeamMoney)}</b>";
            RefreshSelfBetInfo();

            _shownSeconds = -1;
            var risk = session.MoneyAtRisk;
            _atRisk = risk > 0 ? $"<color={RED}>Fuera de bandejas: {MoneyFormat.Amount(risk)}</color>" : string.Empty;

            var seconds = Mathf.CeilToInt(session.PhaseSecondsLeft);
            _status.text = session.Phase switch
            {
                MatchPhase.Betting => session.IsBettingTimed ? BettingStatus(seconds) : JoinLines("¡A apostar!", _atRisk),
                MatchPhase.Locked => LockedStatus(seconds),
                _ => RevealStatus(session),
            };
        }

        private static bool HasClock(IMatchSession session) =>
            session.Phase == MatchPhase.Locked || (session.Phase == MatchPhase.Betting && session.IsBettingTimed);

        private string BettingStatus(int seconds)
        {
            var color = seconds <= URGENT_SECONDS ? RED : "#FFFFFF";
            return JoinLines($"<size=150%><b><color={color}>{seconds / 60}:{seconds % 60:00}</color></b></size>", _atRisk);
        }

        private static string LockedStatus(int seconds) => $"<size=140%><b>CERRADO · {seconds}</b></size>";

        /// <summary>Resultado económico de la ronda (la respuesta correcta y su explicación están en la pantalla).</summary>
        private static string RevealStatus(IMatchSession session)
        {
            var round = session.LastRound;
            var result = round.Lost > 0 ? $"<color={RED}>−{MoneyFormat.Amount(round.Lost)}</color>" : string.Empty;
            if (round.Kept + round.Bonus > 0)
                result += $"{(result.Length > 0 ? "   " : string.Empty)}<color={GREEN}>+{MoneyFormat.Amount(round.Kept + round.Bonus)}</color>";
            if (session.LastMultiplier > 1)
                result += $"  <b>x{session.LastMultiplier}</b>";
            if (result.Length == 0)
                result = "Sin cambios";
            result = $"<size=130%><b>{result}</b></size>";
            if (!string.IsNullOrEmpty(session.LastEliminated))
                result += $"\n<color={RED}>Expulsados: {session.LastEliminated}</color>";
            return result;
        }

        /// <summary>Comodines del jugador local y de la reserva, o el aviso de expulsado.</summary>
        private void RefreshSelfBetInfo()
        {
            var bets = _selfBet.Session;
            if (bets == null)
            {
                _selfBetInfo.text = string.Empty;
                return;
            }

            var me = bets.LocalClientId;
            if (_avatars.IsEliminated(me))
            {
                _selfBetInfo.text = $"<color={RED}><b>EXPULSADO</b></color>\n<size=70%>espectador</size>";
                return;
            }

            var book = bets.Book;
            var index = book.IndexOf(me);
            var personal = index >= 0 ? book.Bettors[index].PersonalUses : 0;
            var bet = index >= 0 && book.Bettors[index].HasBet
                ? $"\n<color={GREEN}>Apostado en {(char)('A' + book.Bettors[index].Booth)}</color>"
                : string.Empty;
            _selfBetInfo.text = $"<size=60%>COMODINES</size>\n<b>{personal}</b>  <size=70%>reserva {book.Reserve}</size>{bet}";
        }

        private static string JoinLines(string first, string second) =>
            string.IsNullOrEmpty(second) ? first : $"{first}\n<size=85%>{second}</size>";
    }
}
