using Trivias.Shared.Carrying;
using Trivias.Shared.Events;
using Trivias.Shared.Interaction;
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
        private const string CARRY_SOCKET_REGISTRY_PATH = CONFIG_ROOT + "/Carrying/CarrySocketRegistry.asset";

        public static TransformEventChannel LocalPlayerCameraTarget() =>
            EditorAssets.GetOrCreateAsset<TransformEventChannel>(LOCAL_PLAYER_CAMERA_TARGET_PATH);

        public static InteractableRegistry InteractableRegistry() =>
            EditorAssets.GetOrCreateAsset<InteractableRegistry>(INTERACTABLE_REGISTRY_PATH);

        public static CarrySocketRegistry CarrySocketRegistry() =>
            EditorAssets.GetOrCreateAsset<CarrySocketRegistry>(CARRY_SOCKET_REGISTRY_PATH);

        public static SpawnPointSet StageSpawnPoints() =>
            EditorAssets.GetOrCreateAsset<SpawnPointSet>(STAGE_SPAWN_POINTS_PATH);
    }
}
