using TMPro;
using Trivias.Interaction.Components;
using Trivias.Interaction.Network;
using Trivias.Shared.Editor;
using Trivias.Shared.Events;
using Trivias.Shared.Interaction;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Trivias.Interaction.Editor
{
    /// <summary>Construye el aviso de interacción en pantalla y añade el interactor al prefab del concursante.</summary>
    public static class InteractionBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Interaction";
        public const string HUD_PREFAB_PATH = MODULE_ROOT + "/Prefabs/InteractionHud.prefab";
        private const string PROMPT_CHANNEL_PATH = MODULE_ROOT + "/Config/InteractionPromptChannel.asset";
        /// <summary>Acciones que puede ofrecer un aviso a la vez (la mesa ofrece hasta tres: E, Q y R).</summary>
        private const int MAX_ACTIONS = 4;

        public static StringEventChannel PromptChannel() =>
            EditorAssets.GetOrCreateAsset<StringEventChannel>(PROMPT_CHANNEL_PATH);

        [MenuItem("Tools/Trivias/Build/Interaction HUD")]
        public static GameObject BuildHud()
        {
            var root = new GameObject("InteractionHud", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            // Píldora abajo en el centro que se ajusta a sus acciones; cada acción, su tecla dibujada y su texto.
            var panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(root.transform, false);
            var panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, 140f);
            var background = panel.AddComponent<Image>();
            background.sprite = UiKit.HudPlateSprite();
            background.type = Image.Type.Sliced;
            background.raycastTarget = false;
            var row = panel.AddComponent<HorizontalLayoutGroup>();
            row.padding = new RectOffset(22, 26, 10, 10);
            row.spacing = 30f;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = row.childControlHeight = true;
            row.childForceExpandWidth = row.childForceExpandHeight = false;
            var fitter = panel.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var slots = new GameObject[MAX_ACTIONS];
            var keys = new GameObject[MAX_ACTIONS];
            var keyLabels = new TextMeshProUGUI[MAX_ACTIONS];
            var actions = new TextMeshProUGUI[MAX_ACTIONS];
            for (var i = 0; i < MAX_ACTIONS; i++)
            {
                slots[i] = UiKit.KeyHint($"Action{i}", panel.transform, "E", "Interactuar", 28f, out keyLabels[i], out actions[i]);
                keys[i] = keyLabels[i].transform.parent.gameObject;
            }

            var view = root.AddComponent<InteractionPromptView>();
            SerializedWiring.Set(view, "_promptChannel", PromptChannel());
            SerializedWiring.Set(view, "_panel", panel);
            SerializedWiring.SetArray(view, "_slots", slots);
            SerializedWiring.SetArray(view, "_keys", keys);
            SerializedWiring.SetArray(view, "_keyLabels", keyLabels);
            SerializedWiring.SetArray(view, "_actions", actions);

            return EditorAssets.SaveAsPrefab(root, HUD_PREFAB_PATH);
        }

        /// <summary>Extensión para el prefab del concursante (se pasa a PlayerPrefabBuilder.Build).</summary>
        public static void AttachInteractor(GameObject playerRoot, InteractableRegistry registry,
            InputActionReference primary, InputActionReference secondary, InputActionReference tertiary)
        {
            var interactor = playerRoot.AddComponent<PlayerInteractor>();
            SerializedWiring.Set(interactor, "_registry", registry);
            SerializedWiring.Set(interactor, "_avatars", SharedAssets.PlayerAvatars());
            SerializedWiring.Set(interactor, "_promptChannel", PromptChannel());
            SerializedWiring.Set(interactor, "_primaryAction", primary);
            SerializedWiring.Set(interactor, "_secondaryAction", secondary);
            SerializedWiring.Set(interactor, "_tertiaryAction", tertiary);
            interactor.enabled = false;
        }
    }
}
