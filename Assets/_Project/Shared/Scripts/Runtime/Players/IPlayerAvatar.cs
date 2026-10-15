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

        /// <summary>Eliminado hasta la siguiente partida (GDD 5.3): ni se mueve, ni toca dinero, ni verifica.</summary>
        bool IsEliminated { get; }

        /// <summary>Solo en el host: elimina al concursante (se replica a todos y reproduce la expulsión).</summary>
        void Eliminate();
    }
}
