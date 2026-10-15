using System.Collections;
using UnityEngine;

namespace Trivias.Player.Components
{
    /// <summary>
    /// Expulsión del concursante eliminado (GDD 8): sale disparado hacia arriba girando y desaparece.
    /// Animación breve por código, sin simular el tubo. Ocurre una vez por partida, no por frame.
    /// </summary>
    public sealed class PlayerExpulsionView : MonoBehaviour
    {
        [SerializeField] private Transform _model;
        [SerializeField] private GameObject _nameTag;
        [SerializeField] private CharacterController _controller;
        [SerializeField, Min(0.1f)] private float _duration = 1.2f;
        [SerializeField, Min(0f)] private float _height = 8f;
        [SerializeField] private float _spinDegreesPerSecond = 720f;

        private Vector3 _modelRestPosition;
        private Quaternion _modelRestRotation;

        private void Awake()
        {
            _modelRestPosition = _model.localPosition;
            _modelRestRotation = _model.localRotation;
        }

        /// <summary>Deshace la expulsión para una nueva partida.</summary>
        public void Restore(bool showNameTag)
        {
            StopAllCoroutines();
            _model.gameObject.SetActive(true);
            _model.SetLocalPositionAndRotation(_modelRestPosition, _modelRestRotation);
            _controller.enabled = true;
            _nameTag.SetActive(showNameTag);
        }

        public void Play()
        {
            // Deja de ocupar sitio en el plató enseguida para no estorbar a los demás.
            _controller.enabled = false;
            _nameTag.SetActive(false);
            StartCoroutine(Launch());
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
