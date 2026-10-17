using TMPro;
using UnityEngine.UI;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Money.Logic;
using UnityEngine;

namespace Trivias.Match.Components
{
    /// <summary>
    /// Pantalla grande del plató, con el estilo de los menús: cabecera, pregunta, las 4 opciones (píldoras del color de
    /// su cabina) y estado (reloj, cierre o «CORRECTA: B» con la explicación). Al revelar solo queda encendida la correcta. Fuera de las preguntas
    /// muestra un mensaje (¿PREPARADOS?, DERROTA). Al ganar muestra el cheque gigante con los supervivientes y el premio.
    /// Se redibuja por eventos; el reloj solo cada segundo.
    /// </summary>
    public sealed class QuestionScreenView : MonoBehaviour
    {
        private const string RED = "#FF8A8A";
        private const int URGENT_SECONDS = 10;
        private static readonly Color DimmedText = new(1f, 1f, 1f, 0.4f);

        [SerializeField] private MatchRuntime _runtime;
        [SerializeField] private GameObject _content;
        [SerializeField] private TMP_Text _message;
        [SerializeField] private TMP_Text _header;
        [SerializeField] private TMP_Text _question;
        [SerializeField] private TMP_Text[] _optionTexts = new TMP_Text[4];
        [SerializeField] private GameObject[] _optionBoxes = new GameObject[4];
        [SerializeField] private TMP_Text _status;
        [SerializeField] private Image _statusBox;
        [Header("Revelación")]
        [SerializeField] private GameObject _options;
        [SerializeField] private GameObject _reveal;
        [SerializeField] private TMP_Text _answer;
        [SerializeField] private TMP_Text _explanation;
        [Header("Cheque de la victoria")]
        [SerializeField] private GameObject _check;
        [SerializeField] private TMP_Text _checkPayee;
        [SerializeField] private TMP_Text _checkAmount;

        public bool IsShowingCheck => _check.activeSelf;

        private int _shownSeconds = -1;

        private void OnEnable()
        {
            _runtime.Changed += Refresh;
            Refresh();
        }

        private void OnDisable() => _runtime.Changed -= Refresh;

        private void Update()
        {
            var session = _runtime.Session;
            if (session == null || !HasClock(session))
                return;

            var seconds = Mathf.CeilToInt(session.PhaseSecondsLeft);
            if (seconds == _shownSeconds)
                return;

            _shownSeconds = seconds;
            _status.text = ClockText(session.Phase, seconds);
        }

        private void Refresh()
        {
            var session = _runtime.Session;
            var question = session?.CurrentQuestion;
            var showsQuestion = question != null && (session.Phase == MatchPhase.Betting ||
                                                     session.Phase == MatchPhase.Locked ||
                                                     session.Phase == MatchPhase.Reveal);

            var victory = session != null && session.Phase == MatchPhase.Victory;
            _check.SetActive(victory);
            _content.SetActive(showsQuestion);
            _message.gameObject.SetActive(!showsQuestion && !victory);
            if (victory)
            {
                var result = session.Result;
                _checkPayee.text = string.IsNullOrEmpty(result.Survivors) ? "El equipo" : result.Survivors;
                _checkAmount.text = MoneyFormat.Amount(result.FinalMoney);
                return;
            }
            if (!showsQuestion)
            {
                _message.text = MessageFor(session);
                return;
            }

            _header.text = $"PREGUNTA {session.QuestionNumber}/{session.TotalQuestions}  ·  {question.Category.ToUpperInvariant()}";
            _question.text = question.Text;

            var correct = session.CorrectBooth;
            for (var booth = 0; booth < _optionTexts.Length; booth++)
            {
                _optionTexts[booth].text = question.Option(session.Order.OriginalAt(booth));
                var lit = correct < 0 || correct == booth;
                _optionBoxes[booth].SetActive(lit);
                _optionTexts[booth].color = lit ? Color.white : DimmedText;
            }

            _shownSeconds = -1;
            var reveal = session.Phase == MatchPhase.Reveal;
            _status.text = reveal
                ? $"CORRECTA: {(char)('A' + correct)}"
                : ClockText(session.Phase, Mathf.CeilToInt(session.PhaseSecondsLeft), session.IsBettingTimed);
            // Al revelar: la respuesta en grande con «CORRECTA» y la explicación, en lugar de las opciones y el reloj.
            _options.SetActive(!reveal);
            _statusBox.gameObject.SetActive(!reveal);
            _reveal.SetActive(reveal);
            if (reveal)
            {
                _answer.text = $"{(char)('A' + correct)} · {question.Option(session.Order.OriginalAt(correct)).ToUpperInvariant()}";
                _explanation.text = question.Explanation;
            }
        }

        private static bool HasClock(IMatchSession session) =>
            session.Phase == MatchPhase.Locked || (session.Phase == MatchPhase.Betting && session.IsBettingTimed);

        private static string ClockText(MatchPhase phase, int seconds, bool bettingTimed = true)
        {
            if (phase == MatchPhase.Locked)
                return $"CERRADO · {seconds}";
            if (!bettingTimed)
                return "¡A APOSTAR!";
            var clock = $"{seconds / 60:00}:{seconds % 60:00}"; // «01:29», como el HUD.
            return seconds <= URGENT_SECONDS ? $"<color={RED}>{clock}</color>" : clock;
        }

        private static string MessageFor(IMatchSession session) => session == null ? "TRIVIAS" : session.Phase switch
        {
            MatchPhase.Victory => "¡VICTORIA!",
            MatchPhase.Defeat => "DERROTA",
            _ => "¿PREPARADOS?",
        };
    }
}
