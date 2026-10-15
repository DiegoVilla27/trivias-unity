using System;
using Trivias.Feedback.Logic;
using UnityEngine;

namespace Trivias.Feedback.Data
{
    [Serializable]
    public struct MoodLight
    {
        [Tooltip("Color de los focos de colores del plató.")]
        public Color SpotColor;
        [Min(0f)] public float SpotIntensity;
        [Tooltip("Tinte de la luz ambiente (se multiplica por la de la escena).")]
        public Color AmbientTint;
        [Tooltip("Multiplicador de la luz principal (1 = la de la escena).")]
        [Min(0f)] public float KeyLightScale;

        public MoodLight(Color spotColor, float spotIntensity, Color ambientTint, float keyLightScale)
        {
            SpotColor = spotColor;
            SpotIntensity = spotIntensity;
            AmbientTint = ambientTint;
            KeyLightScale = keyLightScale;
        }
    }

    /// <summary>Luz de cada ambiente del plató (ajustable sin tocar código).</summary>
    [CreateAssetMenu(menuName = "Trivias/Feedback/Stage Mood Config", fileName = "StageMoodConfig")]
    public sealed class StageMoodConfig : ScriptableObject
    {
        [SerializeField] private MoodLight[] _moods = new MoodLight[StageMoods.COUNT];
        [Tooltip("Rapidez del fundido entre ambientes (más alto = más rápido).")]
        [SerializeField, Min(0.1f)] private float _fadeSpeed = 2.5f;
        [Tooltip("Cuánto sube la intensidad de los focos en cada latido de la cuenta atrás.")]
        [SerializeField, Range(0f, 2f)] private float _countdownPulse = 0.8f;

        public MoodLight For(StageMood mood) => _moods[(int)mood];
        public int Count => _moods.Length;
        public float FadeSpeed => _fadeSpeed;
        public float CountdownPulse => _countdownPulse;

#if UNITY_EDITOR
        public void EditorSet(MoodLight[] moods) => _moods = moods;
#endif
    }
}
