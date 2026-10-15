using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Trivias.App.Editor
{
    /// <summary>
    /// Mantiene prefabs y escena siempre al día sin tener que pulsar Build All.
    /// Tras cada compilación compara una huella del código y del contenido con la del último build;
    /// si no coinciden, reconstruye todo antes de que se pueda jugar con una escena desfasada.
    /// </summary>
    [InitializeOnLoad]
    internal static class AutoBuild
    {
        private const string PROJECT_ROOT = "Assets/_Project";
        private const string STAMP_PATH = PROJECT_ROOT + "/Settings/BuildStamp.txt";

        static AutoBuild()
        {
            // En modo consola el build se lanza explícitamente (y los tests no deben reconstruir).
            if (Application.isBatchMode)
                return;
            EditorApplication.delayCall += RebuildIfOutdated;
        }

        public static bool IsOutdated => ReadStamp() != ComputeFingerprint();

        /// <summary>Guarda la huella del estado actual: lo llama Build All al terminar.</summary>
        public static void WriteStamp()
        {
            File.WriteAllText(STAMP_PATH, ComputeFingerprint());
            AssetDatabase.ImportAsset(STAMP_PATH);
        }

        private static void RebuildIfOutdated()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || !IsOutdated)
                return;

            // Si hay otra escena con cambios sin guardar, se pregunta antes de reconstruir.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Trivias] La escena está desfasada respecto al código. Ejecuta Tools > Trivias > Build All.");
                return;
            }

            Debug.Log("[Trivias] El código cambió: reconstruyendo prefabs y escena automáticamente...");
            ProjectBuilder.BuildAll();
        }

        /// <summary>Huella de lo que determina prefabs y escena: scripts, contenido y controles.</summary>
        private static string ComputeFingerprint()
        {
            var files = Directory.GetFiles(PROJECT_ROOT, "*.*", SearchOption.AllDirectories)
                .Where(path => path.EndsWith(".cs") || path.EndsWith(".json") || path.EndsWith(".inputactions") ||
                               path.EndsWith(".asmdef"))
                .Where(path => !path.Replace('\\', '/').Contains("/Tests/"))
                .OrderBy(path => path, StringComparer.Ordinal);

            using var md5 = MD5.Create();
            var builder = new StringBuilder();
            foreach (var file in files)
            {
                builder.Append(file.Replace('\\', '/')).Append('\n');
                builder.Append(File.ReadAllText(file).Replace("\r", string.Empty)).Append('\n');
            }

            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        private static string ReadStamp() =>
            File.Exists(STAMP_PATH) ? File.ReadAllText(STAMP_PATH).Trim() : string.Empty;
    }
}
