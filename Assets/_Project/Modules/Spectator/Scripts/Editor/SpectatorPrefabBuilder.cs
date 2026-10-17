using TMPro;
using Trivias.Shared.Editor;
using Trivias.Spectator.Components;
using Trivias.Spectator.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Spectator.Editor
{
    /// <summary>Construye el espectador: el controlador y su cinta abajo en el centro (no bloquea clics).</summary>
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

            // Cinta abajo en el centro (boceto 07 del HUD): pestaña burdeos «ELIMINADO», a quién mira y sus teclas.
            var bottom = new GameObject("Bottom", typeof(RectTransform));
            bottom.transform.SetParent(root.transform, false);
            var bottomRect = (RectTransform)bottom.transform;
            bottomRect.anchorMin = bottomRect.anchorMax = bottomRect.pivot = new Vector2(0.5f, 0f);
            bottomRect.sizeDelta = new Vector2(1920f, 1080f);
            var banner = UiKit.Rect("Banner", bottomRect, 0f, 0f, 1920f, 1080f).gameObject;
            var ribbon = UiKit.Picture("Ribbon", banner.transform, UiKit.HudPlateSprite(), 560f, 830f, 800f, 132f);
            var tab = UiKit.Picture("Tab", ribbon.transform, UiKit.DangerSprite(), 270f, -28f, 260f, 58f);
            UiKit.Text("Title", tab.transform, SpectatorText.TITLE, 0f, 2f, 260f, 54f, 38f, UiKit.UiFont.Display, UiKit.Ivory);
            UiKit.Text("Subtitle", ribbon.transform, SpectatorText.SUBTITLE, 20f, 38f, 760f, 36f, 24f, UiKit.UiFont.Body, UiKit.Ivory);
            UiKit.Divider("Divider", ribbon.transform, 120f, 80f, 560f);
            var watching = UiKit.Text("Watching", ribbon.transform, string.Empty, 20f, 84f, 760f, 42f, 30f, UiKit.UiFont.Bold, UiKit.Ivory);

            var keys = UiKit.KeyRow("Keys", banner.transform, 460f, 976f, 1000f, 52f, 34f);
            var follow = UiKit.KeyHint("Follow", keys, "A / D", SpectatorText.FollowAction(true), 24f, out _, out var followAction);
            var overview = UiKit.KeyHint("Overview", keys, "Espacio", "Vista general", 24f, out _, out _);
            UiKit.KeyHint("Menu", keys, "Esc", "Menú", 24f, out _, out _);

            var controller = root.AddComponent<SpectatorController>();
            SerializedWiring.Set(controller, "_avatars", SharedAssets.PlayerAvatars());
            SerializedWiring.Set(controller, "_cameraTarget", SharedAssets.LocalPlayerCameraTarget());
            SerializedWiring.Set(controller, "_controlGate", SharedAssets.PlayerControlGate());
            SerializedWiring.Set(controller, "_banner", banner);
            SerializedWiring.Set(controller, "_watching", watching);
            SerializedWiring.Set(controller, "_followHint", follow);
            SerializedWiring.Set(controller, "_followAction", followAction);
            SerializedWiring.Set(controller, "_overviewHint", overview);
            SerializedWiring.Set(controller, "_startDelay", 2.5f);

            banner.SetActive(false);
            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }
    }
}
