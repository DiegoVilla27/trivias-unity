namespace Trivias.Shared.Physics
{
    /// <summary>
    /// Índices y máscaras de capas, fijados en ProjectSettings/TagManager.asset.
    /// Constantes: se resuelven en compilación, sin búsquedas por nombre en runtime.
    /// </summary>
    public static class GameLayers
    {
        public const int DEFAULT = 0;
        public const int PLAYER = 6;
        public const int MONEY = 7;
        public const int INTERACTABLE = 8;
        public const int STAGE = 9;

        public const int DEFAULT_MASK = 1 << DEFAULT;
        public const int PLAYER_MASK = 1 << PLAYER;
        public const int MONEY_MASK = 1 << MONEY;
        public const int INTERACTABLE_MASK = 1 << INTERACTABLE;
        public const int STAGE_MASK = 1 << STAGE;

        /// <summary>Superficies sobre las que se puede caminar.</summary>
        public const int GROUND_MASK = DEFAULT_MASK | STAGE_MASK;
    }
}
