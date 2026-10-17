using TMPro;
using Trivias.Match.Data;
using Trivias.Ranking.Editor;
using Trivias.Results.Components;
using Trivias.Shared.Editor;
using UnityEngine;
using UnityEngine.UI;
using static Trivias.Shared.Editor.UiKit;

namespace Trivias.Results.Editor
{
    /// <summary>
    /// Construye la pantalla de resultados con la convención de los menús (bocetos de docs/bocetos/ui/menu): velo sobre
    /// el plató desenfocado, panel azul noche con marco dorado que aparece con un rebote, título grande de Lilita One,
    /// cifras en tarjetas, fichas en una pastilla dorada y los botones del kit.
    /// </summary>
    public static class ResultsPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Results";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/ResultsScreen.prefab";
        private static readonly Color OverlayVeil = new(0.02f, 0.04f, 0.1f, 0.45f);
        public const int SORTING_ORDER = 35;

        public static GameObject Build(MatchRuntime match)
        {
            var root = new GameObject("ResultsScreen", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Por encima del menú (30): su velo de fondo desenfocado queda detrás de los resultados, no encima.
            canvas.sortingOrder = SORTING_ORDER;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            // Como las ventanas del menú: velo a pantalla completa y un lienzo de 1920 × 1080 centrado.
            var overlay = FullScreen("Panel", root.transform);
            overlay.gameObject.AddComponent<Image>().color = OverlayVeil;
            var screen = UiKit.Screen("Screen", overlay);
            var card = UiKit.Panel("Card", screen, 510f, 110f, 900f, 830f);

            var title = Text("Title", card, "DERROTA", 0f, 22f, 900f, 126f, 110f, UiFont.Display, Color.white);
            title.fontSharedMaterial = StageFonts.DisplayTitle();
            title.enableVertexGradient = true;
            title.characterSpacing = 2f;
            var subtitle = Text("Subtitle", card, string.Empty, 40f, 146f, 820f, 44f, 30f, UiFont.Body, Ivory);

            var questions = Stat(card, 0, "PREGUNTAS");
            var money = Stat(card, 1, "DINERO FINAL");
            var time = Stat(card, 2, "TIEMPO");

            var record = Text("Record", card, string.Empty, 40f, 360f, 820f, 40f, 26f, UiFont.Bold, Gold);
            Pill("Tokens", card, PillGoldSprite(), Icon("Token"), string.Empty, 170f, 412f, 560f, 66f, DarkText, out var tokens, out _, 28f);
            tokens.fontStyle = FontStyles.Bold;
            var tokensHint = Text("TokensHint", card, string.Empty, 40f, 484f, 820f, 34f, 22f, UiFont.Body, Muted);
            Divider("Divider", card, 100f, 536f, 700f);
            var survivors = Text("Survivors", card, string.Empty, 40f, 552f, 820f, 40f, 26f, UiFont.Body, Ivory);
            var eliminated = Text("Eliminated", card, string.Empty, 40f, 594f, 820f, 36f, 23f, UiFont.Body, Muted);

            var playAgain = MakeButton("PlayAgain", card, "Volver a jugar", 48f, 668f, 470f, 104f, ButtonStyle.Primary, out var playAgainLabel, 40f);
            var leave = MakeButton("Leave", card, "Salir de la sala", 538f, 674f, 314f, 92f, ButtonStyle.Danger, 32f);

            var view = root.AddComponent<ResultsView>();
            SerializedWiring.Set(view, "_match", match);
            SerializedWiring.Set(view, "_leaveRoomRequested", SharedAssets.LeaveRoomRequested());
            SerializedWiring.Set(view, "_profile", SharedAssets.PlayerProfile());
            SerializedWiring.Set(view, "_records", RankingBuilder.LocalRecords());
            SerializedWiring.Set(view, "_backdrop", SharedAssets.UiBackdrop());
            SerializedWiring.Set(view, "_panel", overlay.gameObject);
            SerializedWiring.Set(view, "_title", title);
            SerializedWiring.Set(view, "_subtitle", subtitle);
            SerializedWiring.Set(view, "_questions", questions);
            SerializedWiring.Set(view, "_money", money);
            SerializedWiring.Set(view, "_time", time);
            SerializedWiring.Set(view, "_record", record);
            SerializedWiring.Set(view, "_tokens", tokens);
            SerializedWiring.Set(view, "_tokensHint", tokensHint);
            SerializedWiring.Set(view, "_survivors", survivors);
            SerializedWiring.Set(view, "_eliminated", eliminated);
            SerializedWiring.Set(view, "_playAgainButton", playAgain);
            SerializedWiring.Set(view, "_playAgainLabel", playAgainLabel);
            SerializedWiring.Set(view, "_leaveButton", leave);
            SerializedWiring.Set(view, "_victoryCelebrationSeconds", 5f);

            overlay.gameObject.SetActive(false);
            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }

        /// <summary>Tarjeta de una cifra (preguntas, dinero, tiempo), en una fila de tres.</summary>
        private static TextMeshProUGUI Stat(Transform card, int column, string caption)
        {
            var box = Picture($"Stat{column}", card, CardSprite(), 40f + column * 280f, 206f, 260f, 134f);
            Text("Label", box.transform, caption, 0f, 16f, 260f, 28f, 20f, UiFont.Bold, Muted);
            return Text("Value", box.transform, string.Empty, 10f, 48f, 240f, 70f, 52f, UiFont.Display, Gold);
        }
    }
}
