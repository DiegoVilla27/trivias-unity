using TMPro;
using Trivias.Match.Data;
using Trivias.Menu.Components;
using Trivias.Shared.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Menu.Editor
{
    /// <summary>Construye el menú: pantalla principal (crear o unirse) y sala de espera (jugadores, empezar, salir).</summary>
    public static class MenuPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Menu";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/Menu.prefab";

        private static readonly Color PanelColor = new(0.05f, 0.06f, 0.16f, 0.94f);
        private static readonly Color Gold = new(0.95f, 0.70f, 0.22f);
        private static readonly Color DarkText = new(0.08f, 0.08f, 0.18f);

        public static GameObject Build(MatchRuntime match)
        {
            var root = new GameObject("Menu", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            // ---- Pantalla principal ----
            var main = CreatePanel("MainMenu", root.transform, new Vector2(620f, 560f));
            CreateLabel("Title", main, "TRIVIAS", 84f, new Vector2(0f, -40f), new Vector2(560f, 100f), FontStyles.Bold, Gold);
            CreateLabel("Subtitle", main, "El concurso donde tus amigos también son la apuesta", 24f,
                new Vector2(0f, -140f), new Vector2(560f, 40f), FontStyles.Italic, Color.white);
            var host = CreateButton("Host", main, "Crear sala", new Vector2(0f, -210f));
            CreateLabel("AddressLabel", main, "Dirección del host", 22f, new Vector2(0f, -310f), new Vector2(420f, 30f),
                FontStyles.Normal, new Color(1f, 1f, 1f, 0.7f));
            var address = CreateInputField("Address", main, new Vector2(0f, -345f));
            var join = CreateButton("Join", main, "Unirse", new Vector2(0f, -410f));
            var mainStatus = CreateLabel("Status", main, string.Empty, 22f, new Vector2(0f, -495f), new Vector2(560f, 50f),
                FontStyles.Normal, Color.white);

            // ---- Sala de espera ----
            var lobby = CreatePanel("Lobby", root.transform, new Vector2(620f, 600f));
            CreateLabel("Title", lobby, "SALA DE ESPERA", 54f, new Vector2(0f, -40f), new Vector2(560f, 70f), FontStyles.Bold, Gold);
            var players = CreateLabel("Players", lobby, string.Empty, 32f, new Vector2(0f, -130f), new Vector2(520f, 260f),
                FontStyles.Normal, Color.white);
            var lobbyInfo = CreateLabel("Info", lobby, string.Empty, 22f, new Vector2(0f, -400f), new Vector2(560f, 40f),
                FontStyles.Italic, new Color(1f, 1f, 1f, 0.75f));
            var start = CreateButton("Start", lobby, "Empezar partida", new Vector2(0f, -450f));
            var leave = CreateButton("Leave", lobby, "Salir", new Vector2(0f, -525f), small: true);

            var view = root.AddComponent<MenuView>();
            SerializedWiring.Set(view, "_room", SharedAssets.RoomConnection());
            SerializedWiring.Set(view, "_match", match);
            SerializedWiring.Set(view, "_avatars", SharedAssets.PlayerAvatars());
            SerializedWiring.Set(view, "_mainPanel", main.gameObject);
            SerializedWiring.Set(view, "_address", address);
            SerializedWiring.Set(view, "_hostButton", host);
            SerializedWiring.Set(view, "_joinButton", join);
            SerializedWiring.Set(view, "_mainStatus", mainStatus);
            SerializedWiring.Set(view, "_lobbyPanel", lobby.gameObject);
            SerializedWiring.Set(view, "_players", players);
            SerializedWiring.Set(view, "_lobbyInfo", lobbyInfo);
            SerializedWiring.Set(view, "_startButton", start);
            SerializedWiring.Set(view, "_leaveButton", leave);

            lobby.gameObject.SetActive(false);
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

        private static RectTransform CreatePanel(string name, Transform parent, Vector2 size)
        {
            var rect = CreateRect(name, parent, new Vector2(0.5f, 0.5f), Vector2.zero, size);
            rect.gameObject.AddComponent<Image>().color = PanelColor;
            return rect;
        }

        private static TextMeshProUGUI CreateLabel(string name, Transform parent, string text, float size, Vector2 position,
            Vector2 box, FontStyles style, Color color)
        {
            var rect = CreateRect(name, parent, new Vector2(0.5f, 1f), position, box);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.fontStyle = style;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.richText = true;
            label.raycastTarget = false;
            return label;
        }

        private static Button CreateButton(string name, Transform parent, string caption, Vector2 position, bool small = false)
        {
            var size = small ? new Vector2(220f, 52f) : new Vector2(380f, 72f);
            var rect = CreateRect(name, parent, new Vector2(0.5f, 1f), position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = small ? new Color(1f, 1f, 1f, 0.15f) : Gold;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            var label = CreateLabel("Label", rect, caption, small ? 24f : 32f, new Vector2(0f, small ? -10f : -14f),
                new Vector2(size.x - 20f, size.y - 20f), FontStyles.Bold, small ? Color.white : DarkText);
            label.alignment = TextAlignmentOptions.Center;
            return button;
        }

        /// <summary>Campo de texto TMP (área con máscara, texto y marcador de posición).</summary>
        private static TMP_InputField CreateInputField(string name, Transform parent, Vector2 position)
        {
            var rect = CreateRect(name, parent, new Vector2(0.5f, 1f), position, new Vector2(380f, 56f));
            var background = rect.gameObject.AddComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0.92f);

            var area = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
            area.transform.SetParent(rect, false);
            var areaRect = (RectTransform)area.transform;
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(14f, 6f);
            areaRect.offsetMax = new Vector2(-14f, -6f);

            var placeholder = CreateFill("Placeholder", areaRect, "127.0.0.1", new Color(0f, 0f, 0f, 0.35f));
            var text = CreateFill("Text", areaRect, string.Empty, DarkText);

            var field = rect.gameObject.AddComponent<TMP_InputField>();
            field.textViewport = areaRect;
            field.textComponent = text;
            field.placeholder = placeholder;
            field.targetGraphic = background;
            field.characterLimit = 64;
            return field;
        }

        private static TextMeshProUGUI CreateFill(string name, RectTransform parent, string text, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 28f;
            label.color = color;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.raycastTarget = false;
            return label;
        }
    }
}
