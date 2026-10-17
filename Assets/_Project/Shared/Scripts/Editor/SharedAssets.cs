using System.Collections.Generic;
using Trivias.Shared.Characters;
using Trivias.Shared.Events;
using Trivias.Shared.Gameplay;
using Trivias.Shared.Input;
using Trivias.Shared.Interaction;
using Trivias.Shared.Players;
using Trivias.Shared.Profile;
using Trivias.Shared.Rooms;
using Trivias.Shared.Settings;
using Trivias.Shared.Spawning;
using Trivias.Shared.Tubes;

namespace Trivias.Shared.Editor
{
    /// <summary>Assets compartidos entre módulos (canales de eventos, datos del plató). Se crean si no existen.</summary>
    public static class SharedAssets
    {
        public const string CONFIG_ROOT = "Assets/_Project/Shared/Config";
        private const string LOCAL_PLAYER_CAMERA_TARGET_PATH = CONFIG_ROOT + "/Events/LocalPlayerCameraTarget.asset";
        private const string STAGE_SPAWN_POINTS_PATH = CONFIG_ROOT + "/Spawning/StageSpawnPoints.asset";
        private const string INTERACTABLE_REGISTRY_PATH = CONFIG_ROOT + "/Interaction/InteractableRegistry.asset";
        private const string PLAYER_AVATAR_REGISTRY_PATH = CONFIG_ROOT + "/Players/PlayerAvatarRegistry.asset";
        private const string MATCH_START_REQUESTED_PATH = CONFIG_ROOT + "/Events/MatchStartRequested.asset";
        private const string PLAYER_CONTROL_GATE_PATH = CONFIG_ROOT + "/Gameplay/PlayerControlGate.asset";
        private const string PLAYER_MOVEMENT_LIMIT_PATH = CONFIG_ROOT + "/Gameplay/PlayerMovementLimit.asset";
        private const string VERIFY_FROM_ZONE_PATH = CONFIG_ROOT + "/Events/VerifyFromZoneRequested.asset";
        private const string RETURNED_TO_LOBBY_PATH = CONFIG_ROOT + "/Events/ReturnedToLobby.asset";
        private const string LEAVE_ROOM_REQUESTED_PATH = CONFIG_ROOT + "/Events/LeaveRoomRequested.asset";
        private const string ROOM_CONNECTION_PATH = CONFIG_ROOT + "/Rooms/RoomConnectionRuntime.asset";
        private const string PLAYER_PROFILE_PATH = CONFIG_ROOT + "/Profile/PlayerProfile.asset";
        private const string GAME_SETTINGS_PATH = CONFIG_ROOT + "/Settings/GameSettings.asset";
        private const string OPEN_SHOP_PATH = CONFIG_ROOT + "/Events/OpenShopRequested.asset";
        private const string CURSOR_CAPTURE_BLOCKERS_PATH = CONFIG_ROOT + "/Input/CursorCaptureBlockers.asset";
        private const string TUBE_REGISTRY_PATH = CONFIG_ROOT + "/Stage/TubeRegistry.asset";
        public const string CHARACTER_CATALOG_PATH = CONFIG_ROOT + "/Characters/CharacterCatalog.asset";
        private const string CHARACTER_PREVIEW_PATH = CONFIG_ROOT + "/Characters/CharacterPreview.asset";

        /// <summary>Tamaño de la vista del personaje en el menú (vertical, con transparencia).</summary>
        public const int CHARACTER_PREVIEW_WIDTH = 640;
        public const int CHARACTER_PREVIEW_HEIGHT = 800;

        /// <summary>Precio de cada personaje de pago: el de un atuendo especial (GDD 11).</summary>
        public const int CHARACTER_PRICE = 1200;

        /// <summary>Vista 3D del personaje en el menú: qué se quiere ver y la imagen donde se pinta.</summary>
        public static CharacterPreview CharacterPreview() => EditorAssets.GetOrCreateAsset<CharacterPreview>(CHARACTER_PREVIEW_PATH);

        /// <summary>
        /// Los diez personajes (bocetos de docs/bocetos/player/characters): id, nombre, carpeta de su modelo en
        /// Player/Models/Characters y un color que los representa.
        /// </summary>
        public static readonly (string id, string name, string folder, UnityEngine.Color tint)[] Characters =
        {
            ("tracksuit", "Blue Tracksuit", "Tracksuit", new(0.25f, 0.5f, 0.95f)),
            ("casual", "Casual", "Casual", new(0.95f, 0.55f, 0.45f)),
            ("gardener", "Gardener", "Gardener", new(0.45f, 0.65f, 0.45f)),
            ("elegant", "Elegant", "Elegant", new(0.3f, 0.45f, 0.85f)),
            ("explorer", "Explorer", "Explorer", new(0.8f, 0.72f, 0.5f)),
            ("pajamas", "Pajamas", "Pajamas", new(0.72f, 0.66f, 0.95f)),
            ("raincoat", "Raincoat", "Raincoat", new(0.98f, 0.82f, 0.3f)),
            ("chef", "Chef", "Chef", new(0.95f, 0.95f, 0.95f)),
            ("retro-sport", "Retro Sport", "RetroSport", new(0.85f, 0.25f, 0.3f)),
            ("winter", "Winter", "Winter", new(0.3f, 0.6f, 0.35f)),
        };

        /// <summary>Los seis gratuitos de momento, como en los bocetos (se cambian en el inspector: Free Character Ids).</summary>
        private static readonly string[] DefaultFreeIds = { "tracksuit", "casual", "gardener", "elegant", "explorer", "pajamas" };

        /// <summary>
        /// Catálogo de personajes. Solo se rellena si está vacío: lo que se cambie en el inspector (p. ej. cuáles son
        /// gratuitos) no lo pisa ningún Build All.
        /// </summary>
        public static CharacterCatalog CharacterCatalog()
        {
            var catalog = EditorAssets.GetOrCreateAsset<CharacterCatalog>(CHARACTER_CATALOG_PATH);
            // Se rellena si está vacío o aún tiene los robots provisionales.
            if (catalog.Characters.Count > 0 && catalog.Find("robot-rojo") == null)
                return catalog;
            var characters = new List<CharacterDefinition>();
            foreach (var (id, name, _, tint) in Characters)
                characters.Add(new CharacterDefinition(id, name, tint));
            catalog.EditorSet(characters, (string[])DefaultFreeIds.Clone(), CHARACTER_PRICE);
            UnityEditor.EditorUtility.SetDirty(catalog);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(catalog);
            return catalog;
        }

        /// <summary>Tubos de las cabinas (dinero y concursante), para las succiones.</summary>
        public static TubeRegistry TubeRegistry() => EditorAssets.GetOrCreateAsset<TubeRegistry>(TUBE_REGISTRY_PATH);

        public static TransformEventChannel LocalPlayerCameraTarget() =>
            EditorAssets.GetOrCreateAsset<TransformEventChannel>(LOCAL_PLAYER_CAMERA_TARGET_PATH);

        public static InteractableRegistry InteractableRegistry() =>
            EditorAssets.GetOrCreateAsset<InteractableRegistry>(INTERACTABLE_REGISTRY_PATH);

        public static PlayerMovementLimit PlayerMovementLimit() =>
            EditorAssets.GetOrCreateAsset<PlayerMovementLimit>(PLAYER_MOVEMENT_LIMIT_PATH);

        /// <summary>Petición local de verificar desde una zona de apuesta (cuando todos los activos están apostados).</summary>
        public static VoidEventChannel VerifyFromZoneRequested() =>
            EditorAssets.GetOrCreateAsset<VoidEventChannel>(VERIFY_FROM_ZONE_PATH);

        /// <summary>Lo lanza el host al volver a la sala de espera tras una partida.</summary>
        public static VoidEventChannel ReturnedToLobby() =>
            EditorAssets.GetOrCreateAsset<VoidEventChannel>(RETURNED_TO_LOBBY_PATH);

        /// <summary>Petición local de salir de la sala (p. ej. desde la pantalla de resultados).</summary>
        public static VoidEventChannel LeaveRoomRequested() =>
            EditorAssets.GetOrCreateAsset<VoidEventChannel>(LEAVE_ROOM_REQUESTED_PATH);

        /// <summary>Fondo de los menús (plató desenfocado): lo piden las pantallas abiertas.</summary>
        public static Trivias.Shared.Presentation.UiBackdrop UiBackdrop() =>
            EditorAssets.GetOrCreateAsset<Trivias.Shared.Presentation.UiBackdrop>(CONFIG_ROOT + "/UI/UiBackdrop.asset");

        public static PlayerProfile PlayerProfile() =>
            EditorAssets.GetOrCreateAsset<PlayerProfile>(PLAYER_PROFILE_PATH);

        /// <summary>Opciones del jugador (volúmenes, ratón, pantalla).</summary>
        public static GameSettings GameSettings() => EditorAssets.GetOrCreateAsset<GameSettings>(GAME_SETTINGS_PATH);

        /// <summary>Petición local de abrir la tienda (botón del vestíbulo).</summary>
        public static VoidEventChannel OpenShopRequested() =>
            EditorAssets.GetOrCreateAsset<VoidEventChannel>(OPEN_SHOP_PATH);

        public static RoomConnectionRuntime RoomConnection() =>
            EditorAssets.GetOrCreateAsset<RoomConnectionRuntime>(ROOM_CONNECTION_PATH);

        public static CursorCaptureBlockers CursorCaptureBlockers() =>
            EditorAssets.GetOrCreateAsset<CursorCaptureBlockers>(CURSOR_CAPTURE_BLOCKERS_PATH);

        public static PlayerControlGate PlayerControlGate() =>
            EditorAssets.GetOrCreateAsset<PlayerControlGate>(PLAYER_CONTROL_GATE_PATH);

        public static VoidEventChannel MatchStartRequested() =>
            EditorAssets.GetOrCreateAsset<VoidEventChannel>(MATCH_START_REQUESTED_PATH);

        public static PlayerAvatarRegistry PlayerAvatars() =>
            EditorAssets.GetOrCreateAsset<PlayerAvatarRegistry>(PLAYER_AVATAR_REGISTRY_PATH);

        public static SpawnPointSet StageSpawnPoints() =>
            EditorAssets.GetOrCreateAsset<SpawnPointSet>(STAGE_SPAWN_POINTS_PATH);
    }
}
