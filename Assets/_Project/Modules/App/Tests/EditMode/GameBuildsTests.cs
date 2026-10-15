using System.IO;
using System.IO.Compression;
using NUnit.Framework;
using Trivias.App.Editor;

namespace Trivias.App.Tests
{
    /// <summary>Build para compartir: rutas y compresión (la compilación completa se prueba a mano: tarda minutos).</summary>
    public sealed class GameBuildsTests
    {
        private string _folder;

        [SetUp]
        public void CreateTempFolder()
        {
            _folder = Path.Combine(Path.GetTempPath(), "trivias-build-tests", Path.GetRandomFileName());
            Directory.CreateDirectory(Path.Combine(_folder, "Juego", "Datos"));
            File.WriteAllText(Path.Combine(_folder, "Juego", "Trivias.exe"), "exe");
            File.WriteAllText(Path.Combine(_folder, "Juego", "Datos", "nivel.dat"), "datos");
        }

        [TearDown]
        public void DeleteTempFolder()
        {
            if (Directory.Exists(_folder))
                Directory.Delete(_folder, true);
        }

        [Test]
        public void Paths_AreInsideBuildsFolder()
        {
            Assert.That(GameBuilds.OutputFolder("Mac"), Is.EqualTo(Path.Combine("Builds", "Mac", "Trivias")));
            Assert.That(GameBuilds.ZipPath("Windows"), Is.EqualTo(Path.Combine("Builds", "Trivias-Windows.zip")));
        }

        [Test]
        public void Compress_IncludesEveryFile()
        {
            var zip = Path.Combine(_folder, "juego.zip");
            GameBuilds.Compress(Path.Combine(_folder, "Juego"), zip, keepUnixPermissions: false);
            using var archive = ZipFile.OpenRead(zip);
            Assert.That(archive.GetEntry("Trivias.exe"), Is.Not.Null);
            Assert.That(archive.GetEntry("Datos/nivel.dat"), Is.Not.Null);
        }

        [Test]
        public void Compress_OverwritesAnOldZip()
        {
            var zip = Path.Combine(_folder, "juego.zip");
            File.WriteAllText(zip, "zip viejo");
            GameBuilds.Compress(Path.Combine(_folder, "Juego"), zip, keepUnixPermissions: false);
            using var archive = ZipFile.OpenRead(zip);
            Assert.That(archive.Entries.Count, Is.EqualTo(2));
        }

        [Test]
        public void Compress_ForMac_UsesDittoAndProducesAValidZip()
        {
            var zip = Path.Combine(_folder, "mac.zip");
            GameBuilds.Compress(Path.Combine(_folder, "Juego"), zip, keepUnixPermissions: true);
            using var archive = ZipFile.OpenRead(zip);
            Assert.That(archive.Entries.Count, Is.GreaterThanOrEqualTo(2));
        }
    }
}
