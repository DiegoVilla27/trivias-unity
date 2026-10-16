using System.Collections;
using Trivias.Shared.Tubes;
using UnityEngine;

namespace Trivias.Player.Components
{
    /// <summary>
    /// Expulsión del concursante eliminado (GDD 8): lo succiona el tubo que cuelga sobre su plataforma. Se eleva y se
    /// tambalea hacia la boca (con aire y sonido de succión), entra estirándose y girando, y recorre el tubo hasta el
    /// techo. Sin tubo cerca, sale disparado hacia arriba. Animación breve por código: una vez por partida.
    /// </summary>
    public sealed class PlayerExpulsionView : MonoBehaviour
    {
        [SerializeField] private Transform _model;
        [SerializeField] private GameObject _nameTag;
        [SerializeField] private CharacterController _controller;
        [SerializeField] private TubeRegistry _tubes;
        [Tooltip("Distancia máxima (en horizontal) a la boca de un tubo para que lo succione.")]
        [SerializeField, Min(0f)] private float _tubeReach = 2.5f;
        [Header("Succión por el tubo")]
        [SerializeField, Min(0.05f)] private float _windUpSeconds = 0.45f;
        [SerializeField, Min(0.05f)] private float _riseSeconds = 0.5f;
        [SerializeField, Min(0f)] private float _tubeStartSpeed = 6f;
        [SerializeField, Min(0f)] private float _tubeAcceleration = 30f;
        [Header("Sin tubo")]
        [SerializeField, Min(0.1f)] private float _duration = 1.2f;
        [SerializeField, Min(0f)] private float _height = 8f;
        [SerializeField] private float _spinDegreesPerSecond = 720f;

        private Vector3 _modelRestPosition;
        private Quaternion _modelRestRotation;
        private Vector3 _modelRestScale;

        private void Awake()
        {
            _modelRestPosition = _model.localPosition;
            _modelRestRotation = _model.localRotation;
            _modelRestScale = _model.localScale;
        }

        /// <summary>Deshace la expulsión para una nueva partida.</summary>
        public void Restore(bool showNameTag)
        {
            StopAllCoroutines();
            _model.gameObject.SetActive(true);
            _model.SetLocalPositionAndRotation(_modelRestPosition, _modelRestRotation);
            _model.localScale = _modelRestScale;
            _controller.enabled = true;
            _nameTag.SetActive(showNameTag);
        }

        public void Play()
        {
            // Deja de ocupar sitio en el plató enseguida para no estorbar a los demás.
            _controller.enabled = false;
            _nameTag.SetActive(false);
            if (_tubes != null && _tubes.TryNearest(TubeKind.Player, _model.position, _tubeReach, out var tube))
                StartCoroutine(SuckThrough(tube));
            else
                StartCoroutine(Launch());
        }

        private IEnumerator SuckThrough(TubePath tube)
        {
            tube.PlaySuction();
            var start = _model.position;
            var startRotation = _model.rotation;
            var inlet = tube.Inlet;

            // 1. El aire tira: se eleva un poco y se tambalea, cada vez más fuerte.
            for (var t = 0f; t < _windUpSeconds; t += Time.deltaTime)
            {
                var k = t / _windUpSeconds;
                var wobble = Mathf.Sin(t * 38f) * 6f * k;
                _model.SetPositionAndRotation(start + Vector3.up * (0.25f * k * k), startRotation * Quaternion.Euler(wobble, 0f, wobble * 0.6f));
                _model.localScale = Vector3.Scale(_modelRestScale, new Vector3(1f - 0.06f * k, 1f + 0.08f * k, 1f - 0.06f * k));
                yield return null;
            }

            // 2. Sale disparado hacia la boca: se estira y gira.
            var lifted = _model.position;
            for (var t = 0f; t < _riseSeconds; t += Time.deltaTime)
            {
                var k = PathMath.EaseIn(t / _riseSeconds);
                _model.SetPositionAndRotation(Vector3.LerpUnclamped(lifted, inlet - Vector3.up * 0.9f, k),
                    startRotation * Quaternion.AngleAxis(t * _spinDegreesPerSecond, Vector3.up));
                _model.localScale = Vector3.Scale(_modelRestScale, new Vector3(1f - 0.25f * k, 1f + 0.35f * k, 1f - 0.25f * k));
                yield return null;
            }

            // 3. Por dentro del tubo hasta el techo, acelerando.
            var entry = _model.position;
            for (var t = 0f; ; t += Time.deltaTime)
            {
                var distance = PathMath.SuctionDistance(t, _tubeStartSpeed, _tubeAcceleration);
                if (distance >= tube.Length + 1f)
                    break;
                tube.Sample(Mathf.Max(0f, distance - 1f), out var position, out _);
                // El primer metro, de la entrada a la boca; después, el eje del tubo.
                var target = distance < 1f ? Vector3.LerpUnclamped(entry, inlet, distance) : position;
                _model.SetPositionAndRotation(target - Vector3.up * 0.9f,
                    startRotation * Quaternion.AngleAxis((t + _riseSeconds) * _spinDegreesPerSecond, Vector3.up));
                yield return null;
            }

            _model.gameObject.SetActive(false);
        }

        private IEnumerator Launch()
        {
            var start = _model.localPosition;
            var elapsed = 0f;
            while (elapsed < _duration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / _duration;
                _model.localPosition = start + Vector3.up * (_height * t * t);
                _model.Rotate(Vector3.up, _spinDegreesPerSecond * Time.deltaTime, Space.Self);
                yield return null;
            }

            _model.gameObject.SetActive(false);
        }
    }
}
