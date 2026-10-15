using Trivias.Feedback.Logic;
using Trivias.Match.Data;
using Trivias.Match.Logic;
using UnityEngine;

namespace Trivias.Feedback.Components
{
    /// <summary>
    /// Confeti del plató: chorros de colores a los lados de la pantalla en cada acierto y lluvia dorada en toda la sala
    /// mientras dura la victoria. Los sistemas de partículas se montan en el editor.
    /// </summary>
    public sealed class StageConfetti : MonoBehaviour
    {
        [SerializeField] private MatchRuntime _match;
        [Tooltip("Chorros de colores a izquierda y derecha de la pantalla.")]
        [SerializeField] private ParticleSystem[] _correctBursts = new ParticleSystem[0];
        [Tooltip("Lluvia dorada sobre toda la sala (se repite hasta volver a la sala de espera).")]
        [SerializeField] private ParticleSystem _victoryRain;

        private MatchSnapshot _last = MatchSnapshot.Lobby;
        private bool _dirty = true;

        /// <summary>Aciertos celebrados desde que se cargó la escena (lo usan las pruebas).</summary>
        public int CorrectCelebrations { get; private set; }

        public bool IsVictoryRaining => _victoryRain.isPlaying;

        private void OnEnable() => _match.Changed += OnMatchChanged;
        private void OnDisable() => _match.Changed -= OnMatchChanged;
        private void OnMatchChanged() => _dirty = true;

        private void LateUpdate()
        {
            if (!_dirty)
                return;
            _dirty = false;

            var current = MatchSnapshots.From(_match.Session);
            switch (FeedbackRules.CelebrationFor(_last, current))
            {
                case Celebration.CorrectAnswer:
                    CorrectCelebrations++;
                    for (var i = 0; i < _correctBursts.Length; i++)
                        _correctBursts[i].Play(true);
                    break;
                case Celebration.Victory:
                    _victoryRain.Play(true);
                    break;
            }

            if (current.Phase != MatchPhase.Victory && _victoryRain.isPlaying)
                _victoryRain.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _last = current;
        }
    }
}
