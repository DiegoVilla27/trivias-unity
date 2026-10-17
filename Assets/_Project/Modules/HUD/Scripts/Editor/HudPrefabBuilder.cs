using TMPro;
using Trivias.HUD.Components;
using Trivias.Match.Data;
using Trivias.Money.Data;
using Trivias.SelfBet.Data;
using Trivias.Shared.Editor;
using Trivias.Shared.Presentation;
using UnityEngine;
using UnityEngine.UI;
using static Trivias.Shared.Editor.UiKit;

namespace Trivias.HUD.Editor
{
    /// <summary>
    /// Construye el HUD de la partida según los bocetos de <c>docs/bocetos/ui/hud</c>, en su versión sin saturar: tres
    /// tarjetas pequeñas arriba (pregunta y estado, reloj, dinero y comodines) y tarjetas que solo aparecen cuando hacen
    /// falta. Cada esquina es un marco de 1920 × 1080 pegado a su esquina de la pantalla: las medidas se dan como en los
    /// bocetos y, en pantallas más anchas, cada pieza sigue pegada a su borde.
    /// </summary>
    public static class HudPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/HUD";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/MatchHud.prefab";
        private const string ICONS = MODULE_ROOT + "/UI/Icons";

        public static Sprite HudIcon(string name) => ArtSprite($"{ICONS}/Icon_{name}.png");

        public static GameObject Build(MatchRuntime runtime, SelfBetRuntime selfBet, MoneyRuntime money)
        {
            var root = new GameObject("MatchHud", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            var view = root.AddComponent<MatchHudView>();

            BuildQuestionCard(Corner("TopLeft", root.transform, new Vector2(0f, 1f)), view);
            var top = Corner("Top", root.transform, new Vector2(0.5f, 1f));
            BuildClock(top, view);
            BuildLocked(top, view);
            BuildReveal(top, view);
            BuildMoneyCard(Corner("TopRight", root.transform, new Vector2(1f, 1f)), view);
            BuildSelfBetCard(Corner("Bottom", root.transform, new Vector2(0.5f, 0f)), view);
            BuildCarryCard(Corner("BottomRight", root.transform, new Vector2(1f, 0f)), view);
            BuildEscHint(Corner("BottomLeft", root.transform, new Vector2(0f, 0f)), view);

            SerializedWiring.Set(view, "_runtime", runtime);
            SerializedWiring.Set(view, "_money", money);
            SerializedWiring.Set(view, "_selfBet", selfBet);
            SerializedWiring.Set(view, "_avatars", SharedAssets.PlayerAvatars());
            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }

        /// <summary>Marco de 1920 × 1080 con su pivote en una esquina (o en el centro de un borde) de la pantalla.</summary>
        private static Transform Corner(string name, Transform parent, Vector2 anchor)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(1920f, 1080f);
            return rect;
        }

        private static Image Card(string name, Transform parent, float x, float y, float width, float height) =>
            Picture(name, parent, HudPlateSprite(), x, y, width, height);

        /// <summary>Pivote en el centro, para que crezca o lata desde el centro sin moverse.</summary>
        private static void CenterPivot(RectTransform rect)
        {
            var size = rect.sizeDelta;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition += new Vector2(size.x * 0.5f, -size.y * 0.5f);
        }

        private static void PopIn(GameObject card)
        {
            card.AddComponent<CanvasGroup>();
            card.AddComponent<UiPanelIntro>();
        }

        private static void BuildQuestionCard(Transform corner, MatchHudView view)
        {
            var card = Card("QuestionCard", corner, 48f, 36f, 400f, 104f);
            var question = Text("Question", card.transform, string.Empty, 22f, 8f, 360f, 50f, 40f, UiFont.Display, Ivory,
                TextAlignmentOptions.MidlineLeft);
            var icon = Picture("StateIcon", card.transform, null, 22f, 62f, 28f, 28f, preserveAspect: true);
            var state = Text("State", card.transform, string.Empty, 60f, 56f, 320f, 40f, 26f, UiFont.Bold, Ivory,
                TextAlignmentOptions.MidlineLeft);

            // Aviso de dinero fuera de bandejas, bajo la pregunta (boceto 01): así no tapa el rótulo de la pantalla del plató.
            var risk = Picture("AtRisk", corner, HudWarningSprite(), 48f, 152f, 600f, 50f);
            Picture("Icon", risk.transform, HudIcon("Warning"), 10f, 6f, 38f, 38f, preserveAspect: true);
            var riskText = Text("Text", risk.transform, string.Empty, 56f, 2f, 532f, 46f, 22f, UiFont.Body, Ivory,
                TextAlignmentOptions.MidlineLeft);
            SerializedWiring.Set(view, "_risk", risk.gameObject);
            SerializedWiring.Set(view, "_riskText", riskText);

            SerializedWiring.Set(view, "_questionCard", card.gameObject);
            SerializedWiring.Set(view, "_question", question);
            SerializedWiring.Set(view, "_stateIcon", icon);
            SerializedWiring.Set(view, "_state", state);
            SerializedWiring.Set(view, "_openSprite", Shape("HudStateOpen", 62, 4, Hex("#8BE07A"), Hex("#4FAE45"), Hex("#2E6B2A"), highlight: true));
            SerializedWiring.Set(view, "_lockedSprite", Icon("Lock"));
            SerializedWiring.Set(view, "_revealSprite", Icon("Check"));
        }

        private static void BuildClock(Transform top, MatchHudView view)
        {
            var card = Card("ClockCard", top, 820f, 30f, 280f, 96f);
            CenterPivot(card.rectTransform);
            Picture("Icon", card.transform, HudIcon("Clock"), 18f, 14f, 68f, 68f, preserveAspect: true);
            var clock = Text("Time", card.transform, "00:00", 92f, 2f, 172f, 92f, 64f, UiFont.Display, Ivory);

            SerializedWiring.Set(view, "_clockCard", card.gameObject);
            SerializedWiring.Set(view, "_clock", clock);
        }

        /// <summary>Cierre (boceto 05): candado, «APUESTAS CERRADAS», el 3-2-1 en grande y por qué.</summary>
        private static void BuildLocked(Transform top, MatchHudView view)
        {
            var card = Card("LockedCard", top, 600f, 30f, 720f, 172f);
            PopIn(card.gameObject);
            Picture("Icon", card.transform, Icon("Lock"), 34f, 38f, 96f, 96f, preserveAspect: true);
            Text("Title", card.transform, "APUESTAS CERRADAS", 140f, 10f, 550f, 54f, 44f, UiFont.Display, Gold);
            var countdown = Text("Countdown", card.transform, "3", 140f, 56f, 550f, 74f, 84f, UiFont.Display, Ivory);
            CenterPivot(countdown.rectTransform);
            Text("Reason", card.transform, "Ya no se puede mover dinero ni cambiar comodines", 140f, 128f, 550f, 34f, 21f,
                UiFont.Body, Ivory);

            SerializedWiring.Set(view, "_lockedCard", card.gameObject);
            SerializedWiring.Set(view, "_countdown", countdown);
        }

        /// <summary>Resultado de la ronda (boceto 06): conservado, bono y perdido, con las cifras subiendo.</summary>
        private static void BuildReveal(Transform top, MatchHudView view)
        {
            var card = Card("RevealCard", top, 510f, 30f, 900f, 112f);
            PopIn(card.gameObject);
            var values = new TextMeshProUGUI[3];
            var labels = new TextMeshProUGUI[3];
            string[] captions = { "Conservado", "Bono x1", "Perdido" };
            Color[] colors = { Gold, Green, Danger };
            for (var i = 0; i < 3; i++)
            {
                labels[i] = Text($"Label{i}", card.transform, captions[i], i * 300f, 10f, 300f, 34f, 24f, UiFont.Body, Ivory);
                values[i] = Text($"Value{i}", card.transform, "0", i * 300f, 44f, 300f, 58f, 46f, UiFont.Display, colors[i]);
                if (i > 0)
                    Tinted(Picture($"Divider{i}", card.transform, LineSprite(), i * 300f - 1f, 18f, 2f, 76f), new Color(0.84f, 0.67f, 0.38f, 0.5f));
            }

            var eliminated = Picture("Eliminated", top, HudWarningSprite(), 610f, 152f, 700f, 50f);
            var eliminatedText = Text("Text", eliminated.transform, string.Empty, 20f, 2f, 660f, 46f, 24f, UiFont.Bold, Ivory);

            SerializedWiring.Set(view, "_revealCard", card.gameObject);
            SerializedWiring.Set(view, "_kept", values[0]);
            SerializedWiring.Set(view, "_bonusLabel", labels[1]);
            SerializedWiring.Set(view, "_bonus", values[1]);
            SerializedWiring.Set(view, "_lost", values[2]);
            SerializedWiring.Set(view, "_eliminated", eliminated.gameObject);
            SerializedWiring.Set(view, "_eliminatedText", eliminatedText);
        }

        private static void BuildMoneyCard(Transform corner, MatchHudView view)
        {
            var card = Card("MoneyCard", corner, 1452f, 30f, 420f, 152f);
            Text("Label", card.transform, "DINERO DEL EQUIPO", 0f, 12f, 420f, 30f, 22f, UiFont.Bold, Muted);
            var amount = Text("Amount", card.transform, "0", 0f, 40f, 420f, 64f, 60f, UiFont.Display, Gold);
            Divider("Divider", card.transform, 40f, 108f, 340f);
            // Icono y texto centrados juntos bajo la cifra, midan lo que midan.
            var row = KeyRow("SelfBets", card.transform, 0f, 112f, 420f, 34f, 8f);
            var icon = Picture("Icon", row, HudIcon("SelfBet"), 0f, 0f, 30f, 30f, preserveAspect: true);
            var iconSize = icon.gameObject.AddComponent<LayoutElement>();
            iconSize.preferredWidth = iconSize.preferredHeight = 30f;
            var selfBets = LayoutText("Text", row, string.Empty, 22f, UiFont.Body, Ivory);

            SerializedWiring.Set(view, "_moneyCard", card.gameObject);
            SerializedWiring.Set(view, "_teamMoney", amount);
            SerializedWiring.Set(view, "_selfBetRow", row.gameObject);
            SerializedWiring.Set(view, "_selfBets", selfBets);
        }

        /// <summary>Comodín preparado (boceto 03): una previsión si aciertas y el riesgo, encima de los avisos.</summary>
        private static void BuildSelfBetCard(Transform corner, MatchHudView view)
        {
            var card = Card("SelfBetCard", corner, 610f, 700f, 700f, 150f);
            PopIn(card.gameObject);
            Picture("Icon", card.transform, HudIcon("SelfBet"), 24f, 25f, 100f, 100f, preserveAspect: true);
            var title = Text("Title", card.transform, string.Empty, 142f, 14f, 540f, 46f, 36f, UiFont.Display, Gold,
                TextAlignmentOptions.MidlineLeft);
            var projection = Text("Projection", card.transform, string.Empty, 142f, 62f, 540f, 38f, 26f, UiFont.Bold, Ivory,
                TextAlignmentOptions.MidlineLeft);
            Text("Risk", card.transform, "Si fallas, quedas eliminado", 142f, 104f, 540f, 32f, 22f, UiFont.Body, Danger,
                TextAlignmentOptions.MidlineLeft);

            SerializedWiring.Set(view, "_selfBetCard", card.gameObject);
            SerializedWiring.Set(view, "_selfBetTitle", title);
            SerializedWiring.Set(view, "_selfBetProjection", projection);
        }

        /// <summary>Lo que llevas (boceto 02): solo mientras llevas algo.</summary>
        private static void BuildCarryCard(Transform corner, MatchHudView view)
        {
            var card = Card("CarryCard", corner, 1492f, 914f, 380f, 118f);
            PopIn(card.gameObject);
            var icon = Picture("Icon", card.transform, HudIcon("Bundle"), 18f, 15f, 88f, 88f, preserveAspect: true);
            Text("Label", card.transform, "LLEVAS", 122f, 12f, 240f, 26f, 20f, UiFont.Bold, Muted, TextAlignmentOptions.MidlineLeft);
            var carry = Text("Carry", card.transform, string.Empty, 122f, 38f, 240f, 42f, 32f, UiFont.Display, Ivory,
                TextAlignmentOptions.MidlineLeft);
            var limit = Text("Limit", card.transform, string.Empty, 122f, 80f, 240f, 28f, 20f, UiFont.Body, Ivory,
                TextAlignmentOptions.MidlineLeft);

            SerializedWiring.Set(view, "_carryCard", card.gameObject);
            SerializedWiring.Set(view, "_carryIcon", icon);
            SerializedWiring.Set(view, "_carry", carry);
            SerializedWiring.Set(view, "_carryLimit", limit);
            SerializedWiring.SetArray(view, "_denominationIcons", new[] { HudIcon("Bundle"), HudIcon("Bag"), HudIcon("GoldBar") });
        }

        /// <summary>«Esc · Menú», discreto, solo en la primera pregunta.</summary>
        private static void BuildEscHint(Transform corner, MatchHudView view)
        {
            var row = KeyRow("EscHint", corner, 48f, 984f, 260f, 48f, 0f);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;
            KeyHint("Esc", row, "Esc", "Menú", 22f, out _, out var action);
            action.color = Muted;
            SerializedWiring.Set(view, "_escHint", row.gameObject);
        }
    }
}
