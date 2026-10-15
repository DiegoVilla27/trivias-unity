using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Trivias.App.Editor
{
    /// <summary>
    /// Build para compartir con un clic (docs/JUGAR_CON_AMIGOS.md): compila el juego y deja un zip en <c>Builds/</c>.
    /// </summary>
    public static class GameBuilds
    {
        public const string BUILDS_ROOT = "Builds";
        private const string GAME_NAME = "Trivias";

        [MenuItem("Tools/Trivias/Crear build/macOS")]
        public static void BuildMac() => Build(BuildTarget.StandaloneOSX, "Mac", $"{GAME_NAME}.app");

        [MenuItem("Tools/Trivias/Crear build/Windows")]
        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "Windows", $"{GAME_NAME}.exe");

        /// <summary>Compila para la plataforma y comprime el resultado. Devuelve la ruta del zip o null si no se pudo.</summary>
        public static string Build(BuildTarget target, string platformName, string executable)
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, target))
            {
                EditorUtility.DisplayDialog("Falta el módulo de la plataforma",
                    $"Para crear la build de {platformName} instala el módulo gratuito en Unity Hub → Installs → ⚙ → " +
                    $"Add modules → «{(target == BuildTarget.StandaloneOSX ? "Mac Build Support" : "Windows Build Support (Mono)")}».",
                    "Entendido");
                return null;
            }

            // La build siempre parte de la escena al día con el código.
            if (AutoBuild.IsOutdated)
                ProjectBuilder.BuildAll();

            var folder = OutputFolder(platformName);
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = Path.Combine(folder, executable),
                target = target,
                options = BuildOptions.None,
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[Build] La build de {platformName} falló: {report.summary.result}.");
                return null;
            }

            var zip = ZipPath(platformName);
            Compress(folder, zip, keepUnixPermissions: target == BuildTarget.StandaloneOSX);
            Debug.Log($"[Build] Lista para compartir: {zip}");
            if (!Application.isBatchMode)
                EditorUtility.RevealInFinder(zip);
            return zip;
        }

        public static string OutputFolder(string platformName) => Path.Combine(BUILDS_ROOT, platformName, GAME_NAME);

        public static string ZipPath(string platformName) => Path.Combine(BUILDS_ROOT, $"{GAME_NAME}-{platformName}.zip");

        /// <summary>
        /// Comprime una carpeta. Para macOS se usa <c>ditto</c> (del sistema), que conserva los permisos de ejecución
        /// de la app; si no, el juego no arrancaría en el Mac de quien lo descargue.
        /// </summary>
        public static void Compress(string folder, string zipPath, bool keepUnixPermissions)
        {
            if (File.Exists(zipPath))
                File.Delete(zipPath);

            if (keepUnixPermissions && Application.platform == RuntimePlatform.OSXEditor)
            {
                using var process = Process.Start(new ProcessStartInfo("/usr/bin/ditto",
                    $"-c -k --sequesterRsrc \"{folder}\" \"{zipPath}\"") { UseShellExecute = false, CreateNoWindow = true });
                process?.WaitForExit();
                if (process == null || process.ExitCode != 0)
                    throw new InvalidOperationException($"No se pudo comprimir {folder} con ditto.");
                return;
            }

            ZipFile.CreateFromDirectory(folder, zipPath);
        }
    }
}
