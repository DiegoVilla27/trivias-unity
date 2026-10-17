using Trivias.Match.Data;
using Trivias.Match.Logic;
using Trivias.Presenter.Data;
using UnityEngine;

namespace Trivias.Presenter.Components
{
    /// <summary>
    /// Anima al presentador según lo que pasa: se alegra si el equipo acierta (o gana) y se decepciona si lo pierde
    /// todo (o pierde la partida); mientras habla, se lleva el micrófono a la boca, gesticula con la otra mano y mueve la
    /// cabeza. Hablar va en capas aparte (brazo derecho, brazo izquierdo y cabeza) que se mezclan con suavidad encima del
    /// reposo o de la reacción. Cada jugador lo calcula con la misma revelación: no viaja por red. Por fotograma solo
    /// trabaja mientras una capa cambia de peso.
    /// </summary>
    public sealed class PresenterAnimator : MonoBehaviour
    {
        public const string HAPPY = "Happy";
        public const string SAD = "Sad";
        public const int MIC_LAYER = 1;
        public const int GESTURE_LAYER = 2;
        public const int HEAD_LAYER = 3;
        private static readonly int HappyTrigger = Animator.StringToHash(HAPPY);
        private static readonly int SadTrigger = Animator.StringToHash(SAD);
        private static readonly int HappyState = Animator.StringToHash(HAPPY);
        private static readonly int SadState = Animator.StringToHash(SAD);

        [SerializeField] private MatchRuntime _match;
        [SerializeField] private PresenterRuntime _speech;
        [SerializeField] private Animator _animator;
        [Tooltip("Segundos en llevarse el micro a la boca (o bajarlo).")]
        [SerializeField, Min(0.05f)] private float _talkBlendSeconds = 0.3f;

        private bool _talking;
        private int _reactedTo = -1;
        private readonly float[] _weights = new float[4];

        /// <summary>Está hablando (micro en la boca).</summary>
        public bool IsTalking => _talking;

        public Animator Animator => _animator;

        private void OnEnable()
        {
            _match.Changed += OnMatchChanged;
            _speech.Spoke += OnSpoke;
            OnSpoke(_speech.CurrentLine);
        }

        private void OnDisable()
        {
            _match.Changed -= OnMatchChanged;
            _speech.Spoke -= OnSpoke;
        }

        private void OnSpoke(string line) => SetTalking(!string.IsNullOrEmpty(line));

        /// <summary>Al revelar: alegría si se conserva dinero en la correcta, decepción si no. Al acabar, según el resultado.</summary>
        private void OnMatchChanged()
        {
            var session = _match.Session;
            if (session == null)
            {
                _reactedTo = -1;
                return;
            }
            // Una reacción por momento: la partida avisa de muchos cambios durante la revelación.
            var moment = session.QuestionNumber * 8 + (int)session.Phase;
            if (moment == _reactedTo)
                return;
            switch (session.Phase)
            {
                case MatchPhase.Reveal:
                    _reactedTo = moment;
                    React(session.LastRound.Kept > 0);
                    break;
                case MatchPhase.Victory:
                    _reactedTo = moment;
                    React(true);
                    break;
                case MatchPhase.Defeat:
                    _reactedTo = moment;
                    React(false);
                    break;
            }
        }

        public void React(bool happy)
        {
            _animator.ResetTrigger(happy ? SadTrigger : HappyTrigger);
            _animator.SetTrigger(happy ? HappyTrigger : SadTrigger);
        }

        public void SetTalking(bool talking) => _talking = talking;

        private void Update()
        {
            var step = Time.deltaTime / _talkBlendSeconds;
            var talk = _talking ? 1f : 0f;
            // Mientras celebra o se lamenta, la mano libre hace la reacción, no los gestos de hablar.
            var state = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            var reacting = state == HappyState || state == SadState || _animator.IsInTransition(0);
            Blend(MIC_LAYER, talk, step);
            Blend(GESTURE_LAYER, _talking && !reacting ? 1f : 0f, step);
            Blend(HEAD_LAYER, talk, step);
        }

        private void Blend(int layer, float target, float step)
        {
            var current = _weights[layer];
            if (Mathf.Approximately(current, target))
                return;
            current = Mathf.MoveTowards(current, target, step);
            _weights[layer] = current;
            // Entra y sale con suavidad (no en línea recta).
            _animator.SetLayerWeight(layer, current * current * (3f - 2f * current));
        }
    }
}
