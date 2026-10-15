using TMPro;
using Trivias.HUD.Components;
using Trivias.Match.Data;
using Trivias.SelfBet.Data;
using Trivias.Shared.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.HUD.Editor
{
    /// <summary>Construye la interfaz de la partida: panel de pregunta arriba y dinero del equipo arriba a la derecha.</summary>
    public static class HudPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/HUD";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/MatchHud.prefab";

        private static readonly Color PanelColor = new(0.05f, 0.06f, 0.16f, 0.82f);

        public static GameObject Build(MatchRuntime runtime, SelfBetRuntime selfBet)
        {
            var root = new GameObject("MatchHud", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            // La pregunta y las opciones se leen en la pantalla grande del plató; aquí solo el estado de la ronda.
            var panel = Panel("StatusPanel", root.transform, new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(760f, 118f));
            var status = Text("Status", panel, 28f, new Vector2(0f, -10f), new Vector2(720f, 100f), FontStyles.Normal);

            var moneyPanel = Panel("TeamMoney", root.transform, new Vector2(1f, 1f), new Vector2(-16f, -16f), new Vector2(300f, 92f));
            var money = Text("Amount", moneyPanel, 34f, new Vector2(0f, -10f), new Vector2(280f, 72f), FontStyles.Normal);

            var selfBetPanel = Panel("SelfBet", root.transform, new Vector2(1f, 1f), new Vector2(-16f, -120f), new Vector2(300f, 100f));
            var selfBetInfo = Text("Info", selfBetPanel, 26f, new Vector2(0f, -8f), new Vector2(280f, 86f), FontStyles.Normal);

            var view = root.AddComponent<MatchHudView>();
            SerializedWiring.Set(view, "_runtime", runtime);
            SerializedWiring.Set(view, "_statusPanel", panel.gameObject);
            SerializedWiring.Set(view, "_status", status);
            SerializedWiring.Set(view, "_teamMoney", money);
            SerializedWiring.Set(view, "_selfBet", selfBet);
            SerializedWiring.Set(view, "_avatars", SharedAssets.PlayerAvatars());
            SerializedWiring.Set(view, "_selfBetInfo", selfBetInfo);

            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }

        private static RectTransform Panel(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = go.AddComponent<Image>();
            image.color = PanelColor;
            image.raycastTarget = false;
            return rect;
        }

        private static TextMeshProUGUI Text(string name, Transform parent, float size, Vector2 position, Vector2 box, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = box;

            var text = go.AddComponent<TextMeshProUGUI>();
            text.fontSize = size;
            text.fontStyle = style;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.richText = true;
            text.raycastTarget = false;
            return text;
        }
    }
}
