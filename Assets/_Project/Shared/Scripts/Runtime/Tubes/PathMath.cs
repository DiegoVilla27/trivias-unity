using UnityEngine;

namespace Trivias.Shared.Tubes
{
    /// <summary>
    /// Recorridos por tubos: una polilínea densa (los codos ya vienen muestreados en arco) recorrida por distancia, y el
    /// movimiento de succión (arranca despacio y acelera). Funciones puras, sin estado, probadas en EditMode.
    /// </summary>
    public static class PathMath
    {
        /// <summary>Longitud acumulada hasta cada punto (la primera es 0). Se calcula una vez al cargar.</summary>
        public static float[] CumulativeLengths(Vector3[] points)
        {
            var lengths = new float[points.Length];
            for (var i = 1; i < points.Length; i++)
                lengths[i] = lengths[i - 1] + Vector3.Distance(points[i - 1], points[i]);
            return lengths;
        }

        /// <summary>Punto y dirección a <paramref name="distance"/> metros del inicio (se queda en los extremos).</summary>
        public static void Sample(Vector3[] points, float[] lengths, float distance, out Vector3 position, out Vector3 direction)
        {
            var last = points.Length - 1;
            if (last <= 0)
            {
                position = last == 0 ? points[0] : Vector3.zero;
                direction = Vector3.up;
                return;
            }

            if (distance <= 0f)
            {
                position = points[0];
                direction = (points[1] - points[0]).normalized;
                return;
            }
            if (distance >= lengths[last])
            {
                position = points[last];
                direction = (points[last] - points[last - 1]).normalized;
                return;
            }

            // Búsqueda binaria del tramo: pocos puntos, sin memoria nueva.
            int low = 0, high = last;
            while (high - low > 1)
            {
                var middle = (low + high) / 2;
                if (lengths[middle] <= distance)
                    low = middle;
                else
                    high = middle;
            }

            var span = lengths[high] - lengths[low];
            var t = span > 0f ? (distance - lengths[low]) / span : 0f;
            position = Vector3.LerpUnclamped(points[low], points[high], t);
            direction = (points[high] - points[low]).normalized;
        }

        /// <summary>
        /// Distancia recorrida por la succión a los <paramref name="seconds"/> segundos: velocidad inicial y aceleración
        /// constante (como si el aire tirase cada vez más fuerte).
        /// </summary>
        public static float SuctionDistance(float seconds, float startSpeed, float acceleration) =>
            seconds <= 0f ? 0f : startSpeed * seconds + 0.5f * acceleration * seconds * seconds;

        /// <summary>Tiempo que tarda la succión en recorrer <paramref name="distance"/> metros.</summary>
        public static float SuctionTime(float distance, float startSpeed, float acceleration)
        {
            if (distance <= 0f)
                return 0f;
            if (acceleration <= 0f)
                return startSpeed > 0f ? distance / startSpeed : float.PositiveInfinity;
            return (-startSpeed + Mathf.Sqrt(startSpeed * startSpeed + 2f * acceleration * distance)) / acceleration;
        }

        /// <summary>Punto de una curva de Bézier cuadrática (para ir de la bandeja a la boca del tubo sin esquinas).</summary>
        public static Vector3 Bezier(Vector3 start, Vector3 control, Vector3 end, float t)
        {
            var u = 1f - t;
            return u * u * start + 2f * u * t * control + t * t * end;
        }

        /// <summary>Arranque suave (0..1 → 0..1), para despegar del suelo o de la bandeja.</summary>
        public static float EaseIn(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * t;
        }
    }
}
