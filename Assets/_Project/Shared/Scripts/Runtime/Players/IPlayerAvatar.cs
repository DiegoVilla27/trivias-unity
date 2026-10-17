using Trivias.Shared.Profile;
using UnityEngine;

namespace Trivias.Shared.Players
{
    /// <summary>
    /// Un concursante en la partida, visto por otros módulos sin conocer el módulo Player:
    /// nombre, posición, dónde sujeta el dinero y si está eliminado.
    /// </summary>
    public interface IPlayerAvatar
    {
        ulong ClientId { get; }
        string DisplayName { get; }
        Vector3 Position { get; }
        Transform CarryPoint { get; }

        /// <summary>Hueso de la cabeza: ahí se colocan gorras, gafas, coronas...</summary>
        Transform HeadAnchor { get; }

        /// <summary>Personaje que lleva (id del catálogo de personajes); el host evita que se repita en la sala.</summary>
        string CharacterId { get; }

        /// <summary>Cosméticos que lleva puestos (iguales en todos los clientes).</summary>
        CosmeticLoadout Loadout { get; }

        /// <summary>Es el concursante de este ordenador.</summary>
        bool IsLocal { get; }

        /// <summary>Punto que sigue la cámara en tercera persona (también la del espectador que lo mira).</summary>
        Transform ViewTarget { get; }

        /// <summary>Eliminado hasta la siguiente partida (GDD 5.3): ni se mueve, ni toca dinero, ni verifica.</summary>
        bool IsEliminated { get; }

        /// <summary>Solo en el host: elimina al concursante (se replica a todos y reproduce la expulsión).</summary>
        void Eliminate();

        /// <summary>Solo en el host: nueva partida; vuelve a estar activo y a su punto de aparición.</summary>
        void Respawn();
    }
}
