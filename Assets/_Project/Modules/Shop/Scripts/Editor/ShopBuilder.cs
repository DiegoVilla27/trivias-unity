using System.Collections.Generic;
using TMPro;
using Trivias.Shared.Editor;
using Trivias.Shared.Profile;
using Trivias.Shop.Components;
using Trivias.Shop.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Shop.Editor
{
    /// <summary>
    /// Construye la tienda (GDD 11): cosméticos con primitivas, catálogo con los precios del GDD (accesorio 300,
    /// color 600, atuendo especial 1.200), la ventana de la tienda y el presentador que viste a los concursantes.
    /// </summary>
    public static class ShopBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Shop";
        private const string PREFABS = MODULE_ROOT + "/Prefabs";
        private const string MATERIALS = MODULE_ROOT + "/Materials";
        public const string CATALOG_PATH = MODULE_ROOT + "/Config/CosmeticCatalog.asset";
        public const string SHOP_PREFAB_PATH = PREFABS + "/Shop.prefab";
        public const string PRESENTER_PREFAB_PATH = PREFABS + "/AvatarCosmetics.prefab";

        public const int ACCESSORY_PRICE = 300;
        public const int COLOR_PRICE = 600;
        public const int SPECIAL_PRICE = 1200;

        private static readonly Color PanelColor = new(0.05f, 0.06f, 0.16f, 0.96f);
        private static readonly Color Gold = new(0.95f, 0.70f, 0.22f);

        public static CosmeticCatalog BuildCatalog()
        {
            var items = new List<CosmeticItem>
            {
                new("gorra", "Gorra", CosmeticSlot.Head, ACCESSORY_PRICE, Cap(), new Vector3(0f, 0.15f, 0f), Color.white),
                new("gafas", "Gafas de sol", CosmeticSlot.Face, ACCESSORY_PRICE, Glasses(), new Vector3(0f, 0.02f, 0.17f), Color.white),
                new("pajarita", "Pajarita", CosmeticSlot.Neck, ACCESSORY_PRICE, BowTie(), new Vector3(0f, -0.1f, 0.08f), Color.white),
                new("color-rojo", "Traje rojo", CosmeticSlot.Color, COLOR_PRICE, null, Vector3.zero, new Color(0.95f, 0.35f, 0.35f)),
                new("color-azul", "Traje azul", CosmeticSlot.Color, COLOR_PRICE, null, Vector3.zero, new Color(0.4f, 0.6f, 1f)),
                new("color-verde", "Traje verde", CosmeticSlot.Color, COLOR_PRICE, null, Vector3.zero, new Color(0.45f, 0.95f, 0.55f)),
                new("corona", "Corona de campeón", CosmeticSlot.Head, SPECIAL_PRICE, Crown(), new Vector3(0f, 0.16f, 0f), Color.white),
                new("chistera", "Chistera", CosmeticSlot.Head, SPECIAL_PRICE, TopHat(), new Vector3(0f, 0.16f, 0f), Color.white),
                new("color-dorado", "Traje dorado", CosmeticSlot.Color, SPECIAL_PRICE, null, Vector3.zero, new Color(1f, 0.82f, 0.35f)),
            };

            var catalog = EditorAssets.GetOrCreateAsset<CosmeticCatalog>(CATALOG_PATH);
            catalog.EditorSet(items);
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        /// <summary>Objeto de escena que viste a todos los concursantes con lo que llevan equipado.</summary>
        public static GameObject BuildPresenter(CosmeticCatalog catalog)
        {
            var root = new GameObject("AvatarCosmetics");
            var presenter = root.AddComponent<AvatarCosmeticsPresenter>();
            SerializedWiring.Set(presenter, "_catalog", catalog);
            SerializedWiring.Set(presenter, "_avatars", SharedAssets.PlayerAvatars());
            return EditorAssets.SaveAsPrefab(root, PRESENTER_PREFAB_PATH);
        }

        public static GameObject BuildShopUi(CosmeticCatalog catalog)
        {
            var root = new GameObject("Shop", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            const float rowHeight = 62f;
            var items = catalog.Items;
            var panelHeight = 200f + items.Count * rowHeight + 90f;
            var panel = CreateRect("Panel", root.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, panelHeight));
            panel.gameObject.AddComponent<Image>().color = PanelColor;

            CreateLabel("Title", panel, "TIENDA", 54f, new Vector2(0f, -28f), new Vector2(760f, 64f), FontStyles.Bold, Gold);
            var tokens = CreateLabel("Tokens", panel, string.Empty, 30f, new Vector2(0f, -96f), new Vector2(760f, 40f),
                FontStyles.Normal, Color.white);
            CreateLabel("Hint", panel, "Solo estética: no da ventajas. Las compras son para siempre.", 20f,
                new Vector2(0f, -140f), new Vector2(760f, 30f), FontStyles.Italic, new Color(1f, 1f, 1f, 0.6f));

            var rows = new ShopItemRow[items.Count];
            for (var i = 0; i < items.Count; i++)
                rows[i] = CreateRow(panel, i, new Vector2(0f, -190f - i * rowHeight));

            var close = CreateButton("Close", panel, "Volver", new Vector2(0f, -panelHeight + 80f), new Vector2(260f, 56f),
                new Color(1f, 1f, 1f, 0.15f), Color.white, out _);

            var view = root.AddComponent<ShopView>();
            SerializedWiring.Set(view, "_catalog", catalog);
            SerializedWiring.Set(view, "_profile", SharedAssets.PlayerProfile());
            SerializedWiring.Set(view, "_openRequested", SharedAssets.OpenShopRequested());
            SerializedWiring.Set(view, "_controlGate", SharedAssets.PlayerControlGate());
            SerializedWiring.Set(view, "_panel", panel.gameObject);
            SerializedWiring.Set(view, "_tokens", tokens);
            SerializedWiring.SetArray(view, "_rows", rows);
            SerializedWiring.Set(view, "_closeButton", close);

            panel.gameObject.SetActive(false);
            return EditorAssets.SaveAsPrefab(root, SHOP_PREFAB_PATH);
        }

        // ---- Filas ----

        private static ShopItemRow CreateRow(RectTransform parent, int index, Vector2 position)
        {
            var rect = CreateRect($"Row{index}", parent, new Vector2(0.5f, 1f), position, new Vector2(760f, 54f));
            rect.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.06f);

            var label = CreateLabel("Label", rect, string.Empty, 26f, new Vector2(-150f, -10f), new Vector2(440f, 36f),
                FontStyles.Normal, Color.white);
            label.alignment = TextAlignmentOptions.MidlineLeft;

            var button = CreateButton("Action", rect, "Comprar", new Vector2(250f, -5f), new Vector2(230f, 44f),
                Gold, new Color(0.08f, 0.08f, 0.18f), out var buttonLabel);

            var row = rect.gameObject.AddComponent<ShopItemRow>();
            SerializedWiring.Set(row, "_itemIndex", index);
            SerializedWiring.Set(row, "_label", label);
            SerializedWiring.Set(row, "_button", button);
            SerializedWiring.Set(row, "_buttonLabel", buttonLabel);
            return row;
        }

        // ---- Cosméticos con primitivas (pivote en la cabeza del concursante) ----

        private static GameObject Cap()
        {
            var root = new GameObject("Gorra");
            Part(root, PrimitiveType.Sphere, "Crown", new Vector3(0f, 0.04f, 0f), new Vector3(0.3f, 0.16f, 0.3f), Mat("Cap_Red", new Color(0.85f, 0.2f, 0.2f)));
            Part(root, PrimitiveType.Cube, "Brim", new Vector3(0f, 0f, 0.16f), new Vector3(0.24f, 0.02f, 0.16f), Mat("Cap_Navy", new Color(0.12f, 0.14f, 0.3f)));
            return Save(root);
        }

        private static GameObject Glasses()
        {
            var root = new GameObject("Gafas");
            var black = Mat("Glasses_Black", new Color(0.05f, 0.05f, 0.07f), 0.9f);
            foreach (var x in new[] { -0.065f, 0.065f })
            {
                var lens = Part(root, PrimitiveType.Cylinder, "Lens", new Vector3(x, 0f, 0f), new Vector3(0.1f, 0.01f, 0.08f), black);
                lens.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            Part(root, PrimitiveType.Cube, "Bridge", Vector3.zero, new Vector3(0.05f, 0.012f, 0.012f), black);
            return Save(root);
        }

        private static GameObject BowTie()
        {
            var root = new GameObject("Pajarita");
            var burgundy = Mat("BowTie_Burgundy", StagePalette.Burgundy);
            foreach (var side in new[] { -1f, 1f })
            {
                var wing = Part(root, PrimitiveType.Cube, "Wing", new Vector3(side * 0.05f, 0f, 0f), new Vector3(0.07f, 0.06f, 0.02f), burgundy);
                wing.transform.localRotation = Quaternion.Euler(0f, 0f, side * 20f);
            }
            Part(root, PrimitiveType.Cube, "Knot", Vector3.zero, new Vector3(0.03f, 0.035f, 0.025f), burgundy);
            return Save(root);
        }

        private static GameObject Crown()
        {
            var root = new GameObject("Corona");
            var gold = Mat("Crown_Gold", StagePalette.Gold, 0.8f, 1f);
            Part(root, PrimitiveType.Cylinder, "Band", Vector3.zero, new Vector3(0.28f, 0.04f, 0.28f), gold);
            for (var i = 0; i < 5; i++)
            {
                var angle = i * Mathf.PI * 2f / 5f;
                Part(root, PrimitiveType.Sphere, "Point", new Vector3(Mathf.Sin(angle) * 0.13f, 0.07f, Mathf.Cos(angle) * 0.13f),
                    Vector3.one * 0.05f, Mat("Crown_Ruby", new Color(0.85f, 0.1f, 0.25f)));
            }
            return Save(root);
        }

        private static GameObject TopHat()
        {
            var root = new GameObject("Chistera");
            var black = Mat("TopHat_Black", new Color(0.06f, 0.06f, 0.08f), 0.4f);
            Part(root, PrimitiveType.Cylinder, "Brim", Vector3.zero, new Vector3(0.36f, 0.008f, 0.36f), black);
            Part(root, PrimitiveType.Cylinder, "Body", new Vector3(0f, 0.13f, 0f), new Vector3(0.22f, 0.13f, 0.22f), black);
            Part(root, PrimitiveType.Cylinder, "Ribbon", new Vector3(0f, 0.04f, 0f), new Vector3(0.225f, 0.025f, 0.225f), Mat("TopHat_Ribbon", StagePalette.Burgundy));
            return Save(root);
        }

        private static GameObject Part(GameObject root, PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material) =>
            EditorAssets.CreatePrimitive(type, name, root.transform, position, scale, material, 0, false, keepCollider: false);

        private static GameObject Save(GameObject root) => EditorAssets.SaveAsPrefab(root, $"{PREFABS}/Cosmetics/{root.name}.prefab");

        private static Material Mat(string name, Color color, float smoothness = 0.4f, float metallic = 0f) =>
            EditorAssets.GetOrCreateMaterial($"{MATERIALS}/{name}.mat", color, smoothness, metallic);

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
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.richText = true;
            label.raycastTarget = false;
            return label;
        }

        private static Button CreateButton(string name, Transform parent, string caption, Vector2 position, Vector2 size,
            Color background, Color textColor, out TextMeshProUGUI label)
        {
            var rect = CreateRect(name, parent, new Vector2(0.5f, 1f), position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = background;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            label = CreateLabel("Label", rect, caption, 24f, new Vector2(0f, -(size.y - 32f) * 0.5f), new Vector2(size.x - 16f, 32f),
                FontStyles.Bold, textColor);
            return button;
        }
    }
}
