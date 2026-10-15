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
    /// Construye el presentador (figura con primitivas y bocadillo), importa y valida su catálogo de frases,
    /// y crea su sesión de red y el subtítulo en pantalla. Solo se comunica por texto.
    /// </summary>
    public static class PresenterPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Presenter";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/Presenter.prefab";
        public const string SESSION_PREFAB_PATH = MODULE_ROOT + "/Prefabs/PresenterSession.prefab";
        public const string HUD_PREFAB_PATH = MODULE_ROOT + "/Prefabs/PresenterHud.prefab";
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

            EditorAssets.CreateWorldText("Label", t, "PRESENTADOR", 2.5f, Color.white,
                new Vector3(0f, 2.75f, 0f), Quaternion.Euler(0f, 180f, 0f), new Vector2(2.5f, 0.5f));

            // Bocadillo: siempre de cara a la cámara para leerlo desde cualquier lado.
            var bubble = EditorAssets.CreateWorldText("Speech", t, string.Empty, 3.2f, Color.white,
                new Vector3(0f, 3.6f, 0f), Quaternion.identity, new Vector2(6f, 2f));
            bubble.outlineWidth = 0.25f;
            bubble.outlineColor = Color.black;
            bubble.fontStyle = FontStyles.Italic;
            bubble.gameObject.AddComponent<Billboard>();
            // La vista vive en la raíz (siempre activa); el bocadillo es lo que se muestra u oculta.
            var speech = root.AddComponent<PresenterLineView>();
            SerializedWiring.Set(speech, "_runtime", Runtime());
            SerializedWiring.Set(speech, "_label", bubble);
            SerializedWiring.Set(speech, "_container", bubble.gameObject);

            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
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

        /// <summary>Subtítulo en pantalla con la frase del presentador, encima del aviso de interacción.</summary>
        public static GameObject BuildHud()
        {
            var root = new GameObject("PresenterHud", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 6;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(root.transform, false);
            var panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, 220f);
            panelRect.sizeDelta = new Vector2(1200f, 70f);
            var background = panel.AddComponent<Image>();
            background.color = new Color(0.25f, 0.05f, 0.1f, 0.8f);
            background.raycastTarget = false;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(panel.transform, false);
            var labelRect = (RectTransform)labelGo.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(18f, 4f);
            labelRect.offsetMax = new Vector2(-18f, -4f);
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.fontSize = 28f;
            label.fontStyle = FontStyles.Italic;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.richText = true;
            label.raycastTarget = false;
            label.color = Color.white;

            var view = root.AddComponent<PresenterLineView>();
            SerializedWiring.Set(view, "_runtime", Runtime());
            SerializedWiring.Set(view, "_label", label);
            SerializedWiring.Set(view, "_container", panel);
            SerializedWiring.Set(view, "_prefix", "<b>PRESENTADOR:</b> ");

            return EditorAssets.SaveAsPrefab(root, HUD_PREFAB_PATH);
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
