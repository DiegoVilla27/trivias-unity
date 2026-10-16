using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Trivias.Match.Data;
using Trivias.Presenter.Components;
using Trivias.Presenter.Data;
using Trivias.Presenter.Logic;
using Trivias.Presenter.Network;
using Trivias.SelfBet.Data;
using Trivias.Shared.Editor;
using Trivias.Shared.Physics;
using Trivias.Shared.Presentation;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Presenter.Editor
{
    /// <summary>
    /// Construye el presentador (figura con primitivas y viñeta de cómic junto a la cabeza), importa y valida su catálogo
    /// de frases y crea su sesión de red. Solo se comunica por texto en su viñeta.
    /// </summary>
    public static class PresenterPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Presenter";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/Presenter.prefab";
        public const string SESSION_PREFAB_PATH = MODULE_ROOT + "/Prefabs/PresenterSession.prefab";
        public const string BUBBLE = "Bubble";
        /// <summary>
        /// La viñeta sale al lado de la cabeza (no encima): el presentador está al pie de la pantalla y así ni él ni lo que
        /// dice tapan la pregunta. Gira hacia la cámara, de modo que siempre queda a la derecha de su cabeza.
        /// </summary>
        public const float BUBBLE_HEIGHT = 1.55f;
        public static readonly Vector2 BubbleSize = new(4.8f, 1.8f);
        private const float BUBBLE_GAP = 0.5f;
        /// <summary>Metros por unidad del lienzo de la viñeta (100 unidades = 1 m).</summary>
        private const float CANVAS_UNIT = 0.01f;
        public const string BUBBLE_BACKGROUND = "Background";
        /// <summary>Forma de la viñeta, desde la raíz del presentador.</summary>
        public const string BUBBLE_BACKGROUND_PATH = BUBBLE + "/Canvas/Body/" + BUBBLE_BACKGROUND;
        private const string CONFIG = MODULE_ROOT + "/Config";
        private const string CONTENT_PATH = MODULE_ROOT + "/Content/phrases_es.json";

        public static PresenterRuntime Runtime() => EditorAssets.GetOrCreateAsset<PresenterRuntime>($"{CONFIG}/PresenterRuntime.asset");
        public static PresenterConfig Config() => EditorAssets.GetOrCreateAsset<PresenterConfig>($"{CONFIG}/PresenterConfig.asset");

        public static GameObject Build()
        {
            var suit = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Presenter_Suit.mat", StagePalette.Burgundy, 0.5f);
            var skin = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Presenter_Skin.mat", StagePalette.Skin, 0.3f);
            var hair = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Presenter_Hair.mat", new Color(0.2f, 0.2f, 0.22f), 0.4f);

            var root = new GameObject("Presenter") { layer = GameLayers.STAGE };
            var t = root.transform;
            const int layer = GameLayers.STAGE;

            EditorAssets.CreatePrimitive(PrimitiveType.Capsule, "Body", t, new Vector3(0f, 0.75f, 0f), new Vector3(0.8f, 0.75f, 0.6f), suit, layer, false);
            EditorAssets.CreatePrimitive(PrimitiveType.Sphere, "Head", t, new Vector3(0f, 1.85f, 0f), Vector3.one * 0.9f, skin, layer, false, keepCollider: false);
            EditorAssets.CreatePrimitive(PrimitiveType.Sphere, "Hair", t, new Vector3(0f, 2.1f, -0.08f), new Vector3(0.95f, 0.5f, 0.85f), hair, layer, false, keepCollider: false);
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Moustache", t, new Vector3(0f, 1.72f, 0.43f), new Vector3(0.4f, 0.08f, 0.06f), hair, layer, false, keepCollider: false);

            var label = EditorAssets.CreateWorldText("Label", t, "PRESENTADOR", 2.6f, StagePalette.Gold,
                new Vector3(0f, 2.62f, 0f), Quaternion.Euler(0f, 180f, 0f), new Vector2(2.8f, 0.35f), StageFonts.Display());
            label.characterSpacing = 3f;
            label.textWrappingMode = TextWrappingModes.NoWrap;

            BuildBubble(root, layer);
            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }

        /// <summary>
        /// Viñeta de cómic al lado de la cabeza (GDD 9): el presentador solo se comunica por aquí. Es un lienzo en el
        /// mundo que gira hacia la cámara; la forma, el color y el movimiento de las letras cambian con el ánimo.
        /// </summary>
        private static void BuildBubble(GameObject root, int layer)
        {
            var bubble = EditorAssets.CreateEmpty(BUBBLE, root.transform, new Vector3(0f, BUBBLE_HEIGHT, 0f));
            bubble.layer = layer;
            bubble.AddComponent<Billboard>();

            var side = BubbleSize.x * 0.5f + BUBBLE_GAP;
            var size = BubbleSize / CANVAS_UNIT;
            var canvasRect = UiRect("Canvas", bubble.transform, size + new Vector2(140f, 120f), layer);
            canvasRect.localPosition = new Vector3(side, 0f, 0f);
            canvasRect.localScale = Vector3.one * CANVAS_UNIT;
            var canvas = canvasRect.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var body = UiRect("Body", canvasRect, size, layer);
            var rays = UiImage("Rays", body, SpeechBubbleArt.Rays(), size * 1.3f, Vector2.zero, layer);
            var shadow = UiImage("Shadow", body, SpeechBubbleArt.Round(), size, new Vector2(10f, -12f), layer);
            shadow.color = new Color(0f, 0f, 0f, 0.35f);
            // La cola sale del borde izquierdo hacia la cabeza; su base queda debajo de la forma.
            var tail = UiImage("Tail", body, SpeechBubbleArt.Tail(), new Vector2(76f, 76f), Vector2.zero, layer);
            tail.rectTransform.pivot = new Vector2(1f, 0.5f);
            tail.rectTransform.anchoredPosition = new Vector2(-size.x * 0.5f + 34f, 6f);
            tail.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -12f);
            var shape = UiImage(BUBBLE_BACKGROUND, body, SpeechBubbleArt.Round(), size, Vector2.zero, layer);

            // Caja del texto dentro de la zona lisa de cualquier forma (las puntas del estallido y los bultos de la nube no la pisan).
            var textRect = UiRect("Text", body, new Vector2(size.x * 0.74f, size.y * 0.64f), layer);
            var text = textRect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = StageFonts.Display();
            text.fontSize = 50f;
            text.enableAutoSizing = true;
            text.fontSizeMin = 24f;
            text.fontSizeMax = 50f;
            text.characterSpacing = 1f;
            text.lineSpacing = -6f;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.raycastTarget = false;

            // La vista vive en la raíz (siempre activa); la viñeta es lo que se muestra u oculta.
            var view = root.AddComponent<PresenterBubbleView>();
            SerializedWiring.Set(view, "_runtime", Runtime());
            SerializedWiring.Set(view, "_config", Config());
            SerializedWiring.Set(view, "_container", bubble);
            SerializedWiring.Set(view, "_body", body);
            SerializedWiring.Set(view, "_shape", shape);
            SerializedWiring.Set(view, "_shadow", shadow);
            SerializedWiring.Set(view, "_tail", tail);
            SerializedWiring.Set(view, "_rays", rays);
            SerializedWiring.Set(view, "_label", text);
            WriteStyles(view);
            bubble.SetActive(false);
        }

        /// <summary>Un estilo por ánimo (orden de <see cref="PresenterMood"/>).</summary>
        private static void WriteStyles(PresenterBubbleView view)
        {
            var styles = new BubbleStyle[PresenterMoods.COUNT];
            styles[(int)PresenterMood.Neutral] = new BubbleStyle
            {
                Shape = SpeechBubbleArt.Round(), Fill = new Color(0.99f, 0.97f, 0.9f), Text = new Color(0.09f, 0.08f, 0.2f),
            };
            styles[(int)PresenterMood.Excited] = new BubbleStyle
            {
                Shape = SpeechBubbleArt.Burst(), Fill = new Color(1f, 0.86f, 0.28f), Text = new Color(0.36f, 0.04f, 0.03f),
                Rays = true, RaysColor = new Color(1f, 0.96f, 0.7f, 0.85f),
            };
            styles[(int)PresenterMood.Suspense] = new BubbleStyle
            {
                Shape = SpeechBubbleArt.Cloud(), Fill = new Color(0.22f, 0.13f, 0.38f), Text = new Color(1f, 0.9f, 0.55f),
            };
            styles[(int)PresenterMood.Sad] = new BubbleStyle
            {
                Shape = SpeechBubbleArt.Round(), Fill = new Color(0.68f, 0.79f, 0.98f), Text = new Color(0.07f, 0.1f, 0.3f),
            };

            var so = new SerializedObject(view);
            var array = so.FindProperty("_styles");
            array.arraySize = styles.Length;
            for (var i = 0; i < styles.Length; i++)
            {
                var element = array.GetArrayElementAtIndex(i);
                element.FindPropertyRelative(nameof(BubbleStyle.Shape)).objectReferenceValue = styles[i].Shape;
                element.FindPropertyRelative(nameof(BubbleStyle.Fill)).colorValue = styles[i].Fill;
                element.FindPropertyRelative(nameof(BubbleStyle.Text)).colorValue = styles[i].Text;
                element.FindPropertyRelative(nameof(BubbleStyle.Rays)).boolValue = styles[i].Rays;
                element.FindPropertyRelative(nameof(BubbleStyle.RaysColor)).colorValue = styles[i].RaysColor;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static RectTransform UiRect(string name, Transform parent, Vector2 size, int layer)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = layer };
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = size;
            return rect;
        }

        private static Image UiImage(string name, Transform parent, Sprite sprite, Vector2 size, Vector2 position, int layer)
        {
            var rect = UiRect(name, parent, size, layer);
            rect.anchoredPosition = position;
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>Sesión de red del presentador (el host decide qué dice y cuándo).</summary>
        public static GameObject BuildSession(MatchRuntime match, SelfBetRuntime selfBet)
        {
            EditorUtility.SetDirty(Config());
            var catalog = BuildCatalog();

            var root = new GameObject("PresenterSession");
            root.AddComponent<NetworkObject>();
            var session = root.AddComponent<PresenterSessionNetwork>();
            SerializedWiring.Set(session, "_config", Config());
            SerializedWiring.Set(session, "_catalog", catalog);
            SerializedWiring.Set(session, "_runtime", Runtime());
            SerializedWiring.Set(session, "_match", match);
            SerializedWiring.Set(session, "_selfBet", selfBet);
            return EditorAssets.SaveAsPrefab(root, SESSION_PREFAB_PATH);
        }

        [Serializable] private sealed class PhraseDto { public string id; public string @event; public string text; }

        [Serializable] private sealed class FileDto { public string language; public PhraseDto[] phrases; }

        /// <summary>Importa y valida las frases (tiempo de edición): si hay errores, el build se detiene.</summary>
        private static PresenterCatalog BuildCatalog()
        {
            var file = JsonUtility.FromJson<FileDto>(File.ReadAllText(CONTENT_PATH));
            if (file?.phrases == null || file.phrases.Length == 0)
                throw new InvalidOperationException($"{CONTENT_PATH} no contiene frases.");

            var errors = PhraseValidator.Validate(file.phrases.Select(p => (p.id, p.@event, p.text)).ToList());
            if (errors.Count > 0)
                throw new InvalidOperationException($"Frases con errores en {CONTENT_PATH}:\n- {string.Join("\n- ", errors)}");

            var phrases = new List<PhraseData>(file.phrases.Length);
            foreach (var phrase in file.phrases)
            {
                PresenterEvents.TryParse(phrase.@event, out var presenterEvent);
                phrases.Add(new PhraseData(phrase.id, presenterEvent, phrase.text));
            }

            var catalog = EditorAssets.GetOrCreateAsset<PresenterCatalog>($"{CONFIG}/PresenterCatalog_es.asset");
            catalog.EditorSet(file.language, phrases);
            EditorUtility.SetDirty(catalog);
            Debug.Log($"[Presentador] {phrases.Count} frases importadas y validadas.");
            return catalog;
        }
    }
}
