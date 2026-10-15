using System;
using System.Linq;
using UnityEditor.SceneManagement;

namespace Trivias.App.Editor
{
    /// <summary>
    /// Ejecuta algo que cambia de escena (p. ej. Build All, que recrea Stage) y después devuelve al usuario
    /// las escenas que tenía abiertas, para que la reconstrucción automática no le saque de lo que estaba haciendo.
    /// </summary>
    public static class OpenScenesKeeper
    {
        public static void Run(Action action)
        {
            var setup = EditorSceneManager.GetSceneManagerSetup();
            action();

            // Una escena nueva sin guardar no se puede volver a abrir: en ese caso se queda la que dejó la acción.
            if (setup.Length == 0 || setup.Any(scene => string.IsNullOrEmpty(scene.path)))
                return;
            EditorSceneManager.RestoreSceneManagerSetup(setup);
        }
    }
}
