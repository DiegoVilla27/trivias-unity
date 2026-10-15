using Trivias.Feedback.Logic;
using UnityEngine;

namespace Trivias.Feedback.Data
{
    /// <summary>
    /// Sonidos y música del juego con su volumen. Los clips se sintetizan por código al construir el proyecto
    /// (FeedbackBuilder): no hay audio externo en el MVP.
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Feedback/Feedback Library", fileName = "FeedbackLibrary")]
    public sealed class FeedbackLibrary : ScriptableObject
    {
        [SerializeField] private AudioClip[] _cues = new AudioClip[FeedbackCues.COUNT];
        [SerializeField] private float[] _cueVolumes = new float[FeedbackCues.COUNT];
        [Tooltip("Una pista por bloque de dificultad: fácil, media y difícil.")]
        [SerializeField] private AudioClip[] _music = new AudioClip[FeedbackRules.MUSIC_BLOCKS];
        [SerializeField, Range(0f, 1f)] private float _musicVolume = 0.22f;
        [SerializeField, Min(0.05f)] private float _musicFadeSeconds = 0.8f;

        public AudioClip Clip(FeedbackCue cue) => _cues[(int)cue];
        public float Volume(FeedbackCue cue) => _cueVolumes[(int)cue];
        public AudioClip Music(int block) => block >= 0 && block < _music.Length ? _music[block] : null;
        public int MusicCount => _music.Length;
        public float MusicVolume => _musicVolume;
        public float MusicFadeSeconds => _musicFadeSeconds;

#if UNITY_EDITOR
        public void EditorSet(AudioClip[] cues, float[] volumes, AudioClip[] music)
        {
            _cues = cues;
            _cueVolumes = volumes;
            _music = music;
        }
#endif
    }
}
