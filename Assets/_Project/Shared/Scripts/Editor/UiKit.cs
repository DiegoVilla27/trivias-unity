using TMPro;
using Trivias.Shared.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Shared.Editor
{
    /// <summary>
    /// Piezas de la interfaz de los menús, fieles a los bocetos de <c>docs/bocetos/ui/menu</c>: paneles azul noche con
    /// marco dorado, botones (dorado, azul con borde, discreto, burdeos y de solo texto), campos, pastillas, tarjetas,
    /// filas, pestañas, deslizadores e interruptores. Las formas se dibujan una vez como PNG con sus colores
    /// (<c>Shared/UI/Kit</c>) y se estiran sin deformarse (9-slice). Las medidas se dan como en los bocetos: x, y desde
    /// la esquina superior izquierda del padre, a 1920 × 1080.
    /// </summary>
    public static class UiKit
    {
        private const string KIT = "Assets/_Project/Shared/UI/Kit";
        private const string ICONS = "Assets/_Project/Shared/UI/Icons";

        // ---- Paleta (DISENO.md) ----
        public static readonly Color Ivory = Hex("#F5EFE1");
        public static readonly Color Muted = Hex("#9DA6C0");
        public static readonly Color Gold = Hex("#F2C55C");
        public static readonly Color DarkText = Hex("#101830");
        public static readonly Color Green = Hex("#7BD86A");
        public static readonly Color Danger = Hex("#FF8A8A");

        public enum ButtonStyle
        {
            /// <summary>Dorado: la acción principal de la pantalla.</summary>
            Primary,
            /// <summary>Azul noche con borde dorado.</summary>
            Secondary,
            /// <summary>Azul discreto con borde azul.</summary>
            Quiet,
            /// <summary>Burdeos: salir.</summary>
            Danger,
            /// <summary>Solo texto apagado (p. ej. «Salir» del menú principal).</summary>
            Text,
        }

        public enum UiFont
        {
            /// <summary>Lilita One: títulos y cabeceras.</summary>
            Display,
            /// <summary>Poppins SemiBold.</summary>
            Body,
            /// <summary>Poppins ExtraBold.</summary>
            Bold,
        }

        // ---- Formas ----

        public static Sprite PanelSprite() => Shape("Panel", 30, 3, Hex("#1F2C4C"), Hex("#121A30"), Hex("#E2B868"));
        public static Sprite SubPanelSprite() => Shape("SubPanel", 22, 2, Hex("#18233F"), Hex("#121B31"), Hex("#33466F"));
        public static Sprite CardSprite() => Shape("Card", 18, 2, Hex("#1D2A4B"), Hex("#131C34"), Hex("#34497A"));
        public static Sprite CardSelectedSprite() => Shape("CardSelected", 18, 4, Hex("#22305A"), Hex("#161F3A"), Hex("#F2C55C"));
        public static Sprite RowSprite() => Shape("Row", 16, 2, Hex("#18233F"), Hex("#131C33"), Hex("#30436E"));
        public static Sprite HeaderSprite() => Shape("Header", 16, 2, Hex("#1E2C50"), Hex("#18233F"), Hex("#4660A0"));
        public static Sprite InputSprite() => Shape("Input", 18, 2, Hex("#0E1632"), Hex("#0C1329"), Hex("#6B84CC"));
        public static Sprite PrimarySprite() => Shape("ButtonPrimary", 20, 3, Hex("#FFE597"), Hex("#DDA33C"), Hex("#0B1226"), highlight: true);
        public static Sprite SecondarySprite() => Shape("ButtonSecondary", 18, 3, Hex("#16203A"), Hex("#101830"), Hex("#E0B868"));
        public static Sprite QuietSprite() => Shape("ButtonQuiet", 18, 2, Hex("#1A2542"), Hex("#151F38"), Hex("#2E3F66"));
        public static Sprite DangerSprite() => Shape("ButtonDanger", 18, 3, Hex("#5A1A2C"), Hex("#38101D"), Hex("#E46E88"));
        public static Sprite PillSprite() => Shape("Pill", 30, 2, Hex("#3A3322"), Hex("#2B261A"), Hex("#C9A24E"));
        public static Sprite PillGreenSprite() => Shape("PillGreen", 30, 2, Hex("#26421F"), Hex("#1A3016"), Hex("#6FB04F"));
        public static Sprite PillGoldSprite() => Shape("PillGold", 30, 2, Hex("#FFE597"), Hex("#DDA33C"), Hex("#8A6420"), highlight: true);
        public static Sprite PlateSprite() => Shape("Plate", 18, 3, Hex("#16203A"), Hex("#0F172C"), Hex("#E0B868"));
        public static Sprite TrackSprite() => Shape("Track", 30, 2, Hex("#0E1630"), Hex("#0E1630"), Hex("#3A4B78"));
        public static Sprite TrackFillSprite() => Shape("TrackFill", 30, 0, Hex("#FFE08A"), Hex("#E2A63D"), Color.clear);
        public static Sprite KnobSprite() => Shape("Knob", 62, 4, Hex("#FFE7A0"), Hex("#E0A33A"), Hex("#8A5A12"), highlight: true);
        public static Sprite ToggleKnobSprite() => Shape("ToggleKnob", 62, 3, Hex("#FFFAF0"), Hex("#E2DCCB"), Hex("#BDB4A0"));
        public static Sprite CircleGoldSprite() => Shape("CircleGold", 62, 3, Hex("#FFE597"), Hex("#DDA33C"), Hex("#8A6420"), highlight: true);
        public static Sprite CircleNavySprite() => Shape("CircleNavy", 62, 3, Hex("#1F2C4C"), Hex("#151F38"), Hex("#6F86C2"));
        public static Sprite DotSprite() => Shape("Dot", 62, 0, Color.white, Color.white, Color.clear);
        public static Sprite LineSprite() => Shape("Line", 4, 0, Color.white, Color.white, Color.clear);

        /// <summary>Halo de foco (blanco que se desvanece), para la luz detrás del personaje.</summary>
        public static Sprite GlowSprite()
        {
            const int size = 256;
            var pixels = new Color[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = (x + 0.5f) / size * 2f - 1f;
                    var dy = (y + 0.5f) / size * 2f - 1f;
                    var d = Mathf.Sqrt(dx * dx + dy * dy);
                    var a = Mathf.Clamp01(1f - d);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, a * a * (3f - 2f * a));
                }
            }
            return GeneratedSprites.GetOrCreate($"{KIT}/Glow.png", size, size, pixels);
        }

        // ---- Imágenes del proyecto ----

        public static Sprite Icon(string name) => ArtSprite($"{ICONS}/Icon_{name}.png");

        /// <summary>Un PNG del proyecto como sprite de interfaz (sin mipmaps, con transparencia).</summary>
        public static Sprite ArtSprite(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer != null && (importer.textureType != TextureImporterType.Sprite || importer.mipmapEnabled))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // ---- Colocación ----

        /// <summary>Rectángulo con su esquina superior izquierda en (x, y) dentro del padre.</summary>
        public static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        /// <summary>Capa a pantalla completa (sin imagen).</summary>
        public static RectTransform FullScreen(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        /// <summary>Lienzo de 1920 × 1080 centrado en pantalla: todo se coloca con las medidas de los bocetos.</summary>
        public static RectTransform Screen(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(1920f, 1080f);
            return rect;
        }

        public static Image Picture(string name, Transform parent, Sprite sprite, float x, float y, float width, float height,
            bool preserveAspect = false)
        {
            var image = Rect(name, parent, x, y, width, height).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = sprite != null && sprite.border != Vector4.zero ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
            image.preserveAspect = preserveAspect;
            if (preserveAspect)
            {
                // Una imagen que guarda su proporción se alinea por su pivote: en el centro, queda centrada en su hueco.
                var rect = image.rectTransform;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition += new Vector2(width * 0.5f, -height * 0.5f);
            }
            FitCorners(image);
            image.raycastTarget = false;
            return image;
        }

        /// <summary>
        /// Las esquinas redondeadas nunca más grandes que media pieza: en una pieza baja (pista de un deslizador, pastilla)
        /// quedan como extremos de píldora en vez de cortarse.
        /// </summary>
        public static void FitCorners(Image image)
        {
            if (image.sprite == null || image.type != UnityEngine.UI.Image.Type.Sliced)
                return;
            var border = Mathf.Max(image.sprite.border.x, image.sprite.border.y, image.sprite.border.z, image.sprite.border.w);
            var size = image.rectTransform.sizeDelta;
            var smallest = Mathf.Min(size.x, size.y);
            image.pixelsPerUnitMultiplier = smallest > 0f ? Mathf.Max(1f, 2f * border / smallest) : 1f;
        }

        /// <summary>Panel principal: azul noche con marco dorado. Bloquea los clics a lo que hay detrás.</summary>
        public static RectTransform Panel(string name, Transform parent, float x, float y, float width, float height)
        {
            var image = Picture(name, parent, PanelSprite(), x, y, width, height);
            image.raycastTarget = true;
            image.gameObject.AddComponent<UiPanelIntro>(); // Aparece con un pequeño rebote.
            return image.rectTransform;
        }

        public static RectTransform SubPanel(string name, Transform parent, float x, float y, float width, float height) =>
            Picture(name, parent, SubPanelSprite(), x, y, width, height).rectTransform;

        // ---- Texto ----

        public static TextMeshProUGUI Text(string name, Transform parent, string text, float x, float y, float width, float height,
            float size, UiFont font, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var label = Rect(name, parent, x, y, width, height).gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.font = font == UiFont.Display ? StageFonts.Display() : StageFonts.Body();
            label.fontSize = size;
            label.fontStyle = font == UiFont.Bold ? FontStyles.Bold : FontStyles.Normal;
            label.color = color;
            label.alignment = alignment;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.richText = true;
            label.raycastTarget = false;
            // Si un nombre o una traducción no cabe, encoge en vez de salirse.
            label.enableAutoSizing = true;
            label.fontSizeMax = size;
            label.fontSizeMin = Mathf.Max(12f, size * 0.6f);
            if (font == UiFont.Display)
                label.characterSpacing = 1f;
            return label;
        }

        // ---- Botones ----

        public static Button MakeButton(string name, Transform parent, string caption, float x, float y, float width, float height,
            ButtonStyle style, out TextMeshProUGUI label, float fontSize = 30f)
        {
            var rect = Rect(name, parent, x, y, width, height);
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = style switch
            {
                ButtonStyle.Primary => PrimarySprite(),
                ButtonStyle.Secondary => SecondarySprite(),
                ButtonStyle.Quiet => QuietSprite(),
                ButtonStyle.Danger => DangerSprite(),
                _ => DotSprite(),
            };
            image.type = UnityEngine.UI.Image.Type.Sliced;
            FitCorners(image);
            if (style == ButtonStyle.Text)
                image.color = new Color(1f, 1f, 1f, 0f); // Invisible pero pulsable.
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = style == ButtonStyle.Text ? new Color(1f, 1f, 1f, 0.06f) : new Color(1f, 0.97f, 0.88f);
            colors.normalColor = style == ButtonStyle.Text ? new Color(1f, 1f, 1f, 0f) : Color.white;
            colors.selectedColor = colors.normalColor;
            colors.pressedColor = new Color(0.86f, 0.8f, 0.7f, style == ButtonStyle.Text ? 0.1f : 1f);
            colors.disabledColor = new Color(0.55f, 0.58f, 0.68f, style == ButtonStyle.Text ? 0f : 0.6f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            rect.gameObject.AddComponent<UiButtonFeel>();

            var color = style switch
            {
                ButtonStyle.Primary => DarkText,
                ButtonStyle.Text => Muted,
                _ => Ivory,
            };
            label = Text("Label", rect, caption, 12f, 4f, width - 24f, height - 10f, fontSize,
                style == ButtonStyle.Primary || style == ButtonStyle.Secondary || style == ButtonStyle.Danger ? UiFont.Bold : UiFont.Body, color);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            return button;
        }

        public static Button MakeButton(string name, Transform parent, string caption, float x, float y, float width, float height,
            ButtonStyle style, float fontSize = 30f) => MakeButton(name, parent, caption, x, y, width, height, style, out _, fontSize);

        /// <summary>Botón redondo de flecha (imagen de icono).</summary>
        public static Button IconButton(string name, Transform parent, Sprite icon, float x, float y, float size)
        {
            var image = Picture(name, parent, icon, x, y, size, size, preserveAspect: true);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.95f, 0.8f);
            colors.disabledColor = new Color(1f, 1f, 1f, 0.35f);
            button.colors = colors;
            image.gameObject.AddComponent<UiButtonFeel>();
            return button;
        }

        // ---- Campos y etiquetas ----

        public static TMP_InputField InputField(string name, Transform parent, float x, float y, float width, float height,
            string placeholder, int characterLimit, float fontSize = 32f)
        {
            var image = Picture(name, parent, InputSprite(), x, y, width, height);
            image.raycastTarget = true;
            var area = Rect("TextArea", image.transform, 24f, 0f, width - 48f, height);
            area.gameObject.AddComponent<RectMask2D>();
            var hint = Text("Placeholder", area, placeholder, 0f, 0f, width - 48f, height, fontSize, UiFont.Body,
                new Color(Muted.r, Muted.g, Muted.b, 0.6f), TextAlignmentOptions.MidlineLeft);
            hint.textWrappingMode = TextWrappingModes.NoWrap;
            var text = Text("Text", area, string.Empty, 0f, 0f, width - 48f, height, fontSize, UiFont.Bold, Ivory,
                TextAlignmentOptions.MidlineLeft);
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.richText = false;

            var field = image.gameObject.AddComponent<TMP_InputField>();
            field.targetGraphic = image;
            field.textViewport = area;
            field.textComponent = text;
            field.placeholder = hint;
            field.characterLimit = characterLimit;
            field.caretColor = Ivory;
            field.customCaretColor = true;
            field.selectionColor = new Color(0.42f, 0.52f, 0.8f, 0.5f);
            field.fontAsset = text.font;
            field.pointSize = fontSize;
            return field;
        }

        /// <summary>Pastilla con icono opcional y texto (estado: «Gratis · Elegido», precio…).</summary>
        public static Image Pill(string name, Transform parent, Sprite background, Sprite icon, string caption, float x, float y,
            float width, float height, Color textColor, out TextMeshProUGUI label, out Image iconImage, float fontSize = 24f)
        {
            var pill = Picture(name, parent, background, x, y, width, height);
            var iconSize = height - 14f;
            iconImage = icon != null ? Picture("Icon", pill.transform, icon, 12f, 7f, iconSize, iconSize, preserveAspect: true) : null;
            var left = icon != null ? 18f + iconSize : 12f;
            label = Text("Label", pill.transform, caption, left, 2f, width - left - 12f, height - 4f, fontSize, UiFont.Body, textColor);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            return pill;
        }

        /// <summary>Línea divisoria fina.</summary>
        public static Image Divider(string name, Transform parent, float x, float y, float width) =>
            Tinted(Picture(name, parent, LineSprite(), x, y, width, 2f), new Color(0.2f, 0.27f, 0.42f, 1f));

        public static Image Tinted(Image image, Color color)
        {
            image.color = color;
            return image;
        }

        // ---- Controles ----

        /// <summary>Deslizador: pista azul, relleno dorado y mando dorado.</summary>
        public static Slider MakeSlider(string name, Transform parent, float x, float y, float width, float min, float max)
        {
            const float height = 38f;
            var root = Rect(name, parent, x, y, width, height);
            var track = Picture("Track", root, TrackSprite(), 0f, 6f, width, 26f);
            track.raycastTarget = true;
            var fillArea = Rect("FillArea", root, 2f, 8f, width - 4f, 22f);
            var fill = Picture("Fill", fillArea, TrackFillSprite(), 0f, 0f, width - 4f, 22f);
            var fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = fillRect.offsetMax = Vector2.zero;
            fill.pixelsPerUnitMultiplier = Mathf.Max(1f, 2f * fill.sprite.border.x / 22f);

            var handleArea = Rect("HandleArea", root, height * 0.5f, 0f, width - height, height);
            var handle = Picture("Handle", handleArea, KnobSprite(), 0f, 0f, height + 8f, height + 8f);
            var handleRect = handle.rectTransform;
            handleRect.anchorMin = new Vector2(0f, 0.5f);
            handleRect.anchorMax = new Vector2(0f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.anchoredPosition = Vector2.zero;
            handleRect.sizeDelta = new Vector2(height + 10f, 10f); // El deslizador estira el alto: así queda redondo.
            handle.raycastTarget = true;

            var slider = root.gameObject.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle;
            slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            return slider;
        }

        /// <summary>Interruptor: pista azul (apagado) o dorada (encendido) con el mando a un lado u otro, y «Sí»/«No».</summary>
        public static Toggle Switch(string name, Transform parent, float x, float y, out TextMeshProUGUI state)
        {
            const float width = 112f;
            const float height = 60f;
            var root = Rect(name, parent, x, y, width + 90f, height);
            var off = Picture("Track", root, TrackSprite(), 0f, 0f, width, height);
            off.raycastTarget = true;
            var on = Picture("TrackOn", root, PillGoldSprite(), 0f, 0f, width, height);
            var knob = Picture("Knob", root, ToggleKnobSprite(), 4f, 4f, height - 8f, height - 8f);
            state = Text("State", root, "No", width + 18f, 0f, 70f, height, 28f, UiFont.Body, Ivory, TextAlignmentOptions.MidlineLeft);

            var toggle = root.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = off;
            toggle.graphic = on;
            toggle.toggleTransition = Toggle.ToggleTransition.None;
            var view = root.gameObject.AddComponent<UiSwitch>();
            SerializedWiring.Set(view, "_toggle", toggle);
            SerializedWiring.Set(view, "_knob", knob.rectTransform);
            SerializedWiring.Set(view, "_state", state);
            SerializedWiring.Set(view, "_offX", 4f);
            SerializedWiring.Set(view, "_onX", width - height + 4f);
            return toggle;
        }

        // ---- Dibujo de formas ----

        /// <summary>
        /// Rectángulo redondeado con relleno en degradado vertical (<paramref name="top"/> → <paramref name="bottom"/>) y
        /// borde de <paramref name="border"/> píxeles; con <paramref name="highlight"/>, un brillo en la mitad superior.
        /// </summary>
        public static Sprite Shape(string name, int radius, int border, Color top, Color bottom, Color borderColor, bool highlight = false)
        {
            var size = Mathf.Max(64, radius * 2 + 8);
            var pixels = new Color[size * size];
            var half = size * 0.5f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var d = RoundedDistance(x + 0.5f - half, y + 0.5f - half, half - 1f, radius);
                    var coverage = Mathf.Clamp01(0.5f - d);
                    if (coverage <= 0f)
                        continue;
                    var t = (float)y / (size - 1); // 0 abajo, 1 arriba.
                    var fill = Color.Lerp(bottom, top, t);
                    if (highlight && t > 0.55f)
                        fill = Color.Lerp(fill, Color.white, 0.18f * Mathf.InverseLerp(0.55f, 1f, t));
                    var inBorder = border > 0 ? Mathf.Clamp01(d + border + 0.5f) : 0f;
                    var color = Color.Lerp(fill, borderColor, inBorder);
                    color.a *= coverage;
                    pixels[y * size + x] = color;
                }
            }
            return GeneratedSprites.GetOrCreate($"{KIT}/{name}.png", size, size, pixels, radius + border + 2);
        }

        /// <summary>Distancia con signo al rectángulo redondeado (negativa dentro).</summary>
        private static float RoundedDistance(float px, float py, float half, float radius)
        {
            var qx = Mathf.Abs(px) - (half - radius);
            var qy = Mathf.Abs(py) - (half - radius);
            var outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
            return outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
        }

        public static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var color) ? color : Color.magenta;
    }
}
