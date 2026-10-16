using System.Collections.Generic;
using UnityEngine;

namespace Trivias.Shared.Tubes
{
    /// <summary>
    /// Tubos presentes en el plató. Cada tubo se registra al activarse; el dinero y la expulsión los buscan aquí sin
    /// conocer el módulo de las cabinas.
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Stage/Tube Registry", fileName = "TubeRegistry")]
    public sealed class TubeRegistry : ScriptableObject
    {
        private readonly List<TubePath> _tubes = new();

        public int Count => _tubes.Count;

        public void Register(TubePath tube)
        {
            if (!_tubes.Contains(tube))
                _tubes.Add(tube);
        }

        public void Unregister(TubePath tube) => _tubes.Remove(tube);

        /// <summary>Tubo de un tipo de una cabina (0..3 = A..D).</summary>
        public bool TryGet(TubeKind kind, int booth, out TubePath tube)
        {
            foreach (var candidate in _tubes)
            {
                if (candidate.Kind == kind && candidate.Booth == booth)
                {
                    tube = candidate;
                    return true;
                }
            }
            tube = null;
            return false;
        }

        /// <summary>El tubo de ese tipo cuya entrada está más cerca en horizontal, si no está más lejos que el máximo.</summary>
        public bool TryNearest(TubeKind kind, Vector3 position, float maxDistance, out TubePath tube)
        {
            tube = null;
            var best = maxDistance * maxDistance;
            foreach (var candidate in _tubes)
            {
                if (candidate.Kind != kind)
                    continue;
                var offset = candidate.Inlet - position;
                offset.y = 0f;
                var distance = offset.sqrMagnitude;
                if (distance <= best)
                {
                    best = distance;
                    tube = candidate;
                }
            }
            return tube != null;
        }

        private void OnEnable() => _tubes.Clear();
    }
}
