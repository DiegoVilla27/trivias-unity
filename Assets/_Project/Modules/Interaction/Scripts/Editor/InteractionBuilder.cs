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

            var panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(root.transform, false);
            var panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, 140f);
            panelRect.sizeDelta = new Vector2(1100f, 64f);
            var background = panel.AddComponent<Image>();
            background.color = new Color(0.05f, 0.05f, 0.12f, 0.75f);
            background.raycastTarget = false;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(panel.transform, false);
            var labelRect = (RectTransform)labelGo.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(16f, 0f);
            labelRect.offsetMax = new Vector2(-16f, 0f);
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.fontSize = 30f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;
            label.text = "E: interactuar";

            var view = root.AddComponent<InteractionPromptView>();
            SerializedWiring.Set(view, "_promptChannel", PromptChannel());
            SerializedWiring.Set(view, "_label", label);
            SerializedWiring.Set(view, "_panel", panel);

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
