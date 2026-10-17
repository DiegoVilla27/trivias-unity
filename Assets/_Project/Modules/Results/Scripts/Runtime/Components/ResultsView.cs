using TMPro;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Money.Logic;
using Trivias.Ranking.Data;
using Trivias.Ranking.Logic;
using Trivias.Shared.Events;
using Trivias.Shared.Presentation;
using Trivias.Shared.Profile;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Results.Components
{
    /// <summary>
    /// Pantalla de resultados (GDD 3.3), con la convención de los menús: plató desenfocado detrás, panel con marco
    /// dorado, título grande, cifras en tarjetas y botones del kit. Victoria o derrota, preguntas superadas, dinero final,
    /// tiempo total, fichas para cada jugador, supervivientes y expulsados. Las victorias se presentan a los récords
    /// locales (GDD 12) y se dice en qué puesto quedan. El host puede volver a la sala para otra partida; todos pueden salir.
    /// </summary>
    public sealed class ResultsView : MonoBehaviour
    {
        private const string GOLD = "#FFD15C";
        private static readonly VertexGradient VictoryGradient = new(Hex(0xFFF3B0), Hex(0xFFF3B0), Hex(0xE9A63B), Hex(0xE9A63B));
        private static readonly VertexGradient DefeatGradient = new(Hex(0xFFC2C2), Hex(0xFFC2C2), Hex(0xD2475E), Hex(0xD2475E));

        [SerializeField] private MatchRuntime _match;
        [SerializeField] private VoidEventChannel _leaveRoomRequested;
        [SerializeField] private PlayerProfile _profile;
        [SerializeField] private LocalRecords _records;
        [SerializeField] private UiBackdrop _backdrop;
        [Tooltip("Capa a pantalla completa con el panel (se ve o no).")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _subtitle;
        [SerializeField] private TextMeshProUGUI _questions;
        [SerializeField] private TextMeshProUGUI _money;
        [SerializeField] private TextMeshProUGUI _time;
        [SerializeField] private TextMeshProUGUI _record;
        [SerializeField] private TextMeshProUGUI _tokens;
        [SerializeField] private TextMeshProUGUI _tokensHint;
        [SerializeField] private TextMeshProUGUI _survivors;
        [SerializeField] private TextMeshProUGUI _eliminated;
        [SerializeField] private Button _playAgainButton;
        [SerializeField] private TextMeshProUGUI _playAgainLabel;
        [SerializeField] private Button _leaveButton;
        [Tooltip("Al ganar, segundos de celebración (cheque, podio, confeti) antes de mostrar los resultados.")]
        [SerializeField, Min(0f)] private float _victoryCelebrationSeconds = 5f;

        private float _showAt = -1f;

        public bool IsShown => _panel.activeSelf;

        /// <summary>Lo que respondieron los récords a la última partida mostrada.</summary>
        public RecordOutcome LastRecord { get; private set; }

        private void OnEnable()
        {
            _match.Changed += Refresh;
            _playAgainButton.onClick.AddListener(OnPlayAgain);
            _leaveButton.onClick.AddListener(OnLeave);
            Refresh();
        }

        private void OnDisable()
        {
            _match.Changed -= Refresh;
            _playAgainButton.onClick.RemoveListener(OnPlayAgain);
            _leaveButton.onClick.RemoveListener(OnLeave);
            _backdrop.Show(this, false);
        }

        private void Refresh()
        {
            var session = _match.Session;
            var finished = session != null && MatchRules.IsFinished(session.Phase);
            if (!finished)
            {
                SetShown(false);
                _showAt = -1f;
                return;
            }

            var result = session.Result;
            // Las fichas se guardan en el perfil local una sola vez por partida (GDD 11).
            _profile.AwardMatch(result.MatchId.ToString(), result.TokensEach);
            // Cada jugador guarda la victoria en sus récords (una vez por partida; repetir devuelve lo mismo).
            LastRecord = _records.Submit(
                RankingRules.Entry(result.MatchId.ToString(), result.StartingPlayers, result.FinalMoney, result.DurationSeconds,
                    result.Survivors, result.Eliminated, System.DateTime.Now),
                RankingRules.Eligibility(result.Victory, result.StartedAtFirstQuestion, result.StartingPlayers));
            var record = RecordText.Outcome(LastRecord);
            _record.gameObject.SetActive(record.Length > 0);
            _record.text = record;
            _record.color = LastRecord.Kind == RecordResultKind.NotEligible ? new Color(0.62f, 0.65f, 0.75f) : Hex(0xFFD15C);

            // Al ganar, primero se celebra en el plató; los resultados aparecen después.
            if (!_panel.activeSelf && _showAt < 0f)
                _showAt = Time.time + (result.Victory ? _victoryCelebrationSeconds : 0f);
            SetShown(Time.time >= _showAt);

            _title.text = result.Victory ? "¡VICTORIA!" : "DERROTA";
            _title.colorGradient = result.Victory ? VictoryGradient : DefeatGradient;
            _subtitle.text = result.Victory
                ? "¡Habéis superado todas las preguntas!"
                : $"Habéis superado {result.QuestionsPassed} de {session.TotalQuestions} preguntas";

            _questions.text = $"{result.QuestionsPassed}/{session.TotalQuestions}";
            _money.text = MoneyFormat.Amount(result.FinalMoney);
            _time.text = MatchResult.FormatDuration(result.DurationSeconds);
            _tokens.text = $"+{result.TokensEach} fichas para cada jugador";
            _tokensHint.text = $"Tienes <color={GOLD}><b>{_profile.Tokens}</b></color> fichas · gástalas en la tienda de la sala";

            var eliminated = string.IsNullOrEmpty(result.Eliminated) ? "nadie" : result.Eliminated;
            var survivors = string.IsNullOrEmpty(result.Survivors) ? "nadie" : result.Survivors;
            _survivors.text = $"Supervivientes: <b>{survivors}</b>";
            _eliminated.text = $"Expulsados: {eliminated}";

            _playAgainButton.interactable = session.IsHost;
            _playAgainLabel.text = session.IsHost ? "Volver a jugar" : "Esperando al anfitrión…";
        }

        private void SetShown(bool shown)
        {
            if (_panel.activeSelf != shown)
                _panel.SetActive(shown);
            _backdrop.Show(this, shown);
        }

        private void Update()
        {
            if (_showAt >= 0f && !_panel.activeSelf && Time.time >= _showAt)
                Refresh();
        }

        private void OnPlayAgain() => _match.Session?.RequestReturnToLobby();

        private void OnLeave() => _leaveRoomRequested.Raise();

        private static Color32 Hex(uint rgb) => new((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 255);
    }
}
