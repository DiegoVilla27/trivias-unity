using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Trivias.Match.Data;
using Trivias.Menu.Components;
using Trivias.Menu.Data;
using Trivias.Ranking.Editor;
using Trivias.Ranking.Logic;
using Trivias.Shared.Editor;
using Trivias.Shared.Presentation;
using Trivias.Shared.Profile;
using Trivias.Shared.Settings;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static Trivias.Shared.Editor.UiKit;

namespace Trivias.Menu.Editor
{
    /// <summary>
    /// Construye el menú tal como los bocetos de <c>docs/bocetos/ui/menu</c> (medidas a 1920 × 1080): principal con tu
    /// personaje, unirme a una sala, sala de espera, cómo se juega, récords, configuración, pausa y confirmación de salida.
    /// Detrás de todo, el plató real desenfocado (<see cref="MenuBackdropView"/>).
    /// </summary>
    public static class MenuPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Menu";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/Menu.prefab";
        public const string HOW_TO_PLAY_JSON = MODULE_ROOT + "/Content/howtoplay_es.json";
        public const string HOW_TO_PLAY_ASSET = MODULE_ROOT + "/Config/HowToPlay.asset";
        private const string BLUR_PROFILE = MODULE_ROOT + "/Config/MenuBlur.asset";
        private const string TITLES = MODULE_ROOT + "/UI/Titles";
        private const string ILLUSTRATIONS = MODULE_ROOT + "/UI/Illustrations";

        /// <summary>Plazas de la sala de espera (máximo de jugadores).</summary>
        public const int SEATS = 6;
        /// <summary>Filas de la tabla de récords (los diez mejores).</summary>
        public const int RECORD_ROWS = RankingRules.TABLE_SIZE;

        private static readonly Color Veil = new(0.02f, 0.04f, 0.1f, 0.28f);
        /// <summary>Teclas que puede dibujar una página del tutorial (la de controles usa once).</summary>
        private const int TUTORIAL_KEYS = 14;
        private static readonly Color OverlayVeil = new(0.02f, 0.04f, 0.1f, 0.45f);

        private static readonly string[] TutorialArt =
            { "Tutorial_1_Goal", "Tutorial_2_Money", "Tutorial_3_Verify", "Tutorial_4_SelfBet", "Tutorial_5_Tokens", "Tutorial_6_Controls" };

        [Serializable]
        private sealed class PageJson
        {
            public string step;
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
                pages.Add(new HowToPlayPage(page.step, page.title, page.body));

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

            // ---- Fondo: el plató desenfocado y un velo suave ----
            var backdropView = BuildBackdrop(root);

            var main = BuildMain(root.transform, out var mainParts);
            var join = BuildJoin(root.transform, out var joinParts);
            var lobby = BuildLobby(root.transform, out var lobbyParts);
            var tutorial = BuildHowToPlayOverlay(root, howToPlayContent);
            var records = BuildRecordsOverlay(root);
            var settingsView = BuildSettingsOverlay(root, settings);
            var pauseView = BuildPause(root, match);
            // Lo que se abre desde la pausa (cómo se juega, configuración) va por encima de ella.
            var pauseIndex = root.transform.Find("Pause").GetSiblingIndex();
            root.transform.Find("ConfirmLeave").SetSiblingIndex(pauseIndex + 1);
            foreach (var overlay in new[] { "HowToPlay", "Records", "Settings" })
                root.transform.Find(overlay).SetAsLastSibling();

            var view = root.AddComponent<MenuView>();
            SerializedWiring.Set(view, "_room", SharedAssets.RoomConnection());
            SerializedWiring.Set(view, "_match", match);
            SerializedWiring.Set(view, "_avatars", SharedAssets.PlayerAvatars());
            SerializedWiring.Set(view, "_profile", SharedAssets.PlayerProfile());
            SerializedWiring.Set(view, "_characters", SharedAssets.CharacterCatalog());
            SerializedWiring.Set(view, "_preview", SharedAssets.CharacterPreview());
            SerializedWiring.Set(view, "_backdrop", SharedAssets.UiBackdrop());
            SerializedWiring.Set(view, "_settings", settings);
            SerializedWiring.Set(view, "_openShopRequested", SharedAssets.OpenShopRequested());
            SerializedWiring.Set(view, "_howToPlay", tutorial);
            SerializedWiring.Set(view, "_settingsView", settingsView);
            SerializedWiring.Set(view, "_recordsView", records);
            SerializedWiring.Set(view, "_pause", pauseView);
            SerializedWiring.Set(view, "_mainPanel", main.gameObject);
            foreach (var (field, value) in mainParts)
                SerializedWiring.Set(view, field, value);
            SerializedWiring.Set(view, "_joinPanel", join.gameObject);
            foreach (var (field, value) in joinParts)
                SerializedWiring.Set(view, field, value);
            SerializedWiring.Set(view, "_lobbyPanel", lobby.gameObject);
            foreach (var (field, value) in lobbyParts)
                SerializedWiring.Set(view, field, value);
            SerializedWiring.SetArray(view, "_rows", lobby.GetComponentsInChildren<LobbyPlayerRow>(true));

            join.gameObject.SetActive(false);
            lobby.gameObject.SetActive(false);
            backdropView.transform.Find("Backdrop").gameObject.SetActive(false);
            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }

        // ---- Fondo ----

        private static MenuBackdropView BuildBackdrop(GameObject root)
        {
            var dim = FullScreen("Backdrop", root.transform);
            // Solo oscurece: no atrapa clics (los resultados, en otro lienzo, se pulsan aunque el velo esté encima).
            var veil = dim.gameObject.AddComponent<Image>();
            veil.color = Veil;
            veil.raycastTarget = false;

            var volume = root.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 50f;
            volume.weight = 0f;
            volume.sharedProfile = BlurProfile();

            var view = root.AddComponent<MenuBackdropView>();
            SerializedWiring.Set(view, "_backdrop", SharedAssets.UiBackdrop());
            SerializedWiring.Set(view, "_dim", dim.gameObject);
            SerializedWiring.Set(view, "_blur", volume);
            return view;
        }

        /// <summary>Desenfoque gaussiano de todo el plató (como el fondo de los bocetos).</summary>
        private static VolumeProfile BlurProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(BLUR_PROFILE);
            if (profile == null)
            {
                EditorAssets.EnsureFolder(Path.GetDirectoryName(BLUR_PROFILE));
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, BLUR_PROFILE);
            }
            if (!profile.TryGet<DepthOfField>(out var blur))
            {
                blur = profile.Add<DepthOfField>(true);
                blur.name = nameof(DepthOfField);
                AssetDatabase.AddObjectToAsset(blur, profile);
            }
            blur.active = true;
            blur.mode.Override(DepthOfFieldMode.Gaussian);
            blur.gaussianStart.Override(0f);
            blur.gaussianEnd.Override(2.5f);
            blur.gaussianMaxRadius.Override(1.5f);
            blur.highQualitySampling.Override(true);
            EditorUtility.SetDirty(blur);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        // ---- Principal (01-main-menu) ----

        private static RectTransform BuildMain(Transform root, out List<(string, UnityEngine.Object)> parts)
        {
            var screen = UiKit.Screen("MainScreen", root);
            Picture("Title", screen, Title("Trivias"), 214f, 14f, 600f, 199f, preserveAspect: true);

            var menu = Panel("MainMenu", screen, 209f, 218f, 666f, 776f);
            Text("NameLabel", menu, "TU NOMBRE", 54f, 40f, 400f, 34f, 25f, UiFont.Bold, Ivory, TextAlignmentOptions.MidlineLeft);
            var playerName = InputField("PlayerName", menu, 57f, 80f, 549f, 75f, "Escribe tu nombre", PlayerNameRules.MAX_LENGTH, 34f);
            var nameHint = Text("NameHint", menu, string.Empty, 57f, 162f, 549f, 30f, 21f, UiFont.Body, Muted, TextAlignmentOptions.MidlineLeft);
            var host = MakeButton("Host", menu, "Crear sala", 34f, 214f, 597f, 101f, ButtonStyle.Primary, 46f);
            var joinMenu = MakeButton("JoinMenu", menu, "Unirme a una sala", 40f, 338f, 584f, 76f, ButtonStyle.Secondary, 34f);
            var howToPlay = MakeButton("HowToPlay", menu, "Cómo se juega", 46f, 441f, 572f, 71f, ButtonStyle.Quiet, 28f);
            var records = MakeButton("Records", menu, "Récords", 46f, 521f, 572f, 71f, ButtonStyle.Quiet, 28f);
            var settings = MakeButton("Settings", menu, "Configuración", 46f, 602f, 572f, 71f, ButtonStyle.Quiet, 28f);
            var quit = MakeButton("Quit", menu, "Salir", 233f, 684f, 200f, 50f, ButtonStyle.Text, 28f);
            var status = Text("Status", menu, string.Empty, 40f, 735f, 586f, 32f, 21f, UiFont.Body, Ivory);

            BuildCharacterPanel(screen);
            Text("Footer", screen, "2–6 jugadores  ·  15 preguntas", 560f, 1024f, 800f, 34f, 24f, UiFont.Body, new Color(0.84f, 0.86f, 0.92f));

            parts = new List<(string, UnityEngine.Object)>
            {
                ("_playerName", playerName), ("_nameHint", nameHint), ("_hostButton", host), ("_joinMenuButton", joinMenu),
                ("_howToPlayButton", howToPlay), ("_recordsButton", records), ("_settingsButton", settings), ("_quitButton", quit),
                ("_mainStatus", status),
            };
            return screen;
        }

        /// <summary>«TU PERSONAJE»: el personaje en 3D en su peana, flechas, nombre, estado y cuántos más hay en la tienda.</summary>
        private static void BuildCharacterPanel(RectTransform screen)
        {
            var panel = Panel("CharacterPanel", screen, 925f, 138f, 786f, 856f);
            Text("Title", panel, "TU PERSONAJE", 60f, 26f, 666f, 72f, 54f, UiFont.Display, Ivory);
            Tinted(Picture("Spotlight", panel, GlowSprite(), 143f, 70f, 500f, 560f), new Color(0.55f, 0.65f, 1f, 0.22f));
            var preview = Rect("Preview", panel, 113f, 92f, 560f, 700f).gameObject.AddComponent<RawImage>();
            preview.raycastTarget = false;
            var previous = IconButton("PreviousCharacter", panel, Icon("ArrowLeft"), 60f, 328f, 104f);
            var next = IconButton("NextCharacter", panel, Icon("ArrowRight"), 622f, 328f, 104f);
            var plate = Picture("NamePlate", panel, PlateSprite(), 183f, 660f, 420f, 68f);
            var name = Text("CharacterName", plate.transform, string.Empty, 10f, 2f, 400f, 62f, 34f, UiFont.Bold, Ivory);
            Pill("Status", panel, PillSprite(), Icon("Check"), string.Empty, 268f, 736f, 250f, 52f, Ivory, out var status, out _, 23f);
            var hint = Text("CharacterHint", panel, string.Empty, 93f, 796f, 600f, 34f, 22f, UiFont.Body, Muted);

            var view = panel.gameObject.AddComponent<CharacterPickerView>();
            SerializedWiring.Set(view, "_catalog", SharedAssets.CharacterCatalog());
            SerializedWiring.Set(view, "_profile", SharedAssets.PlayerProfile());
            SerializedWiring.Set(view, "_previousButton", previous);
            SerializedWiring.Set(view, "_nextButton", next);
            SerializedWiring.Set(view, "_preview", SharedAssets.CharacterPreview());
            SerializedWiring.Set(view, "_image", preview);
            SerializedWiring.Set(view, "_name", name);
            SerializedWiring.Set(view, "_status", status);
            SerializedWiring.Set(view, "_hint", hint);
        }

        // ---- Unirme a una sala (02-join-room) ----

        private static RectTransform BuildJoin(Transform root, out List<(string, UnityEngine.Object)> parts)
        {
            var screen = UiKit.Screen("JoinMenu", root);
            Picture("Character", screen, Art("Character_Pointing"), 96f, 200f, 520f, 740f, preserveAspect: true);
            var panel = Panel("Panel", screen, 540f, 160f, 1039f, 760f);
            Picture("Title", panel, Title("JoinRoom"), 100f, 36f, 840f, 104f, preserveAspect: true);
            Text("Subtitle", panel, "Introduce la dirección que te ha pasado el anfitrión", 60f, 146f, 919f, 40f, 28f, UiFont.Bold, Ivory);
            Text("AddressLabel", panel, "Dirección del anfitrión", 99f, 214f, 700f, 36f, 26f, UiFont.Bold, Ivory, TextAlignmentOptions.MidlineLeft);
            var address = InputField("Address", panel, 87f, 256f, 866f, 88f, "p. ej. 192.168.1.24", 64, 40f);
            var status = Text("Status", panel, string.Empty, 99f, 352f, 854f, 34f, 22f, UiFont.Body, Muted, TextAlignmentOptions.MidlineLeft);
            var connect = MakeButton("Connect", panel, "Conectar", 87f, 410f, 866f, 96f, ButtonStyle.Primary, 44f);
            var back = MakeButton("Back", panel, "Volver", 87f, 528f, 866f, 74f, ButtonStyle.Secondary, 34f);
            Divider("Divider", panel, 41f, 636f, 957f);
            var name = Text("Name", panel, string.Empty, 60f, 664f, 919f, 48f, 30f, UiFont.Body, Muted);

            parts = new List<(string, UnityEngine.Object)>
            {
                ("_address", address), ("_connectButton", connect), ("_joinBackButton", back), ("_joinStatus", status), ("_joinName", name),
            };
            return screen;
        }

        // ---- Sala de espera (03-lobby) ----

        private static RectTransform BuildLobby(Transform root, out List<(string, UnityEngine.Object)> parts)
        {
            var screen = UiKit.Screen("Lobby", root);
            Picture("Title", screen, Title("Lobby"), 214f, 34f, 820f, 132f, preserveAspect: true);

            // Arriba a la derecha: tú y tus fichas.
            var me = Picture("Me", screen, SecondarySprite(), 1160f, 55f, 330f, 77f);
            var mePortraitFrame = Picture("PortraitFrame", me.transform, CircleNavySprite(), 8f, 6f, 64f, 64f);
            mePortraitFrame.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var mePortrait = Rect("Portrait", mePortraitFrame.transform, 2f, 2f, 60f, 60f).gameObject.AddComponent<RawImage>();
            mePortrait.raycastTarget = false;
            var meName = Text("Name", me.transform, string.Empty, 82f, 4f, 238f, 68f, 26f, UiFont.Bold, Ivory, TextAlignmentOptions.MidlineLeft);
            var tokens = Picture("Tokens", screen, SecondarySprite(), 1504f, 55f, 271f, 77f);
            Picture("Icon", tokens.transform, Icon("Token"), 14f, 10f, 57f, 57f, preserveAspect: true);
            var tokensText = Text("Text", tokens.transform, string.Empty, 80f, 4f, 180f, 68f, 28f, UiFont.Bold, Ivory, TextAlignmentOptions.MidlineLeft);

            // Izquierda: las plazas y la invitación.
            var players = Panel("Players", screen, 237f, 172f, 820f, 859f);
            var playersTitle = Text("PlayersTitle", players, string.Empty, 73f, 18f, 600f, 58f, 44f, UiFont.Display, Ivory,
                TextAlignmentOptions.MidlineLeft);
            for (var i = 0; i < SEATS; i++)
                BuildSeat(players, i, 48f, 83f + i * 93f);

            var invite = SubPanel("Invite", players, 48f, 650f, 728f, 191f);
            Text("Title", invite, "Invita a tus amigos", 41f, 16f, 600f, 46f, 32f, UiFont.Bold, Ivory, TextAlignmentOptions.MidlineLeft);
            var field = Picture("Address", invite, InputSprite(), 41f, 75f, 470f, 63f);
            var addressText = Text("Text", field.transform, string.Empty, 22f, 2f, 430f, 58f, 34f, UiFont.Bold, Ivory, TextAlignmentOptions.MidlineLeft);
            var copy = MakeButton("Copy", invite, "Copiar", 527f, 75f, 172f, 63f, ButtonStyle.Primary, out var copyLabel, 28f);
            copyLabel.rectTransform.anchoredPosition += new Vector2(22f, 0f);
            Picture("Icon", copy.transform, Icon("Copy"), 16f, 15f, 34f, 34f, preserveAspect: true);
            Text("Hint", invite, "2–6 jugadores", 41f, 146f, 400f, 30f, 22f, UiFont.Body, Muted, TextAlignmentOptions.MidlineLeft);

            // Derecha: tu personaje y las acciones.
            var actions = Panel("Actions", screen, 1077f, 153f, 698f, 881f);
            var heading = Text("Heading", actions, string.Empty, 40f, 30f, 618f, 58f, 42f, UiFont.Display, Ivory);
            Tinted(Picture("Spotlight", actions, GlowSprite(), 99f, 70f, 500f, 470f), new Color(0.55f, 0.65f, 1f, 0.22f));
            var preview = Rect("Preview", actions, 160f, 86f, 378f, 472f).gameObject.AddComponent<RawImage>();
            preview.raycastTarget = false;
            var previewBinder = preview.gameObject.AddComponent<CharacterPreviewImage>();
            SerializedWiring.Set(previewBinder, "_preview", SharedAssets.CharacterPreview());
            SerializedWiring.Set(previewBinder, "_image", preview);
            var start = MakeButton("Start", actions, "Empezar partida", 52f, 540f, 597f, 86f, ButtonStyle.Primary, 42f);
            var waiting = Text("Waiting", actions, "Esperando al anfitrión…", 52f, 540f, 597f, 86f, 30f, UiFont.Body, Muted);
            var shop = MakeButton("Shop", actions, "Tienda", 52f, 641f, 597f, 65f, ButtonStyle.Secondary, 32f);
            var howToPlay = MakeButton("HowToPlay", actions, "Cómo se juega", 52f, 719f, 597f, 65f, ButtonStyle.Secondary, 32f);
            var leave = MakeButton("Leave", actions, "Salir de la sala", 52f, 797f, 597f, 65f, ButtonStyle.Quiet, out var leaveLabel, 28f);
            leaveLabel.color = Muted;

            parts = new List<(string, UnityEngine.Object)>
            {
                ("_playersTitle", playersTitle), ("_meName", meName), ("_mePortrait", mePortrait), ("_tokens", tokensText),
                ("_invitePanel", invite.gameObject), ("_inviteAddress", addressText), ("_copyButton", copy), ("_copyLabel", copyLabel),
                ("_lobbyHeading", heading), ("_startButton", start), ("_waiting", waiting), ("_shopButton", shop),
                ("_lobbyHowToPlayButton", howToPlay), ("_leaveButton", leave),
            };
            return screen;
        }

        /// <summary>Una plaza: retrato, nombre, personaje e insignia de anfitrión; vacía, «Esperando jugador…».</summary>
        private static void BuildSeat(Transform parent, int index, float x, float y)
        {
            var row = Picture($"Seat{index + 1}", parent, RowSprite(), x, y, 728f, 82f);
            var group = row.gameObject.AddComponent<CanvasGroup>();

            var filled = Rect("Filled", row.transform, 0f, 0f, 728f, 82f);
            var frame = Picture("PortraitFrame", filled, CardSprite(), 10f, 5f, 104f, 72f);
            frame.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var portrait = Rect("Portrait", frame.transform, 16f, -8f, 72f, 88f).gameObject.AddComponent<RawImage>();
            portrait.raycastTarget = false;
            var name = Text("Name", filled, string.Empty, 142f, 6f, 380f, 40f, 30f, UiFont.Bold, Ivory, TextAlignmentOptions.MidlineLeft);
            var character = Text("Character", filled, string.Empty, 142f, 42f, 380f, 32f, 23f, UiFont.Body, Muted, TextAlignmentOptions.MidlineLeft);
            var badge = Pill("HostBadge", filled, PillSprite(), Icon("Crown"), "Anfitrión", 547f, 18f, 166f, 46f, Ivory, out _, out _, 22f);

            var empty = Rect("Empty", row.transform, 0f, 0f, 728f, 82f);
            Picture("Icon", empty, Icon("EmptyPlayer"), 31f, 8f, 66f, 66f, preserveAspect: true);
            Text("Text", empty, "Esperando jugador…", 142f, 0f, 500f, 82f, 25f, UiFont.Body, Muted, TextAlignmentOptions.MidlineLeft);

            var seat = row.gameObject.AddComponent<LobbyPlayerRow>();
            SerializedWiring.Set(seat, "_filled", filled.gameObject);
            SerializedWiring.Set(seat, "_empty", empty.gameObject);
            SerializedWiring.Set(seat, "_group", group);
            SerializedWiring.Set(seat, "_portrait", portrait);
            SerializedWiring.Set(seat, "_name", name);
            SerializedWiring.Set(seat, "_character", character);
            SerializedWiring.Set(seat, "_hostBadge", badge.gameObject);
            filled.gameObject.SetActive(false);
        }

        // ---- Cómo se juega (06-how-to-play) ----

        private static HowToPlayView BuildHowToPlayOverlay(GameObject root, HowToPlayContent content)
        {
            var overlay = Overlay("HowToPlay", root.transform, out var screen);

            // Tira de pasos (detrás del panel, asoma por arriba).
            var strip = Panel("Steps", screen, 333f, 40f, 1254f, 160f);
            var count = content.Count;
            var steps = new Button[count];
            var circles = new Image[count];
            var numbers = new TextMeshProUGUI[count];
            var labels = new TextMeshProUGUI[count];
            var underlines = new GameObject[count];
            var centers = new[] { 133f, 322f, 511f, 718f, 919f, 1120f };
            for (var i = 0; i < count; i++)
            {
                var center = i < centers.Length ? centers[i] : 133f + i * 197f;
                var step = Rect($"Step{i + 1}", strip, center - 90f, 10f, 180f, 108f);
                var hit = step.gameObject.AddComponent<Image>();
                hit.sprite = DotSprite(); // Con imagen propia: el tema no le pone marco.
                hit.type = Image.Type.Sliced;
                FitCorners(hit);
                hit.color = new Color(1f, 1f, 1f, 0f);
                steps[i] = step.gameObject.AddComponent<Button>();
                steps[i].targetGraphic = hit;
                step.gameObject.AddComponent<Trivias.Shared.Presentation.UiButtonFeel>();
                circles[i] = Picture("Circle", step, CircleNavySprite(), 66f, 8f, 48f, 48f);
                numbers[i] = Text("Number", circles[i].transform, (i + 1).ToString(), 0f, 0f, 48f, 48f, 26f, UiFont.Bold, Ivory);
                labels[i] = Text("Label", step, string.Empty, 0f, 60f, 180f, 34f, 23f, UiFont.Bold, Muted);
                underlines[i] = Tinted(Picture("Underline", step, LineSprite(), 38f, 98f, 104f, 4f), Gold).gameObject;
            }

            var panel = Panel("Panel", screen, 230f, 163f, 1461f, 857f);
            Picture("Title", panel, Title("HowToPlay"), 340f, 18f, 781f, 116f, preserveAspect: true);
            var pageLabel = Text("Page", panel, string.Empty, 560f, 134f, 90f, 30f, 24f, UiFont.Body, Ivory, TextAlignmentOptions.MidlineRight);
            var dots = new Image[count];
            for (var i = 0; i < count; i++)
                dots[i] = Picture($"Dot{i + 1}", panel, DotSprite(), 666f + i * 39f, 139f, 21f, 21f);
            var pill = Picture("PagePill", panel, PlateSprite(), 436f, 178f, 588f, 60f);
            var title = Text("PageTitle", pill.transform, string.Empty, 10f, 2f, 568f, 56f, 30f, UiFont.Bold, Ivory);
            var illustration = Picture("Illustration", panel, null, 140f, 248f, 1181f, 296f, preserveAspect: true);
            // Las teclas del texto («[E]») se dibujan como en el juego, en una capa detrás de las letras.
            // Cuatro líneas con teclas (página de controles) caben sin encoger mucho.
            var keyLayer = Rect("Keys", panel, 100f, 552f, 1261f, 186f);
            var keys = new Image[TUTORIAL_KEYS];
            for (var i = 0; i < keys.Length; i++)
            {
                keys[i] = Picture($"Key{i + 1}", keyLayer, KeycapSprite(), 0f, 0f, 40f, 40f);
                keys[i].gameObject.SetActive(false);
            }
            var body = Text("Body", panel, string.Empty, 100f, 552f, 1261f, 186f, 33f, UiFont.Bold, Ivory);
            body.lineSpacing = 4f;
            var bodyKeys = body.gameObject.AddComponent<Trivias.Shared.Presentation.UiKeyedText>();
            SerializedWiring.Set(bodyKeys, "_text", body);
            SerializedWiring.SetArray(bodyKeys, "_keys", keys);
            var previous = MakeButton("Previous", panel, "‹  Anterior", 94f, 751f, 390f, 76f, ButtonStyle.Quiet, 32f);
            var next = MakeButton("Next", panel, "Siguiente  ›", 530f, 746f, 400f, 86f, ButtonStyle.Primary, out var nextLabel, 34f);
            var close = MakeButton("Close", panel, "Cerrar", 974f, 751f, 393f, 76f, ButtonStyle.Secondary, 32f);

            var illustrations = new Sprite[TutorialArt.Length];
            for (var i = 0; i < TutorialArt.Length; i++)
                illustrations[i] = Art(TutorialArt[i]);

            var view = root.AddComponent<HowToPlayView>();
            SerializedWiring.Set(view, "_content", content);
            SerializedWiring.Set(view, "_backdrop", SharedAssets.UiBackdrop());
            SerializedWiring.Set(view, "_panel", overlay);
            SerializedWiring.Set(view, "_title", title);
            SerializedWiring.Set(view, "_body", body);
            SerializedWiring.Set(view, "_bodyKeys", bodyKeys);
            SerializedWiring.Set(view, "_pageLabel", pageLabel);
            SerializedWiring.Set(view, "_illustration", illustration);
            SerializedWiring.SetArray(view, "_illustrations", illustrations);
            SerializedWiring.SetArray(view, "_steps", steps);
            SerializedWiring.SetArray(view, "_stepCircles", circles);
            SerializedWiring.SetArray(view, "_stepNumbers", numbers);
            SerializedWiring.SetArray(view, "_stepLabels", labels);
            SerializedWiring.SetArray(view, "_stepUnderlines", underlines);
            SerializedWiring.SetArray(view, "_dots", dots);
            SerializedWiring.Set(view, "_activeCircle", CircleGoldSprite());
            SerializedWiring.Set(view, "_idleCircle", CircleNavySprite());
            SerializedWiring.Set(view, "_previousButton", previous);
            SerializedWiring.Set(view, "_nextButton", next);
            SerializedWiring.Set(view, "_nextLabel", nextLabel);
            SerializedWiring.Set(view, "_closeButton", close);
            overlay.SetActive(false);
            return view;
        }

        // ---- Récords (07-records) ----

        private static RecordsView BuildRecordsOverlay(GameObject root)
        {
            var overlay = Overlay("Records", root.transform, out var screen);
            Picture("Title", screen, Title("Records"), 625f, 30f, 670f, 150f, preserveAspect: true);
            Text("Subtitle", screen, "Las mejores victorias de este ordenador", 460f, 174f, 1000f, 44f, 32f, UiFont.Bold, Ivory);

            var panel = Panel("Panel", screen, 130f, 227f, 1662f, 767f);
            var tabCount = RankingRules.MAX_GROUP - RankingRules.MIN_GROUP + 1;
            var tabs = new Button[tabCount];
            var tabLabels = new TextMeshProUGUI[tabCount];
            for (var i = 0; i < tabCount; i++)
                tabs[i] = MakeButton($"Tab{RankingRules.MIN_GROUP + i}", panel, RecordText.Category(RankingRules.MIN_GROUP + i),
                    45f + i * 316f, 34f, 306f, 78f, ButtonStyle.Secondary, out tabLabels[i], 30f);

            var header = Picture("Header", panel, HeaderSprite(), 40f, 136f, 1581f, 67f);
            var columns = Columns();
            var titles = new[] { "Puesto", "Premio", "Tiempo", "Fecha", "Participantes", "Supervivientes" };
            for (var i = 0; i < titles.Length; i++)
                Text(titles[i], header.transform, titles[i], columns[i].x, 0f, columns[i].y, 67f, 25f, UiFont.Bold, Ivory);

            // Tabla con desplazamiento: las diez mejores.
            var viewport = Rect("Viewport", panel, 40f, 216f, 1581f, 376f);
            viewport.gameObject.AddComponent<RectMask2D>();
            var hit = viewport.gameObject.AddComponent<Image>();
            hit.sprite = DotSprite();
            hit.color = new Color(1f, 1f, 1f, 0f);
            var content = Rect("Content", viewport, 0f, 0f, 1581f, RECORD_ROWS * 98f);
            var rows = new RecordRowView[RECORD_ROWS];
            for (var i = 0; i < RECORD_ROWS; i++)
                rows[i] = BuildRecordRow(content, i, columns);
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.content = content;
            scroll.viewport = viewport;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;
            var empty = Text("Empty", viewport, Trivias.Menu.Logic.RecordsTable.EMPTY, 0f, 120f, 1581f, 60f, 28f, UiFont.Body, Muted);

            Divider("Divider", panel, 40f, 600f, 1581f);
            Text("Rule", panel, "Hasta 10 victorias por tamaño de grupo.", 61f, 640f, 900f, 36f, 26f, UiFont.Body, Ivory, TextAlignmentOptions.MidlineLeft);
            Text("RuleHint", panel, "Más premio primero. En empate, menos tiempo.", 61f, 680f, 900f, 32f, 23f, UiFont.Body, Muted,
                TextAlignmentOptions.MidlineLeft);
            var close = MakeButton("Close", panel, "Cerrar", 1257f, 632f, 364f, 98f, ButtonStyle.Primary, 44f);

            var view = root.AddComponent<RecordsView>();
            SerializedWiring.Set(view, "_records", RankingBuilder.LocalRecords());
            SerializedWiring.Set(view, "_backdrop", SharedAssets.UiBackdrop());
            SerializedWiring.Set(view, "_panel", overlay);
            SerializedWiring.SetArray(view, "_tabs", tabs);
            SerializedWiring.SetArray(view, "_tabLabels", tabLabels);
            SerializedWiring.Set(view, "_selectedTab", PrimarySprite());
            SerializedWiring.Set(view, "_idleTab", SecondarySprite());
            SerializedWiring.SetArray(view, "_rows", rows);
            SerializedWiring.SetArray(view, "_medals", new[] { Icon("Medal1"), Icon("Medal2"), Icon("Medal3") });
            SerializedWiring.Set(view, "_empty", empty);
            SerializedWiring.Set(view, "_scroll", scroll);
            SerializedWiring.Set(view, "_closeButton", close);
            overlay.SetActive(false);
            return view;
        }

        /// <summary>Columnas de la tabla: (x, ancho) dentro de la fila.</summary>
        private static Vector2[] Columns() => new[]
        {
            new Vector2(12f, 140f), new Vector2(170f, 270f), new Vector2(450f, 190f), new Vector2(650f, 200f),
            new Vector2(860f, 470f), new Vector2(1340f, 230f),
        };

        private static RecordRowView BuildRecordRow(Transform content, int index, Vector2[] columns)
        {
            var row = Picture($"Row{index + 1}", content, RowSprite(), 0f, index * 98f, 1581f, 83f);
            var medal = Picture("Medal", row.transform, null, columns[0].x + columns[0].y * 0.5f - 34f, 7f, 68f, 68f, preserveAspect: true);
            var position = Text("Position", row.transform, string.Empty, columns[0].x, 0f, columns[0].y, 83f, 32f, UiFont.Bold, Ivory);
            var money = Text("Money", row.transform, string.Empty, columns[1].x, 0f, columns[1].y, 83f, 36f, UiFont.Bold, Ivory);
            var time = Text("Time", row.transform, string.Empty, columns[2].x, 0f, columns[2].y, 83f, 31f, UiFont.Body, Ivory);
            var date = Text("Date", row.transform, string.Empty, columns[3].x, 0f, columns[3].y, 83f, 28f, UiFont.Body, Ivory);
            var players = Text("Players", row.transform, string.Empty, columns[4].x, 0f, columns[4].y, 83f, 28f, UiFont.Body, Ivory);
            var survivors = Text("Survivors", row.transform, string.Empty, columns[5].x, 0f, columns[5].y, 83f, 30f, UiFont.Body, Ivory);

            var view = row.gameObject.AddComponent<RecordRowView>();
            SerializedWiring.Set(view, "_medal", medal);
            SerializedWiring.Set(view, "_position", position);
            SerializedWiring.Set(view, "_money", money);
            SerializedWiring.Set(view, "_time", time);
            SerializedWiring.Set(view, "_date", date);
            SerializedWiring.Set(view, "_players", players);
            SerializedWiring.Set(view, "_survivors", survivors);
            return view;
        }

        // ---- Configuración (08-settings) ----

        private static SettingsView BuildSettingsOverlay(GameObject root, GameSettings settings)
        {
            var overlay = Overlay("Settings", root.transform, out var screen);
            var panel = Panel("Panel", screen, 166f, 181f, 1590f, 808f);
            Picture("Title", screen, Title("Settings"), 446f, 70f, 1028f, 160f, preserveAspect: true);

            var sound = SubPanel("Sound", panel, 40f, 67f, 742f, 567f);
            Picture("Icon", sound, Icon("Speaker"), 29f, 11f, 92f, 92f, preserveAspect: true);
            Text("SoundTitle", sound, "SONIDO", 152f, 22f, 560f, 70f, 50f, UiFont.Display, Ivory, TextAlignmentOptions.MidlineLeft);
            Divider("Divider", sound, 37f, 115f, 668f);
            var master = SliderRow(sound, "MasterVolume", "Volumen general", 142f, 0f, 1f, out var masterValue);
            var music = SliderRow(sound, "MusicVolume", "Música", 282f, 0f, 1f, out var musicValue);
            var effects = SliderRow(sound, "EffectsVolume", "Efectos", 422f, 0f, 1f, out var effectsValue);

            var controls = SubPanel("Controls", panel, 808f, 67f, 744f, 567f);
            Picture("Icon", controls, Icon("Gear"), 29f, 11f, 92f, 92f, preserveAspect: true);
            Text("ControlsTitle", controls, "CONTROLES Y PANTALLA", 152f, 22f, 570f, 70f, 46f, UiFont.Display, Ivory,
                TextAlignmentOptions.MidlineLeft);
            Divider("Divider", controls, 37f, 115f, 670f);
            var sensitivity = SliderRow(controls, "MouseSensitivity", "Sensibilidad del ratón", 142f, SettingsRules.MIN_SENSITIVITY,
                SettingsRules.MAX_SENSITIVITY, out var sensitivityValue);
            Divider("Divider2", controls, 37f, 303f, 670f);
            Text("InvertYLabel", controls, "Invertir eje vertical", 38f, 330f, 460f, 60f, 30f, UiFont.Body, Ivory, TextAlignmentOptions.MidlineLeft);
            var invertY = Switch("InvertY", controls, 514f, 330f, out _);
            Divider("Divider3", controls, 37f, 429f, 670f);
            Text("FullscreenLabel", controls, "Pantalla completa", 38f, 457f, 460f, 60f, 30f, UiFont.Body, Ivory, TextAlignmentOptions.MidlineLeft);
            var fullscreen = Switch("Fullscreen", controls, 514f, 457f, out _);

            var reset = MakeButton("Reset", panel, "Restablecer", 42f, 682f, 374f, 82f, ButtonStyle.Secondary, 32f);
            Text("Hint", panel, "Los cambios se aplican al momento y se guardan al cerrar.", 470f, 668f, 650f, 40f, 21f, UiFont.Body, Muted);
            var close = MakeButton("Close", panel, "Cerrar", 1173f, 678f, 374f, 88f, ButtonStyle.Primary, 44f);

            var view = root.AddComponent<SettingsView>();
            SerializedWiring.Set(view, "_settings", settings);
            SerializedWiring.Set(view, "_backdrop", SharedAssets.UiBackdrop());
            SerializedWiring.Set(view, "_panel", overlay);
            SerializedWiring.Set(view, "_masterVolume", master);
            SerializedWiring.Set(view, "_musicVolume", music);
            SerializedWiring.Set(view, "_effectsVolume", effects);
            SerializedWiring.Set(view, "_mouseSensitivity", sensitivity);
            SerializedWiring.Set(view, "_masterValue", masterValue);
            SerializedWiring.Set(view, "_musicValue", musicValue);
            SerializedWiring.Set(view, "_effectsValue", effectsValue);
            SerializedWiring.Set(view, "_sensitivityValue", sensitivityValue);
            SerializedWiring.Set(view, "_invertY", invertY);
            SerializedWiring.Set(view, "_fullscreen", fullscreen);
            SerializedWiring.Set(view, "_invertYSwitch", invertY.GetComponent<Trivias.Shared.Presentation.UiSwitch>());
            SerializedWiring.Set(view, "_fullscreenSwitch", fullscreen.GetComponent<Trivias.Shared.Presentation.UiSwitch>());
            SerializedWiring.Set(view, "_resetButton", reset);
            SerializedWiring.Set(view, "_closeButton", close);
            overlay.SetActive(false);
            return view;
        }

        /// <summary>«Volumen general» encima y, debajo, el deslizador con su valor en una cajita a la derecha.</summary>
        private static Slider SliderRow(Transform parent, string name, string caption, float y, float min, float max,
            out TextMeshProUGUI value)
        {
            Text($"{name}Label", parent, caption, 37f, y, 520f, 44f, 30f, UiFont.Body, Ivory, TextAlignmentOptions.MidlineLeft);
            var slider = MakeSlider(name, parent, 38f, y + 50f, 527f, min, max);
            var box = Picture($"{name}Value", parent, HeaderSprite(), 590f, y + 44f, 118f, 57f);
            value = Text("Text", box.transform, string.Empty, 0f, 0f, 118f, 57f, 27f, UiFont.Bold, Ivory);
            return slider;
        }

        // ---- Pausa y confirmación de salida (09-pause, 10-exit-confirmation) ----

        private static PauseMenuView BuildPause(GameObject root, MatchRuntime match)
        {
            var pause = Overlay("Pause", root.transform, out var pauseScreen);
            var panel = Panel("Panel", pauseScreen, 549f, 163f, 823f, 721f);
            Picture("Title", panel, Title("Pause"), 77f, 42f, 672f, 100f, preserveAspect: true);
            Text("Subtitle", panel, "La partida continúa para los demás.", 40f, 146f, 743f, 44f, 30f, UiFont.Body, Ivory);
            var resume = MakeButton("Resume", panel, "Continuar", 54f, 219f, 715f, 117f, ButtonStyle.Primary, 50f);
            var howToPlay = MakeButton("HowToPlay", panel, "Cómo se juega", 54f, 357f, 715f, 88f, ButtonStyle.Secondary, 38f);
            var settings = MakeButton("Settings", panel, "Configuración", 54f, 467f, 715f, 88f, ButtonStyle.Secondary, 38f);
            var leave = MakeButton("Leave", panel, "Salir de la sala", 54f, 578f, 715f, 88f, ButtonStyle.Danger, 38f);
            Text("Footer", pauseScreen, "Esc  ·  Continuar", 760f, 1004f, 400f, 36f, 24f, UiFont.Body, Ivory);

            var confirm = Overlay("ConfirmLeave", root.transform, out var confirmScreen);
            var dialog = Panel("Panel", confirmScreen, 517f, 187f, 886f, 754f);
            Picture("Title", dialog, Title("LeaveRoom"), 110f, 34f, 666f, 82f, preserveAspect: true);
            Picture("Illustration", dialog, Art("Character_Door"), 290f, 110f, 306f, 360f, preserveAspect: true);
            Text("Text", dialog, "Volverás al menú principal.", 40f, 476f, 806f, 44f, 32f, UiFont.Bold, Ivory);
            var hostWarning = Text("HostWarning", dialog, "Si eres el anfitrión, la sala se cerrará para todos.", 40f, 518f, 806f,
                40f, 27f, UiFont.Body, Ivory);
            var stay = MakeButton("Cancel", dialog, "Quedarme", 42f, 572f, 400f, 96f, ButtonStyle.Primary, 40f);
            var confirmLeave = MakeButton("Confirm", dialog, "Salir de la sala", 468f, 572f, 376f, 96f, ButtonStyle.Danger, 36f);
            Text("Footer", dialog, "Esc  ·  Cancelar", 243f, 694f, 400f, 36f, 23f, UiFont.Body, Muted);

            var view = root.AddComponent<PauseMenuView>();
            SerializedWiring.Set(view, "_match", match);
            SerializedWiring.Set(view, "_room", SharedAssets.RoomConnection());
            SerializedWiring.Set(view, "_backdrop", SharedAssets.UiBackdrop());
            SerializedWiring.Set(view, "_controlGate", SharedAssets.PlayerControlGate());
            SerializedWiring.Set(view, "_leaveRoomRequested", SharedAssets.LeaveRoomRequested());
            SerializedWiring.Set(view, "_howToPlay", root.GetComponent<HowToPlayView>());
            SerializedWiring.Set(view, "_settingsView", root.GetComponent<SettingsView>());
            SerializedWiring.Set(view, "_recordsView", root.GetComponent<RecordsView>());
            SerializedWiring.Set(view, "_panel", pause);
            SerializedWiring.Set(view, "_resumeButton", resume);
            SerializedWiring.Set(view, "_howToPlayButton", howToPlay);
            SerializedWiring.Set(view, "_settingsButton", settings);
            SerializedWiring.Set(view, "_leaveButton", leave);
            SerializedWiring.Set(view, "_confirmPanel", confirm);
            SerializedWiring.Set(view, "_confirmLeaveButton", confirmLeave);
            SerializedWiring.Set(view, "_cancelLeaveButton", stay);
            SerializedWiring.Set(view, "_hostWarning", hostWarning.gameObject);
            pause.SetActive(false);
            confirm.SetActive(false);
            return view;
        }

        // ---- Utilidades ----

        /// <summary>Capa a pantalla completa (velo que bloquea lo de detrás) con un lienzo de 1920 × 1080 centrado.</summary>
        private static GameObject Overlay(string name, Transform parent, out RectTransform screen)
        {
            var overlay = FullScreen(name, parent);
            overlay.gameObject.AddComponent<Image>().color = OverlayVeil;
            screen = UiKit.Screen("Screen", overlay);
            return overlay.gameObject;
        }

        private static Sprite Title(string name) => ArtSprite($"{TITLES}/Title_{name}.png");

        private static Sprite Art(string name) => ArtSprite($"{ILLUSTRATIONS}/{name}.png");
    }
}
