using Trivias.Shared.Settings;
using UnityEngine;

namespace Trivias.Shared.Tubes
{
    /// <summary>
    /// Recorrido interior de un tubo de cabina (de su entrada a su salida en el techo), con el aire que entra y el
    /// sonido de succión. Los puntos los calcula el constructor de la cabina en edición; aquí solo se recorren.
    /// </summary>
    public sealed class TubePath : MonoBehaviour
    {
        [SerializeField] private TubeRegistry _registry;
        [SerializeField] private TubeKind _kind;
        [Tooltip("Cabina (0..3 = A..D).")]
        [SerializeField] private int _booth;
        [Tooltip("Recorrido en coordenadas locales, de la entrada al final (codos muestreados en arco).")]
        [SerializeField] private Vector3[] _points = new Vector3[0];
        [Tooltip("Aire que entra en el tubo al succionar.")]
        [SerializeField] private ParticleSystem _air;
        [SerializeField] private AudioSource _audio;
        [SerializeField] private GameSettings _settings;
        [SerializeField, Range(0f, 1f)] private float _volume = 0.9f;

        private Vector3[] _worldPoints;
        private float[] _lengths;

        public TubeKind Kind => _kind;
        public int Booth => _booth;
        public float Length => Lengths[Lengths.Length - 1];
        public Vector3 Inlet => transform.TransformPoint(_points.Length > 0 ? _points[0] : Vector3.zero);
        public bool IsSucking => (_audio != null && _audio.isPlaying) || (_air != null && _air.isPlaying);

        private float[] Lengths
        {
            get
            {
                if (_lengths == null)
                    Prepare();
                return _lengths;
            }
        }

        private void OnEnable()
        {
            if (_registry != null)
                _registry.Register(this);
        }

        private void OnDisable()
        {
            if (_registry != null)
                _registry.Unregister(this);
        }

        /// <summary>Punto y dirección del recorrido a <paramref name="distance"/> metros de la entrada (en el mundo).</summary>
        public void Sample(float distance, out Vector3 position, out Vector3 direction)
        {
            _ = Lengths;
            PathMath.Sample(_worldPoints, _lengths, distance, out position, out direction);
        }

        /// <summary>Aire hacia dentro y sonido de succión (si ya está sonando, no vuelve a empezar).</summary>
        public void PlaySuction()
        {
            if (_air != null && !_air.isPlaying)
                _air.Play();
            if (_audio != null && _audio.clip != null && !_audio.isPlaying)
            {
                _audio.volume = _volume * (_settings != null ? _settings.EffectsVolume : 1f);
                _audio.Play();
            }
        }

        /// <summary>Los tubos son decorado estático: el recorrido en el mundo se calcula una sola vez.</summary>
        private void Prepare()
        {
            _worldPoints = new Vector3[_points.Length];
            for (var i = 0; i < _points.Length; i++)
                _worldPoints[i] = transform.TransformPoint(_points[i]);
            _lengths = PathMath.CumulativeLengths(_worldPoints);
        }

#if UNITY_EDITOR
        public void EditorSet(TubeRegistry registry, TubeKind kind, int booth, Vector3[] points, ParticleSystem air,
            AudioSource audio, GameSettings settings)
        {
            _registry = registry;
            _kind = kind;
            _booth = booth;
            _points = points;
            _air = air;
            _audio = audio;
            _settings = settings;
        }
#endif
    }
}
