using System.IO;
using System.Linq;
using TMPro;
using Trivias.Shared.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Shared.Editor
{
    /// <summary>
    /// Tema de la interfaz, el mismo estilo que el plató (azul noche, oro y bombillas). Se aplica solo al guardar
    /// cualquier prefab con Canvas (<see cref="EditorAssets.SaveAsPrefab"/>), así ninguna pantalla se queda sin él:
    /// <list type="bullet">
    /// <item>Tipografías: Lilita One en títulos (con degradado dorado) y Poppins en el resto; los textos se ajustan de
    /// tamaño si no caben.</item>
    /// <item>Paneles redondeados con un leve degradado y marco dorado; aparecen con un pequeño rebote.</item>
    /// <item>Botones redondeados que crecen al pasar el ratón y se hunden al pulsar; los secundarios, con borde dorado.</item>
    /// </list>
    /// </summary>
    public static class UiTheme
    {
        private const string SPRITES = "Assets/_Project/Shared/UI/Sprites";
        private const int SPRITE_SIZE = 64;
        private const int SPRITE_RADIUS = 22;
        private const int SPRITE_BORDER = 24;

        public static readonly Color Gold = new(0.98f, 0.76f, 0.26f);
        public static readonly Color GoldDeep = new(0.86f, 0.52f, 0.12f);
        private static readonly Color FrameGold = new(0.98f, 0.78f, 0.32f, 0.95f);

        public const string FRAME = "ThemeFrame";

        public static Sprite RoundedFill() => GetOrCreateSprite("RoundedFill", Rounded(gradient: true, ring: false), SPRITE_BORDER);
        public static Sprite RoundedFrame() => GetOrCreateSprite("RoundedFrame", Rounded(gradient: false, ring: true), SPRITE_BORDER);
        public static Sprite Bulb() => GetOrCreateSprite("Bulb", Glow(), 0);

        /// <summary>Aplica el tema a toda la interfaz que cuelga de <paramref name="root"/>.</summary>
        public static void Apply(GameObject root)
        {
            foreach (var text in root.GetComponentsInChildren<TextMeshProUGUI>(true))
                StyleText(text);

            foreach (var image in root.GetComponentsInChildren<Image>(true).ToList())
            {
                if (image.sprite != null || image.name == FRAME || IsFullScreen(image.rectTransform))
                    continue;
                var selectable = image.GetComponent<Selectable>();
                if (selectable != null && selectable.targetGraphic == image && selectable is Button button)
                    StyleButton(button, image);
                else if (IsPanel(image))
                    StylePanel(image, IsTopLevelPanel(image));
                else
                    Round(image, 10f);
            }
        }

        /// <summary>Marquesina de bombillas que recorre el borde de un panel (menú principal, resultados).</summary>
        public static UiMarquee AddMarquee(RectTransform panel, float spacing = 38f, float bulbSize = 15f)
        {
            var holder = new GameObject("Marquee", typeof(RectTransform));
            holder.transform.SetParent(panel, false);
            var rect = (RectTransform)holder.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var size = panel.sizeDelta;
            var inset = 13f;
            var width = size.x - inset * 2f;
            var height = size.y - inset * 2f;
            var perimeter = 2f * (width + height);
            var count = Mathf.Max(8, Mathf.RoundToInt(perimeter / spacing));
            var bulbs = new Image[count];
            for (var i = 0; i < count; i++)
            {
                var d = i * perimeter / count;
                Vector2 p;
                if (d < width) p = new Vector2(d, 0f);
                else if ((d -= width) < height) p = new Vector2(width, d);
                else if ((d -= height) < width) p = new Vector2(width - d, height);
                else p = new Vector2(0f, height - (d - width));

                var bulb = new GameObject($"Bulb{i + 1}", typeof(RectTransform));
                bulb.transform.SetParent(rect, false);
                var bulbRect = (RectTransform)bulb.transform;
                bulbRect.anchorMin = bulbRect.anchorMax = Vector2.zero;
                bulbRect.sizeDelta = Vector2.one * bulbSize;
                bulbRect.anchoredPosition = new Vector2(inset, inset) + p;
                var image = bulb.AddComponent<Image>();
                image.sprite = Bulb();
                image.color = Gold;
                image.raycastTarget = false;
                bulbs[i] = image;
            }

            var marquee = holder.AddComponent<UiMarquee>();
            SerializedWiring.SetArray(marquee, "_bulbs", bulbs);
            return marquee;
        }

        // ---- Reglas ----

        private static void StyleText(TextMeshProUGUI text)
        {
            var title = text.name.Contains("Title") || text.fontSize >= 44f;
            text.font = title ? StageFonts.Display() : StageFonts.Body();
            if (title)
            {
                text.fontStyle &= ~FontStyles.Bold; // Lilita One ya es gruesa: sin negrita falsa.
                text.characterSpacing = 2f;
                if (IsGoldish(text.color))
                {
                    // Degradado dorado como el rótulo del plató (colores de vértice: no crea materiales).
                    text.enableVertexGradient = true;
                    text.colorGradient = new VertexGradient(new Color(1f, 0.92f, 0.55f), new Color(1f, 0.92f, 0.55f), GoldDeep, GoldDeep);
                    text.color = Color.white;
                }
            }

            // Si una traducción o un nombre largo no cabe, encoge en vez de salirse.
            text.enableAutoSizing = true;
            text.fontSizeMax = text.fontSize;
            text.fontSizeMin = Mathf.Max(12f, text.fontSize * 0.6f);
        }

        private static void StyleButton(Button button, Image image)
        {
            var size = image.rectTransform.sizeDelta;
            Round(image, Mathf.Min(18f, size.y * 0.5f));
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.97f, 0.88f);
            colors.selectedColor = Color.white;
            colors.pressedColor = new Color(0.86f, 0.8f, 0.7f);
            colors.disabledColor = new Color(0.6f, 0.6f, 0.65f, 0.55f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            // Botones secundarios (fondo translúcido): borde dorado fino para que se lean como botones.
            if (image.color.a < 0.5f)
                AddFrame(image.rectTransform, new Color(FrameGold.r, FrameGold.g, FrameGold.b, 0.7f), Mathf.Min(18f, size.y * 0.5f));
            if (button.GetComponent<UiButtonFeel>() == null)
                button.gameObject.AddComponent<UiButtonFeel>();
        }

        private static void StylePanel(Image image, bool topLevel)
        {
            Round(image, topLevel ? 26f : 16f);
            if (!topLevel)
                return;
            AddFrame(image.rectTransform, FrameGold, 26f);
            if (image.GetComponent<UiPanelIntro>() == null)
                image.gameObject.AddComponent<UiPanelIntro>();
        }

        private static void Round(Image image, float radius)
        {
            image.sprite = RoundedFill();
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = SPRITE_BORDER / Mathf.Max(2f, radius);
        }

        private static void AddFrame(RectTransform target, Color color, float radius)
        {
            if (target.Find(FRAME) != null)
                return;
            var frame = new GameObject(FRAME, typeof(RectTransform));
            frame.transform.SetParent(target, false);
            frame.transform.SetAsFirstSibling(); // Detrás del contenido, encima del fondo.
            var rect = (RectTransform)frame.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = frame.AddComponent<Image>();
            image.sprite = RoundedFrame();
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = SPRITE_BORDER / Mathf.Max(2f, radius);
            image.color = color;
            image.raycastTarget = false;
        }

        private static bool IsPanel(Image image)
        {
            var size = image.rectTransform.rect.size;
            if (size == Vector2.zero)
                size = image.rectTransform.sizeDelta;
            return image.transform.childCount > 0 && size.x >= 160f && size.y >= 60f && image.GetComponent<Selectable>() == null
                   && image.GetComponentInParent<Selectable>() == null;
        }

        private static bool IsTopLevelPanel(Image image)
        {
            for (var parent = image.transform.parent; parent != null; parent = parent.parent)
            {
                var other = parent.GetComponent<Image>();
                if (other != null && !IsFullScreen(other.rectTransform) && IsPanel(other))
                    return false;
            }
            return true;
        }

        private static bool IsFullScreen(RectTransform rect) => rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.one &&
                                                                rect.parent != null && rect.parent.GetComponent<Canvas>() != null;

        private static bool IsGoldish(Color color) => color.r > 0.8f && color.g > 0.55f && color.g < 0.9f && color.b < 0.45f;

        // ---- Imágenes del tema (se generan una vez) ----

        private static Color[] Rounded(bool gradient, bool ring)
        {
            var pixels = new Color[SPRITE_SIZE * SPRITE_SIZE];
            for (var y = 0; y < SPRITE_SIZE; y++)
            {
                for (var x = 0; x < SPRITE_SIZE; x++)
                {
                    var inside = Coverage(x, y, 0f);
                    var alpha = ring ? inside * (1f - Coverage(x, y, 3f)) : inside;
                    var light = gradient ? Mathf.Lerp(0.82f, 1f, (float)y / (SPRITE_SIZE - 1)) : 1f;
                    pixels[y * SPRITE_SIZE + x] = new Color(light, light, light, alpha);
                }
            }
            return pixels;
        }

        /// <summary>Cuánto cubre el rectángulo redondeado (encogido <paramref name="inset"/> píxeles) este píxel (suavizado).</summary>
        private static float Coverage(int x, int y, float inset)
        {
            var radius = SPRITE_RADIUS - inset;
            var half = SPRITE_SIZE * 0.5f - inset;
            var px = Mathf.Abs(x + 0.5f - SPRITE_SIZE * 0.5f) - (half - radius);
            var py = Mathf.Abs(y + 0.5f - SPRITE_SIZE * 0.5f) - (half - radius);
            var outside = new Vector2(Mathf.Max(px, 0f), Mathf.Max(py, 0f)).magnitude + Mathf.Min(Mathf.Max(px, py), 0f) - radius;
            return Mathf.Clamp01(0.5f - outside);
        }

        private static Color[] Glow()
        {
            var pixels = new Color[SPRITE_SIZE * SPRITE_SIZE];
            for (var y = 0; y < SPRITE_SIZE; y++)
            {
                for (var x = 0; x < SPRITE_SIZE; x++)
                {
                    var d = new Vector2(x + 0.5f - SPRITE_SIZE * 0.5f, y + 0.5f - SPRITE_SIZE * 0.5f).magnitude / (SPRITE_SIZE * 0.5f);
                    var core = Mathf.Clamp01((0.55f - d) * 8f);
                    var halo = Mathf.Clamp01(1f - d) * 0.45f;
                    pixels[y * SPRITE_SIZE + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(core + halo * halo));
                }
            }
            return pixels;
        }

        private static Sprite GetOrCreateSprite(string name, Color[] pixels, int border)
        {
            var path = $"{SPRITES}/{name}.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
                return sprite;

            EditorAssets.EnsureFolder(SPRITES);
            var texture = new Texture2D(SPRITE_SIZE, SPRITE_SIZE, TextureFormat.RGBA32, false);
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = new Vector4(border, border, border, border);
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
