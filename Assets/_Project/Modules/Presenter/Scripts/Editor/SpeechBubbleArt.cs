using UnityEngine;
using Trivias.Shared.Editor;

namespace Trivias.Presenter.Editor
{
    /// <summary>
    /// Dibuja en edición las piezas de la viñeta de cómic: formas (redonda, estallido y nube) con contorno de tinta,
    /// la cola que apunta a la cabeza y los rayos de énfasis. Relleno blanco: el color lo pone cada ánimo.
    /// </summary>
    public static class SpeechBubbleArt
    {
        public const int WIDTH = 512;
        public const int HEIGHT = 200;
        private const int TAIL_SIZE = 96;
        private const int RAYS_WIDTH = 512;
        private const int RAYS_HEIGHT = 256;
        private const float INK_WIDTH = 9f;
        private const float MARGIN = 14f;
        private const string FOLDER = PresenterPrefabBuilder.MODULE_ROOT + "/UI/Sprites";
        private static readonly Color Ink = new(0.07f, 0.06f, 0.14f, 1f);

        public static Sprite Round() => Shape("BubbleRound", 2.8f, _ => 1f);

        /// <summary>Estallido con puntas, para las frases con ánimo.</summary>
        public static Sprite Burst() => Shape("BubbleBurst", 2f, angle => 0.86f + 0.14f * Zigzag(angle * 22f / (2f * Mathf.PI)));

        /// <summary>Nube con bultos, para el suspense.</summary>
        public static Sprite Cloud() => Shape("BubbleCloud", 2.4f, angle => 0.93f + 0.07f * Mathf.Abs(Mathf.Sin(angle * 9f)));

        /// <summary>Cola triangular que apunta a la izquierda (hacia la cabeza), con contorno en sus dos lados libres.</summary>
        public static Sprite Tail()
        {
            var tip = new Vector2(4f, 48f);
            var top = new Vector2(TAIL_SIZE, 86f);
            var bottom = new Vector2(TAIL_SIZE, 10f);
            var pixels = new Color[TAIL_SIZE * TAIL_SIZE];
            for (var y = 0; y < TAIL_SIZE; y++)
            {
                for (var x = 0; x < TAIL_SIZE; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    var toEdges = Mathf.Min(Segment(p, tip, top), Segment(p, tip, bottom));
                    var inside = Side(p, tip, top) * Side(p, tip, bottom) < 0f && p.x <= TAIL_SIZE;
                    var alpha = inside ? 1f : Mathf.Clamp01(0.5f - toEdges + 0.5f);
                    var ink = Mathf.Clamp01(INK_WIDTH + 0.5f - toEdges);
                    pixels[y * TAIL_SIZE + x] = Paint(ink, inside ? 1f : alpha * ink);
                }
            }
            return GeneratedSprites.GetOrCreate($"{FOLDER}/BubbleTail.png", TAIL_SIZE, TAIL_SIZE, pixels);
        }

        /// <summary>Rayos de énfasis alrededor de la viñeta (manga): solo en el anillo exterior.</summary>
        public static Sprite Rays()
        {
            var pixels = new Color[RAYS_WIDTH * RAYS_HEIGHT];
            for (var y = 0; y < RAYS_HEIGHT; y++)
            {
                for (var x = 0; x < RAYS_WIDTH; x++)
                {
                    var u = (x + 0.5f - RAYS_WIDTH * 0.5f) / (RAYS_WIDTH * 0.5f);
                    var v = (y + 0.5f - RAYS_HEIGHT * 0.5f) / (RAYS_HEIGHT * 0.5f);
                    var radius = new Vector2(u, v).magnitude;
                    var angle = Mathf.Atan2(v, u);
                    var ray = Mathf.Clamp01((Zigzag(angle * 30f / (2f * Mathf.PI)) - 0.55f) * 6f);
                    var ring = Mathf.Clamp01((radius - 0.62f) / 0.12f) * Mathf.Clamp01((1f - radius) / 0.2f);
                    pixels[y * RAYS_WIDTH + x] = new Color(1f, 1f, 1f, ray * ring);
                }
            }
            return GeneratedSprites.GetOrCreate($"{FOLDER}/BubbleRays.png", RAYS_WIDTH, RAYS_HEIGHT, pixels);
        }

        // ---- Dibujo ----

        /// <summary>Forma cerrada: superelipse de exponente <paramref name="roundness"/> deformada por <paramref name="edge"/>(ángulo).</summary>
        private static Sprite Shape(string name, float roundness, System.Func<float, float> edge)
        {
            var a = WIDTH * 0.5f - MARGIN;
            var b = HEIGHT * 0.5f - MARGIN;
            float Field(float px, float py)
            {
                var u = (px - WIDTH * 0.5f) / a;
                var v = (py - HEIGHT * 0.5f) / b;
                var rho = Mathf.Pow(Mathf.Pow(Mathf.Abs(u), roundness) + Mathf.Pow(Mathf.Abs(v), roundness), 1f / roundness);
                return rho - edge(Mathf.Atan2(v, u));
            }

            var pixels = new Color[WIDTH * HEIGHT];
            for (var y = 0; y < HEIGHT; y++)
            {
                for (var x = 0; x < WIDTH; x++)
                {
                    var px = x + 0.5f;
                    var py = y + 0.5f;
                    var f = Field(px, py);
                    var gx = Field(px + 0.5f, py) - Field(px - 0.5f, py);
                    var gy = Field(px, py + 0.5f) - Field(px, py - 0.5f);
                    var distance = f / Mathf.Max(1e-4f, new Vector2(gx, gy).magnitude); // En píxeles (+ fuera).
                    var alpha = Mathf.Clamp01(0.5f - distance);
                    var ink = Mathf.Clamp01(distance + INK_WIDTH + 0.5f);
                    pixels[y * WIDTH + x] = Paint(ink, alpha);
                }
            }
            return GeneratedSprites.GetOrCreate($"{FOLDER}/{name}.png", WIDTH, HEIGHT, pixels);
        }

        private static Color Paint(float ink, float alpha)
        {
            var color = Color.Lerp(Color.white, Ink, ink);
            color.a = alpha;
            return color;
        }

        /// <summary>Diente de sierra simétrico: 0 en los enteros, 1 en los medios.</summary>
        private static float Zigzag(float x)
        {
            var fraction = x - Mathf.Floor(x);
            return 1f - Mathf.Abs(2f * fraction - 1f);
        }

        private static float Segment(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            var t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return (p - (a + ab * t)).magnitude;
        }

        private static float Side(Vector2 p, Vector2 a, Vector2 b) => (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
    }
}
