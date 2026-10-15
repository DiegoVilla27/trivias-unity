namespace Trivias.Shared.Interaction
{
    /// <summary>Tecla de interacción pulsada. Cada interactivo decide qué significa en su contexto.</summary>
    public enum InteractionAction : byte
    {
        /// <summary>E: una unidad (coger, dejar o recuperar un objeto).</summary>
        Primary = 0,

        /// <summary>Q: varias unidades (todas las que se puedan).</summary>
        Secondary = 1,

        /// <summary>R: acción especial del interactivo (p. ej. devolver todo a la mesa).</summary>
        Tertiary = 2,
    }

    /// <summary>Quién interactúa y con qué tecla. Se pasa por valor para no generar basura.</summary>
    public readonly struct InteractionContext
    {
        public readonly ulong ClientId;
        public readonly InteractionAction Action;

        public InteractionContext(ulong clientId, InteractionAction action = InteractionAction.Primary)
        {
            ClientId = clientId;
            Action = action;
        }
    }
}
