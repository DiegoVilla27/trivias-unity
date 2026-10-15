using UnityEngine;

namespace Trivias.Shared.Interaction
{
    /// <summary>
    /// Algo con lo que el jugador local puede interactuar por cercanía (mesa, bandejas, estación, botón...).
    /// Lo implementa cada módulo; el módulo Interaction solo elige el más cercano y llama a <see cref="Interact"/>.
    /// </summary>
    public interface IInteractable
    {
        Vector3 Position { get; }

        /// <summary>Distancia máxima (en el plano horizontal) a la que se puede interactuar.</summary>
        float Radius { get; }

        /// <summary>Texto del aviso ("E: coger fajo"). False si ahora no se puede interactuar.</summary>
        bool TryGetPrompt(in InteractionContext context, out string prompt);

        void Interact(in InteractionContext context);
    }
}
