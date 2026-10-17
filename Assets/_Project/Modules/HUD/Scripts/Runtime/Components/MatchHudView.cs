using TMPro;
using Trivias.HUD.Logic;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Money.Data;
using Trivias.Money.Logic;
using Trivias.SelfBet.Data;
using Trivias.Shared.Players;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.HUD.Components
{
    /// <summary>
    /// HUD de la partida (bocetos de docs/bocetos/ui/hud, versión sin saturar). Siempre, tres piezas pequeñas arriba:
    /// pregunta y estado, reloj, y dinero del equipo con los comodines. El resto solo cuando importa: dinero fuera de
    /// bandejas, lo que llevas, el comodín preparado, el cierre (3-2-1) y el resultado de la ronda, que ocupan el sitio
    /// del reloj. La pregunta y las opciones NO van aquí: se leen en la pantalla del plató.
    /// Se redibuja por eventos; por fotograma solo el reloj (al cambiar el segundo) y las animaciones mientras duran,
    /// escribiendo las cifras en búferes de caracteres para no reservar memoria.
    /// </summary>
    public sealed class MatchHudView : MonoBehaviour
    {
        private const int URGENT_SECONDS = 10;
        private const float COUNT_SECONDS = 0.6f;
        private static readonly Color Ivory = new(0.96f, 0.94f, 0.88f);
        private static readonly Color Urgent = new(1f, 0.42f, 0.42f);

        [SerializeField] private MatchRuntime _runtime;
        [SerializeField] private MoneyRuntime _money;
        [SerializeField] private SelfBetRuntime _selfBet;
        [SerializeField] private PlayerAvatarRegistry _avatars;

        [Header("Pregunta y estado (arriba a la izquierda)")]
        [SerializeField] private GameObject _questionCard;
        [SerializeField] private TMP_Text _question;
        [SerializeField] private Image _stateIcon;
        [SerializeField] private TMP_Text _state;
        [SerializeField] private Sprite _openSprite;
        [SerializeField] private Sprite _lockedSprite;
        [SerializeField] private Sprite _revealSprite;

        [Header("Reloj, cierre y resultado (arriba en el centro)")]
        [SerializeField] private GameObject _clockCard;
        [SerializeField] private TMP_Text _clock;
        [SerializeField] private GameObject _risk;
        [SerializeField] private TMP_Text _riskText;
        [SerializeField] private GameObject _lockedCard;
        [SerializeField] private TMP_Text _countdown;
        [SerializeField] private GameObject _revealCard;
        [SerializeField] private TMP_Text _kept;
        [SerializeField] private TMP_Text _bonusLabel;
        [SerializeField] private TMP_Text _bonus;
        [SerializeField] private TMP_Text _lost;
        [SerializeField] private GameObject _eliminated;
        [SerializeField] private TMP_Text _eliminatedText;

        [Header("Dinero y comodines (arriba a la derecha)")]
        [SerializeField] private GameObject _moneyCard;
        [SerializeField] private TMP_Text _teamMoney;
        [SerializeField] private GameObject _selfBetRow;
        [SerializeField] private TMP_Text _selfBets;

        [Header("Solo cuando importa")]
        [SerializeField] private GameObject _carryCard;
        [SerializeField] private Image _carryIcon;
        [SerializeField] private TMP_Text _carry;
        [SerializeField] private TMP_Text _carryLimit;
        [Tooltip("Icono de fajo, bolsa y lingote (orden de Denomination).")]
        [SerializeField] private Sprite[] _denominationIcons = new Sprite[3];
        [SerializeField] private GameObject _selfBetCard;
        [SerializeField] private TMP_Text _selfBetTitle;
        [SerializeField] private TMP_Text _selfBetProjection;
        [Tooltip("«Esc · Menú»: solo en la primera pregunta.")]
        [SerializeField] private GameObject _escHint;

        private readonly char[] _clockBuffer = new char[8];
        private readonly char[] _amountBuffer = new char[HudText.AMOUNT_BUFFER];
        private readonly char[] _countdownBuffer = new char[2];
        private int _shownSeconds = -1;
        private bool _moneyDirty;
        private bool _playing;

        // Animaciones (por fotograma solo mientras duran).
        private long _moneyFrom;
        private long _moneyTarget = -1;
        private float _moneyElapsed = COUNT_SECONDS;
        private long _keptTarget;
        private long _bonusTarget;
        private long _lostTarget;
        private float _revealElapsed = COUNT_SECONDS;
        private int _revealedQuestion = -1;
        private float _countdownPop = 1f;

        /// <summary>El HUD de la ronda se está viendo (en la sala y en los resultados, no).</summary>
        public bool IsShowing => _playing;

        private void OnEnable()
        {
            _runtime.Changed += Refresh;
            _selfBet.Changed += Refresh;
            _money.Changed += OnMoneyChanged;
            _avatars.Changed += OnAvatarChanged;
            Refresh();
        }

        private void OnDisable()
        {
            _runtime.Changed -= Refresh;
            _selfBet.Changed -= Refresh;
            _money.Changed -= OnMoneyChanged;
            _avatars.Changed -= OnAvatarChanged;
        }

        private void OnAvatarChanged(ulong _) => Refresh();

        // El dinero avisa una vez por objeto que cambia: se agrupan y se redibuja una sola vez al final del fotograma.
        private void OnMoneyChanged(int _) => _moneyDirty = true;

        private void LateUpdate()
        {
            if (_moneyDirty)
            {
                _moneyDirty = false;
                RefreshMoney();
            }
        }

        private void Update()
        {
            if (!_playing)
                return;
            var session = _runtime.Session;
            if (session == null)
                return;

            if (HasClock(session))
            {
                var seconds = Mathf.CeilToInt(session.PhaseSecondsLeft);
                if (seconds != _shownSeconds)
                    ShowSeconds(session.Phase, seconds);
                // Los últimos segundos, el reloj late.
                if (session.Phase == MatchPhase.Betting && seconds <= URGENT_SECONDS)
                    _clockCard.transform.localScale = Vector3.one * (1f + 0.06f * Mathf.Abs(Mathf.Sin(Time.time * Mathf.PI * 2f)));
            }

            if (_countdownPop < 1f)
            {
                _countdownPop = Mathf.Min(1f, _countdownPop + Time.deltaTime / 0.35f);
                _countdown.transform.localScale = Vector3.one * Mathf.LerpUnclamped(1.6f, 1f, EaseOutBack(_countdownPop));
            }

            if (_moneyElapsed < COUNT_SECONDS)
            {
                _moneyElapsed += Time.deltaTime;
                WriteAmount(_teamMoney, Lerp(_moneyFrom, _moneyTarget, _moneyElapsed / COUNT_SECONDS), '\0');
            }

            if (_revealElapsed < COUNT_SECONDS)
            {
                _revealElapsed += Time.deltaTime;
                var t = _revealElapsed / COUNT_SECONDS;
                WriteAmount(_kept, Lerp(0, _keptTarget, t), '\0');
                WriteAmount(_bonus, Lerp(0, _bonusTarget, t), '+');
                WriteAmount(_lost, Lerp(0, _lostTarget, t), '–');
            }
        }

        private void Refresh()
        {
            var session = _runtime.Session;
            _playing = session != null && session.CurrentQuestion != null &&
                       (session.Phase == MatchPhase.Betting || session.Phase == MatchPhase.Locked ||
                        session.Phase == MatchPhase.Reveal);
            _questionCard.SetActive(_playing);
            _moneyCard.SetActive(_playing);
            if (!_playing)
            {
                _clockCard.SetActive(false);
                _lockedCard.SetActive(false);
                _revealCard.SetActive(false);
                _risk.SetActive(false);
                _eliminated.SetActive(false);
                _carryCard.SetActive(false);
                _selfBetCard.SetActive(false);
                _escHint.SetActive(false);
                _moneyTarget = -1;
                return;
            }

            var phase = session.Phase;
            _question.text = HudText.Question(session.QuestionNumber, session.TotalQuestions);
            _state.text = HudText.State(phase);
            _stateIcon.sprite = phase switch
            {
                MatchPhase.Locked => _lockedSprite,
                MatchPhase.Reveal => _revealSprite,
                _ => _openSprite,
            };

            // Arriba en el centro: el reloj (si hay tiempo), o el cierre, o el resultado; nunca dos a la vez.
            _clockCard.SetActive(phase == MatchPhase.Betting && session.IsBettingTimed);
            _clockCard.transform.localScale = Vector3.one;
            _lockedCard.SetActive(phase == MatchPhase.Locked);
            _revealCard.SetActive(phase == MatchPhase.Reveal);
            var risk = session.MoneyAtRisk;
            _risk.SetActive(phase == MatchPhase.Betting && risk > 0);
            if (risk > 0)
                _riskText.text = HudText.AtRisk(risk);
            _escHint.SetActive(phase == MatchPhase.Betting && session.QuestionNumber == 1);

            _shownSeconds = -1;
            if (HasClock(session))
                ShowSeconds(phase, Mathf.CeilToInt(session.PhaseSecondsLeft));
            // El resultado se cuenta una vez por ronda (la partida avisa de más cambios durante la revelación).
            if (phase == MatchPhase.Reveal && _revealedQuestion != session.QuestionNumber)
            {
                _revealedQuestion = session.QuestionNumber;
                ShowRound(session);
            }
            else if (phase != MatchPhase.Reveal)
            {
                _revealedQuestion = -1;
                _revealElapsed = COUNT_SECONDS;
                _eliminated.SetActive(false);
            }

            ShowTeamMoney(session.TeamMoney);
            RefreshSelfBets();
            RefreshMoney();
        }

        private static bool HasClock(IMatchSession session) =>
            session.Phase == MatchPhase.Locked || (session.Phase == MatchPhase.Betting && session.IsBettingTimed);

        private void ShowSeconds(MatchPhase phase, int seconds)
        {
            _shownSeconds = seconds;
            if (phase == MatchPhase.Locked)
            {
                // Cuenta atrás del cierre: un número grande que salta en cada segundo.
                _countdownBuffer[0] = (char)('0' + Mathf.Clamp(seconds, 0, 9));
                _countdown.SetCharArray(_countdownBuffer, 0, 1);
                _countdownPop = 0f;
                return;
            }
            _clock.SetCharArray(_clockBuffer, 0, HudText.WriteClock(seconds, _clockBuffer));
            _clock.color = seconds <= URGENT_SECONDS ? Urgent : Ivory;
            if (seconds > URGENT_SECONDS)
                _clockCard.transform.localScale = Vector3.one;
        }

        /// <summary>Conservado, bono y perdido de la ronda, subiendo desde cero.</summary>
        private void ShowRound(IMatchSession session)
        {
            var round = session.LastRound;
            _keptTarget = round.Kept;
            _bonusTarget = round.Bonus;
            _lostTarget = round.Lost;
            _bonusLabel.text = HudText.Bonus(Mathf.Max(1, session.LastMultiplier));
            _revealElapsed = 0f;
            WriteAmount(_kept, 0, '\0');
            WriteAmount(_bonus, 0, '+');
            WriteAmount(_lost, 0, '–');
            var eliminated = session.LastEliminated;
            _eliminated.SetActive(!string.IsNullOrEmpty(eliminated));
            if (_eliminated.activeSelf)
                _eliminatedText.text = $"Expulsados: {eliminated}";
        }

        /// <summary>El dinero del equipo cuenta hasta la cifra nueva cuando cambia.</summary>
        private void ShowTeamMoney(long amount)
        {
            if (amount == _moneyTarget)
                return;
            if (_moneyTarget < 0)
            {
                _moneyTarget = amount;
                _moneyElapsed = COUNT_SECONDS;
                WriteAmount(_teamMoney, amount, '\0');
                return;
            }
            _moneyFrom = _moneyTarget;
            _moneyTarget = amount;
            _moneyElapsed = 0f;
        }

        /// <summary>Comodines propios y de reserva (en la tarjeta del dinero) y el comodín preparado, si lo hay.</summary>
        private void RefreshSelfBets()
        {
            var bets = _selfBet.Session;
            var session = _runtime.Session;
            if (bets == null || session == null || _avatars.IsEliminated(bets.LocalClientId))
            {
                // Un eliminado ya no tiene comodines ni nada que preparar.
                _selfBetRow.SetActive(false);
                _selfBetCard.SetActive(false);
                return;
            }

            var book = bets.Book;
            var index = book.IndexOf(bets.LocalClientId);
            var personal = index >= 0 ? book.Bettors[index].PersonalUses : 0;
            _selfBetRow.SetActive(true);
            _selfBets.text = HudText.SelfBets(personal, book.Reserve);

            var booth = index >= 0 && book.Bettors[index].HasBet ? book.Bettors[index].Booth : -1;
            _selfBetCard.SetActive(booth >= 0 && session.Phase == MatchPhase.Betting);
            if (_selfBetCard.activeSelf)
                RefreshProjection(booth);
        }

        private void RefreshProjection(int booth)
        {
            var ledger = _money.Session?.Ledger;
            var tray = ledger?.TotalAt(MoneyPlace.Tray, booth) ?? 0;
            _selfBetTitle.text = HudText.SelfBetTitle(booth);
            _selfBetProjection.text = HudText.Projection(tray, 1 + _selfBet.Session.Book.CountOn(booth));
        }

        /// <summary>Lo que lleva el jugador local (solo mientras lleva algo) y la previsión del comodín.</summary>
        private void RefreshMoney()
        {
            var money = _money.Session;
            var session = _runtime.Session;
            if (money == null || session == null || !_playing || session.Phase != MatchPhase.Betting)
            {
                _carryCard.SetActive(false);
                return;
            }

            var ledger = money.Ledger;
            var me = money.LocalClientId;
            var first = ledger.FindCarriedBy(me);
            _carryCard.SetActive(first >= 0);
            if (first >= 0)
            {
                var denomination = ledger.Items[first].Denomination;
                var count = ledger.CountCarriedBy(me);
                _carryIcon.sprite = _denominationIcons[(int)denomination];
                _carry.text = HudText.Carry(denomination, count, ledger.CarriedValue(me));
                _carryLimit.text = HudText.CarryLimit(denomination, count, ledger.CarryLimit(denomination));
                _carryLimit.color = count >= ledger.CarryLimit(denomination) ? Urgent : Ivory;
            }

            if (_selfBetCard.activeSelf)
            {
                var bets = _selfBet.Session;
                var index = bets.Book.IndexOf(bets.LocalClientId);
                if (index >= 0 && bets.Book.Bettors[index].HasBet)
                    RefreshProjection(bets.Book.Bettors[index].Booth);
            }
        }

        private void WriteAmount(TMP_Text label, long value, char sign)
        {
            var length = HudText.WriteAmount(value, _amountBuffer, value == 0 ? '\0' : sign);
            label.SetCharArray(_amountBuffer, 0, length);
        }

        private static long Lerp(long from, long to, float t)
        {
            t = Mathf.Clamp01(t);
            t = 1f - (1f - t) * (1f - t); // Frena al llegar.
            return from + (long)((to - from) * (double)t);
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            var x = t - 1f;
            return 1f + c3 * x * x * x + c1 * x * x;
        }
    }
}
