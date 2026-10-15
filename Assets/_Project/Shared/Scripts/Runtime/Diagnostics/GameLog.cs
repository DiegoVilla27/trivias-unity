using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Trivias.Shared.Diagnostics
{
    /// <summary>
    /// Logs de depuración. Las llamadas a Info/Warning desaparecen por completo (también sus argumentos)
    /// fuera del editor y de las development builds. Error se conserva siempre.
    /// </summary>
    public static class GameLog
    {
        private const string EDITOR = "UNITY_EDITOR";
        private const string DEV_BUILD = "DEVELOPMENT_BUILD";

        [Conditional(EDITOR), Conditional(DEV_BUILD)]
        public static void Info(string message, UnityEngine.Object context = null) => Debug.Log(message, context);

        [Conditional(EDITOR), Conditional(DEV_BUILD)]
        public static void Warning(string message, UnityEngine.Object context = null) => Debug.LogWarning(message, context);

        public static void Error(string message, UnityEngine.Object context = null) => Debug.LogError(message, context);
    }
}
