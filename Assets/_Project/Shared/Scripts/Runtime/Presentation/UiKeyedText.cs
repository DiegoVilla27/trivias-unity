using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Shared.Presentation
{
    /// <summary>
    /// Texto en el que las teclas se ven como teclas del juego: «[E] coge uno» pinta la E sobre una tecla dibujada (la
    /// misma de los avisos). Las teclas son imágenes ya creadas detrás del texto; al cambiar el texto se colocan bajo
    /// sus letras (solo entonces: no hay trabajo por fotograma).
    /// </summary>
    public sealed class UiKeyedText : MonoBehaviour
    {
        /// <summary>Hueco a cada lado de la letra, en «em» (tamaño de la letra), para que la tecla la abrace.</summary>
        private const float SIDE_EM = 0.42f;

        [SerializeField] private TMP_Text _text;
        [Tooltip("Teclas ya creadas detrás del texto (las que sobran se esconden).")]
        [SerializeField] private Image[] _keys;
        [SerializeField] private Color _keyColor = new(0.96f, 0.94f, 0.88f);

        private readonly StringBuilder _builder = new();
        private readonly List<(int First, int Count)> _tokens = new();

        public TMP_Text Label => _text;

        /// <summary>Teclas visibles ahora (las pruebas lo leen).</summary>
        public int ShownKeys { get; private set; }

        /// <summary>Pone el texto; cada «[tecla]» se dibuja como tecla.</summary>
        public void SetText(string source)
        {
            _text.text = Markup(source, _builder, _tokens, ColorUtility.ToHtmlStringRGB(_keyColor));
            _text.ForceMeshUpdate(true);
            PlaceKeys();
        }

        /// <summary>
        /// Convierte «[E]» en la letra con hueco a los lados y apunta qué caracteres visibles ocupa cada tecla (las
        /// etiquetas de formato no cuentan como caracteres). C# puro: lo prueban los tests.
        /// </summary>
        public static string Markup(string source, StringBuilder builder, List<(int First, int Count)> tokens, string keyColor)
        {
            builder.Clear();
            tokens.Clear();
            var visible = 0;
            for (var i = 0; i < source.Length; i++)
            {
                var c = source[i];
                if (c == '<')
                {
                    // Una etiqueta de formato se copia tal cual y no cuenta.
                    var close = source.IndexOf('>', i);
                    if (close > i)
                    {
                        builder.Append(source, i, close - i + 1);
                        i = close;
                        continue;
                    }
                }
                if (c == '[')
                {
                    var end = source.IndexOf(']', i);
                    if (end > i + 1)
                    {
                        var key = source.Substring(i + 1, end - i - 1);
                        builder.Append("<space=").Append(SIDE_EM.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append("em>")
                            .Append("<b><color=#").Append(keyColor).Append('>').Append(key).Append("</color></b>")
                            .Append("<space=").Append(SIDE_EM.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append("em>");
                        tokens.Add((visible, key.Length));
                        visible += key.Length;
                        i = end;
                        continue;
                    }
                }
                builder.Append(c);
                visible++;
            }
            return builder.ToString();
        }

        private void PlaceKeys()
        {
            var info = _text.textInfo;
            var textRect = _text.rectTransform;
            ShownKeys = 0;
            for (var k = 0; k < _keys.Length; k++)
            {
                var shown = k < _tokens.Count && _tokens[k].First + _tokens[k].Count <= info.characterCount;
                if (shown)
                {
                    var (first, count) = _tokens[k];
                    var start = info.characterInfo[first];
                    var end = info.characterInfo[first + count - 1];
                    shown = start.isVisible && start.lineNumber == end.lineNumber;
                    if (shown)
                    {
                        var line = info.lineInfo[start.lineNumber];
                        var side = start.pointSize * SIDE_EM * 0.85f;
                        var pad = start.pointSize * 0.12f;
                        var min = new Vector2(start.bottomLeft.x - side, line.descender - pad);
                        var max = new Vector2(end.topRight.x + side, line.ascender + pad);
                        var key = _keys[k].rectTransform;
                        // Las teclas viven en una capa del mismo tamaño y pivote que el texto: mismas coordenadas.
                        key.anchorMin = key.anchorMax = textRect.pivot;
                        key.pivot = new Vector2(0.5f, 0.5f);
                        key.anchoredPosition = (min + max) * 0.5f;
                        key.sizeDelta = max - min;
                        ShownKeys++;
                    }
                }
                _keys[k].gameObject.SetActive(shown);
            }
        }
    }
}
