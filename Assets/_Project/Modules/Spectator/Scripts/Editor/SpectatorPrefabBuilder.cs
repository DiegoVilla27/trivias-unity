using TMPro;
using Trivias.Shared.Editor;
using Trivias.Spectator.Components;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Spectator.Editor
{
    /// <summary>Construye el espectador: el controlador y su rótulo abajo en el centro (no bloquea clics).</summary>
    public static class SpectatorPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Spectator";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/Spectator.prefab";

        public static GameObject Build()
        {
            var root = new GameObject("Spectator", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 15;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var banner = new GameObject("Banner", typeof(RectTransform));
            banner.transform.SetParent(root.transform, false);
            var bannerRect = (RectTransform)banner.transform;
            bannerRect.anchorMin = bannerRect.anchorMax = bannerRect.pivot = new Vector2(0.5f, 0f);
            bannerRect.anchoredPosition = new Vector2(0f, 36f);
            bannerRect.sizeDelta = new Vector2(1180f, 96f);
            var background = banner.AddComponent<Image>();
            background.color = new Color(0.05f, 0.06f, 0.16f, 0.85f);
            background.raycastTarget = false;

            var textObject = new GameObject("Text", typeof(RectTransform));
            textObject.transform.SetParent(bannerRect, false);
            var textRect = (RectTransform)textObject.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20f, 8f);
            textRect.offsetMax = new Vector2(-20f, -8f);
            var text = textObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = 28f;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.richText = true;
            text.raycastTarget = false;
            text.color = Color.white;

            var controller = root.AddComponent<SpectatorController>();
            SerializedWiring.Set(controller, "_avatars", SharedAssets.PlayerAvatars());
            SerializedWiring.Set(controller, "_cameraTarget", SharedAssets.LocalPlayerCameraTarget());
            SerializedWiring.Set(controller, "_controlGate", SharedAssets.PlayerControlGate());
            SerializedWiring.Set(controller, "_banner", banner);
            SerializedWiring.Set(controller, "_bannerText", text);
            SerializedWiring.Set(controller, "_startDelay", 2.5f);

            banner.SetActive(false);
            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }
    }
}
