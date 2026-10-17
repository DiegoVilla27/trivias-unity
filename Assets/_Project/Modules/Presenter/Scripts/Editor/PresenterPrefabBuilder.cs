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
        public const string ANIMATIONS = MODULE_ROOT + "/Animations";
        public const string ANIMATOR_PATH = ANIMATIONS + "/Presenter.controller";
        private const string OLD_ANIMATOR_PATH = ANIMATIONS + "/PresenterIdle.controller";
        public const string TALK_CLIP = ANIMATIONS + "/Presenter_Talk.fbx";
        public const string HAPPY_CLIP = ANIMATIONS + "/Presenter_Happy.fbx";
        public const string SAD_CLIP = ANIMATIONS + "/Presenter_Sad.fbx";
        public const string MICROPHONE = "Microphone";
        public const string MIC_GRILLE = "Grille";
        private const string MIC_MODEL = MODULE_ROOT + "/Models/Microphone/Microphone.fbx";
        private const string MIC_TEXTURE = MODULE_ROOT + "/Models/Microphone/shaded.png";
        private const string MIC_MATERIAL = MODULE_ROOT + "/Models/Microphone/Microphone_Mat.mat";
        /// <summary>Lo que asoma el mango por detrás del puño (el micro mide 34 cm desde la base).</summary>
        private const float MIC_GRIP = 0.03f;
        /// <summary>Altura del centro de la rejilla desde la base del micro.</summary>
        private const float MIC_GRILLE_HEIGHT = 0.285f;
        private static readonly string[] Layers = { "Base", "TalkMic", "TalkGesture", "TalkHead" };
        /// <summary>Del centro del puño a la muñeca, por la mano (igual que en las animaciones de Blender).</summary>
        private const float FIST = 0.09f;
        /// <summary>Fotogramas por segundo con los que se diseñaron sus animaciones en Blender.</summary>
        public const float DESIGN_FPS = 30f;
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

        public static GameObject Build(MatchRuntime match)
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
            animator.runtimeAnimatorController = Controller();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms; // Si nadie lo ve, no mueve los huesos.
            AddMicrophone(model.transform, layer);

            var body = root.AddComponent<CapsuleCollider>();
            body.center = new Vector3(0f, HEIGHT * 0.5f, 0f);
            body.height = HEIGHT;
            body.radius = BODY_RADIUS;

            var presenterAnimator = root.AddComponent<PresenterAnimator>();
            SerializedWiring.Set(presenterAnimator, "_match", match);
            SerializedWiring.Set(presenterAnimator, "_speech", Runtime());
            SerializedWiring.Set(presenterAnimator, "_animator", animator);
            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }

        /// <summary>
        /// Su micrófono (el modelo del usuario tal cual, preparado con <c>Tools/Blender/accessories.py</c>: 34 cm y el origen
        /// en la base) en la mano derecha: en la pose T apunta al frente desde el puño, igual que en las animaciones de
        /// Blender, con el puño cerrado en la parte baja del mango; así al hablar la rejilla llega a la boca.
        /// </summary>
        private static void AddMicrophone(Transform model, int layer)
        {
            var hand = FindDeep(model, "RightHand");
            var forearm = FindDeep(model, "RightLowerArm");
            var fist = hand.position + (hand.position - forearm.position).normalized * FIST;
            var forward = model.forward;

            var mic = new GameObject(MICROPHONE) { layer = layer };
            // El modelo va de pie (arriba +Y): se tumba para que apunte al frente.
            mic.transform.SetPositionAndRotation(fist - forward * MIC_GRIP, model.rotation * Quaternion.Euler(90f, 0f, 0f));
            var microphone = (GameObject)PrefabUtility.InstantiatePrefab(HumanoidModels.EnsureStaticImported(MIC_MODEL, MIC_TEXTURE,
                MIC_MATERIAL, "Microphone_Mat"), mic.transform);
            microphone.name = "Model";
            foreach (var part in mic.GetComponentsInChildren<Transform>(true))
                part.gameObject.layer = layer;
            // Centro de la rejilla (lo que debe quedar en la boca al hablar).
            var grille = new GameObject(MIC_GRILLE).transform;
            grille.SetParent(mic.transform, false);
            grille.localPosition = new Vector3(0f, MIC_GRILLE_HEIGHT, 0f);
            mic.transform.SetParent(hand, true);
        }

        /// <summary>
        /// Controlador: reposo (Starter Assets), alegría y decepción en la capa base; hablar en tres capas que solo tocan
        /// su parte (brazo derecho con el micro, brazo izquierdo que gesticula y cabeza, esta sumada al resto).
        /// </summary>
        private static RuntimeAnimatorController Controller()
        {
            var idle = AssetDatabase.LoadAllAssetsAtPath(IDLE_CLIP_PATH).OfType<AnimationClip>()
                .First(clip => !clip.name.StartsWith("__preview__"));
            var talk = HumanoidModels.EnsureAnimationImported(TALK_CLIP, MODEL_PATH, loop: true);
            var happy = HumanoidModels.EnsureAnimationImported(HAPPY_CLIP, MODEL_PATH, loop: false);
            var sad = HumanoidModels.EnsureAnimationImported(SAD_CLIP, MODEL_PATH, loop: false);
            var masks = new[]
            {
                null,
                Mask("Presenter_RightArm", AvatarMaskBodyPart.RightArm),
                Mask("Presenter_LeftArm", AvatarMaskBodyPart.LeftArm),
                Mask("Presenter_Head", AvatarMaskBodyPart.Head),
            };

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(OLD_ANIMATOR_PATH) != null)
                AssetDatabase.DeleteAsset(OLD_ANIMATOR_PATH);
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ANIMATOR_PATH);
            if (controller != null && !controller.layers.Select(l => l.name).SequenceEqual(Layers))
            {
                AssetDatabase.DeleteAsset(ANIMATOR_PATH);
                controller = null;
            }
            if (controller == null)
                controller = CreateController(masks);

            // Lo que puede cambiar sin rehacer el controlador: clips y máscaras.
            var layers = controller.layers;
            Motion(layers[0], "Idle", idle);
            Motion(layers[0], PresenterAnimator.HAPPY, happy);
            Motion(layers[0], PresenterAnimator.SAD, sad);
            for (var i = 1; i < layers.Length; i++)
            {
                Motion(layers[i], "Talk", talk);
                layers[i].avatarMask = masks[i];
            }
            controller.layers = layers;
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssetIfDirty(controller);
            return controller;
        }

        private static AnimatorController CreateController(AvatarMask[] masks)
        {
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ANIMATOR_PATH);
            controller.AddParameter(PresenterAnimator.HAPPY, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(PresenterAnimator.SAD, AnimatorControllerParameterType.Trigger);

            var layers = controller.layers;
            layers[0].name = Layers[0];
            controller.layers = layers;
            var machine = controller.layers[0].stateMachine;
            var idle = machine.AddState("Idle");
            machine.defaultState = idle;
            foreach (var reaction in new[] { PresenterAnimator.HAPPY, PresenterAnimator.SAD })
            {
                var state = machine.AddState(reaction);
                var enter = machine.AddAnyStateTransition(state);
                enter.AddCondition(AnimatorConditionMode.If, 0f, reaction);
                enter.hasExitTime = false;
                enter.hasFixedDuration = true;
                enter.duration = 0.2f;
                enter.canTransitionToSelf = true;
                var back = state.AddTransition(idle);
                back.hasExitTime = true;
                back.exitTime = 0.86f;
                back.hasFixedDuration = true;
                back.duration = 0.35f;
            }

            for (var i = 1; i < Layers.Length; i++)
            {
                var talkMachine = new AnimatorStateMachine { name = Layers[i], hideFlags = HideFlags.HideInHierarchy };
                AssetDatabase.AddObjectToAsset(talkMachine, controller);
                talkMachine.defaultState = talkMachine.AddState("Talk");
                controller.AddLayer(new AnimatorControllerLayer
                {
                    name = Layers[i],
                    stateMachine = talkMachine,
                    avatarMask = masks[i],
                    defaultWeight = 0f,
                    // La cabeza se suma (asiente y ladea sobre lo que ya hace); los brazos se sustituyen.
                    blendingMode = i == PresenterAnimator.HEAD_LAYER ? AnimatorLayerBlendingMode.Additive : AnimatorLayerBlendingMode.Override,
                });
            }
            return controller;
        }

        private static void Motion(AnimatorControllerLayer layer, string stateName, Motion motion)
        {
            var state = layer.stateMachine.states.Select(s => s.state).First(s => s.name == stateName);
            if (state.motion != motion)
                state.motion = motion;
            // Las animaciones del presentador se diseñaron a 30 fotogramas por segundo: si el archivo dice otra cosa
            // (Blender exporta a 24 por defecto), se reproducen al ritmo con el que se hicieron.
            var clip = (AnimationClip)motion;
            var speed = AssetDatabase.GetAssetPath(clip).StartsWith(ANIMATIONS) ? DESIGN_FPS / clip.frameRate : 1f;
            if (!Mathf.Approximately(state.speed, speed))
                state.speed = speed;
        }

        /// <summary>Máscara humanoide con una sola parte del cuerpo.</summary>
        private static AvatarMask Mask(string name, AvatarMaskBodyPart part)
        {
            var path = $"{ANIMATIONS}/{name}.mask";
            var mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(path);
            if (mask == null)
            {
                mask = new AvatarMask();
                AssetDatabase.CreateAsset(mask, path);
            }
            for (var p = AvatarMaskBodyPart.Root; p < AvatarMaskBodyPart.LastBodyPart; p++)
                mask.SetHumanoidBodyPartActive(p, p == part);
            EditorUtility.SetDirty(mask);
            return mask;
        }

        private static Transform FindDeep(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;
                var found = FindDeep(child, name);
                if (found != null)
                    return found;
            }
            return null;
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
