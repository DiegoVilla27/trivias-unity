using UnityEngine;

namespace Trivias.Player.Components
{
    /// <summary>
    /// Recibe los eventos de las animaciones de locomoción (pasos y aterrizaje).
    /// Vive junto al Animator. Usa un AudioSource propio: sin crear objetos por cada paso.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class PlayerAnimationEvents : MonoBehaviour
    {
        private const float MIN_CLIP_WEIGHT = 0.5f;

        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _footstepClips;
        [SerializeField] private AudioClip _landingClip;
        [SerializeField, Range(0f, 1f)] private float _volume = 0.3f;

        // Llamado por eventos de animación.
        public void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight < MIN_CLIP_WEIGHT || _footstepClips.Length == 0)
                return;

            var clip = _footstepClips[Random.Range(0, _footstepClips.Length)];
            _audioSource.PlayOneShot(clip, _volume);
        }

        // Llamado por eventos de animación.
        public void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight < MIN_CLIP_WEIGHT || _landingClip == null)
                return;

            _audioSource.PlayOneShot(_landingClip, _volume);
        }
    }
}
