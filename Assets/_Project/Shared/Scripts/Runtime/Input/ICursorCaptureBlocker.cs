using UnityEngine;

namespace Trivias.Shared.Input
{
    /// <summary>Interfaz en pantalla que necesita el ratón libre: un clic sobre ella no debe capturar el cursor.</summary>
    public interface ICursorCaptureBlocker
    {
        /// <param name="screenPosition">Posición del ratón en píxeles, origen abajo a la izquierda (Input System).</param>
        bool BlocksAt(Vector2 screenPosition);
    }
}
