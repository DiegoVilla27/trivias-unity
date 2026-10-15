using UnityEngine;

namespace Trivias.Match.Data
{
    /// <summary>Ritmo y reglas de la partida (GDD 3-4). Valores ajustables sin tocar código.</summary>
    [CreateAssetMenu(menuName = "Trivias/Match/Match Config", fileName = "MatchConfig")]
    public sealed class MatchConfig : ScriptableObject
    {
        [SerializeField, Range(1, 30)] private int _questionsPerMatch = 15;
        [Tooltip("Segundos para repartir el dinero en cada pregunta. Al llegar a 0 se verifica sola. 0 = sin límite.")]
        [SerializeField, Range(0, 300)] private int _bettingSeconds = 90;
        [Tooltip("Cuenta atrás tras pulsar VERIFICAR (GDD: 3 segundos).")]
        [SerializeField, Range(1f, 10f)] private float _lockCountdownSeconds = 3f;
        [Tooltip("Tiempo mostrando la respuesta y el resultado antes de la siguiente pregunta.")]
        [SerializeField, Range(2f, 20f)] private float _revealSeconds = 7f;
        [Tooltip("Regla provisional (GDD 4.2): el dinero que no está en una bandeja al verificar se pierde.")]
        [SerializeField] private bool _loseUnplacedMoney = true;

        [Header("Fichas por tramo (GDD 11)")]
        [Tooltip("Preguntas superadas necesarias para cada tramo (orden creciente). El tramo de todas exige victoria.")]
        [SerializeField] private int[] _tokenTierMinQuestions = { 3, 6, 11, 15 };
        [Tooltip("Fichas para cada jugador en cada tramo.")]
        [SerializeField] private int[] _tokenTierRewards = { 50, 100, 150, 250 };

        public int QuestionsPerMatch => _questionsPerMatch;
        public int BettingSeconds => _bettingSeconds;
        public bool HasBettingTimer => _bettingSeconds > 0;
        public float LockCountdownSeconds => _lockCountdownSeconds;
        public float RevealSeconds => _revealSeconds;
        public bool LoseUnplacedMoney => _loseUnplacedMoney;
        public int[] TokenTierMinQuestions => _tokenTierMinQuestions;
        public int[] TokenTierRewards => _tokenTierRewards;

        private void OnValidate()
        {
            if (_tokenTierRewards.Length != _tokenTierMinQuestions.Length)
                Debug.LogWarning($"[{name}] Los tramos de fichas deben tener el mismo número de mínimos y recompensas.", this);
        }
    }
}
