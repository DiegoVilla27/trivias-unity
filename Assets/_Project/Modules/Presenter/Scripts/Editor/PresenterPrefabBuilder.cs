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

namespace Trivias.Presenter.Editor
{
    /// <summary>
    /// Construye el presentador (figura con primitivas y viñeta sobre la cabeza), importa y valida su catálogo
    /// de frases y crea su sesión de red. Solo se comunica por texto en su viñeta.
    /// </summary>
    public static class PresenterPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Presenter";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/Presenter.prefab";
        public const string SESSION_PREFAB_PATH = MODULE_ROOT + "/Prefabs/PresenterSession.prefab";
        public const string BUBBLE = "Bubble";
        public const float BUBBLE_HEIGHT = 4.3f;
        public static readonly Vector2 BubbleSize = new(7f, 2.6f);
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

            // Viñeta sobre la cabeza (GDD 9): el presentador solo se comunica por aquí. Siempre de cara a la cámara.
            var bubbleMaterial = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Presenter_Bubble.mat",
                new Color(0.97f, 0.96f, 0.92f), 0.2f, emission: new Color(0.45f, 0.45f, 0.43f));
            var bubble = EditorAssets.CreateEmpty(BUBBLE, t, new Vector3(0f, BUBBLE_HEIGHT, 0f));
            bubble.AddComponent<Billboard>();
            // El texto se lee mirando a su +Z: el fondo va detrás (+Z) y la punta hacia la cabeza.
            EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Background", bubble.transform, new Vector3(0f, 0f, 0.06f),
                new Vector3(BubbleSize.x, BubbleSize.y, 0.05f), bubbleMaterial, layer, false, keepCollider: false);
            var tail = EditorAssets.CreatePrimitive(PrimitiveType.Cube, "Tail", bubble.transform,
                new Vector3(0f, -BubbleSize.y * 0.5f - 0.15f, 0.06f), new Vector3(0.45f, 0.45f, 0.05f), bubbleMaterial, layer, false,
                keepCollider: false);
            tail.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);

            var text = EditorAssets.CreateWorldText("Text", bubble.transform, string.Empty, 4.4f, new Color(0.08f, 0.08f, 0.18f),
                Vector3.zero, Quaternion.identity, new Vector2(BubbleSize.x - 0.5f, BubbleSize.y - 0.3f));
            text.fontStyle = FontStyles.Bold;

            // La vista vive en la raíz (siempre activa); la viñeta es lo que se muestra u oculta.
            var speech = root.AddComponent<PresenterLineView>();
            SerializedWiring.Set(speech, "_runtime", Runtime());
            SerializedWiring.Set(speech, "_label", text);
            SerializedWiring.Set(speech, "_container", bubble);
            bubble.SetActive(false);

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
