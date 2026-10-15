using UnityEngine;

namespace Trivias.Menu.Components
{
    /// <summary>Cierra el juego (en el editor, detiene el modo de juego).</summary>
    internal static class GameQuit
    {
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
