using Trivias.Feedback.Logic;
using Trivias.Match.Data;
using UnityEngine;

namespace Trivias.Feedback.Components
{
    /// <summary>
    /// Bombillas de las cabinas y de la marquesina de la pantalla: persecución mientras se juega, parpadeo en la cuenta
    /// atrás, la cabina correcta encendida (y las falsas apagadas) al revelar, y fiesta en la victoria. Cambia el brillo
    /// con un MaterialPropertyBlock (sin duplicar materiales), unas 24 veces por segundo.
    /// </summary>
    public sealed class StageBulbs : MonoBehaviour
    {
        private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
        private const float TICK = 1f / 24f;

        [SerializeField] private MatchRuntime _match;
        [SerializeField] private Renderer[] _bulbs = new Renderer[0];
        [Tooltip("Cabina de cada bombilla (0..3) o -1 si es de la marquesina.")]
        [SerializeField] private int[] _booths = new int[0];
        [Tooltip("Posición de cada bombilla en su fila (para las persecuciones).")]
        [SerializeField] private int[] _orders = new int[0];

        private Color[] _baseEmission;
        private MaterialPropertyBlock _block;
        private float _sinceTick;
        private bool _dirty = true;

        public StageMood Mood { get; private set; } = StageMood.Lobby;

        /// <summary>Brillo actual de una bombilla (lo usan las pruebas).</summary>
        public float LevelOf(int bulb) => StageShowRules.BulbLevel(Mood, _booths[bulb], CorrectBooth, _orders[bulb], Time.time);

        public int Count => _bulbs.Length;
        public int BoothOf(int bulb) => _booths[bulb];

        private int CorrectBooth => _match.Session?.CorrectBooth ?? -1;

        private void Awake()
        {
            _block = new MaterialPropertyBlock();
            _baseEmission = new Color[_bulbs.Length];
            for (var i = 0; i < _bulbs.Length; i++)
                _baseEmission[i] = _bulbs[i].sharedMaterial.GetColor(EmissionId);
        }

        private void OnEnable() => _match.Changed += OnMatchChanged;

        private void OnDisable() => _match.Changed -= OnMatchChanged;

        private void OnMatchChanged() => _dirty = true;

        private void LateUpdate()
        {
            if (_dirty)
            {
                _dirty = false;
                var session = _match.Session;
                Mood = FeedbackRules.MoodFor(MatchSnapshots.From(session), session?.TotalQuestions ?? 0);
            }

            _sinceTick += Time.deltaTime;
            if (_sinceTick < TICK)
                return;
            _sinceTick = 0f;

            var time = Time.time;
            var correct = CorrectBooth;
            for (var i = 0; i < _bulbs.Length; i++)
            {
                var level = StageShowRules.BulbLevel(Mood, _booths[i], correct, _orders[i], time);
                _block.SetColor(EmissionId, _baseEmission[i] * level);
                _bulbs[i].SetPropertyBlock(_block);
            }
        }
    }
}
