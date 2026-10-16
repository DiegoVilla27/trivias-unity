using TMPro;
using Trivias.Match.Data;
using Trivias.Ranking.Editor;
using Trivias.Results.Components;
using Trivias.Shared.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Results.Editor
{
    /// <summary>Construye la pantalla de resultados centrada, con los botones «Volver a jugar» y «Salir de la sala».</summary>
    public static class ResultsPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Results";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/ResultsScreen.prefab";

        public static GameObject Build(MatchRuntime match)
        {
            var root = new GameObject("ResultsScreen", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            var panel = CreateRect("Panel", root.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 640f));
            var background = panel.gameObject.AddComponent<Image>();
            background.color = new Color(0.05f, 0.06f, 0.16f, 0.94f);

            var title = CreateText("Title", panel, 64f, new Vector2(0f, -30f), new Vector2(700f, 80f), FontStyles.Bold);
            UiTheme.AddMarquee(panel); // Bombillas alrededor del resultado, como en el plató.
            var details = CreateText("Details", panel, 28f, new Vector2(0f, -125f), new Vector2(680f, 400f), FontStyles.Normal);

            var playAgain = CreateButton("PlayAgain", panel, new Vector2(-170f, -540f), "Volver a jugar", out var playAgainLabel);
            var leave = CreateButton("Leave", panel, new Vector2(170f, -540f), "Salir de la sala", out _);

            var view = root.AddComponent<ResultsView>();
            SerializedWiring.Set(view, "_match", match);
            SerializedWiring.Set(view, "_leaveRoomRequested", SharedAssets.LeaveRoomRequested());
            SerializedWiring.Set(view, "_profile", SharedAssets.PlayerProfile());
            SerializedWiring.Set(view, "_records", RankingBuilder.LocalRecords());
            SerializedWiring.Set(view, "_panel", panel.gameObject);
            SerializedWiring.Set(view, "_title", title);
            SerializedWiring.Set(view, "_details", details);
            SerializedWiring.Set(view, "_playAgainButton", playAgain);
            SerializedWiring.Set(view, "_playAgainLabel", playAgainLabel);
            SerializedWiring.Set(view, "_leaveButton", leave);
            SerializedWiring.Set(view, "_victoryCelebrationSeconds", 5f);

            panel.gameObject.SetActive(false);
            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, float size, Vector2 position, Vector2 box, FontStyles style)
        {
            var rect = CreateRect(name, parent, new Vector2(0.5f, 1f), position, box);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.richText = true;
            text.raycastTarget = false;
            text.color = Color.white;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, Vector2 position, string caption, out TextMeshProUGUI label)
        {
            var rect = CreateRect(name, parent, new Vector2(0.5f, 1f), position, new Vector2(300f, 70f));
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.95f, 0.70f, 0.22f);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            label = CreateText("Label", rect, 30f, new Vector2(0f, -14f), new Vector2(280f, 44f), FontStyles.Bold);
            label.color = new Color(0.08f, 0.08f, 0.18f);
            label.text = caption;
            return button;
        }
    }
}
