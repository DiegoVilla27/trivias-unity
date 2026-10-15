using System.Collections.Generic;
using Trivias.Shared.Interaction;
using UnityEngine;

namespace Trivias.Interaction.Logic
{
    /// <summary>Elige el interactivo más cercano dentro de su radio (en el plano horizontal). Sin estado, sin allocations.</summary>
    public static class InteractionSelector
    {
        public static IInteractable FindClosest(IReadOnlyList<IInteractable> candidates, Vector3 origin)
        {
            IInteractable best = null;
            var bestSqrDistance = float.MaxValue;

            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                var offset = candidate.Position - origin;
                offset.y = 0f;
                var sqrDistance = offset.sqrMagnitude;
                var radius = candidate.Radius;

                if (sqrDistance > radius * radius || sqrDistance >= bestSqrDistance)
                    continue;

                best = candidate;
                bestSqrDistance = sqrDistance;
            }

            return best;
        }
    }
}
