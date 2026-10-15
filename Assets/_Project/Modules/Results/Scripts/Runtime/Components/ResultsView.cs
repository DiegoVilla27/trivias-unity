using TMPro;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Money.Logic;
using Trivias.Ranking.Data;
using Trivias.Ranking.Logic;
using Trivias.Shared.Events;
using Trivias.Shared.Profile;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Results.Components
{
    /// <summary>
    /// Pantalla de resultados (GDD 3.3): victoria o derrota, preguntas superadas, dinero final, tiempo total, fichas para
    /// cada jugador, supervivientes y expulsados. Las victorias se presentan a los récords locales (GDD 12) y se dice en qué
    /// puesto quedan. El host puede volver a la sala para otra partida; todos pueden salir.
    /// </summary>
    public sealed class ResultsView : MonoBehaviour
    {
        private const string GOLD = "#FFD15C";
        private const string MUTED = "#FFFFFFAA";

        [SerializeField] private MatchRuntime _match;
        [SerializeField] private VoidEventChannel _leaveRoomRequested;
        [SerializeField] private PlayerProfile _profile;
        [SerializeField] private LocalRecords _records;
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _details;
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
        }

        private void Refresh()
        {
            var session = _match.Session;
            var finished = session != null && MatchRules.IsFinished(session.Phase);
            if (!finished)
            {
                _panel.SetActive(false);
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
            if (record.Length > 0)
                record = LastRecord.Kind == RecordResultKind.NotEligible
                    ? $"<size=80%><color={MUTED}>{record}</color></size>\n"
                    : $"<color={GOLD}><b>{record}</b></color>\n";

            // Al ganar, primero se celebra en el plató; los resultados aparecen después.
            if (!_panel.activeSelf && _showAt < 0f)
                _showAt = Time.time + (result.Victory ? _victoryCelebrationSeconds : 0f);
            _panel.SetActive(Time.time >= _showAt);

            _title.text = result.Victory ? "¡VICTORIA!" : "DERROTA";
            _title.color = result.Victory ? new Color(0.36f, 1f, 0.48f) : new Color(1f, 0.45f, 0.45f);

            var eliminated = string.IsNullOrEmpty(result.Eliminated) ? "nadie" : result.Eliminated;
            var survivors = string.IsNullOrEmpty(result.Survivors) ? "nadie" : result.Survivors;
            _details.text =
                $"Preguntas superadas: <b>{result.QuestionsPassed} / {session.TotalQuestions}</b>\n" +
                $"Dinero final: <b>{MoneyFormat.Amount(result.FinalMoney)}</b>\n" +
                $"Tiempo total: <b>{MatchResult.FormatDuration(result.DurationSeconds)}</b>\n" +
                record + "\n" +
                $"<color={GOLD}><b>{result.TokensEach} fichas para cada jugador</b></color>\n" +
                $"<size=80%>Tienes {_profile.Tokens} fichas · gástalas en la tienda</size>\n\n" +
                $"Supervivientes: {survivors}\n<color={MUTED}>Expulsados: {eliminated}</color>";

            _playAgainButton.interactable = session.IsHost;
            _playAgainLabel.text = session.IsHost ? "Volver a jugar" : "Esperando al host…";
        }

        private void Update()
        {
            if (_showAt >= 0f && !_panel.activeSelf && Time.time >= _showAt)
                Refresh();
        }

        private void OnPlayAgain() => _match.Session?.RequestReturnToLobby();

        private void OnLeave() => _leaveRoomRequested.Raise();
    }
}
