using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Trivias.Match.Data;
using Trivias.Menu.Components;
using Trivias.Menu.Data;
using Trivias.Shared.Editor;
using Trivias.Shared.Settings;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Menu.Editor
{
    /// <summary>
    /// Construye el menú: principal (crear, unirme, cómo se juega, configuración, salir), unirme a una sala,
    /// sala de espera (con «Invita a tus amigos»), tutorial, configuración y menú de pausa (Esc).
    /// </summary>
    public static class MenuPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Menu";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/Menu.prefab";
        public const string HOW_TO_PLAY_JSON = MODULE_ROOT + "/Content/howtoplay_es.json";
        public const string HOW_TO_PLAY_ASSET = MODULE_ROOT + "/Config/HowToPlay.asset";

        private static readonly Color PanelColor = new(0.05f, 0.06f, 0.16f, 0.96f);
        private static readonly Color BoxColor = new(1f, 1f, 1f, 0.06f);
        private static readonly Color Dim = new(0f, 0f, 0.03f, 0.72f);
        private static readonly Color Gold = new(0.95f, 0.70f, 0.22f);
        private static readonly Color DarkText = new(0.08f, 0.08f, 0.18f);
        private static readonly Color Muted = new(1f, 1f, 1f, 0.7f);
        private static readonly Color Secondary = new(1f, 1f, 1f, 0.15f);

        [Serializable]
        private sealed class PageJson
        {
            public string title;
            public string body;
        }

        [Serializable]
        private sealed class HowToPlayJson
        {
            public string language;
            public PageJson[] pages;
        }

        /// <summary>Importa el tutorial del JSON (tiempo de edición). La validación está en las pruebas de contenido.</summary>
        public static HowToPlayContent BuildHowToPlay()
        {
            var json = JsonUtility.FromJson<HowToPlayJson>(File.ReadAllText(HOW_TO_PLAY_JSON));
            var pages = new List<HowToPlayPage>();
            foreach (var page in json.pages)
                pages.Add(new HowToPlayPage(page.title, page.body));

            var content = EditorAssets.GetOrCreateAsset<HowToPlayContent>(HOW_TO_PLAY_ASSET);
            content.EditorSet(pages);
            EditorUtility.SetDirty(content);
            return content;
        }

        public static GameObject Build(MatchRuntime match)
        {
            var howToPlayContent = BuildHowToPlay();
            var settings = SharedAssets.GameSettings();

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
            var main = CreatePanel("MainMenu", root.transform, new Vector2(620f, 700f));
            CreateLabel("Title", main, "TRIVIAS", 84f, new Vector2(0f, -40f), new Vector2(560f, 100f), FontStyles.Bold, Gold);
            CreateLabel("Subtitle", main, "El concurso donde tus amigos también son la apuesta", 24f,
                new Vector2(0f, -140f), new Vector2(560f, 40f), FontStyles.Italic, Color.white);
            var host = CreateButton("Host", main, "Crear sala", new Vector2(0f, -210f));
            var joinMenu = CreateButton("JoinMenu", main, "Unirme a una sala", new Vector2(0f, -300f));
            var howToPlay = CreateButton("HowToPlay", main, "Cómo se juega", new Vector2(0f, -400f), secondary: true);
            var settingsButton = CreateButton("Settings", main, "Configuración", new Vector2(0f, -470f), secondary: true);
            var quit = CreateButton("Quit", main, "Salir", new Vector2(0f, -540f), secondary: true);
            var mainStatus = CreateLabel("Status", main, string.Empty, 22f, new Vector2(0f, -620f), new Vector2(560f, 60f),
                FontStyles.Normal, Color.white);

            // ---- Unirme a una sala ----
            var join = CreatePanel("JoinMenu", root.transform, new Vector2(620f, 500f));
            CreateLabel("Title", join, "UNIRME A UNA SALA", 48f, new Vector2(0f, -40f), new Vector2(560f, 64f), FontStyles.Bold, Gold);
            CreateLabel("AddressLabel", join, "Dirección del host", 24f, new Vector2(0f, -135f), new Vector2(420f, 32f),
                FontStyles.Normal, Muted);
            var address = CreateInputField("Address", join, new Vector2(0f, -175f));
            var connect = CreateButton("Connect", join, "Conectar", new Vector2(0f, -255f));
            var joinBack = CreateButton("Back", join, "Volver", new Vector2(0f, -345f), secondary: true);
            var joinStatus = CreateLabel("Status", join, string.Empty, 22f, new Vector2(0f, -415f), new Vector2(560f, 70f),
                FontStyles.Normal, Color.white);

            // ---- Sala de espera: jugadores a la izquierda, invitación a la derecha ----
            var lobby = CreatePanel("Lobby", root.transform, new Vector2(1060f, 760f));
            CreateLabel("Title", lobby, "SALA DE ESPERA", 54f, new Vector2(0f, -36f), new Vector2(1000f, 70f), FontStyles.Bold, Gold);
            var players = CreateLabel("Players", lobby, string.Empty, 32f, new Vector2(-250f, -130f), new Vector2(480f, 270f),
                FontStyles.Normal, Color.white);
            var lobbyInfo = CreateLabel("Info", lobby, string.Empty, 22f, new Vector2(-250f, -405f), new Vector2(480f, 60f),
                FontStyles.Italic, new Color(1f, 1f, 1f, 0.75f));
            var start = CreateButton("Start", lobby, "Empezar partida", new Vector2(-250f, -475f));
            var shop = CreateButton("Shop", lobby, "Tienda", new Vector2(-250f, -565f), secondary: true);
            var lobbyHowToPlay = CreateButton("HowToPlay", lobby, "Cómo se juega", new Vector2(-250f, -630f), secondary: true);
            var leave = CreateButton("Leave", lobby, "Salir", new Vector2(-250f, -695f), secondary: true);

            var invite = CreateRect("Invite", lobby, new Vector2(0.5f, 1f), new Vector2(255f, -130f), new Vector2(460f, 560f));
            invite.gameObject.AddComponent<Image>().color = BoxColor;
            CreateLabel("Title", invite, "INVITA A TUS AMIGOS", 32f, new Vector2(0f, -24f), new Vector2(420f, 44f), FontStyles.Bold, Gold);
            var inviteText = CreateLabel("Text", invite, string.Empty, 26f, new Vector2(0f, -86f), new Vector2(420f, 330f),
                FontStyles.Normal, Color.white);
            var copy = CreateButton("Copy", invite, "Copiar", new Vector2(0f, -440f), out var copyLabel);

            // ---- Pausa (Esc durante la partida) ----
            var pause = CreateOverlay("Pause", root.transform, new Vector2(560f, 560f), out var pausePanel);
            CreateLabel("Title", pausePanel, "PAUSA", 60f, new Vector2(0f, -36f), new Vector2(500f, 76f), FontStyles.Bold, Gold);
            CreateLabel("Subtitle", pausePanel, "La partida sigue para los demás", 22f, new Vector2(0f, -116f),
                new Vector2(500f, 32f), FontStyles.Italic, Muted);
            var resume = CreateButton("Resume", pausePanel, "Continuar", new Vector2(0f, -170f));
            var pauseHowToPlay = CreateButton("HowToPlay", pausePanel, "Cómo se juega", new Vector2(0f, -270f), secondary: true);
            var pauseSettings = CreateButton("Settings", pausePanel, "Configuración", new Vector2(0f, -340f), secondary: true);
            var pauseLeave = CreateButton("Leave", pausePanel, "Salir de la sala", new Vector2(0f, -410f), secondary: true);

            var confirm = CreateOverlay("ConfirmLeave", pause.transform, new Vector2(620f, 330f), out var confirmPanel);
            CreateLabel("Title", confirmPanel, "¿Salir de la sala?", 42f, new Vector2(0f, -36f), new Vector2(560f, 56f), FontStyles.Bold, Color.white);
            CreateLabel("Text", confirmPanel, "Si eres el host, la partida termina para todos.", 24f, new Vector2(0f, -108f),
                new Vector2(560f, 60f), FontStyles.Normal, Muted);
            var confirmLeave = CreateButton("Confirm", confirmPanel, "Sí, salir", new Vector2(0f, -180f));
            var cancelLeave = CreateButton("Cancel", confirmPanel, "Cancelar", new Vector2(0f, -265f), secondary: true);

            // ---- Cómo se juega ----
            var tutorial = CreateOverlay("HowToPlay", root.transform, new Vector2(1180f, 780f), out var tutorialPanel);
            var tutorialTitle = CreateLabel("Title", tutorialPanel, string.Empty, 52f, new Vector2(0f, -36f), new Vector2(1000f, 70f),
                FontStyles.Bold, Gold);
            var tutorialBody = CreateLabel("Body", tutorialPanel, string.Empty, 29f, new Vector2(0f, -130f), new Vector2(1060f, 480f),
                FontStyles.Normal, Color.white);
            tutorialBody.alignment = TextAlignmentOptions.TopLeft;
            var pageLabel = CreateLabel("Page", tutorialPanel, string.Empty, 24f, new Vector2(0f, -640f), new Vector2(200f, 32f),
                FontStyles.Normal, Muted);
            var previous = CreateButton("Previous", tutorialPanel, "Anterior", new Vector2(-360f, -680f), secondary: true);
            var next = CreateButton("Next", tutorialPanel, "Siguiente", new Vector2(330f, -660f), out var nextLabel);
            var tutorialClose = CreateButton("Close", tutorialPanel, "Cerrar", new Vector2(450f, -30f), secondary: true, width: 180f);

            // ---- Configuración ----
            var settingsOverlay = CreateOverlay("Settings", root.transform, new Vector2(820f, 720f), out var settingsPanel);
            CreateLabel("Title", settingsPanel, "CONFIGURACIÓN", 52f, new Vector2(0f, -36f), new Vector2(760f, 70f), FontStyles.Bold, Gold);
            var master = CreateSliderRow(settingsPanel, "MasterVolume", "Volumen general", -140f, 0f, 1f, out var masterValue);
            var music = CreateSliderRow(settingsPanel, "MusicVolume", "Música", -215f, 0f, 1f, out var musicValue);
            var effects = CreateSliderRow(settingsPanel, "EffectsVolume", "Efectos", -290f, 0f, 1f, out var effectsValue);
            var sensitivity = CreateSliderRow(settingsPanel, "MouseSensitivity", "Sensibilidad del ratón", -365f,
                SettingsRules.MIN_SENSITIVITY, SettingsRules.MAX_SENSITIVITY, out var sensitivityValue);
            var invertY = CreateToggleRow(settingsPanel, "InvertY", "Invertir eje vertical del ratón", -445f);
            var fullscreen = CreateToggleRow(settingsPanel, "Fullscreen", "Pantalla completa", -515f);
            var reset = CreateButton("Reset", settingsPanel, "Restablecer", new Vector2(-200f, -600f), secondary: true, width: 300f);
            var settingsClose = CreateButton("Close", settingsPanel, "Guardar y cerrar", new Vector2(170f, -590f), width: 340f);

            // ---- Vistas ----
            var howToPlayView = root.AddComponent<HowToPlayView>();
            SerializedWiring.Set(howToPlayView, "_content", howToPlayContent);
            SerializedWiring.Set(howToPlayView, "_panel", tutorial);
            SerializedWiring.Set(howToPlayView, "_title", tutorialTitle);
            SerializedWiring.Set(howToPlayView, "_body", tutorialBody);
            SerializedWiring.Set(howToPlayView, "_pageLabel", pageLabel);
            SerializedWiring.Set(howToPlayView, "_previousButton", previous);
            SerializedWiring.Set(howToPlayView, "_nextButton", next);
            SerializedWiring.Set(howToPlayView, "_nextLabel", nextLabel);
            SerializedWiring.Set(howToPlayView, "_closeButton", tutorialClose);

            var settingsView = root.AddComponent<SettingsView>();
            SerializedWiring.Set(settingsView, "_settings", settings);
            SerializedWiring.Set(settingsView, "_panel", settingsOverlay);
            SerializedWiring.Set(settingsView, "_masterVolume", master);
            SerializedWiring.Set(settingsView, "_musicVolume", music);
            SerializedWiring.Set(settingsView, "_effectsVolume", effects);
            SerializedWiring.Set(settingsView, "_mouseSensitivity", sensitivity);
            SerializedWiring.Set(settingsView, "_masterValue", masterValue);
            SerializedWiring.Set(settingsView, "_musicValue", musicValue);
            SerializedWiring.Set(settingsView, "_effectsValue", effectsValue);
            SerializedWiring.Set(settingsView, "_sensitivityValue", sensitivityValue);
            SerializedWiring.Set(settingsView, "_invertY", invertY);
            SerializedWiring.Set(settingsView, "_fullscreen", fullscreen);
            SerializedWiring.Set(settingsView, "_resetButton", reset);
            SerializedWiring.Set(settingsView, "_closeButton", settingsClose);

            var pauseView = root.AddComponent<PauseMenuView>();
            SerializedWiring.Set(pauseView, "_match", match);
            SerializedWiring.Set(pauseView, "_controlGate", SharedAssets.PlayerControlGate());
            SerializedWiring.Set(pauseView, "_leaveRoomRequested", SharedAssets.LeaveRoomRequested());
            SerializedWiring.Set(pauseView, "_howToPlay", howToPlayView);
            SerializedWiring.Set(pauseView, "_settingsView", settingsView);
            SerializedWiring.Set(pauseView, "_panel", pause);
            SerializedWiring.Set(pauseView, "_resumeButton", resume);
            SerializedWiring.Set(pauseView, "_howToPlayButton", pauseHowToPlay);
            SerializedWiring.Set(pauseView, "_settingsButton", pauseSettings);
            SerializedWiring.Set(pauseView, "_leaveButton", pauseLeave);
            SerializedWiring.Set(pauseView, "_confirmPanel", confirm);
            SerializedWiring.Set(pauseView, "_confirmLeaveButton", confirmLeave);
            SerializedWiring.Set(pauseView, "_cancelLeaveButton", cancelLeave);

            var view = root.AddComponent<MenuView>();
            SerializedWiring.Set(view, "_room", SharedAssets.RoomConnection());
            SerializedWiring.Set(view, "_match", match);
            SerializedWiring.Set(view, "_avatars", SharedAssets.PlayerAvatars());
            SerializedWiring.Set(view, "_profile", SharedAssets.PlayerProfile());
            SerializedWiring.Set(view, "_settings", settings);
            SerializedWiring.Set(view, "_openShopRequested", SharedAssets.OpenShopRequested());
            SerializedWiring.Set(view, "_howToPlay", howToPlayView);
            SerializedWiring.Set(view, "_settingsView", settingsView);
            SerializedWiring.Set(view, "_mainPanel", main.gameObject);
            SerializedWiring.Set(view, "_hostButton", host);
            SerializedWiring.Set(view, "_joinMenuButton", joinMenu);
            SerializedWiring.Set(view, "_howToPlayButton", howToPlay);
            SerializedWiring.Set(view, "_settingsButton", settingsButton);
            SerializedWiring.Set(view, "_quitButton", quit);
            SerializedWiring.Set(view, "_mainStatus", mainStatus);
            SerializedWiring.Set(view, "_joinPanel", join.gameObject);
            SerializedWiring.Set(view, "_address", address);
            SerializedWiring.Set(view, "_connectButton", connect);
            SerializedWiring.Set(view, "_joinBackButton", joinBack);
            SerializedWiring.Set(view, "_joinStatus", joinStatus);
            SerializedWiring.Set(view, "_lobbyPanel", lobby.gameObject);
            SerializedWiring.Set(view, "_players", players);
            SerializedWiring.Set(view, "_lobbyInfo", lobbyInfo);
            SerializedWiring.Set(view, "_invitePanel", invite.gameObject);
            SerializedWiring.Set(view, "_inviteText", inviteText);
            SerializedWiring.Set(view, "_copyButton", copy);
            SerializedWiring.Set(view, "_copyLabel", copyLabel);
            SerializedWiring.Set(view, "_startButton", start);
            SerializedWiring.Set(view, "_shopButton", shop);
            SerializedWiring.Set(view, "_lobbyHowToPlayButton", lobbyHowToPlay);
            SerializedWiring.Set(view, "_leaveButton", leave);

            join.gameObject.SetActive(false);
            lobby.gameObject.SetActive(false);
            pause.SetActive(false);
            confirm.SetActive(false);
            tutorial.SetActive(false);
            settingsOverlay.SetActive(false);
            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }

        // ---- Utilidades de interfaz ----

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

        /// <summary>Capa a pantalla completa que oscurece y bloquea lo de detrás, con un panel centrado.</summary>
        private static GameObject CreateOverlay(string name, Transform parent, Vector2 size, out RectTransform panel)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = Dim;
            panel = CreatePanel("Panel", rect, size);
            return go;
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

        private static Button CreateButton(string name, Transform parent, string caption, Vector2 position,
            bool secondary = false, float width = 0f) => CreateButton(name, parent, caption, position, out _, secondary, width);

        private static Button CreateButton(string name, Transform parent, string caption, Vector2 position,
            out TextMeshProUGUI label, bool secondary = false, float width = 0f)
        {
            var size = secondary ? new Vector2(width > 0f ? width : 380f, 56f) : new Vector2(width > 0f ? width : 380f, 72f);
            var rect = CreateRect(name, parent, new Vector2(0.5f, 1f), position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = secondary ? Secondary : Gold;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            label = CreateLabel("Label", rect, caption, secondary ? 26f : 32f, new Vector2(0f, secondary ? -10f : -14f),
                new Vector2(size.x - 20f, size.y - 20f), FontStyles.Bold, secondary ? Color.white : DarkText);
            return button;
        }

        /// <summary>Fila «Nombre ———o——— 80%» de la configuración.</summary>
        private static Slider CreateSliderRow(RectTransform parent, string name, string caption, float y, float min, float max,
            out TextMeshProUGUI value)
        {
            var row = CreateRect(name, parent, new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(740f, 56f));
            var label = CreateLabel("Label", row, caption, 26f, new Vector2(-215f, -10f), new Vector2(300f, 36f), FontStyles.Normal, Color.white);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            value = CreateLabel("Value", row, string.Empty, 26f, new Vector2(320f, -10f), new Vector2(100f, 36f), FontStyles.Bold, Gold);

            var sliderRect = CreateRect("Slider", row, new Vector2(0.5f, 1f), new Vector2(85f, -18f), new Vector2(330f, 20f));
            var background = sliderRect.gameObject.AddComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0.18f);

            var fillArea = Stretch("FillArea", sliderRect, new Vector2(4f, 4f), new Vector2(-4f, -4f));
            var fill = Stretch("Fill", fillArea, Vector2.zero, Vector2.zero);
            fill.gameObject.AddComponent<Image>().color = Gold;

            var handleArea = Stretch("HandleArea", sliderRect, new Vector2(10f, 0f), new Vector2(-10f, 0f));
            var handle = CreateRect("Handle", handleArea, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(26f, 34f));
            var handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.color = Color.white;

            var slider = sliderRect.gameObject.AddComponent<Slider>();
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = max;
            return slider;
        }

        /// <summary>Fila «[✓] Nombre» de la configuración.</summary>
        private static Toggle CreateToggleRow(RectTransform parent, string name, string caption, float y)
        {
            var row = CreateRect(name, parent, new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(740f, 52f));
            var box = CreateRect("Box", row, new Vector2(0.5f, 1f), new Vector2(-345f, -6f), new Vector2(40f, 40f));
            var background = box.gameObject.AddComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0.9f);
            var check = Stretch("Check", box, new Vector2(8f, 8f), new Vector2(-8f, -8f));
            var checkImage = check.gameObject.AddComponent<Image>();
            checkImage.color = Gold;

            var label = CreateLabel("Label", row, caption, 26f, new Vector2(30f, -8f), new Vector2(640f, 36f), FontStyles.Normal, Color.white);
            label.alignment = TextAlignmentOptions.MidlineLeft;

            var toggle = row.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = background;
            toggle.graphic = checkImage;
            toggle.isOn = false;
            return toggle;
        }

        private static RectTransform Stretch(string name, Transform parent, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        /// <summary>Campo de texto TMP (área con máscara, texto y marcador de posición).</summary>
        private static TMP_InputField CreateInputField(string name, Transform parent, Vector2 position)
        {
            var rect = CreateRect(name, parent, new Vector2(0.5f, 1f), position, new Vector2(420f, 60f));
            var background = rect.gameObject.AddComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0.92f);

            var area = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
            area.transform.SetParent(rect, false);
            var areaRect = (RectTransform)area.transform;
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(14f, 6f);
            areaRect.offsetMax = new Vector2(-14f, -6f);

            var placeholder = CreateFill("Placeholder", areaRect, "p. ej. 10.147.17.5", new Color(0f, 0f, 0f, 0.35f));
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
            var rect = Stretch(name, parent, Vector2.zero, Vector2.zero);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
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
