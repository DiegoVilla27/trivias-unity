using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Shared.Presentation
{
    /// <summary>Marquesina de bombillas alrededor de un panel, como las del plató: una onda de luz que da la vuelta.</summary>
    public sealed class UiMarquee : MonoBehaviour
    {
        private const float TICK = 1f / 20f;

        [SerializeField] private Image[] _bulbs = new Image[0];
        [SerializeField] private Color _on = new(1f, 0.86f, 0.45f, 1f);
        [SerializeField] private Color _off = new(0.55f, 0.38f, 0.12f, 0.55f);
        [SerializeField] private float _speed = 7f;

        private float _sinceTick;

        public int Count => _bulbs.Length;

        private void Update()
        {
            _sinceTick += Time.unscaledDeltaTime;
            if (_sinceTick < TICK)
                return;
            _sinceTick = 0f;
            var time = Time.unscaledTime;
            for (var i = 0; i < _bulbs.Length; i++)
            {
                var wave = 0.5f + 0.5f * Mathf.Sin(i * 0.8f - time * _speed);
                _bulbs[i].color = Color.Lerp(_off, _on, wave * wave);
            }
        }
    }
}
