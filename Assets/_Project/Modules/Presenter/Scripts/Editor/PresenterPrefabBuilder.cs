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
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Presenter.Editor
{
    /// <summary>
    /// Construye el presentador (el modelo del usuario tal cual, Humanoid, en reposo con la animación de Starter Assets)
    /// y su rótulo de televisión en pantalla, importa y valida su catálogo de frases y crea su sesión de red. Solo se
    /// comunica por texto en su rótulo.
    /// </summary>
    public static class PresenterPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Presenter";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/Presenter.prefab";
        public const string SESSION_PREFAB_PATH = MODULE_ROOT + "/Prefabs/PresenterSession.prefab";
        public const string MODEL = "Model";
        public const string MODEL_PATH = MODULE_ROOT + "/Models/Presenter/Presenter_Rigged.fbx";
        private const string TEXTURE_PATH = MODULE_ROOT + "/Models/Presenter/shaded.png";
        private const string MATERIAL_PATH = MODULE_ROOT + "/Models/Presenter/Presenter_Body.mat";
        public const string ANIMATOR_PATH = MODULE_ROOT + "/Animations/PresenterIdle.controller";
        private const string IDLE_CLIP_PATH = "Assets/ThirdParty/StarterAssets/Runtime/ThirdPersonController/Character/Animations/Stand--Idle.anim.fbx";
        /// <summary>Altura del modelo (la del original, sin escalar).</summary>
        public const float HEIGHT = 1.9f;
        private const float BODY_RADIUS = 0.42f;
        public const string CAPTION_PREFAB_PATH = MODULE_ROOT + "/Prefabs/PresenterCaption.prefab";
        private const string PORTRAIT_PATH = MODULE_ROOT + "/UI/Portrait/Portrait_Presenter.png";
        private const string CONFIG = MODULE_ROOT + "/Config";
        private const string CONTENT_PATH = MODULE_ROOT + "/Content/phrases_es.json";

        public static PresenterRuntime Runtime() => EditorAssets.GetOrCreateAsset<PresenterRuntime>($"{CONFIG}/PresenterRuntime.asset");
        public static PresenterConfig Config() => EditorAssets.GetOrCreateAsset<PresenterConfig>($"{CONFIG}/PresenterConfig.asset");

        public static GameObject Build()
        {
            HumanoidModels.EnsureImported(MODEL_PATH, TEXTURE_PATH, MATERIAL_PATH, "Presenter_Body");
            const int layer = GameLayers.STAGE;
            var root = new GameObject("Presenter") { layer = layer };

            // El modelo tal cual; nadie lo atraviesa (una cápsula sencilla, el modelo no trae colisiones).
            var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(MODEL_PATH), root.transform);
            model.name = MODEL;
            foreach (var part in model.GetComponentsInChildren<Transform>(true))
                part.gameObject.layer = layer;
            foreach (var skinned in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                skinned.updateWhenOffscreen = false;
            if (!model.TryGetComponent<Animator>(out var animator))
                animator = model.AddComponent<Animator>();
            animator.avatar = HumanoidModels.AvatarOf(MODEL_PATH);
            animator.runtimeAnimatorController = IdleController();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms; // Si nadie lo ve, no mueve los huesos.

            var body = root.AddComponent<CapsuleCollider>();
            body.center = new Vector3(0f, HEIGHT * 0.5f, 0f);
            body.height = HEIGHT;
            body.radius = BODY_RADIUS;

            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }

        /// <summary>Un solo estado: el reposo de Starter Assets en bucle (el presentador no anda).</summary>
        private static RuntimeAnimatorController IdleController()
        {
            var idle = AssetDatabase.LoadAllAssetsAtPath(IDLE_CLIP_PATH).OfType<AnimationClip>()
                .First(clip => !clip.name.StartsWith("__preview__"));
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ANIMATOR_PATH);
            if (controller == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ANIMATOR_PATH)!);
                controller = AnimatorController.CreateAnimatorControllerAtPath(ANIMATOR_PATH);
            }
            var machine = controller.layers[0].stateMachine;
            var state = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == "Idle") ?? machine.AddState("Idle");
            if (state.motion != idle || machine.defaultState != state)
            {
                state.motion = idle;
                machine.defaultState = state;
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssetIfDirty(controller);
            }
            return controller;
        }

        /// <summary>
        /// Rótulo de televisión del presentador (GDD 9), abajo a la izquierda con el estilo del HUD: su cara en un círculo
        /// dorado (retrato sacado de su modelo), «PRESENTADOR» y la frase. El ánimo cambia el color del nombre y su raya.
        /// Se dibuja por encima del HUD y por debajo de los avisos.
        /// </summary>
        public static GameObject BuildCaption()
        {
            var root = new GameObject("PresenterCaption", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 6;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            // Marco de 1920 × 1080 pegado a la esquina de abajo a la izquierda (como las esquinas del HUD).
            var corner = new GameObject("BottomLeft", typeof(RectTransform));
            corner.transform.SetParent(root.transform, false);
            var cornerRect = (RectTransform)corner.transform;
            cornerRect.anchorMin = cornerRect.anchorMax = cornerRect.pivot = Vector2.zero;
            cornerRect.sizeDelta = new Vector2(1920f, 1080f);

            var card = UiKit.Picture("Card", cornerRect, UiKit.HudPlateSprite(), 48f, 742f, 550f, 132f);
            var group = card.gameObject.AddComponent<CanvasGroup>();
            card.gameObject.AddComponent<UiPanelIntro>();
            UiKit.Picture("Ring", card.transform, UiKit.CircleGoldSprite(), 14f, 14f, 104f, 104f);
            UiKit.Picture("Portrait", card.transform, UiKit.ArtSprite(PORTRAIT_PATH), 18f, 18f, 96f, 96f, preserveAspect: true);
            var name = UiKit.Text("Name", card.transform, "PRESENTADOR", 132f, 12f, 390f, 30f, 22f, UiKit.UiFont.Display, UiKit.Gold,
                TextAlignmentOptions.MidlineLeft);
            name.characterSpacing = 3f;
            var accent = UiKit.Picture("Accent", card.transform, UiKit.LineSprite(), 132f, 42f, 56f, 3f);
            var label = UiKit.Text("Line", card.transform, string.Empty, 132f, 50f, 398f, 72f, 23f, UiKit.UiFont.Bold, UiKit.Ivory,
                TextAlignmentOptions.TopLeft);
            label.fontSizeMin = 17f;
            label.lineSpacing = -4f;

            var view = root.AddComponent<PresenterCaptionView>();
            SerializedWiring.Set(view, "_runtime", Runtime());
            SerializedWiring.Set(view, "_config", Config());
            SerializedWiring.Set(view, "_card", card.gameObject);
            SerializedWiring.Set(view, "_group", group);
            SerializedWiring.Set(view, "_name", name);
            SerializedWiring.Set(view, "_accent", accent);
            SerializedWiring.Set(view, "_label", label);
            WriteMoodColors(view);
            card.gameObject.SetActive(false);
            return EditorAssets.SaveAsPrefab(root, CAPTION_PREFAB_PATH);
        }

        /// <summary>Un color por ánimo (orden de <see cref="PresenterMood"/>): dorado, naranja de fiesta, lila y azul.</summary>
        private static void WriteMoodColors(PresenterCaptionView view)
        {
            var colors = new Color[PresenterMoods.COUNT];
            colors[(int)PresenterMood.Neutral] = UiKit.Gold;
            colors[(int)PresenterMood.Excited] = UiKit.Hex("#FFA94D");
            colors[(int)PresenterMood.Suspense] = UiKit.Hex("#B9A2FF");
            colors[(int)PresenterMood.Sad] = UiKit.Hex("#8FB8FF");
            var so = new SerializedObject(view);
            var array = so.FindProperty("_moodColors");
            array.arraySize = colors.Length;
            for (var i = 0; i < colors.Length; i++)
                array.GetArrayElementAtIndex(i).colorValue = colors[i];
            so.ApplyModifiedPropertiesWithoutUndo();
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
