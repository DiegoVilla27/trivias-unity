using System.Collections.Generic;
using TMPro;
using Trivias.Shared.Editor;
using Trivias.Shared.Presentation;
using Trivias.Shared.Profile;
using Trivias.Shop.Components;
using Trivias.Shop.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Shop.Editor
{
    /// <summary>
    /// Construye la tienda (GDD 11): cosméticos con primitivas, catálogo con los precios del GDD (accesorio 300, especial
    /// 1.200), los personajes de pago (del catálogo de personajes), la ventana de la tienda y el presentador que viste a
    /// los concursantes.
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
        public const int SPECIAL_PRICE = 1200;


        public static CosmeticCatalog BuildCatalog()
        {
            var items = new List<CosmeticItem>
            {
                new("gorra", "Gorra", CosmeticSlot.Head, ACCESSORY_PRICE, Cap(), new Vector3(0f, 0.15f, 0f)),
                new("gafas", "Gafas de sol", CosmeticSlot.Face, ACCESSORY_PRICE, Glasses(), new Vector3(0f, 0.02f, 0.17f)),
                new("pajarita", "Pajarita", CosmeticSlot.Neck, ACCESSORY_PRICE, BowTie(), new Vector3(0f, -0.1f, 0.08f)),
                new("corona", "Corona de campeón", CosmeticSlot.Head, SPECIAL_PRICE, Crown(), new Vector3(0f, 0.16f, 0f)),
                new("chistera", "Chistera", CosmeticSlot.Head, SPECIAL_PRICE, TopHat(), new Vector3(0f, 0.16f, 0f)),
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

        public const int CHARACTER_CARDS = 10;
        private const string SHOP_TITLE = MODULE_ROOT + "/UI/Title_Shop.png";

        /// <summary>La tienda como los bocetos 04 (personajes) y 05 (accesorios), a 1920 × 1080.</summary>
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

            var overlay = UiKit.FullScreen("Panel", root.transform);
            var veil = overlay.gameObject.AddComponent<Image>();
            veil.sprite = UiKit.DotSprite(); // Con imagen propia: el tema no la toca.
            veil.color = new Color(0.02f, 0.04f, 0.1f, 0.3f);
            var screen = UiKit.Screen("Screen", overlay);

            UiKit.Picture("Title", screen, UiKit.ArtSprite(SHOP_TITLE), 128f, 22f, 470f, 128f, preserveAspect: true);
            var tokens = UiKit.Picture("Tokens", screen, UiKit.SecondarySprite(), 1472f, 52f, 302f, 66f);
            UiKit.Picture("Icon", tokens.transform, UiKit.Icon("Token"), 16f, 8f, 50f, 50f, preserveAspect: true);
            var tokensText = UiKit.Text("Text", tokens.transform, string.Empty, 76f, 2f, 214f, 62f, 30f, UiKit.UiFont.Bold, UiKit.Ivory,
                TextAlignmentOptions.MidlineLeft);

            // Pestañas.
            UiKit.Picture("Tabs", screen, UiKit.SecondarySprite(), 129f, 149f, 853f, 78f);
            var charactersTab = UiKit.MakeButton("CharactersTab", screen, "Personajes", 135f, 155f, 452f, 66f, UiKit.ButtonStyle.Primary,
                out var charactersLabel, 34f);
            var accessoriesTab = UiKit.MakeButton("AccessoriesTab", screen, "Accesorios", 594f, 155f, 382f, 66f, UiKit.ButtonStyle.Secondary,
                out var accessoriesLabel, 34f);

            // Tarjetas.
            var grid = UiKit.Panel("Grid", screen, 106f, 236f, 1108f, 748f);
            var charactersGrid = UiKit.Rect("Characters", grid, 0f, 0f, 1108f, 748f);
            var characterCards = new ShopCard[CHARACTER_CARDS];
            for (var i = 0; i < CHARACTER_CARDS; i++)
                characterCards[i] = BuildCard(charactersGrid, $"Character{i + 1}", 24f + (i % 5) * 215f, 26f + (i / 5) * 360f, 200f, 340f,
                    fullBody: true);
            var accessoriesGrid = UiKit.Rect("Accessories", grid, 0f, 0f, 1108f, 748f);
            var accessoryCards = new ShopCard[catalog.Items.Count];
            for (var i = 0; i < accessoryCards.Length; i++)
                accessoryCards[i] = BuildCard(accessoriesGrid, $"Accessory{i + 1}", 46f + (i % 3) * 348f, 26f + (i / 3) * 362f, 320f, 342f,
                    fullBody: false);
            accessoriesGrid.gameObject.SetActive(false);

            // A la derecha: el artículo en 3D.
            var detail = UiKit.Panel("Detail", screen, 1226f, 140f, 573f, 845f);
            UiKit.Tinted(UiKit.Picture("Spotlight", detail, UiKit.GlowSprite(), 36f, 30f, 500f, 600f), new Color(0.55f, 0.65f, 1f, 0.22f));
            var preview = UiKit.Rect("Preview", detail, 46f, 24f, 480f, 600f).gameObject.AddComponent<RawImage>();
            preview.raycastTarget = false;
            var previewBinder = preview.gameObject.AddComponent<CharacterPreviewImage>();
            SerializedWiring.Set(previewBinder, "_preview", SharedAssets.CharacterPreview());
            SerializedWiring.Set(previewBinder, "_image", preview);
            var previous = UiKit.IconButton("Previous", detail, UiKit.Icon("ArrowLeft"), 25f, 284f, 92f);
            var next = UiKit.IconButton("Next", detail, UiKit.Icon("ArrowRight"), 456f, 284f, 92f);
            var plate = UiKit.Picture("NamePlate", detail, UiKit.PlateSprite(), 114f, 586f, 345f, 60f);
            var name = UiKit.Text("Name", plate.transform, string.Empty, 8f, 2f, 329f, 56f, 34f, UiKit.UiFont.Bold, UiKit.Ivory);
            var badge = UiKit.Pill("Badge", detail, UiKit.PillGoldSprite(), UiKit.Icon("Token"), string.Empty, 137f, 652f, 299f, 50f,
                UiKit.DarkText, out var badgeText, out var badgeIcon, 26f);
            var action = UiKit.MakeButton("Action", detail, string.Empty, 63f, 718f, 447f, 76f, UiKit.ButtonStyle.Primary, out var actionLabel, 36f);
            UiKit.Text("Hint", detail, "Solo apariencia. Sin ventajas.", 40f, 800f, 493f, 32f, 22f, UiKit.UiFont.Body, UiKit.Muted);

            var close = UiKit.MakeButton("Close", screen, "Volver a la sala", 797f, 998f, 326f, 58f, UiKit.ButtonStyle.Secondary, 28f);

            var view = root.AddComponent<ShopView>();
            SerializedWiring.Set(view, "_catalog", catalog);
            SerializedWiring.Set(view, "_characters", SharedAssets.CharacterCatalog());
            SerializedWiring.Set(view, "_profile", SharedAssets.PlayerProfile());
            SerializedWiring.Set(view, "_preview", SharedAssets.CharacterPreview());
            SerializedWiring.Set(view, "_backdrop", SharedAssets.UiBackdrop());
            SerializedWiring.Set(view, "_openRequested", SharedAssets.OpenShopRequested());
            SerializedWiring.Set(view, "_controlGate", SharedAssets.PlayerControlGate());
            SerializedWiring.Set(view, "_panel", overlay.gameObject);
            SerializedWiring.Set(view, "_tokens", tokensText);
            SerializedWiring.Set(view, "_charactersTab", charactersTab);
            SerializedWiring.Set(view, "_accessoriesTab", accessoriesTab);
            SerializedWiring.Set(view, "_charactersTabLabel", charactersLabel);
            SerializedWiring.Set(view, "_accessoriesTabLabel", accessoriesLabel);
            SerializedWiring.Set(view, "_charactersGrid", charactersGrid.gameObject);
            SerializedWiring.Set(view, "_accessoriesGrid", accessoriesGrid.gameObject);
            SerializedWiring.SetArray(view, "_characterCards", characterCards);
            SerializedWiring.SetArray(view, "_accessoryCards", accessoryCards);
            SerializedWiring.Set(view, "_previousButton", previous);
            SerializedWiring.Set(view, "_nextButton", next);
            SerializedWiring.Set(view, "_name", name);
            SerializedWiring.Set(view, "_badge", badge);
            SerializedWiring.Set(view, "_badgeIcon", badgeIcon);
            SerializedWiring.Set(view, "_badgeText", badgeText);
            SerializedWiring.Set(view, "_actionButton", action);
            SerializedWiring.Set(view, "_actionLabel", actionLabel);
            SerializedWiring.Set(view, "_closeButton", close);
            SerializedWiring.Set(view, "_tabSelected", UiKit.PrimarySprite());
            SerializedWiring.Set(view, "_tabIdle", UiKit.SecondarySprite());
            SerializedWiring.Set(view, "_card", UiKit.CardSprite());
            SerializedWiring.Set(view, "_cardSelected", UiKit.CardSelectedSprite());
            SerializedWiring.Set(view, "_badgeDark", UiKit.PillSprite());
            SerializedWiring.Set(view, "_badgeGreen", UiKit.PillGreenSprite());
            SerializedWiring.Set(view, "_badgeGold", UiKit.PillGoldSprite());
            SerializedWiring.Set(view, "_checkIcon", UiKit.Icon("Check"));
            SerializedWiring.Set(view, "_tokenIcon", UiKit.Icon("Token"));

            overlay.gameObject.SetActive(false);
            return EditorAssets.SaveAsPrefab(root, SHOP_PREFAB_PATH);
        }

        /// <summary>Tarjeta: foto (cuerpo entero o cabeza con el complemento), nombre y pastilla de estado.</summary>
        private static ShopCard BuildCard(Transform parent, string name, float x, float y, float width, float height, bool fullBody)
        {
            var background = UiKit.Picture(name, parent, UiKit.CardSprite(), x, y, width, height);
            background.raycastTarget = true;
            var button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            background.gameObject.AddComponent<Trivias.Shared.Presentation.UiButtonFeel>();
            var photoHeight = height - 104f;
            var photoWidth = fullBody ? photoHeight * 0.8f : photoHeight;
            var portrait = UiKit.Rect("Portrait", background.transform, (width - photoWidth) * 0.5f, 6f, photoWidth, photoHeight)
                .gameObject.AddComponent<RawImage>();
            portrait.raycastTarget = false;
            var label = UiKit.Text("Name", background.transform, string.Empty, 6f, height - 100f, width - 12f, 40f, fullBody ? 25f : 30f,
                UiKit.UiFont.Bold, UiKit.Ivory);
            var badge = UiKit.Pill("Badge", background.transform, UiKit.PillSprite(), UiKit.Icon("Check"), string.Empty, 10f, height - 56f,
                width - 20f, 44f, UiKit.Ivory, out var badgeText, out var badgeIcon, fullBody ? 21f : 25f);
            var padlock = UiKit.Picture("Lock", badge.transform, UiKit.Icon("Lock"), width - 58f, 7f, 30f, 30f, preserveAspect: true);
            badgeText.rectTransform.sizeDelta -= new Vector2(30f, 0f); // Sitio para el candado.

            var card = background.gameObject.AddComponent<ShopCard>();
            SerializedWiring.Set(card, "_button", button);
            SerializedWiring.Set(card, "_background", background);
            SerializedWiring.Set(card, "_portrait", portrait);
            SerializedWiring.Set(card, "_name", label);
            SerializedWiring.Set(card, "_badge", badge);
            SerializedWiring.Set(card, "_badgeIcon", badgeIcon);
            SerializedWiring.Set(card, "_badgeText", badgeText);
            SerializedWiring.Set(card, "_lock", padlock);
            return card;
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

    }
}
