using Trivias.Feedback.Data;
using Trivias.Feedback.Logic;
using Trivias.Match.Data;
using Trivias.Shared.Settings;
using UnityEngine;

namespace Trivias.Feedback.Components
{
    /// <summary>
    /// Público del plató: respira y se mueve un poco siempre; aplaude al empezar y en los aciertos, celebra la victoria
    /// dando saltos y suelta un «oooh» (se hunde y se inclina) con las expulsiones y las derrotas.
    /// </summary>
    public sealed class AudienceCrowd : MonoBehaviour
    {
        [SerializeField] private MatchRuntime _match;
        [SerializeField] private FeedbackLibrary _library;
        [SerializeField] private GameSettings _settings;
        [SerializeField] private AudioSource _audio;
        [Tooltip("La parte que se mueve de cada espectador (cuerpo y cabeza; el asiento no).")]
        [SerializeField] private Transform[] _figures = new Transform[0];

        private Vector3[] _restPositions;
        private Quaternion[] _restRotations;
        private float[] _phases;
        private MatchSnapshot _last = MatchSnapshot.Lobby;
        private bool _dirty = true;
        private float _reactionStart = float.NegativeInfinity;

        public CrowdReaction Reaction { get; private set; } = CrowdReaction.None;

        private void Awake()
        {
            _restPositions = new Vector3[_figures.Length];
            _restRotations = new Quaternion[_figures.Length];
            _phases = new float[_figures.Length];
            for (var i = 0; i < _figures.Length; i++)
            {
                _restPositions[i] = _figures[i].localPosition;
                _restRotations[i] = _figures[i].localRotation;
                _phases[i] = Mathf.Repeat(i * 2.399963f, Mathf.PI * 2f); // Ángulo áureo: nadie se mueve igual.
            }
        }

        private void OnEnable() => _match.Changed += OnMatchChanged;

        private void OnDisable() => _match.Changed -= OnMatchChanged;

        private void OnMatchChanged() => _dirty = true;

        private void LateUpdate()
        {
            if (_dirty)
            {
                _dirty = false;
                var current = MatchSnapshots.From(_match.Session);
                var reaction = StageShowRules.Reaction(_last, current);
                _last = current;
                if (reaction != CrowdReaction.None)
                    React(reaction);
            }

            var time = Time.time;
            var since = time - _reactionStart;
            var duration = Reaction == CrowdReaction.Cheer ? 3.6f : 2.6f;
            var strength = since < duration ? Mathf.Clamp01(since / 0.2f) * Mathf.Clamp01((duration - since) / 0.6f) : 0f;
            if (since >= duration)
                Reaction = CrowdReaction.None; // Solo al acabar: al empezar la fuerza también vale 0.

            for (var i = 0; i < _figures.Length; i++)
            {
                var phase = _phases[i];
                var offset = new Vector3(0f, 0.012f * Mathf.Sin(time * 1.3f + phase), 0f); // Respira.
                var tilt = Quaternion.identity;
                switch (Reaction)
                {
                    case CrowdReaction.Applause:
                        offset.y += strength * 0.05f * Mathf.Abs(Mathf.Sin(time * 13f + phase));
                        tilt = Quaternion.Euler(0f, 0f, strength * 4f * Mathf.Sin(time * 3f + phase));
                        break;
                    case CrowdReaction.Cheer:
                        offset.y += strength * 0.22f * Mathf.Abs(Mathf.Sin(time * 6f + phase));
                        tilt = Quaternion.Euler(0f, 0f, strength * 9f * Mathf.Sin(time * 4f + phase));
                        break;
                    case CrowdReaction.Aww:
                        offset.y -= strength * 0.07f;
                        tilt = Quaternion.Euler(strength * 14f, 0f, 0f); // Se inclinan, decepcionados.
                        break;
                }
                _figures[i].SetLocalPositionAndRotation(_restPositions[i] + offset, _restRotations[i] * tilt);
            }
        }

        private void React(CrowdReaction reaction)
        {
            Reaction = reaction;
            _reactionStart = Time.time;
            var clip = reaction switch
            {
                CrowdReaction.Applause => _library.Applause,
                CrowdReaction.Cheer => _library.Cheer,
                CrowdReaction.Aww => _library.Aww,
                _ => null,
            };
            if (clip != null)
                _audio.PlayOneShot(clip, _library.CrowdVolume * _settings.EffectsVolume);
        }
    }
}
