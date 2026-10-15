using UnityEngine;

namespace Trivias.Presenter.Data
{
    /// <summary>Ritmo de los comentarios: no hablar por cada interacción menor ni tapar la pregunta (GDD 9.1).</summary>
    [CreateAssetMenu(menuName = "Trivias/Presenter/Presenter Config", fileName = "PresenterConfig")]
    public sealed class PresenterConfig : ScriptableObject
    {
        [Tooltip("Segundos que se muestra cada frase.")]
        [SerializeField, Range(2f, 15f)] private float _lineSeconds = 6f;
        [Tooltip("Discusión prolongada: primera frase tras estos segundos sin verificar.")]
        [SerializeField, Range(10f, 180f)] private float _longDiscussionAfter = 45f;
        [Tooltip("Discusión prolongada: separación mínima entre comentarios.")]
        [SerializeField, Range(10f, 180f)] private float _longDiscussionRepeat = 40f;
        [Tooltip("Con tiempo para apostar: avisar cuando queden estos segundos.")]
        [SerializeField, Range(3f, 30f)] private float _timeWarningSeconds = 10f;
        [Tooltip("Separación mínima entre comentarios por comodines.")]
        [SerializeField, Range(2f, 60f)] private float _selfBetCooldown = 10f;

        public float LineSeconds => _lineSeconds;
        public float LongDiscussionAfter => _longDiscussionAfter;
        public float LongDiscussionRepeat => _longDiscussionRepeat;
        public float SelfBetCooldown => _selfBetCooldown;
        public float TimeWarningSeconds => _timeWarningSeconds;
    }
}
