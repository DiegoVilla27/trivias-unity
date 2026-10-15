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
    /// Interfaz durante las preguntas: pregunta y opciones arriba, una línea de estado corta (reloj, cierre, resultado)
    /// y, a la derecha, el dinero del equipo y los comodines. En la sala y en los resultados no se muestra (los cubren
    /// el menú y la pantalla final). Se redibuja por eventos; el reloj solo reescribe el texto cuando cambia el segundo.
    /// </summary>
    public sealed class MatchHudView : MonoBehaviour
    {
        private const string GOLD = "#FFD15C";
        private const string GREEN = "#5CFF7A";
        private const string RED = "#FF6B6B";
        private const string DIMMED = "#FFFFFF55";
        private const int URGENT_SECONDS = 10;

        [SerializeField] private MatchRuntime _runtime;
        [SerializeField] private GameObject _questionPanel;
        [SerializeField] private TextMeshProUGUI _header;
        [SerializeField] private TextMeshProUGUI _question;
        [SerializeField] private TextMeshProUGUI[] _options = new TextMeshProUGUI[4];
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
            _status.text = session.Phase switch
            {
                MatchPhase.Betting => BettingStatus(seconds),
                MatchPhase.Locked => $"<size=140%><b>CERRADO · {seconds}</b></size>",
                _ => _status.text,
            };
        }

        private void Refresh()
        {
            var session = _runtime.Session;
            var playing = session != null && (session.Phase == MatchPhase.Betting || session.Phase == MatchPhase.Locked ||
                                              session.Phase == MatchPhase.Reveal) && session.CurrentQuestion != null;
            _questionPanel.SetActive(playing);
            _teamMoney.transform.parent.gameObject.SetActive(playing);
            _selfBetInfo.transform.parent.gameObject.SetActive(playing);
            if (!playing)
                return;

            _teamMoney.text = $"<size=60%>DINERO</size>\n<b>{MoneyFormat.Amount(session.TeamMoney)}</b>";
            RefreshSelfBetInfo();
            ShowQuestion(session);
        }

        private static bool HasClock(IMatchSession session) =>
            session.Phase == MatchPhase.Locked || (session.Phase == MatchPhase.Betting && session.IsBettingTimed);

        private void ShowQuestion(IMatchSession session)
        {
            var question = session.CurrentQuestion;
            _header.text = $"PREGUNTA {session.QuestionNumber}/{session.TotalQuestions}  ·  {question.Category.ToUpperInvariant()}";
            _question.text = question.Text;

            var correct = session.CorrectBooth;
            for (var booth = 0; booth < _options.Length; booth++)
            {
                var option = $"<color={GOLD}><b>{(char)('A' + booth)}</b></color>  {question.Option(session.Order.OriginalAt(booth))}";
                _options[booth].text = correct < 0
                    ? option
                    : booth == correct ? $"<color={GREEN}><b>{(char)('A' + booth)}  {question.Option(session.Order.OriginalAt(booth))}</b></color>"
                    : $"<color={DIMMED}>{(char)('A' + booth)}  {question.Option(session.Order.OriginalAt(booth))}</color>";
            }

            _shownSeconds = -1;
            var risk = session.MoneyAtRisk;
            _atRisk = risk > 0 ? $"<color={RED}>Fuera de bandejas: {MoneyFormat.Amount(risk)}</color>" : string.Empty;

            switch (session.Phase)
            {
                case MatchPhase.Betting:
                    _status.text = session.IsBettingTimed
                        ? BettingStatus(Mathf.CeilToInt(session.PhaseSecondsLeft))
                        : JoinLines("Pulsad VERIFICAR cuando estéis listos", _atRisk);
                    break;
                case MatchPhase.Locked:
                    _status.text = $"<size=140%><b>CERRADO · {Mathf.CeilToInt(session.PhaseSecondsLeft)}</b></size>";
                    break;
                case MatchPhase.Reveal:
                    _status.text = RevealStatus(session, correct);
                    break;
            }
        }

        private string BettingStatus(int seconds)
        {
            var color = seconds <= URGENT_SECONDS ? RED : "#FFFFFF";
            var clock = $"<size=150%><b><color={color}>{seconds / 60}:{seconds % 60:00}</color></b></size>";
            return JoinLines(clock, _atRisk);
        }

        private static string RevealStatus(IMatchSession session, int correct)
        {
            var round = session.LastRound;
            var result = $"<color={GREEN}><b>Correcta: {(char)('A' + correct)}</b></color>";
            if (round.Lost > 0)
                result += $"   <color={RED}>−{MoneyFormat.Amount(round.Lost)}</color>";
            if (round.Kept + round.Bonus > 0)
                result += $"   <color={GREEN}>+{MoneyFormat.Amount(round.Kept + round.Bonus)}</color>";
            if (session.LastMultiplier > 1)
                result += $"  <b>x{session.LastMultiplier}</b>";
            if (!string.IsNullOrEmpty(session.LastEliminated))
                result += $"\n<color={RED}>Expulsados: {session.LastEliminated}</color>";
            return $"{result}\n<size=80%><i>{session.CurrentQuestion.Explanation}</i></size>";
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
