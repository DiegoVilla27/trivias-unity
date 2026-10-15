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
    /// Interfaz de la partida: pregunta, opciones A-D, estado de la ronda y dinero del equipo.
    /// Se redibuja por eventos; solo durante la cuenta atrás y la revelación revisa el reloj, y solo
    /// reescribe el texto cuando cambia el segundo mostrado.
    /// </summary>
    public sealed class MatchHudView : MonoBehaviour
    {
        private const string CORRECT_COLOR = "#5CFF7A";
        private const string DIMMED_COLOR = "#FFFFFF66";
        private const string WARNING_COLOR = "#FFB84D";

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
        private string _statusPrefix = string.Empty;

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
            if (session == null || (session.Phase != MatchPhase.Locked && session.Phase != MatchPhase.Reveal))
                return;

            var seconds = Mathf.CeilToInt(session.PhaseSecondsLeft);
            if (seconds == _shownSeconds)
                return;

            _shownSeconds = seconds;
            _status.text = session.Phase == MatchPhase.Locked
                ? $"<size=150%><b>APUESTAS CERRADAS · {seconds}</b></size>"
                : $"{_statusPrefix}\n<size=80%>Siguiente pregunta en {seconds}…</size>";
        }

        private void Refresh()
        {
            var session = _runtime.Session;
            _questionPanel.SetActive(session != null);
            _teamMoney.gameObject.SetActive(session != null);
            _selfBetInfo.transform.parent.gameObject.SetActive(session != null);
            if (session == null)
                return;

            RefreshSelfBetInfo(session.Phase);

            _teamMoney.text = $"<size=70%>DINERO DEL EQUIPO</size>\n<b>{MoneyFormat.Amount(session.TeamMoney)}</b>";
            _shownSeconds = -1;

            switch (session.Phase)
            {
                case MatchPhase.Lobby:
                    ShowMessage("SALA ABIERTA", "Esperando a que el host empiece la partida.",
                        "Cuando estéis todos, el host pulsa «Empezar partida».");
                    break;
                case MatchPhase.Betting:
                case MatchPhase.Locked:
                case MatchPhase.Reveal:
                    if (session.CurrentQuestion == null)
                        ShowMessage("PREPARADOS", "Cargando la pregunta…", string.Empty);
                    else
                        ShowQuestion(session);
                    break;
                case MatchPhase.Victory:
                    ShowMessage("¡VICTORIA!",
                        $"Habéis superado las {session.TotalQuestions} preguntas con {MoneyFormat.Amount(session.TeamMoney)}.",
                        string.Empty);
                    break;
                case MatchPhase.Defeat:
                    ShowMessage("DERROTA",
                        $"Os habéis quedado sin dinero en la pregunta {session.QuestionNumber}.",
                        $"Preguntas superadas: {MatchRules.QuestionsPassed(session.Phase, session.QuestionNumber)}");
                    break;
            }
        }

        private void ShowQuestion(IMatchSession session)
        {
            var question = session.CurrentQuestion;
            _header.text = $"PREGUNTA {session.QuestionNumber} / {session.TotalQuestions}  ·  {question.Category.ToUpperInvariant()}";
            _question.text = question.Text;

            var correct = session.CorrectBooth;
            for (var booth = 0; booth < _options.Length; booth++)
            {
                var option = $"<b>{(char)('A' + booth)}</b>  {question.Option(session.Order.OriginalAt(booth))}";
                _options[booth].gameObject.SetActive(true);
                _options[booth].text = correct < 0
                    ? option
                    : $"<color={(booth == correct ? CORRECT_COLOR : DIMMED_COLOR)}>{option}</color>";
            }

            switch (session.Phase)
            {
                case MatchPhase.Betting:
                    var atRisk = session.MoneyAtRisk;
                    _status.text = atRisk > 0
                        ? $"Repartid el dinero en las bandejas y pulsad VERIFICAR cuando estéis listos.\n" +
                          $"<color={WARNING_COLOR}>Fuera de las bandejas: {MoneyFormat.Amount(atRisk)} (se perderá al verificar)</color>"
                        : "Todo el dinero está apostado. Pulsad VERIFICAR cuando estéis listos.";
                    break;
                case MatchPhase.Locked:
                    _status.text = "<size=150%><b>APUESTAS CERRADAS</b></size>";
                    break;
                case MatchPhase.Reveal:
                    var round = session.LastRound;
                    var bonus = round.Bonus > 0
                        ? $" · <b>x{session.LastMultiplier}</b> bono {MoneyFormat.Amount(round.Bonus)}"
                        : string.Empty;
                    var expelled = string.IsNullOrEmpty(session.LastEliminated)
                        ? string.Empty
                        : $"\n<color={WARNING_COLOR}>Expulsados: {session.LastEliminated}</color>";
                    _statusPrefix =
                        $"<color={CORRECT_COLOR}><b>Correcta: {(char)('A' + correct)}</b></color>  ·  " +
                        $"Perdido {MoneyFormat.Amount(round.Lost)} · Conservado {MoneyFormat.Amount(round.Kept)}{bonus}" +
                        $"{expelled}\n<size=85%><i>{question.Explanation}</i></size>";
                    _status.text = _statusPrefix;
                    break;
            }
        }

        /// <summary>Comodines del jugador local y de la reserva, o el aviso de expulsado.</summary>
        private void RefreshSelfBetInfo(MatchPhase phase)
        {
            var bets = _selfBet.Session;
            if (bets == null || phase == MatchPhase.Lobby)
            {
                _selfBetInfo.text = "<size=70%>COMODINES</size>\nal empezar";
                return;
            }

            var me = bets.LocalClientId;
            if (_avatars.IsEliminated(me))
            {
                _selfBetInfo.text = $"<color={WARNING_COLOR}><b>EXPULSADO</b></color>\n<size=70%>espectador hasta la próxima partida</size>";
                return;
            }

            var book = bets.Book;
            var index = book.IndexOf(me);
            var personal = index >= 0 ? book.Bettors[index].PersonalUses : 0;
            var betText = index >= 0 && book.Bettors[index].HasBet
                ? $"\n<color={CORRECT_COLOR}>Apostado en {(char)('A' + book.Bettors[index].Booth)}</color>"
                : string.Empty;
            _selfBetInfo.text = $"<size=70%>COMODINES</size>\nTuyos: <b>{personal}</b> · Reserva: <b>{book.Reserve}</b>{betText}";
        }

        private void ShowMessage(string header, string message, string status)
        {
            _header.text = header;
            _question.text = message;
            foreach (var option in _options)
                option.gameObject.SetActive(false);
            _status.text = status;
        }
    }
}
