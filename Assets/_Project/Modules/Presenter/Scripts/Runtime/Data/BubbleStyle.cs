using System;
using UnityEngine;

namespace Trivias.Presenter.Data
{
    /// <summary>Aspecto de la viñeta para un ánimo (los rellena el constructor; ajustables en el prefab).</summary>
    [Serializable]
    public struct BubbleStyle
    {
        [Tooltip("Forma de la viñeta (con su contorno).")]
        public Sprite Shape;
        public Color Fill;
        public Color Text;
        [Tooltip("Rayos de énfasis detrás de la viñeta (estallido de ánimo).")]
        public bool Rays;
        public Color RaysColor;
    }
}
