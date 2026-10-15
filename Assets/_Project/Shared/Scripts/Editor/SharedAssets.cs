using Trivias.Shared.Events;
using Trivias.Shared.Gameplay;
using Trivias.Shared.Input;
using Trivias.Shared.Interaction;
using Trivias.Shared.Players;
using Trivias.Shared.Spawning;

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
        private const string CURSOR_CAPTURE_BLOCKERS_PATH = CONFIG_ROOT + "/Input/CursorCaptureBlockers.asset";

        public static TransformEventChannel LocalPlayerCameraTarget() =>
            EditorAssets.GetOrCreateAsset<TransformEventChannel>(LOCAL_PLAYER_CAMERA_TARGET_PATH);

        public static InteractableRegistry InteractableRegistry() =>
            EditorAssets.GetOrCreateAsset<InteractableRegistry>(INTERACTABLE_REGISTRY_PATH);

        public static PlayerMovementLimit PlayerMovementLimit() =>
            EditorAssets.GetOrCreateAsset<PlayerMovementLimit>(PLAYER_MOVEMENT_LIMIT_PATH);

        /// <summary>Petición local de verificar desde una zona de apuesta (cuando todos los activos están apostados).</summary>
        public static VoidEventChannel VerifyFromZoneRequested() =>
            EditorAssets.GetOrCreateAsset<VoidEventChannel>(VERIFY_FROM_ZONE_PATH);

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
